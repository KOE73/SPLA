using SPLA.Domain.Host;
using System.Text.RegularExpressions;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using SPLA.Domain.Secrets;

namespace SPLA.Plugins.Ssh.Transfer;

/// <summary>One thing that went wrong for one entry, without failing the whole transfer.</summary>
public sealed record TransferProblem(string Path, string Reason);

/// <summary>Outcome of a transfer. Partial success is first-class: a single unreadable file in
/// <c>/etc</c> must not throw away the other three hundred that copied fine.</summary>
public sealed record TransferResult(
    string Target,
    int Transferred,
    int Skipped,
    long BytesTransferred,
    IReadOnlyList<TransferProblem> Problems);

/// <summary>
/// File transfer over the SFTP subsystem: listing, download and upload, with no shell involved.
/// <para>
/// Going through SFTP rather than <c>scp</c>/<c>cat</c> is the whole point — no quoting, no Windows
/// backslashes fed to a Linux shell, no dependence on which commands the host allows, and real
/// structured errors instead of parsed text. It also means the <see cref="ReadOnlyGuard"/> never
/// sees these operations: reading is consistent with what the guard is for (it prevents CHANGE, not
/// reading), while anything that writes to the remote host is gated on the host's
/// <see cref="SshHostConfig.AllowWrite"/> instead.
/// </para>
/// </summary>
public sealed class SftpTransfer
{
    // NAIVE: fixed budgets rather than settings. They exist to turn "download /" from a wedged agent
    // and a full disk into an immediate, explanatory refusal. Raise them when a real case needs it.
    private const long MaxTotalBytes = 512L * 1024 * 1024;
    private const long MaxFileBytes = 128L * 1024 * 1024;
    private const int MaxEntries = 5000;

    private readonly Func<SshSettings> _settings;
    private readonly ISecretResolver _resolver;
    /// <summary>The project's boundary, not a root string to build one from. A transfer that carried
    /// its own idea of "inside" was the last place this seam held in three directions out of four.</summary>
    private readonly PathBoundary _boundary;

    public SftpTransfer(Func<SshSettings> settings, ISecretResolver resolver, PathBoundary boundary)
    {
        _settings = settings;
        _resolver = resolver;
        _boundary = boundary;
    }

    public (string Name, SshHostConfig Config) ResolveHost(string? requested)
    {
        var settings = _settings();
        var name = requested ?? settings.DefaultHost
            ?? (settings.Hosts.Count == 1 ? settings.Hosts.Keys.First() : null)
            ?? throw new InvalidOperationException("no host given and no default_host configured");

        return settings.Hosts.TryGetValue(name, out var cfg)
            ? (name, cfg)
            : throw new InvalidOperationException($"unknown host '{name}' — configured hosts: {string.Join(", ", settings.Hosts.Keys)}");
    }

    private Task<SftpClient> ConnectAsync(SshHostConfig cfg, CancellationToken ct)
        => SshConnectionFactory.ConnectSftpAsync(cfg, _settings().TimeoutSeconds, _resolver, ct);

