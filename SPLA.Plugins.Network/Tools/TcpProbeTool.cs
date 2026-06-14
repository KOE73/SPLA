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

public class TcpProbeTool : IMcpTool
{
    public string Name => "network.tcp.probe";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Establishes a raw TCP connection, optionally sends a payload, and reads the response. Ideal for banner grabbing (SSH, FTP) or raw protocol testing.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Target IP or hostname." },
                    port = new { type = "integer", description = "TCP port (1-65535)." },
                    payload = new { type = "string", description = "Optional data to send after connecting." },
                    payloadIsHex = new { type = "boolean", description = "If true, 'payload' is parsed as a hex string (e.g. '0A 0D'). Default: false." },
                    timeout = new { type = "integer", description = "Timeout in milliseconds (default: 3000)." }
                },
                required = new[] { "host", "port" }
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

            if (!root.TryGetProperty("host", out var hostElement) || !root.TryGetProperty("port", out var portElement))
                return "Error: Missing 'host' or 'port' parameters.";

            host = hostElement.GetString() ?? string.Empty;
            if (!portElement.TryGetInt32(out port)) return "Error: Invalid port.";

            var payload = root.TryGetProperty("payload", out var pe) ? pe.GetString() : null;
            var payloadIsHex = root.TryGetProperty("payloadIsHex", out var ph) && ph.GetBoolean();
            var timeout = root.TryGetProperty("timeout", out var to) && to.TryGetInt32(out var t) ? t : 3000;

            byte[]? sendData = null;
            if (!string.IsNullOrEmpty(payload))
            {
                if (payloadIsHex)
                {
                    payload = payload.Replace("-", "").Replace(" ", "");
                    sendData = Convert.FromHexString(payload);
                }
                else
                {
                    sendData = Encoding.UTF8.GetBytes(payload);
                }
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            using var client = new TcpClient();
            
            var sw = Stopwatch.StartNew();
            await client.ConnectAsync(host, port, cts.Token);
            var connectTime = sw.ElapsedMilliseconds;

            await using var stream = client.GetStream();

            if (sendData != null)
            {
                await stream.WriteAsync(sendData, cts.Token);
                await stream.FlushAsync(cts.Token);
            }

            byte[] buffer = new byte[8192];
            int bytesRead = 0;
            
            try
            {
                // If we didn't send anything, servers like SSH/FTP might take a moment to send their banner
                if (sendData == null) 
                {
                    await Task.Delay(200, cts.Token);
                }
                
                if (stream.DataAvailable)
                {
                    bytesRead = await stream.ReadAsync(buffer, cts.Token);
                }
                else
                {
                    // Block read with remaining timeout
                    var readTask = stream.ReadAsync(buffer, cts.Token).AsTask();
                    var timeoutTask = Task.Delay(Math.Max(100, timeout - (int)connectTime), cts.Token);
                    var completedTask = await Task.WhenAny(readTask, timeoutTask);
                    if (completedTask == readTask)
                    {
                        bytesRead = await readTask;
                    }
                }
            }
            catch (OperationCanceledException) { /* Read timeout, that's fine, we return what we have (0 bytes) */ }
            catch (Exception ex) { return $"Connected to {host}:{port} in {connectTime}ms. Error reading: {ex.Message}"; }

            var sb = new StringBuilder();
            sb.AppendLine($"Connection: Established in {connectTime}ms");
            if (sendData != null)
            {
                sb.AppendLine($"Sent: {sendData.Length} bytes");
            }
            sb.AppendLine($"Received: {bytesRead} bytes");
            
            if (bytesRead > 0)
            {
                sb.AppendLine("\n--- Text (UTF-8) ---");
                var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                sb.AppendLine(text.Replace("\0", ".").Replace("\r", "\\r").Replace("\n", "\\n\n"));
                
                sb.AppendLine("\n--- Hex Dump ---");
                sb.AppendLine(BitConverter.ToString(buffer, 0, Math.Min(bytesRead, 512)));
                if (bytesRead > 512) sb.AppendLine("... (hex dump truncated)");
            }

            return sb.ToString();
        }
        catch (OperationCanceledException)
        {
            return $"Timeout connecting to or reading from {host}:{port}.";
        }
        catch (Exception ex)
        {
            return $"Error probing TCP {host}:{port} - {ex.Message}";
        }
    }
}
