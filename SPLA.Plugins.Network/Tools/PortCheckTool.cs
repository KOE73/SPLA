using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class PortCheckTool : IMcpTool
{
    public string Name => "network.port.check";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Checks if a specific TCP port is open on a host (host or IP). Useful for quick port diagnostics.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Target IP address or domain name (e.g. '127.0.0.1' or 'example.com')." },
                    port = new { type = "integer", description = "TCP port number to check (e.g. 80, 443, 22)." },
                    timeout = new { type = "integer", description = "Connection timeout in milliseconds (default: 3000)." }
                },
                required = new[] { "host", "port" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string host = "Unknown";
        int port = 0;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("host", out var hostElement) ||
                !doc.RootElement.TryGetProperty("port", out var portElement))
            {
                return "Error: Missing 'host' or 'port' parameters.";
            }

            host = hostElement.GetString() ?? "Unknown";
            if (string.IsNullOrWhiteSpace(host))
            {
                return "Error: Host is empty.";
            }

            if (!portElement.TryGetInt32(out var p) || p < 1 || p > 65535)
            {
                return "Error: Port must be an integer between 1 and 65535.";
            }
            port = p;

            var timeout = doc.RootElement.TryGetProperty("timeout", out var timeoutElement) && timeoutElement.TryGetInt32(out var t)
                ? t
                : 3000;

            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, port, cancellationToken).AsTask();
            var delayTask = Task.Delay(timeout, cancellationToken);

            var completedTask = await Task.WhenAny(connectTask, delayTask);

            if (completedTask == connectTask)
            {
                // Verify no exception occurred during connection
                await connectTask;
                if (tcpClient.Connected)
                {
                    return $"Host: {host}\n" +
                           $"Port: {port}\n" +
                           $"Status: Open";
                }
            }

            return $"Host: {host}\n" +
                   $"Port: {port}\n" +
                   $"Status: Closed or Timeout";
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Host: {host}\n" +
                   $"Port: {port}\n" +
                   $"Status: Closed (Error: {ex.Message})";
        }
    }
}
