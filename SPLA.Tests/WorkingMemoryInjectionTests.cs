using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;

namespace SPLA.Tests;

public class WorkingMemoryInjectionTests
{
    [Fact]
    public void Render_only_includes_context_prefixed_keys()
    {
        var block = WorkingMemoryInjector.Render(new[]
        {
            ("session", "context:plan", "do X"),
            ("session", "scratch", "ignore me"),
            ("project", "context:env", "prod")
        });

        Assert.NotNull(block);
        Assert.Contains("context:plan = do X", block);
        Assert.Contains("[project] context:env = prod", block);
        Assert.DoesNotContain("scratch", block);
    }

    [Fact]
    public void Render_returns_null_when_no_context_keys()
    {
        var block = WorkingMemoryInjector.Render(new[] { ("session", "scratch", "v") });
        Assert.Null(block);
    }

    [Fact]
    public async Task Orchestrator_injects_live_memory_into_each_turn()
    {
        var llm = new FakeLlm(new[] { new ChatMessage { Role = ChatRole.Assistant, Content = "ok" } });
        var orch = new ConversationOrchestrator(llm, new NoTools())
        {
            ToolFilter = (t, _) => t,
            WorkingMemory = () => new List<(string, string, string)> { ("session", "context:plan", "step 1") }
        };
        var convo = new List<ChatMessage>
        {
            new() { Role = ChatRole.System, Content = "SYS" },
            new() { Role = ChatRole.User, Content = "hi" }
        };

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());

        var seen = llm.SeenContexts.Single();
        // Injected as a system message right after the leading system prompt — and not persisted.
        Assert.Contains(seen, m => m.Role == ChatRole.System && (m.Content?.Contains("context:plan = step 1") ?? false));
        Assert.DoesNotContain(convo, m => (m.Content?.Contains("Working memory") ?? false));
    }

    // Minimal fakes (kept local to avoid coupling to ConversationOrchestratorTests' private types).
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

    private sealed class NoTools : IToolHost
    {
        public IEnumerable<ToolDefinition> GetToolDefinitions() => System.Array.Empty<ToolDefinition>();
        public Task<string> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken cancellationToken = default)
            => Task.FromResult("");
    }
}
