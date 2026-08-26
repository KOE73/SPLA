using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.MCP.Core.Permissions;

public interface IPermissionManager
{
    /// <summary>
    /// Decides whether this call is allowed. Pure and cheap — safe to call twice: once ahead of time
    /// by whoever might want to ask a human before a call ever crosses a wire, and once at the point
    /// of execution by the host that actually enforces it. The verdict this returns is advice; only
    /// the host applying it at execution is the one that counts.
    /// </summary>
    PermissionVerdict CheckPermission(AgentMode mode, ToolFunctionDefinition toolMetadata, string argumentsJson);

    /// <summary>
    /// Whether this tool can do <i>anything at all</i> in this mode — the question disclosure asks,
    /// which is not the question a call asks.
    /// <para>
    /// The two were one method, told apart by passing <c>"{}"</c> for "there are no arguments yet".
    /// That worked only while nothing read the arguments: the moment an
    /// <see cref="IToolArgumentPolicy"/> does, a module handed an empty object has no statement to
    /// judge, and a denial from it would delete the tool from the model's list in every mode — a
    /// failure that looks like "the tool vanished", not like "policy worked".
    /// </para>
    /// <para>
    /// So this one never consults argument modules. It answers from mode, metadata and the project's
    /// standing policy alone.
    /// </para>
    /// </summary>
    PermissionVerdict CheckToolCeiling(AgentMode mode, ToolFunctionDefinition toolMetadata);
}

public class PermissionManager : IPermissionManager
{
    private readonly List<RememberedToolPermission> _rememberedPermissions;

    /// <summary>Live reference to the project's settings — read at check time (not copied at
    /// construction) so an in-session <c>agent.save</c> permission-override edit takes effect on the
    /// very next tool call, the same way <see cref="ResolvedSettings.Mode"/> already does.</summary>
    private readonly ResolvedSettings? _settings;

    /// <summary>Domain checks on the arguments, consulted last and only to narrow. Empty by default:
    /// this is a seat, and every module that will sit in it arrives with its own decision.</summary>
    private readonly List<IToolArgumentPolicy> _argumentPolicies;

    private readonly ILogger? _logger;

    public PermissionManager(
        IEnumerable<RememberedToolPermission>? rememberedPermissions = null,
        ResolvedSettings? settings = null,
        IEnumerable<IToolArgumentPolicy>? argumentPolicies = null,
        ILogger? logger = null)
    {
        _rememberedPermissions = rememberedPermissions?.ToList() ?? new List<RememberedToolPermission>();
        _settings = settings;
        _argumentPolicies = argumentPolicies?.ToList() ?? new List<IToolArgumentPolicy>();
        _logger = logger;
    }

    public void Remember(RememberedToolPermission permission)
    {
        _rememberedPermissions.RemoveAll(x =>
            string.Equals(x.Tool, permission.Tool, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Arguments, permission.Arguments, StringComparison.Ordinal));
        _rememberedPermissions.Add(permission);
    }

    /// <summary>Which project-level override category (if any) governs this tool — mirrors the same
    /// scope/effect discrimination the mode-based branches below already use, so the override maps
    /// onto exactly the categories a project can actually configure (read/write/shell/internet).</summary>
    private static string? ClassifyCategory(ToolFunctionDefinition toolMetadata) => toolMetadata switch
    {
        // Scope discrimination must win before anything else can claim a Foreign tool — a foreign
        // tool that happens to report Effect.Write must not be classified as "write".
        { Scope: ToolScope.Foreign } => "foreign",
        { Scope: ToolScope.Shell } => "shell",
        { Scope: ToolScope.Internet } => "internet",
        { Effect: ToolEffect.Write } => "write",
        { Effect: ToolEffect.Read, Scope: ToolScope.Project or ToolScope.Local } => "read",
        _ => null
    };

    private static PermissionResult? ParseOverride(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "allow" => PermissionResult.Allow,
        "deny" => PermissionResult.Deny,
        "ask" => PermissionResult.Ask,
        _ => null
    };

    /// <summary>The project's explicit override for this tool's category, or null when unset (falls
    /// through to mode-based logic). A project policy is a hard floor/ceiling: it wins over BOTH
    /// mode defaults and session "remembered" exceptions — "most restrictive/most explicit wins".</summary>
    private PermissionResult? ProjectOverride(ToolFunctionDefinition toolMetadata) => ClassifyCategory(toolMetadata) switch
    {
        "read" => ParseOverride(_settings?.PermRead),
        "write" => ParseOverride(_settings?.PermWrite),
        "shell" => ParseOverride(_settings?.PermShell),
        "internet" => ParseOverride(_settings?.PermInternet),
        "foreign" => ParseOverride(_settings?.PermForeign),
        _ => null
    };

    public PermissionVerdict CheckToolCeiling(AgentMode mode, ToolFunctionDefinition toolMetadata)
        => Decide(mode, toolMetadata, "{}");

