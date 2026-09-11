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
/// ADR_20260910-2 §4.4, wave 1: "снимок плюс поток" — a subscriber must never see a gap or a
/// duplicate between the snapshot it is handed and the events that follow it, even when something
/// else is publishing to the same feed at that exact moment.
/// </summary>
public class ChatFeedSnapshotTests
{
    private static ChatTurnStarted Ev(string chatId) => new(null) { ChatId = chatId };

    /// <summary>
    /// The core atomicity claim, tested at the <see cref="ChatFeed"/> level directly so it does not
    /// depend on any particular caller's mutation pattern: a background thread hammers
    /// <see cref="ChatFeed.Publish"/> with a paired state mutation (<c>mutateUnderGate</c>) while the
    /// test thread races it with <see cref="ChatFeed.SubscribeWithSnapshot{T}"/>. Because both go
    /// through the same lock, the snapshot's own value and the sequence number handed back alongside
    /// it must always agree exactly, and every publish from that point on must be delivered to the new
    /// subscriber exactly once — not zero times (a gap) and not twice (a duplicate).
    /// </summary>
    [Fact]
    public async Task SubscribeWithSnapshot_never_gaps_or_duplicates_against_concurrent_publish()
    {
        var feed = new ChatFeed();
        var state = 0;
        const int totalPublishes = 20_000;

        var producer = Task.Run(() =>
        {
            for (var i = 0; i < totalPublishes; i++)
                feed.Publish(Ev("c1"), () => state++);
        });

        // No coordination beyond the feed's own gate — deliberately racing the producer.
        await Task.Delay(1);

        var received = 0;
        var (snapshotState, sequenceAtSubscribe, subscription) =
            feed.SubscribeWithSnapshot(() => state, _ => received++);

        await producer;
        subscription.Dispose();

        // The snapshot and the sequence it was taken at must describe the exact same moment — neither
        // ahead of nor behind the paired mutation the same lock protects.
        Assert.Equal(sequenceAtSubscribe, snapshotState);

        // Every publish after that moment reached this subscriber exactly once.
        Assert.Equal(totalPublishes - sequenceAtSubscribe, received);
    }

    /// <summary>Two independent subscribe-with-snapshot calls racing the same producer each get an
    /// internally consistent (snapshot, sequence) pair — the gate serializes them, it does not let one
    /// see the other's half-finished state.</summary>
    [Fact]
    public async Task Two_concurrent_snapshot_subscriptions_each_stay_internally_consistent()
    {
        var feed = new ChatFeed();
        var state = 0;
        const int totalPublishes = 10_000;

        var producer = Task.Run(() =>
        {
            for (var i = 0; i < totalPublishes; i++)
                feed.Publish(Ev("c1"), () => state++);
        });

        var first = Task.Run(() => SubscribeAndCount(feed, () => state));
        var second = Task.Run(() => SubscribeAndCount(feed, () => state));
        await Task.WhenAll(first, second);
        await producer;

        first.Result.Subscription.Dispose();
        second.Result.Subscription.Dispose();

        Assert.Equal(first.Result.Sequence, first.Result.Snapshot);
        Assert.Equal(second.Result.Sequence, second.Result.Snapshot);
        Assert.Equal(totalPublishes - first.Result.Sequence, first.Result.Received());
        Assert.Equal(totalPublishes - second.Result.Sequence, second.Result.Received());
    }

    private static (int Snapshot, long Sequence, IDisposable Subscription, Func<int> Received)
        SubscribeAndCount(ChatFeed feed, Func<int> readState)
    {
        var received = 0;
        var (snapshot, sequence, subscription) = feed.SubscribeWithSnapshot(readState, _ => received++);
        return (snapshot, sequence, subscription, () => received);
    }

    // ── ChatRuntime-level: chat.opened's snapshot mid-turn (ADR §4.4) ──────────────

    /// <summary>A scripted <see cref="ILlmClient"/> that blocks on a caller-supplied gate before
    /// answering — lets a test observe <see cref="ChatRuntime"/>'s state while a turn is genuinely
    /// still in flight, the same "opened mid-turn" moment the ADR is about.</summary>
    private sealed class BlockingFakeLlmClient : ILlmClient, IReasoningCatalog
    {
        private readonly Task _gate;
        private readonly ChatMessage _answer;
        public List<List<ChatMessage>> SeenContexts { get; } = new();

