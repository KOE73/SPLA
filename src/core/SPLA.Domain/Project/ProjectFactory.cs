using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPLA.Domain.Settings;

namespace SPLA.Domain.Project;

/// <summary>
/// The one place a project comes into existence.
///
/// <para>Creation used to be spelled out four times — in <see cref="LocalProjectProvider.Create"/>,
/// in <c>ConfigLoader.ScaffoldIfNew</c>, in the desktop shell's "New Project…", and in the service's
/// project handler — and each knew a slightly different amount: one could not set a name, another
/// could not set a directory, none of them knew about profiles. Four spellings of "make a project"
/// is four answers to what a new project is allowed to do.</para>
/// </summary>
public static class ProjectFactory
{
    /// <summary>The manifest a project of this name in this directory would live in. Pure — callers
    /// use it to show the person the exact path before anything is written.</summary>
    public static string ManifestPathFor(string directory, string name)
        => Path.Combine(Path.GetFullPath(directory), SanitizeFileName(name) + ".spla");

    /// <summary>
    /// Creates a project in <paramref name="directory"/> and returns the manifest path.
    /// </summary>
    /// <param name="name">Display name; defaults to the directory's own name. Also names the file,
    /// after being made safe for one.</param>
    /// <param name="profile">What the new project may do. <see cref="ProjectProfile.Inherit"/> is not
    /// a project and is rejected here — the caller decides to run without one, it is not created.</param>
    /// <exception cref="InvalidOperationException">A manifest already exists in the directory.</exception>
    public static string Create(string directory, string? name, ProjectProfile profile)
    {
        var dir = Path.GetFullPath(directory);
        var projectName = string.IsNullOrWhiteSpace(name)
            ? new DirectoryInfo(dir).Name
            : name.Trim();

        var existing = Directory.Exists(dir) ? Directory.GetFiles(dir, "*.spla") : [];
        if (existing.Length > 0)
            throw new InvalidOperationException(
                $"{dir} already holds a project ({Path.GetFileName(existing[0])}). Open it instead of creating a second one.");

        return CreateAt(ManifestPathFor(dir, projectName), projectName, profile);
    }

    /// <summary>
    /// Creates a project at an exact manifest path. Separate from <see cref="Create"/> because a
    /// caller that already decided the file name (an import, a server area, a test) must not have it
    /// re-derived from the display name behind its back.
    /// </summary>
    public static string CreateAt(string manifestPath, string? name, ProjectProfile profile)
    {
        if (profile == ProjectProfile.Inherit)
            throw new ArgumentException(
                "The 'inherit' profile means running without a project — there is nothing to create.", nameof(profile));

        var full = Path.GetFullPath(manifestPath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        var project = new SplaProject
        {
            Name = string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(full) : name.Trim(),
            Ignore = [.. ConfigLoader.DefaultIgnorePatterns]
        };
        Apply(project, profile);

        ConfigLoader.SaveProject(project, full);
        return full;
    }

    /// <summary>
    /// Writes a profile's meaning into a manifest as ordinary settings. Nothing records which profile
    /// was used, on purpose — see <see cref="ProjectProfile"/>.
    /// </summary>
    public static void Apply(SplaProject project, ProjectProfile profile)
    {
        switch (profile)
        {
            case ProjectProfile.Minimal:
                // Empty list — not a missing key, which means "everything" for compatibility with
                // manifests written before capabilities existed.
                project.Agent ??= new SplaAgentSection();
                project.Agent.Capabilities = [];
                // One wildcard entry instead of naming every plugin installed on this machine: the
                // manifest travels in git, and a list of today's plugins would be a snapshot of one
                // computer baked into a portable file. An explicit per-plugin entry still wins.
                project.Plugins ??= new Dictionary<string, SplaPluginSection>();
                project.Plugins[PluginWildcard] = new SplaPluginSection { Enabled = false };
                break;

            case ProjectProfile.Standard:
                // Deliberately writes nothing: "everything enabled" is what an absent key already
                // means, and spelling it out would freeze today's feature list into the manifest.
                break;

            case ProjectProfile.Inherit:
                throw new ArgumentException("The 'inherit' profile writes no manifest.", nameof(profile));
        }
    }

    /// <summary>The plugin id that stands for "every plugin without an entry of its own".</summary>
    public const string PluginWildcard = "*";

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Trim().Select(c => invalid.Contains(c) ? '-' : c).ToArray()).Trim('.', ' ');
        return cleaned.Length == 0 ? "project" : cleaned;
    }
}
