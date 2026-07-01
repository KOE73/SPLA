using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserWaitElementTool : IMcpTool
{
    public string Name => "browser_wait_element";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Waits for an element to reach a state (visible/hidden/attached/detached) instead of " +
                          "sleeping an arbitrary amount of time.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    @ref = new { type = "string", description = "Element ref from browser_snapshot. Preferred over selector." },
                    selector = new { type = "string", description = "CSS selector, when no ref is available." },
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    state = new { type = "string", @enum = new[] { "visible", "hidden", "attached", "detached" }, description = "Default: visible." },
                    timeout = new { type = "integer", description = "Timeout in milliseconds. Default: 15000." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? refId, selector, tabId, state;
        int timeout;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            refId = ToolJson.GetStringTrimmed(root, "ref");
            selector = ToolJson.GetStringTrimmed(root, "selector");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            state = ToolJson.GetStringTrimmed(root, "state");
            timeout = ToolJson.GetInt32Clamped(root, "timeout", 15_000, 500, 120_000);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        var (locator, targetError) = BrowserToolBase.ResolveTarget(mgr, page, resolvedTabId!, refId, selector);
        if (locator is null) return targetError!;

        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions { State = ParseState(state), Timeout = timeout });
            return $"{(refId ?? selector)} reached state '{state ?? "visible"}' on tab {resolvedTabId}.";
        }
        catch (TimeoutException ex) { return $"Error: wait timed out — {ex.Message}"; }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_wait_element", ex); }
    }

    private static WaitForSelectorState ParseState(string? value) => value?.ToLowerInvariant() switch
    {
        "hidden" => WaitForSelectorState.Hidden,
        "attached" => WaitForSelectorState.Attached,
        "detached" => WaitForSelectorState.Detached,
        _ => WaitForSelectorState.Visible
    };
}
