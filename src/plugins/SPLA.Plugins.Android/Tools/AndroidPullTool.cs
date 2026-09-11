using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidPullTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_pull";
    protected override string Description => "Copies a device file to the host.";
    protected override string Fields => "remote:s:Absolute device file path.|local:s:Host destination path, absolute or project-relative.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        return DeviceText(session,await adb.CheckedAsync(session.Serial,["pull",Remote(args),Local(settings,args,"local")],ct,300000));
    }
}

