using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.BasicTools.Network;

public class WebFetchTool : IMcpTool
{
    private static readonly HttpClient _httpClient = new();

    public string Name => "web_fetch";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Fetches the text content of a concrete URL (HTML is stripped to plain text). Use web_search first for search queries, news lookup, or finding pages; do not pass search-engine result URLs here.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url = new { type = "string", description = "The URL to fetch." }
                },
                required = new[] { "url" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var url = ToolJson.GetStringTrimmed(doc.RootElement, "url");
            if (url is null) return "Error: Missing 'url' parameter.";
            if (!url.StartsWith("http")) url = "https://" + url;

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("SPLA/1.0");
                
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                // Very basic HTML stripping for readability
                var text = Regex.Replace(content, "<style.*?>.*?</style>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                text = Regex.Replace(text, "<script.*?>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                text = Regex.Replace(text, "<.*?>", " ", RegexOptions.Singleline);
                text = WebUtility.HtmlDecode(text);
                text = Regex.Replace(text, @"\s+", " ").Trim();

                if (text.Contains("You are being redirected to the non-JavaScript site", StringComparison.OrdinalIgnoreCase))
                {
                    return "Error: web_fetch received a search-engine redirect page instead of article content. Use web_search for search queries, then web_fetch a concrete result URL.";
                }

                // Limit output to prevent huge context bloat
                if (text.Length > 8000)
                {
                    text = text.Substring(0, 8000) + "\n...[Content truncated due to length]...";
                }

                return text;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return $"Error fetching URL: {ex.Message}";
        }
    }
}
