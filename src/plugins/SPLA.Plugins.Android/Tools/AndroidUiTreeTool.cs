using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidUiTreeTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_ui_tree";
    protected override string Description => "Lists visible UI elements and refs for precise taps or swipes.";
    protected override string Fields => "filter:s:Optional case-insensitive text filter.";
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        return DeviceText(session, await DumpAsync(session, adb, ToolJson.GetStringTrimmed(args, "filter"), ct));
    }
}

