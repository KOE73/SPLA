using SPLA.Domain.Project;
using SPLA.Domain.Settings;
using SPLA.Library.Sources;

namespace SPLA.Tests;

/// <summary>
/// The same model on a server, with one thing different. A user writing in their own area is not a
/// risk — the right to write was never the axis worth cutting on. The trust level that entry claims
/// is, so that is what the deployment holds back.
/// </summary>
public class SkillServerLayoutTests : IDisposable
{
    private readonly string _temp = Path.Combine(
        Path.GetTempPath(), "spla_server_" + Path.GetRandomFileName());

    public SkillServerLayoutTests() => Directory.CreateDirectory(_temp);

    public void Dispose()
    {
        ConfigLoader.PersonalDirResolver = null;
        if (Directory.Exists(_temp)) Directory.Delete(_temp, recursive: true);
    }

    private SkillSourceContext Context() => new(Path.Combine(_temp, "ws"), Path.Combine(_temp, "home"), null);

    private static SplaSkillSourceSection Mine(string path) => new()
    {
        Id = "ops", Type = "directory", Path = path, Origin = SourceOrigin.Granted
    };

    // ── Whose folders these are ──────────────────────────────────────────────

    [Fact]
    public void A_project_inside_a_users_area_resolves_to_that_area()
    {
        var root = new ServerProjectRoot(Path.Combine(_temp, "root"));
        var area = root.EnsureUserArea("S-1-5-21-alice");

        Assert.Equal(area, root.UserAreaFor(area));
        Assert.Equal(area, root.UserAreaFor(Path.Combine(area, "myproject")));
        Assert.Equal(area, root.UserAreaFor(Path.Combine(area, "myproject", "deep", "inside")));
    }

    [Fact]
    public void Anything_outside_the_users_root_belongs_to_nobody_in_particular()
    {
        var root = new ServerProjectRoot(Path.Combine(_temp, "root"));

        Assert.Null(root.UserAreaFor(Path.Combine(_temp, "elsewhere")));
        Assert.Null(root.UserAreaFor(Path.Combine(_temp, "root")));
        Assert.Null(root.UserAreaFor(null));
    }

    [Fact]
    public void Two_users_of_one_server_share_neither_a_fond_nor_an_approval()
    {
        var root = new ServerProjectRoot(Path.Combine(_temp, "root"));
        var alice = root.EnsureUserArea("alice");
        var bob = root.EnsureUserArea("bob");

        new FileSkillSourceStore(alice).Save([Mine(Path.Combine(_temp, "alice-skills"))]);
        new FileSkillTrustStore(alice).Grant(Path.Combine(_temp, "alice-skills"));

        Assert.Single(new FileSkillSourceStore(alice).Load());
        Assert.Empty(new FileSkillSourceStore(bob).Load());
        Assert.False(new FileSkillTrustStore(bob).IsGranted(Path.Combine(_temp, "alice-skills")));
    }

    // ── What they may claim ──────────────────────────────────────────────────

    private ISkillSource Built(bool userMayVouch, ISkillTrustStore? grants = null)
    {
        var folder = Path.Combine(_temp, "outside-skills");
        Directory.CreateDirectory(folder);
        return SkillSourceRegistry.Build(
            [Mine(folder)], Context(), inheritDefaults: false,
            trustStore: grants, userMayVouch: userMayVouch).Single();
    }

    [Fact]
    public void Locally_a_person_is_their_own_administrator()
    {
        Assert.Equal(SkillTrust.Trusted, Built(userMayVouch: true).Trust);
    }

    [Fact]
    public void On_a_server_neither_the_entry_nor_the_users_own_grant_lifts_it()
    {
        var folder = Path.Combine(_temp, "outside-skills");
        Directory.CreateDirectory(folder);
        var grants = new FileSkillTrustStore(_temp);
        grants.Grant(folder);

        // Both routes up close together: a user who may not vouch cannot do it by declaring the entry
        // trusted, nor by ticking the box that records an approval.
        Assert.Equal(SkillTrust.Untrusted, Built(userMayVouch: false, grants).Trust);
    }

    [Fact]
    public void The_administrators_own_layer_still_vouches_on_a_server()
    {
        var folder = Path.Combine(_temp, "outside-skills");
        Directory.CreateDirectory(folder);

        // On a server the machine layer is the service account's home, which a user cannot write.
        var built = SkillSourceRegistry.Build(
            [new SplaSkillSourceSection
            {
                Id = "ops", Type = "directory", Path = folder,
                Trust = "trusted", Origin = SourceOrigin.Machine
            }],
            Context(), inheritDefaults: false, userMayVouch: false).Single();

        Assert.Equal(SkillTrust.Trusted, built.Trust);
    }

    [Fact]
    public void The_deployment_default_follows_from_being_multi_user_and_policy_overrides_it()
    {
        var single = new ResolvedSettings();
        Assert.True(single.SkillsUserMayVouchEffective);

        var server = new ResolvedSettings { IsMultiUserDeployment = true };
        Assert.False(server.SkillsUserMayVouchEffective);

        // An administrator handing the judgement back is one line in their own layer.
        server.SkillsUserMayVouch = true;
        Assert.True(server.SkillsUserMayVouchEffective);
    }

    [Fact]
    public void Policy_reaches_resolved_settings_only_from_the_machine_layer()
    {
        var defaults = new SplaDefaults
        {
            Skills = new SplaSkillsSection { Policy = new SplaSkillsPolicySection { UserMayVouch = false } }
        };
        var project = new SplaProject
        {
            Skills = new SplaSkillsSection { Policy = new SplaSkillsPolicySection { UserMayVouch = true } }
        };

        Assert.False(SettingsResolver.Resolve(defaults, project).SkillsUserMayVouch);
    }
}
