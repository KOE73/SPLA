using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

/// <summary>
/// Sets local files into a file input. No directory allowlist yet (Wave 2's UploadPolicy) — every
/// path the model can already read via the filesystem tools can be uploaded; this only verifies the
/// files exist before handing them to Playwright, so a bad path fails clearly instead of inside the
/// browser.
/// </summary>
public sealed class BrowserUploadTool : IMcpTool
{
    public string Name => "browser_upload";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Sets one or more local files into a file input element, targeted by ref (preferred) or a CSS selector.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.High,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    @ref = new { type = "string", description = "Element ref from browser_snapshot (the file input). Preferred over selector." },
                    selector = new { type = "string", description = "CSS selector, when no ref is available." },
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    files = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "Absolute local file path(s) to upload."
                    }
                },
                required = new[] { "files" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        string? refId, selector, tabId;
        string[]? files;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            refId = ToolJson.GetStringTrimmed(root, "ref");
            selector = ToolJson.GetStringTrimmed(root, "selector");
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            files = ToolJson.GetStringArray(root, "files");
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        if (files is null || files.Length == 0) return "Error: 'files' must be a non-empty array of local paths.";

        var missing = files.Where(f => !File.Exists(f)).ToArray();
        if (missing.Length > 0) return $"Error: file(s) not found: {string.Join(", ", missing)}";

        var (resolvedTabId, page, tabError) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return tabError!;

        var (locator, targetError) = BrowserToolBase.ResolveTarget(mgr, page, resolvedTabId!, refId, selector);
        if (locator is null) return targetError!;

        try
        {
            await locator.SetInputFilesAsync(files);
            return $"Uploaded {files.Length} file(s) to {(refId ?? selector)} on tab {resolvedTabId}.";
        }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_upload", ex); }
    }
}
