using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SPLA.Platform;

/// <summary>
/// Windows shell hooks that make the app feel native: a distinct AppUserModelID (so the taskbar
/// groups/pins us separately from other apps sharing the same exe name) and the Explorer "recent
/// documents" jump list. Moved here from the Avalonia project — this is OS integration, not UI, and
/// belongs next to the other platform-specific code rather than living in the desktop shell by
/// accident of history. No-ops on non-Windows so callers don't need their own OS guard.
/// </summary>
public static class WindowsShellIntegration
{
    private const string AppUserModelId = "SPLA.Desktop";

    private const uint SHARD_PATHW = 0x00000003;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appId);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern void SHAddToRecentDocs(uint flags, string path);

    public static void Initialize()
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            Marshal.ThrowExceptionForHR(SetCurrentProcessExplicitAppUserModelID(AppUserModelId));
        }
        catch
        {
            // Ignore if already set or fails
        }
    }

    public static void AddRecentProject(string splaFilePath)
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (string.IsNullOrWhiteSpace(splaFilePath))
            return;

        try
        {
            string fullPath = Path.GetFullPath(splaFilePath);

            if (!File.Exists(fullPath))
                return;

            SHAddToRecentDocs(SHARD_PATHW, fullPath);
        }
        catch
        {
            // Ignore errors here
        }
    }
}
