using System;
using System.Collections.Generic;

namespace SPLA.Library.Sources;

/// <summary>How far a source's content is trusted. A skill body becomes part of the system prompt,
/// so an untrusted source is a prompt-injection surface: its skills stay off until the user opts in
/// explicitly. Trust is a property of the SOURCE, not of the individual skill — one flag instead of
/// a check on every entry.</summary>
public enum SkillTrust
{
    /// <summary>Authored locally by the user or shipped with an installed plugin.</summary>
    Trusted,
    /// <summary>Fetched from somewhere the user did not write — off unless explicitly enabled.</summary>
    Untrusted
}

/// <summary>What a skill needs from the running agent. Both lists are usually empty: a plain
/// procedural skill ("how I write release notes") requires nothing and is always available.</summary>
public sealed record SkillRequirements(
    IReadOnlyList<string> Tools,
    IReadOnlyList<string> Features)
{
    public static readonly SkillRequirements None = new([], []);
    public bool IsEmpty => Tools.Count == 0 && Features.Count == 0;
}

/// <summary>One skill as reported by its source. <see cref="Ref"/> is opaque to the core — a relative
/// path for a directory source, a primary key for a database source, a URL for a remote one. Nothing
/// outside the owning source may interpret it.</summary>
public sealed record SkillEntry(
    string Id,
    string Description,
    string Ref,
    SkillRequirements Requires,
    SkillRequirements Uses,
    bool DefaultEnabled,
    IReadOnlyList<string>? Tags = null)
{
    /// <summary>Subject words for catalog lookup, already normalised and deduplicated. Empty is the
    /// honest answer for a skill whose author wrote none — an untagged skill is simply not findable
    /// by subject, which is a fact about the skill rather than an error.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Tags ?? [];
}

/// <summary>
/// A place skills come from. Deliberately minimal: enumerate and read. Everything else — parsing
/// frontmatter, resolving availability, deciding what reaches the prompt — is the manager's job, so
/// adding a source type (server, database, git) never touches the core.
/// </summary>
public interface ISkillSource
{
    /// <summary>Stable id, unique within a registry: "project", "machine", "plugin:network".
    /// Used as the first half of a skill's address and shown in the UI.</summary>
    string Id { get; }

    /// <summary>Human-readable name for the settings panel.</summary>
    string Label { get; }

    SkillTrust Trust { get; }

    /// <summary>How much of this source reaches the model unasked. Defaults to <see cref="SourceLevel.OnShelf"/>
    /// so a source that says nothing behaves the way sources always have — a handful of local skills
    /// listed in full is the right answer for a handful of local skills.</summary>
    SourceLevel Level => SourceLevel.OnShelf;

    /// <summary>Every skill this source currently offers. Called on every reload — implementations
    /// should be cheap or cache internally. Must not throw: an unreachable source returns empty.</summary>
    IReadOnlyList<SkillEntry> Enumerate();

    /// <summary>The skill's procedure text, or null when the ref is unknown or unreadable.</summary>
    string? ReadBody(string skillRef);

    /// <summary>
    /// The skill's own attachments — the <c>references/</c> and <c>assets/</c> that live with it and
    /// are useless without it. Paths are relative to the skill and forward-slashed; they are handed to
    /// <see cref="ReadResource"/> unchanged and are as opaque to the core as <see cref="SkillEntry.Ref"/>.
    ///
    /// <para>Returns empty for a skill that carries none, and for an unknown ref. Must not throw:
    /// listing attachments is never the reason a skill fails.</para>
    /// </summary>
    IReadOnlyList<string> ListResources(string skillRef) => [];

    /// <summary>
    /// One attachment's text, or null when the skill, the attachment, or the source itself is gone.
    ///
    /// <para>Resolution happens HERE, inside the source, for the same reason <see cref="SkillEntry.Ref"/>
    /// is opaque: only the source knows what its refs mean, and only it can tell an attachment of this
    /// skill from anything else it can reach. A file-shaped source must refuse a path that escapes the
    /// skill's own folder — these strings arrive from a model.</para>
    /// </summary>
    string? ReadResource(string skillRef, string resourcePath) => null;

    /// <summary>Raised when the underlying content changed and the manager should re-enumerate.</summary>
    event Action? Changed;
}

/// <summary>Optional second role for sources whose content can be written back. Declared now so a
/// skill editor can be added without reopening <see cref="ISkillSource"/>; only the directory source
/// implements it today, and no UI calls it yet.</summary>
public interface IEditableSkillSource : ISkillSource
{
    bool CanWrite { get; }
    void WriteBody(string skillRef, string text);
}
