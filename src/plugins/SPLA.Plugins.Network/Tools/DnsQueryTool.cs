using DnsClient;
using DnsClient.Protocol;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Network;

/// <summary>
/// Full DNS record query via DnsClient.NET (NuGet: DnsClient 1.8+).
/// Supports A, AAAA, CNAME, MX, NS, PTR, SOA, SRV, TXT record types.
/// </summary>
public class DnsQueryTool : IMcpTool
{
    public string Name => "network_query_dns";

    public ToolDefinition GetDefinition() => new ToolDefinition
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Queries specific DNS record types (A, AAAA, CNAME, MX, NS, PTR, SOA, SRV, TXT). Use this instead of network_resolve_host when you need record types other than A/AAAA, e.g. to check SPF (TXT), mail servers (MX), or name servers (NS).",
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
                        description = "DNS record type. One of: A, AAAA, CNAME, MX, NS, PTR, SOA, SRV, TXT. Default: A.",
                        @enum = new[] { "A", "AAAA", "CNAME", "MX", "NS", "PTR", "SOA", "SRV", "TXT" }
                    },
                    nameserver = new { type = "string", description = "Optional DNS server IP to query instead of the system default (e.g. '8.8.8.8')." }
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
                return $"Error: Unknown record type '{typeName}'. Supported: A, AAAA, CNAME, MX, NS, PTR, SOA, SRV, TXT.";

            LookupClient client;
            var nameserver = ToolJson.GetStringTrimmed(root, "nameserver");
            if (!string.IsNullOrWhiteSpace(nameserver))
            {
                var nsAddress = System.Net.IPAddress.Parse(nameserver);
                client = new LookupClient(nsAddress);
            }
            else
            {
                client = new LookupClient();
            }

            var result = await client.QueryAsync(host, queryType, cancellationToken: cancellationToken);

            if (result.HasError)
                return $"DNS error: {result.ErrorMessage}";

            if (!result.Answers.Any())
                return $"No {typeName} records found for '{host}'.";

            var sb = new StringBuilder();
            sb.AppendLine($"DNS query: {host} IN {typeName}");
            sb.AppendLine($"Server: {(result.NameServer?.Address?.ToString() ?? "system default")}");
            sb.AppendLine($"Records ({result.Answers.Count}):");

            foreach (var record in result.Answers)
            {
                var formatted = record switch
                {
                    ARecord a           => $"  A      {a.Address}  TTL={a.TimeToLive}",
                    AaaaRecord aaaa     => $"  AAAA   {aaaa.Address}  TTL={aaaa.TimeToLive}",
                    CNameRecord cname   => $"  CNAME  {cname.CanonicalName}  TTL={cname.TimeToLive}",
                    MxRecord mx         => $"  MX     priority={mx.Preference} exchange={mx.Exchange}  TTL={mx.TimeToLive}",
                    NsRecord ns         => $"  NS     {ns.NSDName}  TTL={ns.TimeToLive}",
                    PtrRecord ptr       => $"  PTR    {ptr.PtrDomainName}  TTL={ptr.TimeToLive}",
                    SoaRecord soa       => $"  SOA    mname={soa.MName} rname={soa.RName} serial={soa.Serial} refresh={soa.Refresh} retry={soa.Retry} expire={soa.Expire} minttl={soa.Minimum}  TTL={soa.TimeToLive}",
                    SrvRecord srv       => $"  SRV    priority={srv.Priority} weight={srv.Weight} port={srv.Port} target={srv.Target}  TTL={srv.TimeToLive}",
                    TxtRecord txt       => $"  TXT    \"{string.Join("\" \"", txt.Text)}\"  TTL={txt.TimeToLive}",
                    _                   => $"  {record.RecordType,-6} {record}"
                };
                sb.AppendLine(formatted);
            }

            return sb.ToString();
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON arguments.";
        }
        catch (Exception ex)
        {
            return $"Error querying DNS: {ex.Message}";
        }
    }
}
