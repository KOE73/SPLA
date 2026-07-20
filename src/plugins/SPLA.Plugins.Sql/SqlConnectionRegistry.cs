using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;

namespace SPLA.Plugins.Sql;

/// <summary>
/// Live, mutable registry of SQL connections for one agent session.
/// Shared by reference across all sql tools in the session so that
/// sql_manage_connection changes are immediately visible to sql_query etc.
///
/// Storage split (secrets rule): connection <em>definitions</em> go in the committable <c>*.spla</c>
/// (<c>plugins.sql.settings</c>); literal passwords are kept in the global <see cref="ISecretStore"/>
/// and the config holds only a <c>secret:sql:&lt;name&gt;</c> reference. <c>env:</c> references stay
/// in the project file. Passwords are materialized at connection-open via <see cref="ISecretResolver"/>.
/// </summary>
public sealed class SqlConnectionRegistry
{
    private readonly Dictionary<string, SqlConnectionConfig> _connections;
    private readonly string? _projectFilePath;
    private readonly ISecretStore _secrets;
    private readonly ISecretResolver _resolver;
    private string? _defaultConnection;

    /// <summary>Secret-store key for a connection's password (shared with the GUI editor).</summary>
    public static string SecretKey(string name) => $"sql:{name}";

    /// <summary>The <c>secret:</c> reference stored in <c>*.spla</c> for a connection's password.</summary>
    public static string SecretRef(string name) => $"secret:{SecretKey(name)}";

    public SqlConnectionRegistry(
        Dictionary<string, SqlConnectionConfig> connections,
        string? defaultConnection,
        string? projectFilePath,
        ISecretStore secrets,
        ISecretResolver resolver)
    {
        _connections = new Dictionary<string, SqlConnectionConfig>(
            connections, StringComparer.OrdinalIgnoreCase);
        _defaultConnection = defaultConnection;
        _projectFilePath = projectFilePath;
        _secrets = secrets;
        _resolver = resolver;
    }

    public IReadOnlyDictionary<string, SqlConnectionConfig> Connections => _connections;

    public string? DefaultConnection => _defaultConnection;

    public void Add(string name, SqlConnectionConfig cfg)
    {
        _connections[name] = cfg;
        if (_defaultConnection is null) _defaultConnection = name;
    }

    public bool Remove(string name)
    {
        var removed = _connections.Remove(name);
        if (removed && string.Equals(_defaultConnection, name, StringComparison.OrdinalIgnoreCase))
            _defaultConnection = _connections.Count > 0 ? _connections.Keys.First() : null;
        return removed;
    }

    public void SetDefault(string name) => _defaultConnection = name;

    /// <summary>
    /// Returns a copy of the named connection ready to open, with secrets materialized to plaintext.
    /// When <see cref="SqlConnectionConfig.Credential"/> names a secret-store entry, its <c>user</c>
    /// (unless overridden here) and <c>password</c> are pulled from that record; otherwise the legacy
    /// <see cref="SqlConnectionConfig.Password"/> pointer (<c>secret:</c>/<c>env:</c>) is resolved.
    /// The stored config is left untouched. The plaintext lives only on the returned clone.
    /// </summary>
    public SqlConnectionConfig ResolveForOpen(SqlConnectionConfig cfg)
    {
        var clone = Clone(cfg, dropPassword: false);

        if (!string.IsNullOrWhiteSpace(cfg.Credential))
        {
            // Whole credential record — resolved only here, at connection-open. Secret backends are
            // local (file/DPAPI); blocking briefly is consistent with Persist's SetAsync path.
            var entry = _resolver.GetEntryAsync(cfg.Credential).AsTask().GetAwaiter().GetResult()
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

    /// <summary>
    /// Persists connections: definitions (with passwords replaced by <c>secret:</c> references) go to
    /// <c>plugins.sql.settings</c> in the committable <c>*.spla</c>; literal passwords go to the
    /// global secrets store (project scope). <c>env:</c>/<c>secret:</c> references stay as-is.
    /// No-op if there is no project file.
    /// </summary>
    public string Persist()
    {
        if (string.IsNullOrWhiteSpace(_projectFilePath))
            return "No project file — changes are in-memory only for this session.";

        try
        {
            var project = ConfigLoader.LoadProjectRaw(_projectFilePath);
            project.Plugins ??= new Dictionary<string, SplaPluginSection>();

            if (!project.Plugins.TryGetValue("sql", out var section))
            {
                section = new SplaPluginSection { Enabled = true };
                project.Plugins["sql"] = section;
            }

            var stored = new Dictionary<string, SqlConnectionConfig>(StringComparer.OrdinalIgnoreCase);
            var secretCount = 0;

            foreach (var (name, cfg) in _connections)
            {
                if (string.IsNullOrWhiteSpace(cfg.Credential)
                    && !string.IsNullOrEmpty(cfg.Password) && !ISecretResolver.IsReference(cfg.Password))
                {
                    // Literal password → secret store; config keeps only a reference.
                    _secrets.SetAsync(SecretKey(name), cfg.Password, SecretScope.Project)
                        .AsTask().GetAwaiter().GetResult();
                    var def = Clone(cfg, dropPassword: false);
                    def.Password = SecretRef(name);
                    stored[name] = def;
                    secretCount++;
                }
                else
                {
                    stored[name] = cfg; // env:/secret:/no password — store as-is
                }
            }

            var settings = new SqlSettings
            {
                DefaultConnection = _defaultConnection,
                Connections = stored
            };
            section.Settings = ConfigLoader.DeserializeBlob(settings.ToYaml());
            ConfigLoader.SaveProjectSections(project, _projectFilePath, "plugins");

            var note = secretCount > 0 ? $" ({secretCount} password(s) → secrets store)" : "";
            return $"Saved {_connections.Count} connection(s) to {Path.GetFileName(_projectFilePath)}{note}.";
        }
        catch (Exception ex)
        {
            return $"Failed to save: {ex.Message}";
        }
    }

    private static SqlConnectionConfig Clone(SqlConnectionConfig c, bool dropPassword) => new()
    {
        Provider = c.Provider,
        Server = c.Server,
        Host = c.Host,
        Port = c.Port,
        Database = c.Database,
        User = c.User,
        Credential = c.Credential,
        Password = dropPassword ? null : c.Password,
        TrustedConnection = c.TrustedConnection,
        File = c.File,
        Description = c.Description
    };
}