    // ── listing ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<TransferEntry>> ListAsync(
        SshHostConfig cfg, string remotePath, bool recursive, string? pattern, int limit, CancellationToken ct)
    {
        using var sftp = await ConnectAsync(cfg, ct);
        var root = ResolveRemotePath(sftp, remotePath);

        var attributes = sftp.GetAttributes(root);
        if (!attributes.IsDirectory)
            return new[] { ToEntry(root, root, attributes, null) };

        var matcher = BuildMatcher(pattern);
        var found = new List<TransferEntry>();
        Walk(sftp, root, recursive, matcher, includeDirectories: true, found, new List<TransferProblem>(), limit, ct);
        return await FillLinkTargetsAsync(cfg, found, ct);
    }

    /// <summary>Replaces link entries with ones that know their target, in a single round trip.</summary>
    private async Task<List<TransferEntry>> FillLinkTargetsAsync(
        SshHostConfig cfg, List<TransferEntry> entries, CancellationToken ct)
    {
        var links = entries.Where(e => e.Type == TransferEntryType.SymbolicLink).ToList();
        if (links.Count == 0) return entries;

        var targets = await ResolveLinkTargetsAsync(cfg, links.Select(l => "/" + l.Path).ToList(), ct);
        return entries
            .Select(e => e.Type == TransferEntryType.SymbolicLink && targets.TryGetValue("/" + e.Path, out var target)
                ? e with { LinkTarget = target }
                : e)
            .ToList();
    }

    // ── download ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Copies a remote file or tree to a local destination. The destination decides the shape and
    /// nothing else does: a path ending in <c>.tar</c> is a container, anything else is a folder.
    /// </summary>
    public async Task<TransferResult> DownloadAsync(
        SshHostConfig cfg, string remotePath, string localPath, bool recursive,
        string? include, string? exclude, bool overwrite, bool append, CancellationToken ct)
    {
        using var sftp = await ConnectAsync(cfg, ct);
        var root = ResolveRemotePath(sftp, remotePath);
        var rootAttributes = sftp.GetAttributes(root);

        var problems = new List<TransferProblem>();
        var entries = new List<TransferEntry>();

        if (rootAttributes.IsDirectory)
        {
            var matcher = BuildMatcher(include);
            var excluder = BuildMatcher(exclude);
            Walk(sftp, root, recursive, matcher, includeDirectories: true, entries, problems, MaxEntries, ct);
            if (excluder is not null)
                entries.RemoveAll(e => excluder.IsMatch(e.Path));
            entries = await FillLinkTargetsAsync(cfg, entries, ct);
        }
        else
        {
            entries.Add(ToEntry(root, root, rootAttributes, null));
        }

        var toContainer = TarContainer.IsContainerPath(localPath);
        var destination = LocalTarget.Resolve(_boundary, localPath);

        Preflight(entries, destination, toContainer);

        // Reported to the model as the address it can use again, not as this machine's layout: a host
        // path handed back comes straight home as the next call's argument.
        var reported = _boundary.ToCanonical(destination) ?? destination;

        return toContainer
            ? WriteToContainer(sftp, entries, destination, reported, append, problems)
            : WriteToFolder(sftp, entries, destination, reported, overwrite, problems, ct);
    }

    /// <summary>
    /// Everything that must be true BEFORE a single byte moves. Size and free space are checked here
    /// rather than discovered halfway through, and for a folder destination so are the names — a file
    /// Windows cannot store must fail the call, not silently go missing from the result.
    /// </summary>
    private static void Preflight(IReadOnlyList<TransferEntry> entries, string destination, bool toContainer)
    {
        var files = entries.Where(e => e.Type == TransferEntryType.File).ToList();
        if (files.Count == 0 && entries.Count == 0)
            throw new InvalidOperationException("nothing matched — no files to transfer");

        if (entries.Count > MaxEntries)
            throw new InvalidOperationException(
                $"{entries.Count} entries matched, over the {MaxEntries} limit. Narrow the path or use include patterns.");

        var oversized = files.Where(f => f.Size > MaxFileBytes).ToList();
        if (oversized.Count > 0)
            throw new InvalidOperationException(
                $"{oversized.Count} file(s) exceed the {MaxFileBytes / (1024 * 1024)} MB per-file limit, largest: " +
                string.Join(", ", oversized.OrderByDescending(f => f.Size).Take(3).Select(f => $"{f.Path} ({f.Size / (1024 * 1024)} MB)")));

        var total = files.Sum(f => f.Size);
        if (total > MaxTotalBytes)
            throw new InvalidOperationException(
                $"the selection is {total / (1024 * 1024)} MB, over the {MaxTotalBytes / (1024 * 1024)} MB limit. " +
                "Narrow the path or use include patterns.");

        var root = Path.GetPathRoot(Path.GetFullPath(destination));
        if (!string.IsNullOrEmpty(root))
        {
            var free = new DriveInfo(root).AvailableFreeSpace;
            // Ask for headroom: a container rewrite holds the old and the new copy at once.
            var needed = toContainer ? total * 2 + (1L << 20) : total + (1L << 20);
            if (free < needed)
                throw new InvalidOperationException(
                    $"not enough free space on {root}: {free / (1024 * 1024)} MB available, {needed / (1024 * 1024)} MB needed.");
        }

        if (toContainer) return; // a container stores Linux names verbatim — none of the below applies

        var blocked = entries
            .Select(e => (e.Path, Problem: LocalTarget.WindowsNameProblem(e.Path)))
            .Where(x => x.Problem is not null)
            .ToList();
        if (blocked.Count > 0)
            throw new InvalidOperationException(
                $"{blocked.Count} entr(ies) cannot be written into a Windows folder — " +
                $"{string.Join("; ", blocked.Take(3).Select(x => x.Problem))}. " +
                "Give local_path a name ending in .tar to keep them as they are.");

        var collisions = LocalTarget.CaseCollisions(entries.Select(e => e.Path));
        if (collisions.Count > 0)
            throw new InvalidOperationException(
                $"{collisions.Count} name collision(s) — these differ only in case and would overwrite each other on Windows: " +
                $"{string.Join("; ", collisions.Take(3).Select(g => string.Join(" vs ", g)))}. " +
                "Give local_path a name ending in .tar to keep them separate.");

        var links = entries.Where(e => e.Type == TransferEntryType.SymbolicLink).ToList();
        if (links.Count > 0)
            throw new InvalidOperationException(
                $"{links.Count} symbolic link(s) in the selection ({string.Join(", ", links.Take(3).Select(l => l.Path))}) — " +
                "a plain folder cannot hold them. Use a .tar destination, which records them as links.");
    }

    /// <param name="destination">Where the bytes actually go on this machine.</param>
    /// <param name="reported">The same place as the model may name it again — canonical, never the
    /// host layout.</param>
    private static TransferResult WriteToContainer(
        SftpClient sftp, IReadOnlyList<TransferEntry> entries, string destination, string reported,
        bool append, List<TransferProblem> problems)
    {
        var container = new TarContainer(destination);

        // A link whose target the server would not resolve (dangling, or out of reach for this user)
        // cannot be stored as a link — tar has nowhere to put "unknown". Recording it as a problem is
        // honest; inventing a target would put a wrong fact in front of the model later.
        var unresolved = entries
            .Where(e => e.Type == TransferEntryType.SymbolicLink && string.IsNullOrEmpty(e.LinkTarget))
            .ToList();
        foreach (var link in unresolved)
            problems.Add(new TransferProblem(link.Path, "symbolic link skipped — the host would not resolve its target"));
        entries = entries.Except(unresolved).ToList();

        var pending = entries.Select(entry => new PendingEntry(
            entry,
            entry.Type == TransferEntryType.File
                // Opened lazily by the writer: bytes go SFTP → tar without a buffer in between.
                ? () => sftp.OpenRead(entry.Path.StartsWith('/') ? entry.Path : "/" + entry.Path)
                : null));

        container.Write(pending, append);

        var files = entries.Count(e => e.Type == TransferEntryType.File);
        return new TransferResult(
            reported, files, entries.Count - files,
            entries.Where(e => e.Type == TransferEntryType.File).Sum(e => e.Size), problems);
    }

    /// <inheritdoc cref="WriteToContainer"/>
    private static TransferResult WriteToFolder(
        SftpClient sftp, IReadOnlyList<TransferEntry> entries, string destination, string reported,
        bool overwrite, List<TransferProblem> problems, CancellationToken ct)
    {
        Directory.CreateDirectory(destination);
        int transferred = 0, skipped = 0;
        long bytes = 0;

        foreach (var entry in entries)
        {
            ct.ThrowIfCancellationRequested();
            var target = Path.Combine(destination, entry.Path.Replace('/', Path.DirectorySeparatorChar));

            if (entry.Type == TransferEntryType.Directory)
            {
                Directory.CreateDirectory(target);
                continue;
            }
            if (entry.Type != TransferEntryType.File) { skipped++; continue; }

            if (File.Exists(target) && !overwrite) { skipped++; continue; }

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            // Write to .part and rename: an aborted transfer must not leave a truncated file that
            // looks complete — that failure is silent and shows up much later as a broken config.
            var temp = target + ".part";
            try
            {
                using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                    sftp.DownloadFile(entry.Path.StartsWith('/') ? entry.Path : "/" + entry.Path, output);

                File.Move(temp, target, overwrite: true);
                File.SetLastWriteTimeUtc(target, entry.ModifiedUtc.UtcDateTime);
                transferred++;
                bytes += entry.Size;
            }
            catch (Exception ex)
            {
                if (File.Exists(temp)) try { File.Delete(temp); } catch (IOException) { }
                problems.Add(new TransferProblem(entry.Path, ex.Message));
            }
        }

        return new TransferResult(reported, transferred, skipped, bytes, problems);
    }

    // ── upload ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Writes given bytes to one remote path — the tool behind <c>sftp_write_file</c>, for content
    /// that exists only as text or as a blob handle and has no file in the project.
    /// <para>
    /// Writing to a server is exactly what <see cref="SshHostConfig.AllowWrite"/> gates, and SFTP
    /// bypasses the shell guard entirely, so the check has to live here — it is the only thing
    /// standing between an agent and a modified production host.
    /// </para>
    /// </summary>
    public async Task<TransferResult> WriteFileAsync(
        SshHostConfig cfg, string hostName, string remotePath, byte[] content, bool overwrite, CancellationToken ct)
    {
        RequireWritable(cfg, hostName);

        using var sftp = await ConnectAsync(cfg, ct);
        var target = ResolveRemotePath(sftp, remotePath);

        if (!overwrite && sftp.Exists(target))
            throw new InvalidOperationException($"{target} already exists on {hostName} and overwrite is off.");

        EnsureRemoteDirectory(sftp, ParentOf(target), new HashSet<string>(StringComparer.Ordinal));

        using var source = new MemoryStream(content);
        await Task.Run(() => sftp.UploadFile(source, target, overwrite), ct);

        return new TransferResult(target, 1, 0, content.LongLength, Array.Empty<TransferProblem>());
    }

    /// <summary>
    /// Reads one remote file into memory — the read side of <see cref="WriteFileAsync"/>, and what
    /// the <c>sftp://</c> resource provider calls for <c>ResourceVerb.Read</c>.
    /// <para>
    /// <see cref="SftpClient.OpenRead"/> streams lazily, so without a size check first a multi-gigabyte
    /// remote file would still end up whole in the returned <c>byte[]</c> — the same unbounded-slurp
    /// risk <see cref="Preflight"/> exists to catch on the multi-file path, here collapsed to the one
    /// file this call is about. The size comes from the attributes lookup that already has to happen
    /// to reject a directory, so the guard costs nothing extra on the wire.
    /// </para>
    /// </summary>
    public async Task<byte[]> ReadFileAsync(SshHostConfig cfg, string remotePath, CancellationToken ct)
    {
        using var sftp = await ConnectAsync(cfg, ct);
        var target = ResolveRemotePath(sftp, remotePath);

        var attributes = sftp.GetAttributes(target);
        if (attributes.IsDirectory)
            throw new InvalidOperationException($"{target} is a directory, not a file — use sftp_ls or a list read instead.");
        if (attributes.Size > MaxFileBytes)
            throw new InvalidOperationException(
                $"{target} is {attributes.Size / (1024 * 1024)} MB, over the {MaxFileBytes / (1024 * 1024)} MB " +
                "limit for reading a remote file whole into memory. Use sftp_download for a file this size.");

        using var source = sftp.OpenRead(target);
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, ct);
        return buffer.ToArray();
    }

    /// <summary>Whether anything is at this path — the cheap probe behind the resource provider's
    /// <c>ExistsAsync</c>, an attributes lookup rather than a listing or a read.</summary>
    public async Task<bool> ExistsAsync(SshHostConfig cfg, string remotePath, CancellationToken ct)
    {
        using var sftp = await ConnectAsync(cfg, ct);
        var target = ResolveRemotePath(sftp, remotePath);
        return sftp.Exists(target);
    }

    /// <summary>
    /// Sends a file, a folder or a container from the project to the remote host — the mirror of
    /// <see cref="DownloadAsync"/>.
    /// <para>
    /// It is deliberately one call for the whole job. The half-tool that only took bytes forced a
    /// model to assemble the transfer itself — read a file, carry it, send it, repeat per entry —
    /// and a small model loses that thread somewhere in the middle. Whatever can be decided from
    /// the arguments is decided here instead: what the source is, whether the destination is a
    /// directory, which targets already exist, and what to do about them.
    /// </para>
    /// </summary>
    public async Task<TransferResult> UploadAsync(
        SshHostConfig cfg, string hostName, string localPath, string remotePath, bool recursive,
        string? include, string? exclude, UploadConflict conflict, CancellationToken ct)
    {
        RequireWritable(cfg, hostName);

        var set = UploadSource.Build(_boundary, localPath, recursive, BuildMatcher(include), BuildMatcher(exclude));
        PreflightUpload(set);

        using var sftp = await ConnectAsync(cfg, ct);
        var (destination, targetOf) = ResolveUploadDestination(sftp, set, remotePath, localPath);

        var problems = new List<TransferProblem>();
        var plan = PlanUpload(sftp, set, targetOf, conflict, hostName, problems);

        return await Task.Run(() => Send(sftp, plan, set, destination, problems, ct), ct);
    }

    private static void RequireWritable(SshHostConfig cfg, string hostName)
    {
        if (!cfg.AllowWrite)
            throw new InvalidOperationException(
                $"host '{hostName}' is read-only — uploading is refused. The operator must set allow_write: true for this host.");
    }

    /// <summary>Same budgets as a download, applied before connecting: an accidental upload of the
    /// whole project should be a refusal, not a slow disaster on someone's server.</summary>
    private static void PreflightUpload(UploadSet set)
    {
        var files = set.Items.Where(i => i.Type == TransferEntryType.File).ToList();
        if (set.Items.Count == 0)
            throw new InvalidOperationException(
                $"nothing to send from {set.Description} — the source is empty or every entry was filtered out by include/exclude.");

        if (set.Items.Count > MaxEntries)
            throw new InvalidOperationException(
                $"{set.Items.Count} entries selected, over the {MaxEntries} limit. Narrow local_path or use include patterns.");

        var oversized = files.Where(f => f.Size > MaxFileBytes).ToList();
        if (oversized.Count > 0)
            throw new InvalidOperationException(
                $"{oversized.Count} file(s) exceed the {MaxFileBytes / (1024 * 1024)} MB per-file limit, largest: " +
                string.Join(", ", oversized.OrderByDescending(f => f.Size).Take(3).Select(f => $"{f.Path} ({f.Size / (1024 * 1024)} MB)")));

        var total = files.Sum(f => f.Size);
        if (total > MaxTotalBytes)
            throw new InvalidOperationException(
                $"the selection is {total / (1024 * 1024)} MB, over the {MaxTotalBytes / (1024 * 1024)} MB limit. " +
                "Narrow local_path or use include patterns.");
    }

    /// <summary>
    /// Where the entries land. A tree always goes INTO <c>remote_path</c> as a directory. A single
    /// file is the ambiguous case, and it is resolved by looking at the host rather than by adding a
    /// parameter: if the path is an existing directory the file goes inside it under its own name,
    /// otherwise the path IS the file — which is how <c>cp</c> behaves and therefore what the model
    /// has already been trained to expect.
    /// </summary>
    private static (string Destination, Func<UploadItem, string> TargetOf) ResolveUploadDestination(
        SftpClient sftp, UploadSet set, string remotePath, string localPath)
    {
        var root = ResolveRemotePath(sftp, remotePath);
        var rootIsDirectory = TryGetAttributes(sftp, root)?.IsDirectory == true;

        if (set.IsSingleFile && !rootIsDirectory)
            return (root, _ => root);

        if (set.IsSingleFile)
            return (root, item => Join(root, item.Path));

        if (TryGetAttributes(sftp, root) is { IsDirectory: false })
            throw new InvalidOperationException(
                $"{root} is an existing FILE on the host, but '{localPath}' is a tree — remote_path must be a directory.");

        return (root, item => Join(root, item.Path));
    }

    /// <summary>
    /// Decides, before anything is sent, what happens to every entry. Doing the whole decision up
    /// front is what makes <see cref="UploadConflict.Abort"/> possible at all, and it also means the
    /// per-entry rule cannot drift halfway through a transfer.
    /// </summary>
    private static List<(UploadItem Item, string Target, bool Send)> PlanUpload(
        SftpClient sftp, UploadSet set, Func<UploadItem, string> targetOf, UploadConflict conflict,
        string hostName, List<TransferProblem> problems)
    {
        var plan = new List<(UploadItem, string, bool)>(set.Items.Count);
        var existing = new List<string>();

        foreach (var item in set.Items)
        {
            var target = targetOf(item);
            if (item.Type == TransferEntryType.Directory)
            {
                plan.Add((item, target, true)); // creating a directory that exists is a no-op, never a conflict
                continue;
            }

            var attributes = TryGetAttributes(sftp, target);
            if (attributes is null) { plan.Add((item, target, true)); continue; }

            existing.Add(target);
            var send = conflict switch
            {
                UploadConflict.Overwrite => true,
                UploadConflict.Skip => false,
                UploadConflict.Abort => false,
                UploadConflict.Newer => item.ModifiedUtc.UtcDateTime
                    > DateTime.SpecifyKind(attributes.LastWriteTime, DateTimeKind.Utc),
                _ => true
            };

            if (!send && conflict == UploadConflict.Newer)
                problems.Add(new TransferProblem(item.Path, "kept — the copy on the host is the same age or newer"));

            plan.Add((item, target, send));
        }

        if (conflict == UploadConflict.Abort && existing.Count > 0)
            throw new InvalidOperationException(
                $"nothing was sent: {existing.Count} of the selected file(s) already exist on {hostName} " +
                $"({string.Join(", ", existing.Take(3))}{(existing.Count > 3 ? ", …" : "")}). " +
                "That is what on_conflict='abort' asks for — choose 'overwrite', 'skip' or 'newer' to proceed.");

        return plan;
    }

    private static TransferResult Send(
        SftpClient sftp, List<(UploadItem Item, string Target, bool Send)> plan, UploadSet set,
        string destination, List<TransferProblem> problems, CancellationToken ct)
    {
        var created = new HashSet<string>(StringComparer.Ordinal);
        int transferred = 0, skipped = 0;
        long bytes = 0;

        foreach (var (item, target, send) in plan)
        {
            ct.ThrowIfCancellationRequested();
            if (!send) { skipped++; continue; }

            try
            {
                switch (item.Type)
                {
                    case TransferEntryType.Directory:
                        EnsureRemoteDirectory(sftp, target, created);
                        break;

                    case TransferEntryType.SymbolicLink:
                        EnsureRemoteDirectory(sftp, ParentOf(target), created);
                        if (string.IsNullOrEmpty(item.LinkTarget))
                            throw new InvalidOperationException("the container recorded no target for this link");
                        if (sftp.Exists(target)) sftp.DeleteFile(target); // a link cannot be overwritten in place
                        sftp.SymbolicLink(item.LinkTarget, target);
                        skipped++;
                        break;

                    default:
                        EnsureRemoteDirectory(sftp, ParentOf(target), created);
                        using (var content = item.Open!())
                            sftp.UploadFile(content, target, canOverride: true);
                        ApplyMetadata(sftp, target, item, set.ModeIsRecorded);
                        transferred++;
                        bytes += item.Size;
                        break;
                }
            }
            catch (Exception ex)
            {
                problems.Add(new TransferProblem(item.Path, ex.Message));
            }
        }

        return new TransferResult(destination, transferred, skipped, bytes, problems);
    }

    /// <summary>
    /// Modification time is always carried across — without it <see cref="UploadConflict.Newer"/>
    /// would compare the local file against the moment of the last upload and answer nonsense. The
    /// permission mode is only set when it CAME from a Linux host (a container); for a Windows file
    /// there is no mode to carry and the server's umask is the honest default.
    /// </summary>
    private static void ApplyMetadata(SftpClient sftp, string target, UploadItem item, bool modeIsRecorded)
    {
        try
        {
            var attributes = sftp.GetAttributes(target);
            attributes.LastWriteTime = item.ModifiedUtc.UtcDateTime;
            sftp.SetAttributes(target, attributes);

            if (modeIsRecorded && item.Mode != UnixFileMode.None)
                sftp.ChangePermissions(target, (short)ToOctal(item.Mode));
        }
        catch (Exception)
        {
            // Metadata is a nicety; a server that refuses it has still received the file.
        }
    }

    private static int ToOctal(UnixFileMode mode)
    {
        var bits = 0;
        if (mode.HasFlag(UnixFileMode.UserRead)) bits |= 0x100;
        if (mode.HasFlag(UnixFileMode.UserWrite)) bits |= 0x080;
        if (mode.HasFlag(UnixFileMode.UserExecute)) bits |= 0x040;
        if (mode.HasFlag(UnixFileMode.GroupRead)) bits |= 0x020;
        if (mode.HasFlag(UnixFileMode.GroupWrite)) bits |= 0x010;
        if (mode.HasFlag(UnixFileMode.GroupExecute)) bits |= 0x008;
        if (mode.HasFlag(UnixFileMode.OtherRead)) bits |= 0x004;
        if (mode.HasFlag(UnixFileMode.OtherWrite)) bits |= 0x002;
        if (mode.HasFlag(UnixFileMode.OtherExecute)) bits |= 0x001;
        return bits;
    }

    /// <summary>SFTP has no <c>mkdir -p</c>: each level has to be created in turn. The cache is not
    /// an optimisation for its own sake — without it a thousand-file tree asks the server about the
    /// same parent directory a thousand times.</summary>
    private static void EnsureRemoteDirectory(SftpClient sftp, string path, HashSet<string> created)
    {
        if (path.Length == 0 || path == "/" || !created.Add(path)) return;

        var parent = ParentOf(path);
        if (parent.Length > 0 && parent != "/") EnsureRemoteDirectory(sftp, parent, created);

        if (sftp.Exists(path)) return;
        try { sftp.CreateDirectory(path); }
        catch (Exception) when (sftp.Exists(path)) { /* another writer got there first */ }
    }

    private static string ParentOf(string path)
    {
        var cut = path.LastIndexOf('/');
        return cut <= 0 ? "/" : path[..cut];
    }

    private static string Join(string root, string relative)
        => root.TrimEnd('/') + "/" + relative.TrimStart('/');

    /// <summary>Attributes, or null when there is nothing there. SSH.NET answers "does not exist"
    /// by throwing, and an upload asks that question about every single target.</summary>
    private static SftpFileAttributes? TryGetAttributes(SftpClient sftp, string path)
    {
        try { return sftp.Exists(path) ? sftp.GetAttributes(path) : null; }
        catch (Exception) { return null; }
    }

    // ── remote walking ───────────────────────────────────────────────────────

    /// <summary>
    /// Depth-first walk collecting metadata only. Symlinked directories are recorded but never
    /// entered — that is both the cycle guard (<c>/proc</c>-style loops cannot happen) and the
    /// honest reading of a link: it is a fact about the source host, not a second copy of a subtree.
    /// </summary>
    private static void Walk(
        SftpClient sftp, string directory, bool recursive, Regex? matcher, bool includeDirectories,
        List<TransferEntry> found, List<TransferProblem> problems, int limit, CancellationToken ct)
    {
        if (found.Count >= limit) return;
        ct.ThrowIfCancellationRequested();

        IEnumerable<ISftpFile> children;
        try
        {
            children = sftp.ListDirectory(directory);
        }
        catch (Exception ex)
        {
            // Permission denied on one subtree is normal on a real server: record and keep walking.
            problems.Add(new TransferProblem(directory, ex.Message));
            return;
        }

        foreach (var child in children)
        {
            if (found.Count >= limit) return;
            if (child.Name is "." or "..") continue;

            var full = child.FullName;
            var relative = TarContainer.Normalize(full);

            if (child.IsSymbolicLink)
            {
                // Target is filled in afterwards, in one batch — see ResolveLinkTargetsAsync.
                if (matcher is null || matcher.IsMatch(relative))
                    found.Add(ToEntry(full, full, child.Attributes, null));
                continue;
            }

            if (child.IsDirectory)
            {
                if (includeDirectories && (matcher is null || matcher.IsMatch(relative)))
                    found.Add(ToEntry(full, full, child.Attributes, null));
                if (recursive)
                    Walk(sftp, full, recursive, matcher, includeDirectories, found, problems, limit, ct);
                continue;
            }

            if (!child.IsRegularFile) continue; // sockets, fifos, devices — never configuration
            if (matcher is not null && !matcher.IsMatch(relative)) continue;
            found.Add(ToEntry(full, full, child.Attributes, null));
        }
    }

    /// <summary>
    /// Fills in where each symbolic link points, for links found during the walk.
    /// <para>
    /// SSH.NET's SFTP API cannot read a link target at all — it can only create links — so this is
    /// the one place file transfer borrows the shell, with a single <c>readlink</c> for the whole
    /// batch and only when the selection actually contains links. <c>readlink</c> is a read-only
    /// command (it is in the guard's allowlist), so this does not widen what a transfer can do on a
    /// read-only host; it just recovers a fact SFTP refuses to report. The alternative was dropping
    /// symlinks from the set, which for configuration is where half the meaning lives —
    /// <c>sites-enabled/app</c> pointing at <c>../sites-available/app</c> IS the configuration.
    /// </para>
    /// <para>
    /// NAIVE: one command with a per-path fallback so the output has exactly one line per link;
    /// a host without <c>readlink</c> simply yields blanks and those links become problems.
    /// </para>
    /// </summary>
    private async Task<IReadOnlyDictionary<string, string>> ResolveLinkTargetsAsync(
        SshHostConfig cfg, IReadOnlyList<string> absolutePaths, CancellationToken ct)
    {
        var resolved = new Dictionary<string, string>(StringComparer.Ordinal);
        if (absolutePaths.Count == 0) return resolved;

        var script = string.Join("; ", absolutePaths.Select(p =>
            $"readlink -- '{p.Replace("'", "'\\''")}' 2>/dev/null || printf '\\n'"));

        try
        {
            using var ssh = await SshConnectionFactory.ConnectAsync(cfg, _settings().TimeoutSeconds, _resolver, ct);
            using var command = ssh.CreateCommand(script);
            var output = await Task.Run(() => command.Execute(), ct);

            var lines = output.Split('\n');
            for (var i = 0; i < absolutePaths.Count && i < lines.Length; i++)
            {
                var target = lines[i].Trim();
                if (target.Length > 0) resolved[absolutePaths[i]] = target;
            }
        }
        catch
        {
            // Shell unavailable or refused: the links stay unresolved and are reported as problems.
        }

        return resolved;
    }

    private static TransferEntry ToEntry(string fullPath, string _, SftpFileAttributes attributes, string? linkTarget)
    {
        var type = attributes.IsSymbolicLink ? TransferEntryType.SymbolicLink
            : attributes.IsDirectory ? TransferEntryType.Directory
            : TransferEntryType.File;

        return new TransferEntry(
            TarContainer.Normalize(fullPath),
            type,
            type == TransferEntryType.File ? attributes.Size : 0,
            ToUnixMode(attributes),
            new DateTimeOffset(DateTime.SpecifyKind(attributes.LastWriteTime, DateTimeKind.Utc)),
            linkTarget);
    }

    private static UnixFileMode ToUnixMode(SftpFileAttributes a)
    {
        var mode = UnixFileMode.None;
        if (a.OwnerCanRead) mode |= UnixFileMode.UserRead;
        if (a.OwnerCanWrite) mode |= UnixFileMode.UserWrite;
        if (a.OwnerCanExecute) mode |= UnixFileMode.UserExecute;
        if (a.GroupCanRead) mode |= UnixFileMode.GroupRead;
        if (a.GroupCanWrite) mode |= UnixFileMode.GroupWrite;
        if (a.GroupCanExecute) mode |= UnixFileMode.GroupExecute;
        if (a.OthersCanRead) mode |= UnixFileMode.OtherRead;
        if (a.OthersCanWrite) mode |= UnixFileMode.OtherWrite;
        if (a.OthersCanExecute) mode |= UnixFileMode.OtherExecute;
        return mode;
    }

    /// <summary>
    /// SFTP has no shell behind it: <c>~</c>, <c>$VAR</c> and <c>*</c> are not expanded by anyone.
    /// A weak model will send <c>~/app</c> anyway, so <c>~</c> is resolved here against the login
    /// directory; globbing is handled separately by matching after listing.
    /// </summary>
    private static string ResolveRemotePath(SftpClient sftp, string remotePath)
    {
        var path = remotePath.Trim().Replace('\\', '/');
        if (path.Length == 0) throw new InvalidOperationException("remote_path is empty.");

        if (path == "~") return sftp.WorkingDirectory;
        if (path.StartsWith("~/", StringComparison.Ordinal))
            path = sftp.WorkingDirectory.TrimEnd('/') + path[1..];
        else if (!path.StartsWith('/'))
            path = sftp.WorkingDirectory.TrimEnd('/') + "/" + path;

        if (path.Contains('*') || path.Contains('?'))
            throw new InvalidOperationException(
                $"remote_path cannot contain wildcards ({path}) — SFTP has no shell to expand them. " +
                "Give the directory and pass the pattern in 'include' instead.");

        return path;
    }

    /// <summary>
    /// NAIVE glob: <c>**</c> spans directories, <c>*</c> and <c>?</c> stay within one segment.
    /// Several comma-separated patterns are allowed; anything fancier belongs in a real matcher.
    /// </summary>
    /// <summary>The glob matcher, exposed for tests so an upload's include/exclude can be pinned to
    /// exactly the semantics a download uses — the two halves matching is the point.</summary>
    internal static Regex? MatcherFor(string? patterns) => BuildMatcher(patterns);

    private static Regex? BuildMatcher(string? patterns)
    {
        if (string.IsNullOrWhiteSpace(patterns)) return null;

        var alternatives = patterns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(TranslateGlob);
        return new Regex("^(?:" + string.Join('|', alternatives) + ")$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }

    private static string TranslateGlob(string glob)
    {
        var escaped = Regex.Escape(glob.Replace('\\', '/'))
            .Replace(@"\*\*/", "(?:.*/)?")
            .Replace(@"\*\*", ".*")
            .Replace(@"\*", "[^/]*")
            .Replace(@"\?", "[^/]");
        // A bare pattern like '*.conf' should match at any depth — that is what the model means.
        return escaped.Contains('/') ? escaped : "(?:.*/)?" + escaped;
    }
}
