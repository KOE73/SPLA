using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Tools;
using SPLA.Plugins.Sql.Factory;
using SPLA.Plugins.Sql.Safety;

namespace SPLA.Plugins.Sql.Tools;

public class SqlExecuteTool : SqlToolBase, IMcpTool
{
    public SqlExecuteTool(SqlConnectionRegistry registry) : base(registry) { }

    public string Name => "sql_execute";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = """
                Executes a write SQL statement (INSERT, UPDATE, DELETE).
                DDL (DROP, ALTER, CREATE) is blocked — use a DB admin tool for schema changes.
                Before calling: run sql_query_plan and pass estimated_rows.
                UPDATE/DELETE without WHERE or with estimated_rows > 1000 require confirmation.
                Available in Edit and Agent modes only.
                """,
            Scope = ToolScope.Local,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.High,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    sql            = new { type = "string",  description = "INSERT / UPDATE / DELETE statement. Accepts a blob handle (blob:<id>) from a prior tool call." },
                    connection     = new { type = "string",  description = "Named connection. Omit to use the default." },
                    estimated_rows = new { type = "integer", description = "Estimated affected rows from sql_query_plan. Required for UPDATE/DELETE with WHERE." },
                    confirmed      = new { type = "boolean", description = "Set true after the user has confirmed a high-risk or large-impact operation." }
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

            var sqlRaw = ToolJson.GetStringTrimmed(root, "sql");
            if (string.IsNullOrWhiteSpace(sqlRaw)) return ToolResult.Fail("Error: Missing 'sql' parameter.", "missing sql");
            if (!DataChannel.ResolveText(sqlRaw, out var sql, out var resolveErr)) return ToolResult.Fail(resolveErr!, "blob not resolved");

            if (!TryResolve(ToolJson.GetStringTrimmed(root, "connection"), out var cfg, out var err))
                return ToolResult.Fail(err!, "connection not resolved");

            var estimatedRows = ToolJson.GetInt32(root, "estimated_rows", 0);
            var confirmed = root.TryGetProperty("confirmed", out var cv) && cv.GetBoolean();

            // ── Safety pre-flight ──────────────────────────────────────────────
            var risk = SqlSafetyAnalyzer.Analyze(sql);

            if (risk.Level == SqlRiskLevel.Forbidden)
                return ToolResult.Refuse($"Blocked: {risk.Reason}", risk.Reason);

            if (risk.Level == SqlRiskLevel.High && !confirmed)
                return ToolResult.Refuse(
                    $"Requires confirmation — {risk.Reason}\nCall again with confirmed:true after user approves.",
                    risk.Reason);

            if (risk.Level == SqlRiskLevel.Moderate)
            {
                if (risk.RequiresQueryPlan && estimatedRows == 0)
                    return ToolResult.Refuse("Please call sql_query_plan first and pass the result as 'estimated_rows'.", "row estimate required");

                var warning = SqlSafetyAnalyzer.CheckEstimatedRows(estimatedRows);
                if (warning is not null && !confirmed)
                    return ToolResult.Refuse(
                        $"Requires confirmation — {warning}\nCall again with confirmed:true after user approves.",
                        warning);
            }

            // ── Execute ────────────────────────────────────────────────────────
            using var conn = await SqlConnectionFactory.CreateAsync(cfg!, cancellationToken);
            var affected = await conn.ExecuteAsync(sql);
            return ToolResult.Text($"OK — {affected} row(s) affected.");
        }
        catch (JsonException) { return ToolResult.Fail("Error: Invalid JSON arguments.", "invalid json"); }
        catch (Exception ex)  { return ToolResult.Fail($"Error: {ex.Message}", ex.GetType().Name); }
    }
}
