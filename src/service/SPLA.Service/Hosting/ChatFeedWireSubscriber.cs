using System.Collections.Concurrent;
using System.Linq;
using SPLA.Runtime;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Translates one chat's <see cref="ChatRuntime.Feed"/> onto the wire — the wave-0 subscriber
/// ADR_20260910-2 §4.1/§4.6 asks for ("провод — подписчик"). One instance per chat, constructed from
/// <see cref="ChatRegistry.RuntimeOpened"/> and disposed on <see cref="ChatRegistry.RuntimeClosed"/>,
/// the same lifetime <c>SplaServiceHost.WireChatProgress</c> (now folded in here) and the old
/// per-chat <c>Tasks.Changed</c>/<c>Asks</c> forwarding used to have separately.
///
/// <para>
/// Replaces <c>ChatTurnDriver.BuildCallbacks</c> and <c>SplaServiceHost.WireChatProgress</c> outright:
/// every message type and payload shape below is copied verbatim from what those two used to build, so
/// the wire protocol does not change at all (ADR §4.2's "провод... не меняется"). The one thing that
/// DID move is which events assign <c>msgIndex</c> and which fire <c>runtime.Turns.Touch</c> — that is
/// this class now, uniformly, rather than a per-turn <c>TurnContext</c>.
/// </para>
///
/// <para>
/// <b>Wave 4 — queued, not synchronous.</b> Before this wave <c>OnEvent</c> ran inside
/// <see cref="ChatFeed.Publish"/>'s lock and fired every send with <c>_ = ToWatchers(...)</c>: the
/// chat's own turn was never held up waiting for a socket (the send Task was never awaited), but
/// nothing serialized those un-awaited sends against each other either — two events published back to
/// back could reach <c>ClientConnection.SendAsync</c>'s <c>_sendLock</c> in either order once a socket
/// write actually suspended, which is a silent frame reorder on the wire, not just a slow window. Now
/// <see cref="ChatFeed.SubscribeQueued"/> hands events to <see cref="OnEventAsync"/> one at a time, in
/// order, on a single dedicated consumer task per chat: publishing only ever touches the queue (fast,
/// synchronous, no I/O — see <c>ChatFeed.QueuedSubscription.Enqueue</c>), and this class's own await
/// chain is what now guarantees "per-connection order" for real, because it never starts a second send
/// before the previous one's <c>Task.WhenAll</c> across every watching connection has completed.
/// </para>
/// </summary>
internal sealed class ChatFeedWireSubscriber : IDisposable
{
    private readonly ConnectionHub _hub;
    private readonly AgentRuntimeRegistry _registry;
    private readonly AgentRuntime _runtime;
    private readonly string _projectId;
    private readonly IChatFeedSession _chat;
    private IDisposable _subscription;
    private int _disposed;

    /// <summary>Per-node last-sent time, exactly <c>WireChatProgress</c>'s own — the 120ms throttle is a
    /// wire concern (a slow client should not need every intermediate tick), not something the chat's
    /// own feed enforces.</summary>
    private readonly ConcurrentDictionary<string, DateTime> _lastProgressNodeSent = new();

    /// <summary>Single-bar tool-progress throttle — <c>ChatTurnDriver.BuildCallbacks</c>'s own
    /// <c>lastProgress</c>, one clock per chat now instead of one per turn. The behavioural difference
    /// (a burst of progress split across two turns shares a clock) is not observable on the wire: the
    /// first tick of a new turn's tool call is a different <c>ToolCallId</c> and was never coalesced
    /// with the previous turn's ticks by the client either way.</summary>
    private DateTime _lastToolProgressSent = DateTime.MinValue;

    public ChatFeedWireSubscriber(
        ConnectionHub hub, AgentRuntimeRegistry registry, AgentRuntime runtime, string projectId, IChatFeedSession chat)
    {
        _hub = hub;
        _registry = registry;
        _runtime = runtime;
        _projectId = projectId;
        _chat = chat;
        _subscription = chat.Feed.SubscribeQueued(OnEventAsync, OnDetached);
    }

    public void Dispose()
    {
        Volatile.Write(ref _disposed, 1);
        _subscription.Dispose();
    }

