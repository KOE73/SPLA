using System;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Threading;

namespace SPLA.UI.Avalonia.Helpers;

/// <summary>
/// Bridges events from the hosted web client to the native Avalonia chrome via
/// <c>window.chrome.webview.postMessage</c>:
/// <list type="bullet">
/// <item><c>{ kind: "appearance", theme, density }</c> — applied through
/// <see cref="App.ChangeTheme"/>/<see cref="App.ChangeDensity"/> so the whole app follows one
/// appearance event.</item>
/// <item><c>{ kind: "project", projectName }</c> — the web client's in-page project focus changed
/// (see web/src/state/project.ts). Without this the OS window title/taskbar thumbnail stayed on
/// whatever project the process started with even after the sidebar's ProjectPicker switched focus
/// to a different one — confusing next to the native title-bar picker, which spawns a whole new
/// window (and so gets a correct title "for free"). <paramref name="onProjectChanged"/> lets the
/// specific window that owns this browser update its own <c>Title</c>; optional because only
/// <see cref="MainWindow"/> has a meaningful title to update — <see cref="SurfaceWindow"/> frames
/// (debug/settings/wire) don't call this.</item>
/// </list>
/// </summary>
public static class WebViewBridge
{
    public static void Attach(NativeWebView browser, Action<string?>? onProjectChanged = null)
    {
        browser.WebMessageReceived += (_, e) => Handle(e.Body, onProjectChanged);
    }

    private static void Handle(string? message, Action<string?>? onProjectChanged)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        string? theme = null, density = null, kind = null, projectName = null;
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return;
            if (root.TryGetProperty("kind", out var k)) kind = k.GetString();
            if (root.TryGetProperty("theme", out var t)) theme = t.GetString();
            if (root.TryGetProperty("density", out var d)) density = d.GetString();
            if (root.TryGetProperty("projectName", out var p)) projectName = p.GetString();
        }
        catch { return; }   // not our message — ignore

        // The post may arrive off the UI thread; both branches touch UI-thread-affine state.
        switch (kind)
        {
            case "appearance":
                Dispatcher.UIThread.Post(() =>
                {
                    if (!string.IsNullOrWhiteSpace(theme)) App.ChangeTheme(theme!);
                    if (!string.IsNullOrWhiteSpace(density)) App.ChangeDensity(density!);
                });
                break;

            case "project":
                Dispatcher.UIThread.Post(() => onProjectChanged?.Invoke(projectName));
                break;
        }
    }
}
