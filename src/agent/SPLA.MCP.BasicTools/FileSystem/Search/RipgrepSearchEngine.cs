using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

/// <summary>
/// Search over a real disk path by driving ripgrep. The fast path, and the only one that gets
/// <c>.gitignore</c> awareness, encoding detection and binary sniffing for free.
/// <para>
/// The binary is resolved by explicit path (<see cref="RipgrepBinary"/>), never off bare PATH. When
/// it is absent or fails, the caller falls back to <see cref="WorkspaceSearchEngine"/> — see
/// <c>docs/adr/ADR_20260831_mcp_search-and-listing-tools.md</c> §2.0.
/// </para>
/// </summary>
public class RipgrepSearchEngine : ISearchEngine
{
    /// <summary>Folders never searched, whatever the caller asked for.</summary>
    private static readonly string[] DefaultIgnores = { ".git", "bin", "obj", "node_modules" };

    public async Task<SearchOutcome> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        var exePath = RipgrepBinary.ExecutablePath
            ?? throw new RipgrepUnavailableException("ripgrep binary not found");

        var args = BuildArguments(request);

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = request.RootPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var arg in args) psi.ArgumentList.Add(arg);

        var outcome = new SearchOutcome();
        string stderr;
        int exitCode;

        try
        {
            using var process = new Process { StartInfo = psi };
            process.Start();

            // stderr is read, not discarded: a silent fallback hides why the fast path died, which is
            // exactly the failure this engine used to have.
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            var outputTask = ReadOutputAsync(process.StandardOutput, request, outcome, cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            await outputTask;
            stderr = await errorTask;
            exitCode = process.ExitCode;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            throw new RipgrepUnavailableException($"ripgrep failed to start ({exePath})", ex);
        }

        // rg exits 0 = matches, 1 = no matches, 2+ = real error.
        if (exitCode >= 2)
        {
            var detail = string.IsNullOrWhiteSpace(stderr) ? $"exit code {exitCode}" : stderr.Trim();
            throw new RipgrepUnavailableException($"ripgrep error: {detail}");
        }

        return outcome;
    }

    private static List<string> BuildArguments(SearchRequest request)
    {
        var args = new List<string>();

        // Only Content mode needs the structured stream; the other two have far cheaper output forms
        // and let rg itself stop early instead of us collecting line text nobody asked for.
        switch (request.Mode)
        {
            case SearchOutputMode.FilesWithMatches: args.Add("--files-with-matches"); break;
            case SearchOutputMode.Count:            args.Add("--count"); break;
            default:                                args.Add("--json"); break;
        }

        args.Add(request.CaseSensitive ? "--case-sensitive" : "--ignore-case");

        if (!request.IsRegex) args.Add("-F");
        if (request.Multiline) { args.Add("--multiline"); args.Add("--multiline-dotall"); }

        if (request.Mode == SearchOutputMode.Content)
        {
            if (request.ContextBefore > 0) { args.Add("-B"); args.Add(request.ContextBefore.ToString()); }
            if (request.ContextAfter  > 0) { args.Add("-A"); args.Add(request.ContextAfter.ToString()); }
        }

        foreach (var pattern in request.IncludePatterns ?? Array.Empty<string>())
        {
            args.Add("-g");
            args.Add(pattern);
        }

        foreach (var pattern in request.ExcludePatterns ?? Array.Empty<string>())
        {
            args.Add("-g");
            args.Add($"!{pattern}");
        }

        // Default ignores. The second form used to read "!**/ {name}/**" — the stray space made it a
        // glob that matches nothing, so these folders were never actually excluded here.
        foreach (var ignore in DefaultIgnores)
        {
            args.Add("-g");
            args.Add($"!{ignore}/**");
            args.Add("-g");
            args.Add($"!**/{ignore}/**");
        }

        args.Add("-e");
        args.Add(request.Query);
        args.Add(".");
        return args;
    }

    private static async Task ReadOutputAsync(
        StreamReader reader, SearchRequest request, SearchOutcome outcome, CancellationToken cancellationToken)
    {
        if (request.Mode != SearchOutputMode.Content)
        {
            await ReadPlainAsync(reader, request, outcome, cancellationToken);
            return;
        }

        // Matches and context lines arrive interleaved per file. Collect both, then stitch context
        // onto each match by line number once the file's stream is done.
        var matchesByFile = new Dictionary<string, List<SearchMatch>>(StringComparer.Ordinal);
        var contextByFile = new Dictionary<string, Dictionary<int, string>>(StringComparer.Ordinal);
        var countByFile   = new Dictionary<string, int>(StringComparer.Ordinal);
        var order         = new List<string>();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var typeProp)) continue;
                var type = typeProp.GetString();
                if (type is not ("match" or "context")) continue;

                var data = root.GetProperty("data");
                var relative = data.GetProperty("path").GetProperty("text").GetString() ?? string.Empty;
                var fullPath = Path.IsPathRooted(relative)
                    ? relative
                    : Path.GetFullPath(Path.Combine(request.RootPath, relative));
                var lineNumber = data.TryGetProperty("line_number", out var ln) ? ln.GetInt32() : 0;
                var text = (data.GetProperty("lines").GetProperty("text").GetString() ?? string.Empty)
                    .TrimEnd('\r', '\n');

                if (type == "context")
                {
                    if (!contextByFile.TryGetValue(fullPath, out var ctx))
                        contextByFile[fullPath] = ctx = new Dictionary<int, string>();
                    ctx[lineNumber] = text;
                    continue;
                }

                var column = 1;
                if (data.TryGetProperty("submatches", out var subs) && subs.GetArrayLength() > 0)
                    column = subs[0].GetProperty("start").GetInt32() + 1;

                if (!matchesByFile.TryGetValue(fullPath, out var list))
                {
                    matchesByFile[fullPath] = list = new List<SearchMatch>();
                    order.Add(fullPath);
                }
                list.Add(new SearchMatch { File = fullPath, Line = lineNumber, Column = column, Preview = text });
                countByFile[fullPath] = countByFile.GetValueOrDefault(fullPath) + 1;
            }
            catch (JsonException)
            {
                // A single malformed event must not abort a search that is otherwise fine.
            }
        }

        foreach (var file in order)
        {
            var ctx = contextByFile.GetValueOrDefault(file);
            foreach (var match in matchesByFile[file])
            {
                if (ctx is not null)
                {
                    if (request.ContextBefore > 0)
                        match.Before = Collect(ctx, match.Line - request.ContextBefore, match.Line - 1);
                    if (request.ContextAfter > 0)
                        match.After = Collect(ctx, match.Line + 1, match.Line + request.ContextAfter);
                }
                outcome.Matches.Add(match);
            }
            outcome.Files.Add(new SearchFileHit { File = file, MatchCount = countByFile[file] });
        }
    }

    /// <summary>Reads the line-per-file output of <c>--files-with-matches</c> / <c>--count</c>.</summary>
    private static async Task ReadPlainAsync(
        StreamReader reader, SearchRequest request, SearchOutcome outcome, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var path = line;
            var count = 0;

            if (request.Mode == SearchOutputMode.Count)
            {
                // "<path>:<count>" — split on the LAST colon so drive letters and colons in names survive.
                var sep = line.LastIndexOf(':');
                if (sep <= 0) continue;
                path = line.Substring(0, sep);
                if (!int.TryParse(line.AsSpan(sep + 1), out count)) continue;
            }

            var fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(request.RootPath, path));
            outcome.Files.Add(new SearchFileHit { File = fullPath, MatchCount = count });
        }
    }

    private static List<string> Collect(Dictionary<int, string> context, int from, int to)
    {
        var result = new List<string>();
        for (var i = from; i <= to; i++)
            if (i > 0 && context.TryGetValue(i, out var text)) result.Add(text);
        return result;
    }
}

/// <summary>
/// Ripgrep could not be used for this search — missing binary, failed launch, or a real rg error.
/// Distinct from a generic exception so the caller falls back deliberately and can say <em>why</em>
/// the fast path was unavailable instead of degrading silently.
/// </summary>
public sealed class RipgrepUnavailableException : Exception
{
    public RipgrepUnavailableException(string message, Exception? inner = null) : base(message, inner) { }
}
