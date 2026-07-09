using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class ArpTool : IMcpTool
{
    public string Name => "network_get_arp_cache";

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
                filter = ToolJson.GetStringTrimmed(doc.RootElement, "filter");
            }

            List<ArpEntry> entries;
            if (OperatingSystem.IsLinux())
                entries = ReadProcNetArp(filter);
            else if (OperatingSystem.IsWindows())
                entries = GetWindowsArpEntries(filter);
            else
                entries = await RunMacArpAsync(filter, cancellationToken);

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

    // Linux: /proc/net/arp — no process, no encoding issues.
    private static List<ArpEntry> ReadProcNetArp(string? filter)
    {
        var entries = new List<ArpEntry>();
        if (!File.Exists("/proc/net/arp")) return entries;

        var lines = File.ReadAllLines("/proc/net/arp");
        for (int i = 1; i < lines.Length; i++) // skip header
        {
            var parts = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 6) continue;

            var ip    = parts[0];
            var mac   = parts[3].Replace(':', '-').ToUpperInvariant();
            var iface = parts[5];

            if (mac == "00-00-00-00-00-00") continue; // incomplete entry

            if (Matches(ip, mac, filter))
                entries.Add(new ArpEntry(ip, mac, "dynamic", iface));
        }
        return entries;
    }

    // Windows: GetIpNetTable P/Invoke — structured data, no text parsing, no locale issues.
    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetIpNetTable(IntPtr pIpNetTable, ref uint pdwSize, [MarshalAs(UnmanagedType.Bool)] bool bOrder);

    private static List<ArpEntry> GetWindowsArpEntries(string? filter)
    {
        uint size = 0;
        GetIpNetTable(IntPtr.Zero, ref size, false);

        var buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            uint result = GetIpNetTable(buffer, ref size, false);
            if (result != 0)
                throw new InvalidOperationException($"GetIpNetTable failed with error {result}.");

            // Build interface index → name map from .NET NetworkInterface.
            // GetIPv4Properties() throws NetworkInformationException on loopback/VPN/tunnel adapters — skip those.
            var ifaceNames = new Dictionary<int, string>();
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                try { ifaceNames.TryAdd(ni.GetIPProperties().GetIPv4Properties().Index, ni.Name); }
                catch { /* adapter does not support IPv4 — index unavailable, skip */ }
            }

            int numEntries = Marshal.ReadInt32(buffer);
            var entries = new List<ArpEntry>();

            // MIB_IPNETROW layout (24 bytes):
            //   0:  DWORD dwIndex         — interface index
            //   4:  DWORD dwPhysAddrLen   — MAC length (0 if incomplete)
            //   8:  BYTE[8] bPhysAddr     — MAC bytes (MAXLEN_PHYSADDR = 8)
            //  16:  DWORD dwAddr          — IPv4 address as little-endian DWORD
            //  20:  DWORD dwType          — 1=other, 2=invalid, 3=dynamic, 4=static
            int offset = 4; // skip NumEntries DWORD
            for (int i = 0; i < numEntries; i++)
            {
                int ifIndex  = Marshal.ReadInt32(buffer, offset);
                int physLen  = Marshal.ReadInt32(buffer, offset + 4);
                uint ipDword = (uint)Marshal.ReadInt32(buffer, offset + 16);
                int dwType   = Marshal.ReadInt32(buffer, offset + 20);
                offset += 24;

                if (physLen == 0 || dwType == 2) continue; // incomplete or invalid

                var macBytes = new byte[physLen];
                for (int b = 0; b < physLen; b++)
                    macBytes[b] = Marshal.ReadByte(buffer, offset - 24 + 8 + b);

                var ip    = new IPAddress(BitConverter.GetBytes(ipDword)).ToString();
                var mac   = string.Join("-", macBytes.Take(6).Select(b => b.ToString("X2")));
                var type  = dwType == 4 ? "static" : "dynamic";
                var iface = ifaceNames.TryGetValue(ifIndex, out var name) ? name : ifIndex.ToString();

                if (Matches(ip, mac, filter))
                    entries.Add(new ArpEntry(ip, mac, type, iface));
            }

            return entries;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // macOS: arp -an — ASCII output, -n suppresses hostname resolution.
    private static readonly Regex BsdArpRegex = new(
        @"\((\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\)\s+at\s+([0-9a-f:]+).*\s+on\s+(\S+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static async Task<List<ArpEntry>> RunMacArpAsync(string? filter, CancellationToken cancellationToken)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("arp", "-an")
        {
            RedirectStandardOutput = true,
            StandardOutputEncoding = Encoding.ASCII,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start 'arp' process.");

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

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
