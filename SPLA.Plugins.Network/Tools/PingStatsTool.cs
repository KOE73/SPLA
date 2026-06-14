using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class PingStatsTool : IMcpTool
{
    public string Name => "network.diag.ping_stats";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Sends multiple ICMP pings and returns statistics: min/avg/max round-trip time, jitter, and packet loss percentage. More useful than a single ping for diagnosing latency or link instability.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Target IP or hostname." },
                    count = new { type = "integer", description = "Number of pings (default: 10, max: 100)." },
                    timeout = new { type = "integer", description = "Per-ping timeout in milliseconds (default: 2000)." },
                    interval = new { type = "integer", description = "Delay between pings in milliseconds (default: 200)." }
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
            if (!doc.RootElement.TryGetProperty("host", out var hostEl))
                return "Error: Missing 'host' parameter.";

            var host = hostEl.GetString();
            if (string.IsNullOrWhiteSpace(host))
                return "Error: Host is empty.";

            var count    = doc.RootElement.TryGetProperty("count", out var cEl) && cEl.TryGetInt32(out var c) ? Math.Clamp(c, 1, 100) : 10;
            var timeout  = doc.RootElement.TryGetProperty("timeout", out var tEl) && tEl.TryGetInt32(out var t) ? Math.Clamp(t, 100, 30_000) : 2000;
            var interval = doc.RootElement.TryGetProperty("interval", out var iEl) && iEl.TryGetInt32(out var iv) ? Math.Clamp(iv, 0, 10_000) : 200;

            var rtts = new List<long>(count);
            int lost = 0;

            using var ping = new Ping();
            for (int i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var reply = await ping.SendPingAsync(host, timeout);
                    if (reply.Status == IPStatus.Success)
                        rtts.Add(reply.RoundtripTime);
                    else
                        lost++;
                }
                catch { lost++; }

                if (i < count - 1 && interval > 0)
                    await Task.Delay(interval, cancellationToken);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Ping statistics for {host}:");
            sb.AppendLine($"Packets:         sent={count}  received={rtts.Count}  lost={lost} ({100.0 * lost / count:F1}% loss)");

            if (rtts.Count > 0)
            {
                var min    = rtts.Min();
                var max    = rtts.Max();
                var avg    = rtts.Average();
                var jitter = rtts.Count > 1
                    ? rtts.Zip(rtts.Skip(1), (a, b) => (double)Math.Abs(b - a)).Average()
                    : 0.0;

                sb.AppendLine($"RTT min/avg/max: {min}/{avg:F1}/{max} ms");
                sb.AppendLine($"Jitter (avg δ):  {jitter:F1} ms");
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
