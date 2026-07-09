using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
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
                    exclude_patterns = new { type = new[] { "array",   "null" }, items = new { type = "string" }, description = "Glob patterns to exclude, e.g. ['bin/*', 'obj/*']. Null = none." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "path", "pattern", "max_results", "exclude_patterns", "output", "output_name" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var pattern        = ToolJson.GetStringTrimmed(root, "pattern") ?? "*";
            var maxResults     = ToolJson.GetInt32Clamped(root, "max_results", 1000, 1, 10000);
            var excludePatterns= ToolJson.GetStringArray(root, "exclude_patterns");

            var ws = HostServices.Sandbox.Workspace;
            var rootPath = ws.MapPathToHost(ToolJson.GetStringTrimmed(root, "path") ?? ".");
            if (rootPath is null)
                return Task.FromResult("Error: File search is not available for this workspace.");

            if (!ws.DirectoryExists(rootPath))
                return Task.FromResult($"Error: Directory not found at {rootPath}");

            var includeRegex = GlobToRegex(pattern);
            var excludeRegexes = excludePatterns?.Select(GlobToRegex).ToList();

            var ignoreFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".git", "bin", "obj", "node_modules"
            };

            var matches = new List<string>();
            FindFilesRecursive(ws, rootPath, rootPath, ignoreFolders, includeRegex, excludeRegexes, matches, maxResults, cancellationToken);

            var result = new FindFilesResult
            {
                Files = matches,
                TotalCount = matches.Count
            };

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (target == OutputTarget.Context) return Task.FromResult(json);
            var blobName = ToolJson.GetStringTrimmed(root, "output_name");
            return Task.FromResult(DataChannel.Route(target, BlobPayload.OfText(json), $"system_find_files: {result.TotalCount} files", blobName));
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
        IWorkspace ws,
        string dir,
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
            foreach (var file in ws.GetFiles(dir))
            {
                if (matches.Count >= maxResults) return;
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = Path.GetRelativePath(rootPath, file).Replace("\\", "/");
                var filename = Path.GetFileName(file);

                // Match both full relative path and just filename for convenience
                if (includeRegex.IsMatch(relativePath) || includeRegex.IsMatch(filename))
                {
                    if (excludeRegexes != null && excludeRegexes.Any(r => r.IsMatch(relativePath) || r.IsMatch(filename)))
                    {
                        continue;
                    }
                    matches.Add(file);
                }
            }

            foreach (var subDir in ws.GetDirectories(dir))
            {
                if (matches.Count >= maxResults) return;
                cancellationToken.ThrowIfCancellationRequested();

                if (ignoreFolders.Contains(Path.GetFileName(subDir))) continue;
                FindFilesRecursive(ws, subDir, rootPath, ignoreFolders, includeRegex, excludeRegexes, matches, maxResults, cancellationToken);
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
