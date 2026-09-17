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
/// it composites into Avalonia's own render tree (no real top-level HWND of its own sat on the
/// target monitor), so Chromium's usual automatic per-monitor DPI detection has nothing to key off.
/// Nothing in Avalonia.Controls.WebView calls the one API that fixes this
/// (<c>ICoreWebView2Controller3.RasterizationScale</c>) — confirmed by reading its IL — so without
/// this every <see cref="NativeWebView"/> is stuck at whatever DPI happened to be current the moment
/// its WebView2 controller was created, the PerMonitorV2 manifest notwithstanding.
///
/// The interface below is hand-declared rather than taken from the official WebView2 SDK, whose
/// NuGet package resolves to a WinRT projection with no raw COM surface for this project's TFM and
/// drags in conflicting WPF/WinForms assemblies.
/// </summary>
/// <remarks>
/// The 25 <c>Slot*</c> members are load-bearing and must not be removed, reordered, or called.
/// A COM interface's vtable is IUnknown, then every method of every base interface, then its own:
/// ICoreWebView2Controller3 extends Controller2 (2 methods) extends Controller (23 methods), so the
/// real accessors sit at vtable slot 25 onward. Declaring only the accessors — as an earlier version
/// of this file did — silently aims them at slot 0 instead: SetRasterizationScale then lands on
/// <c>put_IsVisible</c>, so the scale is never applied and the whole thing looks like it merely does
/// nothing, while the member next to it lands on <c>put_Bounds</c>, which takes a RECT by value and
/// dereferences whatever it was handed — a null-deref that kills the process outright and that no
/// try/catch can intercept. Only the count and order of these placeholders matters, never their
/// signatures, since none is ever invoked; the names record what each slot really is. The counts
/// were read back off Avalonia's own interop metadata rather than eyeballed.
/// </remarks>
[ComImport, Guid("F9614724-5D2B-41DC-AEF7-73D62B51543B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICoreWebView2Controller3
{
    // --- ICoreWebView2Controller (23) ---
    void Slot00_get_IsVisible();
    void Slot01_put_IsVisible();
    void Slot02_get_Bounds();
    void Slot03_put_Bounds();
    void Slot04_get_ZoomFactor();
    void Slot05_put_ZoomFactor();
    void Slot06_add_ZoomFactorChanged();
    void Slot07_remove_ZoomFactorChanged();
    void Slot08_SetBoundsAndZoomFactor();
    void Slot09_MoveFocus();
    void Slot10_add_MoveFocusRequested();
    void Slot11_remove_MoveFocusRequested();
    void Slot12_add_GotFocus();
    void Slot13_remove_GotFocus();
    void Slot14_add_LostFocus();
    void Slot15_remove_LostFocus();
    void Slot16_add_AcceleratorKeyPressed();
    void Slot17_remove_AcceleratorKeyPressed();
    void Slot18_get_ParentWindow();
    void Slot19_put_ParentWindow();
    void Slot20_NotifyParentWindowPositionChanged();
    void Slot21_Close();
    void Slot22_get_CoreWebView2();

    // --- ICoreWebView2Controller2 (2) ---
    void Slot23_get_DefaultBackgroundColor();
    void Slot24_put_DefaultBackgroundColor();

    // --- ICoreWebView2Controller3's own: the only members this file calls ---
    double GetRasterizationScale();
    void SetRasterizationScale(double scale);
    int GetShouldDetectMonitorScaleChanges();
    void SetShouldDetectMonitorScaleChanges(int value);
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

            // The documented pairing for taking manual control of the scale: left on, Chromium's own
            // monitor detection can overwrite what we set — and in this composition-hosted setup it
            // has no real HWND to read a monitor from anyway, so it has nothing better to offer.
            controller.SetShouldDetectMonitorScaleChanges(0);
            controller.SetRasterizationScale(scale);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Keeps <paramref name="browser"/> synced to <paramref name="window"/>'s own scale for
    /// as long as the window lives. <see cref="TopLevel.ScalingChanged"/> is the one that matters
    /// when a window is dragged between monitors; <see cref="NativeWebView.AdapterCreated"/> and
    /// <see cref="NativeWebView.NavigationCompleted"/> cover the window's first paint from either
    /// side of whichever one first has a live controller behind it — a call made too early finds a
    /// null pointer and no-ops, so both are tried rather than betting on one.</summary>
    public static void Track(TopLevel window, NativeWebView browser)
    {
        browser.AdapterCreated += (_, _) => TrySync(browser, window.RenderScaling);
        browser.NavigationCompleted += (_, _) => TrySync(browser, window.RenderScaling);
        window.ScalingChanged += (_, _) => TrySync(browser, window.RenderScaling);
    }
}
