using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class UdpProbeTool : IMcpTool
{
    public string Name => "network.udp.probe";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Sends a raw UDP datagram to a host and port, and waits for a response. Required for probing UDP services like DNS, NTP, SNMP.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Target IP or hostname." },
                    port = new { type = "integer", description = "UDP port (1-65535)." },
                    payload = new { type = "string", description = "Data to send (UDP usually requires a payload to elicit a response)." },
                    payloadIsHex = new { type = "boolean", description = "If true, 'payload' is parsed as a hex string (e.g. '0A 0D'). Default: false." },
                    timeout = new { type = "integer", description = "Timeout in milliseconds for receiving (default: 3000)." }
                },
                required = new[] { "host", "port", "payload" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string host = string.Empty;
        int port = 0;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("host", out var hostElement) || 
                !root.TryGetProperty("port", out var portElement) ||
                !root.TryGetProperty("payload", out var payloadElement))
            {
                return "Error: Missing 'host', 'port', or 'payload' parameters.";
            }

            host = hostElement.GetString() ?? string.Empty;
            if (!portElement.TryGetInt32(out port)) return "Error: Invalid port.";

            var payload = payloadElement.GetString();
            if (string.IsNullOrEmpty(payload)) return "Error: UDP probe requires a non-empty payload to elicit a response.";

            var payloadIsHex = root.TryGetProperty("payloadIsHex", out var ph) && ph.GetBoolean();
            var timeout = root.TryGetProperty("timeout", out var to) && to.TryGetInt32(out var t) ? t : 3000;

            byte[] sendData;
            if (payloadIsHex)
            {
                payload = payload.Replace("-", "").Replace(" ", "");
                sendData = Convert.FromHexString(payload);
            }
            else
            {
                sendData = Encoding.UTF8.GetBytes(payload);
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            using var client = new UdpClient();
            
            var sw = Stopwatch.StartNew();
            await client.SendAsync(sendData, sendData.Length, host, port);
            
            try
            {
                // Use ReceiveAsync with cancellation token
                var receiveTask = client.ReceiveAsync(cts.Token).AsTask();
                var result = await receiveTask;
                var receiveTime = sw.ElapsedMilliseconds;

                var bytesRead = result.Buffer.Length;

                var sb = new StringBuilder();
                sb.AppendLine($"Sent: {sendData.Length} bytes");
                sb.AppendLine($"Received: {bytesRead} bytes in {receiveTime}ms from {result.RemoteEndPoint}");
                
                if (bytesRead > 0)
                {
                    sb.AppendLine("\n--- Text (UTF-8) ---");
                    var text = Encoding.UTF8.GetString(result.Buffer);
                    sb.AppendLine(text.Replace("\0", ".").Replace("\r", "\\r").Replace("\n", "\\n\n"));
                    
                    sb.AppendLine("\n--- Hex Dump ---");
                    sb.AppendLine(BitConverter.ToString(result.Buffer, 0, Math.Min(bytesRead, 512)));
                    if (bytesRead > 512) sb.AppendLine("... (hex dump truncated)");
                }

                return sb.ToString();
            }
            catch (OperationCanceledException)
            {
                return $"Sent {sendData.Length} bytes. Timeout ({timeout}ms) waiting for UDP response from {host}:{port}.";
            }
        }
        catch (Exception ex)
        {
            return $"Error probing UDP {host}:{port} - {ex.Message}";
        }
    }
}
