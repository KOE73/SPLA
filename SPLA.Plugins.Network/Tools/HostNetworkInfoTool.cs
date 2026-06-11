using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class HostNetworkInfoTool : IMcpTool
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

    public string Name => "network.host.info";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Collects local network identity: hostname, DNS suffix, local IPv4/IPv6 addresses, interfaces, gateways, DNS servers, and optional public IP. Use this first when AGENTS/project context does not already describe the host network.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    includePublicIp = new { type = "boolean", description = "Query an external endpoint to detect public egress IP. Default: true." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var includePublicIp = true;
        if (!string.IsNullOrWhiteSpace(argumentsJson))
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (doc.RootElement.TryGetProperty("includePublicIp", out var includePublicIpElement) &&
                includePublicIpElement.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                includePublicIp = includePublicIpElement.GetBoolean();
            }
        }

        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var sb = new StringBuilder();
        sb.AppendLine("Host network information");
        sb.AppendLine($"HostName: {Dns.GetHostName()}");
        sb.AppendLine($"DomainName: {properties.DomainName}");
        sb.AppendLine($"NodeType: {properties.NodeType}");

        var hostAddresses = await Dns.GetHostAddressesAsync(Dns.GetHostName(), cancellationToken);
        sb.AppendLine("Host addresses:");
        foreach (var address in hostAddresses)
        {
            sb.AppendLine($"  {address} ({address.AddressFamily})");
        }

        if (includePublicIp)
        {
            sb.AppendLine($"Public egress IP: {await GetPublicIpAsync(cancellationToken)}");
        }

        sb.AppendLine("Interfaces:");
        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces()
                     .Where(x => x.OperationalStatus == OperationalStatus.Up)
                     .OrderByDescending(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                     .ThenByDescending(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                     .ThenBy(x => x.Name))
        {
            var ip = adapter.GetIPProperties();
            sb.AppendLine($"- {adapter.Name}");
            sb.AppendLine($"  Description: {adapter.Description}");
            sb.AppendLine($"  Type: {adapter.NetworkInterfaceType}");
            sb.AppendLine($"  Speed: {adapter.Speed} bps");
            sb.AppendLine($"  MAC: {FormatMac(adapter.GetPhysicalAddress())}");
            sb.AppendLine("  Unicast:");
            foreach (var address in ip.UnicastAddresses)
            {
                sb.AppendLine($"    {address.Address}/{MaskToPrefix(address)} ({address.Address.AddressFamily})");
            }
            sb.AppendLine($"  Gateways: {string.Join(", ", ip.GatewayAddresses.Select(x => x.Address).Where(x => !x.Equals(IPAddress.Any)))}");
            sb.AppendLine($"  DNS: {string.Join(", ", ip.DnsAddresses)}");
        }

        return sb.ToString();
    }

    private static async Task<string> GetPublicIpAsync(CancellationToken cancellationToken)
    {
        try
        {
            return (await HttpClient.GetStringAsync("https://api.ipify.org", cancellationToken)).Trim();
        }
        catch (Exception ex)
        {
            return $"unavailable ({ex.Message})";
        }
    }

    private static string FormatMac(PhysicalAddress address)
    {
        var bytes = address.GetAddressBytes();
        return bytes.Length == 0 ? "" : string.Join(":", bytes.Select(x => x.ToString("X2")));
    }

    private static string MaskToPrefix(UnicastIPAddressInformation address)
    {
        if (address.Address.AddressFamily == AddressFamily.InterNetwork && address.IPv4Mask != null)
        {
            return address.IPv4Mask.GetAddressBytes().Sum(x => Convert.ToString(x, 2).Count(c => c == '1')).ToString();
        }

        return address.PrefixLength.ToString();
    }
}
