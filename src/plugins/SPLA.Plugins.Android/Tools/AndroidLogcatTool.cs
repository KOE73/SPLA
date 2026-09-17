using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidLogcatTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_logcat";
    protected override string Description => "Reads bounded recent logcat output, optionally restricted to a running package.";
    protected override string Fields => "lines:i:Default 200; maximum 2000.|package:s:Optional running package for PID filtering.|level:s:V, D, I, W, E; default I.";
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var level = ToolJson.GetStringTrimmed(args,"level") ?? "I";
        if (level.Length != 1 || !"VDIWE".Contains(level)) throw new ArgumentException("level must be V/D/I/W/E.");
        List<string> command = ["logcat","-d","-t",$"{Bounded(args,"lines",200,1,2000)}"];
        if (ToolJson.GetStringTrimmed(args,"package") is not null)
        {
            var result = await adb.RunAsync(session.Serial,["shell","pidof",Package(args)],TimeSpan.FromSeconds(10),ct);
            var pid = result.StdOut.Trim().Split(' ',StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (pid is null) return ToolResult.Text("Package is not running; no PID-scoped logcat is available.");
            if (!int.TryParse(pid,out _)) throw new IOException("Invalid pidof response."); command.Add("--pid="+pid);
        }
        command.Add("*:"+level);
        return DeviceText(session,await adb.CheckedAsync(session.Serial,command,ct));
    }
}

