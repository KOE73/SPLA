using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using SPLA.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 7 (docs/plans/PLAN_20260902_agent_roles-and-correspondence.md, commits 549e8c2/9aa9347):
/// two real bugs the wave's own commit message calls out.
/// <para>
/// (1) <see cref="ChatMessage.PeerFrom"/> was written to disk but never broadcast — a reply only
/// read as speech ("← from &lt;role&gt;") after a reload, and looked like an ordinary human message
/// while the turn was still live. <see cref="ChatRuntime"/> fixed this by folding
/// <c>InboxItemKind.Peer</c> into the same "pending echo" set <c>InboxItemKind.Human</c> already
/// used to fire <c>onUserMessage</c> the moment the message is added to the conversation — see
/// <see cref="ChatRuntime.SendAsync"/>'s <c>onUserMessage</c> parameter, which is exactly the hook
/// <c>ChatTurnDriver.RunTurnAsync</c> wires to the websocket broadcast in the real service.
/// </para>
/// <para>
/// (2) Per-message token counts were set by the LLM client (<see cref="ChatMessage.PromptTokens"/>/
/// <see cref="ChatMessage.CompletionTokens"/>) but thrown away on save. Fixed alongside PeerFrom in
/// the same two save-mapping sites in <c>ChatRuntime.cs</c>.
/// </para>
/// These drive a real <see cref="ChatRuntime.SendAsync"/> turn end to end — the only way to prove the
/// live callback actually fires for a Peer-kind message, not just that the field round-trips through
/// YAML — using a fake <see cref="ILlmClient"/> registered as a real provider so no network call is
/// ever made and no <c>Provider.Resolve</c> catalog fallback is hit either (the fake also implements
/// <see cref="IReasoningCatalog"/> for that reason).
/// </para>
/// </summary>
public sealed class PeerReplyLiveAndTokenTests
{
    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-peerlive-{Guid.NewGuid():N}")).FullName;

    /// <summary>A scripted <see cref="ILlmClient"/> that never touches the network — registered as a
    /// real provider (id "fake") so <c>ChatRuntime.SendAsync</c> can run a whole turn against it.
    /// Also an <see cref="IReasoningCatalog"/> reporting Unknown directly, so
    /// <c>AgentRuntime.GetReasoningAsync</c> never falls through to its network-backed
    /// model-management branch.</summary>
    private sealed class FakeLlmClient : ILlmClient, IReasoningCatalog
    {
        private readonly Queue<ChatMessage> _responses;
        public List<List<ChatMessage>> SeenContexts { get; } = new();

        public FakeLlmClient(IEnumerable<ChatMessage> responses) => _responses = new(responses);

        public Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
        {
            SeenContexts.Add(ctx.Messages.ToList());
            return Task.FromResult(new LlmTurnResult { Message = _responses.Dequeue(), Status = LlmTurnStatus.Ok });
        }

