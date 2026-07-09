using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Sends a structured question to the user and awaits their choice.
/// Uses <see cref="ClarifyScope"/> — same ambient-channel pattern as ProgressScope.
/// When no scope is active (autonomous/headless), returns null immediately so the agent can
/// proceed with a default or skip the question.
/// Agent-scoped: always allowed — asking is information gathering, not a side effect.
/// </summary>
public sealed class AgentClarifyTool : IMcpTool, IToolHelpProvider
{
    public string Name => "agent_clarify";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "[H] Sends a structured question with options to the user and returns their choice. Returns no_handler in autonomous/headless mode.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    question = new
                    {
                        type = "string",
                        description = "The question to display to the user."
                    },
                    options = new
                    {
                        type = "array",
                        description = "Ordered list of choices.",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                label       = new { type = "string",              description = "Short choice label." },
                                description = new { type = new[] { "string", "null" }, description = "Optional explanation shown alongside the label. Null = no description." }
                            },
                            required = new[] { "label", "description" }
                        }
                    }
                },
                required = new[] { "question", "options" }
            }
        }
    };

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            var question = ToolJson.GetString(root, "question");
            if (string.IsNullOrWhiteSpace(question))
                return "error: 'question' is required";

            if (!root.TryGetProperty("options", out var optEl) || optEl.ValueKind != JsonValueKind.Array)
                return "error: 'options' array is required";

            var options = optEl.EnumerateArray()
                .Select(o =>
                {
                    var label = ToolJson.GetString(o, "label") ?? string.Empty;
                    var desc  = ToolJson.GetString(o, "description");
                    return new ClarifyOption { Label = label, Description = desc };
                })
                .Where(o => !string.IsNullOrWhiteSpace(o.Label))
                .ToArray();

            if (options.Length == 0)
                return "error: at least one option with a non-empty label is required";

            var request = new ClarifyRequest { Question = question!, Options = options };
            var chosen = await ClarifyScope.AskAsync(request);

            if (chosen is null)
                return "clarify: no_handler — proceeding without user input";

            return $"chosen: {chosen}";
        }
        catch (JsonException)
        {
            return "error: invalid_json";
        }
    }

    public string? GetHelpText() => """
        tool: agent_clarify

        summary: Asks the user a structured question and returns their chosen option.
                 Uses an ambient channel — no scope active means headless/autonomous mode (returns null).

        arguments:
          question:
            required: true
            type: string
            description: The question shown to the user.
          options:
            required: true
            type: array of { label: string, description?: string }
            minimum: 1 item

        returns:
          "chosen: <label>"          — the label of the option the user selected
          "clarify: no_handler"      — no UI/CLI handler is active (autonomous mode), proceed with default

        examples:
          - request:
              question: "Ready to run network.range-audit on 10.0.0.0/24?"
              options:
                - { label: "Yes", description: "Proceed with the scan" }
                - { label: "No",  description: "Cancel" }
          - request:
              question: "Which skill should I use?"
              options:
                - { label: "network.range-audit" }
                - { label: "host-audit" }
        """;
}
