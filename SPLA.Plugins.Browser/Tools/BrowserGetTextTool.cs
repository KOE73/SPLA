using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserGetTextTool : IMcpTool
{
    public string Name => "browser_get_text";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Returns the visible text of the page, or of one element when 'ref' is given.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    @ref = new { type = "string", description = "Element ref from browser_snapshot. Omit for the whole page's body text." },
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    max_chars = new { type = "integer", description = "Max characters to return (default 5000)." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? refId, tabId;
        int maxChars;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            refId = ToolJson.GetStringTrimmed(root, "ref");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            maxChars = ToolJson.GetInt32Clamped(root, "max_chars", 5000, 200, 30_000);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        try
        {
            string text;
            if (!string.IsNullOrWhiteSpace(refId))
            {
                var (locator, refError) = mgr.Refs.Resolve(page, resolvedTabId!, refId.Trim());
                if (locator is null) return refError!;
                text = await locator.InnerTextAsync();
            }
            else
            {
                text = await page.Locator("body").InnerTextAsync();
            }

            if (text.Length > maxChars)
                text = text[..maxChars] + $"\n…(truncated at {maxChars} of {text.Length} chars)";

            return text;
        }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_get_text", ex); }
    }
}
