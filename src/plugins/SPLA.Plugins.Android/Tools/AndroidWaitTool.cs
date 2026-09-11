using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidWaitTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_wait";
    protected override string Description => "Waits for stable screen content or for text in the UI tree.";
    protected override string Fields => "until:s:stable or text; required.|text:s:Required for text mode.|timeout_ms:i:Default 10000; 1..300000.";
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var until = Required(args,"until"); var timeout = Bounded(args,"timeout_ms",10000,1,300000);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(timeout);
        try
        {
            if (until == "stable") return await ScreenshotResult(session,"Wait result.",true,deadline.Token);
            if (until != "text") throw new ArgumentException("until must be stable or text.");
            var text = Required(args,"text");
            while (true)
            {
                await DumpAsync(session,adb,null,deadline.Token);
                if (session.Refs.Values.Any(element => element.Text.Contains(text,StringComparison.OrdinalIgnoreCase))) return ToolResult.Text("Text found.");
                await Task.Delay(700,deadline.Token);
            }
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return ToolResult.Fail($"Wait timed out after {timeout} ms.","wait timeout"); }
    }
}

