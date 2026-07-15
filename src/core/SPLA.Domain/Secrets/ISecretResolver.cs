namespace SPLA.Domain.Secrets;

/// <summary>
/// Resolves a config value that may be a <em>reference</em> to a secret rather than the secret
/// itself, so committable config (<c>*.spla</c>) holds only pointers. Forms:
/// <list type="bullet">
/// <item><c>secret:KEY</c> → default field of the entry (see <see cref="SecretEntry.DefaultValue"/>);</item>
/// <item><c>secret:KEY#FIELD</c> → a specific field of the entry;</item>
/// <item><c>env:VAR</c> → read from the environment;</item>
/// <item>anything else → returned as-is (a literal).</item>
/// </list>
/// This is the industry pattern (Vault / k8s / SOPS): the config references a secret manager; the
/// plaintext is only materialized at point of use.
/// </summary>
public interface ISecretResolver
{
    ValueTask<string?> ResolveAsync(string? reference, CancellationToken ct = default);

    /// <summary>Synchronous resolve for hot paths (e.g. building a connection string). Backends are
    /// local and fast; use <see cref="ResolveAsync"/> where an async context is already available.</summary>
    string? Resolve(string? reference);

    /// <summary>Whole entry by store key (NOT a <c>secret:</c>-prefixed reference) — for consumers
    /// that need several fields of one credential (SSH: user + password/private key).</summary>
    ValueTask<SecretEntry?> GetEntryAsync(string key, CancellationToken ct = default);

    /// <summary>True when a value is a secret reference (<c>secret:</c> or <c>env:</c>) rather than a literal.</summary>
    static bool IsReference(string? value) =>
        value is not null &&
        (value.StartsWith("secret:", StringComparison.OrdinalIgnoreCase) ||
         value.StartsWith("env:", StringComparison.OrdinalIgnoreCase));
}
