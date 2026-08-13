using SPLA.Domain.Settings;

namespace SPLA.Tests;

/// <summary>
/// Both halves of the <c>skills:</c> block now layer the same way — by key. Sources used to be
/// replaced wholesale, and these tests used to prove it; the replacement was not a decision but the
/// only expressible behaviour for entries that had no name, and it made "add one folder" mean
/// "restate the whole list".
///
/// <para>The merge itself lives in <c>SkillSourceRegistry</c>, because resolving an unnamed entry's
/// fallback id needs paths. What the resolver owes is accumulation in layer order plus the two
/// scalars — that is what is checked here; <c>SkillSourceMergeTests</c> checks the fold.</para>
/// </summary>
public class SkillSettingsLayeringTests
{
    private static SplaSkillsSection Sources(params string[] paths) => new()
    {
        Sources = paths.Select(p => new SplaSkillSourceSection { Type = "directory", Path = p }).ToList()
    };

    [Fact]
    public void No_skills_block_anywhere_declares_nothing_and_still_inherits_the_built_ins()
    {
        var resolved = SettingsResolver.Resolve(new SplaDefaults(), new SplaProject());

        Assert.Empty(resolved.SkillSources);
        Assert.True(resolved.SkillsInheritDefaults);
        Assert.Empty(resolved.Skills);
    }

    [Fact]
    public void Project_sources_extend_the_machine_list_rather_than_replacing_it()
    {
        var defaults = new SplaDefaults { Skills = Sources("~/skills", "/shared/skills") };
        var project = new SplaProject { Skills = Sources(".spla/skills") };

        var resolved = SettingsResolver.Resolve(defaults, project);

        // Layer order, machine first: the fold downstream keys on the id, not on the position.
        Assert.Equal(["~/skills", "/shared/skills", ".spla/skills"],
            resolved.SkillSources.Select(s => s.Path));
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

        Assert.Equal(["~/skills"], resolved.SkillSources.Select(s => s.Path));
        Assert.False(resolved.Skills["a"].Enabled);
    }

    [Fact]
    public void Inherit_defaults_is_a_scalar_and_the_last_layer_to_mention_it_wins()
    {
        var defaults = new SplaDefaults { Skills = new SplaSkillsSection { InheritDefaults = false } };

        // A project that says nothing leaves the machine's white list alone...
        Assert.False(SettingsResolver.Resolve(defaults, new SplaProject()).SkillsInheritDefaults);

        // ...and a project that says something means it.
        var project = new SplaProject { Skills = new SplaSkillsSection { InheritDefaults = true } };
        Assert.True(SettingsResolver.Resolve(defaults, project).SkillsInheritDefaults);
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
              inherit_defaults: false
              sources:
                - id: ops
                  type: directory
                  path: /srv/shared-skills
                  trust: untrusted
                  label: Shared
                - id: repo
                  enabled: false
              items:
                network.host-audit:
                  enabled: false
            """;

        var project = ConfigLoader.ParseProjectYaml(yaml);

        Assert.False(project.Skills!.InheritDefaults);
        Assert.Equal(2, project.Skills.Sources!.Count);

        Assert.Equal("ops", project.Skills.Sources[0].Id);
        Assert.Equal("/srv/shared-skills", project.Skills.Sources[0].Path);
        Assert.Equal("untrusted", project.Skills.Sources[0].Trust);
        Assert.Equal("Shared", project.Skills.Sources[0].Label);

        // Switching an inherited branch off is a complete statement on its own: no type, no path.
        Assert.Equal("repo", project.Skills.Sources[1].Id);
        Assert.False(project.Skills.Sources[1].Enabled);
        Assert.Null(project.Skills.Sources[1].Path);

        Assert.False(project.Skills.Items!["network.host-audit"].Enabled);
    }
}
