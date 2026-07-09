using Microsoft.Playwright;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser.Tools;

/// <summary>
/// Takes a real screenshot. Unlike every other browser_* tool, this one's result is more than the
/// text it returns: the PNG is stored in the chat's blob store AND queued in the chat's
/// <see cref="IPendingImageSink"/>, so the orchestrator injects it as an actual image on the
/// model's very next turn (see docs/browser-playwright-plugin-plan.md, "Скриншот ОБЯЗАН дойти...").
/// </summary>
public sealed class BrowserScreenshotTool : IMcpTool
{
    public string Name => "browser_screenshot";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Takes a real screenshot (viewport, full page, or one element by ref) and queues it so " +
                          "you SEE it as a picture on your next turn. Prefer browser_snapshot for finding/targeting " +
                          "elements; use this for things text can't capture — visual layout, charts, CAPTCHAs.",
            Scope = ToolScope.Internet,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    tab_id = new { type = "string", description = "Tab id from browser_tabs. Omit for the active tab." },
                    @ref = new { type = "string", description = "Element ref (from browser_snapshot) to screenshot just that element. Omit for the whole viewport/page." },
                    full_page = new { type = "boolean", description = "Capture the entire scrollable page, not just the viewport. Ignored when 'ref' is given. Default: false." }
                }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var mgr = BrowserToolBase.Current;
        if (mgr is null) return BrowserToolBase.NotRunning;

        var session = AgentSessionScope.Current;
        if (session is null) return "Error: no active chat session.";

        string? tabId, refId;
        bool fullPage;
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            tabId = ToolJson.GetStringTrimmed(root, "tab_id");
            refId = ToolJson.GetStringTrimmed(root, "ref");
            fullPage = ToolJson.GetBoolean(root, "full_page", false);
        }
        catch (JsonException) { return "Error: invalid JSON arguments."; }

        var (resolvedTabId, page, error) = BrowserToolBase.ResolveTab(mgr, tabId);
        if (page is null) return error!;

        try
        {
            byte[] bytes;
            string target;
            if (!string.IsNullOrWhiteSpace(refId))
            {
                var (locator, refError) = mgr.Refs.Resolve(page, resolvedTabId!, refId.Trim());
                if (locator is null) return refError!;
                bytes = await locator.ScreenshotAsync(new LocatorScreenshotOptions { Type = ScreenshotType.Png });
                target = $"element {refId}";
            }
            else
            {
                bytes = await page.ScreenshotAsync(new PageScreenshotOptions { Type = ScreenshotType.Png, FullPage = fullPage });
                target = fullPage ? "full page" : "viewport";
            }

            var handle = session.Blobs.Put(SPLA.Domain.Agent.BlobPayload.OfBytes(bytes, "image/png"));
            var dataUrl = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
            session.Images.Push(dataUrl);

            return $"Screenshot of {target} on tab {resolvedTabId} ({bytes.Length} bytes), stored as {handle}. " +
                   "It will appear as an image on your next turn.";
        }
        catch (PlaywrightException ex) { return BrowserToolBase.Fail("browser_screenshot", ex); }
    }
}
