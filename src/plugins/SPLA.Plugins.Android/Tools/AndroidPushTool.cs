using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidPushTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_push";
    protected override string Description => "Copies a host file to the device.";
    protected override string Fields => "local:s:Host file path, absolute or project-relative.|remote:s:Absolute device path.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var path = Local(settings,args,"local"); if (!File.Exists(path)) throw new FileNotFoundException("Local file not found.",path);
        return DeviceText(session,await adb.CheckedAsync(session.Serial,["push",path,Remote(args)],ct,300000));
    }
}

