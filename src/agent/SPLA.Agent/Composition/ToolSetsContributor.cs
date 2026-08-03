using SPLA.Domain.Agent;
using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.ToolSets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPLA.Agent.Composition;

/// <summary>
/// Announces the tool sets the agent may raise itself but which are not raised right now — one line
/// each, and nothing at all for sets that are off, waiting on a skill, or already disclosed.
///
/// <para>This is the whole of what the "on agent demand" level costs. The sets that are raised need
/// no text here: their tools travel as definitions, which is what raising a set means.</para>
/// </summary>
public sealed class ToolSetsContributor : IAgentContributor
{
    private readonly ToolSetRegistry _sets;
    private readonly IToolSetSession? _session;

    /// <param name="session">Explicit activation state; null resolves the running chat's one
    /// ambiently, the same way the skills contributor resolves its session.</param>
    public ToolSetsContributor(ToolSetRegistry sets, IToolSetSession? session = null)
    {
        _sets = sets;
        _session = session;
    }

    public string Id => "toolsets";

    public AgentContribution Contribute(AgentContributionContext context)
    {
        var session = _session ?? AgentSessionScope.Current?.ToolSets;

        var announced = _sets.All
            .Where(set => _sets.LevelOf(set.Id) == ToolSetLevel.AgentDemand)
            .Where(set => session?.IsActive(set.Id) != true)
            .ToList();

        if (announced.Count == 0) return AgentContribution.None;

        var body = new StringBuilder();
        body.Append("--- Tool Sets ---\nThese tool sets are available but not loaded. Call `toolset_activate` with the setId to load one when the task needs it.");
        foreach (var set in announced)
            body.Append($"\n- {set.Id}: {Announce(set)}");

        return AgentContribution.FromContext(new ContextItem
        {
            Source = "toolsets",
            Title = "Tool sets",
            Body = body.ToString(),
            Prefix = "\n\n"
        });
    }

    /// <summary>What the set is, plus when to call for it. The second half is the one the author has
    /// to write — it cannot be derived — so a set without it is announced by description alone rather
    /// than being silently dropped.</summary>
    private static string Announce(ToolSetDescriptor set)
    {
        var description = string.IsNullOrWhiteSpace(set.Description)
            ? $"{set.ToolNames.Count} tools"
            : set.Description.Trim();

        return string.IsNullOrWhiteSpace(set.Summon)
            ? description
            : $"{description} Call when: {set.Summon.Trim()}";
    }
}
