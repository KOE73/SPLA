using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPLA.Domain.Host;
using SPLA.Domain.Settings;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Every rule about mounts fires here, at load, and nowhere else. A check that runs at use instead
/// fires at a moment nobody chose — which is the failure this whole design was written around: a
/// project that stops opening because of somebody else's commit.
///
/// <para>The disk is touched for exactly one question — is the target there — plus the reserved-name
/// check, which is a question about the tree. Everything else is arithmetic on strings.</para>
/// </summary>
public sealed class MountResolverTests : IDisposable
{
    private readonly string _stage = Path.Combine(Path.GetTempPath(), "spla-mounts-" + Guid.NewGuid().ToString("N"));
    private readonly string _root;
    private readonly string _manifest;

    public MountResolverTests()
    {
        _root = Path.Combine(_stage, "project");
        _manifest = Path.Combine(_root, "app.spla");
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try { Directory.Delete(_stage, recursive: true); } catch { /* best effort */ }
    }

    private IReadOnlyList<ProjectMount> Resolve(params SplaMountSection[] sections)
        => MountResolver.Resolve(sections, _root, _manifest);

    private static SplaMountSection Section(
        string? name = "AAA",
        string? path = "../reference",
        string? type = null,
        string? access = null,
        string? trust = null,
        string? description = "reference Linux settings, do not edit")
        => new() { Name = name, Path = path, Type = type, Access = access, Trust = trust, Description = description };

    // ── The path is made absolute here, or nowhere ────────────────────────────

    /// <summary>Relative to the directory holding the manifest — not to the process's current
    /// directory. This is the exact bug that killed <c>workspace:</c>: a path kept as written, which
    /// only ever worked because startup happened to chdir into the right place.</summary>
    [Fact]
    public void A_relative_target_is_resolved_against_the_manifest_not_the_current_directory()
    {
        var mount = Resolve(Section(path: "../reference")).Single();

        Assert.Equal(Path.GetFullPath(Path.Combine(_stage, "reference")), mount.HostPath);
        Assert.True(Path.IsPathRooted(mount.HostPath));
    }

    [Fact]
    public void An_absolute_target_is_kept()
    {
        var elsewhere = Path.Combine(_stage, "elsewhere");
        Assert.Equal(Path.GetFullPath(elsewhere), Resolve(Section(path: elsewhere)).Single().HostPath);
    }

    // ── The reserved name ─────────────────────────────────────────────────────

    /// <summary>Checked whether or not anything is mounted, deliberately: one condition for the life
    /// of the project means adding a mount never re-opens the question of what is already in the
    /// tree. The alternative — check each name on load — fires when somebody else's commit lands a
    /// folder in the root, and refusing to open a project over that is not a price worth paying.
    /// </summary>
    [Fact]
    public void A_real_mnt_folder_in_the_root_refuses_the_project_even_with_nothing_mounted()
    {
        Directory.CreateDirectory(Path.Combine(_root, ProjectMount.Prefix));

        var ex = Assert.Throws<ProjectManifestException>(
            () => MountResolver.Resolve(null, _root, _manifest));

        Assert.Contains("reserved name", ex.Reason);
        Assert.Contains("mnt/<name>/", ex.Reason);
        Assert.Contains(_manifest, ex.Message);
    }

    [Fact]
    public void A_root_with_no_mnt_folder_and_no_mounts_resolves_to_nothing_at_all()
        => Assert.Empty(MountResolver.Resolve(null, _root, _manifest));

    // ── Refusals ──────────────────────────────────────────────────────────────

    [Fact]
    public void A_duplicate_name_is_refused_because_one_address_cannot_have_two_targets()
    {
        var ex = Assert.Throws<ProjectManifestException>(
            () => Resolve(Section(path: "../one"), Section(path: "../two")));

        Assert.Contains("declared twice", ex.Reason);
    }

    [Fact]
    public void Names_that_differ_only_in_case_are_the_same_address()
    {
        var ex = Assert.Throws<ProjectManifestException>(
            () => Resolve(Section(name: "AAA", path: "../one"), Section(name: "aaa", path: "../two")));

        Assert.Contains("declared twice", ex.Reason);
    }

    /// <summary>Otherwise one file has two addresses — <c>mnt/AAA/x</c> and its ordinary project
    /// path — and every rule written on either of them holds only half the time.</summary>
    [Theory]
    [InlineData("docs")]
    [InlineData("./")]
    [InlineData("a/../b/..")]
    public void A_target_inside_the_root_is_refused_as_degenerate(string path)
    {
        var ex = Assert.Throws<ProjectManifestException>(() => Resolve(Section(path: path)));

        Assert.Contains("inside the project root", ex.Reason);
        Assert.Contains("two addresses", ex.Reason);
    }

    [Fact]
    public void An_unknown_type_is_refused_and_names_the_only_one_there_is()
    {
        var ex = Assert.Throws<ProjectManifestException>(() => Resolve(Section(type: "ssh")));

        Assert.Contains("the only mount type is 'file-system'", ex.Reason);
    }

