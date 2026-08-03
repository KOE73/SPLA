using System.Formats.Tar;

namespace SPLA.Plugins.Ssh.Transfer;

/// <summary>
/// A <c>.tar</c> file used as a CONTAINER for a set of files taken off a Linux host: one file on the
/// Windows disk, a Linux directory tree inside it.
/// <para>
/// Why a container rather than a plain folder — two reasons that survive the fact that we never
/// restore metadata. First, it holds Linux names verbatim: <c>a:b</c>, <c>README</c> next to
/// <c>readme</c>, or <c>aux</c> cannot be written into a Windows folder at all, so those files would
/// simply not arrive. Second, it stays ONE object, so a few hundred foreign config files do not get
/// mixed into the project tree and then found by every later search.
/// </para>
/// <para>
/// NAIVE, by decision: every mutation REWRITES THE WHOLE ARCHIVE (tar has no in-place update — an
/// entry cannot be replaced where it sits). Append and delete therefore cost a full read + full
/// write. At configuration sizes (megabytes) that is irrelevant; on a multi-gigabyte archive it is
/// not, and the fix would be an index + a repack pass rather than a per-call rewrite. The prompt
/// tells the model the same thing.
/// </para>
/// <para>
/// NAIVE: reads scan the archive from the start (no entry index is kept), and <see cref="ReadBytes"/>
/// materialises the entry in memory. Both are fine for config-sized entries and wrong for large ones.
/// </para>
/// </summary>
public sealed class TarContainer
{
    /// <summary>Suffix that makes a local path a container rather than a folder. This is the ONLY
    /// switch between the two — there is no format parameter anywhere, the model picks by naming
    /// its destination.</summary>
    public const string Extension = ".tar";

    private readonly string _file;

    public TarContainer(string hostFilePath) => _file = hostFilePath;

    public string HostPath => _file;

    public bool Exists => File.Exists(_file);

