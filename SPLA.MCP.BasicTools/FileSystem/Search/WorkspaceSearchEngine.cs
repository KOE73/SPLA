using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Host;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

/// <summary>
/// Text search over an <see cref="IWorkspace"/> using only its logical-path API
/// (<see cref="IWorkspace.GetFiles"/>/<see cref="IWorkspace.GetDirectories"/>/
/// <see cref="IWorkspace.ReadAllLinesAsync"/>). This is the fallback for a <em>virtual</em>
/// workspace — one whose <see cref="IWorkspace.MapPathToHost"/> returns <c>null</c>, so the disk
/// engines (ripgrep/.NET) have no real path to point at. Slower than ripgrep (no native indexing,
/// reads every candidate file through the workspace), but host-agnostic: it works the same over a
/// server user-folder or an in-memory store. Match shape is identical to the disk engines'
/// (<see cref="SearchMatch"/>), so ranking/output downstream is unchanged.
/// </summary>
public sealed class WorkspaceSearchEngine : ISearchEngine
{
    private readonly IWorkspace _ws;

    public WorkspaceSearchEngine(IWorkspace ws) => _ws = ws;

    public async Task<List<SearchMatch>> SearchAsync(
        string rootPath,
        string query,
        bool isRegex,
        bool caseSensitive,
        string[]? includePatterns,
        string[]? excludePatterns,
        CancellationToken cancellationToken)
    {
        var matches = new List<SearchMatch>();
        if (!_ws.DirectoryExists(rootPath)) return matches;

        var includeRegexes = includePatterns?.Select(SearchPatterns.GlobToRegex).ToList();
        var excludeRegexes = excludePatterns?.Select(SearchPatterns.GlobToRegex).ToList();

        Regex? queryRegex = isRegex
            ? new Regex(query, (caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase) | RegexOptions.Compiled)
            : null;
        var comp = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        foreach (var file in EnumerateFiles(rootPath, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SearchPatterns.IsBinaryByExtension(file)) continue;

            var relative = Relative(rootPath, file);
            if (includeRegexes is { Count: > 0 } && !includeRegexes.Any(r => r.IsMatch(relative))) continue;
            if (excludeRegexes is { Count: > 0 } && excludeRegexes.Any(r => r.IsMatch(relative))) continue;

            string[] lines;
            try { lines = await _ws.ReadAllLinesAsync(file, cancellationToken); }
            catch { continue; }   // unreadable entry — skip, mirror the disk engine

            for (int i = 0; i < lines.Length; i++)
            {
                var lineText = lines[i];
                if (queryRegex != null)
                {
                    var m = queryRegex.Match(lineText);
                    if (m.Success)
                        matches.Add(new SearchMatch { File = file, Line = i + 1, Column = m.Index + 1, Preview = lineText });
                }
                else
                {
                    var idx = lineText.IndexOf(query, comp);
                    if (idx >= 0)
                        matches.Add(new SearchMatch { File = file, Line = i + 1, Column = idx + 1, Preview = lineText });
                }
            }
        }

        return matches;
    }

    /// <summary>Depth-first walk over the workspace, skipping the standard ignore folders by leaf name.</summary>
    private IEnumerable<string> EnumerateFiles(string dir, CancellationToken ct)
    {
        foreach (var file in Safe(() => _ws.GetFiles(dir)))
            yield return file;

        foreach (var sub in Safe(() => _ws.GetDirectories(dir)))
        {
            ct.ThrowIfCancellationRequested();
            if (SearchPatterns.IgnoreFolders.Contains(LeafName(sub))) continue;
            foreach (var file in EnumerateFiles(sub, ct))
                yield return file;
        }
    }

    private static IReadOnlyList<string> Safe(Func<IReadOnlyList<string>> get)
    {
        try { return get(); }
        catch { return Array.Empty<string>(); }   // inaccessible directory — skip, don't abort the walk
    }

    private static string LeafName(string path)
    {
        var trimmed = path.TrimEnd('/', '\\');
        var slash = trimmed.LastIndexOfAny(new[] { '/', '\\' });
        return slash < 0 ? trimmed : trimmed.Substring(slash + 1);
    }

    /// <summary>Path of <paramref name="file"/> relative to <paramref name="root"/>, '/'-separated,
    /// for glob include/exclude matching (same shape the disk engine feeds its globs).</summary>
    private static string Relative(string root, string file)
    {
        var r = root.Replace('\\', '/').TrimEnd('/');
        var f = file.Replace('\\', '/');
        if (r.Length > 0 && f.StartsWith(r + "/", StringComparison.OrdinalIgnoreCase))
            return f.Substring(r.Length + 1);
        return f.TrimStart('/');
    }
}
