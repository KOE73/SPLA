using System.Linq;
using SPLA.Domain.Resources;
using SPLA.Domain.Host;
using SPLA.Plugins.Ssh.Transfer;

namespace SPLA.Plugins.Ssh.Resources;

/// <summary>
/// <c>sftp://</c> resources: files on a configured SSH host, addressed by host name as the
/// authority — <c>sftp://ioBroker/etc/nginx.conf</c> reaches the same place, the same way, as the
/// <c>sftp_*</c> tools reach it, through the same <see cref="SftpTransfer"/> and the same host
/// lookup. That sharing is the point: a resource address and a transfer tool call must never
/// disagree about what a host name means, and the only way to guarantee that is to have one of them
/// call the other rather than re-implement host resolution twice.
///
/// <para>Delete and mkdir are deliberately not implemented. SFTP already has a
/// <see cref="SshHostConfig.AllowWrite"/> gate the write path depends on, but remote delete has no
/// equivalent safety net yet — that is a decision for its own change, not a side effect of wiring up
/// resources.</para>
/// </summary>
public sealed class SftpResourceProvider : IResourceProvider, IResourceWriter
{
    private readonly SftpTransfer _transfer;

    public SftpResourceProvider(SftpTransfer transfer) => _transfer = transfer;

    public string Scheme => "sftp";

    public string Summary =>
        "files on a configured SSH host — sftp://ioBroker/etc/nginx.conf";

    public async Task<ResourceContent> ReadAsync(ResourceUri uri, CancellationToken ct = default)
    {
        var (host, path) = MapAddress(uri);
        var (_, cfg) = _transfer.ResolveHost(host);
        var bytes = await _transfer.ReadFileAsync(cfg, path, ct);

        // The bytes are already here, so the signature costs nothing — no second round trip to the
        // server just to find out what the first one brought back. The remote path doubles as the
        // file name for the extension fallback.
        return new ResourceContent(bytes, ContentTypes.Resolve(declared: null, bytes, path, isText: false));
    }

    public async Task<bool> ExistsAsync(ResourceUri uri, CancellationToken ct = default)
    {
        var (host, path) = MapAddress(uri);
        var (_, cfg) = _transfer.ResolveHost(host);
        return await _transfer.ExistsAsync(cfg, path, ct);
    }

    /// <summary>Direct children only, matching <c>sftp_ls</c> with <c>recursive: false</c> — a
    /// listing that walked subtrees would silently stop being "one directory" the moment a real
    /// server had a few thousand files somewhere underneath.</summary>
    public async Task<IReadOnlyList<ResourceEntry>> ListAsync(ResourceUri uri, CancellationToken ct = default)
    {
        var (host, path) = MapAddress(uri);
        var (_, cfg) = _transfer.ResolveHost(host);
        var entries = await _transfer.ListAsync(cfg, path, recursive: false, pattern: null, limit: 5000, ct);

        return entries
            .Select(entry => new ResourceEntry(
                LastSegment(entry.Path),
                entry.Type == TransferEntryType.Directory,
                entry.Type == TransferEntryType.File ? entry.Size : null))
            .ToList();
    }

    public async Task WriteAsync(ResourceUri uri, byte[] content, bool overwrite, CancellationToken ct = default)
    {
        var (host, path) = MapAddress(uri);
        var (name, cfg) = _transfer.ResolveHost(host);
        await _transfer.WriteFileAsync(cfg, name, path, content, overwrite, ct);
    }

    private static string LastSegment(string path)
    {
        var slash = path.LastIndexOf('/');
        return slash < 0 ? path : path[(slash + 1)..];
    }

    /// <summary>
    /// The pure address→(host, remote path) mapping, split out from everything that needs a live
    /// connection so it can be unit-tested without one.
    /// <para>
    /// The authority IS the configured host name — <c>sftp://ioBroker/var/log/syslog</c> is host
    /// <c>ioBroker</c>, path <c>/var/log/syslog</c>. <see cref="ResourceUri.Path"/> strips the
    /// leading slash for every scheme uniformly, but SFTP paths are absolute by nature — "etc/nginx"
    /// and "/etc/nginx" are different questions to a real server — so the slash goes back on here
    /// rather than being left to whichever caller remembers to add it.
    /// </para>
    /// </summary>
    internal static (string Host, string RemotePath) MapAddress(ResourceUri uri)
    {
        if (uri.Authority.Length == 0)
            throw new InvalidOperationException(
                "sftp:// needs a configured host as its authority, e.g. sftp://ioBroker/etc/nginx.conf.");

        return (uri.Authority, "/" + uri.Path);
    }
}
