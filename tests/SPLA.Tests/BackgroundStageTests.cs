using SPLA.Domain.Agent;
using SPLA.Domain.Host;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Pipeline;
using SPLA.MCP.Core.Pipeline.Stages;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// <see cref="BackgroundStage"/> in isolation — wrapping a bare delegate through
/// <see cref="ToolPipelineBlueprint"/>, the same technique <c>ToolPipelineOrderTests</c> uses, so
/// these tests exercise exactly the stage's own contract and nothing McpHost wires around it.
/// </summary>
public class BackgroundStageTests
{
    private sealed class FakeTool : IMcpTool
    {
        private readonly Func<string, CancellationToken, Task<ToolResult>> _run;
        public FakeTool(bool supportsBackground, Func<string, CancellationToken, Task<ToolResult>> run)
        {
            _supportsBackground = supportsBackground;
            _run = run;
        }
        private readonly bool _supportsBackground;
        public string Name => "fake_tool";
        public ToolDefinition GetDefinition() => new()
        {
            Function = new ToolFunctionDefinition { Name = Name, SupportsBackground = _supportsBackground }
        };
        public Task<ToolResult> ExecuteAsync(string argumentsJson, CancellationToken ct = default)
            => _run(argumentsJson, ct);
    }

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

    private static ToolCallDelegate BuildPipeline(IMcpTool tool, ToolCallDelegate terminal)
    {
        var pipeline = new ToolPipelineBlueprint().Use(new BackgroundStage()).Build(terminal);
        return (call, ct) => { call.Tool = tool; return pipeline(call, ct); };
    }

    private static async Task<ToolResult> RunAsync(
        ToolCallDelegate pipeline, string argsJson, CancellationToken ct = default)
        => await pipeline(new ToolCallInvocation(AgentMode.Agent, "fake_tool", argsJson), ct);

