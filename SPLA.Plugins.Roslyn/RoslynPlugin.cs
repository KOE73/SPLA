using System.Collections.Generic;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Roslyn;

public class RoslynPlugin : ISplaPlugin
{
    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        return new IMcpTool[]
        {
            new CompileCheckTool(),
            new ScriptRunTool(),
        };
    }
}
