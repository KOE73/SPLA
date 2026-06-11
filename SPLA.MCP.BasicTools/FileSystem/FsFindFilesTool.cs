using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsFindFilesTool : IMcpTool
{
    public string Name => "system.fs.find_files";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Finds files recursively within the project workspace matching optional glob patterns.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    Path = new { type = "string", description = "Optional directory path to start search. Defaults to current workspace." },
                    Pattern = new { type = "string", description = "Optional search pattern / glob (e.g. '*.cs'). Defaults to '*'" },
                    MaxResults = new { type = "integer", description = "Maximum number of file paths to return. Defaults to 1000." },
                    ExcludePatterns = new { type = "array", items = new { type = "string" }, description = "Optional glob patterns to exclude (e.g. ['bin/*', 'obj/*'])." }
                }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = JsonSerializer.Deserialize<FindFilesRequest>(argumentsJson, options) ?? new FindFilesRequest();

            var rootPath = string.IsNullOrEmpty(request.Path) ? Directory.GetCurrentDirectory() : request.Path;
            if (!Directory.Exists(rootPath))
            {
                return Task.FromResult($"Error: Directory not found at {rootPath}");
            }

            var pattern = string.IsNullOrEmpty(request.Pattern) ? "*" : request.Pattern;
            var maxResults = request.MaxResults <= 0 ? 1000 : request.MaxResults;

            var includeRegex = GlobToRegex(pattern);
            var excludeRegexes = request.ExcludePatterns?.Select(GlobToRegex).ToList();

            var ignoreFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".git", "bin", "obj", "node_modules"
            };

            var matches = new List<string>();
            var rootDir = new DirectoryInfo(rootPath);
            
            FindFilesRecursive(rootDir, rootPath, ignoreFolders, includeRegex, excludeRegexes, matches, maxResults, cancellationToken);

            var result = new FindFilesResult
            {
                Files = matches,
                TotalCount = matches.Count
            };

            return Task.FromResult(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (JsonException)
        {
            return Task.FromResult("Error: Invalid JSON arguments.");
        }
        catch (Exception ex)
        {
            return Task.FromResult($"Error performing file search: {ex.Message}");
        }
    }

    private void FindFilesRecursive(
        DirectoryInfo dir,
        string rootPath,
        HashSet<string> ignoreFolders,
        Regex includeRegex,
        List<Regex>? excludeRegexes,
        List<string> matches,
        int maxResults,
        CancellationToken cancellationToken)
    {
        if (matches.Count >= maxResults) return;
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            foreach (var file in dir.GetFiles())
            {
                if (matches.Count >= maxResults) return;
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = Path.GetRelativePath(rootPath, file.FullName).Replace("\\", "/");
                var filename = file.Name;

                // Match both full relative path and just filename for convenience
                if (includeRegex.IsMatch(relativePath) || includeRegex.IsMatch(filename))
                {
                    if (excludeRegexes != null && excludeRegexes.Any(r => r.IsMatch(relativePath) || r.IsMatch(filename)))
                    {
                        continue;
                    }
                    matches.Add(file.FullName);
                }
            }

            foreach (var subDir in dir.GetDirectories())
            {
                if (matches.Count >= maxResults) return;
                cancellationToken.ThrowIfCancellationRequested();

                if (ignoreFolders.Contains(subDir.Name)) continue;
                FindFilesRecursive(subDir, rootPath, ignoreFolders, includeRegex, excludeRegexes, matches, maxResults, cancellationToken);
            }
        }
        catch
        {
            // Skip folders we don't have access to
        }
    }

    private static Regex GlobToRegex(string glob)
    {
        var pattern = glob.Replace("\\", "/");
        var escaped = Regex.Escape(pattern);
        escaped = escaped.Replace("\\*\\*", ".*")
                         .Replace("\\*", ".*") // Simplified glob for match
                         .Replace("\\?", ".");

        return new Regex("^" + escaped + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}

public class FindFilesRequest
{
    public string? Path { get; set; }
    public string? Pattern { get; set; }
    public int MaxResults { get; set; } = 1000;
    public string[]? ExcludePatterns { get; set; }
}

public class FindFilesResult
{
    public List<string> Files { get; set; } = new();
    public int TotalCount { get; set; }
}