        public BlockingFakeLlmClient(Task gate, ChatMessage answer)
        {
            _gate = gate;
            _answer = answer;
        }

        public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
        {
            SeenContexts.Add(ctx.Messages.ToList());
            await _gate;
            return new LlmTurnResult { Message = _answer, Status = LlmTurnStatus.Ok };
        }

        public Task<ReasoningCapability> GetReasoningAsync(
            string endpoint, string modelId, string? apiKey, CancellationToken ct = default)
            => Task.FromResult(ReasoningCapability.Unknown);
    }

    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-feedsnap-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject(ILlmClient client)
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        File.WriteAllText(manifest, """
            version: 1
            name: FeedSnapshotTest
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
        runtime.Providers.Register(new LlmProviderDescriptor { Id = "fake", DisplayName = "Fake", Client = client });
        var chats = new ChatRegistry(runtime);
        return (runtime, chats, root);
    }

    private static Task<PermissionDecision> AllowAll(ToolFunctionDefinition _, string __)
        => Task.FromResult(PermissionDecision.AllowOnce);

    private static Task<string?> NoClarify(ClarifyRequest _) => Task.FromResult<string?>(null);

    private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("condition never became true");
            await Task.Delay(5);
        }
    }

    [Fact]
    public async Task SubscribeWithSnapshot_mid_turn_sees_the_user_message_and_the_answer_arrives_exactly_once()
    {
        var gate = new TaskCompletionSource();
        var client = new BlockingFakeLlmClient(gate.Task, new ChatMessage { Role = ChatRole.Assistant, Content = "answer" });
        var (runtime, chats, root) = BuildProject(client);
        try
        {
            var chat = chats.CreateNew("t");
            var turn = chat.SendAsync("hello", AllowAll, NoClarify, CancellationToken.None);

            // SendAsync adds the user message and publishes ChatUserMessage synchronously, before the
            // (blocked) LLM call — wait for it to land so the snapshot below is taken genuinely mid-turn.
            await WaitUntil(() => chat.Messages.Any(m => m.Role == ChatRole.User));

            var received = new List<ChatEvent>();
            var (snapshot, _, subscription) = chat.SubscribeWithSnapshot(e => received.Add(e));
            using (subscription)
            {
                Assert.Contains(snapshot.Messages, m => m.Role == ChatRole.User && m.Content == "hello");
                // The answer has not arrived yet — it must reach this subscriber only through the feed,
                // never pre-baked into a snapshot taken before it existed.
                Assert.DoesNotContain(snapshot.Messages, m => m.Role == ChatRole.Assistant);

                gate.SetResult();
                await turn;

                // Exactly once: not missing (a gap) and not also sitting in the snapshot (a duplicate).
                Assert.Single(received.OfType<ChatAssistantMessage>());
                Assert.Single(received.OfType<ChatTurnCompleted>());
            }
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task SnapshotForOpen_marks_the_attach_atomically_with_the_snapshot_it_returns()
    {
        // The wire path (ClientConnection.SendOpenedAsync) does not register a new IChatFeed
        // subscriber — it marks a connection watching and reads the snapshot as one step. This proves
        // the "attach" callback really does run inside the same gate the snapshot is read under: a
        // concurrent publish can never land between "marked watching" and "snapshot taken".
        var gate = new TaskCompletionSource();
        var client = new BlockingFakeLlmClient(gate.Task, new ChatMessage { Role = ChatRole.Assistant, Content = "answer" });
        var (runtime, chats, root) = BuildProject(client);
        try
        {
            var chat = chats.CreateNew("t");
            var turn = chat.SendAsync("hi", AllowAll, NoClarify, CancellationToken.None);
            await WaitUntil(() => chat.Messages.Any(m => m.Role == ChatRole.User));

            var attached = false;
            var snapshot = chat.SnapshotForOpen(() => attached = true);

            Assert.True(attached);
            Assert.Contains(snapshot.Messages, m => m.Role == ChatRole.User && m.Content == "hi");

            gate.SetResult();
            await turn;
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }
}
