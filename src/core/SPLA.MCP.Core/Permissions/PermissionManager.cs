using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.MCP.Core.Permissions;

public interface IPermissionManager
{
    PermissionResult CheckPermission(AgentMode mode, ToolFunctionDefinition toolMetadata, string argumentsJson);
}

public class PermissionManager : IPermissionManager
{
    private readonly List<RememberedToolPermission> _rememberedPermissions;

    /// <summary>Live reference to the project's settings — read at check time (not copied at
    /// construction) so an in-session <c>agent.save</c> permission-override edit takes effect on the
    /// very next tool call, the same way <see cref="ResolvedSettings.Mode"/> already does.</summary>
    private readonly ResolvedSettings? _settings;

    public PermissionManager(
        IEnumerable<RememberedToolPermission>? rememberedPermissions = null, ResolvedSettings? settings = null)
    {
        _rememberedPermissions = rememberedPermissions?.ToList() ?? new List<RememberedToolPermission>();
        _settings = settings;
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
        _ => null
    };

    public PermissionResult CheckPermission(AgentMode mode, ToolFunctionDefinition toolMetadata, string argumentsJson)
    {
        // Agent-scoped capabilities (memory, info, datetime, context) are fundamental: always
        // allowed, in every mode, regardless of remembered rules. They never touch project/system.
        if (toolMetadata.Scope == ToolScope.Agent)
            return PermissionResult.Allow;

        // Skill-scoped tools: activation requires user confirmation in interactive modes.
        // Deactivation uses ToolScope.Agent (always allowed) and never reaches this branch.
        if (toolMetadata.Scope == ToolScope.Skill)
        {
            return mode switch
            {
                AgentMode.Chat    => PermissionResult.Ask,
                AgentMode.Inspect => PermissionResult.Ask,
                AgentMode.Edit    => PermissionResult.Allow,
                AgentMode.Agent   => PermissionResult.Allow,
                _                 => PermissionResult.Deny   // Research and unknown modes
            };
        }

        // An explicit project policy (Settings → Agent → read/write/shell/internet) is the hard
        // floor/ceiling for its category — applies in every mode, including Agent, and cannot be
        // bypassed by a stale session "remembered" grant from before the policy was set.
        if (ProjectOverride(toolMetadata) is { } forced)
            return forced;

        // Agent mode: mode-based rules are authoritative; remembered denies must not override them.
        // Remembered allows are also redundant here (everything is already allowed), so skip entirely.
        if (mode != AgentMode.Agent)
        {
            var remembered = _rememberedPermissions.FirstOrDefault(x =>
                string.Equals(x.Tool, toolMetadata.Name, StringComparison.OrdinalIgnoreCase) &&
                (x.Arguments == "*" || string.Equals(x.Arguments, argumentsJson, StringComparison.Ordinal)));

            if (remembered != null)
            {
                return remembered.Decision == PermissionDecision.AllowRemember
                    ? PermissionResult.Allow
                    : PermissionResult.Deny;
            }
        }

        if (mode == AgentMode.Chat)
            return PermissionResult.Deny;
            
        if (mode == AgentMode.Research)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Internet) return PermissionResult.Allow;
            return PermissionResult.Deny;
        }

        if (mode == AgentMode.Inspect)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Internet) return PermissionResult.Ask;
            return PermissionResult.Deny;
        }

        if (mode == AgentMode.Edit)
        {
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Project && toolMetadata.Effect == ToolEffect.Write) return PermissionResult.Ask;
            
            if (toolMetadata.Scope == ToolScope.Local && toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
            
            if (toolMetadata.Scope == ToolScope.Shell)
            {
                if (toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
                if (toolMetadata.Risk == ToolRisk.Danger) return PermissionResult.Deny;
                return PermissionResult.Ask; 
            }
            if (toolMetadata.Scope == ToolScope.Internet) return PermissionResult.Allow;
            
            return PermissionResult.Deny;
        }

        if (mode == AgentMode.Agent)
        {
            if (toolMetadata.Scope == ToolScope.Project) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Shell)
            {
                if (toolMetadata.Risk == ToolRisk.Danger) return PermissionResult.Ask;
                return PermissionResult.Allow;
            }
            if (toolMetadata.Scope == ToolScope.Internet) return PermissionResult.Allow;
            if (toolMetadata.Scope == ToolScope.Local)
            {
                if (toolMetadata.Effect == ToolEffect.Read) return PermissionResult.Allow;
                return PermissionResult.Ask;
            }
            
            return PermissionResult.Ask;
        }

        return PermissionResult.Deny;
    }
}

public sealed class RememberedToolPermission
{
    public string Tool { get; init; } = "";
    public string Arguments { get; init; } = "";
    public PermissionDecision Decision { get; init; }
}
