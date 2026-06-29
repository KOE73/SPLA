using System;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Threading;

namespace SPLA.UI.Avalonia.Helpers;

/// <summary>
/// Bridges appearance events from the hosted web client to the native Avalonia chrome. The web client
/// posts <c>{ kind: "appearance", theme, density }</c> via <c>window.chrome.webview.postMessage</c>
/// whenever the theme/density changes (on connect, on a server broadcast, or on instant preview); this
/// applies it to the window frame through <see cref="App.ChangeTheme"/>/<see cref="App.ChangeDensity"/>
/// so the native title bar follows the same single appearance event the webviews react to.
/// </summary>
public static class WebViewBridge
{
    public static void Attach(NativeWebView browser)
    {
        browser.WebMessageReceived += (_, e) => Handle(e.Body);
    }

    private static void Handle(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        string? theme = null, density = null, kind = null;
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return;
            if (root.TryGetProperty("kind", out var k)) kind = k.GetString();
            if (root.TryGetProperty("theme", out var t)) theme = t.GetString();
            if (root.TryGetProperty("density", out var d)) density = d.GetString();
        }
        catch { return; }   // not our message — ignore

        if (kind != "appearance") return;

        // The post may arrive off the UI thread; theming touches Application resources.
        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrWhiteSpace(theme)) App.ChangeTheme(theme!);
            if (!string.IsNullOrWhiteSpace(density)) App.ChangeDensity(density!);
        });
    }
}
