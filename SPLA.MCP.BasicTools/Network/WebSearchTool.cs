using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.BasicTools.Network.SearchEngines;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.Network;

public class WebSearchTool : IMcpTool
{
    private static readonly HttpClient HttpClient = new();

    public string Name => "web_search";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Searches the web using one or more search engines and returns a list of results with attribution.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string", description = "Search query." },
                    engines = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Optional list of search engines to use. Available: 'duckduckgo' (or 'ddg'), 'yahoo', 'google', 'bing'. If omitted, queries DuckDuckGo first, then falls back to Yahoo."
                    }
                },
                required = new[] { "query" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("query", out var queryElement))
            {
                return "Error: Missing 'query' parameter.";
            }

            var query = queryElement.GetString();
            if (string.IsNullOrWhiteSpace(query))
            {
                return "Error: Query is empty.";
            }

            var requestedEngines = new List<string>();
            if (doc.RootElement.TryGetProperty("engines", out var enginesElement) && enginesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in enginesElement.EnumerateArray())
                {
                    var engineName = item.GetString()?.ToLowerInvariant();
                    if (!string.IsNullOrEmpty(engineName))
                    {
                        if (engineName == "ddg") engineName = "duckduckgo";
                        requestedEngines.Add(engineName);
                    }
                }
            }

            // Default fallback chain if no engines specified
            if (requestedEngines.Count == 0)
            {
                requestedEngines.Add("duckduckgo");
                requestedEngines.Add("yahoo");
            }

            var availableEngines = new Dictionary<string, ISearchEngine>(StringComparer.OrdinalIgnoreCase)
            {
                { "duckduckgo", new DuckDuckGoEngine(HttpClient) },
                { "yahoo", new YahooEngine(HttpClient) },
                { "google", new GoogleEngine(HttpClient) },
                { "bing", new BingEngine(HttpClient) }
            };

            var errors = new List<string>();
            foreach (var name in requestedEngines)
            {
                if (!availableEngines.TryGetValue(name, out var engine))
                {
                    errors.Add($"{name}: Unsupported engine.");
                    continue;
                }

                try
                {
                    var results = await engine.SearchAsync(query, cancellationToken);
                    if (results != null && results.Count > 0)
                    {
                        var lines = new List<string>();
                        var count = Math.Min(results.Count, 8);
                        for (var i = 0; i < count; i++)
                        {
                            var r = results[i];
                            lines.Add($"[{engine.Name.ToUpperInvariant()}] {i + 1}. {r.Title}\n   {r.Url}\n   {r.Snippet}");
                        }
                        return string.Join("\n\n", lines);
                    }
                    else
                    {
                        errors.Add($"{engine.Name}: Returned no results.");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{engine.Name}: {ex.Message}");
                }
            }

            return "Error: Search returned no parseable results from all requested sources:\n" + string.Join("\n", errors);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return $"Error parsing arguments: {ex.Message}";
        }
    }
}
