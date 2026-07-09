using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Host;

/// <summary>
/// Passthrough workspace over the real file system: logical path == host path, no boundary.
/// Behaves exactly like direct <see cref="System.IO"/> use, so wiring tools through it changes
/// nothing locally while opening the seam for server/virtual implementations later.
/// </summary>
public sealed class LocalWorkspace : IWorkspace
{
    public string? MapPathToHost(string logicalPath) => Path.GetFullPath(logicalPath);
    public string? MapPathToProject(string hostPath) => Path.GetFullPath(hostPath);

    public bool FileExists(string path) => File.Exists(path);
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public Task<string[]> ReadAllLinesAsync(string path, CancellationToken ct = default)
        => File.ReadAllLinesAsync(path, ct);

    public Task<string> ReadAllTextAsync(string path, CancellationToken ct = default)
        => File.ReadAllTextAsync(path, ct);

    public Task WriteAllTextAsync(string path, string content, CancellationToken ct = default)
        => File.WriteAllTextAsync(path, content, ct);

    public void DeleteFile(string path) => File.Delete(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public IReadOnlyList<string> GetDirectories(string path) => Directory.GetDirectories(path);
    public IReadOnlyList<string> GetFiles(string path) => Directory.GetFiles(path);
}
