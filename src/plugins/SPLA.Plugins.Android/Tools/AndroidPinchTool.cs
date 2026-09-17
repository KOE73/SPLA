using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidPinchTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_pinch";
    protected override string Description => "Performs a two-finger pinch or zoom; requires scrcpy.";
    protected override string Fields => "x:i:Center x.|y:i:Center y.|from_distance:i:Initial finger separation in pixels.|to_distance:i:Final separation.|angle_deg:n:Axis angle; default 0.|duration_ms:i:Default 400; max 60000.|screenshot:b:Default true.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var angle = args.TryGetProperty("angle_deg",out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : 0;
        await session.Backend.PlayAsync(GestureBuilders.Pinch(Integer(args,"x"),Integer(args,"y"),Integer(args,"from_distance"),Integer(args,"to_distance"),angle,Bounded(args,"duration_ms",400)),ct);
        return await AfterAction(session,args,"Pinched.",ct);
    }
}

