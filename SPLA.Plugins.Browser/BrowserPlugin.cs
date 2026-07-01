using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Browser.Tools;
using System.Collections.Generic;

namespace SPLA.Plugins.Browser;

/// <summary>
/// Browser automation (Playwright). Every tool resolves its session lazily from the ambient chat
/// (see <see cref="BrowserSessionRegistry"/>) rather than via constructor injection — unlike e.g.
/// <c>SqlPlugin</c>, a browser is an exclusive per-chat resource, and <see cref="Initialize"/> only
/// runs once per process, not once per chat.
/// </summary>
public class BrowserPlugin : ISplaPlugin
{
    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        settings.Plugins.TryGetValue("browser", out var section);
        var browserSettings = BrowserSettings.FromBlob(section?.Settings);
        return Initialize(settings, browserSettings);
    }

    private static IEnumerable<IMcpTool> Initialize(ResolvedSettings settings, BrowserSettings browserSettings) =>
    [
        // Lifecycle
        new BrowserStartTool(settings.WorkspacePath, browserSettings),
        new BrowserListProfilesTool(settings.WorkspacePath),
        new BrowserStopTool(),
        new BrowserStatusTool(),
        // Tabs
        new BrowserTabsTool(),
        new BrowserNewTabTool(),
        new BrowserSwitchTabTool(),
        new BrowserCloseTabTool(),
        // Navigation
        new BrowserNavigateTool(),
        new BrowserWaitNavigationTool(),
        // Perception
        new BrowserSnapshotTool(),
        new BrowserScreenshotTool(),
        new BrowserInspectTool(),
        new BrowserGetTextTool(),
        // Actions
        new BrowserClickTool(),
        new BrowserScrollTool(),
        new BrowserFillTool(),
        new BrowserPressTool(),
        new BrowserSelectTool(),
        new BrowserWaitElementTool(),
        // Files
        new BrowserUploadTool(),
        // Diagnostics
        new BrowserConsoleTool(),
        new BrowserPageErrorsTool(),
    ];
}
