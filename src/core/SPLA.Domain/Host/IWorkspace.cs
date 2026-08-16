using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Host;

/// <summary>
/// The file surface an agent sees. Tools operate on <em>logical</em> paths and never touch
/// <see cref="System.IO"/> directly, so the same tool works over a real disk (local), a user
/// folder (server), or a virtual store (tests) — only the implementation changes.
/// <para>
/// <see cref="MapPathToHost"/>/<see cref="MapPathToProject"/> are the core of the contract and are
/// exposed to the model as system tools; a virtual or sandboxed workspace may return <c>null</c>
/// for a path it refuses to reveal or that lives outside its bounds.
/// </para>
/// <para>
/// <b>Before extending this contract, read
/// <c>docs/plans/PLAN_20260814_core_workspace-streams.md</c>.</b> Five things the SFTP transport
/// needs are deliberately absent — a lazy read stream, a write stream with atomic replace, an
/// enumeration carrying metadata, a way to spot a link, and a <em>seekable</em> stream openable more
/// than once. They are absent because two of the answers (does a stream promise seek, does the store
/// promise atomic replace) cannot be given honestly without a non-file store to give them for, and
/// getting them wrong means rewriting every implementation twice. The transport opens
/// <c>FileStream</c> directly in the meantime, on a path this boundary handed it.
/// </para>
/// </summary>
public interface IWorkspace
{
    /// <summary>Logical path → real host address, or <c>null</c> if virtual/denied/outside bounds.</summary>
    string? MapPathToHost(string logicalPath);

    /// <summary>Host address → logical path, or <c>null</c> if it lies outside this workspace.</summary>
    string? MapPathToProject(string hostPath);

    bool FileExists(string path);
    bool DirectoryExists(string path);

    Task<string[]> ReadAllLinesAsync(string path, CancellationToken ct = default);
    Task<string> ReadAllTextAsync(string path, CancellationToken ct = default);
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken ct = default);
    Task WriteAllTextAsync(string path, string content, CancellationToken ct = default);

    void DeleteFile(string path);
    void CreateDirectory(string path);
    IReadOnlyList<string> GetDirectories(string path);
    IReadOnlyList<string> GetFiles(string path);
}
