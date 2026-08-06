using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserCloseTabTool : IMcpTool
{
    public string Name => "browser_close_tab";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Closes a tab. Omit tab_id to close the active tab.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new { tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." } }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? tabId;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            tabId = ToolJson.GetStringTrimmed(doc.RootElement, "tab_id");
        }
        catch (JsonException) { return ToolResult.Fail("Error: invalid JSON arguments.", "invalid json"); }

        var (resolvedId, _, error) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (error != null) return error;

        try
        {
            var closed = await mgr.Pages.CloseAsync(resolvedId!);
            return closed
                ? ToolResult.Text($"Closed tab {resolvedId}.")
                : ToolResult.Refuse($"Error: unknown tab '{resolvedId}'.", "unknown tab");
        }
        catch (Exception ex) { return BrowserToolBase.Fail("browser_close_tab", ex); }
    }
}
