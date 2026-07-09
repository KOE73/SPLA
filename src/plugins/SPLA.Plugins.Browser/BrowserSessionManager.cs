using Microsoft.Playwright;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser;

/// <summary>
/// Owns one chat's browser process end to end: launch/teardown, the tab registry, the ref registry,
/// and per-tab console/error diagnostics. One instance per chat, resolved via
/// <see cref="BrowserSessionRegistry"/> — never shared between chats. See that class for why.
/// </summary>
internal sealed class BrowserSessionManager : IAsyncDisposable
{
    private readonly object _lock = new();
    private readonly Dictionary<string, PageDiagnostics> _diagnostics = new();

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;

    public BrowserPageRegistry Pages { get; } = new();
    public ElementRefRegistry Refs { get; } = new();

    public bool IsRunning => _context != null;
    public string? Channel { get; private set; }
    public string? Profile { get; private set; }
    public bool Headless { get; private set; }

    public PageDiagnostics DiagnosticsFor(string tabId)
    {
        lock (_lock)
        {
            if (_diagnostics.TryGetValue(tabId, out var d)) return d;
            d = new PageDiagnostics();
            _diagnostics[tabId] = d;
            return d;
        }
    }

    /// <summary>
    /// Launches the browser. <paramref name="channel"/> null/empty means "auto": try the system
    /// Microsoft Edge first (no download), and only fall back to a managed Chromium (installing it
    /// on first use if needed) if Edge isn't available. An explicit channel is honoured as-is, with
    /// no fallback, so a deliberate choice surfaces its own errors directly.
    /// </summary>
    public async Task<string> StartAsync(
        string? channel, bool headless, string? profilePath, int? viewportWidth, int? viewportHeight,
        IReadOnlyList<string>? extraArgs = null,
        IReadOnlyList<string>? ignoreDefaultArgs = null)
    {
        if (IsRunning)
            return $"Browser already running ({Status()}). Call browser_stop first if you want to relaunch.";

        _playwright ??= await Playwright.CreateAsync();
        var viewport = viewportWidth is { } w && viewportHeight is { } h ? new ViewportSize { Width = w, Height = h } : null;
        var explicitChannel = NormalizeChannel(channel);

        string effectiveChannel;
        if (explicitChannel != null)
        {
            await LaunchWithInstallFallbackAsync(explicitChannel, headless, profilePath, viewport, extraArgs, ignoreDefaultArgs);
            effectiveChannel = explicitChannel;
        }
        else
        {
            try
            {
                await LaunchAsync("msedge", headless, profilePath, viewport, extraArgs, ignoreDefaultArgs);
                effectiveChannel = "msedge";
            }
            catch (PlaywrightException)
            {
                await LaunchWithInstallFallbackAsync(null, headless, profilePath, viewport, extraArgs, ignoreDefaultArgs);
                effectiveChannel = "chromium";
            }
        }

        Channel = effectiveChannel;
        Profile = string.IsNullOrEmpty(profilePath) ? "temporary" : profilePath;
        Headless = headless;

        WireContextEvents();

        var page = _context!.Pages.Count > 0 ? _context.Pages[0] : await _context.NewPageAsync();
        var tabId = RegisterPage(page);

        return $"Browser started. channel={Channel}, profile={Profile}, headless={Headless}. Active tab: {tabId} ({page.Url})";
    }

    private async Task LaunchWithInstallFallbackAsync(
        string? channel, bool headless, string? profilePath, ViewportSize? viewport,
        IReadOnlyList<string>? extraArgs, IReadOnlyList<string>? ignoreDefaultArgs)
    {
        try
        {
            await LaunchAsync(channel, headless, profilePath, viewport, extraArgs, ignoreDefaultArgs);
        }
        catch (PlaywrightException ex) when (channel is null && LooksLikeMissingBrowser(ex))
        {
            ProgressScope.Report(new ToolProgress { Message = "Chromium not installed — downloading it now (first run only)…" });
            var code = await Task.Run(() => Program.Main(new[] { "install", "chromium" }));
            if (code != 0)
                throw new InvalidOperationException(
                    $"Failed to install Chromium (exit code {code}). Try browser_start with channel=\"msedge\" or \"chrome\" instead.");
            await LaunchAsync(channel, headless, profilePath, viewport, extraArgs, ignoreDefaultArgs);
        }
    }

