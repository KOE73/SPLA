using System.Formats.Tar;
using SPLA.Domain.Host;
using SPLA.Plugins.Ssh.Transfer;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Covers the parts of the SFTP transfer that do not need a server: the container itself, and the
/// checks that decide whether a Linux tree can be written into a Windows folder at all. Those checks
/// are the reason a download either arrives whole or fails outright, so they are worth pinning.
/// </summary>
public sealed class SftpTransferTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "spla-sftp-" + Guid.NewGuid().ToString("N")[..8]);

    public SftpTransferTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { }
    }

    /// <summary>The project's boundary, handed to the transfer instead of a root string. The transfer
    /// used to build its own on every call — no cutouts, no mounts — which is how it became the one
    /// way out of an area everything else respected.</summary>
    private PathBoundary Boundary => new(_root);

    private TarContainer NewContainer() => new(Path.Combine(_root, "set.tar"));

    private static PendingEntry TextEntry(string path, string content, UnixFileMode mode = UnixFileMode.UserRead | UnixFileMode.UserWrite)
        => new(
            new TransferEntry(path, TransferEntryType.File, content.Length, mode, DateTimeOffset.UtcNow),
            () => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));

    [Fact]
    public void Container_roundtrips_content_and_metadata()
    {
        var container = NewContainer();
        container.Write(new[] { TextEntry("etc/nginx/nginx.conf", "worker_processes 4;") }, append: false);

        var entries = container.List();
        var entry = Assert.Single(entries);
        Assert.Equal("etc/nginx/nginx.conf", entry.Path);
        Assert.Equal(TransferEntryType.File, entry.Type);
        Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, entry.Mode);
        Assert.Equal("worker_processes 4;", System.Text.Encoding.UTF8.GetString(container.ReadBytes("etc/nginx/nginx.conf")!));
    }

    [Fact]
    public void Container_holds_names_a_windows_folder_cannot()
    {
        // The whole reason containers exist: these three would collide or be rejected on disk.
        var container = NewContainer();
        container.Write(new[]
        {
            TextEntry("etc/README", "upper"),
            TextEntry("etc/readme", "lower"),
            TextEntry("etc/od:d", "colon")
        }, append: false);

        var paths = container.List().Select(e => e.Path).ToList();
        Assert.Equal(3, paths.Count);
        Assert.Contains("etc/README", paths);
        Assert.Contains("etc/readme", paths);
        Assert.Contains("etc/od:d", paths);
        Assert.Equal("upper", System.Text.Encoding.UTF8.GetString(container.ReadBytes("etc/README")!));
        Assert.Equal("lower", System.Text.Encoding.UTF8.GetString(container.ReadBytes("etc/readme")!));
    }

    [Fact]
    public void Append_adds_and_replaces_without_losing_the_rest()
    {
        var container = NewContainer();
        container.Write(new[] { TextEntry("a.conf", "one"), TextEntry("b.conf", "two") }, append: false);
        container.Write(new[] { TextEntry("b.conf", "two-edited"), TextEntry("c.conf", "three") }, append: true);

        var paths = container.List().Select(e => e.Path).OrderBy(p => p).ToList();
        Assert.Equal(new[] { "a.conf", "b.conf", "c.conf" }, paths);
        Assert.Equal("two-edited", System.Text.Encoding.UTF8.GetString(container.ReadBytes("b.conf")!));
        Assert.Equal("one", System.Text.Encoding.UTF8.GetString(container.ReadBytes("a.conf")!));
    }

    [Fact]
    public void Delete_removes_only_the_named_entry()
    {
        var container = NewContainer();
        container.Write(new[] { TextEntry("a.conf", "one"), TextEntry("b.conf", "two") }, append: false);

        Assert.Equal(1, container.Delete(new[] { "a.conf" }));
        Assert.Equal(new[] { "b.conf" }, container.List().Select(e => e.Path));
        Assert.Equal(0, container.Delete(new[] { "missing.conf" }));
    }

    [Fact]
    public void Symlinks_are_recorded_as_links()
    {
        var container = NewContainer();
        container.Write(new[]
        {
            new PendingEntry(new TransferEntry(
                "etc/nginx/sites-enabled/app", TransferEntryType.SymbolicLink, 0,
                UnixFileMode.UserRead, DateTimeOffset.UtcNow, "../sites-available/app"))
        }, append: false);

        var entry = Assert.Single(container.List());
        Assert.Equal(TransferEntryType.SymbolicLink, entry.Type);
        Assert.Equal("../sites-available/app", entry.LinkTarget);
    }

    [Fact]
    public void Entry_paths_never_stay_absolute_or_traversing()
    {
        // The escape can come from the remote side, inside a name — so it is stripped on the way in.
        Assert.Equal("etc/passwd", TarContainer.Normalize("/etc/passwd"));
        Assert.Equal("etc/passwd", TarContainer.Normalize("../../etc/passwd"));
        Assert.Equal("a/b", TarContainer.Normalize("a/./b"));
    }

    [Fact]
    public void A_failed_rewrite_leaves_no_part_file_behind()
    {
        var container = NewContainer();
        container.Write(new[] { TextEntry("a.conf", "one") }, append: false);

        Assert.Throws<InvalidOperationException>(() => container.Write(new[]
        {
            new PendingEntry(
                new TransferEntry("boom.conf", TransferEntryType.File, 1, UnixFileMode.UserRead, DateTimeOffset.UtcNow),
                () => throw new InvalidOperationException("stream unavailable"))
        }, append: true));

        Assert.False(File.Exists(container.HostPath + ".part"));
        Assert.Equal("one", System.Text.Encoding.UTF8.GetString(container.ReadBytes("a.conf")!));
    }

    [Fact]
    public void Container_is_pax_so_long_and_unicode_names_are_not_truncated()
    {
        var deep = string.Join('/', Enumerable.Repeat("каталог-с-длинным-именем", 6)) + "/файл.conf";
        var container = NewContainer();
        container.Write(new[] { TextEntry(deep, "x") }, append: false);

        Assert.Equal(deep, Assert.Single(container.List()).Path);

        using var stream = File.OpenRead(container.HostPath);
        using var reader = new TarReader(stream);
        Assert.Equal(TarEntryFormat.Pax, reader.GetNextEntry()!.Format);
    }

    // ── local destination checks ─────────────────────────────────────────────

    [Theory]
    [InlineData("staging/prod.tar")]
    [InlineData("a/b/c.conf")]
    public void Local_paths_resolve_under_the_project(string path)
    {
        var resolved = LocalTarget.Resolve(Boundary, path);
        Assert.StartsWith(Path.GetFullPath(_root), resolved, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("../outside.tar")]
    [InlineData("staging/../../outside.tar")]
    [InlineData(@"C:\Windows\system32\drivers\etc\hosts")]
    [InlineData(@"\\server\share\x.tar")]
    public void Local_paths_that_leave_the_project_are_refused(string path)
        => Assert.Throws<InvalidOperationException>(() => LocalTarget.Resolve(Boundary, path));

    [Theory]
    [InlineData("etc/od:d", true)]
    [InlineData("etc/aux", true)]
    [InlineData("etc/trailing.", true)]
    [InlineData("etc/nginx/nginx.conf", false)]
    public void Windows_hostile_names_are_reported(string path, bool expectProblem)
        => Assert.Equal(expectProblem, LocalTarget.WindowsNameProblem(path) is not null);

    [Fact]
    public void Case_only_differences_are_reported_as_collisions()
    {
        var collisions = LocalTarget.CaseCollisions(new[] { "etc/README", "etc/readme", "etc/other" });
        var group = Assert.Single(collisions);
        Assert.Equal(new[] { "etc/README", "etc/readme" }, group);
    }

    // ── upload sources ───────────────────────────────────────────────────────
    // The set an upload sends is decided entirely locally, so it is testable without a server —
    // and it is the part a small model depends on: one call has to expand into the whole tree.

    private void WriteLocal(string relative, string content)
    {
        var full = Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
    }

    private static System.Text.RegularExpressions.Regex? Glob(string? patterns)
        => patterns is null ? null : SftpTransfer.MatcherFor(patterns);

    [Fact]
    public void A_folder_expands_into_its_whole_tree_in_one_go()
    {
        WriteLocal("conf/app.yml", "a");
        WriteLocal("conf/sub/db.yml", "b");
        WriteLocal("conf/sub/deep/x.conf", "c");

        var set = UploadSource.Build(Boundary, "conf", recursive: true, null, null);

        Assert.False(set.IsSingleFile);
        Assert.False(set.ModeIsRecorded);
        Assert.Equal(
            new[] { "app.yml", "sub/db.yml", "sub/deep/x.conf" },
            set.Items.Where(i => i.Type == TransferEntryType.File).Select(i => i.Path).OrderBy(p => p));
        // Directories travel too, so an empty conf.d arrives instead of vanishing.
        Assert.Contains(set.Items, i => i.Type == TransferEntryType.Directory && i.Path == "sub");
    }

    [Fact]
    public void Recursive_false_keeps_the_upload_to_the_top_level()
    {
        WriteLocal("conf/app.yml", "a");
        WriteLocal("conf/sub/db.yml", "b");

        var set = UploadSource.Build(Boundary, "conf", recursive: false, null, null);

        Assert.Equal(new[] { "app.yml" }, set.Items.Where(i => i.Type == TransferEntryType.File).Select(i => i.Path));
    }

    [Fact]
    public void Include_and_exclude_narrow_the_set_the_same_way_a_download_does()
    {
        WriteLocal("conf/app.yml", "a");
        WriteLocal("conf/app.log", "b");
        WriteLocal("conf/cache/tmp.yml", "c");

        var set = UploadSource.Build(Boundary, "conf", recursive: true, Glob("*.yml"), Glob("**/cache/**"));

        Assert.Equal(new[] { "app.yml" }, set.Items.Where(i => i.Type == TransferEntryType.File).Select(i => i.Path));
    }

    [Fact]
    public void A_single_file_is_marked_as_one_so_remote_path_can_be_the_file_itself()
    {
        WriteLocal("conf/app.yml", "hello");

        var set = UploadSource.Build(Boundary, "conf/app.yml", recursive: true, null, null);

        Assert.True(set.IsSingleFile);
        var item = Assert.Single(set.Items);
        Assert.Equal("app.yml", item.Path);
        Assert.Equal(5, item.Size);
        using var content = item.Open!();
        Assert.Equal("hello", new StreamReader(content).ReadToEnd());
    }

    [Fact]
    public void A_container_is_sent_back_with_its_links_and_modes()
    {
        var container = NewContainer();
        container.Write(new[]
        {
            TextEntry("etc/nginx/nginx.conf", "worker_processes 4;", UnixFileMode.UserRead | UnixFileMode.UserWrite),
            new PendingEntry(new TransferEntry(
                "etc/nginx/sites-enabled/app", TransferEntryType.SymbolicLink, 0,
                UnixFileMode.UserRead, DateTimeOffset.UtcNow, "../sites-available/app"))
        }, append: false);

        var set = UploadSource.Build(Boundary, "set.tar", recursive: true, null, null);

        // A Linux mode only means something when it CAME from Linux — that is the flag's whole job.
        Assert.True(set.ModeIsRecorded);
        var link = Assert.Single(set.Items, i => i.Type == TransferEntryType.SymbolicLink);
        Assert.Equal("../sites-available/app", link.LinkTarget);
        var file = Assert.Single(set.Items, i => i.Type == TransferEntryType.File);
        Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, file.Mode);
    }

    [Fact]
    public void A_missing_local_path_says_so_instead_of_sending_nothing_quietly()
    {
        var error = Assert.Throws<InvalidOperationException>(
            () => UploadSource.Build(Boundary, "conf", recursive: true, null, null));
        Assert.Contains("no such file or directory", error.Message);
    }

    [Fact]
    public void An_upload_cannot_reach_outside_the_project_either()
        => Assert.Throws<InvalidOperationException>(
            () => UploadSource.Build(Boundary, "../../etc", recursive: true, null, null));

    // ── mounts ───────────────────────────────────────────────────────────────

    /// <summary>
    /// The scenario the whole mount design exists for: a folder of reference configuration that lives
    /// beside the project rather than in it, sent to a server. Before mounts there was no way to name
    /// it — a relative path had no base and an absolute one was refused by the rule right below.
    /// </summary>
    [Fact]
    public void An_upload_can_name_a_declared_mount_and_reach_a_folder_outside_the_project()
    {
        var target = Path.Combine(_root + "-reference");
        Directory.CreateDirectory(Path.Combine(target, "nginx"));
        File.WriteAllText(Path.Combine(target, "nginx", "nginx.conf"), "worker_processes 4;");

        try
        {
            var set = UploadSource.Build(MountedBoundary(target), "mnt/AAA/nginx", recursive: true, null, null);

            var file = Assert.Single(set.Items, i => i.Type == TransferEntryType.File);
            Assert.Equal("nginx.conf", file.Path);
            using var content = file.Open!();
            Assert.Equal("worker_processes 4;", new StreamReader(content).ReadToEnd());
        }
        finally
        {
            try { Directory.Delete(target, recursive: true); } catch (IOException) { }
        }
    }

    /// <summary>The transfer's own rule survives mounts intact: the model still may not name a drive.
    /// <c>mnt/AAA/x</c> gets through because it is an address, not an absolute path — which is exactly
    /// the property that made a reserved prefix worth having.</summary>
    [Fact]
    public void A_mount_address_is_not_an_absolute_path_but_the_target_spelled_out_still_is()
    {
        var target = _root + "-reference";
        Directory.CreateDirectory(target);

        try
        {
            var boundary = MountedBoundary(target);

            Assert.Equal(
                Path.Combine(target, "f.conf"),
                LocalTarget.Resolve(boundary, "mnt/AAA/f.conf"));

            // Naming the same file by its host path is still refused — before the boundary is even asked.
            Assert.Throws<InvalidOperationException>(
                () => LocalTarget.Resolve(boundary, Path.Combine(target, "f.conf")));
        }
        finally
        {
            try { Directory.Delete(target, recursive: true); } catch (IOException) { }
        }
    }

    [Fact]
    public void An_upload_from_a_mount_that_is_not_declared_is_refused()
        => Assert.Throws<InvalidOperationException>(
            () => UploadSource.Build(Boundary, "mnt/AAA/nginx", recursive: true, null, null));

    /// <summary>The project, plus one mount pointing at a sibling folder outside it — the shape a real
    /// manifest produces.</summary>
    private PathBoundary MountedBoundary(string target) =>
        new(_root, null,
            [new ProjectMount("AAA", target, MountAccess.Read, MountTrust.Trusted, "reference settings", true)]);
}
