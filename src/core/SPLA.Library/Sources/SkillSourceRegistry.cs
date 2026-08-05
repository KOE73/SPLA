using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;

namespace SPLA.Library.Sources;

/// <summary>Builds one <see cref="ISkillSource"/> from one configured <c>skills.sources</c> entry.
/// The factory owns validation of its own fields — the registry never inspects them, which is what
/// lets a new source type (server, database, git) ship without touching the core.</summary>
public interface ISkillSourceFactory
{
    /// <summary>Value of the entry's <c>type:</c> field this factory answers to.</summary>
    string Type { get; }

    /// <summary>
    /// Fallback name for an entry that declared no <c>id</c>. Owned by the factory for the same
    /// reason validation is: only it knows what its own fields mean, and only it can say what a
    /// nameless entry should be called.
    ///
    /// <para>Called BEFORE <see cref="Create"/> and on entries that may well be unusable, so it must
    /// not throw and must not touch the disk — a name is needed even for an entry that is about to
    /// be rejected, because that is how the rejection gets reported.</para>
    /// </summary>
    string DeriveId(SplaSkillSourceSection config, SkillSourceContext context);

    /// <summary>
    /// The location a trust grant would be recorded against, or null when this kind of source has no
    /// location to vouch for.
    ///
    /// <para>Null is the honest answer for a plugin branch, and it is not a gap: enabling a plugin
    /// already trusted it with executable code, which is strictly more than trusting its text. A
    /// second switch for the same decision is a way to have the two disagree.</para>
    /// </summary>
    string? GrantKey(SplaSkillSourceSection config, SkillSourceContext context) => null;

    /// <summary>Creates the source under an already-resolved <paramref name="id"/> and an
    /// already-decided <paramref name="trust"/>, or returns null with a reason when the entry is
    /// unusable.
    ///
    /// <para>Trust arrives decided rather than being read off <c>config.Trust</c> here: what an entry
    /// asks for and what it is allowed depend on which layer declared it and on what the user
    /// granted, and a factory that re-derived it would be a second opinion on a security question.</para></summary>
    ISkillSource? Create(SplaSkillSourceSection config, string id, SkillTrust trust, SkillSourceContext context, out string? error);
}

/// <summary>Ambient information every factory may need: where the project lives, where the machine
/// home is, where the installation is, and where to log.</summary>
/// <param name="AppDirectory">The installation folder — where <c>plugins/</c> sits. Skills shipped
/// with the product live in a <c>skills/</c> folder beside it. Empty to disable that source, which is
/// what tests and embedded hosts do.</param>
public sealed record SkillSourceContext(
    string WorkspacePath,
    string MachineHomePath,
    ILoggerFactory? Loggers,
    string AppDirectory = "")
{
    /// <summary>Expands environment variables and a leading <c>~</c> (the user's home, as everywhere
    /// else — NOT the SPLA home), then resolves a relative path against the workspace. Every
    /// file-shaped factory routes its path through this so one spelling means one location.</summary>
    public string ResolvePath(string path)
    {
        var expanded = Environment.ExpandEnvironmentVariables(path.Trim());

        if (expanded is "~" or "~/" or "~\\")
            expanded = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        else if (expanded.StartsWith("~/", StringComparison.Ordinal) || expanded.StartsWith("~\\", StringComparison.Ordinal))
            expanded = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), expanded[2..]);

        return Path.GetFullPath(Path.IsPathRooted(expanded) ? expanded : Path.Combine(WorkspacePath, expanded));
    }
}

/// <summary>Factory for <c>type: directory</c>.</summary>
public sealed class DirectorySkillSourceFactory : ISkillSourceFactory
{
    public string Type => "directory";

    /// <summary>The conventional name of this location, else the folder's own name. A path that is
    /// missing or unresolvable still has to yield something printable — the caller needs a name in
    /// order to say which entry it is rejecting.</summary>
    public string DeriveId(SplaSkillSourceSection config, SkillSourceContext context)
    {
        if (string.IsNullOrWhiteSpace(config.Path)) return "directory";
        try { return Describe(context.ResolvePath(config.Path), context).Id; }
        catch { return config.Path.Trim(); }
    }

