using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System;
using System.Collections.Generic;
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

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Spawns multiple headless agents in parallel (bounded concurrency) and returns all results. Each task is a plain 'input' brief, optionally pinned to a 'skill'. Use for bulk operations like checking many hosts.",
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
                                }
                            },
                            required = new[] { "input", "skill", "mode" }
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
        List<(string? skill, string input, AgentMode mode)> tasks;
        int maxConcurrency;

        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("tasks", out var tasksEl) || tasksEl.ValueKind != JsonValueKind.Array)
                return ToolResult.Fail("error: 'tasks' array is required", "missing tasks");

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

                tasks.Add((skill, input!, mode));
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
            var (skill, input, mode) = tasks[i];
            workers[i] = Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await _runner.RunAsync(skill, input, mode, cancellationToken);
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
