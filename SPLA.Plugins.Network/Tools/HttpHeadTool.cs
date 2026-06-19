using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class HttpHeadTool : IMcpTool
{
    // Timeout.InfiniteTimeSpan — per-request timeout is controlled via CancellationTokenSource below.
    private static readonly HttpClient HttpClient = new() { Timeout = System.Threading.Timeout.InfiniteTimeSpan };

    public string Name => "network_http_head";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Sends an HTTP HEAD request to a URL to retrieve only HTTP status and headers.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url = new { type = "string", description = "Target HTTP/HTTPS URL (e.g. 'https://example.com/index.html')." },
                    timeout = new { type = "integer", description = "Request timeout in milliseconds (default: 30000)." }
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
            var root = doc.RootElement;
            var url = ToolJson.GetStringTrimmed(root, "url");
            if (url is null) return "Error: Missing 'url' parameter.";

            var timeoutMs = ToolJson.GetInt32Clamped(root, "timeout", 30_000, 1000, 300_000);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) SPLA/1.0");

            using var response = await HttpClient.SendAsync(request, cts.Token);
            
            var sb = new StringBuilder();
            sb.AppendLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            sb.AppendLine("Headers:");
            foreach (var header in response.Headers)
            {
                sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
            foreach (var header in response.Content.Headers)
            {
                sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error executing HTTP HEAD: {ex.Message}";
        }
    }
}
