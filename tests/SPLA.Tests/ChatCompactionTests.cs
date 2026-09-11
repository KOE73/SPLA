using Microsoft.Extensions.Logging.Abstractions;
using SPLA.Domain.Context;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SPLA.Tests;

/// <summary>
/// Wave 4 of <c>docs/plans/PLAN_20260911_agent_roles-agents-md-compact.md</c> —
/// <c>docs/adr/ADR_20260911-3_agent_compaction.md</c>: compaction hides history behind
/// <see cref="ContextRetention.Never"/> rather than erasing it, keeps a tail whose boundary never
/// splits a tool-call/result pair, survives a save/load round trip, and a rollback across the
/// compaction boundary restores exactly the state before it (§2.5).
/// </summary>
public class ChatCompactionTests
{
    /// <summary>Answers every call — turn or compaction — with a fixed reply. Every reply is a plain
    /// assistant message, no tool calls, which is exactly what a summarizing call looks like and keeps
    /// ordinary turns simple too.</summary>
    private sealed class FixedFakeLlmClient : ILlmClient, IReasoningCatalog
    {
        private readonly string _reply;
        public List<List<ChatMessage>> SeenContexts { get; } = new();

        public FixedFakeLlmClient(string reply) { _reply = reply; }

        public Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
        {
            SeenContexts.Add(ctx.Messages.ToList());
            return Task.FromResult(new LlmTurnResult
            {
                Message = new ChatMessage { Role = ChatRole.Assistant, Content = _reply },
                Status = LlmTurnStatus.Ok
            });
        }

        public Task<ReasoningCapability> GetReasoningAsync(
            string endpoint, string modelId, string? apiKey, CancellationToken ct = default)
            => Task.FromResult(ReasoningCapability.Unknown);
    }

