using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
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
    private static readonly HttpClient HttpClient = new();

    public string Name => "network.http.get";

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
                    maxResponseLength = new { type = "integer", description = "Max characters of the body to return (default: 2000, max: 10000)." },
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
            if (!doc.RootElement.TryGetProperty("url", out var urlElement))
            {
                return "Error: Missing 'url' parameter.";
            }

            var url = urlElement.GetString();
            if (string.IsNullOrWhiteSpace(url))
            {
                return "Error: URL is empty.";
            }

            var maxLen = doc.RootElement.TryGetProperty("maxResponseLength", out var maxLenElement) && maxLenElement.TryGetInt32(out var m)
                ? Math.Clamp(m, 1, 10000)
                : 2000;

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) SPLA/1.0");

            if (doc.RootElement.TryGetProperty("headers", out var headersElement) && headersElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in headersElement.EnumerateObject())
                {
                    var val = prop.Value.GetString();
                    if (val != null)
                    {
                        request.Headers.TryAddWithoutValidation(prop.Name, val);
                    }
                }
            }

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            
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

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            sb.AppendLine("Body:");
            if (body.Length > maxLen)
            {
                sb.AppendLine(body.Substring(0, maxLen));
                sb.AppendLine($"\n[Output truncated. Body length is {body.Length} characters, maxResponseLength was {maxLen}.]");
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
