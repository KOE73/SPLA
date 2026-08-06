using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class PingTool : IMcpTool
{
    public string Name => "network_ping_host";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Pings a host (IP address or domain name) using ICMP echo requests to check connectivity.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Target IP address or domain name (e.g. '8.8.8.8' or 'google.com')." },
                    timeout = new { type = "integer", description = "Timeout in milliseconds (default: 5000)." }
                },
                required = new[] { "host" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;
            var host = ToolJson.GetStringTrimmed(root, "host");
            if (host is null) return ToolResult.Fail("Error: Missing 'host' parameter.", "missing host");

            var timeout = ToolJson.GetInt32(root, "timeout", 5000);

            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, timeout);

            if (reply.Status == IPStatus.Success)
            {
                return ToolResult.Text($"Status: Success\n" +
                       $"Address: {reply.Address}\n" +
                       $"RoundTrip time: {reply.RoundtripTime} ms\n" +
                       $"TTL: {reply.Options?.Ttl}");
            }

            return ToolResult.Text($"Status: Failed ({reply.Status})");
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error pinging host: {ex.Message}", "ping failed");
        }
    }
}
