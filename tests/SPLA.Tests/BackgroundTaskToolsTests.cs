using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Tools;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

public class BackgroundTaskToolsTests
{
    private sealed class FakeAgentSession : IAgentSession
    {
        public IKeyValueStore SessionKv => throw new NotSupportedException();
        public IBlobStore Blobs => throw new NotSupportedException();
        public MarkManager Checkpoint => throw new NotSupportedException();
        public ISkillSession Skills => throw new NotSupportedException();
        public IToolSetSession ToolSets => throw new NotSupportedException();
        public ISandbox Sandbox => throw new NotSupportedException();
        public SPLA.Domain.Security.ChatDoubt Doubt => throw new NotSupportedException();
        public IBackgroundTaskHost? Background { get; init; }
    }

    private sealed class FakeBackgroundHost : IBackgroundTaskHost
    {
        public BackgroundTaskRegistry Tasks { get; } = new();
        public ProgressHub Progress { get; } = new();
        public ChatInbox Inbox { get; } = new();
    }

    // ---- task_list ------------------------------------------------------------------------

    [Fact]
    public async Task TaskList_with_no_session_scope_says_so_rather_than_erroring()
    {
        var result = await new TaskListTool().ExecuteAsync("{}");
        Assert.False(result.IsError);
        Assert.Contains("cannot run", result.TextContent);
    }

    [Fact]
    public async Task TaskList_empty_registry_says_none()
    {
        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = new FakeBackgroundHost() });
        var result = await new TaskListTool().ExecuteAsync("{}");
        Assert.Equal("No background tasks.", result.TextContent);
    }

    [Fact]
    public async Task TaskList_lists_a_running_task_by_id_and_tool_name()
    {
        var host = new FakeBackgroundHost();
        host.Tasks.TryStart("system_run_shell", "{}");

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        var result = await new TaskListTool().ExecuteAsync("{}");

        Assert.Contains("bg_1", result.TextContent);
        Assert.Contains("system_run_shell", result.TextContent);
        Assert.Contains("Running", result.TextContent);
    }

    // ---- task_output ------------------------------------------------------------------------

    [Fact]
    public async Task TaskOutput_missing_task_id_fails()
    {
        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = new FakeBackgroundHost() });
        var result = await new TaskOutputTool().ExecuteAsync("{}");
        Assert.True(result.IsError);
    }

    [Fact]
    public async Task TaskOutput_unknown_id_is_refused_not_a_hard_error()
    {
        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = new FakeBackgroundHost() });
        var result = await new TaskOutputTool().ExecuteAsync("{\"task_id\":\"bg_999\"}");
        Assert.Equal(ToolOutcome.Refused, result.Outcome);
    }

    [Fact]
    public async Task TaskOutput_reports_still_running_for_a_live_task()
    {
        var host = new FakeBackgroundHost();
        var (record, _) = host.Tasks.TryStart("t", "{}");

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        var result = await new TaskOutputTool().ExecuteAsync($"{{\"task_id\":\"{record!.Id}\"}}");

        Assert.Contains("still running", result.TextContent);
    }

    [Fact]
    public async Task TaskOutput_returns_the_finished_result_and_can_be_asked_again()
    {
        var host = new FakeBackgroundHost();
        var (record, _) = host.Tasks.TryStart("t", "{}");
        host.Tasks.Finish(record!.Id, BackgroundTaskState.Completed, ToolResult.Text("the answer"));

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        var first = await new TaskOutputTool().ExecuteAsync($"{{\"task_id\":\"{record.Id}\"}}");
        var second = await new TaskOutputTool().ExecuteAsync($"{{\"task_id\":\"{record.Id}\"}}");

        Assert.Contains("the answer", first.TextContent);
        Assert.Contains("the answer", second.TextContent); // not a one-time read
    }

    // ---- task_cancel ------------------------------------------------------------------------

    [Fact]
    public async Task TaskCancel_on_a_running_task_cancels_its_token()
    {
        var host = new FakeBackgroundHost();
        var (record, _) = host.Tasks.TryStart("t", "{}");

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        var result = await new TaskCancelTool().ExecuteAsync($"{{\"task_id\":\"{record!.Id}\"}}");

        Assert.False(result.IsError);
        Assert.True(record.Cts.IsCancellationRequested);
    }

    [Fact]
    public async Task TaskCancel_on_an_unknown_task_reports_rather_than_errors()
    {
        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = new FakeBackgroundHost() });
        var result = await new TaskCancelTool().ExecuteAsync("{\"task_id\":\"bg_nope\"}");

        Assert.False(result.IsError);
        Assert.Contains("nothing to cancel", result.TextContent);
    }
}
