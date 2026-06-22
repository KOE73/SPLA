using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;

namespace SPLA.Plugins.Sql.Tools;

public class SqlVerifyContextTool : SqlToolBase, IMcpTool
{
    public SqlVerifyContextTool(SqlConnectionRegistry registry) : base(registry) { }

    public string Name => "sql_verify_context";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = """
                Compares DB context MD files (referenced in AGENTS.md) against the live schema.
                Reports: tables or columns mentioned in MD but missing in DB (stale),
                DB tables not described anywhere (white spots), FK mismatches.
                Call on demand after schema changes or when context may be outdated.
                """,
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    connection = new { type = "string", description = "Named connection. Omit to use the default." }
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

            // TODO:
            // 1. Locate context MD files for this connection (from AGENTS.md index)
            // 2. Parse table/column names from MD (regex on code blocks and bold names)
            // 3. Run sql_schema to get live schema
            // 4. Diff: stale references, renamed columns, undocumented tables
            return $"[stub] sql_verify_context({cfg!.Provider}): schema vs MD comparison not yet implemented.";
        }
        catch (JsonException) { return "Error: Invalid JSON arguments."; }
        catch (Exception ex)  { return $"Error: {ex.Message}"; }
    }
}
