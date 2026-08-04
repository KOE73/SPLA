using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.MCP.Core.Skills;
using System;
using System.Collections.Generic;
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
    private readonly SkillManager _skills;
    private readonly ToolSets.ToolSetRegistry? _toolSets;

    public SkillActivateTool(SkillManager skills, ToolSets.ToolSetRegistry? toolSets = null)
    {
        _skills = skills;
        _toolSets = toolSets;
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
        var session = AgentSessionScope.Current?.Skills;
        if (session is null) return Task.FromResult("error: no active chat session");

        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var id = ToolJson.GetStringTrimmed(doc.RootElement, "id");

            if (string.IsNullOrEmpty(id))
                return Task.FromResult("error: 'id' parameter is required");

            if (session.ActiveSkillId is not null)
                return Task.FromResult(
                    $"error: skill '{session.ActiveSkillId}' is already active — call skill_deactivate first");

            var skill = _skills.Find(id);
            if (skill is null)
            {
                var suggestions = _skills.GetAvailable()
                    .Where(s => s.Id.Contains(id, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Id)
                    .Take(5)
                    .ToArray();

                var msg = $"error: unknown skill '{id}'";
                if (suggestions.Length > 0)
                    msg += "\nsuggestions:\n" + string.Join("\n", suggestions.Select(s => $"  - {s}"));
                return Task.FromResult(msg);
            }

            // A known-but-unavailable skill reports WHY rather than pretending it does not exist:
            // "needs port_scan — from plugin 'network'" is actionable, "unknown skill" is not.
            if (skill.State != SkillState.Available)
                return Task.FromResult(
                    $"error: skill '{skill.Id}' is not available — {skill.StateReason}");

            // The body is read once, here, and pinned in the session for the run. Editing the file
            // of a running skill therefore cannot swap the procedure mid-flight; the edit applies at
            // the next activation. A source that cannot produce the body fails the activation
            // outright rather than activating into an empty ACTIVE SKILL block.
            var body = _skills.LoadBody(skill.Id);
            if (string.IsNullOrWhiteSpace(body))
                return Task.FromResult(
                    $"error: skill '{skill.Id}' has no readable procedure — its source '{skill.SourceId}' returned nothing");

            // The loan slip: where this skill came from, plus what came with it. Only the LIST of
            // attachments is taken now — their text is fetched on demand, so a procedure that reads
            // two references out of fourteen files pays for two.
            var resources = _skills.ListResources(skill.Id);
            session.Activate(skill.Id, body, skill.SourceId, skill.Ref, resources);

            var raised = RaiseRequiredToolSets(skill);
            var raisedNote = raised.Count > 0
                ? $" Tool sets activated for it: {string.Join(", ", raised)}."
                : string.Empty;
            var resourceNote = resources.Count > 0
                ? $" It carries {resources.Count} resource(s) — read them with skill_read_resource; they are listed in the ACTIVE SKILL block."
                : string.Empty;
            return Task.FromResult($"ok: activated '{skill.Id}' — skill procedure is now injected into the prompt.{raisedNote}{resourceNote} Follow the steps and call skill_deactivate when done.");
        }
        catch (JsonException)
        {
            return Task.FromResult("error: invalid_json");
        }
    }

    /// <summary>
    /// Raises the sets this skill's requirements name and that wait for exactly this — the "on skill
    /// demand" level. Mechanical and free of context: the skill already declared what it needs, so
    /// nothing had to be announced to the model beforehand.
    ///
    /// <para>Only that level is touched. A set the user levelled off stays off (a skill must not
    /// widen the project's boundary), and a fully enabled one needs nothing.</para>
    /// </summary>
    private IReadOnlyList<string> RaiseRequiredToolSets(SkillMeta skill)
    {
        var toolSetSession = AgentSessionScope.Current?.ToolSets;
        if (_toolSets is null || toolSetSession is null) return [];

        var raised = new List<string>();
        foreach (var toolName in skill.Requires.Tools)
        {
            if (_toolSets.SetOfTool(toolName) is not { } setId) continue;
            if (_toolSets.LevelOf(setId) != ToolSets.ToolSetLevel.SkillDemand) continue;
            if (toolSetSession.IsActive(setId)) continue;

            toolSetSession.Activate(setId, ToolSetActivationBy.Skill, $"required by skill '{skill.Id}'");
            raised.Add(setId);
        }

        return raised;
    }
}
