using System.Collections.Generic;

namespace SPLA.MCP.Core.ToolSets;

/// <summary>
/// A named set of tools — the unit the user levels and the agent activates. The supplier (a plugin
/// assembly, or the core itself) stays the unit of delivery and takes no part in exposure.
///
/// <para>Today every plugin contributes exactly one set with the plugin's own id, and every core
/// feature is already a set in all but name. Several sets out of one assembly is the case this
/// model exists for (an AD plugin splitting into DNS, DHCP, users), and nothing here assumes one
/// set per supplier.</para>
/// </summary>
public sealed class ToolSetDescriptor
{
    /// <summary>Globally unique id. A skill requires a set by this name, and the agent activates it
    /// by this name — which is why it is not qualified by its supplier.</summary>
    public required string Id { get; init; }

    public required ToolSetOrigin Origin { get; init; }

    /// <summary>Id of the plugin that shipped the set, or <c>"core"</c>. For the UI ("these sets
    /// came from one file, that is why they share credentials") and for diagnostics.</summary>
    public required string OriginId { get; init; }

    /// <summary>What the set is. Taken from the supplier's manifest.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>When to call this set — the one thing the model needs at
    /// <see cref="ToolSetLevel.AgentDemand"/> and the one thing that cannot be derived from
    /// <see cref="Description"/>. Written by the set's author, English only, same role as a skill's
    /// description in the skills index. Empty means the set has nothing to say for itself and will
    /// be announced by description alone.</summary>
    public string Summon { get; init; } = string.Empty;

    /// <summary>Names of the tools in this set. Empty for a plugin whose assembly was never loaded —
    /// its tool names are unknown until it is, which is why the level lives on the set and not on
    /// the individual tools.</summary>
    public IReadOnlyList<string> ToolNames { get; init; } = [];
}
