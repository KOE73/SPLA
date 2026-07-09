using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserPressTool : IMcpTool
{
    public string Name => "browser_press";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Presses a key or key combination (e.g. 'Enter', 'Control+A'), on an element by ref/selector " +
                          "or, if neither is given, on whatever currently has focus on the page.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    key = new { type = "string", description = "Key or combination, e.g. 'Enter', 'Escape', 'Control+A'." },
                    @ref = new { type = "string", description = "Element ref from browser_snapshot. Omit to press on the page's current focus." },
                    selector = new { type = "string", description = "CSS selector, when no ref is available." },
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." }
                },
                required = new[] { "key" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? key, refId, selector, tabId;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            key = ToolJson.GetStringTrimmed(root, "key");
            refId = ToolJson.GetStringTrimmed(root, "ref");
            selector = ToolJson.GetStringTrimmed(root, "selector");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        if (key is null) return "Error: 'key' is required.";

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        try
        {
            if (!string.IsNullOrWhiteSpace(refId) || !string.IsNullOrWhiteSpace(selector))
            {
                var (locator, targetError) = BrowserToolBase.ResolveTarget(mgr, page, resolvedTabId!, refId, selector);
                if (locator is null) return targetError!;
                await locator.PressAsync(key);
            }
            else
            {
                await page.Keyboard.PressAsync(key);
            }
            return $"Pressed '{key}' on tab {resolvedTabId}.";
        }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_press", ex); }
    }
}
