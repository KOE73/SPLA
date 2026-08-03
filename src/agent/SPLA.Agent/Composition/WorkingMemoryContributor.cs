using SPLA.Domain.Agent;
using SPLA.MCP.Core.Composition;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Agent.Composition;

/// <summary>
/// The live <c>context:*</c> snapshot from working memory — the one contribution that is not part of
/// the system prompt.
///
/// <para>It travels as its own message (<see cref="ContextPlacement.TurnMessage"/>) because it is
/// data, not instruction: folding it into the prompt is what made weak models start "maintaining"
/// it. Going through the same mechanism as everything else is still the point — it is measured and
/// attributed in the manifest like any other contribution, and working memory is the piece most
/// likely to grow without anyone noticing.</para>
///
/// <para>Session scope is resolved ambiently (like the active skill), project scope is injected: the
/// first belongs to a chat, the second to the project, and this object outlives both.</para>
/// </summary>
public sealed class WorkingMemoryContributor : IAgentContributor
{
    private readonly IKeyValueStore? _projectKv;
    private readonly IKeyValueStore? _sessionKv;

    /// <param name="projectKv">Project-scoped store, or null to contribute session entries only.</param>
    /// <param name="sessionKv">Explicit session store. Null — the normal case — resolves this chat's
    /// store from the ambient <see cref="AgentSessionScope"/>.</param>
    public WorkingMemoryContributor(IKeyValueStore? projectKv, IKeyValueStore? sessionKv = null)
    {
        _projectKv = projectKv;
        _sessionKv = sessionKv;
    }

    public string Id => "working-memory";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var session = _sessionKv ?? AgentSessionScope.Current?.SessionKv;

        var entries = new List<(string scope, string key, string value)>();
        if (session != null)
            entries.AddRange(session.List().Select(kv => (session.Scope, kv.Key, kv.Value)));
        if (_projectKv != null)
            entries.AddRange(_projectKv.List().Select(kv => (_projectKv.Scope, kv.Key, kv.Value)));

        var block = WorkingMemoryInjector.Render(entries);
        return block is null
            ? AgentContribution.None
            : AgentContribution.FromContext(new ContextItem
            {
                Source = WorkingMemoryInjector.KeyPrefix,
                Title = "Working memory snapshot",
                Body = block,
                Placement = ContextPlacement.TurnMessage
            });
    }
}