    public PermissionVerdict CheckPermission(AgentMode mode, ToolFunctionDefinition toolMetadata, string argumentsJson)
        => Narrow(Decide(mode, toolMetadata, argumentsJson), toolMetadata, argumentsJson);

    /// <summary>How much a verdict forbids. The ordering is what lets modules combine without
    /// registration order deciding anything — the strictest wins, whoever said it.</summary>
    private static int Severity(PermissionResult result) => result switch
    {
        PermissionResult.Allow => 0,
        PermissionResult.Ask => 1,
        _ => 2
    };

    /// <summary>
    /// Lets the domain modules tighten the verdict — and only tighten it.
    /// <para>
    /// Last, after the project's standing policy and after the mode rules, because a module is the
    /// narrowest authority and not the first: a plugin's opinion must not be able to reach past a
    /// policy the project set. And since a module can only raise
    /// <see cref="Severity"/>, "may only narrow" is a property of this loop rather than a rule
    /// modules are asked to respect.
    /// </para>
    /// </summary>
    private PermissionVerdict Narrow(
        PermissionVerdict verdict, ToolFunctionDefinition toolMetadata, string argumentsJson)
    {
        if (_argumentPolicies.Count == 0) return verdict;

        // Agent-scoped capabilities are fundamental and argument-free by nature; asking a domain
        // module about agent_memory_get is asking a question that has no subject.
        if (toolMetadata.Scope == ToolScope.Agent) return verdict;

        // Already the strictest answer there is — nothing a module says could make it stricter, and
        // running a parser to confirm that is work with no possible outcome.
        if (verdict.Result == PermissionResult.Deny) return verdict;

        var strictest = verdict;

        foreach (var policy in _argumentPolicies)
        {
            PermissionVerdict? opinion;
            try
            {
                if (!policy.AppliesTo(toolMetadata)) continue;
                opinion = policy.Evaluate(toolMetadata, argumentsJson);
            }
            catch (Exception ex)
            {
                // A broken module abstains; it does not deny. Denying here would make any bug in any
                // policy a silent way to switch a tool off, and it would present as "the tool stopped
                // working" rather than as the fault it is.
                _logger?.LogError(
                    ex, "Argument policy threw and was skipped. Policy={Policy} Tool={ToolName}",
                    policy.GetType().Name, toolMetadata.Name);
                continue;
            }

            if (opinion is null) continue;

            if (Severity(opinion.Result) > Severity(strictest.Result))
                strictest = opinion with { Category = opinion.Category ?? verdict.Category };
        }

        return strictest;
    }

