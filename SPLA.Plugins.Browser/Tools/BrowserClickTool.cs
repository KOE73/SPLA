using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserClickTool : IMcpTool
{
    public string Name => "browser_click";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Clicks an element, targeted by ref (preferred, from browser_snapshot) or a CSS selector.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    @ref = new { type = "string", description = "Element ref from browser_snapshot. Preferred over selector." },
                    selector = new { type = "string", description = "CSS selector, when no ref is available." },
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    button = new { type = "string", @enum = new[] { "left", "right", "middle" }, description = "Default: left." },
                    double_click = new { type = "boolean", description = "Double-click instead of a single click. Default: false." },
                    timeout = new { type = "integer", description = "Timeout in milliseconds. Default: 10000." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? refId, selector, tabId, button;
        bool doubleClick;
        int timeout;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            refId = ToolJson.GetStringTrimmed(root, "ref");
            selector = ToolJson.GetStringTrimmed(root, "selector");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            button = ToolJson.GetStringTrimmed(root, "button");
            doubleClick = ToolJson.GetBoolean(root, "double_click", false);
            timeout = ToolJson.GetInt32Clamped(root, "timeout", 10_000, 500, 60_000);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        var (locator, targetError) = BrowserToolBase.ResolveTarget(mgr, page, resolvedTabId!, refId, selector);
        if (locator is null) return targetError!;

        try
        {
            if (doubleClick)
                await locator.DblClickAsync(new LocatorDblClickOptions { Button = ParseButton(button), Timeout = timeout });
            else
                await locator.ClickAsync(new LocatorClickOptions { Button = ParseButton(button), Timeout = timeout });

            return $"Clicked {(refId ?? selector)} on tab {resolvedTabId}.";
        }
        catch (TimeoutException ex) { return $"Error: click timed out — {ex.Message}"; }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_click", ex); }
    }

    private static MouseButton? ParseButton(string? value) => value?.ToLowerInvariant() switch
    {
        "right" => MouseButton.Right,
        "middle" => MouseButton.Middle,
        _ => MouseButton.Left
    };
}
