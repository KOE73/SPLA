using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Host;

/// <summary>How hard the ROOT rule bites. Cutouts are unaffected — those are always enforced.</summary>
public enum BoundaryMode
{
    /// <summary>Compute the verdict, record the ones that would have been refused, allow everything.
    /// The point of the exercise: the list of paths a real project legitimately needs outside its
    /// root cannot be guessed, only collected.</summary>
    Shadow,

    /// <summary>Refuse what lands outside.</summary>
    Enforce
}

/// <summary>One path the boundary would have refused. Carried to whoever is counting rather than
/// logged from inside the domain, because the same observations later feed the traffic on the zone
/// map, and a log line is not a number.</summary>
/// <param name="Path">As the caller wrote it.</param>
public readonly record struct BoundaryObservation(string Path, PathRefusal Refusal, string Reason);

/// <summary>
/// Workspace over the real file system, bounded by a <see cref="PathBoundary"/>.
///
/// <para>The boundary lives here rather than in <c>MapPathToHost</c>, because that is not where the
/// traffic is: ten of the twelve tools on this seam call <see cref="FileExists"/> /
/// <see cref="ReadAllTextAsync"/> and friends with a logical path and never map anything. A check in
/// the mapping functions would have guarded two call sites out of twelve.</para>
///
/// <para>Two rules with different force, deliberately. The <b>cutout</b> is decided and enforced now:
/// <c>.spla/</c> holds the chats, the secrets and the accounting, and nothing on the far side of the
/// seam has business there. The <b>root</b> rule starts in <see cref="BoundaryMode.Shadow"/>, because
/// switching it on blind would break the legitimate out-of-root work nobody has enumerated yet —
/// and enumerating it by guesswork is exactly what shadow mode replaces.</para>
/// </summary>
public sealed class LocalWorkspace : IWorkspace
{
    private readonly PathBoundary _boundary;
    private readonly BoundaryMode _mode;
    private readonly Action<BoundaryObservation>? _observer;

    /// <summary>Unbounded, as before: no project, no boundary.</summary>
    public LocalWorkspace() : this(PathBoundary.None) { }

    /// <param name="observer">Called for each path the root rule would refuse while in shadow.</param>
    public LocalWorkspace(
        PathBoundary boundary,
        BoundaryMode mode = BoundaryMode.Shadow,
        Action<BoundaryObservation>? observer = null)
    {
        _boundary = boundary;
        _mode = mode;
        _observer = observer;
    }

    public PathBoundary Boundary => _boundary;
    public BoundaryMode Mode => _mode;

    /// <summary>
    /// The one gate every method below goes through. Returns the real path to use, or throws when
    /// the boundary refuses — a decision, not a fault, which is why it has its own exception type.
    /// </summary>
    private string Guard(string path)
    {
        if (_boundary.TryResolve(path, out var full, out var error, out var refusal))
            return full;

        switch (refusal)
        {
            // Never negotiable: the folder is the application's own.
            case PathRefusal.Cutout:
                throw new PathBoundaryException(refusal, error!);

            // The rule under evaluation. In shadow it is recorded and allowed, so a week of ordinary
            // work produces the list of exceptions instead of a week of broken work.
            case PathRefusal.OutsideRoot:
            case PathRefusal.NetworkShare:
                if (_mode == BoundaryMode.Enforce)
                    throw new PathBoundaryException(refusal, error!);

                _observer?.Invoke(new BoundaryObservation(path, refusal, error!));
                return Path.GetFullPath(path);

            // Empty or malformed input was never allowed by anybody; let it read as the fault it is.
            default:
                throw new PathBoundaryException(refusal, error!);
        }
    }

    public string? MapPathToHost(string logicalPath)
        => _boundary.TryResolve(logicalPath, out var full, out _) ? full : null;

    public string? MapPathToProject(string hostPath)
        => _boundary.TryResolve(hostPath, out var full, out _) ? full : null;

    public bool FileExists(string path) => File.Exists(Guard(path));
    public bool DirectoryExists(string path) => Directory.Exists(Guard(path));

    public Task<string[]> ReadAllLinesAsync(string path, CancellationToken ct = default)
        => File.ReadAllLinesAsync(Guard(path), ct);

    public Task<string> ReadAllTextAsync(string path, CancellationToken ct = default)
        => File.ReadAllTextAsync(Guard(path), ct);

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken ct = default)
        => File.ReadAllBytesAsync(Guard(path), ct);

    public Task WriteAllTextAsync(string path, string content, CancellationToken ct = default)
        => File.WriteAllTextAsync(Guard(path), content, ct);

    public void DeleteFile(string path) => File.Delete(Guard(path));
    public void CreateDirectory(string path) => Directory.CreateDirectory(Guard(path));
    public IReadOnlyList<string> GetDirectories(string path) => Directory.GetDirectories(Guard(path));
    public IReadOnlyList<string> GetFiles(string path) => Directory.GetFiles(Guard(path));
}
