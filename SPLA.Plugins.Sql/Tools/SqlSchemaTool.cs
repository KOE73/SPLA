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

public class SqlSchemaTool : SqlToolBase, IMcpTool
{
    public SqlSchemaTool(SqlConnectionRegistry registry) : base(registry) { }

    public string Name => "sql_schema";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = """
                Returns database schema: tables, columns (name, type, nullable), primary keys, foreign keys, indexes.
                Omit 'table' to list all tables with row counts.
                Specify 'table' to inspect a single table in detail.
                Call before sql_query on an unfamiliar table.
                """,
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    connection  = new { type = "string", description = "Named connection. Omit to use the default." },
                    table       = new { type = "string", description = "Table name to inspect. Omit to list all tables." },
                    schema      = new { type = "string", description = "Schema filter (e.g. 'dbo'). Optional." },
                    output      = SchemaParts.Output,
                    output_name = SchemaParts.OutputName
                },
                required = Array.Empty<string>()
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            if (!TryResolve(ToolJson.GetStringTrimmed(root, "connection"), out var cfg, out var err))
                return err!;

            var table  = ToolJson.GetStringTrimmed(root, "table");
            var schema = ToolJson.GetStringTrimmed(root, "schema");

            using var conn = await SqlConnectionFactory.CreateAsync(cfg!, cancellationToken);

            var result = cfg!.Provider.ToLowerInvariant() switch
            {
                "mssql" => table is null
                    ? await MssqlListTablesAsync(conn, schema)
                    : await MssqlDescribeTableAsync(conn, table, schema),
                _ => "[stub] provider schema not yet implemented"
            };

            var target = DataChannel.ParseTarget(ToolJson.GetStringTrimmed(root, "output"));
            if (target == OutputTarget.Context) return result;
            var name = ToolJson.GetStringTrimmed(root, "output_name");
            var summary = table is null ? $"sql_schema: table list" : $"sql_schema: {schema ?? "dbo"}.{table}";
            return DataChannel.Route(target, BlobPayload.OfText(result), summary, name);
        }
        catch (JsonException) { return "Error: Invalid JSON arguments."; }
        catch (Exception ex)  { return $"Error: {ex.Message}"; }
    }

    // ── MSSQL ──────────────────────────────────────────────────────────────────

    private static async Task<string> MssqlListTablesAsync(System.Data.IDbConnection conn, string? schema)
    {
        var schemaFilter = schema is null ? "" : "AND s.name = @schema";
        var sql = $"""
            SELECT s.name AS [schema], t.name AS [table],
                   SUM(p.rows) AS row_count
            FROM sys.tables t
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            LEFT JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0,1)
            WHERE 1=1 {schemaFilter}
            GROUP BY s.name, t.name
            ORDER BY s.name, t.name
            """;

        var rows = await conn.QueryAsync(sql, schema is null ? null : new { schema });
        var sb = new StringBuilder();
        sb.AppendLine("Tables:");
        foreach (var r in rows)
            sb.AppendLine($"  {r.schema}.{r.table}  ({r.row_count} rows)");
        return sb.ToString().TrimEnd();
    }

    private static async Task<string> MssqlDescribeTableAsync(System.Data.IDbConnection conn, string table, string? schema)
    {
        var schemaPart = schema ?? "dbo";

        // Columns
        var colSql = """
            SELECT c.name, tp.name AS type,
                   c.max_length, c.precision, c.scale,
                   c.is_nullable, c.is_identity,
                   CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS is_pk
            FROM sys.columns c
            JOIN sys.types tp ON c.user_type_id = tp.user_type_id
            JOIN sys.tables t ON c.object_id = t.object_id
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            LEFT JOIN (
                SELECT ic.object_id, ic.column_id
                FROM sys.index_columns ic
                JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                WHERE i.is_primary_key = 1
            ) pk ON pk.object_id = c.object_id AND pk.column_id = c.column_id
            WHERE t.name = @table AND s.name = @schema
            ORDER BY c.column_id
            """;

        var cols = (await conn.QueryAsync(colSql, new { table, schema = schemaPart })).ToList();

        // Foreign keys
        var fkSql = """
            SELECT fk.name AS fk_name,
                   COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS column_name,
                   OBJECT_NAME(fkc.referenced_object_id) AS ref_table,
                   COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ref_column
            FROM sys.foreign_keys fk
            JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            JOIN sys.tables t ON fk.parent_object_id = t.object_id
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE t.name = @table AND s.name = @schema
            """;

        var fks = (await conn.QueryAsync(fkSql, new { table, schema = schemaPart })).ToList();

        // Indexes
        var idxSql = """
            SELECT i.name, i.type_desc, i.is_unique, i.is_primary_key,
                   STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS columns
            FROM sys.indexes i
            JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            JOIN sys.tables t ON i.object_id = t.object_id
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE t.name = @table AND s.name = @schema AND i.type > 0
            GROUP BY i.name, i.type_desc, i.is_unique, i.is_primary_key
            """;

        var idxs = (await conn.QueryAsync(idxSql, new { table, schema = schemaPart })).ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"Table: {schemaPart}.{table}");
        sb.AppendLine();
        sb.AppendLine("Columns:");
        foreach (var c in cols)
        {
            var pk   = (bool)c.is_pk       ? " PK" : "";
            var nn   = !(bool)c.is_nullable  ? " NOT NULL" : "";
            var id   = (bool)c.is_identity  ? " IDENTITY" : "";
            sb.AppendLine($"  {c.name,-30} {c.type}{pk}{nn}{id}");
        }

        if (fks.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Foreign Keys:");
            foreach (var fk in fks)
                sb.AppendLine($"  {fk.column_name} → {fk.ref_table}.{fk.ref_column}  ({fk.fk_name})");
        }

        if (idxs.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Indexes:");
            foreach (var i in idxs)
            {
                var uniq = (bool)i.is_unique ? "UNIQUE " : "";
                sb.AppendLine($"  {uniq}{i.type_desc}: {i.columns}  ({i.name})");
            }
        }

        return sb.ToString().TrimEnd();
    }
}
