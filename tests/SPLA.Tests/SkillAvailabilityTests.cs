using SPLA.Domain.Settings;
using SPLA.Library;
using SPLA.Library.Catalog;
using SPLA.Library.Sources;
using SPLA.Tests.Fakes;

namespace SPLA.Tests;

/// <summary>How a skill's declared requirements, the user's switches and the source's trust combine
/// into one state — the decision that determines what the model is told about.</summary>
public class SkillAvailabilityTests
{
    private static SkillLibrary Manager(FakeSkillSource source, ISkillCapabilityProbe? probe = null)
    {
        var manager = new SkillLibrary([source]);
        if (probe != null) manager.SetProbe(probe);
        return manager;
    }

    [Fact]
    public void A_skill_with_no_requirements_is_available()
    {
        var manager = Manager(new FakeSkillSource().With("my.release-notes"),
            new SkillCapabilityProbe(tools: [], features: []));

        Assert.Equal(SkillState.Available, manager.Find("my.release-notes")!.State);
    }

    [Fact]
    public void A_missing_required_tool_makes_the_skill_unavailable_with_the_names_in_the_reason()
    {
        var manager = Manager(
            new FakeSkillSource().With("network.host-audit", requiresTools: ["dns_lookup", "port_scan"]),
            new SkillCapabilityProbe(
                tools: ["dns_lookup"], features: null,
                toolOwners: new Dictionary<string, string> { ["port_scan"] = "network" }));

        var skill = manager.Find("network.host-audit")!;

        Assert.Equal(SkillState.MissingPrerequisites, skill.State);
        Assert.Equal(["port_scan"], skill.MissingTools);
        Assert.Equal(["network"], skill.MissingPlugins);
        Assert.Contains("port_scan", skill.StateReason);
        Assert.Contains("network", skill.StateReason);
        Assert.Empty(manager.Catalog());
    }

    [Fact]
    public void A_missing_required_feature_makes_the_skill_unavailable()
    {
        var manager = Manager(
            new FakeSkillSource().With("uses.memory", requiresFeatures: ["core.memory"]),
            new SkillCapabilityProbe(tools: null, features: ["core.workspace"]));

        Assert.Equal(SkillState.MissingPrerequisites, manager.Find("uses.memory")!.State);
        Assert.Equal(["core.memory"], manager.Find("uses.memory")!.MissingFeatures);
    }

    [Fact]
    public void Uses_does_not_gate_availability()
    {
        var source = new FakeSkillSource();
        source.With("soft", requiresTools: []);

        var manager = Manager(source, new SkillCapabilityProbe(tools: [], features: []));

        Assert.Equal(SkillState.Available, manager.Find("soft")!.State);
    }

    [Fact]
    public void Settings_can_switch_a_skill_off()
    {
        var manager = Manager(new FakeSkillSource().With("net.audit"));
        manager.ApplySettings(new Dictionary<string, SplaSkillSection>
        {
            ["net.audit"] = new() { Enabled = false }
        });

        var skill = manager.Find("net.audit")!;
        Assert.Equal(SkillState.DisabledByUser, skill.State);
        Assert.Contains("settings", skill.StateReason);
    }

    [Fact]
    public void A_skill_can_switch_itself_off_and_settings_can_switch_it_back_on()
    {
        var manager = Manager(new FakeSkillSource().With("draft", enabled: false));
        Assert.Equal(SkillState.DisabledByUser, manager.Find("draft")!.State);

        manager.ApplySettings(new Dictionary<string, SplaSkillSection>
        {
            ["draft"] = new() { Enabled = true }
        });
        Assert.Equal(SkillState.Available, manager.Find("draft")!.State);
    }

    [Fact]
    public void An_untrusted_sources_skill_stays_off_until_it_is_enabled_by_name()
    {
        var manager = Manager(new FakeSkillSource("server", SkillTrust.Untrusted).With("imported.thing"));

        var skill = manager.Find("imported.thing")!;
        Assert.Equal(SkillState.DisabledByTrust, skill.State);
        Assert.False(skill.IsEnabled);

        manager.ApplySettings(new Dictionary<string, SplaSkillSection>
        {
            ["imported.thing"] = new() { Enabled = true }
        });
        Assert.Equal(SkillState.Available, manager.Find("imported.thing")!.State);
    }

    [Fact]
    public void The_first_source_wins_and_the_loser_is_marked_shadowed()
    {
        var project = new FakeSkillSource("project").With("net.audit", body: "project version");
        var plugin = new FakeSkillSource("plugin:network").With("net.audit", body: "plugin version");

        var manager = new SkillLibrary([project, plugin]);

        Assert.Equal("project", manager.Find("net.audit")!.SourceId);
        Assert.Equal("project version", manager.LoadBody("net.audit"));

        var superseded = Assert.Single(manager.Holdings(), skill => skill.State == SkillState.Superseded);
        Assert.Equal("plugin:network", superseded.SourceId);
        Assert.Contains("overridden by source 'project'", superseded.StateReason);
    }

    [Fact]
    public void A_source_change_rebuilds_the_list()
    {
        var source = new FakeSkillSource().With("a");
        var manager = new SkillLibrary([source]);
        Assert.Single(manager.Holdings());

        source.With("b");
        source.Raise();

        Assert.Equal(2, manager.Holdings().Count);
    }

    [Fact]
    public void A_reload_is_deferred_while_a_skill_is_running()
    {
        var source = new FakeSkillSource().With("a");
        var manager = new SkillLibrary([source]) { IsSkillActive = () => true };

        source.With("b");
        source.Raise();
        Assert.Single(manager.Holdings());

        manager.IsSkillActive = () => false;
        source.Raise();
        Assert.Equal(2, manager.Holdings().Count);
    }

    [Fact]
    public void Unavailable_skills_stay_listed_so_the_panel_can_explain_them()
    {
        var manager = Manager(
            new FakeSkillSource().With("net.audit", requiresTools: ["port_scan"]),
            new SkillCapabilityProbe(tools: [], features: []));

        Assert.Single(manager.Holdings());
        Assert.Empty(manager.Catalog());
    }
}