    [Fact]
    public async Task Tool_without_SupportsBackground_runs_synchronously_even_if_asked()
    {
        var tool = new FakeTool(supportsBackground: false, (_, _) => Task.FromResult(ToolResult.Text("ran")));
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));

        var result = await RunAsync(pipeline, "{\"background\":true}");

        Assert.Equal("ran", result.TextContent);
    }

    [Fact]
    public async Task Background_false_or_absent_runs_synchronously()
    {
        var tool = new FakeTool(supportsBackground: true, (_, _) => Task.FromResult(ToolResult.Text("ran")));
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));

        Assert.Equal("ran", (await RunAsync(pipeline, "{}")).TextContent);
        Assert.Equal("ran", (await RunAsync(pipeline, "{\"background\":false}")).TextContent);
    }

    [Fact]
    public async Task Background_true_with_no_session_capability_degrades_to_synchronous()
    {
        // No AgentSessionScope open at all — the common case for a direct unit-test-style call.
        var tool = new FakeTool(supportsBackground: true, (_, _) => Task.FromResult(ToolResult.Text("ran")));
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));

        var result = await RunAsync(pipeline, "{\"background\":true}");

        Assert.Equal("ran", result.TextContent);
    }

    [Fact]
    public async Task Background_true_with_a_session_whose_Background_is_null_also_degrades()
    {
        var tool = new FakeTool(supportsBackground: true, (_, _) => Task.FromResult(ToolResult.Text("ran")));
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = null });
        var result = await RunAsync(pipeline, "{\"background\":true}");

        Assert.Equal("ran", result.TextContent);
    }

    [Fact]
    public async Task Background_true_with_a_host_returns_a_task_id_immediately_and_registers_a_running_task()
    {
        var gate = new TaskCompletionSource();
        var tool = new FakeTool(supportsBackground: true, async (_, ct) =>
        {
            await gate.Task; // holds the detached run open until the test releases it
            return ToolResult.Text("finished");
        });
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        var result = await RunAsync(pipeline, "{\"background\":true}");

        Assert.False(result.IsError);
        Assert.Contains("bg_", result.TextContent);
        Assert.Single(host.Tasks.All);
        Assert.Equal(BackgroundTaskState.Running, host.Tasks.All[0].State);

        gate.SetResult();
        await WaitUntil(() => host.Tasks.All[0].State != BackgroundTaskState.Running);
        Assert.Equal(BackgroundTaskState.Completed, host.Tasks.All[0].State);
    }

    [Fact]
    public async Task Completion_delivers_exactly_one_inbox_message_naming_the_task()
    {
        var tool = new FakeTool(supportsBackground: true, (_, _) => Task.FromResult(ToolResult.Text("the answer")));
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        var launch = await RunAsync(pipeline, "{\"background\":true}");
        var taskId = host.Tasks.All[0].Id;

        await WaitUntil(() => host.Tasks.All[0].State != BackgroundTaskState.Running);
        var delivered = host.Inbox.DrainAll();

        Assert.Single(delivered);
        Assert.Contains(taskId, delivered[0].Content);
        Assert.Contains("the answer", delivered[0].Content);
        Assert.Empty(host.Inbox.DrainAll()); // delivered once
    }

    [Fact]
    public async Task A_returned_failure_finishes_the_task_as_Failed()
    {
        var tool = new FakeTool(supportsBackground: true, (_, _) => Task.FromResult(ToolResult.Fail("boom")));
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        await RunAsync(pipeline, "{\"background\":true}");
        await WaitUntil(() => host.Tasks.All[0].State != BackgroundTaskState.Running);

        Assert.Equal(BackgroundTaskState.Failed, host.Tasks.All[0].State);
    }

    [Fact]
    public async Task A_thrown_exception_is_converted_not_left_to_crash_the_task()
    {
        var tool = new FakeTool(supportsBackground: true,
            (_, _) => throw new InvalidOperationException("kaboom"));
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        await RunAsync(pipeline, "{\"background\":true}");
        await WaitUntil(() => host.Tasks.All[0].State != BackgroundTaskState.Running);

        Assert.Equal(BackgroundTaskState.Failed, host.Tasks.All[0].State);
        Assert.Contains("kaboom", host.Tasks.All[0].Result!.TextContent);
    }

    [Fact]
    public async Task Cancelling_the_turns_own_token_does_not_touch_the_detached_task()
    {
        // The turn's ct and the task's own Cts are deliberately different tokens — see
        // BackgroundStage.InvokeAsync, which passes record.Cts.Token into `next`, never the `ct` it
        // was called with. This is what "Стоп на ходе не убивает 20-минутную сборку" (plan pitfall 4)
        // actually rests on; assert it directly rather than trusting the wiring by inspection.
        var started = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var tool = new FakeTool(supportsBackground: true, async (_, ct) =>
        {
            started.SetResult();
            await release.Task;
            ct.ThrowIfCancellationRequested();
            return ToolResult.Text("survived");
        });
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();
        using var turnCts = new CancellationTokenSource();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        await RunAsync(pipeline, "{\"background\":true}", turnCts.Token);
        await started.Task;

        turnCts.Cancel(); // the turn ends — the detached task must not notice
        release.SetResult();
        await WaitUntil(() => host.Tasks.All[0].State != BackgroundTaskState.Running);

        Assert.Equal(BackgroundTaskState.Completed, host.Tasks.All[0].State);
        Assert.Equal("survived", host.Tasks.All[0].Result!.TextContent);
    }

    [Fact]
    public async Task Cancelling_the_tasks_own_token_ends_it_as_Cancelled()
    {
        var tool = new FakeTool(supportsBackground: true, async (_, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            return ToolResult.Text("never");
        });
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        await RunAsync(pipeline, "{\"background\":true}");
        var record = host.Tasks.All[0];

        host.Tasks.Cancel(record.Id);
        await WaitUntil(() => record.State != BackgroundTaskState.Running);

        Assert.Equal(BackgroundTaskState.Cancelled, record.State);
    }

    [Fact]
    public async Task At_the_cap_the_launch_itself_is_refused_synchronously()
    {
        var tool = new FakeTool(supportsBackground: true, async (_, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            return ToolResult.Text("never");
        });
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();
        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });

        for (var i = 0; i < BackgroundTaskRegistry.MaxLiveTasks; i++)
            await RunAsync(pipeline, "{\"background\":true}");

        var result = await RunAsync(pipeline, "{\"background\":true}");

        Assert.True(result.IsError);
        Assert.Equal(BackgroundTaskRegistry.MaxLiveTasks, host.Tasks.LiveCount);

        // Unstick every held-open task so the test process does not leave background work running.
        foreach (var t in host.Tasks.All) host.Tasks.Cancel(t.Id);
    }

    [Fact]
    public async Task A_nested_permission_ask_inside_a_background_run_is_auto_denied_not_left_hanging()
    {
        var tool = new FakeTool(supportsBackground: true, async (_, _) =>
        {
            var def = new ToolFunctionDefinition { Name = "nested" };
            var pending = PermissionScope.RequestAsync(def, "{}");
            Assert.NotNull(pending); // a scope IS active (the auto-refuse one)
            var decision = await pending!;
            return ToolResult.Text(decision.ToString());
        });
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        await RunAsync(pipeline, "{\"background\":true}");
        await WaitUntil(() => host.Tasks.All[0].State != BackgroundTaskState.Running);

        Assert.Equal("Deny", host.Tasks.All[0].Result!.TextContent);
    }

    [Fact]
    public async Task A_nested_clarify_ask_inside_a_background_run_gets_no_answer_not_a_hang()
    {
        var tool = new FakeTool(supportsBackground: true, async (_, _) =>
        {
            var answer = await ClarifyScope.AskAsync(
                new ClarifyRequest { Question = "?", Options = Array.Empty<ClarifyOption>() });
            return ToolResult.Text(answer ?? "(null)");
        });
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        await RunAsync(pipeline, "{\"background\":true}");
        await WaitUntil(() => host.Tasks.All[0].State != BackgroundTaskState.Running);

        Assert.Equal("(null)", host.Tasks.All[0].Result!.TextContent);
    }

    [Fact]
    public async Task Background_depth_is_capped_at_one_a_nested_request_runs_in_place()
    {
        // Plan pitfall 9, "фон в фоне": a call already running detached must not be able to detach
        // again — a script's ctx.Run re-entering the same pipeline from inside an already-detached
        // task is exactly the shape that would give unbounded branching without the cap.
        ToolCallDelegate? pipelineRef = null;
        var nestedRegisteredWhileRunning = -1;
        var host = new FakeBackgroundHost();
        var enteredOnce = false;

        var tool = new FakeTool(supportsBackground: true, async (_, ct) =>
        {
            if (!enteredOnce)
            {
                enteredOnce = true;
                // First entry (the outer, detached call) — recurse once, simulating a nested call
                // from inside the already-detached run asking to background too.
                var nested = await pipelineRef!(
                    new ToolCallInvocation(AgentMode.Agent, "fake_tool", "{\"background\":true}"), ct);
                nestedRegisteredWhileRunning = host.Tasks.All.Count; // only the outer task, if capped
                return nested; // the nested call's own result, proving it ran synchronously
            }
            return ToolResult.Text("inner ran");
        });
        pipelineRef = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        await RunAsync(pipelineRef, "{\"background\":true}");
        await WaitUntil(() => host.Tasks.All[0].State != BackgroundTaskState.Running);

        // Exactly one task ever existed — the nested request degraded to synchronous, not a second bg_N.
        Assert.Equal(1, nestedRegisteredWhileRunning);
        Assert.Single(host.Tasks.All);
        Assert.Equal("inner ran", host.Tasks.All[0].Result!.TextContent);
    }

    [Fact]
    public async Task Detached_run_gets_its_own_progress_tree_registered_in_the_hub()
    {
        var tool = new FakeTool(supportsBackground: true, (_, _) =>
        {
            using (ProgressScope.BeginNode("inner")) { }
            return Task.FromResult(ToolResult.Text("ran"));
        });
        var pipeline = BuildPipeline(tool, (call, ct) => tool.ExecuteAsync(call.ArgumentsJson, ct));
        var host = new FakeBackgroundHost();

        using var scope = AgentSessionScope.Begin(new FakeAgentSession { Background = host });
        await RunAsync(pipeline, "{\"background\":true}");
        var record = host.Tasks.All[0];

        Assert.NotNull(record.ProgressTreeId);
        Assert.True(host.Progress.Trees.ContainsKey(record.ProgressTreeId!));
        await WaitUntil(() => record.State != BackgroundTaskState.Running);
    }

    private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 2000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("condition never became true");
            await Task.Delay(5);
        }
    }
}
