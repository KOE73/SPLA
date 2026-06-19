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

public class HttpRedirectsTool : IMcpTool
{
    // AllowAutoRedirect = false so we can observe each hop manually.
    private static readonly HttpClient HttpClient = new(new HttpClientHandler { AllowAutoRedirect = false })
    {
        Timeout = System.Threading.Timeout.InfiniteTimeSpan
    };

    public string Name => "network_check_http_redirects";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Follows HTTP redirects step-by-step and reports each hop: URL, status code, and Location header. Useful for diagnosing redirect loops, HTTP→HTTPS upgrades, and CDN routing chains.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url          = new { type = "string",  description = "Starting URL to follow." },
                    max_redirects = new { type = "integer", description = "Maximum hops to follow (default: 20)." },
                    timeout      = new { type = "integer", description = "Per-request timeout in milliseconds (default: 10000)." }
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

            var maxRedirects = ToolJson.GetInt32Clamped(root, "max_redirects", 20,     1,    50);
            var timeoutMs    = ToolJson.GetInt32Clamped(root, "timeout",       10_000, 1000, 60_000);

            var sb = new StringBuilder();
            sb.AppendLine($"Redirect chain for: {url}");
            sb.AppendLine();

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int hop = 0;

            while (hop <= maxRedirects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!visited.Add(url!))
                {
                    sb.AppendLine($"[{hop}] LOOP DETECTED — already visited: {url}");
                    break;
                }

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeoutMs);

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 SPLA/1.0");

                HttpResponseMessage response;
                try
                {
                    response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"[{hop}] {url}");
                    sb.AppendLine($"     ERROR: {ex.Message}");
                    break;
                }

                using (response)
                {
                    var status     = (int)response.StatusCode;
                    var isRedirect = status is >= 300 and < 400;
                    var location   = response.Headers.Location;

                    sb.AppendLine($"[{hop}] {status} {response.StatusCode}  {url}");

                    if (isRedirect && location != null)
                    {
                        // Resolve relative Location against current URL
                        string nextUrl;
                        if (location.IsAbsoluteUri)
                        {
                            nextUrl = location.ToString();
                        }
                        else
                        {
                            try { nextUrl = new Uri(new Uri(url!), location).ToString(); }
                            catch { nextUrl = location.ToString(); }
                        }

                        sb.AppendLine($"     → {nextUrl}");
                        url = nextUrl;
                        hop++;
                    }
                    else
                    {
                        if (!isRedirect)
                            sb.AppendLine("     Final destination.");
                        else
                            sb.AppendLine("     Redirect without Location header.");
                        break;
                    }
                }
            }

            if (hop > maxRedirects)
                sb.AppendLine($"Stopped after {maxRedirects} redirects (limit reached).");

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error following redirects: {ex.Message}";
        }
    }
}
