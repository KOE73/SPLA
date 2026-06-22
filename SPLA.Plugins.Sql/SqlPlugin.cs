using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Sql.Tools;

namespace SPLA.Plugins.Sql;

public class SqlPlugin : ISplaPlugin
{
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
