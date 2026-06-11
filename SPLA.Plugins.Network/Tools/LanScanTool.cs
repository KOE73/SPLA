using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class LanScanTool : IMcpTool, IToolHelpProvider
{
    public string Name => "network.scan.lan";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Discovers responsive hosts in a bounded IPv4 LAN range. Defaults to ping-only; optional ports are only a lightweight host-presence probe.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    cidr = new { type = "string", description = "IPv4 CIDR range, e.g. 192.168.1.0/24." },
                    start = new { type = "string", description = "Start IPv4 address if cidr is not used." },
                    end = new { type = "string", description = "End IPv4 address if cidr is not used." },
                    ports = new { type = "string", description = "Optional TCP ports for host-presence probing. If omitted, no ports are scanned." },
                    ping = new { type = "boolean", description = "Use ICMP ping. Default: true." },
                    timeout = new { type = "integer", description = "Ping/connect timeout in milliseconds. Default: 500." },
                    concurrency = new { type = "integer", description = "Parallel host scans. Default: 64, max: 256." },
                    maxHosts = new { type = "integer", description = "Safety limit for host count. Default: 4096, max: 4096." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var cidr = doc.RootElement.TryGetProperty("cidr", out var cidrElement) ? cidrElement.GetString() : null;
            var start = doc.RootElement.TryGetProperty("start", out var startElement) ? startElement.GetString() : null;
            var end = doc.RootElement.TryGetProperty("end", out var endElement) ? endElement.GetString() : null;
            var portsValue = doc.RootElement.TryGetProperty("ports", out var portsElement) ? portsElement.GetString() : null;
            var usePortProbe = !string.IsNullOrWhiteSpace(portsValue);
            var usePing = !doc.RootElement.TryGetProperty("ping", out var pingElement) || pingElement.ValueKind != JsonValueKind.False;
            var timeout = doc.RootElement.TryGetProperty("timeout", out var timeoutElement) && timeoutElement.TryGetInt32(out var parsedTimeout)
                ? Math.Clamp(parsedTimeout, 100, 10000)
                : 500;
            var concurrency = doc.RootElement.TryGetProperty("concurrency", out var concurrencyElement) && concurrencyElement.TryGetInt32(out var parsedConcurrency)
                ? Math.Clamp(parsedConcurrency, 1, 256)
                : 64;
            var maxHosts = doc.RootElement.TryGetProperty("maxHosts", out var maxHostsElement) && maxHostsElement.TryGetInt32(out var parsedMaxHosts)
                ? Math.Clamp(parsedMaxHosts, 1, 4096)
                : 4096;

            if (!usePing && !usePortProbe)
            {
                return "Error: Nothing to scan. Enable ping or provide ports.";
            }

            var hosts = NetworkScanHelpers.ParseHostRange(cidr, start, end, maxHosts);
            var ports = usePortProbe ? NetworkScanHelpers.ParsePorts(portsValue) : Array.Empty<int>();
            var results = new ConcurrentBag<HostScanResult>();

            await Parallel.ForEachAsync(hosts, new ParallelOptions
            {
                MaxDegreeOfParallelism = concurrency,
                CancellationToken = cancellationToken
            }, async (address, token) =>
            {
                var host = address.ToString();
                var pingAlive = !usePortProbe && usePing && await NetworkScanHelpers.PingAsync(address, timeout, token);
                int? openPort = null;

                if (usePortProbe)
                {
                    foreach (var port in ports)
                    {
                        token.ThrowIfCancellationRequested();
                        if (await NetworkScanHelpers.CheckPortAsync(host, port, timeout, token))
                        {
                            openPort = port;
                            break;
                        }
                    }
                }

                if ((!usePortProbe && pingAlive) || (usePortProbe && openPort.HasValue))
                {
                    results.Add(new HostScanResult(address, openPort));
                }
            });

            var ordered = results.OrderBy(x => ToSortableUInt32(x.Address)).ToArray();
            var sb = new StringBuilder();
            sb.AppendLine("LAN scan");
            sb.AppendLine($"Range: {cidr ?? $"{start}-{end}"}");
            sb.AppendLine($"Hosts scanned: {hosts.Count}");
            sb.AppendLine($"Mode: {(usePortProbe ? "port-probe" : "ping")}");
            sb.AppendLine($"Ping: {usePing}");
            if (usePortProbe)
            {
                sb.AppendLine($"Ports requested: {NetworkScanHelpers.FormatPorts(ports)}");
            }
            sb.AppendLine($"Timeout: {timeout} ms");
            sb.AppendLine($"Responsive hosts: {ordered.Length}");
            sb.AppendLine();

            foreach (var result in ordered)
            {
                sb.AppendLine(result.OpenPort.HasValue
                    ? $"{result.Address} {result.OpenPort.Value}"
                    : result.Address.ToString());
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error scanning LAN: {ex.Message}";
        }
    }

    public string? GetHelpText() =>
        """
        tool: network.scan.lan

        summary: Discover responsive hosts in a bounded IPv4 LAN range. Defaults to ping-only.

        arguments:
          cidr:
            required: false
            mutuallyExclusiveWith:
              - start
              - end
            formats:
              - ipv4_cidr
            examples:
              - 192.168.1.0/24
          start:
            required: false
            requires:
              - end
            formats:
              - ipv4_address
          end:
            required: false
            requires:
              - start
            formats:
              - ipv4_address
          ports:
            required: false
            default: none
            formats:
              - common
              - all
              - single_port
              - comma_list
              - range
            behavior:
              - If omitted, no ports are scanned.
              - If provided, ports are used only as a host-presence probe.
              - For each host, scanning stops after the first open port.
              - Use network.scan.ports for detailed port scanning.
          ping:
            required: false
            default: true
          timeout:
            required: false
            default: 500
            min: 100
            max: 10000
            unit: milliseconds
          concurrency:
            required: false
            default: 64
            min: 1
            max: 256
          maxHosts:
            required: false
            default: 4096
            min: 1
            max: 4096

        limits:
          maxHostsDefault: 4096
          maxHostsAbsolute: 4096

        risk:
          broadScan: ask the user for an explicit bounded subnet or range before scanning.
          all_ports: use only when explicitly requested by the user.

        output:
          stats: emitted first.
          ping_mode_rows: one responsive IP per line.
          port_probe_rows: one IP and one successful port per line.

        examples:
          - request:
              cidr: 192.168.1.0/24
              ping: true
          - request:
              start: 192.168.1.10
              end: 192.168.1.50
              ports: 22,80,443
        """;

    private static uint ToSortableUInt32(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
    }

    private sealed record HostScanResult(IPAddress Address, int? OpenPort);
}
