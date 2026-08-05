using SPLA.Domain.Settings;
using SPLA.Library.Sources;

namespace SPLA.Tests;

/// <summary>
/// Trust is a grant, kept where its recipient cannot write and addressed by the location whose
/// contents were read. Self-signed trust is not trust — the reason git refuses a config from a
/// repository owned by someone else, and apt keeps repository keys in a keyring the repository does
/// not control.
/// </summary>
public class SkillTrustGrantTests : IDisposable
{
    private readonly string _temp = Path.Combine(
        Path.GetTempPath(), "spla_grant_" + Path.GetRandomFileName());

    public SkillTrustGrantTests() => Directory.CreateDirectory(_temp);

    public void Dispose()
    {
        if (Directory.Exists(_temp)) Directory.Delete(_temp, recursive: true);
    }

    private string Folder(string name)
    {
        var path = Path.Combine(_temp, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private SkillSourceContext Context() => new(_temp, Path.Combine(_temp, "home"), null);

    private ISkillSource Built(string path, ISkillTrustStore? grants, string? maxTrust = null) =>
        SkillSourceRegistry.Build(
            [new SplaSkillSourceSection { Id = "ops", Type = "directory", Path = path, Origin = SourceOrigin.Project }],
            Context(), inheritDefaults: false, maxTrust: maxTrust, trustStore: grants).Single();

    [Fact]
    public void Without_a_grant_a_project_source_stays_untrusted()
    {
        Assert.Equal(SkillTrust.Untrusted, Built(Folder("ops"), new FileSkillTrustStore(_temp)).Trust);
    }

    [Fact]
    public void A_grant_lifts_it_and_revoking_puts_it_back()
    {
        var folder = Folder("ops");
        var grants = new FileSkillTrustStore(_temp);

        grants.Grant(folder);
        Assert.Equal(SkillTrust.Trusted, Built(folder, grants).Trust);

        Assert.True(grants.Revoke(folder));
        Assert.Equal(SkillTrust.Untrusted, Built(folder, grants).Trust);
    }

    [Fact]
    public void The_grant_follows_the_folder_not_the_entry_name()
    {
        var folder = Folder("ops");
        var grants = new FileSkillTrustStore(_temp);
        grants.Grant(folder);

        // Renamed entry, same folder: approval is about contents, and the contents did not move.
        var renamed = SkillSourceRegistry.Build(
            [new SplaSkillSourceSection { Id = "something-else", Type = "directory", Path = folder, Origin = SourceOrigin.Project }],
            Context(), inheritDefaults: false, trustStore: grants).Single();
        Assert.Equal(SkillTrust.Trusted, renamed.Trust);

        // Same entry, different folder: what is there now is not what was read.
        Assert.Equal(SkillTrust.Untrusted, Built(Folder("elsewhere"), grants).Trust);
    }

    [Fact]
    public void The_grant_file_stays_readable_by_the_person_it_belongs_to()
    {
        var folder = Folder("ops");
        var grants = new FileSkillTrustStore(_temp);
        grants.Grant(folder, "koe");

        var text = File.ReadAllText(grants.FilePath);

        // A DateTimeOffset serialised as an object turns one timestamp into twenty lines of
        // day_of_year and total_offset_minutes. This file records a person's own decisions; they
        // have to be able to read it.
        Assert.DoesNotContain("day_of_year", text);
        Assert.Contains("granted_by: koe", text);
        Assert.True(Math.Abs((grants.List().Single().GrantedAt - DateTimeOffset.Now).TotalMinutes) < 5);
    }

    [Fact]
    public void One_spelling_per_location()
    {
        var folder = Folder("ops");
        var grants = new FileSkillTrustStore(_temp);
        grants.Grant(folder + Path.DirectorySeparatorChar);

        Assert.True(grants.IsGranted(folder));
        Assert.Single(grants.List());
    }

    [Fact]
    public void A_grant_written_into_the_project_file_does_nothing()
    {
        // There is no field for it, and that is the point: the store is the only place a grant can
        // live, and a repository cannot write there.
        var yaml = """
            version: 1
            skills:
              sources:
                - id: ops
                  type: directory
                  path: ops
                  trust: trusted
                  granted: true
            """;

        var project = ConfigLoader.ParseProjectYaml(yaml);
        var resolved = SettingsResolver.Resolve(new SplaDefaults(), project);

        var built = SkillSourceRegistry.Build(
            resolved.SkillSources, Context(), inheritDefaults: false,
            trustStore: new FileSkillTrustStore(_temp)).Single();

        Assert.Equal(SkillTrust.Untrusted, built.Trust);
    }

    [Fact]
    public void The_administrators_ceiling_outranks_a_grant()
    {
        var folder = Folder("ops");
        var grants = new FileSkillTrustStore(_temp);
        grants.Grant(folder);

        Assert.Equal(SkillTrust.Untrusted, Built(folder, grants, maxTrust: "untrusted").Trust);
    }

    [Fact]
    public void An_unreadable_grant_file_trusts_nothing_rather_than_everything()
    {
        var folder = Folder("ops");
        var grants = new FileSkillTrustStore(_temp);
        File.WriteAllText(grants.FilePath, "][ not yaml at all: [[[");

        Assert.False(grants.IsGranted(folder));
        Assert.Equal(SkillTrust.Untrusted, Built(folder, grants).Trust);
    }

    [Fact]
    public void Switching_one_skill_on_no_longer_gets_past_an_untrusted_source()
    {
        var library = new SPLA.Library.SkillLibrary(
            [new Fakes.FakeSkillSource("server", SkillTrust.Untrusted).With("imported.thing")]);

        library.ApplySettings(new Dictionary<string, SplaSkillSection>
        {
            ["imported.thing"] = new() { Enabled = true }
        });

        // If the branch as a whole is not trusted, one arbitrarily trusted book out of it is a
        // contradiction: the contents arrived together, from the same place, on the same terms.
        var skill = library.Find("imported.thing")!;
        Assert.Equal(SPLA.Library.Catalog.SkillState.DisabledByTrust, skill.State);
        Assert.Contains("approve the folder", skill.StateReason);
    }
}
