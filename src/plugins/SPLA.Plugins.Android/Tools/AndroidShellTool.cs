using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidShellTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_shell";
    protected override string Description => "Executes an arbitrary command on the Android device, with bounded output and timeout.";
    protected override string Fields => "command:s:Android shell command; required.|timeout_ms:i:Default 30000; 1..300000.";
    protected override ToolEffect Effect => ToolEffect.Execute;
    protected override ToolRisk Risk => ToolRisk.High;
    protected override ToolScope Scope => ToolScope.Shell;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var result = await adb.RunAsync(session.Serial,["shell",Required(args,"command")],TimeSpan.FromMilliseconds(Bounded(args,"timeout_ms",30000,1,300000)),ct);
        return DeviceText(session,$"Exit code: {result.ExitCode}\n{result.StdOut}\n{result.StdErr}");
    }
}
