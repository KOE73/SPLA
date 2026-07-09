using System.Collections.Generic;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Network;

public class NetworkPlugin : ISplaPlugin
{
    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        return new IMcpTool[]
        {
            // Diagnostics
            new HostNetworkInfoTool(),
            new PingTool(),
            new PingStatsTool(),
            new TraceRouteTool(),
            new RouteTool(),
            // DNS
            new NsLookupTool(),
            new DnsQueryTool(),
            new ReverseDnsTool(),
            new DnsPropagationTool(),
            // HTTP
            new HttpGetTool(),
            new HttpHeadTool(),
            new HttpPostTool(),
            new HttpRedirectsTool(),
            // TLS
            new SslCheckTool(),
            // SMTP
            new SmtpProbeTool(),
            // WHOIS
            new WhoisTool(),
            // Scanning
            new LanScanTool(),
            new PortScanTool(),
            new PortCheckTool(),
            new ArpTool(),
            // Probes
            new TcpProbeTool(),
            new UdpProbeTool(),
            // Wake-on-LAN
            new WakeTool(),
        };
    }
}
