namespace SPLA.Domain.Agent;

/// <summary>
/// In-memory <see cref="IBlobStore"/>. Transient: blobs live only for the chat's runtime and are not
/// persisted (they are pipeline buffers, not working memory). Thread-safe for concurrent tool calls.
/// </summary>
public sealed class BlobStore : IBlobStore
{
    public const string HandlePrefix = "blob:";

    private readonly Dictionary<string, (BlobEntry Entry, BlobPayload Payload)> _items = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    public event EventHandler? Changed;

    public string Put(BlobPayload payload, string? name = null)
    {
        if (payload is null) throw new ArgumentNullException(nameof(payload));

        var clean = string.IsNullOrWhiteSpace(name) ? null : Sanitize(name);
        var handle = HandlePrefix + (clean ?? Guid.NewGuid().ToString("N")[..8]);
        var entry = new BlobEntry(handle, clean, payload.Kind, payload.Size, DateTimeOffset.UtcNow);

        lock (_lock) _items[handle] = (entry, payload);
        OnChanged();
        return handle;
    }

    public BlobPayload? Get(string handle)
    {
        var key = Normalize(handle);
        lock (_lock) return _items.TryGetValue(key, out var v) ? v.Payload : null;
    }

    public bool Delete(string handle)
    {
        var key = Normalize(handle);
        bool removed;
        lock (_lock) removed = _items.Remove(key);
        if (removed) OnChanged();
        return removed;
    }

    public IReadOnlyList<BlobEntry> List()
    {
        lock (_lock)
            return _items.Values.Select(v => v.Entry).OrderBy(e => e.CreatedAt).ToList();
    }

    /// <summary>Removes all blobs. Fires <see cref="Changed"/> if anything was removed.</summary>
    public void Clear()
    {
        bool had;
        lock (_lock) { had = _items.Count > 0; _items.Clear(); }
        if (had) OnChanged();
    }

    private static string Normalize(string handle)
        => handle.StartsWith(HandlePrefix, StringComparison.Ordinal) ? handle : HandlePrefix + handle;

    private static string Sanitize(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.StartsWith(HandlePrefix, StringComparison.Ordinal))
            trimmed = trimmed[HandlePrefix.Length..];
        return trimmed;
    }

    private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
