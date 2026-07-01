using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserNewTabTool : IMcpTool
{
    public string Name => "browser_new_tab";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Opens a new tab and makes it active. Optionally navigates it to a URL right away.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url = new { type = "string", description = "Optional URL to navigate the new tab to." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? url;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            url = ToolJson.GetStringTrimmed(doc.RootElement, "url");
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        try
        {
            // Resolving the active page first gives us the live context to open a sibling tab on.
            var (_, activePage, error) = BrowserToolBase.ResolveTab(mgr, null);
            if (activePage is null) return error!;

            var page = await activePage.Context.NewPageAsync();
            var tabId = mgr.RegisterPage(page);
            mgr.Pages.SetActive(tabId);

            if (!string.IsNullOrWhiteSpace(url))
                await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load });

            return $"Opened tab {tabId} ({page.Url}).";
        }
        catch (Exception ex) { return BrowserToolBase.Fail("browser_new_tab", ex); }
    }
}
