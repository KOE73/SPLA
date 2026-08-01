using System.Formats.Tar;
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
        var resolved = LocalTarget.Resolve(_root, path);
        Assert.StartsWith(Path.GetFullPath(_root), resolved, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("../outside.tar")]
    [InlineData("staging/../../outside.tar")]
    [InlineData(@"C:\Windows\system32\drivers\etc\hosts")]
    [InlineData(@"\\server\share\x.tar")]
    public void Local_paths_that_leave_the_project_are_refused(string path)
        => Assert.Throws<InvalidOperationException>(() => LocalTarget.Resolve(_root, path));

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
}
