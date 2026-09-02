using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Spawns multiple headless agents in parallel (bounded concurrency) and collects their results.
/// Each task carries its own input, and its own optional skill. Results are returned in input order.
/// </summary>
public sealed class AgentSpawnBatchTool : IMcpTool
{
    private readonly IAgentSpawner _runner;

    public AgentSpawnBatchTool(IAgentSpawner runner) => _runner = runner;

    public string Name => "agent_spawn_batch";

    /// <summary>Everything about this tool that does not fit its one-line description.
    /// Disclosed together with the tool itself — see <c>ToolFunctionDefinition.Details</c>.</summary>
    private static readonly string DetailsText =
        """
        tool: agent_spawn_batch

        summary: Spawns multiple headless agents in parallel (bounded concurrency) and returns all results.
                 Each task is a plain brief, optionally pinned to a skill or a role.

        arguments:
          tasks:
            required: true
            description: List of tasks to run in parallel. Each task has its own input, optional skill,
                         optional mode, and optional role.
          max_concurrency:
            required: false
            default: 3
            description: Max parallel agents (1–10, default 3).

        task structure:
          input:
            required: true
            description: The task for this spawned agent — the whole brief.
          skill:
            required: false
            default: none (free-form task)
            description: Skill id to pin for this task. Null for a free-form task described in 'input'.
          mode:
            required: false
            default: Edit
            values: Chat | Research | Inspect | Edit | Agent
            description: Agent mode for this task. Null = Edit.
          role:
            required: false
            default: none (default role)
            description: Project role to run under (e.g. reviewer). Null for the default role.

        returns:
          Results for all tasks, one per line, in input order.
          "task N: completed (no output)" if a task produced no text.
          "task N: error: ..." on validation failure.

        notes:
          - Use for bulk operations: checking many hosts, auditing multiple files, running the same
            task against different inputs.
          - Each task runs in a fully isolated session; they do not affect each other.
          - Results are returned in input order, so task pairing is stable.
          - Use 'role' to run all tasks (or each individually) under a specific role with narrowed
            capabilities. Use 'mode' for an ad-hoc run without a role.

        examples:
          - request:
              tasks:
                - input: "Check host alpha for SSH config issues"
                - input: "Check host beta for SSH config issues"
                - input: "Check host gamma for SSH config issues"
          - request:
              tasks:
                - role: reviewer
                  input: "Review src/core/Component.cs for design issues"
                - role: reviewer
                  input: "Review src/core/Handler.cs for design issues"
        """;

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Details = DetailsText,
            Description = "Spawns multiple headless agents in parallel (bounded concurrency) and returns all results. Each task is a plain 'input' brief, optionally pinned to a 'skill' or 'role'. Use for bulk operations.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
            // A batch of sub-agent runs can take as long as the slowest task does. See
            // PLAN_20260824-2 step 1.7.
            SupportsBackground = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    tasks = new
                    {
                        type = "array",
                        description = "List of tasks to run in parallel.",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                input = new { type = "string", description = "The task for this spawned agent — the whole brief." },
                                skill = new
                                {
                                    type = new[] { "string", "null" },
                                    description = "Skill id to pin for this task. Null for a free-form task described in 'input'."
                                },
                                mode = new
                                {
                                    type = new[] { "string", "null" },
                                    @enum = new[] { "Chat", "Research", "Inspect", "Edit", "Agent" },
                                    description = "Agent mode. Null = Edit."
                                },
                                role = new
                                {
                                    type = new[] { "string", "null" },
                                    description = "Project role to run under. Null for the default role."
                                }
                            },
                            required = new[] { "input", "skill", "mode", "role" }
                        }
                    },
                    max_concurrency = new
                    {
                        type = new[] { "integer", "null" },
                        description = "Max parallel agents (1–10, default 3)."
                    }
                },
                required = new[] { "tasks" }
            }
        }
    };

    public async Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        List<(string? skill, string input, AgentMode mode, string? role)> tasks;
        int maxConcurrency;
        IReadOnlyList<string> availableRoles;

        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("tasks", out var tasksEl) || tasksEl.ValueKind != JsonValueKind.Array)
                return ToolResult.Fail("error: 'tasks' array is required", "missing tasks");

            // Pre-load available roles once for validation
            availableRoles = _runner.GetAvailableRoles();

            tasks = new();
            foreach (var item in tasksEl.EnumerateArray())
            {
                var input = ToolJson.GetStringTrimmed(item, "input");
                if (string.IsNullOrEmpty(input))
                    return ToolResult.Fail("error: each task requires 'input'", "incomplete task");

                // Optional, per task: a batch may mix pinned procedures and free-form briefs.
                var skill = ToolJson.GetStringTrimmed(item, "skill");

                var mode = AgentMode.Edit;
                var modeStr = ToolJson.GetStringTrimmed(item, "mode");
                if (modeStr != null) Enum.TryParse<AgentMode>(modeStr, ignoreCase: true, out mode);

                var role = ToolJson.GetStringTrimmed(item, "role");

                // Validate role if one is named.
                if (!string.IsNullOrWhiteSpace(role))
                {
                    if (!availableRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    {
                        var rolesList = availableRoles.Count == 0
                            ? "none declared"
                            : string.Join(", ", availableRoles.OrderBy(r => r, StringComparer.OrdinalIgnoreCase));
                        return ToolResult.Fail(
                            $"error: Role '{role}' is not available. Available roles: {rolesList}",
                            "unknown role");
                    }
                }

                tasks.Add((skill, input!, mode, role));
            }

            maxConcurrency = 3;
            if (root.TryGetProperty("max_concurrency", out var concEl) && concEl.ValueKind == JsonValueKind.Number)
                maxConcurrency = Math.Clamp(concEl.GetInt32(), 1, 10);
        }
        catch (JsonException)
        {
            return ToolResult.Fail("error: invalid_json", "invalid json");
        }

        if (tasks.Count == 0)
            return ToolResult.Fail("error: tasks array is empty", "empty tasks");

        var results = new string[tasks.Count];
        using var semaphore = new SemaphoreSlim(maxConcurrency);

        var workers = new Task[tasks.Count];
        for (int i = 0; i < tasks.Count; i++)
        {
            var idx = i;
            var (skill, input, mode, role) = tasks[i];
            workers[i] = Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await _runner.RunAsync(skill, input, mode, role, cancellationToken);
                    results[idx] = string.IsNullOrWhiteSpace(result)
                        ? $"task {idx + 1}: completed (no output)"
                        : $"task {idx + 1}: {result}";
                }
                catch (Exception ex)
                {
                    results[idx] = $"task {idx + 1}: error: {ex.Message}";
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);
        }

        await Task.WhenAll(workers);

        var sb = new StringBuilder();
        for (int i = 0; i < results.Length; i++)
        {
            if (i > 0) sb.AppendLine("---");
            sb.AppendLine(results[i]);
        }
        return ToolResult.Text(sb.ToString().TrimEnd());
    }
}
