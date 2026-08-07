using System.Text.RegularExpressions;

namespace SPLA.Plugins.Ssh.Transfer;

/// <summary>
/// What to do when the remote side already has the file. One named choice instead of a pair of
/// booleans: a weak model picks a value out of a description far more reliably than it reasons about
/// which combination of <c>overwrite</c>/<c>skip</c> flags expresses its intent, and the four cases
/// below are genuinely different intents rather than points on one scale.
/// </summary>
public enum UploadConflict
{
    /// <summary>Replace whatever is there, without asking.</summary>
    Overwrite,

    /// <summary>Leave existing files alone; the count of untouched files is reported.</summary>
    Skip,

    /// <summary>Replace only when the local file is newer than the remote one.</summary>
    Newer,

    /// <summary>Transfer nothing at all if even one target already exists — checked before any byte moves.</summary>
    Abort
}

/// <summary>One thing to send: a file, a directory to create, or a link to recreate.</summary>
/// <param name="Path">Relative, slash-separated, no leading slash — the path under the destination.</param>
/// <param name="Open">Opens the content, lazily, at the moment it is needed. Null for non-files.</param>
internal sealed record UploadItem(
    string Path,
    TransferEntryType Type,
    long Size,
    DateTimeOffset ModifiedUtc,
    UnixFileMode Mode,
    string? LinkTarget,
    Func<Stream>? Open);

/// <summary>
/// The local side of an upload, resolved into a flat set of entries.
/// <para>
/// <see cref="ModeIsRecorded"/> separates the two kinds of source. A container came off a Linux host
/// and its permission bits are real, so they are worth putting back. A Windows file has no Unix mode
/// at all — anything we sent would be invented — so those uploads leave the mode to the server's
/// umask, which is what the login user would have got anyway.
/// </para>
/// </summary>
internal sealed record UploadSet(
    IReadOnlyList<UploadItem> Items,
    bool IsSingleFile,
    bool ModeIsRecorded,
    string Description);

/// <summary>Builds an <see cref="UploadSet"/> from a path inside the project: one file, a whole
/// folder, or a <c>.tar</c> container — decided by what the path actually is, plus the same
/// <c>.tar</c> naming rule the download side uses.</summary>
internal static class UploadSource
{
    public static UploadSet Build(
        string workspaceRoot, string localPath, bool recursive, Regex? include, Regex? exclude)
    {
        var full = LocalTarget.Resolve(workspaceRoot, localPath);

        if (TarContainer.IsContainerPath(localPath))
            return FromContainer(full, localPath, include, exclude);

        if (File.Exists(full))
            return FromFile(full, localPath, include, exclude);

        if (Directory.Exists(full))
            return FromFolder(full, localPath, recursive, include, exclude);

        throw new InvalidOperationException(
            $"nothing at '{localPath}' in the project — no such file or directory.");
    }

    /// <summary>
    /// A container is the reverse of a <c>.tar</c> download: the tree it holds is unpacked onto the
    /// host with its recorded links and modes.
    /// <para>
    /// NAIVE: each entry's bytes are found by scanning the archive again (tar has no index) and held
    /// in memory while that one entry is sent. Fine at configuration sizes, wrong for a container of
    /// hundreds of megabytes — that wants a single streaming pass instead.
    /// </para>
    /// </summary>
    private static UploadSet FromContainer(string full, string localPath, Regex? include, Regex? exclude)
    {
        var container = new TarContainer(full);
        if (!container.Exists)
            throw new InvalidOperationException($"no container at '{localPath}' in the project.");

        var items = container.List()
            .Where(e => Matches(e.Path, include, exclude))
            .Select(e => new UploadItem(
                e.Path, e.Type, e.Size, e.ModifiedUtc, e.Mode, e.LinkTarget,
                e.Type == TransferEntryType.File
                    ? () => new MemoryStream(container.ReadBytes(e.Path) ?? Array.Empty<byte>())
                    : null))
            .ToList();

        return new UploadSet(items, IsSingleFile: false, ModeIsRecorded: true, $"container {localPath}");
    }

    private static UploadSet FromFile(string full, string localPath, Regex? include, Regex? exclude)
    {
        var name = Path.GetFileName(full);
        // include/exclude on a single named file is almost always the model repeating itself; honour
        // it anyway rather than silently ignoring a filter it asked for.
        var items = Matches(name, include, exclude)
            ? new List<UploadItem> { FileItem(full, name) }
            : new List<UploadItem>();

        return new UploadSet(items, IsSingleFile: true, ModeIsRecorded: false, $"file {localPath}");
    }

    private static UploadSet FromFolder(
        string root, string localPath, bool recursive, Regex? include, Regex? exclude)
    {
        var items = new List<UploadItem>();
        Walk(root, root, recursive, include, exclude, items);
        return new UploadSet(items, IsSingleFile: false, ModeIsRecorded: false, $"folder {localPath}");
    }

    private static void Walk(
        string root, string directory, bool recursive, Regex? include, Regex? exclude, List<UploadItem> items)
    {
        foreach (var entry in new DirectoryInfo(directory).EnumerateFileSystemInfos())
        {
            // A junction or a symlink on the Windows side is not a Linux symlink and following it is
            // how a copy turns into a loop. Skipping is the honest reading: the project's own files
            // are what the model meant to send.
            if (entry.Attributes.HasFlag(FileAttributes.ReparsePoint)) continue;

            var relative = Relative(root, entry.FullName);

            if (entry is DirectoryInfo)
            {
                // Directories are carried so an empty conf.d arrives too, but they are not filtered:
                // a pattern like '*.conf' is about files, and dropping the parent would strand them.
                items.Add(new UploadItem(
                    relative, TransferEntryType.Directory, 0, entry.LastWriteTimeUtc,
                    UnixFileMode.None, null, null));
                if (recursive) Walk(root, entry.FullName, recursive, include, exclude, items);
                continue;
            }

            if (!Matches(relative, include, exclude)) continue;
            items.Add(FileItem(entry.FullName, relative));
        }
    }

    private static UploadItem FileItem(string fullPath, string relative)
    {
        var info = new FileInfo(fullPath);
        return new UploadItem(
            relative, TransferEntryType.File, info.Length, info.LastWriteTimeUtc,
            UnixFileMode.None, null,
            () => new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    private static string Relative(string root, string fullPath)
        => TarContainer.Normalize(Path.GetRelativePath(root, fullPath));

    private static bool Matches(string path, Regex? include, Regex? exclude)
        => (include is null || include.IsMatch(path)) && (exclude is null || !exclude.IsMatch(path));
}
