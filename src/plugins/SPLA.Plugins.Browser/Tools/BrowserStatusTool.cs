using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserStatusTool : IMcpTool
{
    public string Name => "browser_status";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Reports whether the browser is running, its profile/channel, and the list of open tabs.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new { type = "object", properties = new { } }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        return Task.FromResult(mgr?.Status() ?? "Browser is not running. Call browser_start.");
    }
}
