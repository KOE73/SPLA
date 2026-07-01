using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserWaitNavigationTool : IMcpTool
{
    public string Name => "browser_wait_navigation";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Waits for a navigation already in progress to finish — call right after an action " +
                          "that you expect to trigger one (e.g. clicking a submit button or a link).",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    wait_until = new { type = "string", @enum = new[] { "load", "domcontentloaded", "networkidle", "commit" }, description = "Default: load." },
                    timeout = new { type = "integer", description = "Timeout in milliseconds. Default: 15000." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? tabId, waitUntil;
        int timeout;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            waitUntil = ToolJson.GetStringTrimmed(root, "wait_until");
            timeout = ToolJson.GetInt32Clamped(root, "timeout", 15_000, 1000, 120_000);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        var (resolvedTabId, page, error) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return error!;

        try
        {
            await page.WaitForLoadStateAsync(ParseWaitUntil(waitUntil), new PageWaitForLoadStateOptions { Timeout = timeout });
            return $"Tab {resolvedTabId} settled at {page.Url}.";
        }
        catch (TimeoutException ex) { return $"Error: wait timed out — {ex.Message}"; }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_wait_navigation", ex); }
    }

    private static LoadState? ParseWaitUntil(string? value) => value?.ToLowerInvariant() switch
    {
        "domcontentloaded" => LoadState.DOMContentLoaded,
        "networkidle" => LoadState.NetworkIdle,
        "load" => LoadState.Load,
        _ => LoadState.Load
    };
}
