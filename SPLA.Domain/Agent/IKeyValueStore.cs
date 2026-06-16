namespace SPLA.Domain.Agent;

/// <summary>
/// Fundamental agent working memory: a flat key/value scratchpad the agent owns, independent of
/// any task or mode. Two instances exist at runtime — one scoped to the current chat ("session")
/// and one shared across the project ("project"). Keys prefixed with <c>context:</c> are
/// auto-injected into the prompt each turn (see <c>SPLA.Agent.WorkingMemoryInjector</c>).
/// <para>
/// <see cref="Changed"/> lets external observers (e.g. a debug view) react to live updates.
/// </para>
/// </summary>
public interface IKeyValueStore
{
    /// <summary>Identifier of this store, e.g. "session" or "project". Used for labelling.</summary>
    string Scope { get; }

    string? Get(string key);
    void Set(string key, string value);
    bool Delete(string key);

    /// <summary>
    /// Removes all entries whose key contains <paramref name="keyFilter"/> (case-insensitive).
    /// If <paramref name="keyFilter"/> is null or empty, removes ALL entries.
    /// Returns number of entries removed.
    /// </summary>
    int DeleteWhere(string? keyFilter);

    /// <summary>All entries, ordered by key.</summary>
    IReadOnlyList<KeyValuePair<string, string>> List();

    /// <summary>Raised after any mutation (set, delete, bulk load, clear).</summary>
    event EventHandler? Changed;
}
