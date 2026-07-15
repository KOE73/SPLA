using System.Runtime.Versioning;
using Microsoft.Win32;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Registers the .spla file extension with the desktop app in Windows Explorer, so double-clicking
/// a project file launches it. Per-user (HKCU\Software\Classes), no admin rights required.
/// </summary>
public static class SystemOps
{
    public static SystemRegisterAssociationResultPayload RegisterFileAssociation()
    {
        if (!OperatingSystem.IsWindows())
            return new() { Ok = false, Message = "File association is only supported on Windows." };

        var exe = FindUiExecutable();
        if (exe == null)
            return new() { Ok = false, Message = "Could not locate SPLA.UI.Avalonia.exe (desktop app) to register." };

        try
        {
            RegisterWindows(exe);
            return new() { Ok = true, Message = $"Registered .spla files to open with {exe}." };
        }
        catch (Exception ex)
        {
            return new() { Ok = false, Message = $"Error registering association: {ex.Message}" };
        }
    }

    [SupportedOSPlatform("windows")]
    private static void RegisterWindows(string exePath)
    {
        const string progId = "SPLA.Project.1";

        using (var ext = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.spla"))
            ext!.SetValue("", progId);

        using (var cls = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
            cls!.SetValue("", "SPLA Project");

        using (var icon = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}\DefaultIcon"))
            icon!.SetValue("", $"\"{exePath}\",0");

        using (var cmd = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}\shell\open\command"))
            cmd!.SetValue("", $"\"{exePath}\" \"%1\"");

        Shell32.NotifyAssociationsChanged();
    }

    /// <summary>Finds the desktop shell exe: published next to the service, or in the dev build tree.</summary>
    private static string? FindUiExecutable()
    {
        var baseDir = AppContext.BaseDirectory;

        // 1) Published: SPLA.UI.Avalonia.exe sits next to the service exe (see EmbeddedServiceLauncher,
        //    which resolves the inverse — the CLI next to the UI exe).
        var sibling = Path.Combine(baseDir, "SPLA.UI.Avalonia.exe");
        if (File.Exists(sibling)) return sibling;

        // 2) Dev tree: walk up to the repo root and find SPLA.UI.Avalonia/bin/<cfg>/<tfm>/SPLA.UI.Avalonia.exe.
        var dir = new DirectoryInfo(baseDir);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "SPLA.UI.Avalonia", "bin");
            if (Directory.Exists(candidate))
            {
                var found = Directory.EnumerateFiles(candidate, "SPLA.UI.Avalonia.exe", SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (found != null) return found;
            }

            var publishCandidate = Path.Combine(dir.FullName, ".publish", "work", "SPLA.UI.Avalonia.exe");
            if (File.Exists(publishCandidate)) return publishCandidate;
        }

        return null;
    }
}

[SupportedOSPlatform("windows")]
internal static class Shell32
{
    [System.Runtime.InteropServices.DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int wEventId, int uFlags, nint dwItem1, nint dwItem2);

    private const int SHCNE_ASSOCCHANGED = 0x08000000;
    private const int SHCNF_IDLIST = 0x0000;

    public static void NotifyAssociationsChanged() =>
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, 0, 0);
}
