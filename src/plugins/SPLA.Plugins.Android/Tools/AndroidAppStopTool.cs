using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidAppStopTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_app_stop";
    protected override string Description => "Force-stops an Android app.";
    protected override string Fields => "package:s:Android package name; required.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        await adb.CheckedAsync(session.Serial,["shell","am","force-stop",Package(args)],ct);
        return ToolResult.Text("App stopped.");
    }
}

