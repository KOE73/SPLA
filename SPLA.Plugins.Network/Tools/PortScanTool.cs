using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class PortScanTool : IMcpTool, IToolHelpProvider
{
    public string Name => "network_scan_tcp_ports";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Scans TCP ports on one host. Omitting ports scans a practical common service set (~49 ports). Accepts: 'common', comma list, range (e.g. 1-1024), or 'all'.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Target host or IPv4 address." },
                    ports = new { type = "string", description = "Optional ports: 'common' default, comma list, range, or 'all'." },
                    timeout = new { type = "integer", description = "Per-port timeout in milliseconds. Default: 500." },
                    concurrency = new { type = "integer", description = "Parallel connection attempts. Default: 128, max: 512." }
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
            var root = doc.RootElement;
            var host = ToolJson.GetStringTrimmed(root, "host");
            if (host is null) return "Error: Missing 'host' parameter.";

            var portsValue  = ToolJson.GetString(root, "ports");
            var timeout     = ToolJson.GetInt32Clamped(root, "timeout",     500,   100, 10000);
            var concurrency = ToolJson.GetInt32Clamped(root, "concurrency", 128,   1,   512);

            var ports = NetworkScanHelpers.ParsePorts(portsValue);
            var total = ports.Count;
            var openPorts = new List<int>();
            long done = 0;
            // Cap progress chatter at ~200 ticks regardless of port count.
            var step = Math.Max(1, total / 200);

            await Parallel.ForEachAsync(ports, new ParallelOptions
            {
                MaxDegreeOfParallelism = concurrency,
                CancellationToken = cancellationToken
            }, async (port, token) =>
            {
                var isOpen = await NetworkScanHelpers.CheckPortAsync(host, port, timeout, token);
                if (isOpen)
                {
                    lock (openPorts)
                    {
                        openPorts.Add(port);
                    }
                }

                // Report on every open port (interesting), at each step boundary, and at the end.
                var n = Interlocked.Increment(ref done);
                if (isOpen || n % step == 0 || n == total)
                {
                    int[] snapshot;
                    lock (openPorts) snapshot = openPorts.OrderBy(x => x).ToArray();
                    var details = snapshot.Length > 0
                        ? new[] { new ToolProgressDetail("open", NetworkScanHelpers.FormatPorts(snapshot)) }
                        : null;
                    ProgressScope.Report(n, total, $"scanning {host}", details);
                }
            });

            var orderedOpen = openPorts.OrderBy(x => x).ToArray();
            var sb = new StringBuilder();
            sb.AppendLine($"Port scan: {host}");
            sb.AppendLine($"Ports scanned: {ports.Count}");
            sb.AppendLine($"Timeout: {timeout} ms");
            sb.AppendLine($"Concurrency: {concurrency}");
            sb.AppendLine($"Open ports: {(orderedOpen.Length == 0 ? "none" : NetworkScanHelpers.FormatPorts(orderedOpen))}");
            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error scanning ports: {ex.Message}";
        }
    }

    public string? GetHelpText() =>
        """
        tool: network_scan_tcp_ports

        summary: Scan TCP ports on one host. Use for a single host or IP, not for subnet scanning.

        arguments:
          host:
            required: true
            formats:
              - hostname
              - ipv4_address
              - ipv6_address_if_supported_by_runtime_dns
            examples:
              - 192.168.1.10
              - host.example.com
          ports:
            required: false
            default: common
            formats:
              - common
              - all
              - single_port
              - comma_list
              - range
            examples:
              - common
              - 80
              - 22,80,443,3389
              - 1-1024
          timeout:
            required: false
            default: 500
            min: 100
            max: 10000
            unit: milliseconds
          concurrency:
            required: false
            default: 128
            min: 1
            max: 512

        risk:
          all_ports: use only when explicitly requested by the user.
          remote_scan: avoid scanning hosts the user does not own or administer.

        examples:
          - request:
              host: 192.168.1.10
              ports: 80,443,8080
          - request:
              host: localhost
              ports: common
              timeout: 300
        """;
}
