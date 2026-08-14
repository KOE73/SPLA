using System;
using System.IO;
using System.Linq;
using SPLA.Domain.Host;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// What <c>mnt/&lt;name&gt;/...</c> means, fixed as tests rather than asserted in a document.
///
/// <para>Nothing here touches the disk: a <see cref="ProjectMount"/> arrives already resolved and
/// already told whether its target exists, so everything below is path arithmetic. The one test that
/// does need a real file system is the link out of a mount — the escape a string comparison cannot
/// see — and it skips itself where the volume cannot host one.</para>
/// </summary>
public sealed class ProjectMountTests
{
    private static readonly string Root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "spla-mnt-root"));
    private static readonly string Target = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "spla-mnt-aaa"));

    private static ProjectMount Mount(
        string name = "AAA",
        string? hostPath = null,
        MountAccess access = MountAccess.Read,
        MountTrust trust = MountTrust.Trusted,
        bool available = true)
        => new(name, hostPath ?? Target, access, trust, "reference Linux settings", available);

    private static PathBoundary Bounded(params ProjectMount[] mounts) => new(Root, [".spla"], mounts);

    // ── Landing ───────────────────────────────────────────────────────────────

    [Fact]
    public void An_address_under_the_prefix_lands_in_its_mount()
    {
        var landing = Bounded(Mount()).Resolve("mnt/AAA/nginx/nginx.conf");

        Assert.True(landing.Ok);
        Assert.Equal(Path.Combine(Target, "nginx", "nginx.conf"), landing.FullPath);
        Assert.Equal("AAA", landing.Mount?.Name);
    }

    /// <summary>The name is an address, and addresses are not case-sensitive on the file systems this
    /// runs over — telling <c>mnt/AAA</c> from <c>mnt/aaa</c> would invent a distinction the disk
    /// underneath does not have.</summary>
    [Theory]
    [InlineData("mnt/aaa/f.txt")]
    [InlineData("MNT/AAA/f.txt")]
    [InlineData("./mnt/AAA/f.txt")]
    [InlineData(@"mnt\AAA\f.txt")]
    public void The_prefix_and_the_name_are_read_however_they_are_spelled(string path)
    {
        var landing = Bounded(Mount()).Resolve(path);

        Assert.True(landing.Ok);
        Assert.Equal(Path.Combine(Target, "f.txt"), landing.FullPath);
    }

    [Fact]
    public void The_mount_itself_is_a_valid_address()
    {
        var landing = Bounded(Mount()).Resolve("mnt/AAA");

        Assert.True(landing.Ok);
        Assert.Equal(Target, landing.FullPath);
        Assert.Equal("AAA", landing.Mount?.Name);
    }

    /// <summary>A path that merely starts with the mount's name is not inside it — the same sibling
    /// trap the root rule has.</summary>
    [Fact]
    public void A_sibling_of_the_target_with_a_matching_prefix_is_not_inside_the_mount()
    {
        var landing = Bounded(Mount()).Resolve(Target + "-backup" + Path.DirectorySeparatorChar + "f.txt");

        Assert.False(landing.Ok);
        Assert.Equal(PathRefusal.OutsideRoot, landing.Refusal);
    }

    // ── Escapes ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("mnt/AAA/../outside.txt")]
    [InlineData("mnt/AAA/a/b/../../../outside.txt")]
    public void Climbing_out_of_a_mount_is_refused_and_says_which_mount(string path)
    {
        var landing = Bounded(Mount()).Resolve(path);

        Assert.False(landing.Ok);
        Assert.Equal(PathRefusal.OutsideMount, landing.Refusal);
        Assert.Equal("the path leaves mount 'AAA'", landing.Error);
    }

    /// <summary>
    /// Escaping a mount is never shadowed, unlike escaping the root. The root rule waits because the
    /// legitimate exits from it were never enumerated; a mount IS that enumeration, so there is
    /// nothing left to collect and nothing to be lenient about.
    /// </summary>
    [Fact]
    public void Climbing_out_of_a_mount_is_refused_even_in_shadow_mode()
    {
        var ws = new LocalWorkspace(Bounded(Mount()), BoundaryMode.Shadow);

        var ex = Assert.Throws<PathBoundaryException>(() => ws.FileExists("mnt/AAA/../outside.txt"));
        Assert.Equal(PathRefusal.OutsideMount, ex.Refusal);
    }

    /// <summary>The escape a string comparison cannot see, on the mount side: a junction inside the
    /// target pointing anywhere on the machine. Junction rather than symlink for the same reason as
    /// in <see cref="PathBoundaryTests"/> — no elevation needed, so it actually runs.</summary>
    [Fact]
    public void A_junction_out_of_a_mount_is_refused_on_where_it_lands()
    {
        var stage = Path.Combine(AppContext.BaseDirectory, "mount-links-" + Guid.NewGuid().ToString("N"));
        var target = Path.Combine(stage, "reference");
        var outside = Path.Combine(stage, "elsewhere");
        Directory.CreateDirectory(target);
        Directory.CreateDirectory(outside);
        var link = Path.Combine(target, "escape");

        try
        {
            if (!TryCreateJunction(link, outside)) return;   // volume cannot host one; nothing to prove

            var landing = Bounded(Mount(hostPath: target)).Resolve("mnt/AAA/escape/loot.txt");

            Assert.False(landing.Ok);
            Assert.Equal(PathRefusal.OutsideMount, landing.Refusal);

            Directory.CreateDirectory(Path.Combine(target, "ordinary"));
            Assert.True(Bounded(Mount(hostPath: target)).Resolve("mnt/AAA/ordinary/f.txt").Ok);
        }
        finally
        {
            try { Directory.Delete(link); } catch { /* best effort */ }
            try { Directory.Delete(stage, recursive: true); } catch { /* best effort */ }
        }
    }

    private static bool TryCreateJunction(string link, string target)
    {
        try
        {
            using var p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c mklink /J \"{link}\" \"{target}\"",
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            p!.WaitForExit(10_000);
            return Directory.Exists(link);
        }
        catch { return false; }
    }

    // ── Addresses that name nothing ───────────────────────────────────────────

    [Fact]
    public void An_undeclared_mount_name_is_refused_and_the_declared_ones_are_listed()
    {
        var landing = Bounded(Mount()).Resolve("mnt/BBB/f.txt");

        Assert.False(landing.Ok);
        Assert.Equal(PathRefusal.MountUnknown, landing.Refusal);
        Assert.Contains("no mount named 'BBB'", landing.Error);
        Assert.Contains("mnt/AAA", landing.Error);
    }

    /// <summary>The prefix is not a folder. Saying so — and saying what it is instead — costs one
    /// sentence and saves the model a round of guessing.</summary>
    [Theory]
    [InlineData("mnt")]
    [InlineData("mnt/")]
    public void The_bare_prefix_is_refused_with_an_explanation(string path)
    {
        var landing = Bounded(Mount()).Resolve(path);

        Assert.False(landing.Ok);
        Assert.Equal(PathRefusal.MountUnknown, landing.Refusal);
        Assert.Contains("is not a folder", landing.Error);
    }

    [Fact]
    public void With_no_mounts_declared_the_prefix_says_so_rather_than_reading_as_a_folder()
    {
        var landing = Bounded().Resolve("mnt/AAA/f.txt");

        Assert.False(landing.Ok);
        Assert.Equal(PathRefusal.MountUnknown, landing.Refusal);
        Assert.Contains("declares no mounts", landing.Error);
    }

    // ── Host paths that land in a mount ───────────────────────────────────────

    /// <summary>A declared mount is a legal exit from the root. Reporting a path inside one as
    /// "outside the project" would name the single case the manifest exists to describe as the
    /// failure it was written to remove.</summary>
    [Fact]
    public void An_absolute_path_inside_a_target_lands_in_that_mount_not_outside_the_project()
    {
        var landing = Bounded(Mount()).Resolve(Path.Combine(Target, "nginx.conf"));

        Assert.True(landing.Ok);
        Assert.Equal("AAA", landing.Mount?.Name);
    }

    [Fact]
    public void An_absolute_path_in_no_mount_and_outside_the_root_is_still_refused()
    {
        var landing = Bounded(Mount()).Resolve(Path.Combine(Path.GetTempPath(), "spla-mnt-elsewhere", "f.txt"));

        Assert.False(landing.Ok);
        Assert.Equal(PathRefusal.OutsideRoot, landing.Refusal);
    }

    // ── The reverse map ───────────────────────────────────────────────────────

    /// <summary>Both directions, because one without the other is a leak: the model gets a host path,
    /// hands it back as an argument, and it resolves under different rules on the next machine.</summary>
    [Fact]
    public void A_host_path_in_a_mount_maps_back_to_its_canonical_address()
    {
        var boundary = Bounded(Mount());

        Assert.Equal("mnt/AAA/nginx/nginx.conf", boundary.ToCanonical(Path.Combine(Target, "nginx", "nginx.conf")));
        Assert.Equal("mnt/AAA", boundary.ToCanonical(Target));

        // …and the canonical form maps forward to the same host path it came from.
        Assert.True(boundary.TryResolve("mnt/AAA/nginx/nginx.conf", out var back, out _));
        Assert.Equal(Path.Combine(Target, "nginx", "nginx.conf"), back);
    }

    [Fact]
    public void A_host_path_inside_the_root_maps_back_to_a_project_relative_address()
    {
        var boundary = Bounded(Mount());

        Assert.Equal("src/app.cs", boundary.ToCanonical(Path.Combine(Root, "src", "app.cs")));
        Assert.Equal(".", boundary.ToCanonical(Root));
    }

    [Fact]
    public void A_path_that_lands_nowhere_has_no_canonical_form()
        => Assert.Null(Bounded(Mount()).ToCanonical(Path.Combine(Path.GetTempPath(), "spla-mnt-elsewhere")));

    /// <summary>The workspace is where the reverse map is actually reached from, and it used to hand
    /// back the absolute host path — the inverse of nothing.</summary>
    [Fact]
    public void The_workspace_reverse_map_gives_the_canonical_form()
    {
        var ws = new LocalWorkspace(Bounded(Mount()), BoundaryMode.Shadow);

        Assert.Equal("mnt/AAA/f.txt", ws.MapPathToProject(Path.Combine(Target, "f.txt")));
        Assert.Equal("src/app.cs", ws.MapPathToProject(Path.Combine(Root, "src", "app.cs")));
    }

    // ── The floor, and the unplugged target ───────────────────────────────────

    [Fact]
    public void A_read_only_mount_refuses_writes_and_still_allows_reads()
    {
        var ws = new LocalWorkspace(Bounded(Mount(access: MountAccess.Read)), BoundaryMode.Shadow);

        var ex = Assert.Throws<PathBoundaryException>(
            () => ws.WriteAllTextAsync("mnt/AAA/f.txt", "x").GetAwaiter().GetResult());

        Assert.Equal(PathRefusal.MountReadOnly, ex.Refusal);
        Assert.Contains("read-only", ex.Message);

        ws.FileExists("mnt/AAA/f.txt");   // a read gets through the same gate
    }

    [Theory]
    [InlineData("delete")]
    [InlineData("mkdir")]
    public void Every_state_changing_call_asks_the_floor_not_just_writing_text(string operation)
    {
        var ws = new LocalWorkspace(Bounded(Mount(access: MountAccess.Read)), BoundaryMode.Shadow);

        var ex = Assert.Throws<PathBoundaryException>(() =>
        {
            if (operation == "delete") ws.DeleteFile("mnt/AAA/f.txt");
            else ws.CreateDirectory("mnt/AAA/sub");
        });

        Assert.Equal(PathRefusal.MountReadOnly, ex.Refusal);
    }

    [Fact]
    public void A_writable_mount_lets_a_write_through_to_the_file_system()
    {
        var ws = new LocalWorkspace(Bounded(Mount(access: MountAccess.Write)), BoundaryMode.Shadow);

        // Not a refusal: it reaches the disk and fails there, which is a fault, not a decision.
        Assert.IsNotType<PathBoundaryException>(
            Record.Exception(() => ws.WriteAllTextAsync("mnt/AAA/f.txt", "x").GetAwaiter().GetResult()));
    }

    /// <summary>Unplugged is its own diagnosis. "File not found" would send whoever reads it looking
    /// for a file instead of for the folder that is not connected.</summary>
    [Fact]
    public void An_unavailable_mount_is_refused_by_name_not_as_a_missing_file()
    {
        var ws = new LocalWorkspace(Bounded(Mount(available: false)), BoundaryMode.Shadow);

        var ex = Assert.Throws<PathBoundaryException>(() => ws.FileExists("mnt/AAA/f.txt"));

        Assert.Equal(PathRefusal.MountUnavailable, ex.Refusal);
        Assert.Contains("mount 'AAA'", ex.Message);
        Assert.Contains("not on this machine", ex.Message);
    }

    // ── Mounts do not disturb what was already there ──────────────────────────

    [Fact]
    public void Declaring_a_mount_changes_nothing_about_the_root_or_its_cutouts()
    {
        var boundary = Bounded(Mount());

        Assert.True(boundary.Resolve("src/app.cs").Ok);
        Assert.Null(boundary.Resolve("src/app.cs").Mount);

        Assert.Equal(PathRefusal.Cutout, boundary.Resolve(".spla/secrets.yaml").Refusal);
        Assert.Equal(PathRefusal.OutsideRoot, boundary.Resolve("../outside.txt").Refusal);
        Assert.Equal(PathRefusal.NetworkShare, boundary.Resolve(@"\\server\share\f.txt").Refusal);
    }

    [Fact]
    public void Two_mounts_are_two_areas_and_neither_reaches_into_the_other()
    {
        var other = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "spla-mnt-bbb"));
        var boundary = Bounded(Mount(), Mount(name: "BBB", hostPath: other));

        Assert.Equal("AAA", boundary.Resolve("mnt/AAA/f.txt").Mount?.Name);
        Assert.Equal("BBB", boundary.Resolve("mnt/BBB/f.txt").Mount?.Name);
        Assert.Equal(other, boundary.Resolve("mnt/BBB").FullPath);
        Assert.Equal("mnt/BBB/f.txt", boundary.ToCanonical(Path.Combine(other, "f.txt")));
    }

    [Fact]
    public void An_unbounded_boundary_has_no_mounts_and_no_prefix()
    {
        Assert.Empty(PathBoundary.None.Mounts);

        // No project, no reservation: `mnt/x` is just a relative path again.
        var landing = PathBoundary.None.Resolve("mnt/AAA/f.txt");
        Assert.True(landing.Ok);
        Assert.Null(landing.Mount);
    }
}
