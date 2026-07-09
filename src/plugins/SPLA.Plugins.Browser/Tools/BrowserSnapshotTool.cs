using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserSnapshotTool : IMcpTool
{
    public string Name => "browser_snapshot";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Your primary way of perceiving a page: a compact tree of interactive/notable elements " +
                          "(links, buttons, inputs, headings, landmarks), each tagged with a short ref like " +
                          "[ref=e3.12]. Use those refs to target browser_click/fill/press/select. Call this again " +
                          "after navigating or after any action that changes the page — refs from an older " +
                          "snapshot become stale.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    max_chars = new { type = "integer", description = "Max characters of the tree to return (default 8000)." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? tabId;
        int maxChars;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            maxChars = ToolJson.GetInt32Clamped(root, "max_chars", 8000, 500, 30_000);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        var (resolvedTabId, page, error) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return error!;

        try
        {
            return await BrowserSnapshotService.CaptureAsync(page, mgr.Refs, resolvedTabId!, maxChars);
        }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_snapshot", ex); }
    }
}
