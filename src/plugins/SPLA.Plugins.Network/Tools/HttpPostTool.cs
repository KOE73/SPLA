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

public class HttpPostTool : IMcpTool
{
    // Timeout.InfiniteTimeSpan — per-request timeout is controlled via CancellationTokenSource.
    private static readonly HttpClient HttpClient = new() { Timeout = System.Threading.Timeout.InfiniteTimeSpan };

    public string Name => "network_http_post";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Sends an HTTP POST request with an optional body and returns status, headers, and truncated response body.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url = new { type = "string", description = "Target HTTP/HTTPS URL." },
                    body = new { type = "string", description = "Request body (e.g. JSON string or form data). Optional." },
                    content_type = new { type = "string", description = "Content-Type header (default: 'application/json')." },
                    timeout = new { type = "integer", description = "Request timeout in milliseconds (default: 30000)." },
                    max_response_length = new { type = "integer", description = "Max characters of the response body to return (default: 2000, max: 10000)." },
                    headers = new
                    {
                        type = "object",
                        description = "Optional extra HTTP request headers (key-value pairs)."
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

            var body        = ToolJson.GetString(root, "body") ?? "";
            var contentType = ToolJson.GetString(root, "content_type") ?? "application/json";
            var maxLen      = ToolJson.GetInt32Clamped(root, "max_response_length", 2000,  1,    10000);
            var timeoutMs   = ToolJson.GetInt32Clamped(root, "timeout",             30_000, 1000, 300_000);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) SPLA/1.0");
            request.Content = new StringContent(body, Encoding.UTF8, contentType);

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
                sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            foreach (var header in response.Content.Headers)
                sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");

            var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
            sb.AppendLine("Body:");
            if (responseBody.Length > maxLen)
            {
                sb.AppendLine(responseBody.Substring(0, maxLen));
                sb.AppendLine($"\n[Output truncated. Body length is {responseBody.Length} characters, max_response_length was {maxLen}.]");
            }
            else
            {
                sb.AppendLine(responseBody);
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error executing HTTP POST: {ex.Message}";
        }
    }
}
