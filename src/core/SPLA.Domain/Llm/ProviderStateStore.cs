using System.Collections.Concurrent;

namespace SPLA.Domain.Llm;

/// <summary>
/// The last thing each connection's provider told us about itself — rate-limit budget, reset times,
/// whatever else arrived in headers.
/// <para>
/// Deliberately <b>not</b> the usage ledger, and the two must not be merged despite looking alike.
/// The ledger is append-only, one row per network attempt, and answers "what was spent". This is
/// last-write-wins, one entry per connection, and answers "what is the state of this key right now".
/// One table serving both would either grow a row per observation or lose the accounting.
/// </para>
/// <para>
/// Keyed by <i>connection</i>, not by model: a rate limit and a balance belong to the credential, and
/// five models under one key share them. In-memory only — a stale budget from a previous run is worse
/// than no budget, because it reads as current.
/// </para>
/// </summary>
public sealed class ProviderStateStore
{
    private readonly ConcurrentDictionary<string, IReadOnlyList<ProviderFact>> _byConnection =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Records the latest observations for a connection. An empty set is ignored rather than
    /// stored: a provider that sends no headers must not erase what a previous response reported.</summary>
    public void Record(string? connectionId, IReadOnlyList<ProviderFact> facts)
    {
        if (string.IsNullOrWhiteSpace(connectionId) || facts.Count == 0) return;
        _byConnection[connectionId] = facts;
    }

    /// <summary>The last observations for a connection, or empty when nothing has been seen yet.</summary>
    public IReadOnlyList<ProviderFact> Get(string? connectionId)
        => !string.IsNullOrWhiteSpace(connectionId) && _byConnection.TryGetValue(connectionId, out var f)
            ? f
            : [];
}
