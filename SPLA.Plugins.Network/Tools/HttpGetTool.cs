using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class HttpGetTool : IMcpTool
{
    // Timeout.InfiniteTimeSpan — per-request timeout is controlled via CancellationTokenSource below.
    private static readonly HttpClient HttpClient = new() { Timeout = System.Threading.Timeout.InfiniteTimeSpan };

    public string Name => "network_http_get";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Sends an HTTP GET request to a URL and returns status, headers, and truncated body (to avoid context overflow).",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url = new { type = "string", description = "Target HTTP/HTTPS URL (e.g. 'https://api.github.com/status')." },
                    max_response_length = new { type = "integer", description = "Max characters of the body to return (default: 2000, max: 10000)." },
                    timeout = new { type = "integer", description = "Request timeout in milliseconds (default: 30000)." },
                    headers = new
                    {
                        type = "object",
                        description = "Optional HTTP request headers (key-value pairs)."
                    }
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

            var maxLen    = ToolJson.GetInt32Clamped(root, "max_response_length", 2000,  1,       10000);
            var timeoutMs = ToolJson.GetInt32Clamped(root, "timeout",             30_000, 1000,   300_000);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) SPLA/1.0");

            var headers = ToolJson.GetStringDictionary(root, "headers");
            if (headers != null)
            {
                foreach (var (key, val) in headers)
                    request.Headers.TryAddWithoutValidation(key, val);
            }

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

            var body = await response.Content.ReadAsStringAsync(cts.Token);
            sb.AppendLine("Body:");
            if (body.Length > maxLen)
            {
                sb.AppendLine(body.Substring(0, maxLen));
                sb.AppendLine($"\n[Output truncated. Body length is {body.Length} characters, max_response_length was {maxLen}.]");
            }
            else
            {
                sb.AppendLine(body);
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error executing HTTP GET: {ex.Message}";
        }
    }
}