    /// <summary>The folder itself: what a person actually looked at before saying they trust it.</summary>
    public string? GrantKey(SplaSkillSourceSection config, SkillSourceContext context)
    {
        if (string.IsNullOrWhiteSpace(config.Path)) return null;
        try { return context.ResolvePath(config.Path); }
        catch { return null; }
    }

    public ISkillSource? Create(
        SplaSkillSourceSection config, string id, SkillTrust trust, SkillSourceContext context, out string? error)
    {
        if (string.IsNullOrWhiteSpace(config.Path))
        {
            error = "directory source requires a 'path'";
            return null;
        }

        error = null;
        var full = context.ResolvePath(config.Path);

        return new DirectorySkillSource(
            id, config.Label ?? Describe(full, context).Label, full, trust,
            context.Loggers?.CreateLogger<DirectorySkillSource>(),
            level: SkillSourceRegistry.ParseLevel(config.Level));
    }

    /// <summary>Friendly id and display name for the conventional folders, falling back to the folder
    /// name. Now a FALLBACK for entries that declared no id, not the identity itself — an entry that
    /// wants to be addressed from another layer says its name. The path is surfaced separately by the
    /// panel, so a short word still beats a full path in the UI.</summary>
    private static (string Id, string Label) Describe(string fullPath, SkillSourceContext context)
    {
        // The repository's own skills/ — committed, shared with whoever clones the project.
        if (PathEquals(fullPath, Path.Combine(context.WorkspacePath, "skills")))
            return ("repo", "Repository");
        // Shipped with the installation, beside plugins/.
        if (context.AppDirectory.Length > 0 && PathEquals(fullPath, Path.Combine(context.AppDirectory, "skills")))
            return ("builtin", "Built-in");
        // .spla/ is local state and git-ignored in full, so anything here is a personal draft.
        if (PathEquals(fullPath, Path.Combine(context.WorkspacePath, ".spla", "skills")))
            return ("local", "Local drafts");
        if (PathEquals(fullPath, Path.Combine(context.MachineHomePath, "skills")))
            return ("machine", "Machine");

        var name = Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar));
        return (name, name);
    }

    private static bool PathEquals(string a, string b) =>
        string.Equals(Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar),
                      Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar),
                      StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Resolves the declared <c>skills.sources</c> entries into live sources.
///
/// <para>Entries are merged BY NAME, the way every other named collection in these settings files
/// already merges: the built-in entries first, then whatever the configuration layers declared, an
/// entry with an existing id overlaying that entry rather than being appended beside it. Adding a
/// branch is one entry in any layer; dropping an inherited one is <c>enabled: false</c>; clearing the
/// lot is <c>inherit_defaults: false</c>.</para>
///
/// <para>List order is display order, and the tie-break in <c>skill_find</c> ranking. It is NOT
/// priority: two branches offering the same skill id both keep their book, and the reader picks by
/// address.</para>
///
/// <para>Plugin-provided sources are appended last and are not configurable here — they follow their
/// plugin's own toggle, so there is exactly one switch per plugin instead of two.</para>
/// </summary>
public static class SkillSourceRegistry
{
    private static readonly ISkillSourceFactory[] Factories = [new DirectorySkillSourceFactory()];

    /// <summary>
    /// The branches the product comes with, as ordinary named entries: the project's committed
    /// <c>skills/</c>, personal drafts in the git-ignored <c>.spla/skills</c>, the user's machine-wide
    /// folder (under the SPLA home, so SPLA_HOME redirects it along with everything else), and the
    /// skills shipped beside <c>plugins/</c>. Any of them may be missing on disk — that is not an
    /// error.
    ///
    /// <para>These used to be two mechanisms: an implicit default list, plus the installation folder
    /// appended unconditionally after the configured entries so that a project overriding the list
    /// could not lose it silently. Merging by name removes the reason for both — an inherited branch
    /// cannot be lost silently any more, only switched off out loud — so they are now four ordinary
    /// entries with declared names, each switchable one at a time.</para>
    /// </summary>
    public static IReadOnlyList<SplaSkillSourceSection> DefaultSources(SkillSourceContext context)
    {
        var defaults = new List<SplaSkillSourceSection>
        {
            new() { Id = "repo",    Type = "directory", Path = "skills" },
            new() { Id = "local",   Type = "directory", Path = ".spla/skills" },
            new() { Id = "machine", Type = "directory", Path = Path.Combine(context.MachineHomePath, "skills") }
        };

        // Empty AppDirectory disables the shipped branch, which is what tests and embedded hosts want.
        if (context.AppDirectory.Length > 0)
            defaults.Add(new() { Id = "builtin", Type = "directory", Path = Path.Combine(context.AppDirectory, "skills") });

        return defaults;
    }

