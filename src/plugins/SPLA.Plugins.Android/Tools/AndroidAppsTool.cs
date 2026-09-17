using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidAppsTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_apps";
    protected override string Description => "Lists installed packages, optionally including system apps.";
    protected override string Fields => "all:b:Default false: user apps only.|filter:s:Optional package substring.";
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        List<string> command = ["shell","pm","list","packages"]; if (!ToolJson.GetBoolean(args,"all",false)) command.Add("-3");
        var text = await adb.CheckedAsync(session.Serial,command,ct);
        if (ToolJson.GetStringTrimmed(args,"filter") is { } filter) text = string.Join('\n',text.Split('\n').Where(line => line.Contains(filter,StringComparison.OrdinalIgnoreCase)));
        return DeviceText(session,text);
    }
}

