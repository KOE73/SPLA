using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserFillTool : IMcpTool
{
    public string Name => "browser_fill";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Clears a field and sets its value in one step, targeted by ref (preferred) or a CSS selector.",
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
                    value = new { type = "string", description = "Value to set." },
                    timeout = new { type = "integer", description = "Timeout in milliseconds. Default: 10000." }
                },
                required = new[] { "value" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? refId, selector, tabId, value;
        int timeout;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            refId = ToolJson.GetStringTrimmed(root, "ref");
            selector = ToolJson.GetStringTrimmed(root, "selector");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            value = ToolJson.GetString(root, "value");
            timeout = ToolJson.GetInt32Clamped(root, "timeout", 10_000, 500, 60_000);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        if (value is null) return "Error: 'value' is required.";

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        var (locator, targetError) = BrowserToolBase.ResolveTarget(mgr, page, resolvedTabId!, refId, selector);
        if (locator is null) return targetError!;

        try
        {
            await locator.FillAsync(value, new LocatorFillOptions { Timeout = timeout });
            return $"Filled {(refId ?? selector)} on tab {resolvedTabId}.";
        }
        catch (TimeoutException ex) { return $"Error: fill timed out — {ex.Message}"; }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_fill", ex); }
    }
}
