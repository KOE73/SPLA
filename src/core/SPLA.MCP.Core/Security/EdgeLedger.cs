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
/// <para>Process-lifetime and in memory. Persisting it would make it evidence rather than a
/// reading, and evidence of what a person's agent did is a thing to decide about deliberately, not
/// to start writing as a side effect.</para>
/// </summary>
public sealed class EdgeLedger
{
    private readonly Dictionary<string, EdgeTraffic> _seen = new(StringComparer.Ordinal);
    private readonly object _lock = new();

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
    }

    /// <summary>Busiest first — what a person looks at is where the work actually goes.</summary>
    public IReadOnlyList<EdgeTraffic> List()
    {
        lock (_lock) return _seen.Values.OrderByDescending(e => e.Calls).ThenBy(e => e.Edge.Key).ToList();
    }

    public event EventHandler? Changed;
}
