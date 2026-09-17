using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidTapTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_tap";
    protected override string Description => "Taps or holds a point or UI reference; optionally double-taps.";
    protected override string Fields => "x:i:Screenshot x coordinate.|y:i:Screenshot y coordinate.|ref:s:Latest UI ref instead of x/y.|hold_ms:i:Default 0, max 60000 ms.|count:i:1 or 2; default 1.|screenshot:b:Default true.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var (x,y) = Point(session, args, "x", "y"); var hold = Bounded(args, "hold_ms", 0, 0);
        var gesture = hold == 0 ? GestureBuilders.Tap(x,y) : GestureBuilders.LongPress(x,y,hold);
        for (var index = 0; index < Bounded(args,"count",1,1,2); index++)
        { if (index > 0) await Task.Delay(100,ct); await session.Backend.PlayAsync(gesture,ct); }
        return await AfterAction(session,args,"Tapped.",ct);
    }
}

