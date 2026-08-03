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
        public List<List<ChatMessage>> SeenContexts { get; } = new();

        public FakeLlm(IEnumerable<ChatMessage> responses) => _responses = new(responses);

        public Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
        {
            SeenContexts.Add(ctx.Messages.ToList());
            return Task.FromResult(new LlmTurnResult { Message = _responses.Dequeue() });
        }
    }

    private sealed class FakeToolHost : IToolHost
    {
        public List<(string name, string args)> Executed { get; } = new();
        public IEnumerable<ToolDefinition> GetToolDefinitions() => System.Array.Empty<ToolDefinition>();

        public Task<string> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken cancellationToken = default)
        {
            Executed.Add((name, argumentsJson));
            return Task.FromResult($"result of {name}");
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
        // Stopped well before exhausting the 10 scripted responses.
        Assert.True(host.Executed.Count <= 3);
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
}
