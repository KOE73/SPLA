namespace SPLA.Plugins.Android.Runtime;

/// <summary>
/// Where the android runtime components live (ADR §3.6). The only place that resolves
/// adb/scrcpy folders — settings path → project-relative → the shared user default. Never
/// PATH, ANDROID_HOME, or the global environment (AGENTS.md invariant).
/// </summary>
internal static class RuntimePaths
{
    /// <summary>
    /// Returns the absolute folder of a component, or an error text.
    /// Empty/whitespace <paramref name="configured"/> → <see cref="DefaultFolder"/>; a rooted
    /// path → as-is; a relative path → against the project file's directory, and a configuration
    /// error (visible in the status) when there is no project file.
    /// </summary>
    public static (string? Folder, string? Error) Resolve(string component, string? configured, string? projectFilePath)
    {
        if (string.IsNullOrWhiteSpace(configured)) return (DefaultFolder(component), null);
        if (Path.IsPathRooted(configured)) return (configured, null);
        if (projectFilePath is not null)
            return (Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectFilePath)!, configured)), null);
        return (null, "relative adb_path needs an open project");
    }

    /// <summary>The shared per-user default: %LOCALAPPDATA%\SPLA\runtime\android\&lt;component&gt;.</summary>
    public static string DefaultFolder(string component) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SPLA", "runtime", "android", component);

    /// <summary>&lt;folder&gt;\adb.exe.</summary>
    public static string AdbExe(string adbFolder) => Path.Combine(adbFolder, "adb.exe");

    /// <summary>&lt;folder&gt;\scrcpy-server, or null when the file is missing.</summary>
    public static string? ScrcpyServer(string scrcpyFolder)
    {
        var serverPath = Path.Combine(scrcpyFolder, "scrcpy-server");
        return File.Exists(serverPath) ? serverPath : null;
    }
}
