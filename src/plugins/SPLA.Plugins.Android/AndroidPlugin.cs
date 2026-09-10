using System.Collections.Generic;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Android;

/// <summary>
/// Android device control: live screen for the model, touch (including multi-touch), APK install.
/// Wave 0 (PLAN_20260910_plugins_android-device.md) registers the plugin with no tools yet —
/// the adb-backend tools arrive in wave 2, the scrcpy stream in wave 3.
/// </summary>
public sealed class AndroidPlugin : ISplaPlugin
{
    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings) => [];
}
