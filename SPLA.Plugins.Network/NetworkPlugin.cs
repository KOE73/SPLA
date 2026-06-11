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
            new HostNetworkInfoTool(),
            new LanScanTool(),
            new PortScanTool(),
            new PingTool(),
            new NsLookupTool(),
            new HttpGetTool(),
            new HttpHeadTool(),
            new PortCheckTool(),
            new TraceRouteTool()
        };
    }
}
