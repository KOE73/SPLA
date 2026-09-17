using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidTypeTextTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_type_text";
    protected override string Description => "Types text; Unicode uses scrcpy clipboard paste and overwrites the phone clipboard.";
    protected override string Fields => "text:s:Text to type; required.|submit:b:Default false; press Enter afterwards.|screenshot:b:Default true.";
    protected override ToolEffect Effect => ToolEffect.Write;
    protected override ToolRisk Risk => ToolRisk.Medium;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var result = await session.Backend.TypeTextAsync(Required(args,"text"),ct);
        if (ToolJson.GetBoolean(args,"submit",false)) await session.Backend.KeyAsync(66,false,ct);
        return await AfterAction(session,args,$"Text entered via {result.Method}." + (result.ClipboardOverwritten ? " The phone clipboard was overwritten." : ""),ct);
    }
}

