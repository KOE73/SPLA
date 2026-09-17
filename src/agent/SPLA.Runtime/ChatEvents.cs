using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;

namespace SPLA.Runtime;

/// <summary>
/// The closed set of events a chat's own stream (<see cref="IChatFeed"/>) carries — ADR_20260910-2
/// §4.2, wave 0. Every entry here is exactly what today reaches the wire through one of the paths the
/// ADR counted (<c>AgentCallbacks</c>, <c>ChatTurnDriver.BuildCallbacks</c>, <c>WireChatProgress</c>,
/// the asks/tasks forwarding in <c>SplaServiceHost.WireOneRuntime</c>): nothing new is introduced.
///
/// <para><b>Why this lives in <c>SPLA.Runtime</c>, not <c>SPLA.Domain</c>.</b> The ADR's own wording
/// ("типы событий... в ядре") points at Domain, and most of the payload types below (
/// <see cref="ChatMessage"/>, <see cref="ToolCall"/>, <see cref="ToolResult"/>, <see cref="ToolProgress"/>,
/// <see cref="ProgressNode"/>, <see cref="LlmTurnResult"/>, <see cref="GenerationAttempt"/>,
/// <see cref="BackgroundTaskRecord"/>) already live there. But the ask events need <see cref="PendingAsk"/>
/// and <see cref="AskResolution"/>, and those are declared in <c>SPLA.Runtime/PendingAsks.cs</c> — they
/// carry <see cref="PermissionDecision"/> plumbing and are already a "runtime" concept the way
/// <see cref="ChatRuntime"/> itself is, not a Domain primitive. Splitting the closed set across two
/// projects (most records in Domain, the two ask records in Runtime) would only make "the events" two
/// different things a reader has to know to find. So the whole set stays together, in the lowest
/// project that can reference every payload type it needs: <c>SPLA.Runtime</c>.</para>
/// </summary>
public abstract record ChatEvent
{
    /// <summary>The chat this event belongs to — every subscriber is already scoped to one chat's
    /// <see cref="IChatFeed"/>, but events are still allowed to carry it for a subscriber (the parent
    /// of a spawned chat, wave 2) that multiplexes several feeds into one place.</summary>
    public required string ChatId { get; init; }

