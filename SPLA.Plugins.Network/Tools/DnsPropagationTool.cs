using DnsClient;
using DnsClient.Protocol;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

public class DnsPropagationTool : IMcpTool
{
    private static readonly (string Label, string Ip)[] PublicResolvers =
    {
        ("Google",       "8.8.8.8"),
        ("Google alt",   "8.8.4.4"),
        ("Cloudflare",   "1.1.1.1"),
        ("Cloudflare alt","1.0.0.1"),
        ("Quad9",        "9.9.9.9"),
        ("OpenDNS",      "208.67.222.222"),
        ("OpenDNS alt",  "208.67.220.220"),
        ("AdGuard",      "94.140.14.14"),
        ("Level3",       "4.2.2.1"),
        ("Comodo",       "8.26.56.26"),
    };

    public string Name => "network_check_dns_propagation";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Queries a domain's DNS record across 10 public resolvers simultaneously to check propagation consistency. Highlights differences — useful right after a DNS change to see which resolvers have picked it up.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    host = new { type = "string", description = "Domain name to query (e.g. 'example.com')." },
                    type = new
                    {
                        type = "string",
                        description = "DNS record type (default: A). One of: A, AAAA, CNAME, MX, NS, TXT.",
                        @enum = new[] { "A", "AAAA", "CNAME", "MX", "NS", "TXT" }
                    }
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
            var root = doc.RootElement;
            var host = ToolJson.GetStringTrimmed(root, "host");
            if (host is null) return "Error: Missing 'host' parameter.";

            var typeName = ToolJson.GetString(root, "type") ?? "A";
            if (!Enum.TryParse<QueryType>(typeName, ignoreCase: true, out var queryType))
                return $"Error: Unknown record type '{typeName}'.";

            var tasks = PublicResolvers
                .Select(r => QueryResolverAsync(r.Label, r.Ip, host, queryType, cancellationToken))
                .ToArray();
            var results = await Task.WhenAll(tasks);

            var sb = new StringBuilder();
            sb.AppendLine($"DNS propagation: {host} IN {typeName}  ({results.Length} resolvers)");
            sb.AppendLine();

            var uniqueAnswers = results
                .Select(r => r.Answer)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            bool allAgree = uniqueAnswers.Length == 1;

            foreach (var (label, answer, rtt) in results)
                sb.AppendLine($"  [{(allAgree ? "=" : "!")}] {label,-14} {rtt,4} ms  {answer}");

            sb.AppendLine();
            if (allAgree)
                sb.AppendLine("All resolvers agree — propagation is consistent.");
            else
            {
                sb.AppendLine($"Inconsistency detected — {uniqueAnswers.Length} distinct answers:");
                foreach (var ans in uniqueAnswers)
                    sb.AppendLine($"  {ans}");
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

    private static async Task<(string Label, string Answer, long Rtt)> QueryResolverAsync(
        string label, string resolverIp, string host, QueryType queryType, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var client = new LookupClient(IPAddress.Parse(resolverIp));
            var result = await client.QueryAsync(host, queryType, cancellationToken: cancellationToken);
            sw.Stop();

            if (result.HasError)
                return (label, $"ERROR: {result.ErrorMessage}", sw.ElapsedMilliseconds);

            if (!result.Answers.Any())
                return (label, "NXDOMAIN", sw.ElapsedMilliseconds);

            var answers = result.Answers.Select(r => r switch
            {
                ARecord a      => a.Address.ToString(),
                AaaaRecord aa  => aa.Address.ToString(),
                CNameRecord cn => cn.CanonicalName.ToString(),
                MxRecord mx    => $"{mx.Preference} {mx.Exchange}",
                NsRecord ns    => ns.NSDName.ToString(),
                TxtRecord txt  => string.Join("|", txt.Text),
                _              => r.ToString() ?? ""
            });

            return (label, string.Join(", ", answers), sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            return (label, "TIMEOUT", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return (label, $"ERROR: {ex.Message}", sw.ElapsedMilliseconds);
        }
    }
}
