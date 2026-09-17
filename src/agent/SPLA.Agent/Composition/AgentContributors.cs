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
/// project's own AGENTS.md tree, the user's prompt, whatever the host added for this invocation,
/// skills, plugins. Two contributors are conditional, and on exactly the same decision that gates
/// their tools — a capability that is off must not leave text behind describing tools that are not
/// registered.</para>
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
    /// <param name="hostExtras">Context the HOST adds for one invocation — the CLI's <c>--sys-prompt</c>
    /// and friends. Supplied as "what", never as "where": the slot is decided here, immediately after
    /// <see cref="CustomPromptContributor"/>, because a host addition speaks with the same authority as
    /// the user's own prompt and must land after the project's word rather than replace it. Kept a
    /// parameter of this method rather than a mutator on the composer so order stays owned by the one
    /// place that owns it, and so the composer stays immutable after construction — it is shared by
    /// every chat of a project and recomposed on every iteration of the agent loop, where a list
    /// mutated mid-flight would surface as a torn enumeration in an unrelated chat.</param>
    public static IReadOnlyList<IAgentContributor> Default(
        SkillLibrary skills,
        PluginManager plugins,
        ISkillSession? session = null,
        IReadOnlyList<IAgentFeature>? enabledFeatures = null,
        IKeyValueStore? projectKv = null,
        ToolSetRegistry? toolSets = null,
        IEnumerable<IAgentContributor>? hostExtras = null)
    {
        var features = enabledFeatures ?? FullCatalog.Value;
        var enabledIds = new HashSet<string>(features.Select(f => f.Id), StringComparer.Ordinal);

        var contributors = new List<IAgentContributor>
        {
            new ModeContributor(),
            new CoreFeatureContributor(features),
            // Before the instruction files, because those may name a mount and the reader has to know
            // what such an address is before meeting one.
            new MountsContributor(),
            new InstructionsContributor(),
            // Project's own AGENTS.md tree (root today; nested scopes are a later wave). Right after
            // instructions, at the same authority tier as the rest of the project's own word — before
            // the project's custom_prompt, which is free-form and meant to read as coming after it.
            new ProjectAgentsContributor(),
            new CustomPromptContributor()
        };

        if (hostExtras != null) contributors.AddRange(hostExtras);

        // Two gates, both needed. At assembly: a capability outside the offered feature list gets no
        // contributor at all. At compose time: one that is offered speaks only when the settings being
        // composed for have it on — a role's, for a role's session — the same answer McpHost gives for
        // its tools.
        if (enabledIds.Contains("core.skills"))
            contributors.Add(new CapabilityGatedContributor("core.skills", new SkillsContributor(skills, session)));

        // Announcements of sets the agent may raise itself. Same gate as the tools that do the
        // raising: no toolset_activate, no index telling the model to call it.
        if (toolSets != null && enabledIds.Contains("core.toolsets"))
            contributors.Add(new CapabilityGatedContributor("core.toolsets", new ToolSetsContributor(toolSets)));

        contributors.Add(new PluginPromptContributor(plugins));
        contributors.Add(new PluginCommandContributor(plugins));

        if (enabledIds.Contains("core.resources"))
            contributors.Add(new CapabilityGatedContributor("core.resources", new ResourceSchemesContributor()));

        // core.memory owns both the agent_memory_* tools AND the auto-injected "context:*" snapshot,
        // so a disabled core.memory cannot leave a live-memory block with no tools behind it.
        if (enabledIds.Contains("core.memory"))
            contributors.Add(new CapabilityGatedContributor("core.memory", new WorkingMemoryContributor(projectKv)));

        // MUST stay last (ADR_20260911-2 §2.4 note 2): working memory above already changes the
        // prompt mid-session, and a folder entering view must not shift what sits behind it. Any
        // contributor added below this comment is a bug — add it above instead.
        contributors.Add(new ScopedAgentsContributor());

        return contributors;
    }
}

/// <summary>
/// Lets <paramref name="inner"/> speak only when capability <paramref name="featureId"/> is on for the
/// settings being composed for. Keeps the inner contributor's id, so the composition manifest still
/// names who said what.
/// </summary>
public sealed class CapabilityGatedContributor(string featureId, IAgentContributor inner) : IAgentContributor
{
    public string Id => inner.Id;

    public AgentContribution Contribute(AgentContributionContext context)
        => AgentFeatureCatalog.EnabledSet(context.Settings.Capabilities).Contains(featureId)
            ? inner.Contribute(context)
            : AgentContribution.None;
}