    /// <summary>
    /// Builds every source for a runtime: the built-in entries, the declared ones merged over them by
    /// name, then one per plugin that carries skills.
    /// </summary>
    /// <param name="declared">Entries accumulated from the settings layers, in declaration order.</param>
    /// <param name="inheritDefaults">False = a white list: only what <paramref name="declared"/> names.</param>
    /// <param name="maxTrust">The administrator's ceiling (<c>skills.policy.max_trust</c>), or null.</param>
    public static IReadOnlyList<ISkillSource> Build(
        IReadOnlyList<SplaSkillSourceSection>? declared,
        SkillSourceContext context,
        IEnumerable<ISkillSource>? pluginSources = null,
        ILogger? logger = null,
        bool inheritDefaults = true,
        string? maxTrust = null,
        ISkillTrustStore? trustStore = null)
    {
        var sources = new List<ISkillSource>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (id, entry) in Merge(declared, context, logger, inheritDefaults))
        {
            if (entry.Enabled == false)
            {
                logger?.LogDebug("Skill source '{Id}' is switched off.", id);
                continue;
            }

            // Type is guaranteed resolvable here: Merge drops entries whose type has no factory,
            // because an entry nobody can build also has no name to be addressed by.
            var factory = FactoryFor(entry.Type)!;

            // The grant is looked up by where the source points, not by what the entry is called:
            // rename the folder and approval does not follow it, which is the intended behaviour.
            var grantKey = factory.GrantKey(entry, context);
            var granted = grantKey is not null && trustStore?.IsGranted(grantKey) == true
                ? SkillTrust.Trusted
                : (SkillTrust?)null;

            var trust = EffectiveTrust(entry, id, granted, maxTrust, logger,
                insideWorkspace: grantKey is not null && IsInsideWorkspace(grantKey, context));
            var source = factory.Create(entry, id, trust, context, out var error);
            if (source is null)
            {
                logger?.LogWarning("Skill source '{Id}' not created: {Error}", id, error ?? "unknown reason");
                continue;
            }

            seenIds.Add(source.Id);
            sources.Add(source);
        }

        foreach (var pluginSource in pluginSources ?? [])
            if (seenIds.Add(pluginSource.Id))
                sources.Add(pluginSource);
            else
                logger?.LogWarning(
                    "Plugin skill source '{Id}' collides with a configured source of the same name — plugin source skipped.",
                    pluginSource.Id);

        logger?.LogInformation("Skill sources built. Count={Count} Ids={Ids}",
            sources.Count, string.Join(", ", sources.Select(s => s.Id)));