    /// <summary>When the chat itself observed this event — not when a subscriber received it (delivery
    /// is synchronous today, wave 4 may make it not). Informational; no wire mapping depends on it.</summary>
    public DateTimeOffset At { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>A turn started — either from a real human message (<see cref="Text"/> set) or woken by the
/// pump with nothing of its own to add. Mirrors <see cref="MessageTypes"/>' turn-busy bookkeeping,
/// which today comes from <c>ChatTurnDriver</c> rather than the chat.</summary>
public sealed record ChatTurnStarted(string? Text) : ChatEvent;

/// <summary>A turn ended — successfully, cancelled, or with an error. One-to-one with
/// <c>MessageTypes.TurnComplete</c>.</summary>
public sealed record ChatTurnCompleted(bool Cancelled, string? Error, string? ActiveSkillId) : ChatEvent;

/// <summary>A user (or peer-delivered) message just entered the conversation. Replaces the
/// <c>onUserMessage</c> parameter <see cref="ChatRuntime.SendAsync"/> used to take.</summary>
public sealed record ChatUserMessage(ChatMessage Message) : ChatEvent;

/// <summary>An LLM call is about to be made; carries the exact context and the bubble index this
/// call's streamed output belongs to (<see cref="ChatRuntime.NextBubbleIndex"/> — the wire, not the
/// event, is what needs this index; the event carries it so the wire subscriber never has to call back
/// into the chat to learn which bubble it is looking at).</summary>
public sealed record ChatLlmCallStarted(int MsgIndex, IReadOnlyList<ChatMessage> Context, string? ProgressTreeId) : ChatEvent;

/// <summary>A chunk of assistant answer text.</summary>
public sealed record ChatDelta(int MsgIndex, string Text) : ChatEvent;

/// <summary>A chunk of reasoning/chain-of-thought text.</summary>
public sealed record ChatReasoning(int MsgIndex, string Text) : ChatEvent;

/// <summary>A generation attempt the repetition guard abandoned mid-stream.</summary>
public sealed record ChatAttempt(int MsgIndex, GenerationAttempt Attempt) : ChatEvent;

/// <summary>The fully assembled assistant message for the bubble at <paramref name="MsgIndex"/>.</summary>
public sealed record ChatAssistantMessage(int MsgIndex, ChatMessage Message) : ChatEvent;

/// <summary>A tool call is about to run.</summary>
public sealed record ChatToolStarted(ToolCall Call) : ChatEvent;

/// <summary>A running top-level tool call reported progress. Structural events (a node's first
/// appearance/finish) are never throttled by the feed itself — see <see cref="ChatProgressNode"/>,
/// which is what a tree-aware subscriber should prefer; this one is the flat single-bar view
/// <c>AgentCallbacks.OnToolProgress</c> already was.</summary>
public sealed record ChatToolProgress(ToolCall Call, ToolProgress Progress) : ChatEvent;

/// <summary>A tool call finished, outcome included.</summary>
public sealed record ChatToolResult(ToolCall Call, ToolResult Result) : ChatEvent;

/// <summary>One node of a progress tree changed — the turn's own tree or a background task's, keyed by
/// the hub's tree id the way <c>SplaServiceHost.WireChatProgress</c> already namespaces it (that
/// namespacing is wire concern and stays with the wire subscriber, not here — this event carries the
/// tree id and the node itself, unprefixed).</summary>
public sealed record ChatProgressNode(string TreeId, ProgressNode Node) : ChatEvent;

/// <summary>An ephemeral notice for the user. Never sent to the model.</summary>
public sealed record ChatNotice(string Text) : ChatEvent;

/// <summary>The whole provider-reported outcome of one LLM call — token counters included.</summary>
public sealed record ChatLlmTurn(LlmTurnResult Turn, int? ContextLength) : ChatEvent;

/// <summary>A question this chat's turn is waiting on was raised.</summary>
public sealed record ChatAskRaised(PendingAsk Ask) : ChatEvent;

/// <summary>A question was resolved (answered, cancelled, or timed out), whoever closed it.</summary>
public sealed record ChatAskResolved(PendingAsk Ask, AskResolution Reason) : ChatEvent;

/// <summary>A background task belonging to this chat changed state.</summary>
public sealed record ChatTaskChanged(BackgroundTaskRecord Task) : ChatEvent;

/// <summary>
/// A chat's own event stream — ADR_20260910-2 §4.1: "у каждого сеанса... есть поток событий". One per
/// live <see cref="ChatRuntime"/> (and, from wave 2, per spawned session). Subscribing costs nothing
/// beyond holding a delegate; publishing to zero subscribers must be ~free (§4.7) — see
/// <see cref="ChatFeed"/>'s own comment for how that is kept true.
/// <para>
/// Wave 0 delivers synchronously and in order, and publishing may still block on a slow subscriber —
/// the non-blocking queue is wave 4's job, not this interface's. Wave 1 adds the snapshot-on-subscribe
/// promise (<see cref="ChatFeed.SubscribeWithSnapshot{T}"/>, concrete-typed like <see cref="ChatFeed.Publish"/>
/// because only the chat that owns the feed needs it). The shape here is deliberately already what
/// those waves need: a single <see cref="Subscribe"/> call handing back one <see cref="IDisposable"/>.
/// </para>
/// </summary>
public interface IChatFeed
{
    /// <summary>Registers <paramref name="handler"/> to receive every event published from this point
    /// on. Disposing the return value unsubscribes — idempotent, safe to call more than once.</summary>
    IDisposable Subscribe(Action<ChatEvent> handler);
}

/// <summary>
/// What the service's <c>ChatFeedWireSubscriber</c> needs from a session to fan its feed onto the
/// wire — id and stream, nothing else. Implemented identically by <see cref="ChatRuntime"/> and
/// <see cref="SpawnedSession"/> (ADR_20260910-2 §4.8, wave 2: "провод — подписчик... одним и тем же
/// кодом") so one subscriber class serves a human chat and a spawned run's one turn alike.
/// </summary>
public interface IChatFeedSession
{
    string ChatId { get; }
    ChatFeed Feed { get; }

    /// <summary>
    /// Re-subscribes a queued subscription and captures a resync snapshot atomically (same gate
    /// acquisition as the subscription — see <see cref="ChatFeed.SubscribeQueuedWithSnapshot{T}"/>).
    /// The wire subscriber calls this after an overflow-detach instead of calling
    /// <see cref="ChatFeed.SubscribeQueued"/> and a session's own snapshot builder (e.g.
    /// <see cref="ChatRuntime.SnapshotForOpen"/>) separately — doing those two as one call is what
    /// keeps the fresh <c>chat.opened</c> it sends from ever describing a state ahead of (or behind)
    /// the sequence number the new queue actually starts delivering from.
    /// </summary>
    (ChatFeedSnapshot Snapshot, IDisposable Subscription) ResubscribeQueuedWithSnapshot(
        Func<ChatEvent, Task> handler, Action onDetached);
}

/// <summary>One node of an open progress tree in a <see cref="ChatFeedSnapshot"/> — the tree id it
/// belongs to (the hub id <see cref="SPLA.Domain.Tools.ProgressHub.Register"/> returned, unprefixed —
/// wire namespacing is the subscriber's job, same split <see cref="ChatProgressNode"/> already makes)
/// paired with the node itself.</summary>
public sealed record ChatFeedProgressNode(string TreeId, ProgressNode Node);

/// <summary>
/// Everything a subscriber needs to render a chat it is attaching to mid-turn, captured atomically
/// with the feed sequence it was taken at (ADR_20260910-2 §4.4, wave 1): the conversation, the
/// in-flight bubble (if any), every progress node still <see cref="SPLA.Domain.Models.ProgressState.Running"/>,
/// every pending ask, and every background task still running. Read from the chat's own memory, never
/// from disk — a chat mid-turn has nothing on disk worth reading yet (§4.4's "не из файла").
/// </summary>
public sealed record ChatFeedSnapshot(
    IReadOnlyList<ChatMessage> Messages,
    ChatRuntime.LivePartial? Live,
    IReadOnlyList<ChatFeedProgressNode> OpenProgressNodes,
    IReadOnlyList<PendingAsk> PendingAsks,
    IReadOnlyList<BackgroundTaskRecord> RunningTasks);
