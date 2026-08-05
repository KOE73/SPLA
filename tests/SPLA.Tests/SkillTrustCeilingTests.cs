using SPLA.Domain.Settings;
using SPLA.Library.Sources;

namespace SPLA.Tests;

/// <summary>
/// Trust is granted from outside, never declared from inside. The scenario is one sentence long:
/// clone a repository, its <c>.spla</c> adds a source pointing inside that same repository and calls
/// it trusted, and its text is now part of your system prompt. The layer ceiling closes that with a
/// rule rather than with vigilance.
/// </summary>
public class SkillTrustCeilingTests : IDisposable
{
    private readonly string _temp = Path.Combine(
        Path.GetTempPath(), "spla_trust_" + Path.GetRandomFileName());

    public void Dispose()
    {
        if (Directory.Exists(_temp)) Directory.Delete(_temp, recursive: true);
    }

    private SkillSourceContext Context() => new(_temp, Path.Combine(_temp, "home"), null);

    private static SplaSkillSourceSection Entry(
        string id, SourceOrigin origin, string? trust = null, string? path = null, string? level = null) => new()
    {
        Id = id, Type = "directory", Path = path ?? id, Trust = trust, Level = level, Origin = origin
    };

    private ISkillSource Built(SplaSkillSourceSection entry, string? maxTrust = null) =>
        SkillSourceRegistry.Build([entry], Context(), inheritDefaults: false, maxTrust: maxTrust)
            .Single();

    [Fact]
    public void A_project_cannot_vouch_for_itself()
    {
        Assert.Equal(SkillTrust.Untrusted,
            Built(Entry("ops", SourceOrigin.Project, trust: "trusted")).Trust);
    }

    [Fact]
    public void A_project_source_that_says_nothing_is_untrusted_too()
    {
        // Silence is not a claim, but it is not a grant either — and the content still arrived with
        // somebody else's repository.
        Assert.Equal(SkillTrust.Untrusted, Built(Entry("ops", SourceOrigin.Project)).Trust);
    }

    [Fact]
    public void The_person_at_the_keyboard_may_vouch()
    {
        Assert.Equal(SkillTrust.Trusted, Built(Entry("ops", SourceOrigin.Machine)).Trust);
        Assert.Equal(SkillTrust.Trusted, Built(Entry("ops", SourceOrigin.Granted)).Trust);
    }

    [Fact]
    public void The_administrators_ceiling_outranks_everyone()
    {
        Assert.Equal(SkillTrust.Untrusted,
            Built(Entry("ops", SourceOrigin.Machine, trust: "trusted"), maxTrust: "untrusted").Trust);
        Assert.Equal(SkillTrust.Untrusted,
            Built(Entry("ops", SourceOrigin.Granted), maxTrust: "untrusted").Trust);
    }

    [Fact]
    public void Level_is_not_under_the_ceiling()
    {
        // The two axes are separated all the way down. A project may say "show my skills only when
        // asked" — that is context economy, and the fond's owner decides it. It may not say "my
        // skills have been vetted".
        var source = Built(Entry("ops", SourceOrigin.Project, level: "out-of-catalog"));

        Assert.Equal(SourceLevel.OutOfCatalog, source.Level);
        Assert.Equal(SkillTrust.Untrusted, source.Trust);
    }

    [Fact]
    public void Repointing_an_inherited_branch_demotes_it_but_relabelling_does_not()
    {
        var context = Context();

        // A project that only adjusts disclosure changes neither what arrives nor how far it is
        // believed, so the machine's own folder keeps the machine's standing.
        var relabelled = SkillSourceRegistry.Build(
        [
            Entry("ops", SourceOrigin.Machine, trust: "trusted"),
            new SplaSkillSourceSection { Id = "ops", Level = "findable", Origin = SourceOrigin.Project }
        ], context, inheritDefaults: false).Single();
        Assert.Equal(SkillTrust.Trusted, relabelled.Trust);

        // Repointing it is a different act: the project chose what content arrives, so the project's
        // ceiling is the one that applies.
        var repointed = SkillSourceRegistry.Build(
        [
            Entry("ops", SourceOrigin.Machine, trust: "trusted"),
            new SplaSkillSourceSection { Id = "ops", Path = "somewhere-in-the-repo", Origin = SourceOrigin.Project }
        ], context, inheritDefaults: false).Single();
        Assert.Equal(SkillTrust.Untrusted, repointed.Trust);
    }

    [Fact]
    public void Policy_is_heard_only_from_the_machine_layer()
    {
        var defaults = new SplaDefaults
        {
            Skills = new SplaSkillsSection { Policy = new SplaSkillsPolicySection { MaxTrust = "untrusted" } }
        };
        // A project raising its own ceiling would be the exact move the ceiling exists to stop.
        var project = new SplaProject
        {
            Skills = new SplaSkillsSection { Policy = new SplaSkillsPolicySection { MaxTrust = "trusted" } }
        };

        Assert.Equal("untrusted", SettingsResolver.Resolve(defaults, project).SkillsMaxTrust);
    }

    [Fact]
    public void Origin_is_stamped_by_the_layer_and_never_read_from_the_file()
    {
        var yaml = """
            version: 1
            skills:
              sources:
                - id: ops
                  type: directory
                  path: ops
                  origin: deployment
            """;

        var project = ConfigLoader.ParseProjectYaml(yaml);
        var resolved = SettingsResolver.Resolve(new SplaDefaults(), project);

        // The word in the file is ignored: an entry that could name its own origin could name a
        // privileged one, which is the whole thing being prevented.
        Assert.Equal(SourceOrigin.Project, resolved.SkillSources.Single().Origin);
    }
}
