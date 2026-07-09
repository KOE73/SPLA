namespace SPLA.Domain.Secrets;

/// <summary>
/// Default <see cref="ISecretResolver"/>: resolves <c>secret:KEY</c> via the <see cref="ISecretStore"/>,
/// <c>env:VAR</c> via the environment, and treats anything else as a literal.
/// </summary>
public sealed class SecretResolver : ISecretResolver
{
    private readonly ISecretStore _store;

    public SecretResolver(ISecretStore store) => _store = store;

    public async ValueTask<string?> ResolveAsync(string? reference, CancellationToken ct = default)
    {
        if (reference is null) return null;
        if (reference.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
            return Environment.GetEnvironmentVariable(reference[4..].Trim());
        if (reference.StartsWith("secret:", StringComparison.OrdinalIgnoreCase))
            return await _store.GetAsync(reference[7..].Trim(), ct);
        return reference;
    }

    public string? Resolve(string? reference)
    {
        if (reference is null) return null;
        if (reference.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
            return Environment.GetEnvironmentVariable(reference[4..].Trim());
        if (reference.StartsWith("secret:", StringComparison.OrdinalIgnoreCase))
            // FileSecretStore completes synchronously; safe to unwrap.
            return _store.GetAsync(reference[7..].Trim()).AsTask().GetAwaiter().GetResult();
        return reference;
    }
}
