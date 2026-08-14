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
    ///
    /// <para>This is also where a mount's own rules are <b>executed</b>. The boundary only reports
    /// which mount a path fell into; whether an unplugged target is an error and whether this caller
    /// may write there are policy, and the class that says "mechanism only, no policy" is not the
    /// place for them. The role already exists here — the <c>.spla/</c> cutout is enforced on this
    /// same seam.</para>
    /// </summary>
    /// <param name="intent">What the caller is about to do. Only reads and writes are distinguished,
    /// because that is the granularity of the floor: a mount is open for changes or it is not.</param>
    private string Guard(string path, MountAccess intent = MountAccess.Read)
    {
        var landing = _boundary.Resolve(path);

        if (landing.Ok)
        {
            if (landing.Mount is not { } mount) return landing.FullPath;

            // Unplugged, not missing. The two call for different reactions — plug the folder in
            // versus create the file — and collapsing them into "file not found" sends whoever reads
            // it looking for the wrong thing.
            //
            // The one refusal that names a host path, deliberately: "the target is not on this
            // machine" is not actionable without saying which target, the person who can act on it is
            // the one holding the machine, and there is nothing here for a model to feed back — the
            // mount is unreachable either way. Everywhere else, the canonical form.
            if (!mount.IsAvailable)
                throw new PathBoundaryException(PathRefusal.MountUnavailable,
                    $"mount '{mount.Name}' is declared but its target is not on this machine " +
                    $"({mount.HostPath}); nothing under {mount.Address()}/ can be reached until it is.");

            if (intent == MountAccess.Write && mount.Access == MountAccess.Read)
                throw new PathBoundaryException(PathRefusal.MountReadOnly,
                    $"mount '{mount.Name}' is declared read-only, so {mount.Address()}/ cannot be " +
                    "written to. Change its access in the project manifest if that is wrong.");

            return landing.FullPath;
        }

        var (error, refusal) = (landing.Error, landing.Refusal);

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

    /// <summary>The canonical address — <c>mnt/&lt;name&gt;/...</c> for a mount, otherwise relative
    /// to the root. It used to hand back the absolute host path, which made it the inverse of nothing:
    /// a host path returned to the model comes back as an argument, and a host path written into
    /// saved state is what stops a project being portable.</summary>
    public string? MapPathToProject(string hostPath) => _boundary.ToCanonical(hostPath);

    public bool FileExists(string path) => File.Exists(Guard(path));
    public bool DirectoryExists(string path) => Directory.Exists(Guard(path));

    public Task<string[]> ReadAllLinesAsync(string path, CancellationToken ct = default)
        => File.ReadAllLinesAsync(Guard(path), ct);

    public Task<string> ReadAllTextAsync(string path, CancellationToken ct = default)
        => File.ReadAllTextAsync(Guard(path), ct);

    public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken ct = default)
        => File.ReadAllBytesAsync(Guard(path), ct);

    public Task WriteAllTextAsync(string path, string content, CancellationToken ct = default)
        => File.WriteAllTextAsync(Guard(path, MountAccess.Write), content, ct);

    public void DeleteFile(string path) => File.Delete(Guard(path, MountAccess.Write));
    public void CreateDirectory(string path) => Directory.CreateDirectory(Guard(path, MountAccess.Write));
    public IReadOnlyList<string> GetDirectories(string path) => Directory.GetDirectories(Guard(path));
    public IReadOnlyList<string> GetFiles(string path) => Directory.GetFiles(Guard(path));
}
