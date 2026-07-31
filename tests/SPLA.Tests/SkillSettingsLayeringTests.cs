using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>The two halves of the <c>skills:</c> block layer differently on purpose — sources are
/// replaced, items are merged. Getting this backwards would either make an inherited source
/// impossible to drop or force every project to restate every switch.</summary>
public class SkillSettingsLayeringTests
{
    private static SplaSkillsSection Sources(params string[] paths) => new()
    {
        Sources = paths.Select(p => new SplaSkillSourceSection { Type = "directory", Path = p }).ToList()
    };

    [Fact]
    public void No_skills_block_anywhere_leaves_sources_at_the_built_in_default()
    {
        var resolved = SettingsResolver.Resolve(new SplaDefaults(), new SplaProject());

        Assert.Null(resolved.SkillSources);
        Assert.Empty(resolved.Skills);
    }

    [Fact]
    public void Project_sources_replace_the_machine_list_rather_than_extending_it()
    {
        var defaults = new SplaDefaults { Skills = Sources("~/skills", "/shared/skills") };
        var project = new SplaProject { Skills = Sources(".spla/skills") };

        var resolved = SettingsResolver.Resolve(defaults, project);

        Assert.Equal([".spla/skills"], resolved.SkillSources!.Select(s => s.Path));
    }

    [Fact]
    public void A_project_that_omits_sources_keeps_the_inherited_ones()
    {
        var defaults = new SplaDefaults { Skills = Sources("~/skills") };
        var project = new SplaProject
        {
            Skills = new SplaSkillsSection
            {
                Items = new Dictionary<string, SplaSkillSection> { ["a"] = new() { Enabled = false } }
            }
        };

        var resolved = SettingsResolver.Resolve(defaults, project);

        Assert.Equal(["~/skills"], resolved.SkillSources!.Select(s => s.Path));
        Assert.False(resolved.Skills["a"].Enabled);
    }

    [Fact]
    public void Items_merge_by_id_with_the_project_winning()
    {
        var defaults = new SplaDefaults
        {
            Skills = new SplaSkillsSection
            {
                Items = new Dictionary<string, SplaSkillSection>
                {
                    ["a"] = new() { Enabled = false },
                    ["b"] = new() { Enabled = false }
                }
            }
        };
        var project = new SplaProject
        {
            Skills = new SplaSkillsSection
            {
                Items = new Dictionary<string, SplaSkillSection> { ["b"] = new() { Enabled = true } }
            }
        };

        var resolved = SettingsResolver.Resolve(defaults, project);

        Assert.False(resolved.Skills["a"].Enabled);
        Assert.True(resolved.Skills["b"].Enabled);
    }

    [Fact]
    public void The_skills_block_round_trips_through_yaml()
    {
        var yaml = """
            version: 1
            skills:
              sources:
                - type: directory
                  path: .spla/skills
                - type: directory
                  path: /srv/shared-skills
                  trust: untrusted
                  label: Shared
              items:
                network.host-audit:
                  enabled: false
            """;

        var project = ConfigLoader.ParseProjectYaml(yaml);

        Assert.Equal(2, project.Skills!.Sources!.Count);
        Assert.Equal("directory", project.Skills.Sources[0].Type);
        Assert.Equal(".spla/skills", project.Skills.Sources[0].Path);
        Assert.Equal("untrusted", project.Skills.Sources[1].Trust);
        Assert.Equal("Shared", project.Skills.Sources[1].Label);
        Assert.False(project.Skills.Items!["network.host-audit"].Enabled);
    }
}
