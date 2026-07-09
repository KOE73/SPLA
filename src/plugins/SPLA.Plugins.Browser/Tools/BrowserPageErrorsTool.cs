using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

public sealed class BrowserPageErrorsTool : IMcpTool
{
    public string Name => "browser_page_errors";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Recent uncaught JavaScript errors for a tab.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new { tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." } }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return Task.FromResult(BrowserToolBase.NotRunning);

        string? tabId;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            tabId = ToolJson.GetStringTrimmed(doc.RootElement, "tab_id");
        }
        catch (JsonException) { return Task.FromResult("Error: invalid JSON arguments."); }

        var (resolvedTabId, _, error) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (error != null) return Task.FromResult(error);

        var entries = mgr.DiagnosticsFor(resolvedTabId!).Errors();
        return Task.FromResult(entries.Count == 0
            ? $"No page errors recorded for tab {resolvedTabId}."
            : string.Join("\n---\n", entries));
    }
}
