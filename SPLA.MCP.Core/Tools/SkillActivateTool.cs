using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Plugins;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Activates a skill — transitions the session from Idle to Active.
/// Gated by mode: Chat and Inspect require user confirmation (PermissionResult.Ask).
/// Skill-scoped so the standard PermissionManager routes it correctly.
/// </summary>
public sealed class SkillActivateTool : IMcpTool
{
    private readonly ISkillSession _session;
    private readonly SkillManager _skills;

    public SkillActivateTool(ISkillSession session, SkillManager skills)
    {
        _session = session;
        _skills = skills;
    }

    public string Name => "skill_activate";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Activates a skill and injects its procedure into the current session. Returns an error if another skill is already active.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    id = new
                    {
                        type = "string",
                        description = "Skill id to activate (e.g. network.range-audit)."
                    }
                },
                required = new[] { "id" }
            }
        }
    };

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var id = ToolJson.GetStringTrimmed(doc.RootElement, "id");

            if (string.IsNullOrEmpty(id))
                return Task.FromResult("error: 'id' parameter is required");

            if (_session.ActiveSkillId is not null)
                return Task.FromResult(
                    $"error: skill '{_session.ActiveSkillId}' is already active — call skill_deactivate first");

            var skill = _skills.Find(id);
            if (skill is null)
            {
                var suggestions = _skills.GetEnabled()
                    .Where(s => s.Id.Contains(id, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Id)
                    .Take(5)
                    .ToArray();

                var msg = $"error: unknown skill '{id}'";
                if (suggestions.Length > 0)
                    msg += "\nsuggestions:\n" + string.Join("\n", suggestions.Select(s => $"  - {s}"));
                return Task.FromResult(msg);
            }

            _session.Activate(id);
            return Task.FromResult($"ok: activated '{id}' — skill procedure is now injected into the prompt. Follow the steps and call skill_deactivate when done.");
        }
        catch (JsonException)
        {
            return Task.FromResult("error: invalid_json");
        }
    }
}
