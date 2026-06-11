using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
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
    public string Name => "network.scan.ports";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Scans TCP ports on one host. If ports are omitted, scans a practical common service set. Use ports like 'common', '1-1024', '22,80,443,3389', or 'all' only when explicitly requested.",
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
            if (!doc.RootElement.TryGetProperty("host", out var hostElement))
            {
                return "Error: Missing 'host' parameter.";
            }

            var host = hostElement.GetString();
            if (string.IsNullOrWhiteSpace(host))
            {
                return "Error: Host is empty.";
            }

            var portsValue = doc.RootElement.TryGetProperty("ports", out var portsElement) ? portsElement.GetString() : null;
            var timeout = doc.RootElement.TryGetProperty("timeout", out var timeoutElement) && timeoutElement.TryGetInt32(out var parsedTimeout)
                ? Math.Clamp(parsedTimeout, 100, 10000)
                : 500;
            var concurrency = doc.RootElement.TryGetProperty("concurrency", out var concurrencyElement) && concurrencyElement.TryGetInt32(out var parsedConcurrency)
                ? Math.Clamp(parsedConcurrency, 1, 512)
                : 128;

            var ports = NetworkScanHelpers.ParsePorts(portsValue);
            var openPorts = new List<int>();

            await Parallel.ForEachAsync(ports, new ParallelOptions
            {
                MaxDegreeOfParallelism = concurrency,
                CancellationToken = cancellationToken
            }, async (port, token) =>
            {
                if (await NetworkScanHelpers.CheckPortAsync(host, port, timeout, token))
                {
                    lock (openPorts)
                    {
                        openPorts.Add(port);
                    }
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
        tool: network.scan.ports

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
