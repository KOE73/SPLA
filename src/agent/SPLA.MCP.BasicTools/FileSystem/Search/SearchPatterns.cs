using System.IO;
using System.Text.RegularExpressions;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

/// <summary>
/// Shared glob→regex and binary-file heuristics used by <see cref="WorkspaceSearchEngine"/> and by
/// the tree listing. Kept separate so every non-ripgrep code path applies identical include/exclude
/// and skip rules — a divergence here would make one tool silently disagree with another about which
/// files exist.
/// </summary>
internal static class SearchPatterns
{
    /// <summary>Directory names never descended into, matched by leaf name (case-insensitive).</summary>
    public static readonly System.Collections.Generic.HashSet<string> IgnoreFolders =
        new(System.StringComparer.OrdinalIgnoreCase) { ".git", "bin", "obj", "node_modules" };

    private static readonly string[] BinaryExtensions =
        { ".exe", ".dll", ".pdb", ".bin", ".zip", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".pdf", ".mp3", ".mp4" };

    public static Regex GlobToRegex(string glob)
    {
        // Normalize separators, then translate: ** -> .*, * -> [^/]*, ? -> [^/].
        var pattern = glob.Replace("\\", "/");
        var escaped = Regex.Escape(pattern)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/]*")
            .Replace("\\?", "[^/]");
        return new Regex("^" + escaped + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    /// <summary>True for paths whose extension marks them binary — cheap pre-filter before reading.</summary>
    public static bool IsBinaryByExtension(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return System.Array.IndexOf(BinaryExtensions, ext) >= 0;
    }
}
