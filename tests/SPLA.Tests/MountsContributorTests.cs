using System.Linq;
using SPLA.Agent.Composition;
using SPLA.Domain.Host;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// What the model is told about mounts. The address alone is not enough: a folder named without a
/// purpose beside it gets opened so the purpose can be discovered, which is the whole reason the
/// manifest refuses a mount with no description.
/// </summary>
public sealed class MountsContributorTests
{
    private static ProjectMount Mount(
        string name = "AAA",
        MountAccess access = MountAccess.Read,
        MountTrust trust = MountTrust.Trusted,
        bool available = true,
        string description = "reference Linux settings, do not edit")
        => new(name, @"C:\elsewhere\" + name, access, trust, description, available);

    private static string Body(params ProjectMount[] mounts)
    {
        var settings = new ResolvedSettings { Mounts = mounts };
        var contribution = new MountsContributor()
            .Contribute(new AgentContributionContext(settings, @"C:\project"));

        return string.Concat(contribution.Context.Select(i => i.Body));
    }

    /// <summary>Most projects have none, and they must not pay a paragraph of prompt for it.</summary>
    [Fact]
    public void A_project_with_no_mounts_contributes_nothing()
        => Assert.Empty(new MountsContributor()
            .Contribute(new AgentContributionContext(new ResolvedSettings(), @"C:\project")).Context);

    [Fact]
    public void Each_mount_is_announced_by_address_and_by_what_it_is_for()
    {
        var body = Body(Mount());

        Assert.Contains("mnt/AAA/", body);
        Assert.Contains("reference Linux settings, do not edit", body);
    }

    /// <summary>Knowing a mount is read-only up front costs nothing; discovering it costs a turn.</summary>
    [Theory]
    [InlineData(MountAccess.Read, "read-only")]
    [InlineData(MountAccess.Write, "writable")]
    public void The_floor_is_stated_rather_than_left_to_be_discovered(MountAccess access, string expected)
        => Assert.Contains(expected, Body(Mount(access: access)));

    /// <summary>Unplugged has to read as unplugged, or the model goes looking for a missing file.</summary>
    [Fact]
    public void An_unavailable_mount_says_so_loudly()
        => Assert.Contains("NOT CONNECTED", Body(Mount(available: false)));

    [Fact]
    public void An_untrusted_mount_says_who_writes_there()
        => Assert.Contains("others write here", Body(Mount(trust: MountTrust.Untrusted)));

    /// <summary>The instruction that keeps the address portable. Without it the model reasonably
    /// substitutes the machine path it can see elsewhere, and the project stops travelling.</summary>
    [Fact]
    public void The_model_is_told_to_use_the_address_and_not_a_machine_path()
    {
        var body = Body(Mount());

        Assert.Contains("never substitute a path from this machine", body);
        Assert.DoesNotContain(@"C:\elsewhere", body);
    }

    [Fact]
    public void Several_mounts_are_all_listed()
    {
        var body = Body(Mount(), Mount(name: "BBB", description: "shared deployment target"));

        Assert.Contains("mnt/AAA/", body);
        Assert.Contains("mnt/BBB/", body);
        Assert.Contains("shared deployment target", body);
    }
}
