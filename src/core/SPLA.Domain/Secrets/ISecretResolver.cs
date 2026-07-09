namespace SPLA.Domain.Secrets;

/// <summary>
/// Resolves a config value that may be a <em>reference</em> to a secret rather than the secret
/// itself, so committable config (<c>*.spla</c>) holds only pointers. Three forms:
/// <list type="bullet">
/// <item><c>secret:KEY</c> → looked up in <see cref="ISecretStore"/>;</item>
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

    /// <summary>True when a value is a secret reference (<c>secret:</c> or <c>env:</c>) rather than a literal.</summary>
    static bool IsReference(string? value) =>
        value is not null &&
        (value.StartsWith("secret:", StringComparison.OrdinalIgnoreCase) ||
         value.StartsWith("env:", StringComparison.OrdinalIgnoreCase));
}
