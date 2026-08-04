using System.Collections.Generic;
using SPLA.Library.Sources;

namespace SPLA.Library.Catalog;

/// <summary>Why a skill is or is not offered to the model. Everything except
/// <see cref="Available"/> keeps the skill visible in the settings panel with a reason, rather than
/// letting it disappear without explanation.</summary>
public enum SkillState
{
    /// <summary>Listed in the prompt and activatable.</summary>
    Available,
    /// <summary>Declares tools or features that are not present right now (usually a disabled plugin).</summary>
    MissingPrerequisites,
    /// <summary>Turned off in settings, or by its own <c>enabled: false</c> frontmatter.</summary>
    DisabledByUser,
    /// <summary>Comes from an untrusted source and has not been explicitly enabled.</summary>
    DisabledByTrust,
    /// <summary>A higher-priority source provides the same id.</summary>
    Superseded
}

/// <summary>
/// A skill as the runtime sees it: what the source reported, plus the resolved state.
///
/// <para>Addressed by <see cref="SourceId"/> + <see cref="Ref"/>, never by a file path. That is what
/// makes a non-filesystem source possible at all — while the model carried a path, every provider
/// had to be a folder.</para>
/// </summary>
public sealed class SkillCard
{
    public required string Id { get; init; }
    public required string SourceId { get; init; }
    public required string SourceLabel { get; init; }
    public required string Ref { get; init; }
    public required SkillTrust Trust { get; init; }

    public string Description { get; init; } = string.Empty;

    /// <summary>Normalised subject words, as the source reported them. What a tag librarian intersects
    /// and what the prompt's tag cloud is built from.</summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>How much of this card reaches the model unasked — inherited from its source, not
    /// declared per skill. Copied onto the card so nothing downstream has to hold the source.</summary>
    public SourceLevel Level { get; init; } = SourceLevel.OnShelf;
    public SkillRequirements Requires { get; init; } = SkillRequirements.None;
    public SkillRequirements Uses { get; init; } = SkillRequirements.None;

    /// <summary>Effective on/off after settings are layered over the frontmatter default.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Body goes into the base prompt instead of being listed for on-demand activation.</summary>
    public bool IsPreloaded { get; set; }

    public SkillState State { get; set; } = SkillState.Available;

    /// <summary>Human-readable explanation for any non-Available state, e.g.
    /// "needs dns_lookup, port_scan — from plugin 'network'".</summary>
    public string StateReason { get; set; } = string.Empty;

    /// <summary>Requirements the runtime could not satisfy — the raw material for
    /// <see cref="StateReason"/> and for the settings panel's "enable the missing plugin" action.</summary>
    public IReadOnlyList<string> MissingTools { get; set; } = [];
    public IReadOnlyList<string> MissingFeatures { get; set; } = [];

    /// <summary>Plugins that own the missing tools, deduplicated. Empty when the tools belong to no
    /// known plugin (a core tool that was gated off by agent.capabilities).</summary>
    public IReadOnlyList<string> MissingPlugins { get; set; } = [];
}
