using System.Collections.Generic;
using SPLA.Domain.Settings;

namespace SPLA.MCP.Core.Interfaces;

public interface ISplaPlugin
{
    IEnumerable<IMcpTool> Initialize(ResolvedSettings settings);
}
