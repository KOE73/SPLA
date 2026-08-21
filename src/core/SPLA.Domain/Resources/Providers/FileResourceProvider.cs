using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Host;

namespace SPLA.Domain.Resources.Providers;

/// <summary>
/// The <c>file</c> scheme: the project's own workspace, addressed the way every filesystem tool
/// already addresses it — a logical path, resolved through <see cref="IWorkspace"/> and therefore
/// through whatever <see cref="PathBoundary"/> and mount table that workspace enforces. This
/// provider adds no rule of its own; it is a second doorway onto the same room
/// <c>system_read_file</c> and friends already use, not a parallel one with its own opinions.
///
/// <para><b>Mounts need no scheme of their own.</b> <c>mnt/&lt;name&gt;/...</c> already lives inside
/// the workspace's logical path space — <see cref="PathBoundary"/> recognises the reserved prefix on
/// its own, before this class ever sees the path — so <c>file:///mnt/AAA/nginx.conf</c> needs
/// nothing beyond the leading slash <see cref="ResourceUri"/> already stripped. Inventing a second
/// mapping here would be the boundary's job done twice, in two places that could disagree about
/// where a mount ends.</para>
///
/// <para><b>Resolved per call, not captured once.</b> The constructor takes a factory rather than an
/// <see cref="IWorkspace"/> because the ambient sandbox is per-chat
/// (<see cref="HostServices.Sandbox"/>) — a provider registered once for the process and holding a
/// workspace from construction time would keep answering for whichever session happened to be
/// current when it was built, which is wrong for every session after that one.</para>
/// </summary>
public sealed class FileResourceProvider
    : IResourceProvider, IResourceWriter, IResourceRemover, IResourceContainerMaker
{
    private readonly Func<IWorkspace> _workspace;

    public FileResourceProvider(Func<IWorkspace> workspace)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
    }

    public string Scheme => "file";

    public string Summary =>
        "project files and declared mounts — file:///src/app.cs, file:///mnt/AAA/nginx.conf";

    public async Task<ResourceContent> ReadAsync(ResourceUri uri, CancellationToken ct = default)
    {
        var path = PathOf(uri);
        var bytes = await Ws().ReadAllBytesAsync(path, ct);

        // Nothing is declared here — the filesystem stores no type — so the answer comes from the
        // bytes first and the name second, which is the only order under which a .txt full of PNG
        // is called a PNG.
        return new ResourceContent(bytes, ContentTypes.Resolve(declared: null, bytes, path, isText: false));
    }

    public Task<bool> ExistsAsync(ResourceUri uri, CancellationToken ct = default)
    {
        var ws = Ws();
        var path = PathOf(uri);
        return Task.FromResult(ws.FileExists(path) || ws.DirectoryExists(path));
    }

    /// <summary>Direct children only — <see cref="IWorkspace.GetDirectories"/> and
    /// <see cref="IWorkspace.GetFiles"/> are themselves non-recursive, so there is nothing extra to
    /// enforce here beyond refusing an address that is not a directory at all.</summary>
    public Task<IReadOnlyList<ResourceEntry>> ListAsync(ResourceUri uri, CancellationToken ct = default)
    {
        var ws = Ws();
        var path = PathOf(uri);

        if (!ws.DirectoryExists(path))
        {
            throw ws.FileExists(path)
                ? new InvalidOperationException(
                    $"'{uri}' names a file, not a directory — list a container, not content.")
                : new DirectoryNotFoundException($"'{uri}' does not exist.");
        }

        // Only the last segment survives into the entry, on purpose: IWorkspace hands back
        // whatever shape of path its own implementation uses (an absolute host path for
        // LocalWorkspace, something else for a virtual store in a test) — the last segment is the
        // one part every implementation agrees on, and Size stays null rather than guessed because
        // IWorkspace exposes no cheap way to ask a store's size without reaching around it (see the
        // note on WriteAsync for the same gap on the write side).
        var entries = ws.GetDirectories(path)
            .Select(d => new ResourceEntry(LastSegment(d), IsContainer: true))
            .Concat(ws.GetFiles(path).Select(f => new ResourceEntry(LastSegment(f), IsContainer: false)))
            .ToList();

        return Task.FromResult<IReadOnlyList<ResourceEntry>>(entries);
    }

    public async Task WriteAsync(ResourceUri uri, byte[] content, bool overwrite, CancellationToken ct = default)
    {
        var ws = Ws();
        var path = PathOf(uri);

        if (!overwrite && ws.FileExists(path))
            throw new InvalidOperationException(
                $"'{uri}' already exists and overwrite was not requested — create-only means refuse, " +
                "not replace.");

        // The same create-parents-first sequence FsWriteTool/FsCreateTool already use: a resource
        // write is meant to feel identical to those tools from the caller's side, not merely
        // equivalent to them.
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !ws.DirectoryExists(dir))
            ws.CreateDirectory(dir);

        // IWorkspace has no byte-writing member, only WriteAllTextAsync(string) — one of the gaps
        // IWorkspace's own remarks already name (docs/plans/PLAN_20260814_core_workspace-streams.md,
        // the missing write stream). So the bytes have to go through UTF-8 to reach the disk.
        //
        // Which is lossless for text and LOSSY for anything else, and that asymmetry is the whole
        // reason for the check below. Writing the mangled bytes and noting the caveat in a comment
        // would make this seam quietly corrupt any binary content handed to it — a data loss whose
        // only symptom is a broken file discovered much later. Refusing is recoverable; corrupting
        // is not, so the content is asked what it is rather than assumed.
        //
        // The refusal is unchanged in what it allows; what changed is that it now knows what it is
        // refusing and says so, instead of reporting a failed round-trip as if the caller had made
        // an encoding mistake.
        if (!ContentTypes.LooksLikeUtf8Text(content))
            throw new NotSupportedException(
                $"'{uri}' was given binary content ({ContentTypes.Resolve(null, content, path, isText: false)}), " +
                "and the workspace backend can only be written as text today — writing it would " +
                "silently corrupt the bytes. Binary writes wait on the write stream in " +
                "docs/plans/PLAN_20260814_core_workspace-streams.md.");

        await ws.WriteAllTextAsync(path, Encoding.UTF8.GetString(content), ct);
    }

    public Task DeleteAsync(ResourceUri uri, CancellationToken ct = default)
    {
        Ws().DeleteFile(PathOf(uri));
        return Task.CompletedTask;
    }

    public Task MakeDirAsync(ResourceUri uri, CancellationToken ct = default)
    {
        Ws().CreateDirectory(PathOf(uri));
        return Task.CompletedTask;
    }

    private IWorkspace Ws() => _workspace();

    /// <summary>
    /// The one rule this scheme still has to check that <see cref="ResourceUri.TryParse"/> did not:
    /// <c>file://</c> names no second instance the way <c>sftp://host</c> does — the project
    /// workspace is the only one there is — so an authority here is not a smaller mistake than a bad
    /// scheme, it is the same kind of mistake, and it is caught before it reaches
    /// <see cref="IWorkspace"/> rather than being silently absorbed into the path.
    /// </summary>
    private static string PathOf(ResourceUri uri)
    {
        if (uri.Authority.Length != 0)
            throw new ArgumentException(
                $"'{uri}' names an authority ('{uri.Authority}'), but file:// has none — the project " +
                "workspace is the only instance. The correct form has three slashes and no host: " +
                "file:///path, e.g. file:///src/app.cs.");

        // "file:///" is the obvious way to name the root, and PathBoundary refuses an empty path
        // outright (PathRefusal.Empty). Left alone, the most natural address a model can write for
        // the project root would be the one address that cannot work, and it would learn otherwise
        // only by being refused — the affordance trap this whole scheme exists to close, reappearing
        // at the very first path anyone tries. So the root is spelled here, once, rather than in
        // every prompt that has to warn about it.
        return uri.Path.Length == 0 ? "." : uri.Path;
    }

    private static string LastSegment(string path)
        => Path.GetFileName(path.TrimEnd('/', '\\'));
}
