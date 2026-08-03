namespace SPLA.Plugins.Ssh.Transfer;

/// <summary>What a transferred entry is. Mirrors the three things a Linux config tree is made of;
/// devices, sockets and fifos are deliberately absent — they are never configuration and are skipped.</summary>
public enum TransferEntryType
{
    File,
    Directory,
    SymbolicLink
}

/// <summary>
/// One entry in a transfer set, whether it currently lives on the remote host or inside a container.
/// <para>
/// <see cref="Path"/> is always RELATIVE and slash-separated, with no leading <c>/</c>: the remote
/// <c>/etc/nginx/nginx.conf</c> becomes <c>etc/nginx/nginx.conf</c>. Keeping absolute paths out of
/// the set is what makes it safe to unpack anywhere later — a stored <c>/</c> or <c>..</c> would let
/// the SOURCE host decide where bytes land on the target.
/// </para>
/// <para>
/// <see cref="Mode"/> and <see cref="LinkTarget"/> are recorded as INFORMATION about how the source
/// host was set up (a key at 0600, <c>sites-enabled/x</c> pointing at <c>../sites-available/x</c>),
/// so the model can reason about what to recreate. Nothing here is restored automatically — on a
/// different host the right owner, mode and layout are usually different, and deciding that is the
/// point of the transfer.
/// </para>
/// </summary>
public sealed record TransferEntry(
    string Path,
    TransferEntryType Type,
    long Size,
    UnixFileMode Mode,
    DateTimeOffset ModifiedUtc,
    string? LinkTarget = null);

/// <summary>
/// An entry queued for writing into a container, with a factory for its bytes. The factory is
/// deliberately lazy and returns a fresh stream: it is called at write time and lets a download go
/// remote → container without ever holding the file in memory.
/// </summary>
public sealed record PendingEntry(TransferEntry Meta, Func<Stream>? OpenContent = null);
