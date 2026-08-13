using System;
using System.Collections.Generic;
using System.Linq;
using SPLA.Domain.Security;

namespace SPLA.MCP.Core.Security;

/// <summary>One edge and how much has actually gone along it.</summary>
/// <param name="Calls">Times traversed. The map draws thickness from this, and a rule with no
/// traffic behind it is the first thing worth deleting.</param>
public sealed record EdgeTraffic(ZoneEdge Edge, int Calls, DateTimeOffset FirstSeen, DateTimeOffset LastSeen, string LastTool);

/// <summary>
/// What has moved where, counted while nothing is being refused.
///
/// <para>The whole argument for shadow mode: which edges a real project needs cannot be guessed, and
/// a week of ordinary work answers it exactly. Enforcement defaults get chosen from this rather than
/// from imagination — the lesson of every policy system complex enough that everyone ran it
/// permissive.</para>
///
/// <para><b>Persisted, because a week of ordinary work is the unit here and a reading that dies at
/// exit measures nothing.</b> The reservation that argued against it — that a record of what
/// somebody's agent did should be started deliberately rather than as a side effect — is answered by
/// where it lives rather than by not keeping it: under <c>.spla/</c>, which is inside the cutout, so
/// the agent cannot read its own account of itself; never in git; and deletable by deleting one
/// file.</para>
/// </summary>
public sealed class EdgeLedger
{
    private readonly Dictionary<string, EdgeTraffic> _seen = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    /// <summary>Raised after a change, for whoever owns the file. The ledger itself knows nothing
    /// about storage — same reason the domain takes a callback instead of a logger.</summary>
    public event EventHandler? Persist;

    /// <summary>Restores a previous run's counts. Later runs add to them: the question the ledger
    /// answers is what a project needs over a week, not what it needed since breakfast.</summary>
    public void Restore(IEnumerable<EdgeTraffic> traffic)
    {
        lock (_lock)
        {
            _seen.Clear();
            foreach (var t in traffic) _seen[t.Edge.Key] = t;
        }
    }

    public void Record(ZoneEdge edge, string tool)
    {
        // Agent-internal movement is not traffic between perimeters; counting it would bury the
        // handful of rows that matter under memory reads.
        if (edge.Source == Zone.Agent && edge.Sink == Zone.Agent) return;

        var now = DateTimeOffset.Now;
        lock (_lock)
        {
            _seen[edge.Key] = _seen.TryGetValue(edge.Key, out var prior)
                ? prior with { Calls = prior.Calls + 1, LastSeen = now, LastTool = tool }
                : new EdgeTraffic(edge, 1, now, now, tool);
        }

        Changed?.Invoke(this, EventArgs.Empty);
        Persist?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Busiest first — what a person looks at is where the work actually goes.</summary>
    public IReadOnlyList<EdgeTraffic> List()
    {
        lock (_lock) return _seen.Values.OrderByDescending(e => e.Calls).ThenBy(e => e.Edge.Key).ToList();
    }

    public event EventHandler? Changed;
}
