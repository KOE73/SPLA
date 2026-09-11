using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidKeyTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_key";
    protected override string Description => "Presses an Android key by name or integer keycode.";
    protected override string Fields => "key:s:home, back, enter, del, tab, escape, app_switch, power, volume_up/down, dpad_up/down/left/right/center, wakeup, sleep.|keycode:i:Alternative Android keycode, 0..1000.|long_press:b:Default false.|screenshot:b:Default true.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var code = ToolJson.GetInt32(args,"keycode") ?? KeyCode(Required(args,"key"));
        if (code is < 0 or > 1000) throw new ArgumentException("keycode must be 0..1000.");
        await session.Backend.KeyAsync(code,ToolJson.GetBoolean(args,"long_press",false),ct);
        return await AfterAction(session,args,"Key pressed.",ct);
    }

    internal static int KeyCode(string name) => name.ToLowerInvariant() switch
    {
        "home"=>3,"back"=>4,"call"=>5,"endcall"=>6,"dpad_up"=>19,"dpad_down"=>20,"dpad_left"=>21,"dpad_right"=>22,"dpad_center"=>23,
        "volume_up"=>24,"volume_down"=>25,"power"=>26,"camera"=>27,"tab"=>61,"space"=>62,"enter"=>66,"del"=>67,"menu"=>82,
        "search"=>84,"media_play_pause"=>85,"page_up"=>92,"page_down"=>93,"escape"=>111,"forward_del"=>112,"move_home"=>122,
        "move_end"=>123,"app_switch"=>187,"wakeup"=>224,"sleep"=>223,
        _ => throw new ArgumentException("Unknown key name. Use an integer keycode.")
    };
}

