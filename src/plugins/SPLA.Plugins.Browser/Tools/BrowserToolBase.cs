using Microsoft.Playwright;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using System;

namespace SPLA.Plugins.Browser.Tools;

/// <summary>Shared per-chat session resolution and target-locator helpers for every browser_* tool.
/// Tools hold no constructor-injected state (see <see cref="BrowserSessionRegistry"/>) — everything
/// resolves from the ambient <see cref="AgentSessionScope"/> at call time.</summary>
internal static class BrowserToolBase
{
    public static ToolResult NotRunning
        => ToolResult.Refuse("Error: browser is not running. Call browser_start first.", "browser not running");

    /// <summary>The current chat's browser session, or null if none has been created yet
    /// (no browser_start call this chat).</summary>
    public static BrowserSessionManager? Current
        => AgentSessionScope.Current is { } s ? BrowserSessionRegistry.TryGet(s) : null;

    /// <summary>The current chat's browser session, creating an empty (not-yet-started) one if needed.
    /// Use from browser_start; every other tool should use <see cref="Current"/> and report
    /// <see cref="NotRunning"/> when null.</summary>
    public static BrowserSessionManager GetOrCreateSession()
        => BrowserSessionRegistry.GetOrCreate(
            AgentSessionScope.Current ?? throw new InvalidOperationException("No active chat session."));

    /// <summary>Resolves the target tab: explicit <paramref name="tabId"/>, or the session's active tab.</summary>
    public static (string? TabId, IPage? Page, ToolResult? Error) ResolveTab(BrowserSessionManager mgr, string? tabId)
    {
        if (!string.IsNullOrWhiteSpace(tabId))
            return mgr.Pages.TryGet(tabId, out var p)
                ? (tabId, p, null)
                : (null, null, ToolResult.Refuse(
                    $"Error: unknown tab '{tabId}'. Call browser_tabs to list open tabs.", "unknown tab"));

        var activeId = mgr.Pages.ActiveTabId;
        var active = mgr.Pages.Active;
        return active != null
            ? (activeId, active, null)
            : (null, null, ToolResult.Refuse(
                "Error: no open tab. Call browser_start or browser_new_tab.", "no open tab"));
    }

    /// <summary>Resolves an action target by ref (preferred, from the latest browser_snapshot) or a
    /// raw CSS selector. Role/name/text and coordinate fallbacks are not implemented yet (Wave 2).</summary>
    public static (ILocator? Locator, ToolResult? Error) ResolveTarget(
        BrowserSessionManager mgr, IPage page, string tabId, string? refId, string? selector)
    {
        if (!string.IsNullOrWhiteSpace(refId))
        {
            var (locator, refError) = mgr.Refs.Resolve(page, tabId, refId.Trim());
            return (locator, refError is null ? null : RefFailure(refError));
        }
        if (!string.IsNullOrWhiteSpace(selector))
            return (page.Locator(selector), null);
        return (null, ToolResult.Fail(
            "Error: provide either 'ref' (from browser_snapshot) or 'selector' (CSS).", "no target"));
    }

    /// <summary>Wraps an <see cref="ElementRefRegistry"/> resolution error (unknown or stale ref).</summary>
    public static ToolResult RefFailure(string message) => ToolResult.Fail(message, "ref not resolved");

    /// <summary>Standard error formatting for a failed Playwright call (timeout, navigation error, …).</summary>
    public static ToolResult Fail(string action, Exception ex)
        => ToolResult.Fail($"Error: {action} failed — {ex.Message}", $"{action} failed");
}
