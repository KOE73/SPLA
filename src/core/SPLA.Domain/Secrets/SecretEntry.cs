namespace SPLA.Domain.Secrets;

/// <summary>
/// One record in the secret store: a named set of fields (<c>user</c>, <c>password</c>, <c>token</c>,
/// <c>private_key</c>, …). Deliberately schema-free — a user+password credential, a lone API token and
/// a PEM certificate are all the same shape, just different field names. <see cref="SecretFields"/>
/// holds the conventional names consumers agree on; nothing enforces them.
/// </summary>
public sealed class SecretEntry
{
    public string Key { get; }

    /// <summary>Field name → plaintext value. Case-insensitive by construction.</summary>
    public IReadOnlyDictionary<string, string> Fields { get; }

    public SecretEntry(string key, IReadOnlyDictionary<string, string> fields)
    {
        Key = key;
        Fields = fields is Dictionary<string, string> d && ReferenceEquals(d.Comparer, StringComparer.OrdinalIgnoreCase)
            ? d
            : new Dictionary<string, string>(fields, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Value of a field, or null when absent.</summary>
    public string? this[string field] => Fields.GetValueOrDefault(field);

    /// <summary>Sorted field names — safe to list/log; values never are.</summary>
    public IReadOnlyList<string> FieldNames => Fields.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();

    /// <summary>
    /// What a bare <c>secret:KEY</c> reference (no <c>#field</c>) resolves to: the only field when
    /// there is exactly one, else <c>password</c>, then <c>token</c>, then <c>value</c>, else null.
    /// </summary>
    public string? DefaultValue =>
        Fields.Count == 1 ? Fields.Values.First()
        : this[SecretFields.Password] ?? this[SecretFields.Token] ?? this[SecretFields.Value];
}

/// <summary>Key + field names of an entry — the listing shape. Never carries values.</summary>
public sealed record SecretEntryInfo(string Key, IReadOnlyList<string> Fields);

/// <summary>
/// Conventional field names. Conventions, not schema — any field name is storable; these are what
/// shared consumers (SSH, SQL, resolver default) look for.
/// </summary>
public static class SecretFields
{
    public const string User = "user";
    public const string Password = "password";
    public const string Token = "token";
    /// <summary>PEM private key text (multi-line), loaded via stream at point of use — never written to disk.</summary>
    public const string PrivateKey = "private_key";
    public const string Passphrase = "passphrase";
    public const string Value = "value";
}
