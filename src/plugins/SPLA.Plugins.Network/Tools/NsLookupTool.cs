using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class NsLookupTool : IMcpTool
{
    public string Name => "network_resolve_host";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Resolves a host name to its IP addresses (A/AAAA records) using DNS.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Domain name to resolve (e.g. 'google.com')." }
                },
                required = new[] { "host" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var host = ToolJson.GetStringTrimmed(doc.RootElement, "host");
            if (host is null) return ToolResult.Fail("Error: Missing 'host' parameter.", "missing host");

            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
            if (addresses.Length == 0)
            {
                return ToolResult.Text($"Error: No IP addresses resolved for '{host}'.");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"DNS Resolution for: {host}");
            for (var i = 0; i < addresses.Length; i++)
            {
                sb.AppendLine($"{i + 1}. {addresses[i]} (Type: {addresses[i].AddressFamily})");
            }

            return ToolResult.Text(sb.ToString());
        }
        catch (JsonException)
        {
            return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json");
        }
        catch (Exception ex)
        {
            return ToolResult.Fail($"Error resolving host: {ex.Message}", "resolve failed");
        }
    }
}
