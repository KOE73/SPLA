namespace SPLA.Domain.Secrets;

/// <summary>
/// Default <see cref="ISecretResolver"/>: resolves <c>secret:KEY</c> / <c>secret:KEY#FIELD</c> via
/// the <see cref="ISecretStore"/>, <c>env:VAR</c> via the environment, and treats anything else as
/// a literal.
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
        {
            var (key, field) = SplitRef(reference[7..]);
            var entry = await _store.GetEntryAsync(key, ct);
            return field is null ? entry?.DefaultValue : entry?[field];
        }
        return reference;
    }

    public string? Resolve(string? reference)
        // File-based stores complete synchronously; safe to unwrap.
        => ResolveAsync(reference).AsTask().GetAwaiter().GetResult();

    public ValueTask<SecretEntry?> GetEntryAsync(string key, CancellationToken ct = default)
        => _store.GetEntryAsync(key.Trim(), ct);

    /// <summary>Splits <c>KEY#FIELD</c> (field optional). '#' can't appear in keys.</summary>
    private static (string Key, string? Field) SplitRef(string body)
    {
        var i = body.IndexOf('#');
        return i < 0
            ? (body.Trim(), null)
            : (body[..i].Trim(), body[(i + 1)..].Trim());
    }
}
