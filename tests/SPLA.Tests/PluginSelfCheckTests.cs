using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Roslyn;

namespace SPLA.Tests;

/// <summary>The generic plugin self-check contract, and the Roslyn plugin's use of it. A healthy
/// scripting runtime must report <see cref="PluginHealthStatus.Ok"/> — this is the load-time guard
/// that would have caught the earlier roslyn_script_run ALC regression before a model ever called it.</summary>
public class PluginSelfCheckTests
{
    [Fact]
    public void Roslyn_self_check_is_ok_when_scripting_runs()
    {
        var health = new RoslynPlugin().CheckHealth();
        Assert.Equal(PluginHealthStatus.Ok, health.Status);
        Assert.Null(health.Message);
    }

    [Fact]
    public void PluginHealth_degraded_carries_a_reason()
    {
        var h = PluginHealth.Degraded("native dependency missing");
        Assert.Equal(PluginHealthStatus.Degraded, h.Status);
        Assert.Equal("native dependency missing", h.Message);
    }

    /// <summary>Documents the contract the host relies on: a plugin should return Degraded rather than
    /// throw. (The host also guards the call, converting a throw into Degraded — see PluginAssemblyLoader.)</summary>
    [Fact]
    public void A_selfcheck_can_report_degraded_without_throwing()
    {
        ISplaPluginSelfCheck check = new FlakyCheck();
        var health = check.CheckHealth();
        Assert.Equal(PluginHealthStatus.Degraded, health.Status);
        Assert.Contains("offline", health.Message);
    }

    private sealed class FlakyCheck : ISplaPluginSelfCheck
    {
        public PluginHealth CheckHealth() => PluginHealth.Degraded("backend offline");
    }
}
