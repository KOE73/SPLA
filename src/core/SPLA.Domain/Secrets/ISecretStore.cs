namespace SPLA.Domain.Secrets;

/// <summary>Where a secret lives.</summary>
public enum SecretScope
{
    /// <summary>Bound to the current project (<c>&lt;workspace&gt;/.spla/secrets.yaml</c>). Travels with the project, not the machine.</summary>
    Project,

    /// <summary>Bound to the current user/machine (<c>~/.spla/secrets.yaml</c>). Shared across all projects.</summary>
    Machine
}

/// <summary>
/// Global secrets store — NOT tied to any plugin. SQL, SSH, other plugins and the host all use it to
/// keep credentials out of committable config files. The unit of storage is a <see cref="SecretEntry"/>:
/// a named record of fields (user+password, a lone token, a PEM key…). The interface is the stable
/// contract; only the backend changes (plaintext dev default ↔ DPAPI via <c>secrets.backend</c>).
/// Async because real backends (keychains, cloud KMS) are async.
///
/// <para>Single-value convenience methods (<see cref="GetAsync(string,CancellationToken)"/> /
/// <see cref="SetAsync"/>) are default-implemented over entries so simple consumers (a plugin storing
/// one password) never touch the record shape: set writes the <c>password</c> field, get reads
/// <see cref="SecretEntry.DefaultValue"/>.</para>
/// </summary>
public interface ISecretStore
{
    /// <summary>Entry by key, searching Project first then Machine (project overrides), or null if absent in both.</summary>
    ValueTask<SecretEntry?> GetEntryAsync(string key, CancellationToken ct = default);

    /// <summary>Entry from a specific scope, or null.</summary>
    ValueTask<SecretEntry?> GetEntryAsync(string key, SecretScope scope, CancellationToken ct = default);

    /// <summary>Stores (fully overwrites) an entry in the given scope. Empty fields delete the entry.</summary>
    ValueTask SetEntryAsync(string key, IReadOnlyDictionary<string, string> fields, SecretScope scope, CancellationToken ct = default);

    /// <summary>Removes an entry from the given scope. Returns true if it existed.</summary>
    ValueTask<bool> DeleteAsync(string key, SecretScope scope, CancellationToken ct = default);

    /// <summary>Entries in a scope — keys and field NAMES, never values. For management UIs / listing.
    /// Optional case-insensitive key-prefix filter.</summary>
    ValueTask<IReadOnlyList<SecretEntryInfo>> ListEntriesAsync(SecretScope scope, string? prefix = null, CancellationToken ct = default);

    /// <summary>True if the key exists in either scope, without returning any value.</summary>
    ValueTask<bool> ContainsAsync(string key, CancellationToken ct = default);

    // ── Single-value conveniences (default field semantics; see SecretEntry.DefaultValue) ──

    /// <summary>Default-field value by key (project overrides machine), or null.</summary>
    async ValueTask<string?> GetAsync(string key, CancellationToken ct = default)
        => (await GetEntryAsync(key, ct))?.DefaultValue;

    /// <summary>Default-field value from a specific scope, or null.</summary>
    async ValueTask<string?> GetAsync(string key, SecretScope scope, CancellationToken ct = default)
        => (await GetEntryAsync(key, scope, ct))?.DefaultValue;

    /// <summary>Stores a single-value secret: an entry whose one field is <c>password</c>.</summary>
    ValueTask SetAsync(string key, string value, SecretScope scope, CancellationToken ct = default)
        => SetEntryAsync(key, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            { [SecretFields.Password] = value }, scope, ct);

    /// <summary>Keys in a scope — NEVER values.</summary>
    async ValueTask<IReadOnlyList<string>> ListKeysAsync(SecretScope scope, string? prefix = null, CancellationToken ct = default)
        => (await ListEntriesAsync(scope, prefix, ct)).Select(e => e.Key).ToList();
}