    /// <summary>True when a local path names a container. Case-insensitive: the path is a Windows one.</summary>
    public static bool IsContainerPath(string localPath)
        => localPath.EndsWith(Extension, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Every entry in the archive. Directories are included: an empty <c>conf.d</c> is a fact about
    /// the source host, not noise.
    /// </summary>
    public IReadOnlyList<TransferEntry> List()
    {
        var result = new List<TransferEntry>();
        if (!Exists) return result;

        using var stream = File.OpenRead(_file);
        using var reader = new TarReader(stream);
        while (reader.GetNextEntry() is { } entry)
        {
            var type = FromTarType(entry.EntryType);
            if (type is null) continue; // devices, fifos, sockets — never configuration
            result.Add(new TransferEntry(
                Normalize(entry.Name),
                type.Value,
                entry.Length,
                entry.Mode,
                entry.ModificationTime,
                string.IsNullOrEmpty(entry.LinkName) ? null : entry.LinkName));
        }
        return result;
    }

    /// <summary>Bytes of one entry, or null when the archive has no such file entry.</summary>
    public byte[]? ReadBytes(string entryPath)
    {
        if (!Exists) return null;
        var wanted = Normalize(entryPath);

        using var stream = File.OpenRead(_file);
        using var reader = new TarReader(stream);
        while (reader.GetNextEntry() is { } entry)
        {
            if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile)) continue;
            if (!string.Equals(Normalize(entry.Name), wanted, StringComparison.Ordinal)) continue;
            if (entry.DataStream is null) return Array.Empty<byte>();

            using var buffer = new MemoryStream();
            entry.DataStream.CopyTo(buffer);
            return buffer.ToArray();
        }
        return null;
    }

    /// <summary>
    /// Writes <paramref name="entries"/> into the container. With <paramref name="append"/> the
    /// existing content is kept and an entry with the same path is REPLACED (last write wins);
    /// without it the container is created from scratch, dropping whatever was there.
    /// </summary>
    public void Write(IEnumerable<PendingEntry> entries, bool append)
    {
        var pending = entries.ToList();
        var replaced = new HashSet<string>(pending.Select(p => Normalize(p.Meta.Path)), StringComparer.Ordinal);
        Rewrite(keep: append ? name => !replaced.Contains(name) : null, add: pending);
    }

    /// <summary>
    /// Removes entries by exact path. Returns how many were dropped. Removing a directory does NOT
    /// remove what is under it — the caller decides that, because "delete the whole subtree" is a
    /// different intent from "delete this entry".
    /// </summary>
    public int Delete(IEnumerable<string> entryPaths)
    {
        if (!Exists) return 0;
        var doomed = new HashSet<string>(entryPaths.Select(Normalize), StringComparer.Ordinal);
        if (doomed.Count == 0) return 0;

        var removed = 0;
        Rewrite(keep: name =>
        {
            if (!doomed.Contains(name)) return true;
            removed++;
            return false;
        }, add: null);
        return removed;
    }

    /// <summary>
    /// The single mutation path: build a new archive next to the old one and swap it in. Writing to
    /// a <c>.part</c> file and renaming means an interrupted or failed rewrite leaves the previous
    /// container intact instead of a plausible-looking truncated one.
    /// </summary>
    private void Rewrite(Predicate<string>? keep, IReadOnlyList<PendingEntry>? add)
    {
        var directory = Path.GetDirectoryName(_file);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        var temp = _file + ".part";
        try
        {
            using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new TarWriter(output, TarEntryFormat.Pax))
            {
                if (keep is not null && Exists)
                {
                    using var input = File.OpenRead(_file);
                    using var reader = new TarReader(input);
                    while (reader.GetNextEntry() is { } entry)
                        if (keep(Normalize(entry.Name)))
                            writer.WriteEntry(entry);
                }

                foreach (var item in add ?? Array.Empty<PendingEntry>())
                {
                    // The content stream is opened as late as possible and closed straight away, so a
                    // remote download flows SFTP → tar without the file ever being held in memory.
                    Stream? content = null;
                    try
                    {
                        var entry = ToTarEntry(item, ref content);
                        writer.WriteEntry(entry);
                    }
                    finally
                    {
                        content?.Dispose();
                    }
                }
            }

            File.Move(temp, _file, overwrite: true);
        }
        finally
        {
            if (File.Exists(temp)) TryDelete(temp);
        }
    }

    private static TarEntry ToTarEntry(PendingEntry item, ref Stream? content)
    {
        var meta = item.Meta;
        var tarType = meta.Type switch
        {
            TransferEntryType.Directory => TarEntryType.Directory,
            TransferEntryType.SymbolicLink => TarEntryType.SymbolicLink,
            _ => TarEntryType.RegularFile
        };

        // PAX format on purpose: it stores arbitrary-length UTF-8 names, so a deep path or a
        // non-ASCII filename is not truncated the way the old ustar 100-char field would.
        var entry = new PaxTarEntry(tarType, Normalize(meta.Path))
        {
            Mode = meta.Mode,
            ModificationTime = meta.ModifiedUtc
        };

        if (tarType == TarEntryType.SymbolicLink)
            // tar has no way to say "a link to somewhere unknown", and an empty LinkName is rejected
            // outright — so the caller must resolve the target or not offer the entry at all.
            entry.LinkName = string.IsNullOrEmpty(meta.LinkTarget)
                ? throw new InvalidOperationException($"symbolic link '{meta.Path}' has no target to record")
                : meta.LinkTarget;
        else if (tarType == TarEntryType.RegularFile && item.OpenContent is not null)
            entry.DataStream = content = item.OpenContent();

        return entry;
    }

    /// <summary>Tar entry names have no leading slash and use forward slashes; a stored <c>..</c>
    /// or absolute path would let the source host aim at somewhere on the target, so both are
    /// stripped here rather than trusted.</summary>
    internal static string Normalize(string path)
    {
        var normalized = path.Replace('\\', '/').Trim();
        while (normalized.StartsWith('/')) normalized = normalized[1..];
        var parts = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p != "." && p != "..");
        return string.Join('/', parts);
    }

    private static TransferEntryType? FromTarType(TarEntryType type) => type switch
    {
        TarEntryType.RegularFile or TarEntryType.V7RegularFile => TransferEntryType.File,
        TarEntryType.Directory => TransferEntryType.Directory,
        TarEntryType.SymbolicLink => TransferEntryType.SymbolicLink,
        _ => null
    };

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }
}
