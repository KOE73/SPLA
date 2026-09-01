using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Json;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.MCP.Core.Tools;

/// <summary>
/// Lists this chat's background tasks — the model's own view of what <c>BackgroundStage</c> has
/// launched, since the result of a still-running one has not arrived yet and nothing else tells the
/// model whether one exists at all. See
/// <c>docs/adr/ADR_20260824-2_core_background-tool-calls.md</c> and plan step 1.5.
/// <para>
/// <see cref="ToolScope.Agent"/> and <see cref="ToolFunctionDefinition.ConversationBound"/>: a task
/// list means something only inside the chat that launched the tasks, the same reasoning
/// <c>mark_set</c>/<c>agent_clarify</c> already apply to state that belongs to one conversation.</para>
/// </summary>
public sealed class TaskListTool : IMcpTool
{
    public string Name => "task_list";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "List this chat's background tasks (started with background: true), running and recently finished.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            ConversationBound = true,
            Parameters = new { type = "object", properties = new { } }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var host = AgentSessionScope.Current?.Background;
        if (host is null) return Task.FromResult(ToolResult.Text("No background tasks — this chat cannot run them."));

        var tasks = host.Tasks.All;
        if (tasks.Count == 0) return Task.FromResult(ToolResult.Text("No background tasks."));

        var lines = tasks.Select(t =>
        {
            var when = t.FinishedAt is { } f ? $"finished {f:HH:mm:ss}" : $"started {t.StartedAt:HH:mm:ss}";
            return $"{t.Id}  {t.ToolName}  {t.State}  {when}  args: {t.ArgumentsSummary}";
        });

        return Task.FromResult(ToolResult.Text(string.Join("\n", lines)));
    }
}

/// <summary>
/// Reads one background task's outcome — its finished result if it is done, or a note that it is
/// still running plus the live tail of its progress tree if not.
/// </summary>
public sealed class TaskOutputTool : IMcpTool
{
    public string Name => "task_output";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Read a background task's result. Fine to call again — a finished task's result " +
                          "does not go away after task_list has shown it once, unlike the one-time delivery message.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Read,
            Risk = ToolRisk.Low,
            ConversationBound = true,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    task_id = new { type = "string", description = "The bg_N id from task_list or the launch message." }
                },
                required = new[] { "task_id" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var host = AgentSessionScope.Current?.Background;
        if (host is null) return Task.FromResult(ToolResult.Refuse("This chat cannot run background tasks.", "no background host"));

        using var doc = JsonDocument.Parse(argumentsJson);
        var taskId = ToolJson.GetStringTrimmed(doc.RootElement, "task_id");
        if (string.IsNullOrEmpty(taskId))
            return Task.FromResult(ToolResult.Fail("error: 'task_id' is required", "missing task_id"));

        if (!host.Tasks.TryGet(taskId, out var record))
            return Task.FromResult(ToolResult.Refuse($"No background task '{taskId}' in this chat.", "unknown task"));

        if (record.State == BackgroundTaskState.Running)
        {
            var latest = record.ProgressTreeId is { } treeId && host.Progress.Trees.TryGetValue(treeId, out var tree)
                ? tree.Nodes.LastOrDefault()?.Latest?.Message
                : null;
            var tail = latest is null ? "" : $" Latest: {latest}";
            return Task.FromResult(ToolResult.Text($"{taskId} ({record.ToolName}) is still running.{tail}"));
        }

        return Task.FromResult(ToolResult.Text(
            $"{taskId} ({record.ToolName}) {record.State.ToString().ToLowerInvariant()}.\n{record.Result?.TextContent}"));
    }
}

/// <summary>Cancels a live background task. Cancelling a finished or unknown one is reported, not
/// refused — the model asking about a task that already ended is not an error, it is a race it lost.</summary>
public sealed class TaskCancelTool : IMcpTool
{
    public string Name => "task_cancel";

    public ToolDefinition GetDefinition() => new()
    {
        Type = "function",
        Function = new ToolFunctionDefinition
        {
            Name = Name,
            Description = "Cancel a background task started with background: true.",
            Scope = ToolScope.Agent,
            Effect = ToolEffect.Write,
            Risk = ToolRisk.Low,
            ConversationBound = true,
            StrictSchema = true,
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    task_id = new { type = "string", description = "The bg_N id from task_list or the launch message." }
                },
                required = new[] { "task_id" }
            }
        }
    };

    public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        var host = AgentSessionScope.Current?.Background;
        if (host is null) return Task.FromResult(ToolResult.Refuse("This chat cannot run background tasks.", "no background host"));

        using var doc = JsonDocument.Parse(argumentsJson);
        var taskId = ToolJson.GetStringTrimmed(doc.RootElement, "task_id");
        if (string.IsNullOrEmpty(taskId))
            return Task.FromResult(ToolResult.Fail("error: 'task_id' is required", "missing task_id"));

        return Task.FromResult(host.Tasks.Cancel(taskId)
            ? ToolResult.Text($"{taskId} cancelled. Its result will still arrive, marked cancelled.")
            : ToolResult.Text($"{taskId} is not a running task — nothing to cancel."));
    }
}
