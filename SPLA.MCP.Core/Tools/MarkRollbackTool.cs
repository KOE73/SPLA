using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Signals the orchestrator to truncate the conversation back to the message
/// carrying the specified named mark (set via mark_set). KV memory is unaffected.
/// </summary>
public sealed class MarkRollbackTool : IMcpTool
{
    private readonly MarkManager _marks;

    public MarkRollbackTool(MarkManager marks) => _marks = marks;

    public string Name => "mark_rollback";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Truncate the conversation back to the named mark set by mark_set, discarding all intermediate reasoning. KV memory is not affected. The mark remains on the anchor message and can be used again.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    name = new { type = "string", description = "The mark name to roll back to." }
                },
                required = new[] { "name" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse(argumentsJson);
        var root = doc.RootElement;
        if (!root.TryGetProperty("name", out var nameProp) || string.IsNullOrWhiteSpace(nameProp.GetString()))
            return Task.FromResult("error: 'name' is required");

        return Task.FromResult(_marks.MarkRollback(nameProp.GetString()!));
    }
}
