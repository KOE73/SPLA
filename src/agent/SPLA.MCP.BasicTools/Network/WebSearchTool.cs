using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
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
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string", description = "Search query." },
                    engines = new
                    {
                        type = new[] { "array", "null" },
                        items = new { type = "string" },
                        description = "Search engines to use: 'duckduckgo' (or 'ddg'), 'yahoo', 'google', 'bing'. Null = DuckDuckGo → Yahoo fallback."
                    },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "query", "engines", "output", "output_name" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var query = ToolJson.GetStringTrimmed(doc.RootElement, "query");
            if (query is null) return "Error: Missing 'query' parameter.";

            var rawEngines = ToolJson.GetStringArray(doc.RootElement, "engines");
            var requestedEngines = rawEngines?
                .Select(e => { var n = e.ToLowerInvariant(); return n == "ddg" ? "duckduckgo" : n; })
                .ToList() ?? new List<string>();

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
                        var searchResult = string.Join("\n\n", lines);
                        var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));
                        if (target == OutputTarget.Context) return searchResult;
                        var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
                        return DataChannel.Route(target, BlobPayload.OfText(searchResult), $"web_search: {query}", blobName);
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
