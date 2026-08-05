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
    public void An_untrusted_sources_skill_stays_off_and_no_switch_here_lifts_it()
    {
        var manager = Manager(new FakeSkillSource("server", SkillTrust.Untrusted).With("imported.thing"));

        var skill = manager.Find("imported.thing")!;
        Assert.Equal(SkillState.DisabledByTrust, skill.State);
        Assert.False(skill.IsEnabled);

        // This used to be the way past it. It is not any more: trust belongs to the source, and one
        // arbitrarily trusted book out of an untrusted branch is a contradiction. The grant — kept
        // where the source cannot write it — is the only way up, and copying the skill into a branch
        // you already trust is the other. See SkillTrustGrantTests.
        manager.ApplySettings(new Dictionary<string, SplaSkillSection>
        {
            ["imported.thing"] = new() { Enabled = true }
        });
        Assert.Equal(SkillState.DisabledByTrust, manager.Find("imported.thing")!.State);
    }

    [Fact]
    public void Two_branches_holding_the_same_name_both_keep_their_book()
    {
        var project = new FakeSkillSource("project").With("net.audit", body: "project version");
        var plugin = new FakeSkillSource("plugin:network").With("net.audit", body: "plugin version");

        var manager = new SkillLibrary([project, plugin]);

        // Neither is evicted, and neither is demoted: two editions of one title is a normal state of
        // a fond. The reader picks by address.
        Assert.Equal(2, manager.Holdings().Count);
        Assert.All(manager.Holdings(), skill => Assert.Equal(SkillState.Available, skill.State));

        Assert.Equal("project version", manager.LoadBody("project:net.audit"));
        Assert.Equal("plugin version", manager.LoadBody("plugin:network:net.audit"));
    }

    [Fact]
    public void A_name_two_branches_answer_to_is_an_error_that_names_the_candidates()
    {
        var project = new FakeSkillSource("project").With("net.audit");
        var plugin = new FakeSkillSource("plugin:network").With("net.audit");

        var manager = new SkillLibrary([project, plugin]);

        var lookup = manager.Resolve("net.audit");

        Assert.True(lookup.IsAmbiguous);
        Assert.Null(lookup.Card);
        Assert.Equal(["project:net.audit", "plugin:network:net.audit"],
            lookup.Candidates.Select(c => c.Address));
    }

    [Fact]
    public void A_shared_name_is_printed_as_an_address_and_a_unique_one_stays_short()
    {
        var project = new FakeSkillSource("project").With("net.audit").With("only.here");
        var plugin = new FakeSkillSource("plugin:network").With("net.audit");

        var manager = new SkillLibrary([project, plugin]);

        Assert.Equal("only.here", manager.Find("only.here")!.DisplayId);
        Assert.All(manager.Holdings().Where(s => s.Id == "net.audit"),
            skill => Assert.Equal(skill.Address, skill.DisplayId));
    }

    [Fact]
    public void One_available_edition_settles_the_name_without_asking()
    {
        var project = new FakeSkillSource("project").With("net.audit");
        var plugin = new FakeSkillSource("plugin:network").With("net.audit");

        var manager = new SkillLibrary([project, plugin]);
        // A full address switches off exactly one edition; the bare name then means the other.
        manager.ApplySettings(new Dictionary<string, SplaSkillSection>
        {
            ["plugin:network:net.audit"] = new() { Enabled = false }
        });

        var lookup = manager.Resolve("net.audit");

        Assert.False(lookup.IsAmbiguous);
        Assert.Equal("project", lookup.Card!.SourceId);
    }

    [Fact]
    public void A_bare_item_key_is_a_predicate_and_reaches_every_edition()
    {
        var project = new FakeSkillSource("project").With("net.audit");
        var plugin = new FakeSkillSource("plugin:network").With("net.audit");

        var manager = new SkillLibrary([project, plugin]);
        manager.ApplySettings(new Dictionary<string, SplaSkillSection>
        {
            ["net.audit"] = new() { Enabled = false }
        });

        Assert.All(manager.Holdings(),
            skill => Assert.Equal(SkillState.DisabledByUser, skill.State));
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
    public void A_deferred_rebuild_lands_when_the_book_is_returned()
    {
        var source = new FakeSkillSource().With("a");
        var manager = new SkillLibrary([source]) { IsSkillActive = () => true };

        source.With("b");
        source.Raise();
        Assert.Single(manager.Holdings());

        // Nothing else is scheduled to make it appear. Skipping was survivable while only file
        // changes triggered this — another save would come along — and stops being survivable once
        // the source LIST can change: a folder added mid-procedure would never show up at all.
        manager.IsSkillActive = () => false;
        manager.ApplyDeferredRebuild();
        Assert.Equal(2, manager.Holdings().Count);
    }

    [Fact]
    public void Replacing_the_branches_disposes_the_old_ones_exactly_once()
    {
        var first = new FakeSkillSource("first").With("a");
        var manager = new SkillLibrary([first]);

        manager.SetSources([new FakeSkillSource("second").With("b")]);

        Assert.Equal(["second"], manager.Sources.Select(s => s.Id));
        Assert.Equal(["b"], manager.Holdings().Select(s => s.Id));

        // A watcher nobody reads is a handle nobody closes.
        Assert.Equal(1, first.DisposeCount);

        // And it is unsubscribed: a source that is gone must not still be able to trigger rebuilds.
        first.With("c");
        first.Raise();
        Assert.Equal(["b"], manager.Holdings().Select(s => s.Id));
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
