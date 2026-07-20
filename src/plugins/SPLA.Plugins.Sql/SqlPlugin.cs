using System.Text.Json;
using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Sql.Tools;

namespace SPLA.Plugins.Sql;

public class SqlPlugin : ISplaPlugin, ISplaPluginAction, SPLA.Domain.Editor.ISchemaContributor
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // Captured at Initialize so the settings-UI "testConnection" action can materialize a
    // credential/secret reference server-side — the same way tools do. The action never receives
    // or returns the secret value itself.
    private ISecretResolver? _resolver;

    /// <summary>Handles actions invoked from the plugin's web settings UI. Currently just
    /// "testConnection", shared with the <c>sql_test_connection</c> agent tool via
    /// <see cref="SqlConnectionTester"/> so both paths report identical results.</summary>
    public SPLA.Domain.Editor.IJsonSchemaProvider GetSchemaProvider() => SqlSchemaProvider.Create();

    public async Task<object?> InvokeActionAsync(string action, string? valueJson, CancellationToken ct = default)
    {
        if (action != "testConnection")
            throw new InvalidOperationException($"Unknown sql plugin action: {action}");

        var cfg = string.IsNullOrWhiteSpace(valueJson)
            ? throw new InvalidOperationException("testConnection requires a connection config")
            : JsonSerializer.Deserialize<SqlConnectionConfig>(valueJson, JsonOpts)
              ?? throw new InvalidOperationException("invalid connection config");

        // Materialize a credential entry / secret reference before testing, so the UI can test a
        // connection that stores no literal password. Resolved values stay on this local copy.
        cfg = await ResolveForTestAsync(cfg, ct);

        var result = await SqlConnectionTester.TestAsync(cfg, ct);
        return new { ok = result.Ok, message = result.Message };
    }

    /// <summary>Pulls user+password from the named credential entry, or resolves the legacy
    /// <c>secret:</c>/<c>env:</c> password pointer. No-op literal passwords pass straight through.</summary>
    private async Task<SqlConnectionConfig> ResolveForTestAsync(SqlConnectionConfig cfg, CancellationToken ct)
    {
        if (_resolver is null) return cfg;

        if (!string.IsNullOrWhiteSpace(cfg.Credential))
        {
            var entry = await _resolver.GetEntryAsync(cfg.Credential, ct)
                ?? throw new InvalidOperationException(
                    $"credential '{cfg.Credential}' not found in the secret store");
            cfg.User = string.IsNullOrWhiteSpace(cfg.User) ? entry[SecretFields.User] : cfg.User;
            cfg.Password = entry[SecretFields.Password] ?? entry.DefaultValue;
        }
        else if (ISecretResolver.IsReference(cfg.Password))
        {
            cfg.Password = await _resolver.ResolveAsync(cfg.Password, ct);
        }

        return cfg;
    }

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        _resolver = settings.SecretResolver;

        settings.Plugins.TryGetValue("sql", out var section);
        var sql = SqlSettings.FromBlob(section?.Settings);

        // Passwords are NOT overlaid here — connection definitions keep their secret:/env: references
        // and are materialized at connection-open via the resolver (see SqlConnectionRegistry).
        var defaultConnection = sql.DefaultConnection
            ?? (sql.Connections.Count > 0 ? sql.Connections.Keys.First() : null);

        // One shared registry per session — sql_manage_connection changes are
        // immediately visible to all other sql tools without restarting.
        var registry = new SqlConnectionRegistry(
            sql.Connections, defaultConnection, settings.ProjectFilePath,
            settings.Secrets, settings.SecretResolver);

        return
        [
            new SqlConnectionsTool(registry),
            new SqlTestConnectionTool(registry),
            new SqlManageConnectionTool(registry),
            new SqlQueryTool(registry),
            new SqlSchemaTool(registry),
            new SqlQueryPlanTool(registry),
            new SqlExecuteTool(registry),
            new SqlVerifyContextTool(registry),
        ];
    }
}
