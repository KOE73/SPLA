using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidConnectTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_connect";
    protected override string Description => "Leases a device for this chat, optionally connecting an already paired TCP address.";
    protected override string Fields => "address:s:Optional host:port; does not perform pairing.|take_over:b:Default false. Explicitly take the device from another chat.";
    protected override Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        return Task.FromResult(ToolResult.Text($"Connected {session.Serial} via {session.Backend.Kind}, {session.Backend.ScreenSize.Width}x{session.Backend.ScreenSize.Height}." + (session.FallbackReason is null ? "" : $" Fallback: {session.FallbackReason}")));
    }
}

