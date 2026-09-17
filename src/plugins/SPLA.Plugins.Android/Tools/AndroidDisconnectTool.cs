using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidDisconnectTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_disconnect";
    protected override string Description => "Releases this chat's device lease.";
    protected override string Fields => "";
    protected override Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        return Task.FromResult(ToolResult.Text("Lease release is handled before device acquisition."));
    }
}