        return sources;
    }

    /// <summary>
    /// Folds the built-in entries and every declared layer into one named list: first appearance of a
    /// name fixes its position, later entries with that name overlay its fields.
    ///
    /// <para>Position is fixed by first appearance rather than by the last layer to touch the entry,
    /// so that editing a <c>label</c> cannot silently reorder the fond. Order is display and ranking
    /// only, but "changed the label, changed the ranking" is exactly the kind of surprise that costs
    /// an afternoon to trace.</para>
    /// </summary>
    internal static IReadOnlyList<(string Id, SplaSkillSourceSection Entry)> Merge(
        IReadOnlyList<SplaSkillSourceSection>? declared,
        SkillSourceContext context,
        ILogger? logger = null,
        bool inheritDefaults = true)
    {
        var order = new List<string>();
        var byId = new Dictionary<string, SplaSkillSourceSection>(StringComparer.OrdinalIgnoreCase);

        var all = new List<SplaSkillSourceSection>();
        if (inheritDefaults) all.AddRange(DefaultSources(context));
        if (declared != null) all.AddRange(declared);

        foreach (var entry in all)
        {
            string id;
            if (entry.Id?.Trim() is { Length: > 0 } declaredId)
            {
                // A named entry needs nothing else to be placed. That is what makes switching an
                // inherited branch off a complete statement — `- id: local` + `enabled: false`, with
                // no type and no path, because both are inherited from the entry it lands on.
                id = declaredId;
            }
            else
            {
                var factory = FactoryFor(entry.Type);
                if (factory is null)
                {
                    logger?.LogWarning(
                        "Skill source entry has neither an 'id' nor a usable 'type' ('{Type}') — nothing to call it by, skipped.",
                        entry.Type ?? "(none)");
                    continue;
                }
                id = factory.DeriveId(entry, context);
            }

            if (byId.TryGetValue(id, out var existing))
                byId[id] = Overlay(existing, entry, context);
            else
            {
                byId[id] = entry;
                order.Add(id);
            }
        }

        // Type is validated on the MERGED entry, not on each layer: an overlay is allowed to omit it,
        // and the only type that ever mattered is the one the finished entry ends up with.
        var merged = new List<(string, SplaSkillSourceSection)>();
        foreach (var id in order)
        {
            var entry = byId[id];
            if (FactoryFor(entry.Type) is null)
            {
                logger?.LogWarning(
                    "Skill source '{Id}' has an unknown or missing type '{Type}' — skipped.",
                    id, entry.Type ?? "(none)");
                continue;
            }
            merged.Add((id, entry));
        }

        return merged;
    }

    /// <summary>
    /// One entry laid over another: a field the newer entry left unset keeps the inherited value.
    /// That is what makes <c>- id: local</c> + <c>enabled: false</c> a complete statement — you switch
    /// a branch off without having to restate where it points.
    ///
    /// <para><b>Standing belongs to whoever last chose the content</b> — the layer that set
    /// <c>path</c> or <c>trust</c> — and not to whoever merely touched the entry. A layer adjusting
    /// only <c>level</c> or <c>label</c> changes neither what arrives nor how far it is believed, so
    /// the entry keeps the standing of the layer that pointed it.</para>
    ///
    /// <para>Deliberately not "the least privileged of the two". That version demotes on any touch,
    /// so a project setting <c>level: findable</c> would quietly untrust the user's own drafts folder;
    /// worse, it also demotes upward-corrections, so a person repointing a project's branch at a
    /// folder of their own would still be treated as reading the project's content. Whoever supplies
    /// the text carries the standing, in both directions.</para>
    ///
    /// <para>And a path that RESOLVES to where the entry already pointed is not a repointing. Restating
    /// a default is extremely common — this very repository's <c>.spla</c> lists <c>skills</c> and
    /// <c>.spla/skills</c>, the same two folders the built-in entries name — and treating it as a
    /// choice of content would untrust every project that ever wrote its own list out in full. The
    /// comparison is on the resolved location, because <c>skills</c> and an absolute path to it are
    /// the same folder written two ways.</para>
    /// </summary>
    private static SplaSkillSourceSection Overlay(
        SplaSkillSourceSection under, SplaSkillSourceSection over, SkillSourceContext context)
    {
        var factory = FactoryFor(over.Type ?? under.Type);
        var repoints = over.Path is not null && factory is not null &&
            !string.Equals(factory.GrantKey(under, context), factory.GrantKey(over, context),
                           StringComparison.OrdinalIgnoreCase);

        // An explicit trust claim always belongs to whoever wrote it, wherever the folder is.
        var choosesContent = over.Trust is not null || repoints;

        return new()
        {
            Id      = over.Id      ?? under.Id,
            Origin  = choosesContent ? over.Origin : under.Origin,
            Type    = over.Type    ?? under.Type,
            Path    = over.Path    ?? under.Path,
            Level   = over.Level   ?? under.Level,
            Trust   = over.Trust   ?? under.Trust,
            Label   = over.Label   ?? under.Label,
            Enabled = over.Enabled ?? under.Enabled,
            Options = over.Options ?? under.Options
        };
    }

    /// <summary>
    /// How far this entry is believed: what it asked for, capped by what its layer may claim, then
    /// capped again by the administrator.
    ///
    /// <para>The two caps are not redundant. The first says a project cannot vouch for itself — the
    /// scenario is one sentence long: clone a repository, its <c>.spla</c> adds a source pointing
    /// inside that same repository and calls it trusted, and its text is now in your system prompt.
    /// The second is the administrator's right to forbid what the user granted, which is a different
    /// question with a different answer on a server.</para>
    ///
    /// <para>A source declaring nothing is trusted, so the built-in branches keep behaving as they
    /// always have; the cap is what makes that safe for entries arriving from elsewhere.</para>
    /// </summary>
    internal static SkillTrust EffectiveTrust(
        SplaSkillSourceSection entry, string id, SkillTrust? granted, string? maxTrust, ILogger? logger,
        bool insideWorkspace = false)
    {
        var asked = ParseTrust(entry.Trust);
        var trust = asked;

        // Only the person at the keyboard and the administrator may vouch. A project file travels
        // with somebody else's repository, so it may ask and may not decide.
        if (trust == SkillTrust.Trusted && entry.Origin == SourceOrigin.Project)
        {
            trust = SkillTrust.Untrusted;
            if (entry.Trust is not null)
                logger?.LogWarning(
                    "Skill source '{Id}' declared trust '{Declared}' from the project layer, which cannot vouch for itself — lowered to untrusted. Trust it from the settings panel if you have read it.",
                    id, entry.Trust);
        }

        // Where the folder IS, not who declared it. A branch inside the workspace arrives with the
        // clone whether a .spla named it or it was simply sitting in skills/ — and the second case is
        // both easier to arrange and, before this, entirely unguarded: the layer ceiling only ever
        // looked at who wrote the entry, so a stranger's repository with a skills/ folder and no
        // config at all walked straight into the system prompt. Opening a project in a folder that
        // already has skills now means seeing them and deciding, which is the whole point.
        if (trust == SkillTrust.Trusted && insideWorkspace)
        {
            trust = SkillTrust.Untrusted;
            logger?.LogInformation(
                "Skill source '{Id}' lives inside the workspace, so it arrives with whoever wrote the project — untrusted until approved in the settings panel.",
                id);
        }

        // The grant is the user's separate, explicitly recorded decision, so it outranks the entry.
        if (granted is { } g && g == SkillTrust.Trusted) trust = SkillTrust.Trusted;

        if (trust == SkillTrust.Trusted && ParseTrust(maxTrust) == SkillTrust.Untrusted)
        {
            trust = SkillTrust.Untrusted;
            logger?.LogInformation(
                "Skill source '{Id}' held at untrusted by skills.policy.max_trust.", id);
        }

        return trust;
    }

    /// <summary>
    /// Whether a folder sits inside the project being worked on — i.e. whether it arrives with the
    /// clone.
    ///
    /// <para>The installation folder is excluded even when it happens to lie inside the workspace,
    /// which it does whenever this product is developed on itself: skills shipped beside the
    /// executable are the product's own wherever the executable was built. Build output is not
    /// somebody else's content.</para>
    /// </summary>
    private static bool IsInsideWorkspace(string fullPath, SkillSourceContext context)
    {
        if (context.AppDirectory.Length > 0 && IsUnder(fullPath, context.AppDirectory)) return false;
        return IsUnder(fullPath, context.WorkspacePath);
    }

    private static bool IsUnder(string path, string root)
    {
        try
        {
            var p = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var r = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (r.Length == 0) return false;

            return p.Equals(r, StringComparison.OrdinalIgnoreCase) ||
                   p.StartsWith(r + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static ISkillSourceFactory? FactoryFor(string? type) =>
        string.IsNullOrWhiteSpace(type)
            ? null
            : Factories.FirstOrDefault(f => f.Type.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Reads the <c>level:</c> field. Unset or unrecognised means <see cref="SourceLevel.OnShelf"/>
    /// — the behaviour sources have always had. Hyphens are optional, so both <c>in-catalog</c> and
    /// <c>incatalog</c> work; a typo falls back rather than silently hiding a source's skills, because
    /// hiding is the failure that is hardest to notice.</summary>
    internal static SourceLevel ParseLevel(string? value) =>
        value?.Trim().Replace("-", "").Replace("_", "").ToLowerInvariant() switch
        {
            "outofcatalog" or "hidden" => SourceLevel.OutOfCatalog,
            "findable" => SourceLevel.Findable,
            "incatalog" => SourceLevel.InCatalog,
            _ => SourceLevel.OnShelf
        };

    internal static SkillTrust ParseTrust(string? value) =>
        string.Equals(value?.Trim(), "untrusted", StringComparison.OrdinalIgnoreCase)
            ? SkillTrust.Untrusted
            : SkillTrust.Trusted;
}
