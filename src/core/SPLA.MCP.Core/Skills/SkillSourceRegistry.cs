using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;

namespace SPLA.MCP.Core.Skills;

/// <summary>Builds one <see cref="ISkillSource"/> from one configured <c>skills.sources</c> entry.
/// The factory owns validation of its own fields — the registry never inspects them, which is what
/// lets a new source type (server, database, git) ship without touching the core.</summary>
public interface ISkillSourceFactory
{
    /// <summary>Value of the entry's <c>type:</c> field this factory answers to.</summary>
    string Type { get; }

    /// <summary>Creates the source, or returns null with a reason when the entry is unusable.</summary>
    ISkillSource? Create(SplaSkillSourceSection config, SkillSourceContext context, out string? error);
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

    public ISkillSource? Create(SplaSkillSourceSection config, SkillSourceContext context, out string? error)
    {
        if (string.IsNullOrWhiteSpace(config.Path))
        {
            error = "directory source requires a 'path'";
            return null;
        }

        error = null;
        var full = context.ResolvePath(config.Path);
        var (id, label) = Describe(full, context);

        return new DirectorySkillSource(
            id, config.Label ?? label, full, SkillSourceRegistry.ParseTrust(config.Trust),
            context.Loggers?.CreateLogger<DirectorySkillSource>());
    }

    /// <summary>Friendly id and display name for the conventional folders, falling back to the folder
    /// name. The id is part of a skill's address and shows in the UI, so a short word beats a full
    /// path; the path itself is surfaced separately by the panel.</summary>
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
/// Resolves the configured <c>skills.sources</c> list into live sources, in priority order.
///
/// <para>Priority is list order: the first source offering a skill id wins, later ones are shadowed.
/// Plugin-provided sources are appended last and are not configurable here — they follow their
/// plugin's own toggle, so there is exactly one switch per plugin instead of two.</para>
/// </summary>
public static class SkillSourceRegistry
{
    private static readonly ISkillSourceFactory[] Factories = [new DirectorySkillSourceFactory()];

    /// <summary>
    /// Sources used when <c>skills.sources</c> is absent, in priority order — most specific first:
    /// the project's committed <c>skills/</c>, personal drafts in the git-ignored
    /// <c>.spla/skills</c>, then the user's machine-wide folder (under the SPLA home, so SPLA_HOME
    /// redirects it along with everything else). Any of them may be missing on disk — that is not an
    /// error. Skills that shipped with the installation are not listed here; see <see cref="Build"/>.
    /// </summary>
    public static IReadOnlyList<SplaSkillSourceSection> DefaultSources(SkillSourceContext context) =>
    [
        new() { Type = "directory", Path = "skills" },
        new() { Type = "directory", Path = ".spla/skills" },
        new() { Type = "directory", Path = Path.Combine(context.MachineHomePath, "skills") }
    ];

    /// <summary>
    /// Builds every source for a runtime, in priority order: the configured ones first, then the two
    /// that come from the installation itself — skills shipped beside <c>plugins/</c>, and one per
    /// plugin that carries skills.
    ///
    /// <para>The installation's own sources are appended unconditionally rather than being part of
    /// the configurable list, for the same reason plugins are discovered rather than declared: they
    /// are what the product came with. Putting them in <c>skills.sources</c> would make a project
    /// that overrides that list lose them without saying so. Coming last, they can still be
    /// overridden per skill — a project's own file with the same id wins and the shipped one is
    /// marked Shadowed.</para>
    /// </summary>
    public static IReadOnlyList<ISkillSource> Build(
        IReadOnlyList<SplaSkillSourceSection>? configured,
        SkillSourceContext context,
        IEnumerable<ISkillSource>? pluginSources = null,
        ILogger? logger = null)
    {
        var sources = new List<ISkillSource>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var entries = new List<SplaSkillSourceSection>(configured ?? DefaultSources(context));
        if (context.AppDirectory.Length > 0)
            entries.Add(new() { Type = "directory", Path = Path.Combine(context.AppDirectory, "skills") });

        foreach (var entry in entries)
        {
            var type = entry.Type?.Trim();
            if (string.IsNullOrEmpty(type))
            {
                logger?.LogWarning("Skill source entry has no 'type' — skipped.");
                continue;
            }

            var factory = Factories.FirstOrDefault(f =>
                f.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
            if (factory is null)
            {
                logger?.LogWarning("Unknown skill source type '{Type}' — skipped.", type);
                continue;
            }

            var source = factory.Create(entry, context, out var error);
            if (source is null)
            {
                logger?.LogWarning("Skill source '{Type}' not created: {Error}", type, error ?? "unknown reason");
                continue;
            }

            if (!seenIds.Add(source.Id))
            {
                logger?.LogWarning("Duplicate skill source id '{Id}' — later entry skipped.", source.Id);
                (source as IDisposable)?.Dispose();
                continue;
            }

            sources.Add(source);
        }

        foreach (var pluginSource in pluginSources ?? [])
            if (seenIds.Add(pluginSource.Id))
                sources.Add(pluginSource);

        logger?.LogInformation("Skill sources built. Count={Count} Ids={Ids}",
            sources.Count, string.Join(", ", sources.Select(s => s.Id)));

        return sources;
    }

    internal static SkillTrust ParseTrust(string? value) =>
        string.Equals(value?.Trim(), "untrusted", StringComparison.OrdinalIgnoreCase)
            ? SkillTrust.Untrusted
            : SkillTrust.Trusted;
}
