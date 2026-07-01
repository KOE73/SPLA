using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserScrollTool : IMcpTool
{
    public string Name => "browser_scroll";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Scrolls the page. With 'ref', scrolls that element into view instead of scrolling by an amount.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    @ref = new { type = "string", description = "Element ref from browser_snapshot — scroll it into view. When given, dx/dy are ignored." },
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    dx = new { type = "integer", description = "Horizontal scroll amount in pixels. Default: 0." },
                    dy = new { type = "integer", description = "Vertical scroll amount in pixels (positive = down). Default: 800." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? refId, tabId;
        int dx, dy;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            refId = ToolJson.GetStringTrimmed(root, "ref");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            dx = ToolJson.GetInt32(root, "dx", 0);
            dy = ToolJson.GetInt32(root, "dy", 800);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        try
        {
            if (!string.IsNullOrWhiteSpace(refId))
            {
                var (locator, refError) = mgr.Refs.Resolve(page, resolvedTabId!, refId.Trim());
                if (locator is null) return refError!;
                await locator.ScrollIntoViewIfNeededAsync();
                return $"Scrolled {refId} into view on tab {resolvedTabId}.";
            }

            await page.Mouse.WheelAsync(dx, dy);
            return $"Scrolled tab {resolvedTabId} by (dx={dx}, dy={dy}).";
        }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_scroll", ex); }
    }
}
