using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;

namespace SPLA.Platform;

/// <summary>Which browser to target when opening a URL.</summary>
public enum BrowserTarget
{
    /// <summary>Whatever the OS/user has configured as the default handler for http(s).</summary>
    SystemDefault,
    Chrome,
    Edge
}

/// <summary>
/// Opens a URL in a browser, either the OS default or a specific one (used e.g. by "open in Chrome"
/// affordances that want a real browser tab rather than the app's embedded WebView). Cross-platform:
/// Windows resolves known executables via the registry App Paths key and well-known Program Files
/// locations; macOS and Linux shell out to the platform's own "open with app" commands.
/// <para>
/// Every launch is wrapped in try/catch and reported back as a bool rather than thrown. This is not
/// defensive paranoia: on a headless server (SPLA.Server, CI, an SSH session with no desktop) there is
/// no shell/desktop session for <c>ShellExecute</c> or <c>xdg-open</c> to hand off to, and that failure
/// is an expected runtime condition — not a bug to crash the host over. Callers that DO have somewhere
/// to show the error (a status bar, a toast) get a bool to react to; callers that don't can ignore it.
/// </para>
/// </summary>
public static class BrowserLauncher
{
    /// <summary>Opens <paramref name="url"/> with the requested <paramref name="target"/>, falling back
    /// to the system default if a specific browser can't be resolved. Returns false instead of throwing
    /// on failure (see class remarks).</summary>
    public static bool Open(string url, BrowserTarget target = BrowserTarget.SystemDefault)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        try
        {
            if (OperatingSystem.IsWindows()) return OpenWindows(url, target);
            if (OperatingSystem.IsMacOS()) return OpenMacOs(url, target);
            if (OperatingSystem.IsLinux()) return OpenLinux(url, target);

            // Unknown platform: best effort via ShellExecute-equivalent, still guarded by the outer catch.
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool OpenWindows(string url, BrowserTarget target)
    {
        var exe = target switch
        {
            BrowserTarget.Chrome => ResolveWindowsAppPath("chrome.exe") ?? ResolveWindowsWellKnownPath(
                @"Google\Chrome\Application\chrome.exe"),
            BrowserTarget.Edge => ResolveWindowsAppPath("msedge.exe") ?? ResolveWindowsWellKnownPath(
                @"Microsoft\Edge\Application\msedge.exe"),
            _ => null
        };

        if (exe != null)
        {
            var psi = new ProcessStartInfo { FileName = exe, UseShellExecute = false };
            psi.ArgumentList.Add(url);
            Process.Start(psi);
            return true;
        }

        // Default handler, or a specific browser we couldn't find — ShellExecute against the URL lets
        // Windows pick, which is at least a working browser even if not the one that was asked for.
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        return true;
    }

    /// <summary>Looks up an executable registered under the "App Paths" registry convention, which is
    /// how installers (including Chrome's and Edge's) tell Windows where they live without requiring
    /// PATH edits.</summary>
    [SupportedOSPlatform("windows")]
    private static string? ResolveWindowsAppPath(string exeName)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                $@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{exeName}");
            var path = key?.GetValue(null) as string;
            return path != null && File.Exists(path) ? path : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Falls back to the two Program Files roots when the App Paths registry key is missing
    /// (seen on some portable/per-user Chrome installs that don't register it).</summary>
    private static string? ResolveWindowsWellKnownPath(string relativePath)
    {
        foreach (var root in new[]
                 {
                     Environment.GetEnvironmentVariable("ProgramFiles"),
                     Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
                     Environment.GetEnvironmentVariable("LocalAppData")
                 })
        {
            if (string.IsNullOrEmpty(root)) continue;
            var candidate = Path.Combine(root, relativePath);
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    private static bool OpenMacOs(string url, BrowserTarget target)
    {
        var psi = target switch
        {
            BrowserTarget.Chrome => MacOpen("-a", "Google Chrome", url),
            BrowserTarget.Edge => MacOpen("-a", "Microsoft Edge", url),
            _ => MacOpen(url)
        };
        Process.Start(psi);
        return true;
    }

    private static ProcessStartInfo MacOpen(params string[] args)
    {
        var psi = new ProcessStartInfo { FileName = "open", UseShellExecute = false };
        foreach (var a in args) psi.ArgumentList.Add(a);
        return psi;
    }

    private static bool OpenLinux(string url, BrowserTarget target)
    {
        var command = target switch
        {
            BrowserTarget.Chrome => "google-chrome",
            BrowserTarget.Edge => "microsoft-edge",
            _ => "xdg-open"
        };

        try
        {
            var psi = new ProcessStartInfo { FileName = command, UseShellExecute = false };
            psi.ArgumentList.Add(url);
            Process.Start(psi);
            return true;
        }
        catch when (target != BrowserTarget.SystemDefault)
        {
            // Requested browser isn't installed under its usual command name — fall back to whatever
            // xdg-open resolves to rather than failing the whole request.
            var psi = new ProcessStartInfo { FileName = "xdg-open", UseShellExecute = false };
            psi.ArgumentList.Add(url);
            Process.Start(psi);
            return true;
        }
    }
}
