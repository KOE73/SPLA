using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;

namespace SPLA.UI.Avalonia.Helpers;

/// <summary>
/// Opens the real WebView2 DevTools window for a specific <see cref="NativeWebView"/> instance —
/// not "open this URL in a separate browser" (a different process, different WebView2 profile,
/// tells you nothing about what THIS window is actually rendering).
/// Avalonia.Controls.WebView has no devtools API of its own, but on Win32 its platform handle
/// implements <see cref="IWindowsWebView2PlatformHandle"/>, which hands back the raw
/// <c>ICoreWebView2</c> COM pointer. QueryInterface it through <see cref="ICoreWebView2"/> — a
/// hand-declared vtable stub, not the official WebView2 SDK: that SDK's NuGet package resolves to a
/// WinRT-projection assembly for this project's TFM with no raw COM interop surface, and pulls in
/// WPF/WinForms assembly-version conflicts besides. The GUID and method order below are copied from
/// Avalonia.Controls.WebView's own (internal, unreachable from here) interop declaration for the
/// same interface, so the vtable layout matches exactly up through OpenDevToolsWindow; everything
/// before it must stay in place for the vtable slot to land right, even though this type never calls
/// those members — their signatures are widened to IntPtr since only slot position, not the exact
/// parameter types, matters for members we never invoke.
/// </summary>
[ComImport, Guid("76ECEACB-0462-4D94-AC83-423A6793775E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICoreWebView2
{
    IntPtr GetSettings();
    string GetSource();
    void Navigate(string uri);
    void NavigateToString(string html);
    void AddNavigationStarting(IntPtr handler, out long token);
    void RemoveNavigationStarting(long token);
    void AddContentLoading(IntPtr handler, out long token);
    void RemoveContentLoading(long token);
    void AddSourceChanged(IntPtr handler, out long token);
    void RemoveSourceChanged(long token);
    void AddHistoryChanged(IntPtr handler, out long token);
    void RemoveHistoryChanged(long token);
    void AddNavigationCompleted(IntPtr handler, out long token);
    void RemoveNavigationCompleted(long token);
    void AddFrameNavigationStarting(IntPtr handler, out long token);
    void RemoveFrameNavigationStarting(long token);
    void AddFrameNavigationCompleted(IntPtr handler, out long token);
    void RemoveFrameNavigationCompleted(long token);
    void AddScriptDialogOpening(IntPtr handler, out long token);
    void RemoveScriptDialogOpening(long token);
    void AddPermissionRequested(IntPtr handler, out long token);
    void RemovePermissionRequested(long token);
    void AddProcessFailed(IntPtr handler, out long token);
    void RemoveProcessFailed(long token);
    void AddScriptToExecuteOnDocumentCreated(string script, IntPtr handler);
    void RemoveScriptToExecuteOnDocumentCreated(string id);
    void ExecuteScript(string script, IntPtr handler);
    void CapturePreview(int imageFormat, IntPtr stream, IntPtr handler);
    void Reload();
    void PostWebMessageAsJson(string json);
    void PostWebMessageAsString(string message);
    void AddWebMessageReceived(IntPtr handler, out long token);
    void RemoveWebMessageReceived(long token);
    void CallDevToolsProtocolMethod(string methodName, string parametersJson, IntPtr handler);
    uint GetBrowserProcessId();
    int GetCanGoBack();
    int GetCanGoForward();
    void GoBack();
    void GoForward();
    IntPtr GetDevToolsProtocolEventReceiver(string eventName);
    void Stop();
    void AddNewWindowRequested(IntPtr handler, out long token);
    void RemoveNewWindowRequested(long token);
    void AddDocumentTitleChanged(IntPtr handler, out long token);
    void RemoveDocumentTitleChanged(long token);
    string GetDocumentTitle();
    void AddHostObjectToScript(string name, IntPtr value);
    void RemoveHostObjectFromScript(string name);

    void OpenDevToolsWindow();
}

public static class WebViewDevTools
{
    public static bool TryOpen(NativeWebView browser)
    {
        try
        {
            if (browser.TryGetPlatformHandle() is not IWindowsWebView2PlatformHandle handle) return false;
            var ptr = handle.CoreWebView2;
            if (ptr == IntPtr.Zero) return false;

            var unknown = Marshal.GetObjectForIUnknown(ptr);
            if (unknown is not ICoreWebView2 coreWebView2) return false;

            coreWebView2.OpenDevToolsWindow();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
