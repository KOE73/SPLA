using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class TraceRouteTool : IMcpTool
{
    public string Name => "network.diag.traceroute";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Performs a traceroute to a host using ICMP echo requests with increasing TTLs.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Target IP address or domain name (e.g. '8.8.8.8' or 'google.com')." },
                    maxHops = new { type = "integer", description = "Maximum number of hops to trace (default: 20, max: 30)." },
                    timeout = new { type = "integer", description = "Ping timeout in milliseconds for each hop (default: 1000)." }
                },
                required = new[] { "host" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("host", out var hostElement))
            {
                return "Error: Missing 'host' parameter.";
            }

            var host = hostElement.GetString();
            if (string.IsNullOrWhiteSpace(host))
            {
                return "Error: Host is empty.";
            }

            var maxHops = doc.RootElement.TryGetProperty("maxHops", out var maxHopsElement) && maxHopsElement.TryGetInt32(out var m)
                ? Math.Clamp(m, 1, 30)
                : 20;

            var timeout = doc.RootElement.TryGetProperty("timeout", out var timeoutElement) && timeoutElement.TryGetInt32(out var t)
                ? t
                : 1000;

            using var ping = new Ping();
            var sb = new StringBuilder();
            sb.AppendLine($"Traceroute to {host} (max {maxHops} hops):");

            for (var ttl = 1; ttl <= maxHops; ttl++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var options = new PingOptions(ttl, true);
                var reply = await ping.SendPingAsync(host, timeout, Array.Empty<byte>(), options);

                if (reply.Status == IPStatus.TtlExpired)
                {
                    sb.AppendLine($"{ttl,2}. {reply.Address}");
                }
                else if (reply.Status == IPStatus.Success)
                {
                    sb.AppendLine($"{ttl,2}. {reply.Address} (Reached)");
                    break;
                }
                else if (reply.Status == IPStatus.TimedOut)
                {
                    sb.AppendLine($"{ttl,2}. *");
                }
                else
                {
                    sb.AppendLine($"{ttl,2}. * (Error: {reply.Status})");
                }
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error performing traceroute: {ex.Message}";
        }
    }
}
