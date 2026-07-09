using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
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
            Description = "Fetches the text content of a concrete URL (HTML is stripped to plain text). " +
                          "Use web_search first for search queries; do not pass search-engine result URLs here. " +
                          "Set output='blob' to store the full page body as a blob:<handle> without flooding context.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url         = new { type = "string", description = "The URL to fetch." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "url", "output", "output_name" }
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

                var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(doc.RootElement, "output"));

                // Only apply the 8000-char cap when going to context; blob captures the full text.
                if (target == OutputTarget.Context && text.Length > 8000)
                    text = text[..8000] + "\n...[Content truncated — use output='blob' to capture in full]...";

                if (target == OutputTarget.Context) return text;
                var blobName = ToolJson.GetStringTrimmed(doc.RootElement, "output_name");
                return DataChannel.Route(target, BlobPayload.OfText(text), $"web_fetch: {url} ({text.Length} chars)", blobName);
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