        public Task<ReasoningCapability> GetReasoningAsync(
            string endpoint, string modelId, string? apiKey, CancellationToken ct = default)
            => Task.FromResult(ReasoningCapability.Unknown);
    }

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject(FakeLlmClient client)
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: PeerLiveTest
            workspace: .
            agent:
              mode: Edit
            connections:
              - id: fake
                name: Fake
                provider: fake
                endpoint: http://127.0.0.1:1/v1
                api_key: fake-key
                models:
                  - id: fake-model
                    model: fake-model
                    default: true
            """);
        var settings = ConfigLoader.LoadAndResolve(manifest);
        var runtime = new AgentRuntime(settings, NullLoggerFactory.Instance);
        // Registration is closed before the first turn (LlmProviderRegistry's own contract) — this
        // happens well before any SendAsync call below.
        runtime.Providers.Register(new LlmProviderDescriptor { Id = "fake", DisplayName = "Fake", Client = client });
        var chats = new ChatRegistry(runtime);
        return (runtime, chats, root);
    }

    private static Task<PermissionDecision> AllowAll(ToolFunctionDefinition _, string __)
        => Task.FromResult(PermissionDecision.AllowOnce);

    private static Task<string?> NoClarify(ClarifyRequest _) => Task.FromResult<string?>(null);

    [Fact]
    public async Task An_incoming_reply_reaches_the_live_onUserMessage_hook_with_its_PeerFrom_intact()
    {
        var client = new FakeLlmClient(new[]
        {
            new ChatMessage { Role = ChatRole.Assistant, Content = "ack" }
        });
        var (runtime, chats, root) = BuildProject(client);
        try
        {
            // The sender's own role ("reviewer") is what ChatRuntime.SendReply stamps onto the
            // recipient's message as PeerFrom — see the ownRoleForPeer local in SendAsync's
            // neighbourhood in ChatRuntime.cs.
            var reviewer = chats.CreateNew("Reviewer", role: "reviewer");
            var architect = chats.CreateNew("Architect");

            reviewer.OpenCorrespondence("architect", "", architect.ChatId, CorrespondenceInitiator.Self);
            var sent = reviewer.SendReply("architect", 1, "what do you think of this API?");
            Assert.True(sent.Delivered);

            // Nothing has drained yet — the message sits in architect's inbox exactly like a woken
            // pump turn would find it (ChatPump's runTurn calls SendAsync(text: null, ...)).
            var echoed = new List<ChatMessage>();
            await architect.SendAsync(
                text: null,
                callbacks: new SPLA.Agent.AgentCallbacks(),
                permissionHandler: AllowAll,
                clarifyHandler: NoClarify,
                cancellationToken: CancellationToken.None,
                images: null,
                onUserMessage: m => echoed.Add(m));

            // The live hook fired for the Peer-kind message, not only the reload path — this is
            // exactly what was broken before wave 7's fix (only Human-kind messages fired it).
            var liveEcho = Assert.Single(echoed);
            Assert.Equal("reviewer", liveEcho.PeerFrom);
            Assert.Equal("what do you think of this API?", liveEcho.Content);

            // And it also survives the chat's own save/reload — the other half of the same bug
            // (PeerFrom was written to disk correctly; only the live broadcast was missing it).
            var reloaded = runtime.ChatManager.LoadChat(architect.ChatId)!;
            var persisted = reloaded.Messages.Single(m => m.Content == "what do you think of this API?");
            Assert.Equal("reviewer", persisted.PeerFrom);

            // A message with nothing to do with a correspondence never carries PeerFrom.
            var reply = reloaded.Messages.Single(m => m.Content == "ack");
            Assert.Null(reply.PeerFrom);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Per_message_token_counts_persist_and_sum_correctly_across_a_chat()
    {
        var client = new FakeLlmClient(new[]
        {
            new ChatMessage { Role = ChatRole.Assistant, Content = "first answer", PromptTokens = 100, CompletionTokens = 20 },
            new ChatMessage { Role = ChatRole.Assistant, Content = "second answer", PromptTokens = 150, CompletionTokens = 30 }
        });
        var (runtime, chats, root) = BuildProject(client);
        try
        {
            var chat = chats.CreateNew("Chat");

            await chat.SendAsync("first question", new SPLA.Agent.AgentCallbacks(),
                AllowAll, NoClarify, CancellationToken.None);
            await chat.SendAsync("second question", new SPLA.Agent.AgentCallbacks(),
                AllowAll, NoClarify, CancellationToken.None);

            // Persistence: both assistant messages kept their own token counts through save/load,
            // not just in the live in-memory conversation.
            var reloaded = runtime.ChatManager.LoadChat(chat.ChatId)!;
            var first = reloaded.Messages.Single(m => m.Content == "first answer");
            var second = reloaded.Messages.Single(m => m.Content == "second answer");
            Assert.Equal(100, first.PromptTokens);
            Assert.Equal(20, first.CompletionTokens);
            Assert.Equal(150, second.PromptTokens);
            Assert.Equal(30, second.CompletionTokens);

            // Aggregation: RuntimeProjections.List sums per-message counts into the chat summary the
            // sessions panel (and the tree) reads.
            var summary = chats.List().Single(c => c.Id == chat.ChatId);
            Assert.Equal(250, summary.PromptTokens);
            Assert.Equal(50, summary.CompletionTokens);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task A_chat_with_no_usage_reported_shows_no_token_totals()
    {
        var client = new FakeLlmClient(new[]
        {
            new ChatMessage { Role = ChatRole.Assistant, Content = "no usage here" }
        });
        var (runtime, chats, root) = BuildProject(client);
        try
        {
            var chat = chats.CreateNew("Chat");
            await chat.SendAsync("hi", new SPLA.Agent.AgentCallbacks(), AllowAll, NoClarify, CancellationToken.None);

            var summary = chats.List().Single(c => c.Id == chat.ChatId);
            Assert.Null(summary.PromptTokens);
            Assert.Null(summary.CompletionTokens);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
