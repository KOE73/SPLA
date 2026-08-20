using SPLA.Domain.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;

namespace SPLA.Tests;

public class ConversationOrchestratorTests
{
    // A scripted LLM: returns each queued response in order. Records the context it was given.
    private sealed class FakeLlm : ILlmGateway
    {
        private readonly Queue<ChatMessage> _responses;
        private readonly Queue<LlmTurnStatus> _statuses;
        public List<List<ChatMessage>> SeenContexts { get; } = new();

        /// <summary>Attempts to report via ctx.OnAttempt before a call returns — one queue entry
        /// consumed per call. A call with nothing queued reports none, so existing tests that never
        /// touch this queue see no behavior change.</summary>
        public Queue<List<GenerationAttempt>> AttemptsPerCall { get; } = new();

        public FakeLlm(IEnumerable<ChatMessage> responses, IEnumerable<LlmTurnStatus>? statuses = null)
        {
            _responses = new(responses);
            _statuses = new(statuses ?? Array.Empty<LlmTurnStatus>());
        }

        public Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
        {
            SeenContexts.Add(ctx.Messages.ToList());
            if (AttemptsPerCall.Count > 0)
                foreach (var attempt in AttemptsPerCall.Dequeue())
                    ctx.OnAttempt?.Invoke(attempt);
            var status = _statuses.Count > 0 ? _statuses.Dequeue() : LlmTurnStatus.Ok;
            return Task.FromResult(new LlmTurnResult { Message = _responses.Dequeue(), Status = status });
        }
    }

    private sealed class FakeToolHost : IToolHost
    {
        public List<(string name, string args)> Executed { get; } = new();
        public IEnumerable<ToolDefinition> GetToolDefinitions() => System.Array.Empty<ToolDefinition>();

