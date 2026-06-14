using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class NsLookupTool : IMcpTool
{
    public string Name => "network.dns.nslookup";

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

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("host", out var hostElement))
            {
                return "Error: Missing 'host' parameter.";
            }

            var host = hostElement.GetString();
            if (string.IsNullOrWhiteSpace(host))
            {
                return "Error: Host is empty.";
            }

            var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
            if (addresses.Length == 0)
            {
                return $"Error: No IP addresses resolved for '{host}'.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"DNS Resolution for: {host}");
            for (var i = 0; i < addresses.Length; i++)
            {
                sb.AppendLine($"{i + 1}. {addresses[i]} (Type: {addresses[i].AddressFamily})");
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error resolving host: {ex.Message}";
        }
    }
}
