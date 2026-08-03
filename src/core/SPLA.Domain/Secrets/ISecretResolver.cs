namespace SPLA.Domain.Secrets;

/// <summary>
/// Resolves a config value that may be a <em>reference</em> to a secret rather than the secret
/// itself, so committable config (<c>*.spla</c>) holds only pointers. The grammar and every
/// separator live in <see cref="SecretRef"/> — the one place they exist.
/// <list type="bullet">
/// <item><c>secret:&lt;scope&gt;:&lt;key&gt;</c> → default field of the entry (see <see cref="SecretEntry.DefaultValue"/>);</item>
/// <item><c>secret:&lt;scope&gt;:&lt;key&gt;#&lt;field&gt;</c> → a specific field;</item>
/// <item><c>env:VAR</c> → read from the environment;</item>
/// <item>anything else → returned as-is (a literal).</item>
/// </list>
/// This is the industry pattern (Vault / k8s / SOPS): the config references a secret manager; the
/// plaintext is only materialized at point of use — and only for a caller allowed to have it.
/// </summary>
public interface ISecretResolver
{
    ValueTask<string?> ResolveAsync(string? reference, CancellationToken ct = default);

    /// <summary>Synchronous resolve for hot paths (e.g. building a connection string). Backends are
    /// local and fast; use <see cref="ResolveAsync"/> where an async context is already available.</summary>
    string? Resolve(string? reference);

    /// <summary>Whole entry by scope and key (NOT a reference string) — for consumers that need
    /// several fields of one credential (SSH: user + password/private key). Subject to the same
    /// access check as <see cref="ResolveAsync"/>.</summary>
    ValueTask<SecretEntry?> GetEntryAsync(string key, SecretScope scope, CancellationToken ct = default);

    /// <summary>
    /// Whole entry named by a full reference string (<c>secret:&lt;scope&gt;:&lt;key&gt;</c>) — what a
    /// <c>credential:</c> field in config holds. One syntax everywhere: a credential is a reference
    /// like any other, so nothing has to remember a second way of naming the same thing.
    /// </summary>
    async ValueTask<SecretEntry?> GetEntryByRefAsync(string reference, CancellationToken ct = default)
    {
        if (!SecretRef.TryParse(reference, out var scope, out var key, out _, out var error))
            throw new FormatException($"Bad credential reference '{reference}': {error}");
        return await GetEntryAsync(key, scope, ct);
    }

    /// <summary>True when a value is a reference rather than a literal.</summary>
    static bool IsReference(string? value) => SecretRef.IsReference(value);
}