    [Fact]
    public void A_missing_description_is_refused_because_it_goes_into_the_prompt()
    {
        var ex = Assert.Throws<ProjectManifestException>(() => Resolve(Section(description: "  ")));

        Assert.Contains("no description", ex.Reason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void A_mount_with_no_name_is_refused(string? name)
    {
        var ex = Assert.Throws<ProjectManifestException>(() => Resolve(Section(name: name)));

        Assert.Contains("no name", ex.Reason);
    }

    [Theory]
    [InlineData("a/b")]
    [InlineData(@"a\b")]
    [InlineData("..")]
    [InlineData("C:")]
    public void A_name_that_is_not_one_plain_segment_is_refused(string name)
    {
        var ex = Assert.Throws<ProjectManifestException>(() => Resolve(Section(name: name)));

        Assert.Contains("one plain segment", ex.Reason);
    }

    [Fact]
    public void A_mount_with_no_path_is_refused()
    {
        var ex = Assert.Throws<ProjectManifestException>(() => Resolve(Section(path: null)));

        Assert.Contains("no path", ex.Reason);
    }

    [Theory]
    [InlineData("access", "rw")]
    [InlineData("trust", "maybe")]
    public void A_word_that_is_not_one_of_the_two_is_refused_and_both_are_named(string field, string value)
    {
        var section = field == "access" ? Section(access: value) : Section(trust: value);

        var ex = Assert.Throws<ProjectManifestException>(() => Resolve(section));

        Assert.Contains(value, ex.Reason);
        Assert.Contains(field == "access" ? "'read' or 'write'" : "'trusted' or 'untrusted'", ex.Reason);
    }

    // ── Defaults ──────────────────────────────────────────────────────────────

    /// <summary>Read-only unless opted in, the same as <c>allow_write</c> on an SSH host: the
    /// operator opens a folder up deliberately.</summary>
    [Fact]
    public void Access_defaults_to_read()
        => Assert.Equal(MountAccess.Read, Resolve(Section()).Single().Access);

    /// <summary>A mount is a source the operator named — the same standing an SSH host or a database
    /// connection gets. Distrusting your own infrastructure by default is the paranoia that makes a
    /// product unusable while buying nothing.</summary>
    [Fact]
    public void Trust_defaults_to_trusted()
        => Assert.Equal(MountTrust.Trusted, Resolve(Section()).Single().Trust);

    [Fact]
    public void An_absent_type_means_the_only_type_there_is()
        => Assert.Single(Resolve(Section(type: null)));

    [Theory]
    [InlineData("write", MountAccess.Write)]
    [InlineData("WRITE", MountAccess.Write)]
    [InlineData("read", MountAccess.Read)]
    public void Access_is_read_however_it_is_cased(string written, MountAccess expected)
        => Assert.Equal(expected, Resolve(Section(access: written)).Single().Access);

    [Fact]
    public void Untrusted_is_carried_through_for_the_one_case_that_needs_it()
        => Assert.Equal(MountTrust.Untrusted, Resolve(Section(trust: "untrusted")).Single().Trust);

    // ── A missing target is not a refusal ─────────────────────────────────────

    /// <summary>The project still opens. Refusing to open it because one folder is unplugged puts
    /// the failure at the worst possible moment; saying so at the point of use is a better answer,
    /// and the mount carries the flag that lets the gate say it.</summary>
    [Fact]
    public void A_target_that_is_not_on_this_machine_is_marked_unavailable_rather_than_refused()
    {
        var mount = Resolve(Section(path: "../not-here")).Single();

        Assert.False(mount.IsAvailable);
        Assert.Equal(Path.GetFullPath(Path.Combine(_stage, "not-here")), mount.HostPath);
    }

    [Fact]
    public void A_target_that_is_there_is_marked_available()
    {
        Directory.CreateDirectory(Path.Combine(_stage, "reference"));

        Assert.True(Resolve(Section(path: "../reference")).Single().IsAvailable);
    }

    // ── Order and shape ───────────────────────────────────────────────────────

    [Fact]
    public void Mounts_keep_the_order_the_manifest_listed_them_in()
    {
        var mounts = Resolve(
            Section(name: "AAA", path: "../one"),
            Section(name: "BBB", path: "../two"),
            Section(name: "CCC", path: "../three"));

        Assert.Equal(["AAA", "BBB", "CCC"], mounts.Select(m => m.Name));
    }

    [Fact]
    public void Whitespace_around_the_written_values_is_not_part_of_them()
    {
        var mount = Resolve(Section(name: "  AAA  ", access: " write ", description: " why  ")).Single();

        Assert.Equal("AAA", mount.Name);
        Assert.Equal(MountAccess.Write, mount.Access);
        Assert.Equal("why", mount.Description);
    }
}
