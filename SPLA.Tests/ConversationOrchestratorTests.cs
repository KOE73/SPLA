using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

public class ConversationOrchestratorTests
{
    // A scripted LLM: returns each queued response in order. Records the context it was given.
    private sealed class FakeLlm : ILLMService
    {
        private readonly Queue<ChatMessage> _responses;
        public List<List<ChatMessage>> SeenContexts { get; } = new();

        public FakeLlm(IEnumerable<ChatMessage> responses) => _responses = new(responses);

        public Task<ChatMessage> SendMessageStreamFullAsync(
            IEnumerable<ChatMessage> messages, LLMSettings settings, IEnumerable<ToolDefinition>? tools,
            System.Func<string, Task>? onDelta, CancellationToken cancellationToken = default,
            System.Func<string, Task>? onReasoning = null)
        {
            SeenContexts.Add(messages.ToList());
            return Task.FromResult(_responses.Dequeue());
        }

        public Task<ChatMessage> SendMessageAsync(IEnumerable<ChatMessage> messages, LLMSettings settings, IEnumerable<ToolDefinition>? tools = null, CancellationToken cancellationToken = default)
            => Task.FromResult(_responses.Dequeue());

        public async IAsyncEnumerable<string> SendMessageStreamAsync(IEnumerable<ChatMessage> messages, LLMSettings settings, IEnumerable<ToolDefinition>? tools = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
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
        var orch = new ConversationOrchestrator(llm, host) { ToolFilter = (t, _) => t, ToolLoopWindow = 3 };
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
}
