using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPLA.Domain.Host;

namespace SPLA.Domain.Settings;

/// <summary>
/// Turns the <c>mounts:</c> section into resolved <see cref="ProjectMount"/>s, and refuses the
/// manifest when it asks for something that cannot be honoured.
///
/// <para><b>Every rule about mounts lives here and nowhere else.</b> A check that runs at use instead
/// of at load fires at a moment nobody chose — which is the mistake this whole design was written to
/// avoid: a project that stops opening because of somebody else's commit.</para>
///
/// <para>The one rule that does not depend on the section: <c>mnt</c> may not be a real folder in the
/// root. It is checked whether or not anything is mounted, deliberately — one condition for the life
/// of the project means adding a mount never re-opens the question of what is already in the tree.
/// </para>
/// </summary>
public static class MountResolver
{
    private static readonly char[] IllegalInName = ['/', '\\', ':', '*', '?', '"', '<', '>', '|'];

    /// <param name="declared">The <c>mounts:</c> section as written, or null.</param>
    /// <param name="projectRoot">The project root — the directory holding the manifest.</param>
    /// <param name="manifestPath">Named in every refusal: whoever hits one has to know what to edit.</param>
    /// <exception cref="ProjectManifestException">The manifest cannot be honoured.</exception>
    public static IReadOnlyList<ProjectMount> Resolve(
        IReadOnlyList<SplaMountSection>? declared,
        string projectRoot,
        string manifestPath)
    {
        var root = Normalize(PathBoundary.RealPath(Path.GetFullPath(projectRoot)));

        var reserved = Path.Combine(root, ProjectMount.Prefix);
        if (Directory.Exists(reserved))
            throw new ProjectManifestException(manifestPath,
                $"'{ProjectMount.Prefix}' is a reserved name in a project root — it is the prefix declared " +
                $"mounts are addressed under ({ProjectMount.Prefix}/<name>/...), so a real folder of that " +
                $"name could never be reached. Rename '{reserved}'.");

        if (declared is null || declared.Count == 0) return [];

        var mounts = new List<ProjectMount>(declared.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in declared)
        {
            var name = section.Name?.Trim() ?? string.Empty;

            if (name.Length == 0)
                throw new ProjectManifestException(manifestPath,
                    "a mount has no name. The name is its address — there is no default for it.");

            if (name is "." or ".." || name.IndexOfAny(IllegalInName) >= 0)
                throw new ProjectManifestException(manifestPath,
                    $"mount name '{name}' cannot be used as an address: it appears in paths as " +
                    $"{ProjectMount.Prefix}/{name}/... and has to be one plain segment.");

            if (!seen.Add(name))
                throw new ProjectManifestException(manifestPath,
                    $"mount '{name}' is declared twice. The name is an address, and one address cannot " +
                    "have two targets.");

            var type = string.IsNullOrWhiteSpace(section.Type)
                ? ProjectMount.FileSystemType
                : section.Type!.Trim();

            if (!type.Equals(ProjectMount.FileSystemType, StringComparison.OrdinalIgnoreCase))
                throw new ProjectManifestException(manifestPath,
                    $"mount '{name}' has type '{type}'; the only mount type is " +
                    $"'{ProjectMount.FileSystemType}'. A mount is path rewriting — no state, no partial " +
                    "failures. Anything with a network behind it is a different thing and has its own tools.");

            var declaredPath = section.Path?.Trim() ?? string.Empty;
            if (declaredPath.Length == 0)
                throw new ProjectManifestException(manifestPath,
                    $"mount '{name}' has no path. Give it one, relative to this file or absolute.");

            // Made absolute HERE, against the directory holding the manifest. This is the exact bug
            // that killed `workspace:`: a path was kept as written and never made absolute, so it
            // only ever worked because startup happened to chdir into the right place.
            string hostPath;
            try
            {
                hostPath = Normalize(PathBoundary.RealPath(
                    Path.GetFullPath(Path.IsPathRooted(declaredPath)
                        ? declaredPath
                        : Path.Combine(root, declaredPath))));
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                throw new ProjectManifestException(manifestPath,
                    $"mount '{name}' has a path that cannot be read: {section.Path} ({ex.Message})");
            }

            if (IsUnder(hostPath, root))
                throw new ProjectManifestException(manifestPath,
                    $"mount '{name}' points at '{hostPath}', which is inside the project root. One file " +
                    $"would then have two addresses — {ProjectMount.Prefix}/{name}/... and its ordinary " +
                    "project path. Mounts are for folders outside the root; this one is reachable already.");

            var access = ParseAccess(section.Access, name, manifestPath);
            var trust = ParseTrust(section.Trust, name, manifestPath);

            var description = section.Description?.Trim() ?? string.Empty;
            if (description.Length == 0)
                throw new ProjectManifestException(manifestPath,
                    $"mount '{name}' has no description. Every mount carries one and it goes into the " +
                    "system prompt: with no line saying what the folder is for, the model opens it to " +
                    "find out.");

            // A target that is not on this machine is NOT a refusal. The project still opens; the
            // mount is marked unavailable and saying so at the point of use is a better answer than
            // refusing to open a project because one folder is unplugged.
            mounts.Add(new ProjectMount(
                name, hostPath, access, trust, description, Directory.Exists(hostPath)));
        }

        return mounts;
    }

    private static MountAccess ParseAccess(string? value, string name, string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(value)) return MountAccess.Read;

        return value.Trim().ToLowerInvariant() switch
        {
            "read" => MountAccess.Read,
            "write" => MountAccess.Write,
            _ => throw new ProjectManifestException(manifestPath,
                $"mount '{name}' has access '{value.Trim()}'; use 'read' or 'write'.")
        };
    }

    private static MountTrust ParseTrust(string? value, string name, string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(value)) return MountTrust.Trusted;

        return value.Trim().ToLowerInvariant() switch
        {
            "trusted" => MountTrust.Trusted,
            "untrusted" => MountTrust.Untrusted,
            _ => throw new ProjectManifestException(manifestPath,
                $"mount '{name}' has trust '{value.Trim()}'; use 'trusted' or 'untrusted'.")
        };
    }

    private static bool IsUnder(string path, string root) =>
        path.Equals(root, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string path) =>
        path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) is { Length: > 0 } trimmed
            ? trimmed
            : path;
}
