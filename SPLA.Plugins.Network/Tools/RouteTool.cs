using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class RouteTool : IMcpTool
{
    public string Name => "network_get_routes";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Displays the host's IP routing table. Useful for diagnosing traffic routing, VPN split-tunneling, unexpected default gateways, or missing routes.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    family = new
                    {
                        type        = "string",
                        description = "Address family: 'ipv4' (default), 'ipv6', or 'both'.",
                        @enum       = new[] { "ipv4", "ipv6", "both" }
                    }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var family = "ipv4";
            if (!string.IsNullOrWhiteSpace(argumentsJson))
            {
                using var doc = JsonDocument.Parse(argumentsJson);
                family = ToolJson.GetStringTrimmed(doc.RootElement, "family")?.ToLowerInvariant() ?? "ipv4";
            }

            string output;

            if (OperatingSystem.IsLinux())
            {
                if (family == "both")
                {
                    var v4 = await RunAsync("ip", "route show", cancellationToken);
                    var v6 = await RunAsync("ip", "-6 route show", cancellationToken);
                    output = $"=== IPv4 ===\n{v4}\n=== IPv6 ===\n{v6}";
                }
                else
                {
                    output = await RunAsync("ip", family == "ipv6" ? "-6 route show" : "route show", cancellationToken);
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                var args = family switch
                {
                    "ipv6" => "-rn -f inet6",
                    "both" => "-rn",
                    _      => "-rn -f inet"
                };
                output = await RunAsync("netstat", args, cancellationToken);
            }
            else // Windows
            {
                var args = family switch
                {
                    "ipv6" => "print -6",
                    "both" => "print",
                    _      => "print -4"
                };
                output = await RunAsync("route", args, cancellationToken);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Routing table (family: {family}):");
            sb.AppendLine();
            sb.Append(output);
            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error reading routing table: {ex.Message}";
        }
    }

    private static async Task<string> RunAsync(string file, string args, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo(file, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start '{file}'.");

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return output;
    }
}
