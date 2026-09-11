using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Runtime;
using SPLA.Plugins.Android.Tools;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace SPLA.Plugins.Android;

/// <summary>
/// Android device control: live screen for the model, touch (including multi-touch), APK install.
/// Owns runtime installation actions and registers tools over per-chat device leases.
/// </summary>
public sealed class AndroidPlugin : ISplaPlugin, ISplaPluginAction, ISplaPluginSelfCheck
{
    private ResolvedSettings? resolved;
    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        resolved = settings;
        return AndroidToolBase.CreateTools(settings);
    }
    internal static AndroidSettings Settings(ResolvedSettings settings)
    {
        settings.Plugins.TryGetValue("android", out var section);
        return AndroidSettings.FromBlob(section?.Settings);
    }
    internal static bool Supported => OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X64;
    internal object ComponentStatus(string component, string? path)
    {
        var (folder, error) = RuntimePaths.Resolve(component, path, resolved?.ProjectFilePath);
        var receipt = folder is null ? null : RuntimeReceipt.TryRead(folder);
        if (receipt?.Component != component) receipt = null;
        receipt ??= folder is null ? null : RuntimeReceipt.TryManualInstall(component, folder);
        var installed = folder is not null && File.Exists(RuntimeReceipt.MarkerPath(component, folder));
        if (component == "scrcpy" && installed) installed = Directory.EnumerateFiles(folder!, "avcodec-*.dll").Any() && Directory.EnumerateFiles(folder!, "avutil-*.dll").Any();
        return new { folder, defaultFolder = RuntimePaths.DefaultFolder(component), installed, version = receipt?.Version ?? "unknown",
            channel = receipt?.Channel ?? "manual", verified = receipt?.Verified ?? false,
            experimental = component == "scrcpy" && installed && receipt?.Version != RuntimeManifest.Load().Scrcpy!.Version,
            error = Supported ? error : "Android runtime requires Windows x64.", job = folder is null ? null : RuntimeInstaller.Instance.Get(folder) };
    }
    public Task<object?> InvokeActionAsync(string action, string? valueJson, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(valueJson) ? "{}" : valueJson);
        var root = document.RootElement;
        var settings = resolved is null ? new AndroidSettings() : Settings(resolved);
        var adbPath = ToolJson.GetString(root, "adb_path") ?? settings.AdbPath;
        var scrcpyPath = ToolJson.GetString(root, "scrcpy_path") ?? settings.ScrcpyPath;
        if (action == "runtimeStatus")
        {
            var manifest = RuntimeManifest.Load();
            return Task.FromResult<object?>(new { adb = ComponentStatus("adb", adbPath), scrcpy = ComponentStatus("scrcpy", scrcpyPath),
                pinned = new { adb = manifest.Adb!.Version, scrcpy = manifest.Scrcpy!.Version } });
        }
        if (action != "install") throw new ArgumentException($"Unknown Android action '{action}'.");
        if (!Supported) throw new PlatformNotSupportedException("Android runtime requires Windows x64.");
        var component = ToolJson.GetStringTrimmed(root, "component") ?? throw new ArgumentException("component is required.");
        var (folder, error) = RuntimePaths.Resolve(component, component == "adb" ? adbPath : scrcpyPath, resolved?.ProjectFilePath);
        if (error is not null) throw new ArgumentException(error);
        return Task.FromResult<object?>(RuntimeInstaller.Instance.Start(component, ToolJson.GetStringTrimmed(root, "channel") ?? "pinned", folder!,
            ToolJson.GetInt32Clamped(root, "adb_server_port", settings.AdbServerPort, 0, 65535)));
    }
    public PluginHealth CheckHealth()
    {
        try
        {
            if (!Supported) return PluginHealth.Degraded("Android runtime requires Windows x64.");
            var settings = resolved is null ? new AndroidSettings() : Settings(resolved);
            var (adb, adbError) = RuntimePaths.Resolve("adb", settings.AdbPath, resolved?.ProjectFilePath);
            var (scrcpy, scrcpyError) = RuntimePaths.Resolve("scrcpy", settings.ScrcpyPath, resolved?.ProjectFilePath);
            if (adbError is not null || scrcpyError is not null) return PluginHealth.Degraded(adbError ?? scrcpyError!);
            if (!File.Exists(RuntimePaths.AdbExe(adb!))) return PluginHealth.Degraded("adb is not installed — open the Android plugin page and press Install.");
            if (RuntimePaths.ScrcpyServer(scrcpy!) is null || !Directory.EnumerateFiles(scrcpy!, "avcodec-*.dll").Any() || !Directory.EnumerateFiles(scrcpy!, "avutil-*.dll").Any())
                return PluginHealth.Degraded("scrcpy is not installed — screen will use slow screencap and single-finger input.");
            var version = RuntimeReceipt.TryRead(scrcpy!)?.Version;
            var pinned = RuntimeManifest.Load().Scrcpy!.Version;
            return version == pinned ? PluginHealth.Ok : PluginHealth.Degraded($"scrcpy {version ?? "unknown"} is not the tested version {pinned}.");
        }
        catch (Exception ex) { return PluginHealth.Degraded(ex.Message); }
    }
}
