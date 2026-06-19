using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
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
    public string Name => "network_probe_udp";

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
                    payload_is_hex = new { type = "boolean", description = "If true, 'payload' is parsed as a hex string (e.g. '0A 0D'). Default: false." },
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

            var hostVal = ToolJson.GetStringTrimmed(root, "host");
            if (hostVal is null) return "Error: Missing 'host', 'port', or 'payload' parameters.";
            host = hostVal;

            var portVal = ToolJson.GetInt32(root, "port");
            if (portVal is null || portVal < 1 || portVal > 65535) return "Error: Port must be an integer between 1 and 65535.";
            port = portVal.Value;

            var payload = ToolJson.GetString(root, "payload");
            if (string.IsNullOrEmpty(payload)) return "Error: UDP probe requires a non-empty payload to elicit a response.";

            var payloadIsHex = ToolJson.GetBoolean(root, "payload_is_hex", false);
            var timeout      = ToolJson.GetInt32(root, "timeout", 3000);

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
