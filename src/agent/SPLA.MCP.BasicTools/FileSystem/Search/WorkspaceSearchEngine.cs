using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Host;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

/// <summary>
/// Text search over an <see cref="IWorkspace"/> using only its logical-path API. This is the engine
/// for the <em>virtual</em> substrate — a workspace whose <see cref="IWorkspace.MapPathToHost"/>
/// returns <c>null</c>, so ripgrep has no real path to point at — and it doubles as the fallback when
/// ripgrep is unavailable on a disk workspace.
/// <para>
/// The split is by <b>substrate, not by binary availability</b>: the virtual substrate can never be
/// served by ripgrep, so this engine is permanent rather than a stopgap. That is also why the old
/// third engine (direct-disk .NET) is gone — a disk workspace is an <see cref="IWorkspace"/> too, and
/// a third implementation of every feature bought nothing. See
/// <c>docs/adr/ADR_20260831_mcp_search-and-listing-tools.md</c> §2.0.
/// </para>
/// <para>
/// Regexes compile with <see cref="RegexOptions.NonBacktracking"/>. That is deliberate on two counts:
/// it matches ripgrep's engine class (no lookaround, no backreferences — a linear-time automaton), so
/// one query cannot mean different things on different substrates; and it removes catastrophic
/// backtracking as a failure mode for a pattern the model wrote. See ADR §2.2a.
/// </para>
/// </summary>
public sealed class WorkspaceSearchEngine : ISearchEngine
{
    private readonly IWorkspace _ws;

    public WorkspaceSearchEngine(IWorkspace ws) => _ws = ws;

    public async Task<SearchOutcome> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        var outcome = new SearchOutcome();
        if (!_ws.DirectoryExists(request.RootPath)) return outcome;

        var includeRegexes = request.IncludePatterns?.Select(SearchPatterns.GlobToRegex).ToList();
        var excludeRegexes = request.ExcludePatterns?.Select(SearchPatterns.GlobToRegex).ToList();

        var queryRegex = request.IsRegex ? CompileQuery(request) : null;
        var comparison = request.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        foreach (var file in EnumerateFiles(request.RootPath, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (SearchPatterns.IsBinaryByExtension(file)) continue;

            var relative = Relative(request.RootPath, file);
            if (includeRegexes is { Count: > 0 } && !includeRegexes.Any(r => r.IsMatch(relative))) continue;
            if (excludeRegexes is { Count: > 0 } && excludeRegexes.Any(r => r.IsMatch(relative))) continue;

            string[] lines;
            try { lines = await _ws.ReadAllLinesAsync(file, cancellationToken); }
            catch { continue; }   // unreadable entry — skip rather than abort the walk

            var hits = request.Multiline
                ? MatchMultiline(lines, queryRegex, request, comparison)
                : MatchPerLine(lines, queryRegex, request, comparison);

            if (hits.Count == 0) continue;

            outcome.Files.Add(new SearchFileHit { File = file, MatchCount = hits.Count });

            // Only Content mode needs line text; the other modes already have all they asked for, so
            // stop building previews — this is the early exit that makes -l/-c cheaper than a scan.
            if (request.Mode != SearchOutputMode.Content) continue;

            foreach (var (lineIndex, column) in hits)
            {
                outcome.Matches.Add(new SearchMatch
                {
                    File    = file,
                    Line    = lineIndex + 1,
                    Column  = column,
                    Preview = lines[lineIndex],
                    Before  = request.ContextBefore > 0 ? Slice(lines, lineIndex - request.ContextBefore, lineIndex - 1) : null,
                    After   = request.ContextAfter  > 0 ? Slice(lines, lineIndex + 1, lineIndex + request.ContextAfter) : null
                });
            }
        }

        return outcome;
    }

