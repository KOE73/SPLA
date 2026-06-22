using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Sql.Tools;

/// <summary>
/// Opens a named connection to verify reachability and credentials, without running any query.
/// Shares its implementation with the settings-editor "Test Connection" button via
/// <see cref="SqlConnectionTester"/>.
/// </summary>
public class SqlTestConnectionTool : SqlToolBase, IMcpTool
{
    public SqlTestConnectionTool(SqlConnectionRegistry registry) : base(registry) { }

    public string Name => "sql_test_connection";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Verifies that a database connection can be opened (reachability + credentials), without running a query. Use to diagnose connectivity before querying.",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    connection = new
                    {
                        type = "string",
                        description = "Name of the connection to test. Omit to use the default connection."
                    }
                },
                required = Array.Empty<string>()
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? name = null;
        if (!string.IsNullOrWhiteSpace(argumentsJson))
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (doc.RootElement.TryGetProperty("connection", out var c) && c.ValueKind == JsonValueKind.String)
                name = c.GetString();
        }

        if (!TryResolve(name, out var cfg, out var error))
            return error!;

        var result = await SqlConnectionTester.TestAsync(cfg!, cancellationToken);
        return result.Message;
    }
}
