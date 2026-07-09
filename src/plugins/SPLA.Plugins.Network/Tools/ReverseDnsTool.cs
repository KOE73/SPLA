using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class ReverseDnsTool : IMcpTool, IToolHelpProvider
{
    public string Name => "network_reverse_dns";

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
                        description = "JSON array of IPv4/IPv6 address strings to reverse-resolve, e.g. {\"addresses\":[\"192.168.1.1\",\"8.8.8.8\"]}. Do not pass a comma-separated string."
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
            var ips = ToolJson.GetStringArray(doc.RootElement, "addresses")
                          ?.Where(s => !string.IsNullOrWhiteSpace(s))
                          .Select(s => s.Trim())
                          .ToList();
            if (ips is null || ips.Count == 0)
                return "Error: Missing or empty 'addresses' parameter (expected array of IP strings).";

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

    public string? GetHelpText() =>
        """
        tool: network_reverse_dns

        summary: Bulk reverse DNS lookup. Resolves PTR names for one or more IP addresses.

        arguments:
          addresses:
            required: true
            type: array
            items: string
            formats:
              - ipv4_address
              - ipv6_address
            rules:
              - Pass a real JSON array of strings.
              - Do not pass a comma-separated string.
              - Do not wrap the array in another string.
              - Batching is optional; use smaller batches only if the model or endpoint struggles with large tool arguments.
            examples:
              - {"addresses":["172.16.21.0","172.16.21.1","172.16.21.2"]}
              - {"addresses":["192.168.1.1","8.8.8.8"]}

        common mistakes:
          - wrong: {"addresses":"172.16.21.0,172.16.21.1"}
          - wrong: {"addresses":"[\"172.16.21.0\",\"172.16.21.1\"]"}
          - right: {"addresses":["172.16.21.0","172.16.21.1"]}
        """;

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