    private static string TempRoot() =>
        Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), $"spla-compact-{Guid.NewGuid():N}")).FullName;

    private static (AgentRuntime Runtime, ChatRegistry Chats, string Root) BuildProject(
        ILlmClient client, int? compactTailMessages = null)
    {
        var root = TempRoot();
        var manifest = Path.Combine(root, "test.spla");
        var tailLine = compactTailMessages.HasValue ? $"\n  compact_tail_messages: {compactTailMessages.Value}" : "";
        File.WriteAllText(manifest, $"""
            version: 1
            name: CompactTest
            workspace: .
            agent:
              mode: Edit{tailLine}
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

    [Fact]
    public async Task Compact_hides_the_prefix_behind_Never_and_inserts_the_summary_before_the_tail()
    {
        var client = new FixedFakeLlmClient("ok");
        var (runtime, chats, root) = BuildProject(client, compactTailMessages: 1);
        try
        {
            var chat = chats.CreateNew("t");
            await chat.SendAsync("first", AllowAll, NoClarify, CancellationToken.None);
            await chat.SendAsync("second", AllowAll, NoClarify, CancellationToken.None);
            await chat.SendAsync("third", AllowAll, NoClarify, CancellationToken.None);

            var result = await chat.CompactAsync(CancellationToken.None);

            Assert.True(result.Compacted);

            var messages = chat.Messages;
            var summary = Assert.Single(messages, m => m.CompactSummary);
            Assert.StartsWith("C-", summary.MsgId);
            Assert.Equal(ChatRole.User, summary.Role);
            Assert.Contains("--- Compacted context (summary) ---", summary.Content);

            // Everything the summary covers is hidden from the model, not erased from history.
            var hidden = messages.Where(m => m.CompactedBy == summary.MsgId).ToList();
            Assert.NotEmpty(hidden);
            Assert.All(hidden, m => Assert.Equal(ContextRetention.Never, m.RetentionPolicy));
            Assert.All(hidden, m => Assert.Contains(m, messages));   // still in history, not deleted

            // Only the last human turn ("third") plus its answer, plus the summary, survive assembly.
            var assembled = ContextAssembler.Assemble(messages.Where(m => m.Role != ChatRole.System));
            Assert.Contains(assembled, m => m.CompactSummary);
            Assert.Contains(assembled, m => m.Content == "third");
            Assert.DoesNotContain(assembled, m => m.Content == "first");
            Assert.DoesNotContain(assembled, m => m.Content == "second");

            // The summary sits immediately before the kept tail's first message.
            var list = messages.ToList();
            var summaryIdx = list.IndexOf(summary);
            var thirdIdx = list.FindIndex(m => m.Content == "third");
            Assert.True(summaryIdx >= 0 && thirdIdx == summaryIdx + 1);

            // The tail boundary is structurally a human turn: InsertSummaryBefore was handed the first
            // kept human message, so nothing between an assistant's tool_calls and its results can ever
            // land on either side of it — that pairing only ever sits strictly inside one segment.
            Assert.Equal(ChatRole.User, list[thirdIdx].Role);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Compact_refuses_when_history_is_shorter_than_the_tail()
    {
        var client = new FixedFakeLlmClient("ok");
        var (runtime, chats, root) = BuildProject(client, compactTailMessages: 5);
        try
        {
            var chat = chats.CreateNew("t");
            await chat.SendAsync("only one", AllowAll, NoClarify, CancellationToken.None);

            var result = await chat.CompactAsync(CancellationToken.None);

            Assert.False(result.Compacted);
            Assert.Equal(ChatRuntime.CompactRefusal.NothingToCompact, result.Refusal);
            Assert.DoesNotContain(chat.Messages, m => m.CompactSummary);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Compact_refuses_mid_turn()
    {
        var gate = new TaskCompletionSource();
        var client = new BlockingUntilGateClient(gate.Task);
        var (runtime, chats, root) = BuildProject(client, compactTailMessages: 1);
        try
        {
            var chat = chats.CreateNew("t");
            var turn = chat.SendAsync("hello", AllowAll, NoClarify, CancellationToken.None);
            await WaitUntil(() => chat.Messages.Any(m => m.Role == ChatRole.User));

            var result = await chat.CompactAsync(CancellationToken.None);
            Assert.False(result.Compacted);
            Assert.Equal(ChatRuntime.CompactRefusal.Busy, result.Refusal);

            gate.SetResult();
            await turn;
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    /// <summary>
    /// The ChatRuntime-level tests above exercise <c>CompactAsync</c> through a real chat; this one
    /// isolates the piece of its logic that decides whether a scope marker gets touched (ADR §2.3),
    /// at the <see cref="Conversation"/> primitive it is built from — <c>ChatRuntime</c> has no public
    /// seam to inject a mid-history scope marker without driving the whole AgentsScopeStage pipeline.
    /// Mirrors exactly what <c>ChatRuntime.CompactAsync</c> does to its prefix: mark everything
    /// <see cref="ContextRetention.Never"/> and <see cref="ChatMessage.CompactedBy"/> except scope
    /// markers and labels.
    /// </summary>
    [Fact]
    public void Scope_markers_are_skipped_when_compaction_hides_its_prefix()
    {
        var convo = new Conversation();
        var first = new ChatMessage { Role = ChatRole.User, Content = "first" };
        convo.Add(first);
        var marker = convo.AddScopeMarker("src/backend");
        var second = new ChatMessage { Role = ChatRole.User, Content = "second" };
        convo.Add(second);

        var summary = convo.InsertSummaryBefore(second, "--- Compacted context (summary) ---\nsummary text");
        foreach (var m in new[] { first, marker })
        {
            if (m.ScopeMarker != null || m.IsLabel) continue;
            m.RetentionPolicy = ContextRetention.Never;
            m.CompactedBy = summary.MsgId;
        }

        // Never touched by compaction: still Persistent, never CompactedBy.
        Assert.Equal(ContextRetention.Persistent, marker.RetentionPolicy);
        Assert.Null(marker.CompactedBy);
        // Still excluded from the model, exactly as before compaction (ADR §2.3), and still present.
        Assert.Contains(convo.Messages, m => m.ScopeMarker == "src/backend");
        var assembled = ContextAssembler.Assemble(convo.Messages);
        Assert.DoesNotContain(assembled, m => m.ScopeMarker != null);
        Assert.Contains(assembled, m => m.CompactSummary);
    }

    [Fact]
    public async Task Save_and_reload_after_compaction_shows_the_model_the_same_thing()
    {
        var client = new FixedFakeLlmClient("ok");
        var (runtime, chats, root) = BuildProject(client, compactTailMessages: 1);
        try
        {
            var chat = chats.CreateNew("t");
            await chat.SendAsync("first", AllowAll, NoClarify, CancellationToken.None);
            await chat.SendAsync("second", AllowAll, NoClarify, CancellationToken.None);
            var result = await chat.CompactAsync(CancellationToken.None);
            Assert.True(result.Compacted);

            var beforeAssembled = ContextAssembler.Assemble(chat.Messages.Where(m => m.Role != ChatRole.System))
                .Select(m => (m.Role, m.Content)).ToList();

            var reloaded = runtime.ChatManager.LoadChat(chat.ChatId);
            Assert.NotNull(reloaded);
            var reopened = new ChatRuntime(runtime, reloaded!);

            var afterAssembled = ContextAssembler.Assemble(reopened.Messages.Where(m => m.Role != ChatRole.System))
                .Select(m => (m.Role, m.Content)).ToList();

            Assert.Equal(beforeAssembled, afterAssembled);
            Assert.Contains(reopened.Messages, m => m.CompactSummary);
            Assert.Contains(reopened.Messages, m => m.CompactedBy != null && m.RetentionPolicy == ContextRetention.Never);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Rollback_across_the_compaction_boundary_restores_everything()
    {
        var client = new FixedFakeLlmClient("ok");
        var (runtime, chats, root) = BuildProject(client, compactTailMessages: 1);
        try
        {
            var chat = chats.CreateNew("t");
            await chat.SendAsync("first", AllowAll, NoClarify, CancellationToken.None);
            var anchor = chat.Messages.Last(m => m.Content == "first" && m.Role == ChatRole.User);
            await chat.SendAsync("second", AllowAll, NoClarify, CancellationToken.None);

            var result = await chat.CompactAsync(CancellationToken.None);
            Assert.True(result.Compacted);
            Assert.Contains(chat.Messages, m => m.CompactSummary);

            // Rewind back to (and including) the anchor before the summary was ever inserted — this
            // removes the summary itself, which must hand every message it covered back to Persistent.
            var rewound = chat.Rewind(anchor.MsgId, before: false);
            Assert.True(rewound);

            Assert.DoesNotContain(chat.Messages, m => m.CompactSummary);
            Assert.DoesNotContain(chat.Messages, m => m.CompactedBy != null);
            Assert.All(chat.Messages, m => Assert.Equal(ContextRetention.Persistent, m.RetentionPolicy));
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Double_compact_then_rollback_of_the_second_leaves_the_first_intact()
    {
        var client = new FixedFakeLlmClient("ok");
        var (runtime, chats, root) = BuildProject(client, compactTailMessages: 1);
        try
        {
            var chat = chats.CreateNew("t");
            await chat.SendAsync("first", AllowAll, NoClarify, CancellationToken.None);
            await chat.SendAsync("second", AllowAll, NoClarify, CancellationToken.None);
            var firstCompact = await chat.CompactAsync(CancellationToken.None);
            Assert.True(firstCompact.Compacted);
            var firstSummary = chat.Messages.Single(m => m.CompactSummary);

            await chat.SendAsync("third", AllowAll, NoClarify, CancellationToken.None);
            var secondCompact = await chat.CompactAsync(CancellationToken.None);
            Assert.True(secondCompact.Compacted);

            var secondSummary = chat.Messages.Single(m => m.CompactSummary && m.MsgId != firstSummary.MsgId);
            // The first compaction's own summary is itself now hidden behind the second, like any
            // other message the second compaction's prefix covered.
            Assert.Equal(secondSummary.MsgId, firstSummary.CompactedBy);
            Assert.Equal(ContextRetention.Never, firstSummary.RetentionPolicy);

            var rewound = chat.Rewind(secondSummary.MsgId, before: true);
            Assert.True(rewound);

            // Rolling back the second compaction's own summary un-hides exactly what IT hid — the
            // first summary is visible again, and everything before it stays hidden behind it still.
            Assert.Contains(chat.Messages, m => m.MsgId == firstSummary.MsgId);
            var restoredFirstSummary = chat.Messages.Single(m => m.MsgId == firstSummary.MsgId);
            Assert.Equal(ContextRetention.Persistent, restoredFirstSummary.RetentionPolicy);
            Assert.Null(restoredFirstSummary.CompactedBy);
        }
        finally { runtime.Dispose(); Directory.Delete(root, recursive: true); }
    }

    private static async Task WaitUntil(Func<bool> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("condition never became true");
            await Task.Delay(5);
        }
    }

    /// <summary>Blocks until the test releases it — used to hold a turn open so a concurrent
    /// <c>CompactAsync</c> observes the busy gate.</summary>
    private sealed class BlockingUntilGateClient : ILlmClient, IReasoningCatalog
    {
        private readonly Task _gate;
        public BlockingUntilGateClient(Task gate) { _gate = gate; }

        public async Task<LlmTurnResult> InvokeAsync(LlmTurnContext ctx, CancellationToken ct = default)
        {
            await _gate;
            return new LlmTurnResult
            {
                Message = new ChatMessage { Role = ChatRole.Assistant, Content = "answer" },
                Status = LlmTurnStatus.Ok
            };
        }

        public Task<ReasoningCapability> GetReasoningAsync(
            string endpoint, string modelId, string? apiKey, CancellationToken ct = default)
            => Task.FromResult(ReasoningCapability.Unknown);
    }
}
