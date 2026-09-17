using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidDevicesTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_devices";
    protected override string Description => "Lists attached Android devices and authorization state.";
    protected override string Fields => "";
    protected override Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        return Task.FromResult(ToolResult.Text("Device list is resolved before acquiring a lease."));
    }
}

