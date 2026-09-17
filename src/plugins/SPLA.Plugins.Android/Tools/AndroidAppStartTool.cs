using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidAppStartTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_app_start";
    protected override string Description => "Starts an app through its launcher activity.";
    protected override string Fields => "package:s:Android package name; required.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        return DeviceText(session,await adb.CheckedAsync(session.Serial,["shell","monkey","-p",Package(args),"-c","android.intent.category.LAUNCHER","1"],ct));
    }
}

