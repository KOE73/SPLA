using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.MCP.BasicTools.FileSystem.Search;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SPLA.MCP.BasicTools.FileSystem;

public class FsSearchTextTool : IMcpTool, IToolHelpProvider
{
    public string Name => "system_search_text";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Production-ready advanced search subsystem for SPLA. Search text in files using Ripgrep with .NET fallback, with relevance ranking, cap results and duplicate reduction.",
            Scope = ToolScope.Project,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    query            = new { type = "string",                      description = "Text pattern to search for." },
                    path             = new { type = new[] { "string",  "null" },   description = "Directory to search. Null = current workspace." },
                    regex            = new { type = new[] { "boolean", "null" },   description = "Treat query as regex. Null = false." },
                    case_sensitive   = new { type = new[] { "boolean", "null" },   description = "Case-sensitive match. Null = false." },
                    max_results      = new { type = new[] { "integer", "null" },   description = "Max results. Null = 100." },
                    include_patterns = new { type = new[] { "array",   "null" }, items = new { type = "string" }, description = "Glob patterns to include, e.g. ['*.cs']. Null = all files." },
                    exclude_patterns = new { type = new[] { "array",   "null" }, items = new { type = "string" }, description = "Glob patterns to exclude, e.g. ['bin/*']. Null = none." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "query", "path", "regex", "case_sensitive", "max_results", "include_patterns", "exclude_patterns", "output", "output_name" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var query          = ToolJson.GetStringTrimmed(root, "query");
            var path           = ToolJson.GetStringTrimmed(root, "path");
            var regex          = ToolJson.GetBoolean(root, "regex", false);
            var caseSensitive  = ToolJson.GetBoolean(root, "case_sensitive", false);
            var maxResults     = ToolJson.GetInt32Clamped(root, "max_results", 100, 1, 10000);
            var includePatterns= ToolJson.GetStringArray(root, "include_patterns");
            var excludePatterns= ToolJson.GetStringArray(root, "exclude_patterns");

            if (string.IsNullOrEmpty(query))
                return JsonSerializer.Serialize(new SearchTextResult { Query = string.Empty, TotalMatches = 0, ReturnedMatches = 0, Matches = new List<SearchMatch>() });

            var rootPath = path ?? Directory.GetCurrentDirectory();
            if (!Directory.Exists(rootPath))
                return $"Error: Directory not found at {rootPath}";

            List<SearchMatch> rawMatches;
            try
            {
                var rgEngine = new RipgrepSearchEngine();
                rawMatches = await rgEngine.SearchAsync(
                    rootPath, query, regex, caseSensitive,
                    includePatterns, excludePatterns, cancellationToken);
            }
            catch
            {
                var dotnetEngine = new DotnetSearchEngine();
                rawMatches = await dotnetEngine.SearchAsync(
                    rootPath, query, regex, caseSensitive,
                    includePatterns, excludePatterns, cancellationToken);
            }

            int totalMatches  = rawMatches.Count;
            var rankedMatches = SearchRanking.RankAndFilter(rawMatches, query, maxResults);

            var result = new SearchTextResult
            {
                Query = query,
                TotalMatches = totalMatches,
                ReturnedMatches = rankedMatches.Count,
                Matches = rankedMatches
            };

            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (target == OutputTarget.Context) return json;
            var blobName = ToolJson.GetStringTrimmed(root, "output_name");
            return DataChannel.Route(target, BlobPayload.OfText(json), $"system_search_text: {result.ReturnedMatches}/{result.TotalMatches} matches for '{query}'", blobName);
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error performing search: {ex.Message}";
        }
    }

    public string? GetHelpText() =>
        """
        tool: system_search_text

        summary: Search text in project files using ripgrep when available, with a .NET fallback and relevance-ranked results.

        arguments:
          query:
            required: true
            formats:
              - literal_text
              - regex_when_regex_true
            examples:
              - ToolDescriptor
              - "class\\s+McpHost"
          path:
            required: false
            default: current_workspace
            formats:
              - absolute_directory_path
              - relative_directory_path
          regex:
            required: false
            default: false
          case_sensitive:
            required: false
            default: false
          max_results:
            required: false
            default: 100
          include_patterns:
            required: false
            formats:
              - glob_array
            examples:
              - ["*.cs"]
              - ["SPLA.MCP.Core/**/*.cs"]
          exclude_patterns:
            required: false
            formats:
              - glob_array
            examples:
              - ["bin/*", "obj/*"]

        limits:
          resultCapDefault: 100

        examples:
          - request:
              query: ToolDefinition
              include_patterns: ["*.cs"]
              max_results: 50
        """;
}

public class SearchTextResult
{
    public string Query { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int ReturnedMatches { get; set; }
    public List<SearchMatch> Matches { get; set; } = new();
}

public class SearchMatch
{
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public string Preview { get; set; } = string.Empty;
}
