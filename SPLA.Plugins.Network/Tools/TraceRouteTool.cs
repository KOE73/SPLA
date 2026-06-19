using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class TraceRouteTool : IMcpTool
{
    public string Name => "network_trace_route";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Performs a traceroute using ICMP echo requests with increasing TTLs. NOTE: on Linux requires elevated privileges (CAP_NET_RAW / sudo); on Windows works as standard user.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Target IP address or domain name (e.g. '8.8.8.8' or 'google.com')." },
                    max_hops = new { type = "integer", description = "Maximum number of hops to trace (default: 20, max: 30)." },
                    timeout = new { type = "integer", description = "Ping timeout in milliseconds for each hop (default: 1000)." },
                    resolve_hops = new { type = "boolean", description = "Reverse-resolve each hop IP to a hostname. Adds latency per hop. Default: false." }
                },
                required = new[] { "host" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        // TODO (Linux): ICMP raw sockets require CAP_NET_RAW or root.
        //   Option A: setcap cap_net_raw+ep on the host binary.
        //   Option B: fall back to TCP-based traceroute (connect with increasing TTL via SO_IP_TTL socket option).
        //   Option C: spawn system `traceroute`/`tracepath` and parse output.
        //   Until implemented, this tool will throw SocketException on Linux without elevated privileges.
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;
            var host = ToolJson.GetStringTrimmed(root, "host");
            if (host is null) return "Error: Missing 'host' parameter.";

            var maxHops     = ToolJson.GetInt32Clamped(root, "max_hops", 20,   1,   30);
            var timeout     = ToolJson.GetInt32Clamped(root, "timeout",  1000, 100, 10000);
            var resolveHops = ToolJson.GetBoolean(root, "resolve_hops", false);

            using var ping = new Ping();
            var sb = new StringBuilder();
            sb.AppendLine($"Traceroute to {host} (max {maxHops} hops{(resolveHops ? ", resolving hostnames" : "")}):");

            for (var ttl = 1; ttl <= maxHops; ttl++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var options = new PingOptions(ttl, true);
                var reply = await ping.SendPingAsync(host, timeout, Array.Empty<byte>(), options);

                if (reply.Status == IPStatus.TtlExpired)
                {
                    var name = resolveHops ? await TryResolveAsync(reply.Address, cancellationToken) : null;
                    sb.AppendLine(name != null
                        ? $"{ttl,2}. {reply.Address} ({name})"
                        : $"{ttl,2}. {reply.Address}");
                }
                else if (reply.Status == IPStatus.Success)
                {
                    var name = resolveHops ? await TryResolveAsync(reply.Address, cancellationToken) : null;
                    sb.AppendLine(name != null
                        ? $"{ttl,2}. {reply.Address} ({name}) (Reached)"
                        : $"{ttl,2}. {reply.Address} (Reached)");
                    break;
                }
                else if (reply.Status == IPStatus.TimedOut)
                {
                    sb.AppendLine($"{ttl,2}. *");
                }
                else
                {
                    sb.AppendLine($"{ttl,2}. * (Error: {reply.Status})");
                }
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error performing traceroute: {ex.Message}";
        }
    }

    private static async Task<string?> TryResolveAsync(IPAddress? address, CancellationToken cancellationToken)
    {
        if (address == null) return null;
        try
        {
            var entry = await Dns.GetHostEntryAsync(address.ToString(), cancellationToken);
            return entry.HostName != address.ToString() ? entry.HostName : null;
        }
        catch { return null; }
    }
}
