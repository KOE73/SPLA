using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidSwipeTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_swipe";
    protected override string Description => "Swipes between screenshot coordinates or from a UI ref in a direction.";
    protected override string Fields => "x1:i:Start x.|y1:i:Start y.|x2:i:End x.|y2:i:End y.|ref:s:UI ref instead of start coordinates.|direction:s:For ref: up, down, left, right.|distance:i:Ref swipe pixels; default quarter screen.|duration_ms:i:Default 300; max 60000.|screenshot:b:Default true.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var (x1,y1) = Point(session,args,"x1","y1"); int x2,y2;
        if (ToolJson.GetStringTrimmed(args,"ref") is not null)
        {
            var direction = Required(args,"direction"); var size = session.Backend.ScreenSize;
            var distance = Bounded(args,"distance", direction is "up" or "down" ? size.Height/4 : size.Width/4,1,16384);
            (x2,y2) = direction switch { "up" => (x1,y1-distance), "down" => (x1,y1+distance), "left" => (x1-distance,y1), "right" => (x1+distance,y1), _ => throw new ArgumentException("direction must be up/down/left/right.") };
        }
        else { x2 = Integer(args,"x2"); y2 = Integer(args,"y2"); }
        await session.Backend.PlayAsync(GestureBuilders.Swipe(x1,y1,x2,y2,Bounded(args,"duration_ms",300)),ct);
        return await AfterAction(session,args,"Swiped.",ct);
    }
}

