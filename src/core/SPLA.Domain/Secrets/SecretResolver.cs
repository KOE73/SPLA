namespace SPLA.Domain.Secrets;

/// <summary>
/// Default <see cref="ISecretResolver"/>: materializes <c>secret:&lt;scope&gt;:&lt;key&gt;[#field]</c>
/// from the store, <c>env:VAR</c> from the environment, and treats anything else as a literal.
/// <para>
/// This is also where access is enforced. Checking rights only when listing would be decoration —
/// anything that knew the key could still resolve the material. So every resolve consults
/// <see cref="ISecretAccessPolicy"/> against the ambient <see cref="SecretCallerScope.Identity"/>,
/// and a refusal throws with the scope and key named. That is the difference between an ACL and a
/// filter on a dropdown.
/// </para>
/// <para>
/// A malformed or unknown reference fails loudly. There is no scope search and no fallback: the cost
/// of a clear error is one message, the cost of a silent substitution is using somebody else's
/// credential without knowing it.
/// </para>
/// </summary>
public sealed class SecretResolver : ISecretResolver
{
    private readonly ISecretStore _store;
    private readonly ISecretAccessPolicy _policy;

    public SecretResolver(ISecretStore store, ISecretAccessPolicy? policy = null)
    {
        _store = store;
        _policy = policy ?? PermissiveSecretAccessPolicy.Instance;
    }

    public async ValueTask<string?> ResolveAsync(string? reference, CancellationToken ct = default)
    {
        if (reference is null) return null;

        if (SecretRef.IsEnvReference(reference))
            return Environment.GetEnvironmentVariable(SecretRef.EnvVariable(reference));

        if (!SecretRef.IsSecretReference(reference))
            return reference; // literal

        if (!SecretRef.TryParse(reference, out var scope, out var key, out var field, out var error))
            throw new FormatException($"Bad secret reference '{reference}': {error}");

        var entry = await GetEntryAsync(key, scope, ct);
        return field is null ? entry?.DefaultValue : entry?[field];
    }

    public string? Resolve(string? reference)
        // File-based stores complete synchronously; safe to unwrap.
        => ResolveAsync(reference).AsTask().GetAwaiter().GetResult();

    public async ValueTask<SecretEntry?> GetEntryAsync(string key, SecretScope scope, CancellationToken ct = default)
    {
        var identity = SecretCallerScope.Identity;
        var acl = await _store.Acl.GetAsync(scope, key, ct);

        if (!_policy.CanUse(identity, scope, key, acl))
            throw new SecretAccessDeniedException(scope, key, identity.DisplayName, SecretRight.Use);

        return await _store.GetEntryAsync(key.Trim(), scope, ct);
    }
}
