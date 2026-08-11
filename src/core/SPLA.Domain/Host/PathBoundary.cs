using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SPLA.Domain.Host;

/// <summary>
/// One answer to "is this path inside the area I am allowed to touch", and the only one. Four
/// hand-rolled versions of this check had grown up separately — SFTP transfers, skill sources, the
/// 1C indexer, the web file browser — each with its own idea of what counts as an escape, which is
/// exactly how a boundary ends up holding in three places out of four.
///
/// <para>Mechanism only, no policy: it says where a path lands, never whether the caller may go
/// there. Who is allowed what is the gate's business.</para>
///
/// <para><b>Cutouts</b> are holes inside the root — subtrees that are inside by containment but out
/// of bounds anyway, <c>.spla/</c> being the case that motivated them. A boundary without them
/// would lock the agent in together with the secrets rather than out.</para>
/// </summary>
public sealed class PathBoundary
{
    /// <summary>No boundary at all: every path resolves to itself, nothing is refused. This is what
    /// running without a project means — the current directory is where the process was launched,
    /// which is a default, not a boundary, and pretending otherwise would bound tools by an accident
    /// of how they were started.</summary>
    public static readonly PathBoundary None = new();

    private readonly string? _root;
    private readonly string[] _cutouts;

    private PathBoundary()
    {
        _root = null;
        _cutouts = [];
    }

    /// <param name="root">Absolute path to the area. Relative input is made absolute here.</param>
    /// <param name="cutouts">Paths carved out of <paramref name="root"/>, relative to it or absolute.</param>
    public PathBoundary(string root, IEnumerable<string>? cutouts = null)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new ArgumentException("A boundary needs a root; use PathBoundary.None for no boundary.", nameof(root));

        _root = Normalize(Path.GetFullPath(root));
        _cutouts = cutouts?
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => Normalize(Path.GetFullPath(Path.IsPathRooted(c) ? c : Path.Combine(_root, c))))
            .ToArray() ?? [];
    }

    /// <summary>The area, or null when unbounded.</summary>
    public string? Root => _root;

    public bool IsBounded => _root is not null;

    /// <summary>
    /// Resolves <paramref name="logicalPath"/> and reports whether it stays in bounds.
    /// <paramref name="error"/> is written for a human and for the model: it says what was refused
    /// and why, never how the host is laid out.
    /// </summary>
    public bool TryResolve(string logicalPath, out string fullPath, out string? error)
    {
        fullPath = string.Empty;
        error = null;

        if (string.IsNullOrWhiteSpace(logicalPath))
        {
            error = "the path is empty";
            return false;
        }

        var candidate = logicalPath.Trim();

        if (_root is null)
        {
            // Unbounded: normalise and hand it back. GetFullPath still throws on genuinely malformed
            // input, and that is a fault, not a refusal.
            try { fullPath = Path.GetFullPath(candidate); return true; }
            catch (Exception ex) { error = $"the path cannot be read: {ex.Message}"; return false; }
        }

        // A network share is nobody's project. Caught before combining, because "\\host\share"
        // combined with a root silently becomes the share itself.
        if (candidate.StartsWith(@"\\", StringComparison.Ordinal) ||
            candidate.StartsWith("//", StringComparison.Ordinal))
        {
            error = "network shares are outside the project";
            return false;
        }

        string resolved;
        try
        {
            resolved = Path.GetFullPath(Path.IsPathRooted(candidate) ? candidate : Path.Combine(_root, candidate));
        }
        catch (Exception ex)
        {
            error = $"the path cannot be read: {ex.Message}";
            return false;
        }

        // Follow links BEFORE deciding: a junction inside the project pointing at C:\ is a legitimate
        // path by every string test there is. The check has to be on where it actually lands.
        resolved = Normalize(ResolveLinks(resolved));

        if (!IsUnder(resolved, _root))
        {
            error = "the path is outside the project";
            return false;
        }

        foreach (var cutout in _cutouts)
        {
            if (IsUnder(resolved, cutout))
            {
                error = $"'{Path.GetFileName(cutout)}' is the application's own folder and is not open to tools";
                return false;
            }
        }

        fullPath = resolved;
        return true;
    }

    /// <summary>Whether an already-absolute path lands in bounds. Same rules as
    /// <see cref="TryResolve"/>, for callers that only need the verdict.</summary>
    public bool Contains(string fullPath) => TryResolve(fullPath, out _, out _);

    /// <summary>
    /// The real location behind any links on the way to <paramref name="path"/>.
    ///
    /// <para>The target may not exist yet — writing a new file is the normal case — so the walk goes
    /// up to the nearest ancestor that does exist, resolves that, and puts the remainder back. A
    /// link anywhere along the existing part is therefore caught, which is the point: the escape is
    /// usually a junction on a parent folder, not on the leaf.</para>
    /// </summary>
    private static string ResolveLinks(string path)
    {
        try
        {
            var remainder = new Stack<string>();
            var current = path;

            while (true)
            {
                if (File.Exists(current) || Directory.Exists(current))
                {
                    var target = File.Exists(current)
                        ? File.ResolveLinkTarget(current, returnFinalTarget: true)?.FullName
                        : new DirectoryInfo(current).ResolveLinkTarget(returnFinalTarget: true)?.FullName;

                    current = target ?? current;
                    break;
                }

                var parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent) || parent == current) return path;

                remainder.Push(Path.GetFileName(current));
                current = parent;
            }

            while (remainder.Count > 0) current = Path.Combine(current, remainder.Pop());
            return Path.GetFullPath(current);
        }
        catch (IOException)
        {
            // A path we cannot inspect is not a path we can vouch for; fall back to the literal one
            // and let the containment check decide on that.
            return path;
        }
        catch (UnauthorizedAccessException)
        {
            return path;
        }
    }

    private static bool IsUnder(string path, string root) =>
        path.Equals(root, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string path) =>
        path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) is { Length: > 0 } trimmed
            ? trimmed
            : path;
}
