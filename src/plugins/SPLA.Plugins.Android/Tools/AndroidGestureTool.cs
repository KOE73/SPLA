using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidGestureTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_gesture";
    protected override string Description => "Plays a timed multi-finger gesture; scrcpy supports up to ten fingers.";
    protected override string Fields => "pointers:a:Fingers with path arrays of [x,y,t_ms]; ascending times, max 60000 ms.|screenshot:b:Default true.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        if (!args.TryGetProperty("pointers",out var pointers) || pointers.ValueKind != JsonValueKind.Array) throw new ArgumentException("pointers is required.");
        List<GesturePointer> paths = [];
        foreach (var pointer in pointers.EnumerateArray())
        {
            List<GesturePoint> points = [];
            foreach (var value in pointer.GetProperty("path").EnumerateArray())
            {
                var point = value.EnumerateArray().Select(p => p.GetInt32()).ToArray();
                if (point.Length != 3) throw new ArgumentException("Points must be [x,y,t_ms].");
                points.Add(new(point[0],point[1],point[2]));
            }
            paths.Add(new(points));
        }
        await session.Backend.PlayAsync(new(paths),ct);
        return await AfterAction(session,args,"Gesture played.",ct);
    }
}

