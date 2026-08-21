using System.Collections.Generic;
using System.Linq;

namespace SPLA.Agent;

/// <summary>
/// A bounded, thread-safe, in-memory ring of finished <see cref="SpawnedRun"/>s.
/// <para>
/// Memory only — nothing is written to disk. A batch of twenty spawns is twenty transcripts and
/// nineteen of them are never read, so the default has to be cheap: keep them for this process and
/// let the oldest fall off once the ring is full. Persisting every run was the other option and it
/// was rejected on purpose — that is a real feature (survives a restart, can be searched, has a
/// retention policy of its own) and nobody has needed it yet. Building it speculatively would mean
/// guessing its shape before a real caller has a use for it.
/// </para>
/// </summary>
public sealed class SpawnedRunLog
{
    private readonly object _gate = new();
    private readonly LinkedList<SpawnedRun> _runs = new();
    private readonly Dictionary<string, LinkedListNode<SpawnedRun>> _byId = new();
    private readonly int _capacity;

    public SpawnedRunLog(int capacity = 50)
    {
        // A ring of nothing would silently evict every run as it arrived — a log that accepts writes
        // and keeps none is worse than no log, because callers would have no way to tell.
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _capacity = capacity;
    }

    /// <summary>Adds a finished run, evicting the oldest one once the ring is full.</summary>
    public void Record(SpawnedRun run)
    {
        lock (_gate)
        {
            var node = _runs.AddLast(run);
            _byId[run.Id] = node;

            while (_runs.Count > _capacity)
            {
                var oldest = _runs.First!;
                _runs.RemoveFirst();
                _byId.Remove(oldest.Value.Id);
            }
        }
    }

    public SpawnedRun? Get(string id)
    {
        lock (_gate)
        {
            return _byId.TryGetValue(id, out var node) ? node.Value : null;
        }
    }

    /// <summary>Newest first.</summary>
    public IReadOnlyList<SpawnedRun> List()
    {
        lock (_gate)
        {
            return _runs.Reverse().ToList();
        }
    }
}