    private async Task LaunchAsync(
        string? channel, bool headless, string? profilePath, ViewportSize? viewport,
        IReadOnlyList<string>? extraArgs, IReadOnlyList<string>? ignoreDefaultArgs)
    {
        var ignore = ignoreDefaultArgs is { Count: > 0 } ? ignoreDefaultArgs.ToArray() : null;
        var args   = extraArgs is { Count: > 0 } ? extraArgs : null;

        if (!string.IsNullOrEmpty(profilePath))
        {
            System.IO.Directory.CreateDirectory(profilePath);
            _context = await _playwright!.Chromium.LaunchPersistentContextAsync(profilePath, new BrowserTypeLaunchPersistentContextOptions
            {
                Headless = headless,
                Channel = channel,
                ViewportSize = viewport,
                Args = args,
                IgnoreDefaultArgs = ignore
            });
            _browser = null;
        }
        else
        {
            _browser = await _playwright!.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Channel = channel,
                Args = args,
                IgnoreDefaultArgs = ignore
            });
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = viewport });
        }
    }

    private static string? NormalizeChannel(string? channel)
    {
        var c = channel?.Trim().ToLowerInvariant();
        return c switch
        {
            null or "" or "chromium" => null,
            "edge" => "msedge",
            _ => c
        };
    }

    private static bool LooksLikeMissingBrowser(PlaywrightException ex)
        => ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase)
           || ex.Message.Contains("playwright install", StringComparison.OrdinalIgnoreCase);

    private void WireContextEvents() => _context!.Page += (_, page) => RegisterPage(page);

    /// <summary>
    /// Registers <paramref name="page"/> in <see cref="Pages"/> and wires its console/error
    /// diagnostics, exactly once even if both a tool (e.g. <c>browser_new_tab</c>) and the context's
    /// <c>Page</c> event independently see the same page — see <see cref="BrowserPageRegistry.RegisterEx"/>.
    /// Public so tools that open a page directly (rather than waiting on a popup) can register it.
    /// </summary>
    public string RegisterPage(IPage page)
    {
        var (tabId, isNew) = Pages.RegisterEx(page);
        if (isNew)
        {
            var diag = DiagnosticsFor(tabId);
            page.Console += (_, msg) => diag.AddConsole($"[{msg.Type}] {msg.Text}");
            page.PageError += (_, error) => diag.AddError(error);
        }
        return tabId;
    }

    public string Status()
    {
        if (!IsRunning) return "Browser is not running.";
        var tabs = Pages.All();
        var sb = new StringBuilder();
        sb.AppendLine($"Running. channel={Channel}, profile={Profile}, headless={Headless}, tabs={tabs.Count}, active={Pages.ActiveTabId}");
        foreach (var (id, page) in tabs)
            sb.AppendLine($"  {id}{(id == Pages.ActiveTabId ? " [active]" : "")} — {page.Url}");
        return sb.ToString().TrimEnd();
    }

    public async Task<string> StopAsync()
    {
        if (!IsRunning) return "Browser is not running.";
        await TeardownAsync();
        return "Browser stopped.";
    }

    private async Task TeardownAsync()
    {
        try { if (_context != null) await _context.CloseAsync(); } catch { /* best effort */ }
        try { if (_browser != null) await _browser.CloseAsync(); } catch { /* best effort */ }
        _context = null;
        _browser = null;
        Pages.Clear();
        lock (_lock) _diagnostics.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        await TeardownAsync();
        _playwright?.Dispose();
        _playwright = null;
    }
}
