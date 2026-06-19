using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
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
    public string Name => "system_find_files";

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
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    path             = new { type = new[] { "string",  "null" }, description = "Directory to start search. Null = current workspace." },
                    pattern          = new { type = new[] { "string",  "null" }, description = "Glob pattern, e.g. '*.cs'. Null = '*'." },
                    max_results      = new { type = new[] { "integer", "null" }, description = "Max file paths to return. Null = 1000." },
                    exclude_patterns = new { type = new[] { "array",   "null" }, items = new { type = "string" }, description = "Glob patterns to exclude, e.g. ['bin/*', 'obj/*']. Null = none." }
                },
                required = new[] { "path", "pattern", "max_results", "exclude_patterns" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var rootPath       = ToolJson.GetStringTrimmed(root, "path") ?? Directory.GetCurrentDirectory();
            var pattern        = ToolJson.GetStringTrimmed(root, "pattern") ?? "*";
            var maxResults     = ToolJson.GetInt32Clamped(root, "max_results", 1000, 1, 10000);
            var excludePatterns= ToolJson.GetStringArray(root, "exclude_patterns");

            if (!Directory.Exists(rootPath))
                return Task.FromResult($"Error: Directory not found at {rootPath}");

            var includeRegex = GlobToRegex(pattern);
            var excludeRegexes = excludePatterns?.Select(GlobToRegex).ToList();

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

public class FindFilesResult
{
    public List<string> Files { get; set; } = new();
    public int TotalCount { get; set; }
}
