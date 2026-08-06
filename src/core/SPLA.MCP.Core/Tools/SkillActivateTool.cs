using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using SPLA.Library;
using SPLA.Library.Catalog;
using SPLA.Library.Sources;
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
    private readonly SkillLibrary _skills;
    private readonly ToolSets.ToolSetRegistry? _toolSets;

    public SkillActivateTool(SkillLibrary skills, ToolSets.ToolSetRegistry? toolSets = null)
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
            ConversationBound = true,
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

            var lookup = _skills.Resolve(id);

            // Two branches answer to this name. Deciding for the model here is the one failure nobody
            // would think to check for, so it is refused — and the refusal MUST list the addresses,
            // because an ambiguity error without alternatives is just a dead end with extra words.
            if (lookup.IsAmbiguous)
                return Task.FromResult(
                    $"error: '{id}' is held by more than one source — activate one of these by its full address:\n" +
                    string.Join("\n", lookup.Candidates.Select(c => $"  - {c.Address}")));

            var skill = lookup.Card;
            if (skill is null)
            {
                // Suggestions must respect the level. Naming a skill here that the catalog
                // deliberately withheld would make a wrong guess a way of enumerating the fond —
                // and OutOfCatalog means "the model is not told", not "the model is not told twice".
                var suggestions = _skills.Catalog()
                    .Where(s => s.Level is SourceLevel.InCatalog or SourceLevel.OnShelf)
                    .Where(s => s.Id.Contains(id, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.DisplayId)
                    .Take(5)
                    .ToArray();

                var msg = $"error: unknown skill '{id}'";
                msg += suggestions.Length > 0
                    ? "\nsuggestions:\n" + string.Join("\n", suggestions.Select(s => $"  - {s}"))
                    // The observed failure: a weak model guesses an id from a subject word, gets this
                    // error, and thrashes. Naming the recovery turns a dead end into one more call.
                    : "\nDo not guess skill ids. Call skill_find with a subject or with free text to get real ids, then activate one of those.";
                return Task.FromResult(msg);
            }

            // A known-but-unavailable skill reports WHY rather than pretending it does not exist:
            // "needs port_scan — from plugin 'network'" is actionable, "unknown skill" is not.
            if (skill.State != SkillState.Available)
                return Task.FromResult(
                    $"error: skill '{skill.DisplayId}' is not available — {skill.StateReason}");

            // The body is read once, here, and pinned in the session for the run. Editing the file
            // of a running skill therefore cannot swap the procedure mid-flight; the edit applies at
            // the next activation. A source that cannot produce the body fails the activation
            // outright rather than activating into an empty ACTIVE SKILL block.
            // Addressed, not named: re-resolving a bare id here would be a second chance to pick the
            // wrong edition, in the one place where the answer is already settled.
            var body = _skills.LoadBody(skill.Address);
            if (string.IsNullOrWhiteSpace(body))
                return Task.FromResult(
                    $"error: skill '{skill.DisplayId}' has no readable procedure — its source '{skill.SourceId}' returned nothing");

            // The loan slip: where this skill came from, plus what came with it. Only the LIST of
            // attachments is taken now — their text is fetched on demand, so a procedure that reads
            // two references out of fourteen files pays for two.
            var resources = _skills.ListResources(skill.Address);
            session.Activate(skill.DisplayId, body, skill.SourceId, skill.Ref, resources);

            var raised = RaiseRequiredToolSets(skill);
            var raisedNote = raised.Count > 0
                ? $" Tool sets activated for it: {string.Join(", ", raised)}."
                : string.Empty;
            var resourceNote = resources.Count > 0
                ? $" It carries {resources.Count} resource(s) — read them with skill_read_resource; they are listed in the ACTIVE SKILL block."
                : string.Empty;
            return Task.FromResult($"ok: activated '{skill.DisplayId}' — skill procedure is now injected into the prompt.{raisedNote}{resourceNote} Follow the steps and call skill_deactivate when done.");
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
    private IReadOnlyList<string> RaiseRequiredToolSets(SkillCard skill)
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
