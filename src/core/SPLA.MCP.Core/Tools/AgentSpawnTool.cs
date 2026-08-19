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
/// Spawns a headless agent to run a task to completion and return the result.
/// new Agent(mode) semantics: same code as the interactive agent, isolated session.
/// With a skill named, skill_activate / skill_deactivate routing is bypassed — the skill is
/// activated directly in the spawned session and does not affect the parent session. Without one,
/// the task is the prompt itself.
/// </summary>
public sealed class AgentSpawnTool : IMcpTool
{
    private readonly IAgentSpawner _runner;

    public AgentSpawnTool(IAgentSpawner runner) => _runner = runner;

    public string Name => "agent_spawn";

    /// <summary>Everything about this tool that does not fit its one-line description.
    /// Disclosed together with the tool itself — see <c>ToolFunctionDefinition.Details</c>.</summary>
    private static readonly string DetailsText =
        """
        tool: agent_spawn

        summary: Delegates a task to a headless agent in an isolated session and returns the result.
                 The spawned agent uses the same tools as the parent but has its own conversation,
                 skill session, and memory. Does not affect the parent session state.

        arguments:
          input:
            required: true
            description: The task — what the spawned agent should do. This is the whole brief;
                         it is the only thing the sub-agent knows about your intent.
          skill:
            required: false
            default: none (free-form task)
            description: Skill id to pin for the run (e.g. network.range-audit). Name one when a
                         written procedure covers the work; omit it for an ad-hoc task and describe
                         the work in 'input' instead. A pinned skill is the only thing the sub-agent
                         does; without one it may find and activate a skill itself if one fits.
          mode:
            required: false
            default: Edit
            values: Chat | Research | Inspect | Edit | Agent
            description: Mode for the spawned run. Use a stricter mode to limit capabilities.
                         The spawned mode may differ from the parent mode.

        returns:
          The last assistant message produced by the run.
          "spawn: completed (no output)" if the run produced no text.
          "error: ..." on validation failure.

        notes:
          - Delegate work that would otherwise flood this conversation: bulk reads, wide
            searches, per-host checks. Only the sub-agent's final message comes back.
          - skill_activate / skill_deactivate are NOT called in the parent session.
          - The spawned session is fully isolated — no shared KV, no shared active skill.
            Anything the sub-agent should know must be in 'input'; it cannot see this chat.
          - Ask for what you want back. The return value is the last assistant message, so
            say "report X" in the input or the useful part may stay in the sub-agent's session.
          - Clarification (agent_clarify) in the spawned agent returns no_handler —
            seed the input with enough context to avoid disambiguation.

        examples:
          - request:
              input: "On host MiHomo 21.2, report the OS name and kernel version. Nothing else."
          - request:
              skill: network.range-audit
              input: "Audit 10.0.0.0/24 and report open ports"
              mode: Agent
          - request:
              input: "Read every *.md under Docker/ and list which compose projects define a healthcheck"
              mode: Research
        """;

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Details = DetailsText,
            Description = "[H] Delegates a task to a headless agent and returns its result. Give it a plain task in 'input', or name a 'skill' to pin a written procedure. Isolated from the current session.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    input = new
                    {
                        type = "string",
                        description = "The task for the spawned agent — the whole brief, since it cannot see this conversation."
                    },
                    skill = new
                    {
                        type = new[] { "string", "null" },
                        description = "Skill id to pin (e.g. network.range-audit). Null for a free-form task described in 'input'."
                    },
                    mode = new
                    {
                        type = new[] { "string", "null" },
                        @enum = new[] { "Chat", "Research", "Inspect", "Edit", "Agent" },
                        description = "Agent mode for the spawned run. Null = Edit. Use a stricter mode to limit what the spawned agent can do."
                    }
                },
                required = new[] { "input", "skill", "mode" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var input = ToolJson.GetStringTrimmed(root, "input");
            if (string.IsNullOrEmpty(input))
                return ToolResult.Fail("error: 'input' is required", "missing input");

            // Absent, JSON-null and empty all mean the same thing here — a free-form task. The
            // sub-agent is told what to do by 'input' either way, so there is nothing to refuse.
            var skillId = ToolJson.GetStringTrimmed(root, "skill");

            var mode    = AgentMode.Edit;
            var modeStr = ToolJson.GetStringTrimmed(root, "mode");
            if (modeStr != null) Enum.TryParse<AgentMode>(modeStr, ignoreCase: true, out mode);

            var result = await _runner.RunAsync(skillId, input!, mode, cancellationToken);
            return ToolResult.Text(string.IsNullOrWhiteSpace(result)
                ? (skillId is null
                    ? "spawn: completed (no output)"
                    : $"spawn: completed skill '{skillId}' (no output)")
                : result);
        }
        catch (ArgumentException ex)
        {
            return ToolResult.Fail($"error: {ex.Message}", "invalid argument");
        }
        catch (JsonException)
        {
            return ToolResult.Fail("error: invalid_json", "invalid json");
        }
    }

}
