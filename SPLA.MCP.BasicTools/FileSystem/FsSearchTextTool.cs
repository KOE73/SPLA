using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
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
    public string Name => "system.fs.search_text";

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
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    Query = new { type = "string", description = "The text pattern to search for." },
                    Path = new { type = "string", description = "Optional directory path to search. Defaults to current workspace." },
                    Regex = new { type = "boolean", description = "If true, treats Query as a regular expression. Defaults to false." },
                    CaseSensitive = new { type = "boolean", description = "If true, performs case-sensitive match. Defaults to false." },
                    MaxResults = new { type = "integer", description = "Maximum number of results to return. Defaults to 100." },
                    IncludePatterns = new { type = "array", items = new { type = "string" }, description = "Optional glob patterns to include (e.g. ['*.cs'])." },
                    ExcludePatterns = new { type = "array", items = new { type = "string" }, description = "Optional glob patterns to exclude (e.g. ['bin/*', 'obj/*'])." }
                },
                required = new[] { "Query" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var request = JsonSerializer.Deserialize<SearchTextRequest>(argumentsJson, options);
            if (request == null || string.IsNullOrEmpty(request.Query))
            {
                return JsonSerializer.Serialize(new SearchTextResult
                {
                    Query = request?.Query ?? string.Empty,
                    TotalMatches = 0,
                    ReturnedMatches = 0,
                    Matches = new List<SearchMatch>()
                });
            }

            var rootPath = string.IsNullOrEmpty(request.Path) ? Directory.GetCurrentDirectory() : request.Path;
            if (!Directory.Exists(rootPath))
            {
                return $"Error: Directory not found at {rootPath}";
            }

            var maxResults = request.MaxResults <= 0 ? 100 : request.MaxResults;

            List<SearchMatch> rawMatches;
            try
            {
                var rgEngine = new RipgrepSearchEngine();
                rawMatches = await rgEngine.SearchAsync(
                    rootPath,
                    request.Query,
                    request.Regex,
                    request.CaseSensitive,
                    request.IncludePatterns,
                    request.ExcludePatterns,
                    cancellationToken);
            }
            catch
            {
                // Fallback to pure .NET implementation
                var dotnetEngine = new DotnetSearchEngine();
                rawMatches = await dotnetEngine.SearchAsync(
                    rootPath,
                    request.Query,
                    request.Regex,
                    request.CaseSensitive,
                    request.IncludePatterns,
                    request.ExcludePatterns,
                    cancellationToken);
            }

            // Total matches found before ranking and truncation
            int totalMatches = rawMatches.Count;

            // Apply ranking, deduplication, and truncation
            var rankedMatches = SearchRanking.RankAndFilter(rawMatches, request.Query, maxResults);

            var result = new SearchTextResult
            {
                Query = request.Query,
                TotalMatches = totalMatches,
                ReturnedMatches = rankedMatches.Count,
                Matches = rankedMatches
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
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
        tool: system.fs.search_text

        summary: Search text in project files using ripgrep when available, with a .NET fallback and relevance-ranked results.

        arguments:
          Query:
            required: true
            formats:
              - literal_text
              - regex_when_Regex_true
            examples:
              - ToolDescriptor
              - "class\\s+McpHost"
          Path:
            required: false
            default: current_workspace
            formats:
              - absolute_directory_path
              - relative_directory_path
          Regex:
            required: false
            default: false
          CaseSensitive:
            required: false
            default: false
          MaxResults:
            required: false
            default: 100
          IncludePatterns:
            required: false
            formats:
              - glob_array
            examples:
              - ["*.cs"]
              - ["SPLA.MCP.Core/**/*.cs"]
          ExcludePatterns:
            required: false
            formats:
              - glob_array
            examples:
              - ["bin/*", "obj/*"]

        limits:
          resultCapDefault: 100

        examples:
          - request:
              Query: ToolDefinition
              IncludePatterns: ["*.cs"]
              MaxResults: 50
        """;
}

public class SearchTextRequest
{
    public string Query { get; set; } = string.Empty;
    public string? Path { get; set; }
    public bool Regex { get; set; }
    public bool CaseSensitive { get; set; }
    public int MaxResults { get; set; } = 100;
    public string[]? IncludePatterns { get; set; }
    public string[]? ExcludePatterns { get; set; }
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
