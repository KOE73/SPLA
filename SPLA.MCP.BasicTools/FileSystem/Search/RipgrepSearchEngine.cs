using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.FileSystem.Search;

public class RipgrepSearchEngine : ISearchEngine
{
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

        var args = new List<string> { "--json" };

        if (!caseSensitive)
        {
            args.Add("--ignore-case");
        }
        else
        {
            args.Add("--case-sensitive");
        }

        if (!isRegex)
        {
            args.Add("-F");
        }

        // Include / Exclude globs
        if (includePatterns != null)
        {
            foreach (var pattern in includePatterns)
            {
                args.Add("-g");
                args.Add(pattern);
            }
        }

        if (excludePatterns != null)
        {
            foreach (var pattern in excludePatterns)
            {
                args.Add("-g");
                args.Add($"!{pattern}");
            }
        }

        // Add default ignores to be safe
        var defaultIgnores = new[] { ".git", "bin", "obj", "node_modules" };
        foreach (var ignore in defaultIgnores)
        {
            args.Add("-g");
            args.Add($"!{ignore}/*");
            args.Add("-g");
            args.Add($"!**/ {ignore}/**");
        }

        args.Add("-e");
        args.Add(query);
        args.Add(".");

        var psi = new ProcessStartInfo
        {
            FileName = "rg",
            WorkingDirectory = rootPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };

        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        try
        {
            using var process = new Process { StartInfo = psi };
            process.Start();

            var outputTask = ReadOutputAsync(process.StandardOutput, rootPath, matches, cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            await outputTask;
        }
        catch (Exception ex)
        {
            // If rg fails to start (e.g. not on path), throw to trigger fallback
            throw new InvalidOperationException("Ripgrep execution failed", ex);
        }

        return matches;
    }

    private async Task ReadOutputAsync(StreamReader reader, string rootPath, List<SearchMatch> matches, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                if (root.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "match")
                {
                    var data = root.GetProperty("data");
                    var filePath = data.GetProperty("path").GetProperty("text").GetString() ?? string.Empty;
                    var fullPath = Path.IsPathRooted(filePath) ? filePath : Path.GetFullPath(Path.Combine(rootPath, filePath));

                    var lineNumber = data.GetProperty("line_number").GetInt32();
                    var preview = data.GetProperty("lines").GetProperty("text").GetString() ?? string.Empty;

                    // Get column from first submatch
                    var column = 1;
                    if (data.TryGetProperty("submatches", out var submatchesProp) && submatchesProp.GetArrayLength() > 0)
                    {
                        column = submatchesProp[0].GetProperty("start").GetInt32() + 1;
                    }

                    matches.Add(new SearchMatch
                    {
                        File = fullPath,
                        Line = lineNumber,
                        Column = column,
                        Preview = preview.TrimEnd('\r', '\n')
                    });
                }
            }
            catch
            {
                // Ignore parsing errors for individual lines
            }
        }
    }
}