    private PermissionVerdict Decide(AgentMode mode, ToolFunctionDefinition toolMetadata, string argumentsJson)
    {
        // Agent-scoped capabilities (memory, info, datetime, context) are fundamental: always
        // allowed, in every mode, regardless of remembered rules. They never touch project/system.
        if (toolMetadata.Scope == ToolScope.Agent)
            return PermissionVerdict.Allow("agent-scoped capability, always allowed");

        // Skill-scoped tools: taking a skill on requires user confirmation in interactive modes.
        // Deactivation uses ToolScope.Agent (always allowed) and never reaches this branch.
        if (toolMetadata.Scope == ToolScope.Skill)
        {
            // Reading inside a skill the user already let in is not a second decision. The gate is
            // the activation; skill_read_resource cannot reach past the loan slip — no argument names
            // another skill, and the source refuses anything outside the skill's own folder. Asking
            // again per reference would mean a dozen prompts for one step of one procedure, which
            // trains the user to click through the prompt that actually matters.
            var confirmable = toolMetadata.Effect != ToolEffect.Read;

            return mode switch
            {
                AgentMode.Chat or AgentMode.Inspect => confirmable
                    ? PermissionVerdict.Ask("skill activation requires confirmation in this mode", "skill")
                    : PermissionVerdict.Allow("read-only skill access already granted by activation", "skill"),
                AgentMode.Edit or AgentMode.Agent =>
                    PermissionVerdict.Allow("skill tools allowed outright in this mode", "skill"),
                _ => PermissionVerdict.Deny("skill tools not available in this mode", "skill")
            };
        }

        // An explicit project policy (Settings → Agent → read/write/shell/internet) is the hard
        // floor/ceiling for its category — applies in every mode, including Agent, and cannot be
        // bypassed by a stale session "remembered" grant from before the policy was set.
        var category = ClassifyCategory(toolMetadata);
        if (ProjectOverride(toolMetadata) is { } forced)
            return new PermissionVerdict
            {
                Result = forced, Category = category,
                Reason = $"project policy for '{category}' forces {forced}"
            };

        // Agent mode: mode-based rules are authoritative; remembered denies must not override them.
        // Remembered allows are also redundant here (everything is already allowed), so skip entirely.
        //
        // Foreign is the one scope that premise does not cover, and the exception is load-bearing
        // rather than tidy: a foreign tool is Ask in Agent mode too (see the branch below), so
        // skipping the lookup would mean asking again on every single call — "confirm the first call
        // to each tool" would never take effect in the mode people actually work in, and a prompt
        // that fires every time is a prompt nobody reads.
        if (mode != AgentMode.Agent || toolMetadata.Scope == ToolScope.Foreign)
        {
            var remembered = _rememberedPermissions.FirstOrDefault(x =>
                string.Equals(x.Tool, toolMetadata.Name, StringComparison.OrdinalIgnoreCase) &&
                (x.Arguments == "*" || string.Equals(x.Arguments, argumentsJson, StringComparison.Ordinal)));

            if (remembered != null)
            {
                return remembered.Decision == PermissionDecision.AllowRemember
                    ? PermissionVerdict.Allow("remembered from an earlier confirmation in this session", category)
                    : PermissionVerdict.Deny("remembered denial from an earlier confirmation in this session", category);
            }
        }

        // Foreign-scoped tools (executed by a foreign MCP server) declared none of our axes — there
        // is no Effect/Risk to reason from. Guessing them from a description written by a stranger
        // would make the safety boundary negotiable by that stranger, so the verdict is naive on
        // purpose instead of derived: deny in Chat (no tool calls at all), ask everywhere else.
        // See ADR_20260826_service_mcp-client §2/§3. This sits after the remembered-permissions
        // check above — which the condition on that block deliberately keeps reachable for Foreign
        // in Agent mode — so "ask once, remember" (arguments: "*") turns the second call into Allow
        // in every mode, without any code of its own.
        if (toolMetadata.Scope == ToolScope.Foreign)
        {
            return mode == AgentMode.Chat
                ? PermissionVerdict.Deny("chat mode allows no tool calls", category)
                : PermissionVerdict.Ask("foreign tool server; confirm the first call to each tool", category);
        }

        if (mode == AgentMode.Chat)
            return PermissionVerdict.Deny("chat mode allows no tool calls", category);

        if (mode == AgentMode.Research)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read)
                return PermissionVerdict.Allow("research mode allows project reads", category);
            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read)
                return PermissionVerdict.Allow("research mode allows local reads", category);
            if (toolMetadata.Scope == ToolScope.Internet)
                return PermissionVerdict.Allow("research mode allows internet access", category);
            return PermissionVerdict.Deny("research mode allows only reads and internet access", category);
        }

        if (mode == AgentMode.Inspect)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read)
                return PermissionVerdict.Allow("inspect mode allows project reads", category);
            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read)
                return PermissionVerdict.Allow("inspect mode allows local reads", category);
            if (toolMetadata.Scope == ToolScope.Internet)
                return PermissionVerdict.Ask("inspect mode requires confirmation for internet access", category);
            return PermissionVerdict.Deny("inspect mode allows only reads and confirmed internet access", category);
        }

        if (mode == AgentMode.Edit)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read)
                return PermissionVerdict.Allow("edit mode allows project reads", category);
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Write)
                return PermissionVerdict.Ask("edit mode requires confirmation for project writes", category);

            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read)
                return PermissionVerdict.Allow("edit mode allows local reads", category);

            if (toolMetadata.Scope == ToolScope.Shell)
            {
                if (toolMetadata.Effect == ToolEffect.Read)
                    return PermissionVerdict.Allow("edit mode allows read-only shell commands", category);
                if (toolMetadata.Risk == ToolRisk.Danger)
                    return PermissionVerdict.Deny("edit mode denies dangerous shell commands", category);
                return PermissionVerdict.Ask("edit mode requires confirmation for shell commands", category);
            }
            if (toolMetadata.Scope == ToolScope.Internet)
                return PermissionVerdict.Allow("edit mode allows internet access", category);

            return PermissionVerdict.Deny("no rule in edit mode allows this tool", category);
        }

        if (mode == AgentMode.Agent)
        {
            if (toolMetadata.Scope == ToolScope.Project)
                return PermissionVerdict.Allow("agent mode allows project access", category);
            if (toolMetadata.Scope == ToolScope.Shell)
            {
                if (toolMetadata.Risk == ToolRisk.Danger)
                    return PermissionVerdict.Ask("agent mode requires confirmation for dangerous shell commands", category);
                return PermissionVerdict.Allow("agent mode allows shell commands", category);
            }
            if (toolMetadata.Scope == ToolScope.Internet)
                return PermissionVerdict.Allow("agent mode allows internet access", category);
            if (toolMetadata.Scope == ToolScope.Local)
            {
                if (toolMetadata.Effect == ToolEffect.Read)
                    return PermissionVerdict.Allow("agent mode allows local reads", category);
                return PermissionVerdict.Ask("agent mode requires confirmation for local writes", category);
            }

            return PermissionVerdict.Ask("agent mode requires confirmation for an unclassified tool", category);
        }

        return PermissionVerdict.Deny("unrecognised mode", category);
    }
}

public sealed class RememberedToolPermission
{
    public string Tool { get; init; } = "";
    public string Arguments { get; init; } = "";
    public PermissionDecision Decision { get; init; }
}
