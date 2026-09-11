using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Json;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Session;

namespace SPLA.Plugins.Android.Tools;

internal sealed class AndroidInstallApkTool(ResolvedSettings settings) : AndroidToolBase(settings)
{
    public override string Name => "android_install_apk";
    protected override string Description => "Installs or replaces an APK from the host; timeout five minutes.";
    protected override string Fields => "path:s:APK host path, absolute or project-relative.|grant_permissions:b:Default false; grant app runtime permissions.";
    protected override ToolEffect Effect => ToolEffect.Execute;
    protected override ToolRisk Risk => ToolRisk.High;
    protected override async Task<ToolResult> RunAsync(DeviceSession session, AdbRunner adb, ResolvedSettings settings, JsonElement args, CancellationToken ct)
    {
        var path = Local(settings,args,"path"); if (!File.Exists(path)) throw new FileNotFoundException("APK not found.",path);
        List<string> command = ["install","-r"]; if (ToolJson.GetBoolean(args,"grant_permissions",false)) command.Add("-g"); command.Add(path);
        var text = await adb.CheckedAsync(session.Serial,command,ct,300000);
        if (!text.Split('\n',StringSplitOptions.TrimEntries).Contains("Success")) throw new IOException(text);
        return DeviceText(session,text);
    }
}

