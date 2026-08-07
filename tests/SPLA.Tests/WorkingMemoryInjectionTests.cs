using SPLA.Domain.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPLA.Agent;
using SPLA.Agent.Composition;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Composition;

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

    /// <summary>The snapshot reaches the model through the ordinary contribution mechanism, but with
    /// its own placement: it is a message of its own, after the system prompt, and never persisted.</summary>
    [Fact]
    public async Task Orchestrator_injects_live_memory_into_each_turn()
    {
        var sessionKv = new SPLA.Domain.Agent.KeyValueStore("session");
        sessionKv.Set("context:plan", "step 1");

        var composer = new AgentContextComposer([new WorkingMemoryContributor(projectKv: null, sessionKv)]);
        var settings = new ResolvedSettings
        {
            Mode = AgentMode.Agent,
            Instructions = [],
            CustomPrompt = null,
            Skills = new Dictionary<string, SplaSkillSection>()
        };

        var llm = new FakeLlm(new[] { new ChatMessage { Role = ChatRole.Assistant, Content = "ok" } });
        var orch = new ConversationOrchestrator(llm, new NoTools())
        {
            ToolFilter = (t, _) => t,
            Context = () => composer.Compose(settings, Directory.GetCurrentDirectory())
        };
        var convo = new Conversation();
        convo.Add(new ChatMessage { Role = ChatRole.System, Content = "SYS" });
        convo.Add(new ChatMessage { Role = ChatRole.User, Content = "hi" });

        await orch.RunAsync(convo, new LLMSettings(), AgentMode.Agent, new AgentCallbacks());

        var seen = llm.SeenContexts.Single();
        // Injected as a system message right after the leading system prompt — and not persisted.
        Assert.Contains(seen, m => m.Role == ChatRole.System && (m.Content?.Contains("context:plan = step 1") ?? false));
        Assert.DoesNotContain(convo.Messages, m => (m.Content?.Contains("Working memory") ?? false));

        // This contributor contributes no prompt text at all, so the seeded system message survives.
        Assert.Equal("SYS", seen.First(m => m.Role == ChatRole.System).Content);
    }

    /// <summary>The session store is normally resolved from the running chat's ambient scope — that is
    /// what lets one process-wide contributor serve chats that run in parallel.</summary>
    [Fact]
    public void Contributor_resolves_the_session_store_from_the_ambient_scope()
    {
        var sessionKv = new SPLA.Domain.Agent.KeyValueStore("session");
        sessionKv.Set("context:where", "chat A");

        var settings = new ResolvedSettings
        {
            Mode = AgentMode.Agent,
            Instructions = [],
            CustomPrompt = null,
            Skills = new Dictionary<string, SplaSkillSection>()
        };
        var composer = new AgentContextComposer([new WorkingMemoryContributor(projectKv: null)]);

        Assert.Empty(composer.Compose(settings, Directory.GetCurrentDirectory()).TurnMessages);

        using var scope = SPLA.Domain.Agent.AgentSessionScope.Begin(new SPLA.Domain.Agent.AgentSession(
            sessionKv, new SPLA.Domain.Agent.MarkManager(), new SPLA.Domain.Agent.SkillSession()));

        var composed = composer.Compose(settings, Directory.GetCurrentDirectory());
        var item = Assert.Single(composed.TurnMessages);
        Assert.Contains("context:where = chat A", item.Body);
        Assert.Equal(ContextPlacement.TurnMessage, item.Placement);
        Assert.Equal("working-memory", item.Contributor);
        Assert.Empty(composed.SystemPrompt);
    }

    // Minimal fakes (kept local to avoid coupling to ConversationOrchestratorTests' private types).
    private sealed class FakeLlm : SPLA.Domain.Llm.ILlmGateway
    {
        private readonly Queue<ChatMessage> _responses;
        public List<List<ChatMessage>> SeenContexts { get; } = new();
        public FakeLlm(IEnumerable<ChatMessage> responses) => _responses = new(responses);

        public Task<SPLA.Domain.Llm.LlmTurnResult> InvokeAsync(
            SPLA.Domain.Llm.LlmTurnContext ctx, CancellationToken ct = default)
        {
            SeenContexts.Add(ctx.Messages.ToList());
            return Task.FromResult(new SPLA.Domain.Llm.LlmTurnResult { Message = _responses.Dequeue() });
        }
    }

    private sealed class NoTools : IToolHost
    {
        public IEnumerable<ToolDefinition> GetToolDefinitions() => System.Array.Empty<ToolDefinition>();
        public Task<ToolResult> ExecuteToolAsync(AgentMode mode, string name, string argumentsJson, CancellationToken cancellationToken = default, ToolCallContext? context = null)
            => Task.FromResult(ToolResult.Empty());
    }
}
