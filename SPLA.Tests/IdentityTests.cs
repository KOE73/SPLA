using SPLA.Domain.Identity;
using SPLA.Domain.Project;
using SPLA.Identity.Windows;

namespace SPLA.Tests;

/// <summary>
/// The server multi-user seam: an identity resolved from the real Windows domain token, and each
/// identity getting its own isolated file area. This is the concrete first step of the "запусти
/// сервер, доменные сотрудники коннектятся, каждому автоматом папка, видит своё" scenario — proven
/// against the actual <see cref="System.Security.Principal.WindowsIdentity"/>, not a fake.
/// </summary>
public sealed class IdentityTests
{
    [Fact]
    public void Windows_resolver_yields_real_domain_identity()
    {
        var id = WindowsIdentityResolver.Current();

        // UserKey is the account SID — the stable, path-safe owner key.
        Assert.StartsWith("S-1-", id.UserKey);

        // DisplayName is DOMAIN\user (for humans, never used as a key).
        Assert.Contains("\\", id.DisplayName);

        // A domain account is a member of groups (Everyone/Users at minimum, plus domain groups).
        Assert.NotEmpty(id.Groups);
        Assert.All(id.Groups, g => Assert.StartsWith("S-1-", g));
    }

    [Fact]
    public void Each_identity_gets_an_isolated_user_area()
    {
        var root = new ServerProjectRoot(NewTempRoot());
        var alice = new ClaimIdentity("S-1-5-21-alice", "KOMBINAT\\alice", []);
        var bob = new ClaimIdentity("S-1-5-21-bob", "KOMBINAT\\bob", []);

        // First touch auto-provisions the area, and the two areas are distinct and namespaced by key.
        var areaA = root.EnsureUserArea(alice.UserKey);
        var areaB = root.EnsureUserArea(bob.UserKey);
        Assert.True(Directory.Exists(areaA));
        Assert.True(Directory.Exists(areaB));
        Assert.NotEqual(areaA, areaB);
        Assert.Contains(alice.UserKey, areaA);

        // Alice creates a project in her area; Bob, on his own provider, does not see it.
        var provA = root.ProviderFor(alice);
        var provB = root.ProviderFor(bob);
        provA.Create(new ProjectDescriptor
        {
            Id = "p1",
            Name = "alice-project",
            ManifestPath = Path.Combine(areaA, "p1", "project.spla")
        });

        Assert.Contains(provA.List(), p => p.Name == "alice-project");
        Assert.Empty(provB.List());
    }

    [Fact]
    public void Identity_provider_loads_from_config_by_reflection()
    {
        // Point config at the actual built Windows provider DLL — proves a host loads a platform
        // provider purely by assembly + type name, with NO compile-time reference to it.
        var provider = IdentityProviderLoader.Load(
            new IdentityProviderConfig
            {
                Assembly = typeof(WindowsIdentityProvider).Assembly.Location,
                Type = typeof(WindowsIdentityProvider).FullName
            },
            AppContext.BaseDirectory);

        Assert.IsType<WindowsIdentityProvider>(provider);
        Assert.StartsWith("S-1-", provider.Current().UserKey);   // real domain principal via the DLL
    }

    [Fact]
    public void No_config_yields_the_neutral_provider()
        => Assert.IsType<ClaimsIdentityProvider>(IdentityProviderLoader.Load(null, AppContext.BaseDirectory));

    private static string NewTempRoot()
    {
        var dir = Path.Combine(Path.GetTempPath(), "spla-identity-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
