namespace SPLA.Domain.Agent;

/// <summary>
/// In-memory implementation of <see cref="IKeyValueStore"/>. Pure and dependency-free; persistence
/// is layered on top by the composition root (chat file for session scope, project-kv.yaml for
/// project scope). Thread-safe for concurrent tool execution.
/// </summary>
public sealed class KeyValueStore : IKeyValueStore
{
    private readonly Dictionary<string, string> _items = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    public KeyValueStore(string scope) => Scope = scope;

    public string Scope { get; }

    public event EventHandler? Changed;

    public string? Get(string key)
    {
        lock (_lock) return _items.TryGetValue(key, out var v) ? v : null;
    }

    public void Set(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must not be empty.", nameof(key));
        lock (_lock) _items[key] = value ?? string.Empty;
        OnChanged();
    }

    public bool Delete(string key)
    {
        bool removed;
        lock (_lock) removed = _items.Remove(key);
        if (removed) OnChanged();
        return removed;
    }

    public int DeleteWhere(string? keyFilter)
    {
        int removed;
        lock (_lock)
        {
            if (string.IsNullOrEmpty(keyFilter))
            {
                removed = _items.Count;
                _items.Clear();
            }
            else
            {
                var keys = _items.Keys
                    .Where(k => k.Contains(keyFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var k in keys) _items.Remove(k);
                removed = keys.Count;
            }
        }
        if (removed > 0) OnChanged();
        return removed;
    }

    public IReadOnlyList<KeyValuePair<string, string>> List()
    {
        lock (_lock)
            return _items.OrderBy(kv => kv.Key, StringComparer.Ordinal).ToList();
    }

    /// <summary>Replaces all entries (used when loading from persistence). Fires <see cref="Changed"/> once.</summary>
    public void LoadFrom(IEnumerable<KeyValuePair<string, string>> entries)
    {
        lock (_lock)
        {
            _items.Clear();
            foreach (var e in entries)
                if (!string.IsNullOrWhiteSpace(e.Key))
                    _items[e.Key] = e.Value ?? string.Empty;
        }
        OnChanged();
    }

    /// <summary>Removes all entries. Fires <see cref="Changed"/> if anything was removed.</summary>
    public void Clear()
    {
        bool had;
        lock (_lock) { had = _items.Count > 0; _items.Clear(); }
        if (had) OnChanged();
    }

    /// <summary>Plain dictionary copy for serialization.</summary>
    public Dictionary<string, string> Snapshot()
    {
        lock (_lock) return new Dictionary<string, string>(_items, StringComparer.Ordinal);
    }

    private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
