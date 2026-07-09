using System.Data;
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

public class SqlQueryTool : SqlToolBase, IMcpTool
{
    public SqlQueryTool(SqlConnectionRegistry registry) : base(registry) { }

    public string Name => "sql_query";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = """
                Executes a SELECT query and returns results as a table. Read-only.
                INSERT/UPDATE/DELETE are rejected — use sql_execute for writes.
                Estimate result size before querying: prefer fewer columns and rows.
                Default limit is 10 when results go to context. Set output='blob' to dump the full
                result set (no row cap) into a handle you can pass to system_write_file — use this for
                bulk extraction (e.g. pulling all object definitions) without flooding context.
                """,
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    sql        = new { type = "string",  description = "SELECT statement to execute." },
                    connection = new { type = "string",  description = "Named connection from .spla db_connections. Omit to use the default." },
                    limit      = new { type = "integer", description = "Max rows to return (default: 10 for context; uncapped for blob unless set)." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = new[] { "sql" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var sql = ToolJson.GetStringTrimmed(root, "sql");
            if (string.IsNullOrWhiteSpace(sql)) return "Error: Missing 'sql' parameter.";

            var firstToken = sql.TrimStart().Split([' ', '\n', '\r', '\t'], 2)[0].ToUpperInvariant();
            if (firstToken is not ("SELECT" or "WITH" or "EXPLAIN"))
                return $"Error: sql_query is read-only. Got '{firstToken}'. Use sql_execute for writes.";

            if (!TryResolve(ToolJson.GetStringTrimmed(root, "connection"), out var cfg, out var err))
                return err!;

            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));

            // To context: cap at 10 by default. To blob: dump everything unless a limit is given.
            var defaultLimit = target == OutputTarget.Context ? 10 : int.MaxValue;
            var limit = ToolJson.GetInt32(root, "limit", defaultLimit);

            using var conn = await SqlConnectionFactory.CreateAsync(cfg!, cancellationToken);
            var rows = (await conn.QueryAsync(sql)).Take(limit).ToList();

            if (rows.Count == 0) return "(no rows)";

            var table = FormatTable(rows, limit);
            if (target == OutputTarget.Context)
                return table;

            var name = ToolJson.GetStringTrimmed(root, "output_name");
            var summary = $"sql_query: {rows.Count} row(s)";
            return DataChannel.Route(target, BlobPayload.OfText(table), summary, name);
        }
        catch (JsonException) { return "Error: Invalid JSON arguments."; }
        catch (Exception ex)  { return $"Error: {ex.Message}"; }
    }

    private static string FormatTable(List<dynamic> rows, int limit)
    {
        var dicts = rows.Select(r => (IDictionary<string, object?>)r).ToList();
        var cols = dicts[0].Keys.ToList();

        var widths = cols.Select(c => Math.Max(c.Length,
            dicts.Max(r => r[c]?.ToString()?.Length ?? 4))).ToList();

        var sb = new StringBuilder();
        var header = string.Join(" | ", cols.Select((c, i) => c.PadRight(widths[i])));
        var sep    = string.Join("-+-", widths.Select(w => new string('-', w)));
        sb.AppendLine(header);
        sb.AppendLine(sep);
        foreach (var row in dicts)
            sb.AppendLine(string.Join(" | ", cols.Select((c, i) => (row[c]?.ToString() ?? "NULL").PadRight(widths[i]))));

        if (rows.Count == limit)
            sb.AppendLine($"(showing first {limit} rows)");

        return sb.ToString().TrimEnd();
    }
}
