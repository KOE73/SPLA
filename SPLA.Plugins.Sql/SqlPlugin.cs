using System.Text.Json;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Sql.Tools;

namespace SPLA.Plugins.Sql;

public class SqlPlugin : ISplaPlugin, ISplaPluginAction, SPLA.Domain.Editor.ISchemaContributor
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

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

        var result = await SqlConnectionTester.TestAsync(cfg, ct);
        return new { ok = result.Ok, message = result.Message };
    }

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
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
