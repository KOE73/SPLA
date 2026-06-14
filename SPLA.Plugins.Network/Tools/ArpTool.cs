using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class ArpTool : IMcpTool
{
    public string Name => "network.arp";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Reads the ARP cache — the local table mapping IP addresses to MAC addresses that the host has recently resolved. Useful after a LAN scan to identify devices by MAC, infer vendors, or spot ARP poisoning.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    filter = new
                    {
                        type = "string",
                        description = "Optional substring to filter by IP or MAC address (case-insensitive). E.g. '192.168.1' or 'AA-BB'."
                    }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            string? filter = null;
            if (!string.IsNullOrWhiteSpace(argumentsJson))
            {
                using var doc = JsonDocument.Parse(argumentsJson);
                if (doc.RootElement.TryGetProperty("filter", out var filterEl))
                    filter = filterEl.GetString()?.Trim();
                if (string.IsNullOrEmpty(filter)) filter = null;
            }

            var entries = OperatingSystem.IsLinux()
                ? ReadProcNetArp(filter)
                : await RunArpCommandAsync(filter, cancellationToken);

            var sb = new StringBuilder();
            var suffix = filter != null ? $" (filter: '{filter}')" : "";
            sb.AppendLine($"ARP cache — {entries.Count} entr{(entries.Count == 1 ? "y" : "ies")}{suffix}:");
            sb.AppendLine();
            sb.AppendLine($"{"IP Address",-18} {"MAC Address",-20} {"Type",-10} Interface");
            sb.AppendLine(new string('-', 68));

            foreach (var e in entries)
                sb.AppendLine($"{e.IpAddress,-18} {e.MacAddress,-20} {e.Type,-10} {e.Interface}");

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error reading ARP cache: {ex.Message}";
        }
    }

    // Linux: parse /proc/net/arp directly — no process required.
    private static List<ArpEntry> ReadProcNetArp(string? filter)
    {
        var entries = new List<ArpEntry>();
        if (!File.Exists("/proc/net/arp")) return entries;

        var lines = File.ReadAllLines("/proc/net/arp");
        for (int i = 1; i < lines.Length; i++) // skip header
        {
            var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 6) continue;

            var ip  = parts[0];
            var mac = parts[3].Replace(':', '-').ToUpperInvariant();
            var iface = parts[5];
            if (mac == "00-00-00-00-00-00") continue; // incomplete / no reply

            if (Matches(ip, mac, filter))
                entries.Add(new ArpEntry(ip, mac, "dynamic", iface));
        }
        return entries;
    }

    // Windows / macOS: run `arp -a` and parse platform-specific output.
    private static async Task<List<ArpEntry>> RunArpCommandAsync(string? filter, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo("arp", "-a")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start 'arp' process.");

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return OperatingSystem.IsWindows()
            ? ParseWindowsArp(output, filter)
            : ParseBsdArp(output, filter);
    }

    // Windows arp -a:
    //   Interface: 192.168.1.5 --- 0x5
    //     Internet Address      Physical Address      Type
    //     192.168.1.1           00-50-56-c0-00-08     dynamic
    private static List<ArpEntry> ParseWindowsArp(string output, string? filter)
    {
        var entries = new List<ArpEntry>();
        var currentIface = "";

        foreach (var line in output.Split('\n'))
        {
            var trimmed = line.Trim();

            if (trimmed.StartsWith("Interface:", StringComparison.OrdinalIgnoreCase))
            {
                var parts2 = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                currentIface = parts2.Length > 1 ? parts2[1] : "";
                continue;
            }

            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3 || !IPAddress.TryParse(parts[0], out _)) continue;

            var ip   = parts[0];
            var mac  = parts[1].ToUpperInvariant();
            var type = parts[2];

            if (Matches(ip, mac, filter))
                entries.Add(new ArpEntry(ip, mac, type, currentIface));
        }
        return entries;
    }

    // BSD / macOS arp -a:
    //   ? (192.168.1.1) at 00:50:56:c0:00:08 [ether] on eth0
    private static readonly Regex BsdArpRegex = new(
        @"\((\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\)\s+at\s+([0-9a-f:]+).*\s+on\s+(\S+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static List<ArpEntry> ParseBsdArp(string output, string? filter)
    {
        var entries = new List<ArpEntry>();

        foreach (var line in output.Split('\n'))
        {
            var m = BsdArpRegex.Match(line);
            if (!m.Success) continue;

            var ip    = m.Groups[1].Value;
            var mac   = m.Groups[2].Value.Replace(':', '-').ToUpperInvariant();
            var iface = m.Groups[3].Value;

            if (Matches(ip, mac, filter))
                entries.Add(new ArpEntry(ip, mac, "dynamic", iface));
        }
        return entries;
    }

    private static bool Matches(string ip, string mac, string? filter) =>
        filter == null
        || ip.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || mac.Contains(filter, StringComparison.OrdinalIgnoreCase);

    private sealed record ArpEntry(string IpAddress, string MacAddress, string Type, string Interface);
}
