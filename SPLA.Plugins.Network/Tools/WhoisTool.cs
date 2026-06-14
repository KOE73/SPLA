using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class WhoisTool : IMcpTool
{
    public string Name => "network.whois";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Performs a WHOIS lookup for a domain name or IP address. Automatically follows the IANA referral to the authoritative WHOIS server for the TLD or regional registry (ARIN, RIPE, APNIC, etc.).",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    query   = new { type = "string",  description = "Domain name (e.g. 'example.com') or IP address." },
                    server  = new { type = "string",  description = "Override WHOIS server hostname (e.g. 'whois.verisign-grs.com'). If omitted, resolved automatically via IANA." },
                    timeout = new { type = "integer", description = "Connection timeout per WHOIS server in milliseconds (default: 10000)." }
                },
                required = new[] { "query" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (!doc.RootElement.TryGetProperty("query", out var queryEl))
                return "Error: Missing 'query' parameter.";

            var query = queryEl.GetString()?.Trim();
            if (string.IsNullOrEmpty(query))
                return "Error: Query is empty.";

            var overrideServer = doc.RootElement.TryGetProperty("server", out var serverEl) ? serverEl.GetString()?.Trim() : null;
            var timeoutMs = doc.RootElement.TryGetProperty("timeout", out var toEl) && toEl.TryGetInt32(out var t)
                ? Math.Clamp(t, 1000, 30_000) : 10_000;

            var sb = new StringBuilder();
            sb.AppendLine($"WHOIS: {query}");
            sb.AppendLine();

            string whoisServer;
            if (!string.IsNullOrEmpty(overrideServer))
            {
                whoisServer = overrideServer;
            }
            else
            {
                // Stage 1: ask IANA for the authoritative server
                var ianaResponse = await QueryWhoisAsync("whois.iana.org", query, timeoutMs, cancellationToken);
                var referral     = ExtractReferral(ianaResponse);

                if (string.IsNullOrEmpty(referral))
                {
                    sb.AppendLine("Server: whois.iana.org");
                    sb.AppendLine();
                    sb.Append(ianaResponse);
                    return sb.ToString();
                }

                whoisServer = referral;
            }

            // Stage 2: query the authoritative server
            var response = await QueryWhoisAsync(whoisServer, query, timeoutMs, cancellationToken);
            sb.AppendLine($"Server: {whoisServer}");
            sb.AppendLine();
            sb.Append(response);
            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error performing WHOIS lookup: {ex.Message}";
        }
    }

    private static async Task<string> QueryWhoisAsync(
        string server, string query, int timeoutMs, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeoutMs);

        using var tcp = new TcpClient();
        await tcp.ConnectAsync(server, 43, cts.Token);

        await using var stream = tcp.GetStream();
        await using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true)
        {
            NewLine    = "\r\n",
            AutoFlush  = true
        };
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        await writer.WriteLineAsync(query);

        var text = await reader.ReadToEndAsync(cts.Token);

        // Safety cap: some servers send large bulk data
        const int maxChars = 64_000;
        if (text.Length > maxChars)
            text = text[..maxChars] + "\n... [truncated]";

        return text;
    }

    // Look for "refer: <server>" or "whois: <server>" in IANA response
    private static readonly Regex ReferralRegex = new(
        @"^(?:refer|whois)\s*:\s*(\S+)",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static string? ExtractReferral(string ianaResponse)
    {
        var m = ReferralRegex.Match(ianaResponse);
        return m.Success ? m.Groups[1].Value.Trim() : null;
    }
}
