using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserStopTool : IMcpTool
{
    public string Name => "browser_stop";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Closes the browser and frees its process. Call when you are done browsing.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            Parameters = new { type = "object", properties = new { } }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return "Browser is not running.";
        try { return await mgr.StopAsync(); }
        catch (Exception ex) { return BrowserToolBase.Fail("browser_stop", ex); }
    }
}
