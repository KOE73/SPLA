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

    private SkillSourceContext Context() => new(Path.Combine(_temp, "workspace"), Path.Combine(_temp, "home"), null);

    /// <summary>Points outside the workspace by default, so these cases exercise the LAYER ceiling
    /// on its own. A folder inside the workspace is untrusted regardless of who declared it, which
    /// would otherwise mask every result here.</summary>
    private SplaSkillSourceSection Entry(
        string id, SourceOrigin origin, string? trust = null, string? path = null, string? level = null) => new()
    {
        Id = id, Type = "directory", Path = path ?? Path.Combine(_temp, "elsewhere", id),
        Trust = trust, Level = level, Origin = origin
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
            new SplaSkillSourceSection
            {
                Id = "ops", Path = Path.Combine(_temp, "elsewhere", "other"), Origin = SourceOrigin.Project
            }
        ], context, inheritDefaults: false).Single();
        Assert.Equal(SkillTrust.Untrusted, repointed.Trust);
    }

    [Fact]
    public void Restating_a_default_path_is_not_a_repointing()
    {
        // A project restating `skills` — the same folder a built-in entry already names. Treating a
        // restatement as a choice of content would demote the entry for the wrong reason; whether it
        // is trusted is decided by WHERE it is, which the workspace rule below covers.
        var sources = SkillSourceRegistry.Build(
        [
            new SplaSkillSourceSection { Type = "directory", Path = "skills", Origin = SourceOrigin.Project }
        ], Context());

        Assert.Equal(["repo", "machine"], sources.Select(s => s.Id));
        // machine lives in the person's home, so restating project paths left it alone.
        Assert.Equal(SkillTrust.Trusted, sources.Single(s => s.Id == "machine").Trust);
    }

    // ── Where the folder is, not who declared it ─────────────────────────────

    [Fact]
    public void A_folder_inside_the_workspace_starts_untrusted_even_with_nobody_declaring_it()
    {
        // The case the layer ceiling missed entirely: a stranger's repository with a skills/ folder
        // and no config at all. Nobody declared anything, so nobody could be caught claiming trust —
        // and the text walked straight into the system prompt.
        var sources = SkillSourceRegistry.Build(null, Context());

        Assert.Equal(SkillTrust.Untrusted, sources.Single(s => s.Id == "repo").Trust);
        // The person's own home is not somewhere a clone can put anything.
        Assert.Equal(SkillTrust.Trusted, sources.Single(s => s.Id == "machine").Trust);
    }

    [Fact]
    public void Approving_the_folder_is_what_lifts_it()
    {
        var repoSkills = Path.Combine(_temp, "workspace", "skills");
        Directory.CreateDirectory(repoSkills);

        var grants = new FileSkillTrustStore(_temp);
        Assert.Equal(SkillTrust.Untrusted,
            SkillSourceRegistry.Build(null, Context(), trustStore: grants).Single(s => s.Id == "repo").Trust);

        grants.Grant(repoSkills);
        Assert.Equal(SkillTrust.Trusted,
            SkillSourceRegistry.Build(null, Context(), trustStore: grants).Single(s => s.Id == "repo").Trust);
    }

    [Fact]
    public void The_installation_branch_is_not_workspace_content_even_when_it_sits_inside_one()
    {
        // True whenever this product is developed on itself: bin/ is under the workspace, and skills
        // shipped beside the executable are the product's own wherever it was built.
        var workspace = Path.Combine(_temp, "workspace");
        var app = Path.Combine(workspace, "src", "apps", "bin");
        Directory.CreateDirectory(app);
        var context = new SkillSourceContext(workspace, Path.Combine(_temp, "home"), null, app);

        var sources = SkillSourceRegistry.Build(null, context);

        Assert.Equal(SkillTrust.Trusted, sources.Single(s => s.Id == "builtin").Trust);
        Assert.Equal(SkillTrust.Untrusted, sources.Single(s => s.Id == "repo").Trust);
    }

    [Fact]
    public void A_folder_outside_the_workspace_is_unaffected_by_the_rule()
    {
        var outside = Path.Combine(Path.GetTempPath(), "spla_outside_" + Path.GetRandomFileName());

        var built = SkillSourceRegistry.Build(
            [new SplaSkillSourceSection { Id = "ops", Type = "directory", Path = outside, Origin = SourceOrigin.Granted }],
            Context(), inheritDefaults: false).Single();

        Assert.Equal(SkillTrust.Trusted, built.Trust);
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
