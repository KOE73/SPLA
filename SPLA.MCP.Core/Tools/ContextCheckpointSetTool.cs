using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Pushes the current conversation position onto the checkpoint stack.
/// Pair with context_rollback to pop and truncate back here.
/// </summary>
public sealed class ContextCheckpointSetTool : IMcpTool
{
    private readonly MarkManager _marks;

    public ContextCheckpointSetTool(MarkManager marks) => _marks = marks;

    public string Name => "checkpoint_save";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Push the current conversation position onto the checkpoint stack. Call BEFORE processing one loop item. Pair with context_rollback which pops and truncates back here. Supports nesting — each save/rollback pair is independent. Optional resume note is shown to you after rollback when the rollback anchor is an agent message.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    resume = new { type = "string", description = "Note to yourself shown after rollback. Describe what to do next. Omit for simple loops." }
                },
                required = new string[] { }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        string? resume = null;
        if (!string.IsNullOrWhiteSpace(argumentsJson) && argumentsJson != "{}")
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            if (doc.RootElement.TryGetProperty("resume", out var r))
                resume = r.GetString();
        }
        return Task.FromResult(_marks.CheckpointSave(resume));
    }
}