    /// <summary>Called off the feed's queue consumer when the structural backlog overflows
    /// (<see cref="ChatFeed.StructuralOverflowThreshold"/>) — resubscribes with a fresh queue and
    /// captures the resync snapshot in the SAME gate acquisition
    /// (<see cref="IChatFeedSession.ResubscribeQueuedWithSnapshot"/>), then re-sends every currently
    /// watching connection a <c>chat.opened</c> built from that one snapshot. Doing the two separately
    /// used to leave a gap: events published between "subscribe" and "snapshot, taken later, off the
    /// gate, once per connection" landed in the new queue AND in the snapshot, reaching the client
    /// twice (ADR_20260910-2 plan, wave 4 "Открыто" — resolved here). A duplicate <c>chat.opened</c>
    /// itself is still harmless (the web client resets its item list on every one, wave 1); what this
    /// fixes is the events sent alongside/after it no longer overlapping what the snapshot already
    /// contains.</summary>
    private void OnDetached()
    {
        if (Volatile.Read(ref _disposed) != 0) return; // a real unsubscribe raced the overflow — nothing to resync

        // The new queue must not send anything before the resync chat.opened has gone out: the client
        // rebuilds its log from chat.opened, so an event newer than the snapshot that overtook it would
        // be wiped by that rebuild and never shown. The new subscription's handler waits on this.
        var resent = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var (snapshot, subscription) = _chat.ResubscribeQueuedWithSnapshot(
            async e => { await resent.Task; await OnEventAsync(e); }, OnDetached);
        _subscription = subscription;
        _ = Task.Run(async () =>
        {
            try { await _hub.ForEachWatcherAsync(_chat.ChatId, c => ResendOpenedAsync(c, snapshot)); }
            finally { resent.TrySetResult(); }
        });
    }

    private Task ResendOpenedAsync(ClientConnection c, ChatFeedSnapshot snapshot) => _chat switch
    {
        ChatRuntime chatRuntime => c.SendOpenedFromSnapshotAsync(chatRuntime, snapshot),
        SPLA.Runtime.SpawnedSession spawned => c.SendOpenedSpawnedFromSnapshotAsync(spawned, snapshot),
        _ => Task.CompletedTask
    };

    private async Task OnEventAsync(ChatEvent e)
    {
        // Every event is proof the turn is moving — see TurnRegistry.Touch's own comment. Previously
        // this fired from inside ChatTurnDriver.BuildCallbacks' ToWatchers wrapper; now it fires here,
        // for every event this chat publishes, turn-driven or pump-woken alike.
        _runtime.Turns.Touch(_chat.ChatId);

        switch (e)
        {
            case ChatTurnStarted:
                // The chat already reports itself busy the instant SendAsync acquires its gate — the
                // sidebar mark must appear with no window in which the chat still claims to be idle.
                // A spawned session's list entry is driven by its file's Spawn.Outcome instead (see
                // RuntimeProjections.ToSummary) — nothing to refresh here for one of those.
                if (_chat is ChatRuntime) await BroadcastChatListAsync();
                break;

            case ChatUserMessage m:
                await ToWatchers(MessageTypes.UserMessage, new UserMessagePayload
                {
                    MsgId = m.Message.MsgId,
                    CreatedAt = m.Message.CreatedAt.ToString("o"),
                    Text = m.Message.Content,
                    PeerFrom = m.Message.PeerFrom
                });
                break;

            case ChatLlmCallStarted s:
                await ToWatchers(MessageTypes.LlmTurnStart, new DeltaPayload
                {
                    MsgIndex = s.MsgIndex, Text = "", ProgressTreeId = s.ProgressTreeId
                });
                break;

            case ChatDelta d:
                await ToWatchers(MessageTypes.Delta, new DeltaPayload { MsgIndex = d.MsgIndex, Text = d.Text });
                break;

            case ChatReasoning r:
                await ToWatchers(MessageTypes.Reasoning, new ReasoningPayload { MsgIndex = r.MsgIndex, Text = r.Text });
                break;

            case ChatAttempt a:
                await ToWatchers(MessageTypes.Attempt, new AttemptPayload
                {
                    MsgIndex = a.MsgIndex,
                    Index = a.Attempt.Index,
                    Outcome = a.Attempt.Outcome.ToString(),
                    Note = a.Attempt.Note,
                    Chars = a.Attempt.Chars,
                    DurationMs = (long)a.Attempt.Duration.TotalMilliseconds,
                    WaitMs = a.Attempt.Wait is { } w ? (long)w.TotalMilliseconds : null,
                    WaitStated = a.Attempt.WaitStated,
                    Content = a.Attempt.Content,
                    Reasoning = a.Attempt.Reasoning
                });
                break;

            case ChatAssistantMessage m:
                await ToWatchers(MessageTypes.AssistantMessage,
                    new AssistantMessagePayload { MsgIndex = m.MsgIndex, Message = ProtocolMapper.ToDto(m.Message) });
                break;

            case ChatToolStarted t:
                await ToWatchers(MessageTypes.ToolStarted, new ToolStartedPayload { ToolCall = ProtocolMapper.ToDto(t.Call) });
                break;

            case ChatToolProgress p:
                await OnToolProgressAsync(p);
                break;

            case ChatProgressNode n:
                await OnProgressNodeAsync(n);
                break;

            case ChatToolResult r:
                await ToWatchers(MessageTypes.ToolResult, new ToolResultPayload
                {
                    ToolCallId = r.Call.Id,
                    ToolName = r.Call.Function.Name,
                    Result = r.Result.TextContent,
                    Outcome = r.Result.Outcome.ToString(),
                    Reason = r.Result.Reason,
                    Resources = r.Result.Content.OfType<SPLA.Domain.Models.ToolResource>()
                        .Select(res => new ToolResourceDto { Uri = res.Uri, MimeType = res.MimeType, Description = res.Description })
                        .ToList() is { Count: > 0 } resources ? resources : null
                });
                break;

            case ChatNotice n:
                await ToWatchers(MessageTypes.Notice, new NoticePayload { Text = n.Text });
                break;

            case ChatLlmTurn turn:
                await ToWatchers(MessageTypes.TokenUsage, new TokenUsagePayload
                {
                    PromptTokens = turn.Turn.Message.PromptTokens,
                    CompletionTokens = turn.Turn.Message.CompletionTokens,
                    ContextLength = turn.ContextLength
                });
                await _hub.BroadcastToProjectAsync(_projectId, MessageTypes.UsageResult, SettingsOps.GetUsage(_runtime));
                break;

            case ChatAskRaised ar:
                await _hub.BroadcastToWatchersAsync(
                    _chat.ChatId, ProtocolMapper.MessageTypeFor(ar.Ask), ProtocolMapper.PayloadFor(ar.Ask), ar.Ask.RequestId);
                break;

            case ChatAskResolved ar:
                await _hub.BroadcastToWatchersAsync(
                    _chat.ChatId, MessageTypes.AskResolved,
                    new AskResolvedPayload { Reason = ProtocolMapper.ReasonName(ar.Reason) }, ar.Ask.RequestId);
                break;

            case ChatTaskChanged tc:
                await ToWatchers(MessageTypes.TaskStateChanged,
                    new TaskStateChangedPayload { ChatId = _chat.ChatId, Task = ProtocolMapper.ToDto(tc.Task) });
                break;

            case ChatTurnCompleted c:
                // Same three broadcasts, same order, ChatTurnDriver.RunTurnAsync used to send after
                // awaiting the turn: the end-of-turn state (TurnComplete), what the model reached for
                // (ChatToolSetState) and the sidebar's own mark going idle again. The latter two read a
                // ChatRuntime's tool-set session and the human chat list — neither exists for a spawned
                // session's one run (its "list entry", if any, is its parent's; ToolSets is a human-chat
                // concept), so both are skipped for one of those; TurnComplete itself is what a spawned
                // window's watcher needs (ADR_20260910-2 §4.8: "клиент... получает конец хода").
                await ToWatchers(MessageTypes.TurnComplete,
                    new TurnCompletePayload { Cancelled = c.Cancelled, Error = c.Error, ActiveSkillId = c.ActiveSkillId });
                if (_chat is ChatRuntime chatRuntime)
                {
                    await ToWatchers(MessageTypes.ChatToolSetState,
                        new ChatToolSetStatePayload
                        {
                            ChatId = _chat.ChatId,
                            Sets = ChatHandlers.ToolSetDtos(_registry.Open(_projectId), chatRuntime)
                        });
                    await BroadcastChatListAsync();
                }
                break;
        }
    }

