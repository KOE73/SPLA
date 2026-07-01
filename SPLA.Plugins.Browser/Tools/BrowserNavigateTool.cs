using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserNavigateTool : IMcpTool
{
    public string Name => "browser_navigate";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Opens a URL on a tab (the active tab by default). Waits for the page to finish loading.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url = new { type = "string", description = "URL to navigate to." },
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    wait_until = new { type = "string", @enum = new[] { "load", "domcontentloaded", "networkidle", "commit" }, description = "Default: load." },
                    timeout = new { type = "integer", description = "Timeout in milliseconds. Default: 30000." }
                },
                required = new[] { "url" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? url, tabId, waitUntil;
        int timeout;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            url = ToolJson.GetStringTrimmed(root, "url");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            waitUntil = ToolJson.GetStringTrimmed(root, "wait_until");
            timeout = ToolJson.GetInt32Clamped(root, "timeout", 30_000, 1000, 120_000);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        if (url is null) return "Error: 'url' is required.";

        var (resolvedTabId, page, error) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return error!;

        try
        {
            var response = await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = ParseWaitUntil(waitUntil),
                Timeout = timeout
            });
            var status = response != null ? $"{(int)response.Status} {response.StatusText}" : "(no response)";
            return $"Navigated tab {resolvedTabId} to {page.Url} — {status}. Call browser_snapshot to see the page.";
        }
        catch (TimeoutException ex) { return $"Error: navigation timed out — {ex.Message}"; }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_navigate", ex); }
    }

    private static WaitUntilState? ParseWaitUntil(string? value) => value?.ToLowerInvariant() switch
    {
        "domcontentloaded" => WaitUntilState.DOMContentLoaded,
        "networkidle" => WaitUntilState.NetworkIdle,
        "commit" => WaitUntilState.Commit,
        "load" => WaitUntilState.Load,
        _ => null
    };
}