        public Task<ToolResult> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken cancellationToken = default, ToolCallContext? context = null)
        {
            Executed.Add((name, argumentsJson));
            return Task.FromResult(ToolResult.Text($"result of {name}"));
        }
    }

    private static ToolCall Call(string id, string name) =>
        new() { Id = id, Function = new FunctionCall { Name = name, Arguments = "{}" } };

    [Fact]
    public async Task Plain_answer_ends_the_loop_in_one_call()
    {
        var llm = new FakeLlm(new[] { new ChatMessage { Role = ChatRole.Assistant, Content = "done" } });
        var host = new FakeToolHost();
        var orch = new ConversationOrchestrator(llm, host) { ToolFilter = (t, _) => t };
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "hi" });

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());

        Assert.Single(llm.SeenContexts);
        Assert.Empty(host.Executed);
        Assert.Equal("done", convo.Messages.Last().Content);
    }

    [Fact]
    public async Task Tool_call_then_answer_runs_tool_and_loops_back()
    {
        var llm = new FakeLlm(new[]
        {
            new ChatMessage { Role = ChatRole.Assistant, Content = "", ToolCalls = new() { Call("1", "system_read_file") } },
            new ChatMessage { Role = ChatRole.Assistant, Content = "here it is" }
        });
        var host = new FakeToolHost();
        var orch = new ConversationOrchestrator(llm, host) { ToolFilter = (t, _) => t };
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "read file" });

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());

        Assert.Single(host.Executed);
        Assert.Equal("system_read_file", host.Executed[0].name);
        // tool result is appended and visible to the model on the second call
        Assert.Contains(convo.Messages, m => m.Role == ChatRole.Tool && m.Content == "result of system_read_file");
        Assert.Equal("here it is", convo.Messages.Last().Content);
    }

    [Fact]
    public async Task Repeated_identical_tool_calls_trip_the_loop_guard()
    {
        // Model keeps asking for the same tool forever; guard must stop it and emit a notice.
        var responses = Enumerable.Range(0, 10)
            .Select(_ => new ChatMessage { Role = ChatRole.Assistant, Content = "", ToolCalls = new() { Call("1", "system_read_file") } });
        var llm = new FakeLlm(responses);
        var host = new FakeToolHost();
        var orch = new ConversationOrchestrator(llm, host) { ToolFilter = (t, _) => t, ToolLoopWindow = 3, EnableLoopGuard = true };
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "go" });

        string? notice = null;
        var cb = new AgentCallbacks { OnNotice = n => { notice = n; return Task.CompletedTask; } };

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, cb);

        Assert.NotNull(notice);
        Assert.Contains("repeating", notice!);
        // Stopped well before exhausting the 10 scripted responses. The guard is two-stage: the first
        // streak of 3 only injects the "are you stuck?" challenge and resets the window, so the hard
        // stop lands one full window later — 6 executions, never the whole script.
        Assert.True(host.Executed.Count <= 6, $"ran {host.Executed.Count} tools");
    }

    // ── Abandoned generations (repetition guard) ─────────────────────────────

    [Fact]
    public async Task Attempts_reported_during_a_call_end_up_on_the_message_it_produced()
    {
        var llm = new FakeLlm(new[] { new ChatMessage { Role = ChatRole.Assistant, Content = "done" } });
        llm.AttemptsPerCall.Enqueue(new List<GenerationAttempt>
        {
            new() { Index = 1, Outcome = AttemptOutcome.Repetition, Content = "looped once",
                Chars = 11, Duration = TimeSpan.FromMilliseconds(5) }
        });
        var host = new FakeToolHost();
        var orch = new ConversationOrchestrator(llm, host) { ToolFilter = (t, _) => t };
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "hi" });

        var forwarded = new List<GenerationAttempt>();
        var cb = new AgentCallbacks { OnAttempt = a => forwarded.Add(a) };

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, cb);

        var msg = convo.Messages.Last();
        Assert.Equal("done", msg.Content);
        Assert.NotNull(msg.Attempts);
        var attempt = Assert.Single(msg.Attempts!);
        Assert.Equal("looped once", attempt.Content);
        // Collecting onto the message must not come at the cost of the caller's own sink.
        Assert.Single(forwarded);
    }

    [Fact]
    public async Task Degenerate_call_leaves_a_record_that_is_not_resent_next_turn()
    {
        // Call 1 loops on every attempt. The queued message's Content ("LOOPED-TEXT-MARKER") stands
        // in for what RepetitionGuardMiddleware.Degenerate() actually returns — the last LOOPING
        // generation's text — and the orchestrator must not let it anywhere near the conversation;
        // only Status and the attempts reported through OnAttempt matter here.
        var llm = new FakeLlm(
            new[]
            {
                new ChatMessage { Role = ChatRole.Assistant, Content = "LOOPED-TEXT-MARKER" },
                new ChatMessage { Role = ChatRole.Assistant, Content = "all better now" }
            },
            new[] { LlmTurnStatus.Degenerate, LlmTurnStatus.Ok });
        llm.AttemptsPerCall.Enqueue(new List<GenerationAttempt>
        {
            new() { Index = 1, Outcome = AttemptOutcome.Repetition, Content = "LOOPED-TEXT-MARKER",
                Chars = 18, Duration = TimeSpan.FromMilliseconds(5) },
            new() { Index = 2, Outcome = AttemptOutcome.Repetition, Content = "LOOPED-TEXT-MARKER again",
                Chars = 24, Duration = TimeSpan.FromMilliseconds(5) }
        });
        var host = new FakeToolHost();
        var orch = new ConversationOrchestrator(llm, host) { ToolFilter = (t, _) => t };
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "hi" });

        string? notice = null;
        var cb = new AgentCallbacks { OnNotice = n => { notice = n; return Task.CompletedTask; } };

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, cb);

        // The record survives in the conversation: empty content, the attempts riding on it.
        var placeholder = convo.Messages.Last();
        Assert.Equal(ChatRole.Assistant, placeholder.Role);
        Assert.True(string.IsNullOrEmpty(placeholder.Content));
        Assert.Equal(2, placeholder.Attempts?.Count);
        Assert.NotNull(notice);

        // The next turn must never see the abandoned text: ContextAssembler drops an empty-content,
        // no-tool-calls message regardless of what it carries, so the placeholder never reaches the
        // gateway — assert against what the fake gateway was actually handed, not just the guard's intent.
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "try again" });
        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());

        Assert.Equal(2, llm.SeenContexts.Count);
        Assert.DoesNotContain(llm.SeenContexts[1], m => (m.Content ?? "").Contains("LOOPED-TEXT-MARKER"));
        Assert.Equal("all better now", convo.Messages.Last().Content);
    }

    // ── System prompt provider ───────────────────────────────────────────────

    /// <summary>
    /// Per-iteration, not per-turn. The case that demands it: the model calls skill_activate, and
    /// the activated procedure has to be in the prompt for the LLM call that immediately follows —
    /// not for the user's next message, by which point the model has already acted without it.
    /// </summary>
    [Fact]
    public async Task System_prompt_is_re_rendered_on_every_iteration()
    {
        var llm = new FakeLlm(new[]
        {
            new ChatMessage { Role = ChatRole.Assistant, Content = "", ToolCalls = new() { Call("1", "system_read_file") } },
            new ChatMessage { Role = ChatRole.Assistant, Content = "done" }
        });

        var renders = 0;
        var orch = new ConversationOrchestrator(llm, new FakeToolHost())
        {
            ToolFilter = (t, _) => t,
            Context = () => ComposedContext.FromSystemPrompt($"PROMPT #{++renders}")
        };

        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.System, Content = "seeded placeholder" });
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "go" });

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());

        Assert.Equal(2, llm.SeenContexts.Count);
        Assert.Equal("PROMPT #1", llm.SeenContexts[0].First(m => m.Role == ChatRole.System).Content);
        Assert.Equal("PROMPT #2", llm.SeenContexts[1].First(m => m.Role == ChatRole.System).Content);
    }

    /// <summary>The assembled list holds the very objects the conversation stores, so the refresh
    /// has to replace the entry rather than assign to it — otherwise a per-iteration rendering gets
    /// written into persisted chat history.</summary>
    [Fact]
    public async Task System_prompt_refresh_does_not_touch_stored_history()
    {
        var llm = new FakeLlm(new[] { new ChatMessage { Role = ChatRole.Assistant, Content = "done" } });
        var orch = new ConversationOrchestrator(llm, new FakeToolHost())
        {
            ToolFilter = (t, _) => t,
            Context = () => ComposedContext.FromSystemPrompt("freshly rendered")
        };

        var convo = new Conversation();
        var seeded = new ChatMessage { Role = ChatRole.System, Content = "seeded placeholder" };
        convo.Add(seeded);
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "go" });

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());

        Assert.Equal("freshly rendered", llm.SeenContexts[0].First(m => m.Role == ChatRole.System).Content);
        Assert.Equal("seeded placeholder", seeded.Content);
    }

    /// <summary>No provider — the seeded system message is sent as-is. Keeps the spawned-agent path,
    /// which builds its prompt once up front, working unchanged.</summary>
    [Fact]
    public async Task Without_a_provider_the_seeded_system_message_is_sent_unchanged()
    {
        var llm = new FakeLlm(new[] { new ChatMessage { Role = ChatRole.Assistant, Content = "done" } });
        var orch = new ConversationOrchestrator(llm, new FakeToolHost()) { ToolFilter = (t, _) => t };

        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.System, Content = "built once" });
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "go" });

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());

        Assert.Equal("built once", llm.SeenContexts[0].First(m => m.Role == ChatRole.System).Content);
    }

    // ── Nesting a spawned run into the caller's progress ─────────────────────────────────────────

    /// <summary>Stands in for the pipeline's ProgressNodeStage, which is what actually opens a node per
    /// call — the plain fake host above cannot show where nodes land because it never makes any.</summary>
    private sealed class ReportingToolHost : IToolHost
    {
        public IEnumerable<ToolDefinition> GetToolDefinitions() => System.Array.Empty<ToolDefinition>();

        public Task<ToolResult> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson,
            CancellationToken cancellationToken = default, ToolCallContext? context = null)
        {
            using var node = ProgressScope.BeginNode(name);
            return Task.FromResult(ToolResult.Text($"result of {name}"));
        }
    }

    private static ConversationOrchestrator Spawned(FakeLlm llm, bool nest) =>
        new(llm, new ReportingToolHost()) { ToolFilter = (t, _) => t, NestInAmbientProgress = nest };

    private static FakeLlm OneToolThenAnswer() => new(new[]
    {
        new ChatMessage { Role = ChatRole.Assistant, ToolCalls = new List<ToolCall> { Call("1", "fs_read") } },
        new ChatMessage { Role = ChatRole.Assistant, Content = "done" }
    });

    private static Conversation Seeded()
    {
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "go" });
        return convo;
    }

    /// <summary>
    /// The whole of sub-agent visibility, and the reason it needed no event bus: a spawn runs inside the
    /// caller's <c>agent_spawn</c> node, so a loop that does not open a tree of its own leaves the
    /// ambient one in place and its tool calls become children of that node. Every surface already
    /// rendering the tree then shows the sub-agent's work with no further wiring.
    /// </summary>
    [Fact]
    public async Task A_nested_run_puts_its_tool_calls_under_the_callers_node()
    {
        var tree = new ProgressTree();

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("agent_spawn"))
        {
            await Spawned(OneToolThenAnswer(), nest: true)
                .RunAsync(Seeded(), new LLMSettings(), AgentMode.Agent, new AgentCallbacks());
        }

        var root = Assert.Single(tree.Nodes, n => n.ParentId == null);
        Assert.Equal("agent_spawn", root.Label);

        var child = Assert.Single(tree.Nodes, n => n.ParentId == root.Id);
        Assert.Equal("fs_read", child.Label);
        Assert.Equal(ProgressState.Completed, child.State);
    }

    /// <summary>The default, and what every chat wants: a turn is the unit, and its tree is its own.</summary>
    [Fact]
    public async Task Without_nesting_the_run_reports_into_a_tree_of_its_own()
    {
        var tree = new ProgressTree();

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("agent_spawn"))
        {
            await Spawned(OneToolThenAnswer(), nest: false)
                .RunAsync(Seeded(), new LLMSettings(), AgentMode.Agent, new AgentCallbacks());
        }

        // Only the caller's own node — everything the run did went somewhere else entirely, which is
        // exactly the black box the flag exists to open.
        var only = Assert.Single(tree.Nodes);
        Assert.Equal("agent_spawn", only.Label);
    }

    /// <summary>A tree we did not open is not ours to hand out, and its roots are not our tool calls —
    /// forwarding either would report the caller's work as the nested run's.</summary>
    [Fact]
    public async Task A_nested_run_claims_neither_the_tree_nor_its_roots()
    {
        var tree = new ProgressTree();
        var handedOut = 0;
        var forwarded = 0;

        var callbacks = new AgentCallbacks
        {
            OnProgressTree = _ => handedOut++,
            OnToolProgress = (_, _) => forwarded++
        };

        using (ProgressScope.BeginTree(tree))
        using (ProgressScope.BeginNode("agent_spawn"))
        {
            ProgressScope.Report(1, 1, "caller is busy");
            await Spawned(OneToolThenAnswer(), nest: true)
                .RunAsync(Seeded(), new LLMSettings(), AgentMode.Agent, callbacks);
        }

        Assert.Equal(0, handedOut);
        Assert.Equal(0, forwarded);
    }
}
