using System.Collections.Concurrent;

namespace SPLA.Domain.Project;

/// <summary>
/// A fully in-memory project backend: buckets are <see cref="MemoryBucket"/>s, nothing is persisted
/// to disk. Two roles:
/// <list type="bullet">
/// <item>the deterministic backend for tests (no temp folders, no cleanup);</item>
/// <item>the reference/template for a real server backend — a <c>ServerProjectBackend</c> (DB rows,
/// object storage, per-user cloud folder) implements the very same <see cref="IProjectBackend"/>
/// surface, differing only in where <see cref="IBucket"/> reads/writes land. Anything that runs
/// over this backend is already backend-agnostic and will run over the server one.</item>
/// </list>
/// </summary>
public sealed class MemoryProjectBackend : IProjectBackend
{
    private readonly ConcurrentDictionary<string, MemoryBucket> _buckets = new();

    public MemoryProjectBackend(string projectId = "(memory)") => ProjectId = projectId;

    public string ProjectId { get; }

    public IBucket GetBucket(string name) => _buckets.GetOrAdd(name, n => new MemoryBucket(n));
}
