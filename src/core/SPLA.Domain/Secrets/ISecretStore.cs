namespace SPLA.Domain.Secrets;

/// <summary>
/// Global secrets store — NOT tied to any plugin. SQL, SSH, LLM connections and the host all use it
/// to keep credentials out of committable config files. The unit of storage is a
/// <see cref="SecretEntry"/>: a named record of fields (user+password, a lone token, a PEM key…).
/// The interface is the stable contract; only the backend changes (plaintext dev default ↔ DPAPI via
/// <c>secrets.backend</c>). Async because real backends (keychains, cloud KMS) are async.
///
/// <para>
/// <b>The scope is always explicit.</b> There is no overload that searches, and no precedence order
/// between scopes, because a store that guesses where a credential lives will eventually hand back
/// the wrong one — silently, and under a name that looked right. <c>user</c> means <c>user</c>. An
/// entry that is not where the caller said it is is an error, not a cue to look elsewhere.
/// </para>
///
/// <para>Single-value convenience methods (<see cref="GetAsync"/> / <see cref="SetAsync"/>) are
/// default-implemented over entries so simple consumers (a plugin storing one password) never touch
/// the record shape: set writes the <c>password</c> field, get reads
/// <see cref="SecretEntry.DefaultValue"/>.</para>
/// </summary>
public interface ISecretStore
{
    /// <summary>Entry from a scope, or null when absent there. Never falls through to another scope.</summary>
    ValueTask<SecretEntry?> GetEntryAsync(string key, SecretScope scope, CancellationToken ct = default);

    /// <summary>Stores (fully overwrites) an entry in the given scope. Empty fields delete the entry.</summary>
    ValueTask SetEntryAsync(string key, IReadOnlyDictionary<string, string> fields, SecretScope scope, CancellationToken ct = default);

    /// <summary>Removes an entry from the given scope. Returns true if it existed.</summary>
    ValueTask<bool> DeleteAsync(string key, SecretScope scope, CancellationToken ct = default);

    /// <summary>Entries in a scope — keys and field NAMES, never values. For management UIs / listing.
    /// Optional case-insensitive key-prefix filter.</summary>
    ValueTask<IReadOnlyList<SecretEntryInfo>> ListEntriesAsync(SecretScope scope, string? prefix = null, CancellationToken ct = default);

    /// <summary>True if the key exists in that scope, without returning any value.</summary>
    ValueTask<bool> ContainsAsync(string key, SecretScope scope, CancellationToken ct = default);

    /// <summary>Per-entry ACLs. Only <see cref="SecretScope.Shared"/> uses them meaningfully; see
    /// <see cref="ISecretAccessPolicy"/>.</summary>
    ISecretAclStore Acl { get; }

    // ── Single-value conveniences (default field semantics; see SecretEntry.DefaultValue) ──

    /// <summary>Default-field value from a scope, or null.</summary>
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
