using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class ReverseDnsTool : IMcpTool
{
    public string Name => "network.dns.reverse";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Performs a reverse DNS lookup (PTR record) for one or more IP addresses. Useful after a LAN scan to resolve IPs to hostnames.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    addresses = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "One or more IPv4/IPv6 addresses to reverse-resolve (e.g. ['192.168.1.1', '8.8.8.8'])."
                    }
                },
                required = new[] { "addresses" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("addresses", out var addressesElement) ||
                addressesElement.ValueKind != JsonValueKind.Array)
            {
                return "Error: Missing or invalid 'addresses' parameter (expected array of IP strings).";
            }

            var ips = new System.Collections.Generic.List<string>();
            foreach (var el in addressesElement.EnumerateArray())
            {
                var s = el.GetString();
                if (!string.IsNullOrWhiteSpace(s)) ips.Add(s.Trim());
            }

            if (ips.Count == 0)
                return "Error: 'addresses' array is empty.";

            // Resolve all addresses in parallel.
            var tasks = ips.Select(ip => ResolveAsync(ip, cancellationToken)).ToArray();
            var results = await Task.WhenAll(tasks);

            var sb = new StringBuilder();
            sb.AppendLine($"Reverse DNS results ({ips.Count} addresses):");
            foreach (var (ip, hostname) in results)
            {
                sb.AppendLine($"  {ip,-40} {hostname}");
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error performing reverse DNS: {ex.Message}";
        }
    }

    private static async Task<(string Ip, string Hostname)> ResolveAsync(string ip, CancellationToken cancellationToken)
    {
        try
        {
            if (!IPAddress.TryParse(ip, out var address))
                return (ip, "invalid IP address");

            var entry = await Dns.GetHostEntryAsync(address.ToString(), cancellationToken: cancellationToken);
            return (ip, entry.HostName);
        }
        catch (Exception ex)
        {
            return (ip, $"unresolved ({ex.Message})");
        }
    }
}
