using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Plugins.Android;
using SPLA.Plugins.Android.Runtime;

namespace SPLA.Tests;

public sealed class AndroidPluginTests
{
    [Fact]
    public void Tools_have_strict_schemas_and_honest_effects()
    {
        var tools=new AndroidPlugin().Initialize(new ResolvedSettings()).ToArray();
        Assert.Equal(20,tools.Length); Assert.Equal(20,tools.Select(t=>t.Name).Distinct().Count());
        foreach(var tool in tools)
        {
            var definition=tool.GetDefinition().Function;
            Assert.True(definition.StrictSchema); Assert.Matches("^android_[a-z_]+$",tool.Name);
            Assert.DoesNotContain(definition.Scope,new[]{ToolScope.Agent,ToolScope.Skill});
            using var schema=JsonDocument.Parse(JsonSerializer.Serialize(definition.Parameters));
            var properties=schema.RootElement.GetProperty("properties").EnumerateObject().Select(p=>p.Name).Order();
            Assert.Equal(properties,schema.RootElement.GetProperty("required").EnumerateArray().Select(p=>p.GetString()).Order());
            Assert.False(schema.RootElement.GetProperty("additionalProperties").GetBoolean());
        }
        Assert.Equal(ToolRisk.High,tools.Single(t=>t.Name=="android_install_apk").GetDefinition().Function.Risk);
        Assert.Equal(ToolScope.Shell,tools.Single(t=>t.Name=="android_shell").GetDefinition().Function.Scope);
        Assert.Equal(ToolEffect.Write,tools.Single(t=>t.Name=="android_tap").GetDefinition().Function.Effect);
    }

    [Fact]
    public async Task Status_uses_unsaved_paths_and_handles_manual_install()
    {
        var folder=Path.Combine(Path.GetTempPath(),"spla-android-status-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder,"adb.exe"),"");
            var plugin=new AndroidPlugin(); plugin.Initialize(new ResolvedSettings());
            var value=await plugin.InvokeActionAsync("runtimeStatus",JsonSerializer.Serialize(new{adb_path=folder,scrcpy_path="relative"}));
            using var result=JsonDocument.Parse(JsonSerializer.Serialize(value));
            Assert.True(result.RootElement.GetProperty("adb").GetProperty("installed").GetBoolean());
            Assert.Equal("manual",result.RootElement.GetProperty("adb").GetProperty("channel").GetString());
            Assert.Contains("relative scrcpy_path",result.RootElement.GetProperty("scrcpy").GetProperty("error").GetString());
        }
        finally { Directory.Delete(folder,true); }
    }

    [Fact]
    public void Installation_job_wire_names_match_the_settings_panel()
    {
        using var document=JsonDocument.Parse(JsonSerializer.Serialize(new InstallStatus("adb","folder","downloading",123,456,null)));
        Assert.Equal("downloading",document.RootElement.GetProperty("stage").GetString());
        Assert.Equal(123,document.RootElement.GetProperty("bytesDone").GetInt64());
        Assert.Equal(456,document.RootElement.GetProperty("bytesTotal").GetInt64());
    }
}