    private async Task OnToolProgressAsync(ChatToolProgress p)
    {
        var now = DateTime.UtcNow;
        if ((now - _lastToolProgressSent).TotalMilliseconds < 120 && (p.Progress.Fraction ?? 0) < 1.0) return;
        _lastToolProgressSent = now;

        await ToWatchers(MessageTypes.ToolProgress, new ToolProgressPayload
        {
            ToolCallId = p.Call.Id,
            ToolName = p.Call.Function.Name,
            Current = p.Progress.Current ?? 0,
            Total = p.Progress.Total ?? 0,
            Fraction = p.Progress.Fraction,
            Message = p.Progress.Message,
            Details = p.Progress.Details?.Select(d => new ToolProgressDetailDto { Label = d.Label, Value = d.Value }).ToList()
        });
    }

    private async Task OnProgressNodeAsync(ChatProgressNode n)
    {
        var node = n.Node;
        // Namespaced by tree id, exactly as WireChatProgress used to: a background task's tree can be
        // live alongside the current turn's, and their local node ids collide on the flat wire stream
        // without this.
        var wireId = $"{n.TreeId}:{node.Id}";

        var now = DateTime.UtcNow;
        var known = _lastProgressNodeSent.TryGetValue(wireId, out var last);

        // Structural frames (first appearance, finish) are never throttled — see WireChatProgress's own
        // comment, copied verbatim here.
        if (known && node.State == SPLA.Domain.Models.ProgressState.Running
                  && (now - last).TotalMilliseconds < 120) return;

        _lastProgressNodeSent[wireId] = now;

        await ToWatchers(MessageTypes.ProgressNode, ProtocolMapper.ToDto(new ChatFeedProgressNode(n.TreeId, node)));
    }

    private Task ToWatchers(string type, object payload) => _hub.BroadcastToWatchersAsync(_chat.ChatId, type, payload);

    private Task BroadcastChatListAsync() => _hub.BroadcastToProjectAsync(
        _projectId, MessageTypes.ChatListResult, new ChatListResultPayload { Chats = _registry.Open(_projectId).Chats.List() });
}
