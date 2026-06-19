using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Attaches a named rollback mark to the current conversation position.
/// The mark is stored on the message itself; searching is done by name via mark_rollback.
/// </summary>
public sealed class MarkSetTool : IMcpTool
{
    private readonly MarkManager _marks;

    public MarkSetTool(MarkManager marks) => _marks = marks;

    public string Name => "mark_set";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Attach a named rollback mark to the current conversation position. Use for explicit named checkpoints in complex loops. Pair with mark_rollback. Each message holds at most one mark; re-using a name moves it. Optional resume note is shown after rollback when the anchor is an agent message.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    name = new { type = "string", description = "Mark name, e.g. 'loop-start', 'before-api-calls'." },
                    resume = new { type = "string", description = "Note to yourself shown after rollback. Describe what to do next. Omit for simple loops." }
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

        var name = nameProp.GetString()!;
        string? resume = null;
        if (root.TryGetProperty("resume", out var r))
            resume = r.GetString();

        return Task.FromResult(_marks.MarkSet(name, resume));
    }
}
