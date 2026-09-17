using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;

namespace SPLA.MCP.Core.Pipeline.Stages;

/// <summary>
/// Wave 3.3 of <c>docs/plans/PLAN_20260911_agent_roles-agents-md-compact.md</c> — enforces
/// <c>docs/adr/ADR_20260911-2_agent_agents-md-scopes.md</c> §2.5/§2.6: a filesystem call into a
/// folder whose <c>AGENTS.md</c> chain has not yet been shown to the model gets a marker (so the
/// chain reaches the prompt on the next send — see <c>ScopedAgentsContributor</c>, Wave 3.4) and, if
/// it writes, is refused until the model repeats it once the rules are in view.
/// <para>
/// Sits at <see cref="ToolPipelineStage.Policy"/> beside <see cref="ZoneShadowStage"/> — same
/// reasoning: this and the permission check answer "may this happen" from different ends. Only
/// filesystem tools participate, the same ones <see cref="Security.EdgeClassifier"/> reads a
/// <c>path</c> argument from; shell, SSH and network tools are invisible to both for the same reason
/// (ADR §2.3: a shell call's targets cannot be found statically).
/// </para>
/// </summary>
public sealed class AgentsScopeStage : IToolMiddleware
{
    /// <summary>The project's settings, asked when no session is running — mirrors
    /// <see cref="McpHost.ProjectSettings"/>.</summary>
    private readonly Func<ResolvedSettings?>? _projectSettings;

    public AgentsScopeStage(Func<ResolvedSettings?>? projectSettings = null) => _projectSettings = projectSettings;

    public ToolPipelineStage Stage => ToolPipelineStage.Policy;

    public async Task<ToolResult> InvokeAsync(ToolCallInvocation call, ToolCallDelegate next, CancellationToken ct)
    {
        var settings = AgentSessionScope.Current?.Settings ?? _projectSettings?.Invoke();
        // Ignore: the mechanism is off entirely — no markers, no refusals. Also the correct behaviour
        // when nobody has told us anything (settings null): a narrow role or a bare host should not
        // suddenly start refusing writes because of a mode nobody set.
        if (settings is not { AgentsMd: AgentsMdMode.Inject }) return await next(call, ct);

        var definition = call.Tool?.GetDefinition().Function;
        if (definition is null) return await next(call, ct);

        var path = ExtractPath(call.ArgumentsJson);
        if (string.IsNullOrEmpty(path)) return await next(call, ct);

        var workspace = AgentSessionScope.Current?.Sandbox.Workspace;
        if (workspace is null) return await next(call, ct);

        var scope = new AgentsScopeResolver(workspace).DeepestScope(path);
        if (scope is null) return await next(call, ct);

        var conversation = AgentSessionScope.Current?.Conversation;
        if (conversation is null) return await next(call, ct); // nothing to mark against — pass through

        // The refusal decision is deliberately read OUTSIDE the lock, before this call tries to
        // insert anything: two calls dispatched together (a batch, or two background calls) both
        // arrive here having seen no rules for this scope yet, and both must be refused — refusing
        // only the slower of the two would mean the model watches an identical call it made twice
        // succeed once and fail once for a reason neither carries any evidence of. The lock below
        // only makes the marker itself idempotent — insert-if-still-missing — so the pair still
        // produces exactly one marker.
        // Two short, separately-locked sections rather than one: List&lt;ChatMessage&gt; is not safe
        // to enumerate while another thread mutates it, so the snapshot read still needs its own
        // lock — only the INTERVAL between the two locks is left open on purpose (see above).
        bool coveredWhenThisCallArrived;
        lock (conversation) { coveredWhenThisCallArrived = IsCovered(conversation, scope); }
        lock (conversation)
        {
            if (!IsCovered(conversation, scope)) conversation.AddScopeMarker(scope);
        }

        var isWrite = definition.Effect != ToolEffect.Read;
        if (isWrite && !coveredWhenThisCallArrived)
        {
            return ToolResult.Refuse(
                $"Error: project rules for '{scope}' ({scope}/AGENTS.md) were not loaded yet; " +
                "they are in your instructions from the next step; read them and repeat the call if it still complies.",
                "agents rules not loaded");
        }

        return await next(call, ct);
    }

    private static bool IsCovered(Conversation conversation, string scope) =>
        conversation.Messages.Any(m => m.ScopeMarker is { } marker &&
            (marker == scope || marker.StartsWith(scope + "/", StringComparison.Ordinal)));

    /// <summary>Same argument, same parsing <see cref="Security.EdgeClassifier"/> already does
    /// (<c>ToolJson.GetStringTrimmed(args, "path")</c>) — not duplicated, just called again on this
    /// stage's own parse of the arguments JSON, since the two stages do not share a call-scoped
    /// parsed-args cache today.</summary>
    private static string? ExtractPath(string? argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            return ToolJson.GetStringTrimmed(doc.RootElement, "path");
        }
        catch (JsonException)
        {
            return null; // an unparseable call carries no path this stage can act on
        }
    }
}
