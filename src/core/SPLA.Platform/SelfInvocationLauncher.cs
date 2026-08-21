using System;
using System.Diagnostics;
using System.IO;

namespace SPLA.Platform;

/// <summary>
/// Finds and relaunches "this same app" — used when the desktop shell needs a fresh process of itself
/// (one project = one window = one process, so opening/creating a project spawns a new instance rather
/// than switching in place inside the current one). Mirrors the resolution strategy in
/// <c>EmbeddedServiceLauncher.ResolveCliInvocation</c> for the sibling SPLA.CLI executable: prefer a
/// published, self-contained exe sitting next to the running process, fall back to the framework-
/// dependent dll launched via <c>dotnet</c>.
/// <para>
/// This was duplicated near-verbatim in two call sites in the Avalonia app (the main window's project
/// menu and the tray icon's project switcher) before being consolidated here. The two callers had
/// different error-handling needs — the main window can show a message in its title bar, the tray icon
/// has no UI surface to report through and used to swallow failures silently — so this helper reports
/// failure via the return value / a thrown exception from <see cref="Resolve"/> rather than hard-coding
/// either behaviour; each caller decides whether to surface or swallow it.
/// </para>
/// </summary>
public static class SelfInvocationLauncher
{
    /// <summary>Resolves how to relaunch the currently running app: a published
    /// <paramref name="exeName"/> next to the running process, or its <c>.dll</c> counterpart run via
    /// <c>dotnet</c> (dev tree / framework-dependent publish). Throws <see cref="FileNotFoundException"/>
    /// if neither is found — callers decide whether that's worth surfacing to the user or swallowing.</summary>
    public static (string Exe, string[] Args) Resolve(string exeName)
    {
        var baseDir = AppContext.BaseDirectory;
        var baseName = Path.GetFileNameWithoutExtension(exeName);

        var exe = Path.Combine(baseDir, exeName);
        if (File.Exists(exe)) return (exe, []);

        var dll = Path.Combine(baseDir, baseName + ".dll");
        if (File.Exists(dll)) return ("dotnet", [dll]);

        throw new FileNotFoundException($"Could not locate {exeName} to relaunch.");
    }

    /// <summary>Convenience wrapper over <see cref="Resolve"/>: starts a new detached process of this
    /// same app with <paramref name="extraArgs"/> appended after whatever args resolution already
    /// requires (e.g. the "dotnet" + dll pair). Returns false instead of throwing on failure so a caller
    /// with nowhere to report an error (the tray icon has no status bar) can ignore it, while a caller
    /// that does have somewhere to show it can call <see cref="Resolve"/> directly and catch.</summary>
    public static bool TryLaunch(string exeName, params string[] extraArgs)
    {
        try
        {
            var (exe, args) = Resolve(exeName);
            var psi = new ProcessStartInfo { FileName = exe, UseShellExecute = false };
            foreach (var a in args) psi.ArgumentList.Add(a);
            foreach (var a in extraArgs) psi.ArgumentList.Add(a);
            Process.Start(psi);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
