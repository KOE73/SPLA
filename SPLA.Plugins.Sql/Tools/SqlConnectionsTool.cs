using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.Plugins.Sql.Tools;

public class SqlConnectionsTool : IMcpTool
{
    private readonly SqlConnectionRegistry _registry;

    public SqlConnectionsTool(SqlConnectionRegistry registry) => _registry = registry;

    public string Name => "sql_connections";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Lists available database connections with their descriptions. Call first when unsure which database to use.",
            Scope = ToolScope.Local,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            Parameters = new
            {
                type = "object",
                properties = new { },
                required = Array.Empty<string>()
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        if (_registry.Connections.Count == 0)
            return Task.FromResult("No database connections configured. Use sql_manage_connection to add connections.");

        var sb = new StringBuilder();
        sb.AppendLine("Available connections:");
        foreach (var (name, cfg) in _registry.Connections)
        {
            var mark = string.Equals(name, _registry.DefaultConnection, StringComparison.OrdinalIgnoreCase)
                ? " [default]" : "";
            sb.AppendLine($"  {name}{mark} ({cfg.Provider}) — {cfg.Description ?? "(no description)"}");
        }
        return Task.FromResult(sb.ToString().TrimEnd());
    }
}
