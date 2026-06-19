using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
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
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    // Sources from different jurisdictions so split-tunnel / geo-routing differences are visible.
    // All endpoints must return plain-text IP (trimmed). Add new ones here as needed.
    private static readonly (string Label, string Url)[] DefaultPublicIpSources =
    {
        ("ipify.org (US)",             "https://api.ipify.org"),
        ("checkip.amazonaws.com (US)", "https://checkip.amazonaws.com"),
        ("ifconfig.me (EU)",           "https://ifconfig.me/ip"),
        ("2ip.ru (RU)",                "https://internet-lab.ru/ip"),
        ("ip.sb (CN)",                 "https://api.ip.sb/ip"),
    };

    public string Name => "network_get_host_info";

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
                    include_public_ip = new { type = "boolean", description = "Query external endpoints to detect public egress IP. Default: true." },
                    custom_public_ip_endpoints = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Optional JSON array of extra plain-text IP endpoint URLs to query in addition to built-in sources, e.g. {\"custom_public_ip_endpoints\":[\"https://myproxy.internal/ip\"]}."
                    }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var includePublicIp = true;
        string[] customEndpoints = Array.Empty<string>();

        if (!string.IsNullOrWhiteSpace(argumentsJson))
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;
            includePublicIp = ToolJson.GetBoolean(root, "include_public_ip", true);
            customEndpoints = ToolJson.GetStringArray(root, "custom_public_ip_endpoints") ?? Array.Empty<string>();
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
            // Build full source list: built-in jurisdictions + any user-supplied custom endpoints.
            var sources = DefaultPublicIpSources
                .Concat(customEndpoints.Select(url => (Label: $"custom: {url}", Url: url)))
                .ToArray();

            // Query all sources in parallel so we don't wait sequentially.
            var tasks = sources.Select(s => QueryPublicIpAsync(s.Label, s.Url, cancellationToken)).ToArray();
            var results = await Task.WhenAll(tasks);

            sb.AppendLine("Public egress IPs (queried in parallel):");
            foreach (var (label, ip) in results)
            {
                sb.AppendLine($"  {label}: {ip}");
            }
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

    private static async Task<(string Label, string Ip)> QueryPublicIpAsync(string label, string url, CancellationToken cancellationToken)
    {
        try
        {
            var ip = (await HttpClient.GetStringAsync(url, cancellationToken)).Trim();
            return (label, ip);
        }
        catch (Exception ex)
        {
            return (label, $"unavailable ({ex.Message})");
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
