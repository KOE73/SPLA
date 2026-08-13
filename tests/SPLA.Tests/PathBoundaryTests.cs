using System;
using System.IO;
using SPLA.Domain.Host;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// The boundary is the one thing in this area that is provable, so it is proved here rather than
/// asserted in a document. Four hand-rolled versions of the same check disagreed on the edge cases
/// below; this fixes what the answers are.
/// </summary>
public sealed class PathBoundaryTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-bound-" + Guid.NewGuid().ToString("N"));

    public PathBoundaryTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best effort */ }
    }

    private PathBoundary Bounded(params string[] cutouts) => new(_root, cutouts);

    [Fact]
    public void A_relative_path_lands_under_the_root_not_under_the_current_directory()
    {
        Assert.True(Bounded().TryResolve("src/app.cs", out var full, out _));
        Assert.Equal(Path.Combine(_root, "src", "app.cs"), full);
    }

    [Fact]
    public void The_root_itself_is_in_bounds()
    {
        Assert.True(Bounded().TryResolve(_root, out _, out _));
        Assert.True(Bounded().TryResolve(_root + Path.DirectorySeparatorChar, out _, out _));
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("a/b/../../../outside.txt")]
    [InlineData("./sub/../../outside.txt")]
    public void Climbing_out_is_refused_however_it_is_spelled(string path)
    {
        Assert.False(Bounded().TryResolve(path, out _, out var error));
        Assert.Equal("the path is outside the project", error);
    }

    [Fact]
    public void An_absolute_path_elsewhere_on_the_machine_is_refused()
    {
        var elsewhere = Path.Combine(Path.GetTempPath(), "spla-elsewhere-" + Guid.NewGuid().ToString("N"));

        Assert.False(Bounded().TryResolve(elsewhere, out _, out var error));
        Assert.Equal("the path is outside the project", error);
    }

    [Theory]
    [InlineData(@"\\server\share\file.txt")]
    [InlineData("//server/share/file.txt")]
    public void A_network_share_is_refused_before_it_can_be_combined_with_the_root(string path)
    {
        Assert.False(Bounded().TryResolve(path, out _, out var error));
        Assert.Equal("network shares are outside the project", error);
    }

    /// <summary>A sibling whose name merely starts with the root's — <c>project-backup</c> next to
    /// <c>project</c> — is not inside it. String-prefix checks get this wrong.</summary>
    [Fact]
    public void A_sibling_with_a_matching_prefix_is_not_inside()
    {
        var boundary = new PathBoundary(Path.Combine(_root, "project"));

        Assert.False(boundary.TryResolve(Path.Combine(_root, "project-backup", "f.txt"), out _, out var error));
        Assert.Equal("the path is outside the project", error);
    }

    [Fact]
    public void A_cutout_inside_the_root_is_refused_and_says_which_one()
    {
        var boundary = Bounded(".spla");

        Assert.False(boundary.TryResolve(".spla/secrets.yaml", out _, out var error));
        Assert.Contains(".spla", error);
        // Its neighbours are unaffected.
        Assert.True(boundary.TryResolve("src/app.cs", out _, out _));
    }

    /// <summary>The cutout is a subtree, not a single file, and the boundary must not be fooled by a
    /// path that walks into it sideways.</summary>
    [Fact]
    public void A_cutout_covers_everything_beneath_it()
    {
        var boundary = Bounded(".spla");

        Assert.False(boundary.TryResolve(".spla/chats/2026-01-01.yaml", out _, out _));
        Assert.False(boundary.TryResolve("src/../.spla/grants.yaml", out _, out _));
    }

    /// <summary>
    /// The escape a string comparison cannot see: a junction inside the project pointing anywhere on
    /// the machine. Every path through it reads as in-bounds until you ask where it lands.
    ///
    /// <para>A junction rather than a symbolic link on purpose — symlinks need elevation on Windows,
    /// junctions do not, so this actually runs instead of quietly passing. It does need NTFS, which
    /// the temp volume is not always, hence the fixture next to the test assembly.</para>
    /// </summary>
    [Fact]
    public void A_junction_pointing_out_of_the_root_is_refused_on_where_it_lands()
    {
        var stage = Path.Combine(AppContext.BaseDirectory, "boundary-links-" + Guid.NewGuid().ToString("N"));
        var root = Path.Combine(stage, "project");
        var outside = Path.Combine(stage, "elsewhere");
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(outside);
        var link = Path.Combine(root, "escape");

        try
        {
            if (!TryCreateJunction(link, outside)) return;   // filesystem cannot host one; nothing to prove here

            var boundary = new PathBoundary(root);

            Assert.False(boundary.TryResolve("escape/loot.txt", out _, out var error));
            Assert.Equal("the path is outside the project", error);

            // And an ordinary folder beside it is unaffected — the check is on the link, not on the name.
            Directory.CreateDirectory(Path.Combine(root, "ordinary"));
            Assert.True(boundary.TryResolve("ordinary/file.txt", out _, out _));
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

    [Fact]
    public void An_empty_path_is_refused_rather_than_silently_meaning_the_root()
    {
        Assert.False(Bounded().TryResolve("   ", out _, out var error));
        Assert.Equal("the path is empty", error);
    }

    /// <summary>No project means no boundary. Bounding tools by the directory a process happened to
    /// start in would be an accident dressed up as a rule.</summary>
    [Fact]
    public void Without_a_boundary_every_path_resolves_to_itself()
    {
        Assert.False(PathBoundary.None.IsBounded);

        Assert.True(PathBoundary.None.TryResolve(Path.GetTempPath(), out var temp, out _));
        Assert.Equal(Path.GetFullPath(Path.GetTempPath()), temp);

        Assert.True(PathBoundary.None.TryResolve(@"\\server\share\f.txt", out _, out _));
    }
}
