using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Deactivates the current skill — transitions the session from Active to Idle.
/// Agent-scoped: always allowed in every mode. Stopping is always safe.
/// </summary>
public sealed class SkillDeactivateTool : IMcpTool
{
    private readonly ISkillSession _session;

    public SkillDeactivateTool(ISkillSession session) => _session = session;

    public string Name => "skill_deactivate";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Deactivates the current skill and removes its procedure from the prompt. No-op if no skill is active.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new { },
                required = System.Array.Empty<string>()
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var previous = _session.ActiveSkillId;
        _session.Deactivate();
        return Task.FromResult(previous is not null
            ? $"ok: deactivated '{previous}'"
            : "ok: no active skill");
    }
}
