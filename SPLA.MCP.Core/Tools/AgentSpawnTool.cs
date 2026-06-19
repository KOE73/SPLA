using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Spawns a headless agent to run a skill to completion and return the result.
/// new Agent(mode) semantics: same code as the interactive agent, isolated session.
/// Bypasses skill_activate / skill_deactivate routing — the skill is activated
/// directly in the spawned session and does not affect the parent session.
/// </summary>
public sealed class AgentSpawnTool : IMcpTool, IToolHelpProvider
{
    private readonly IAgentSpawner _runner;

    public AgentSpawnTool(IAgentSpawner runner) => _runner = runner;

    public string Name => "agent_spawn";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "[H] Spawns a headless agent to run a skill to completion and return the result. Isolated from the current session.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    skill = new
                    {
                        type = "string",
                        description = "Skill id to run (e.g. network.range-audit)."
                    },
                    input = new
                    {
                        type = "string",
                        description = "User message / context to seed the spawned agent with."
                    },
                    mode = new
                    {
                        type = new[] { "string", "null" },
                        @enum = new[] { "Chat", "Research", "Inspect", "Edit", "Agent" },
                        description = "Agent mode for the spawned run. Null = Edit. Use a stricter mode to limit what the spawned agent can do."
                    }
                },
                required = new[] { "skill", "input", "mode" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var skillId = ToolJson.GetStringTrimmed(root, "skill");
            if (string.IsNullOrEmpty(skillId))
                return "error: 'skill' is required";

            var input = ToolJson.GetStringTrimmed(root, "input");
            if (string.IsNullOrEmpty(input))
                return "error: 'input' is required";

            var mode    = AgentMode.Edit;
            var modeStr = ToolJson.GetStringTrimmed(root, "mode");
            if (modeStr != null) Enum.TryParse<AgentMode>(modeStr, ignoreCase: true, out mode);

            var result = await _runner.RunSkillAsync(skillId!, input!, mode, cancellationToken);
            return string.IsNullOrWhiteSpace(result)
                ? $"spawn: completed skill '{skillId}' (no output)"
                : result;
        }
        catch (ArgumentException ex)
        {
            return $"error: {ex.Message}";
        }
        catch (JsonException)
        {
            return "error: invalid_json";
        }
    }

    public string? GetHelpText() => """
        tool: agent_spawn

        summary: Runs a skill headlessly in an isolated agent session and returns the result.
                 The spawned agent uses the same tools as the parent but has its own conversation,
                 skill session, and memory. Does not affect the parent session state.

        arguments:
          skill:
            required: true
            description: Skill id to execute (e.g. network.range-audit).
          input:
            required: true
            description: Seed message — what the spawned agent should do within the skill.
          mode:
            required: false
            default: Edit
            values: Chat | Research | Inspect | Edit | Agent
            description: Mode for the spawned run. Use a stricter mode to limit capabilities.
                         The spawned mode may differ from the parent mode.

        returns:
          The last assistant message produced by the skill run.
          "spawn: completed skill '<id>' (no output)" if the skill produced no text.
          "error: ..." on validation failure.

        notes:
          - skill_activate / skill_deactivate are NOT called in the parent session.
          - The spawned session is fully isolated — no shared KV, no shared active skill.
          - Clarification (agent_clarify) in the spawned agent returns no_handler —
            seed the input with enough context to avoid disambiguation.

        examples:
          - request:
              skill: network.range-audit
              input: "Audit 10.0.0.0/24 and report open ports"
              mode: Agent
          - request:
              skill: host-audit
              input: "Full audit of 10.0.0.5"
        """;
}
