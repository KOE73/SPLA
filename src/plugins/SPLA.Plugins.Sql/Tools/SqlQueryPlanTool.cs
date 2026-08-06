using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.Sql.Factory;

namespace SPLA.Plugins.Sql.Tools;

public class SqlQueryPlanTool : SqlToolBase, IMcpTool
{
    public SqlQueryPlanTool(SqlConnectionRegistry registry) : base(registry) { }

    public string Name => "sql_query_plan";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = """
                Returns the native execution plan for a SQL statement without running it.
                Output format is provider-native (MSSQL / Postgres / SQLite) — interpret accordingly.
                Use before sql_execute to estimate affected rows and detect full-table scans.
                """,
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    sql         = new { type = "string", description = "SQL statement to analyse." },
                    connection  = new { type = "string", description = "Named connection. Omit to use the default." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "sql" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var sql = ToolJson.GetStringTrimmed(root, "sql");
            if (string.IsNullOrWhiteSpace(sql)) return ToolResult.Fail("Error: Missing 'sql' parameter.", "missing sql");

            if (!TryResolve(ToolJson.GetStringTrimmed(root, "connection"), out var cfg, out var err))
                return ToolResult.Fail(err!, "connection not resolved");

            using var conn = await SqlConnectionFactory.CreateAsync(cfg!, cancellationToken);

            var result = cfg!.Provider.ToLowerInvariant() switch
            {
                "mssql" => await MssqlPlanAsync(conn, sql),
                _       => "[stub] query plan not yet implemented for this provider"
            };

            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (target == OutputTarget.Context) return ToolResult.Text(result);
            var name = ToolJson.GetStringTrimmed(root, "output_name");
            return ToolResult.Text(DataChannel.Route(target, BlobPayload.OfText(result), "sql_query_plan", name));
        }
        catch (JsonException) { return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json"); }
        catch (Exception ex)  { return ToolResult.Fail($"Error: {ex.Message}", ex.GetType().Name); }
    }

    private static async Task<string> MssqlPlanAsync(System.Data.IDbConnection conn, string sql)
    {
        // Use SET SHOWPLAN_TEXT ON — returns plan without executing, read-only
        await conn.ExecuteAsync("SET SHOWPLAN_TEXT ON");
        try
        {
            var rows = await conn.QueryAsync<string>(sql);
            var sb = new StringBuilder();
            foreach (var r in rows)
                sb.AppendLine(r);
            return sb.ToString().TrimEnd();
        }
        finally
        {
            await conn.ExecuteAsync("SET SHOWPLAN_TEXT OFF");
        }
    }
}
