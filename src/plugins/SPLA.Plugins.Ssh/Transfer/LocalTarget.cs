namespace SPLA.Plugins.Ssh.Transfer;

/// <summary>
/// Turns the model's local path into a real path under the project, and answers whether a Linux
/// entry name can survive on a Windows disk at all.
/// <para>
/// The model never gets to name an absolute Windows path: it says <c>staging/prod.tar</c> and the
/// root is supplied here. That keeps a transfer inside the project whatever the model asks for, and
/// — more importantly — the escape can arrive from the REMOTE side too, inside a filename or a
/// symlink target, so the check is on the resolved path, not on the argument.
/// </para>
/// </summary>
internal static class LocalTarget
{
    /// <summary>Names Windows refuses regardless of extension (with or without one).</summary>
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    private static readonly char[] IllegalChars = { '<', '>', ':', '"', '|', '?', '*' };

    /// <summary>
    /// Resolves <paramref name="logicalPath"/> under <paramref name="rootPath"/> and refuses anything
    /// that leaves it. Rejects rooted paths, drive letters and UNC shares outright — an agent moving
    /// server configuration has no business writing to <c>D:\</c> or <c>\\server\share</c>.
    /// </summary>
    public static string Resolve(string rootPath, string logicalPath)
    {
        if (string.IsNullOrWhiteSpace(logicalPath))
            throw new InvalidOperationException("local_path is empty.");

        var candidate = logicalPath.Replace('/', Path.DirectorySeparatorChar).Trim();

        if (candidate.StartsWith(@"\\", StringComparison.Ordinal))
            throw new InvalidOperationException($"local_path must stay inside the project — UNC paths are not allowed: {logicalPath}");
        if (Path.IsPathRooted(candidate))
            throw new InvalidOperationException($"local_path must be relative to the project, not an absolute path: {logicalPath}");

        var root = Path.GetFullPath(rootPath);
        var full = Path.GetFullPath(Path.Combine(root, candidate));

        // Compare on the normalised full paths: this is what catches '..' regardless of how many
        // segments it hides behind, and it is also what catches a remote-supplied name doing it.
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(full, root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"local_path escapes the project directory: {logicalPath}");

        return full;
    }

    /// <summary>
    /// Why this relative path cannot be written into a Windows folder, or null when it can. This is
    /// the check that makes a container worth having: these files do not arrive at all otherwise, so
    /// the caller must fail loudly rather than quietly transfer a subset.
    /// </summary>
    public static string? WindowsNameProblem(string relativePath)
    {
        foreach (var segment in relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment.IndexOfAny(IllegalChars) >= 0)
                return $"'{segment}' contains a character Windows cannot store in a file name";
            if (segment.EndsWith(' ') || segment.EndsWith('.'))
                return $"'{segment}' ends with a space or dot, which Windows silently strips";

            var stem = segment.Split('.')[0];
            if (ReservedNames.Contains(stem))
                return $"'{segment}' uses the reserved device name '{stem}'";
        }

        // NAIVE: a fixed budget rather than probing the real limit. Long paths may or may not work
        // depending on the machine's LongPathsEnabled policy; refusing near the classic limit is the
        // predictable choice, and the container has no such limit at all.
        if (relativePath.Length > 200)
            return $"path is {relativePath.Length} characters — too long to place reliably under the project";

        return null;
    }

    /// <summary>
    /// Finds entries that are distinct on Linux but would land on the same Windows file, because the
    /// file system is case-insensitive. Returns the colliding groups; an empty result means clean.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<string>> CaseCollisions(IEnumerable<string> relativePaths)
        => relativePaths
            .GroupBy(p => p, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(g => (IReadOnlyList<string>)g.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList())
            .ToList();
}
