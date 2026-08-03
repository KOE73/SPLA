using SPLA.Domain.Secrets;

namespace SPLA.Plugins.Sql;

/// <summary>
/// The connection set of one agent session, shared by reference across all sql tools.
///
/// READ-ONLY by design. Connections are operator configuration — they are edited in the settings UI
/// and live in the committable <c>*.spla</c> (<c>plugins.sql.settings</c>); no tool creates or
/// changes one, so no credential can arrive through the model. A definition holds a
/// <c>credential:</c> entry name (or a legacy <c>secret:</c>/<c>env:</c> pointer) and the value is
/// materialized only in <see cref="ResolveForOpen"/>, on a clone, at connection-open.
/// </summary>
public sealed class SqlConnectionRegistry
{
    private readonly Dictionary<string, SqlConnectionConfig> _connections;
    private readonly ISecretResolver _resolver;

    public SqlConnectionRegistry(
        Dictionary<string, SqlConnectionConfig> connections,
        string? defaultConnection,
        ISecretResolver resolver)
    {
        _connections = new Dictionary<string, SqlConnectionConfig>(
            connections, StringComparer.OrdinalIgnoreCase);
        DefaultConnection = defaultConnection;
        _resolver = resolver;
    }

    public IReadOnlyDictionary<string, SqlConnectionConfig> Connections => _connections;

    public string? DefaultConnection { get; }

    /// <summary>
    /// Returns a copy of the named connection ready to open, with secrets materialized to plaintext.
    /// When <see cref="SqlConnectionConfig.Credential"/> names a secret-store entry, its <c>user</c>
    /// (unless overridden here) and <c>password</c> are pulled from that record; otherwise the legacy
    /// <see cref="SqlConnectionConfig.Password"/> pointer (<c>secret:</c>/<c>env:</c>) is resolved.
    /// The stored config is left untouched. The plaintext lives only on the returned clone.
    /// </summary>
    public SqlConnectionConfig ResolveForOpen(SqlConnectionConfig cfg)
    {
        var clone = Clone(cfg);

        if (!string.IsNullOrWhiteSpace(cfg.Credential))
        {
            // Whole credential record — resolved only here, at connection-open. Secret backends are
            // local files (plain/DPAPI), so blocking briefly here is acceptable.
            var entry = _resolver.GetEntryByRefAsync(cfg.Credential).AsTask().GetAwaiter().GetResult()
                ?? throw new InvalidOperationException(
                    $"credential '{cfg.Credential}' not found in the secret store");
            if (string.IsNullOrWhiteSpace(clone.User))
                clone.User = entry[SecretFields.User];
            clone.Password = entry[SecretFields.Password] ?? entry.DefaultValue;
        }
        else
        {
            clone.Password = _resolver.Resolve(cfg.Password);
        }

        return clone;
    }

    private static SqlConnectionConfig Clone(SqlConnectionConfig c) => new()
    {
        Provider = c.Provider,
        Server = c.Server,
        Host = c.Host,
        Port = c.Port,
        Database = c.Database,
        User = c.User,
        Credential = c.Credential,
        Password = c.Password,
        TrustedConnection = c.TrustedConnection,
        File = c.File,
        Description = c.Description
    };
}
