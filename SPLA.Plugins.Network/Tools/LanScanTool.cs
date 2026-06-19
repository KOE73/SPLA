using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
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
    public string Name => "network_discover_hosts";

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
                    max_hosts = new { type = "integer", description = "Safety limit for host count. Default: 4096, max: 4096." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root        = doc.RootElement;
            var cidr        = ToolJson.GetString(root, "cidr");
            var start       = ToolJson.GetString(root, "start");
            var end         = ToolJson.GetString(root, "end");
            var portsValue  = ToolJson.GetString(root, "ports");
            var usePortProbe = !string.IsNullOrWhiteSpace(portsValue);
            var usePing     = ToolJson.GetBoolean(root, "ping", true);
            var timeout     = ToolJson.GetInt32Clamped(root, "timeout",     500,  100, 10000);
            var concurrency = ToolJson.GetInt32Clamped(root, "concurrency", 64,   1,   256);
            var maxHosts    = ToolJson.GetInt32Clamped(root, "max_hosts",   4096, 1,   4096);

            if (!usePing && !usePortProbe)
            {
                return "Error: Nothing to scan. Enable ping or provide ports.";
            }

            var hosts = NetworkScanHelpers.ParseHostRange(cidr, start, end, maxHosts);
            var ports = usePortProbe ? NetworkScanHelpers.ParsePorts(portsValue) : Array.Empty<int>();
            var results = new ConcurrentBag<HostScanResult>();
            var total = hosts.Count;
            long done = 0;
            var step = Math.Max(1, total / 200);

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

                var found = (!usePortProbe && pingAlive) || (usePortProbe && openPort.HasValue);
                if (found)
                {
                    results.Add(new HostScanResult(address, openPort));
                }

                // Report on each responsive host (interesting), at each step boundary, and at the end.
                var n = Interlocked.Increment(ref done);
                if (found || n % step == 0 || n == total)
                {
                    var details = new[] { new ToolProgressDetail("alive", results.Count.ToString()) };
                    ToolProgressScope.Report(n, total, "scanning hosts", details);
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
        tool: network_discover_hosts

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
              - Use network_scan_tcp_ports for detailed port scanning.
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
          max_hosts:
            required: false
            default: 4096
            min: 1
            max: 4096

        limits:
          max_hosts_default: 4096
          max_hosts_absolute: 4096

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