    /// <summary>
    /// Compiles the caller's pattern under the non-backtracking engine. <see cref="RegexOptions.Compiled"/>
    /// is deliberately absent — the two options are mutually exclusive in .NET.
    /// </summary>
    private static Regex CompileQuery(SearchRequest request)
    {
        var options = RegexOptions.NonBacktracking;
        if (!request.CaseSensitive) options |= RegexOptions.IgnoreCase;
        if (request.Multiline) options |= RegexOptions.Singleline;

        try
        {
            return new Regex(request.Query, options);
        }
        catch (NotSupportedException ex)
        {
            // Lookaround / backreferences: unsupported here *and* on ripgrep. Saying so plainly beats
            // silently behaving differently depending on which substrate ran the query.
            throw new UnsupportedPatternException(
                $"Pattern uses a construct this search does not support (lookaround and backreferences are unavailable on both search backends): {ex.Message}");
        }
    }

    private static List<(int LineIndex, int Column)> MatchPerLine(
        string[] lines, Regex? queryRegex, SearchRequest request, StringComparison comparison)
    {
        var hits = new List<(int, int)>();
        for (var i = 0; i < lines.Length; i++)
        {
            if (queryRegex is not null)
            {
                var m = queryRegex.Match(lines[i]);
                if (m.Success) hits.Add((i, m.Index + 1));
            }
            else
            {
                var idx = lines[i].IndexOf(request.Query, comparison);
                if (idx >= 0) hits.Add((i, idx + 1));
            }

            if (hits.Count > 0 && request.Mode == SearchOutputMode.FilesWithMatches) break;
        }
        return hits;
    }

    /// <summary>
    /// Matches across line boundaries by running the pattern over the joined text and mapping each
    /// match offset back to a line. Keeps the reported shape identical to the per-line path, so a
    /// multiline result is not a second kind of result downstream.
    /// </summary>
    private static List<(int LineIndex, int Column)> MatchMultiline(
        string[] lines, Regex? queryRegex, SearchRequest request, StringComparison comparison)
    {
        var hits = new List<(int, int)>();
        var text = string.Join('\n', lines);

        // Start offset of each line in the joined text, for offset → (line, column) mapping.
        var lineStarts = new int[lines.Length];
        var offset = 0;
        for (var i = 0; i < lines.Length; i++)
        {
            lineStarts[i] = offset;
            offset += lines[i].Length + 1;
        }

        foreach (var index in EnumerateMatchOffsets(text, queryRegex, request, comparison))
        {
            var line = LineOf(lineStarts, index);
            hits.Add((line, index - lineStarts[line] + 1));
            if (request.Mode == SearchOutputMode.FilesWithMatches) break;
        }

        return hits;
    }

    private static IEnumerable<int> EnumerateMatchOffsets(
        string text, Regex? queryRegex, SearchRequest request, StringComparison comparison)
    {
        if (queryRegex is not null)
        {
            for (var m = queryRegex.Match(text); m.Success; m = m.NextMatch())
            {
                yield return m.Index;
                if (m.Length == 0) yield break;   // zero-width pattern would loop forever
            }
            yield break;
        }

        var from = 0;
        while (from <= text.Length - request.Query.Length)
        {
            var idx = text.IndexOf(request.Query, from, comparison);
            if (idx < 0) yield break;
            yield return idx;
            from = idx + Math.Max(1, request.Query.Length);
        }
    }

    private static int LineOf(int[] lineStarts, int offset)
    {
        var lo = 0;
        var hi = lineStarts.Length - 1;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) / 2;
            if (lineStarts[mid] <= offset) lo = mid; else hi = mid - 1;
        }
        return lo;
    }

    private static List<string> Slice(string[] lines, int from, int to)
    {
        var result = new List<string>();
        for (var i = Math.Max(0, from); i <= Math.Min(lines.Length - 1, to); i++)
            result.Add(lines[i]);
        return result;
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
    /// for glob include/exclude matching.</summary>
    private static string Relative(string root, string file)
    {
        var r = root.Replace('\\', '/').TrimEnd('/');
        var f = file.Replace('\\', '/');
        if (r.Length > 0 && f.StartsWith(r + "/", StringComparison.OrdinalIgnoreCase))
            return f.Substring(r.Length + 1);
        return f.TrimStart('/');
    }
}

/// <summary>The caller's regex uses a construct unavailable on both search backends.</summary>
public sealed class UnsupportedPatternException : Exception
{
    public UnsupportedPatternException(string message) : base(message) { }
}
