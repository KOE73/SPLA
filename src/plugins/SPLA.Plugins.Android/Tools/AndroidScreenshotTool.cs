using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidScreenshotTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_screenshot";
    protected override string Description => "Returns the device screen as an image with its coordinate space.";
    protected override string Fields => "wait_stable:b:Default true. Wait for stable content; ADB only pauses.";
    protected override Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        return ScreenshotResult(session, "Screenshot.", ToolJson.GetBoolean(args, "wait_stable", true), ct);
    }
}

