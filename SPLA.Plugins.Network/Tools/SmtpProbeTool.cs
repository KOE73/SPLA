using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class SmtpProbeTool : IMcpTool
{
    public string Name => "network.smtp.probe";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Connects to an SMTP server (port 25 or 587), reads the banner, sends EHLO, and reports advertised capabilities: STARTTLS, AUTH methods, message size limit, PIPELINING, 8BITMIME, and other extensions. Port 465 (implicit TLS/SMTPS) is not supported.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host     = new { type = "string",  description = "SMTP server hostname or IP." },
                    port     = new { type = "integer", description = "SMTP port (default: 25; 587 for submission)." },
                    timeout  = new { type = "integer", description = "Connection timeout in milliseconds (default: 5000)." },
                    ehloName = new { type = "string",  description = "Domain name sent in the EHLO command (default: 'localhost')." }
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

            var port      = doc.RootElement.TryGetProperty("port", out var portEl) && portEl.TryGetInt32(out var p) ? Math.Clamp(p, 1, 65535) : 25;
            var timeoutMs = doc.RootElement.TryGetProperty("timeout", out var toEl) && toEl.TryGetInt32(out var t) ? Math.Clamp(t, 500, 30_000) : 5000;
            var ehloName  = doc.RootElement.TryGetProperty("ehloName", out var eEl) ? eEl.GetString()?.Trim() ?? "localhost" : "localhost";
            if (string.IsNullOrEmpty(ehloName)) ehloName = "localhost";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeoutMs);

            using var tcp = new TcpClient();
            await tcp.ConnectAsync(host, port, cts.Token);

            await using var stream = tcp.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
            await using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true)
            {
                AutoFlush = true,
                NewLine   = "\r\n"
            };

            var sb = new StringBuilder();
            sb.AppendLine($"SMTP probe: {host}:{port}");
            sb.AppendLine();

            // Banner (may be multi-line)
            var banner = await ReadSmtpResponseAsync(reader, cts.Token);
            sb.AppendLine($"Banner:   {banner.Code} {banner.Lines.FirstOrDefault()}");
            if (!banner.Code.StartsWith("220"))
            {
                sb.AppendLine($"Unexpected banner code, aborting.");
                return sb.ToString();
            }

            // EHLO
            await writer.WriteLineAsync($"EHLO {ehloName}");
            var ehlo = await ReadSmtpResponseAsync(reader, cts.Token);
            sb.AppendLine($"EHLO:     {ehlo.Code}");

            // Lines[0] is the greeting ("<domain> Hello"), extensions start at [1]
            var extensions = ehlo.Lines.Skip(1)
                .Where(l => !string.Equals(l, "OK", StringComparison.OrdinalIgnoreCase) && l.Length > 0)
                .ToList();

            if (extensions.Count > 0)
            {
                sb.AppendLine("Extensions:");
                foreach (var ext in extensions)
                    sb.AppendLine($"  {ext}");
            }

            sb.AppendLine();
            sb.AppendLine("Capabilities:");

            bool hasStartTls = extensions.Any(l => l.StartsWith("STARTTLS", StringComparison.OrdinalIgnoreCase));
            sb.AppendLine($"  STARTTLS:      {(hasStartTls ? "yes" : "no")}");

            var authLine = extensions.FirstOrDefault(l => l.StartsWith("AUTH ", StringComparison.OrdinalIgnoreCase));
            sb.AppendLine($"  AUTH methods:  {(authLine != null ? authLine[5..] : "not advertised")}");

            var sizeLine = extensions.FirstOrDefault(l => l.StartsWith("SIZE", StringComparison.OrdinalIgnoreCase));
            if (sizeLine != null)
            {
                var parts = sizeLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1 && long.TryParse(parts[1], out var bytes) && bytes > 0)
                    sb.AppendLine($"  Max msg size:  {bytes / (1024.0 * 1024):F1} MB");
                else
                    sb.AppendLine($"  Max msg size:  reported, no explicit limit");
            }

            bool hasPipelining = extensions.Any(l => l.StartsWith("PIPELINING", StringComparison.OrdinalIgnoreCase));
            sb.AppendLine($"  PIPELINING:    {(hasPipelining ? "yes" : "no")}");

            bool has8bit = extensions.Any(l => l.StartsWith("8BITMIME", StringComparison.OrdinalIgnoreCase));
            sb.AppendLine($"  8BITMIME:      {(has8bit ? "yes" : "no")}");

            bool hasDsn = extensions.Any(l => l.StartsWith("DSN", StringComparison.OrdinalIgnoreCase));
            sb.AppendLine($"  DSN:           {(hasDsn ? "yes" : "no")}");

            bool hasSmtpUtf8 = extensions.Any(l => l.StartsWith("SMTPUTF8", StringComparison.OrdinalIgnoreCase));
            sb.AppendLine($"  SMTPUTF8:      {(hasSmtpUtf8 ? "yes" : "no")}");

            // Graceful quit
            try { await writer.WriteLineAsync("QUIT"); } catch { }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error probing SMTP: {ex.Message}";
        }
    }

    // SMTP responses are either single-line "CODE text" or multi-line "CODE-text\r\nCODE text".
    private static async Task<SmtpResponse> ReadSmtpResponseAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var lines = new List<string>();
        string? code = null;

        while (true)
        {
            var raw = await reader.ReadLineAsync(cancellationToken) ?? "";
            if (raw.Length >= 4)
            {
                code ??= raw[..3];
                lines.Add(raw.Length > 4 ? raw[4..] : "");
                if (raw[3] == ' ') break; // space = final line of response
            }
            else
            {
                lines.Add(raw);
                break;
            }
        }

        return new SmtpResponse(code ?? "???", lines);
    }

    private sealed record SmtpResponse(string Code, List<string> Lines);
}
