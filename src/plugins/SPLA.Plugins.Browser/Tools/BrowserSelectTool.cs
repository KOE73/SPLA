using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserSelectTool : IMcpTool
{
    public string Name => "browser_select";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Selects one or more option(s) by value in a <select>, targeted by ref (preferred) or a CSS selector.",
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
                    values = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Option value(s) to select (the <option value=...>, not its visible label)."
                    }
                },
                required = new[] { "values" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? refId, selector, tabId;
        string[]? values;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            refId = ToolJson.GetStringTrimmed(root, "ref");
            selector = ToolJson.GetStringTrimmed(root, "selector");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            values = ToolJson.GetStringArray(root, "values");
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        if (values is null || values.Length == 0) return "Error: 'values' must be a non-empty array.";

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        var (locator, targetError) = BrowserToolBase.ResolveTarget(mgr, page, resolvedTabId!, refId, selector);
        if (locator is null) return targetError!;

        try
        {
            var selected = await locator.SelectOptionAsync(values);
            return $"Selected [{string.Join(", ", selected)}] on {(refId ?? selector)}, tab {resolvedTabId}.";
        }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_select", ex); }
    }
}
