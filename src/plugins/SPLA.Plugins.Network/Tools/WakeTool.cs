using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class WakeTool : IMcpTool
{
    public string Name => "network_wake_host";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Sends a Wake-on-LAN magic packet (6×0xFF + 16×MAC) to wake a sleeping machine. The target must have WoL enabled in BIOS/UEFI and be reachable on the broadcast domain.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    mac       = new { type = "string",  description = "Target MAC address — any common format: '00:11:22:33:44:55', '00-11-22-33-44-55', or '001122334455'." },
                    broadcast = new { type = "string",  description = "Broadcast address (default: '255.255.255.255'). Use the subnet broadcast for more reliable delivery, e.g. '192.168.1.255'." },
                    port      = new { type = "integer", description = "UDP port (default: 9; port 7 is also common)." },
                    repeat    = new { type = "integer", description = "Number of magic packets to send (default: 3 for reliability)." }
                },
                required = new[] { "mac" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;
            var macStr = ToolJson.GetStringTrimmed(root, "mac");
            if (macStr is null) return "Error: Missing 'mac' parameter.";

            var macBytes = ParseMac(macStr);
            if (macBytes == null)
                return $"Error: Cannot parse MAC address '{macStr}'. Expected: 00:11:22:33:44:55 / 00-11-22-33-44-55 / 001122334455.";

            var broadcastStr = ToolJson.GetStringTrimmed(root, "broadcast") ?? "255.255.255.255";

            if (!IPAddress.TryParse(broadcastStr, out var broadcastAddr))
                return $"Error: Invalid broadcast address '{broadcastStr}'.";

            var port   = ToolJson.GetInt32Clamped(root, "port",   9, 1, 65535);
            var repeat = ToolJson.GetInt32Clamped(root, "repeat", 3, 1, 10);

            var packet   = BuildMagicPacket(macBytes);
            var endpoint = new IPEndPoint(broadcastAddr, port);

            using var udp = new UdpClient();
            udp.EnableBroadcast = true;

            for (int i = 0; i < repeat; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await udp.SendAsync(packet, packet.Length, endpoint);
                if (i < repeat - 1) await Task.Delay(20, cancellationToken);
            }

            var formatted = string.Join("-", macBytes.Select(b => b.ToString("X2")));
            return $"Wake-on-LAN sent.\n" +
                   $"Target MAC:   {formatted}\n" +
                   $"Broadcast:    {broadcastAddr}:{port}\n" +
                   $"Packets sent: {repeat}\n" +
                   $"Packet size:  {packet.Length} bytes";
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error sending Wake-on-LAN: {ex.Message}";
        }
    }

    private static byte[]? ParseMac(string mac)
    {
        var clean = Regex.Replace(mac, @"[:\-\.\s]", "");
        if (clean.Length != 12 || !Regex.IsMatch(clean, @"^[0-9a-fA-F]{12}$"))
            return null;

        var bytes = new byte[6];
        for (int i = 0; i < 6; i++)
            bytes[i] = Convert.ToByte(clean.Substring(i * 2, 2), 16);
        return bytes;
    }

    private static byte[] BuildMagicPacket(byte[] mac)
    {
        // Magic packet = 6 × 0xFF then 16 repetitions of the target MAC
        var packet = new byte[6 + 16 * 6];
        for (int i = 0; i < 6; i++) packet[i] = 0xFF;
        for (int i = 0; i < 16; i++) Array.Copy(mac, 0, packet, 6 + i * 6, 6);
        return packet;
    }
}
