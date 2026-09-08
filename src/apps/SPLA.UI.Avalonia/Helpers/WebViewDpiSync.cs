using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;

namespace SPLA.UI.Avalonia.Helpers;

/// <summary>
/// Keeps a <see cref="NativeWebView"/>'s Chromium rasterization scale in step with the Avalonia
/// window it lives in.
///
/// A real top-level Chrome window gets DPI for free: it owns an HWND, and Windows tells any HWND
/// its monitor's scale automatically, including when the window is dragged to a monitor with a
/// different one. WebView2 hosted through Avalonia.Controls.WebView doesn't have that — on Windows
/// it composites into Avalonia's own render tree via DirectComposition (no real top-level HWND of
/// its own sat on the target monitor), so Chromium's usual automatic per-monitor DPI detection has
/// nothing to key off. Nothing in Avalonia.Controls.WebView calls the one API that fixes this
/// (<c>ICoreWebView2Controller3.RasterizationScale</c>) — confirmed by reading its IL — so every
/// <see cref="NativeWebView"/> in this app is stuck at whatever DPI happened to be current the
/// moment its WebView2 controller was created, DPI-manifest fix (PerMonitorV2) notwithstanding.
///
/// Same access pattern as <see cref="WebViewDevTools"/>: the interface below is not the official
/// WebView2 SDK (whose NuGet package resolves to a WinRT projection with no raw COM surface for this
/// project's TFM, and drags in conflicting WPF/WinForms assemblies) — it is a minimal hand-declared
/// vtable stub with the right GUID and just the two RasterizationScale accessors, which are the
/// first members WebView2Controller3 declares on its own (confirmed via reflection, DeclaredOnly),
/// so they land on the correct vtable slots without needing to restate the rest of the interface.
/// </summary>
[ComImport, Guid("F9614724-5D2B-41DC-AEF7-73D62B51543B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICoreWebView2Controller3
{
    double GetRasterizationScale();
    void SetRasterizationScale(double scale);
}

public static class WebViewDpiSync
{
    public static bool TrySync(NativeWebView browser, double scale)
    {
        try
        {
            if (browser.TryGetPlatformHandle() is not IWindowsWebView2PlatformHandle handle) return false;
            var ptr = handle.CoreWebView2Controller;
            if (ptr == IntPtr.Zero) return false;

            var unknown = Marshal.GetObjectForIUnknown(ptr);
            if (unknown is not ICoreWebView2Controller3 controller) return false;

            controller.SetRasterizationScale(scale);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Keeps <paramref name="browser"/> synced to <paramref name="window"/>'s own scale for
    /// as long as the window lives: on every <see cref="TopLevel.ScalingChanged"/> (fires when the
    /// window is dragged to a monitor with a different DPI), and once as soon as the underlying
    /// WebView2 controller actually exists — an immediate call here is usually too early (the native
    /// control isn't created yet) and would just silently no-op, so this waits for
    /// <see cref="NativeWebView.AdapterCreated"/>, the point the controller is guaranteed to be
    /// there, and applies the window's current scale then.</summary>
    public static void Track(TopLevel window, NativeWebView browser)
    {
        browser.AdapterCreated += (_, _) => TrySync(browser, window.RenderScaling);
        window.ScalingChanged += (_, _) => TrySync(browser, window.RenderScaling);
    }
}
