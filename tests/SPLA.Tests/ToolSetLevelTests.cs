using SPLA.Domain.Settings;
using SPLA.MCP.Core.ToolSets;
using System.Collections.Generic;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Levels are the unit of exposure for a tool set (PLAN_20260803_core, stage 1). Two things are
/// worth pinning: the written form of a level, and — more important — the fallback that keeps every
/// project written before this mechanism behaving exactly as it did, by deriving a level from the
/// supplier's existing on/off flag.
/// </summary>
public sealed class ToolSetLevelTests
{
    private static ToolSetRegistry RegistryWith(Dictionary<string, string> levels)
    {
        var settings = new ResolvedSettings();
        foreach (var kvp in levels) settings.ToolSets[kvp.Key] = kvp.Value;
        return new ToolSetRegistry(settings);
    }

    [Theory]
    [InlineData("disabled", ToolSetLevel.Disabled)]
    [InlineData("skill_demand", ToolSetLevel.SkillDemand)]
    [InlineData("skill-demand", ToolSetLevel.SkillDemand)]
    [InlineData("agent_demand", ToolSetLevel.AgentDemand)]
    [InlineData("Agent", ToolSetLevel.AgentDemand)]
    [InlineData("enabled", ToolSetLevel.Enabled)]
    public void Written_levels_parse(string text, ToolSetLevel expected)
    {
        Assert.True(ToolSetRegistry.TryParseLevel(text, out var level));
        Assert.Equal(expected, level);
    }

    /// <summary>YAML 1.1 reads bare on/off as booleans, so a level that only works when quoted would
    /// be a trap. They are refused outright rather than guessed at.</summary>
    [Theory]
    [InlineData("on")]
    [InlineData("off")]
    [InlineData("")]
    [InlineData("sometimes")]
    public void Ambiguous_or_unknown_words_are_refused(string text)
    {
        Assert.False(ToolSetRegistry.TryParseLevel(text, out _));
    }

    [Fact]
    public void Format_round_trips_every_level()
    {
        foreach (var level in System.Enum.GetValues<ToolSetLevel>())
        {
            Assert.True(ToolSetRegistry.TryParseLevel(ToolSetRegistry.Format(level), out var parsed));
            Assert.Equal(level, parsed);
        }
    }

    [Fact]
    public void An_explicit_entry_sets_the_level()
    {
        var registry = RegistryWith(new() { ["ssh"] = "agent_demand" });
        Assert.Equal(ToolSetLevel.AgentDemand, registry.LevelOf("ssh"));
    }

    /// <summary>An unparsable word must not silently become a different level — the set falls back to
    /// the supplier's flag, exactly as if the entry were absent.</summary>
    [Fact]
    public void An_unparsable_entry_falls_back_instead_of_guessing()
    {
        var registry = RegistryWith(new() { ["ssh"] = "mostly" });
        Assert.Equal(ToolSetLevel.Enabled, registry.LevelOf("ssh"));
    }

    /// <summary>A set nobody declared is not a set: asking about it answers with the default rather
    /// than throwing, because tool names arrive from the model and may be anything.</summary>
    [Fact]
    public void An_unknown_set_answers_with_the_default()
    {
        var registry = RegistryWith([]);
        Assert.Null(registry.Find("nothing-like-this"));
        Assert.Equal(ToolSetLevel.Enabled, registry.LevelOf("nothing-like-this"));
    }

    /// <summary>A tool no set claims is nobody's — the mechanism gates sets, not loose tools.</summary>
    [Fact]
    public void A_tool_outside_every_set_is_not_gated()
    {
        var registry = RegistryWith([]);
        Assert.Null(registry.SetOfTool("system_read_file"));
        Assert.Equal(ToolSetLevel.Enabled, registry.LevelOfTool("system_read_file"));
    }

    /// <summary>Same cascade as every other section: the project overrides the set it mentions and
    /// inherits the levels it does not.</summary>
    [Fact]
    public void Project_levels_override_defaults_entry_by_entry()
    {
        var resolved = SettingsResolver.Resolve(
            new SplaDefaults { ToolSets = new() { ["ssh"] = "agent_demand", ["network"] = "skill_demand" } },
            new SplaProject { ToolSets = new() { ["ssh"] = "enabled" } });

        Assert.Equal("enabled", resolved.ToolSets["ssh"]);
        Assert.Equal("skill_demand", resolved.ToolSets["network"]);
    }
}
