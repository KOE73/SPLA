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
/// Each task runs the same skill with a different input. Results are returned in input order.
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
            Description = "Spawns multiple headless agents in parallel (bounded concurrency) and returns all results. Each task runs a skill with a different input. Use for bulk operations like scanning many hosts.",
            Scope = ToolScope.Skill,
            Effect = ToolEffect.Execute,
            Risk = ToolRisk.Medium,
            StrictSchema = true,
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
                                skill = new { type = "string", description = "Skill id to run." },
                                input = new { type = "string", description = "Seed message for the spawned agent." },
                                mode = new
                                {
                                    type = new[] { "string", "null" },
                                    @enum = new[] { "Chat", "Research", "Inspect", "Edit", "Agent" },
                                    description = "Agent mode. Null = Edit."
                                }
                            },
                            required = new[] { "skill", "input", "mode" }
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

    public async Task<string> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        List<(string skill, string input, AgentMode mode)> tasks;
        int maxConcurrency;

        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("tasks", out var tasksEl) || tasksEl.ValueKind != JsonValueKind.Array)
                return "error: 'tasks' array is required";

            tasks = new();
            foreach (var item in tasksEl.EnumerateArray())
            {
                var skill = ToolJson.GetStringTrimmed(item, "skill");
                var input = ToolJson.GetStringTrimmed(item, "input");
                if (string.IsNullOrEmpty(skill) || string.IsNullOrEmpty(input))
                    return "error: each task requires 'skill' and 'input'";

                var mode = AgentMode.Edit;
                var modeStr = ToolJson.GetStringTrimmed(item, "mode");
                if (modeStr != null) Enum.TryParse<AgentMode>(modeStr, ignoreCase: true, out mode);

                tasks.Add((skill!, input!, mode));
            }

            maxConcurrency = 3;
            if (root.TryGetProperty("max_concurrency", out var concEl) && concEl.ValueKind == JsonValueKind.Number)
                maxConcurrency = Math.Clamp(concEl.GetInt32(), 1, 10);
        }
        catch (JsonException)
        {
            return "error: invalid_json";
        }

        if (tasks.Count == 0)
            return "error: tasks array is empty";

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
                    var result = await _runner.RunSkillAsync(skill, input, mode, cancellationToken);
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
        return sb.ToString().TrimEnd();
    }
}
