using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserSwitchTabTool : IMcpTool
{
    public string Name => "browser_switch_tab";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Makes the given tab active — the implicit target for tools that omit tab_id.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new { tab_id = new { type = "string", description = "Tab id from browser_tabs (e.g. 't2')." } },
                required = new[] { "tab_id" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return Task.FromResult(BrowserToolBase.NotRunning);

        string? tabId;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            tabId = ToolJson.GetStringTrimmed(doc.RootElement, "tab_id");
        }
        catch (JsonException) { return Task.FromResult(ToolResult.Fail("Error: invalid JSON arguments.", "invalid json")); }

        if (tabId is null) return Task.FromResult(ToolResult.Fail("Error: 'tab_id' is required.", "missing tab_id"));
        return Task.FromResult(mgr.Pages.SetActive(tabId)
            ? ToolResult.Text($"Active tab is now {tabId}.")
            : ToolResult.Refuse($"Error: unknown tab '{tabId}'. Call browser_tabs to list open tabs.", "unknown tab"));
    }
}
