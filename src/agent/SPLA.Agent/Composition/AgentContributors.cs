using SPLA.Domain.Agent;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Composition;
using SPLA.MCP.Core.Plugins;
using SPLA.Library;
using SPLA.Library.Catalog;
using SPLA.Library.Sources;
using SPLA.MCP.Core.ToolSets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.Agent.Composition;

/// <summary>
/// SPLA's own contributor list, in assembly order. This is the one place that knows which sources of
/// context this product has — the knowledge that used to be spread across the branches of a single
/// prompt-building method.
///
/// <para>Order is authority order, top-down: mode, built-in capabilities, instruction files, the
/// user's prompt, skills, plugins. Two contributors are conditional, and on exactly the same
/// decision that gates their tools — a capability that is off must not leave text behind describing
/// tools that are not registered.</para>
/// </summary>
public static class AgentContributors
{
    /// <summary>Prompt-side view of the full feature set: every catalog id with its fragment and no
    /// tools. Used only when a caller supplies no feature list (a spawned sub-agent, direct
    /// construction in tests) — i.e. outside the AgentRuntime gating path.</summary>
    private static readonly Lazy<IReadOnlyList<IAgentFeature>> FullCatalog = new(() =>
        AgentFeatureCatalog.Order
            .Select(id => (IAgentFeature)new AgentFeature(
                id, promptFragment: CoreFeaturePrompts.Load(id), requires: AgentFeatureCatalog.RequiresOf(id)))
            .ToList());

    /// <param name="enabledFeatures">The features whose tools were registered. Null = the full
    /// catalog, which keeps callers that predate <c>agent.capabilities</c> on the unrestricted prompt.</param>
    /// <param name="session">Explicit skill session; null resolves the running chat's one ambiently.</param>
    /// <param name="projectKv">Project-scoped working memory. Null contributes session entries only.</param>
    /// <param name="toolSets">The set catalogue. Null means no set announcements — the state of a
    /// host that has no levelled sets to speak of (tests, a directly constructed sub-agent).</param>
    public static IReadOnlyList<IAgentContributor> Default(
        SkillLibrary skills,
        PluginManager plugins,
        ISkillSession? session = null,
        IReadOnlyList<IAgentFeature>? enabledFeatures = null,
        IKeyValueStore? projectKv = null,
        ToolSetRegistry? toolSets = null)
    {
        var features = enabledFeatures ?? FullCatalog.Value;
        var enabledIds = new HashSet<string>(features.Select(f => f.Id), StringComparer.Ordinal);

        var contributors = new List<IAgentContributor>
        {
            new ModeContributor(),
            new CoreFeatureContributor(features),
            new InstructionsContributor(),
            new CustomPromptContributor()
        };

        if (enabledIds.Contains("core.skills"))
            contributors.Add(new SkillsContributor(skills, session));

        // Announcements of sets the agent may raise itself. Same gate as the tools that do the
        // raising: no toolset_activate, no index telling the model to call it.
        if (toolSets != null && enabledIds.Contains("core.toolsets"))
            contributors.Add(new ToolSetsContributor(toolSets));

        contributors.Add(new PluginPromptContributor(plugins));
        contributors.Add(new PluginCommandContributor(plugins));

        // core.memory owns both the agent_memory_* tools AND the auto-injected "context:*" snapshot,
        // so a disabled core.memory cannot leave a live-memory block with no tools behind it.
        if (enabledIds.Contains("core.memory"))
            contributors.Add(new WorkingMemoryContributor(projectKv));

        return contributors;
    }
}
