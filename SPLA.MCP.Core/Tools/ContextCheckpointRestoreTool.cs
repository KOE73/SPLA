using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Pops the top checkpoint from the stack and signals the orchestrator to truncate back to it.
/// All intermediate reasoning since checkpoint_save is discarded; KV memory is unaffected.
/// </summary>
public sealed class ContextCheckpointRestoreTool : IMcpTool
{
    private readonly MarkManager _marks;

    public ContextCheckpointRestoreTool(MarkManager marks) => _marks = marks;

    public string Name => "context_rollback";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Pop the top checkpoint (set by checkpoint_save) and truncate the conversation back to that position, discarding intermediate reasoning. KV memory is not affected. Call ONCE per loop iteration, AFTER writing all results to KV. Skip on the last iteration.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new { },
                required = new string[] { }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
        => Task.FromResult(_marks.ContextRollback());
}
