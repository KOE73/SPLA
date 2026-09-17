using System.Text;
using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;
using SPLA.Domain.Tools;

namespace SPLA.Runtime;

/// <summary>
/// The <see cref="ISpawnedSession"/> a real chat host hands to <c>SpawnedAgentRunner</c> — see
/// <c>docs/adr/ADR_20260902_core_session-unification.md</c> §2.1. Deliberately not a
/// <see cref="ChatRuntime"/>: a spawned run is driven to one result by the runner itself, not by the
/// turn-pump machinery a human chat needs (rewind, fork, live settings edits, reconnecting watchers).
/// What this DOES share with a human chat is the thing the ADR actually asks for — the same on-disk
/// format, the same <see cref="SPLA.Domain.Settings.ChatManager"/>, a real <see cref="ChatInbox"/>,
/// <see cref="ProgressHub"/> and <see cref="BackgroundTaskRegistry"/> wired the identical way
/// <see cref="ChatRuntime"/> wires its own — so any tool this run calls sees the exact same
/// <see cref="IBackgroundTaskHost"/> shape a human chat's tools do.
/// <para>
/// ADR_20260910-2 wave 2 gives it the same <see cref="ChatFeed"/> a <see cref="ChatRuntime"/> has —
/// see <see cref="Feed"/> and the Publish* members below — so a window opened on this session mid-run
/// sees the run live instead of the empty file <see cref="Finish"/> has not written yet.
/// </para>
/// </summary>
public sealed class SpawnedSession : ISpawnedSession, IBackgroundTaskHost, IChatFeedSession
{
    private readonly ChatRegistry _registry;
    private readonly AgentRuntime _runtime;
    private readonly ChatSession _chat;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SPLA.Domain.Host.ISandbox _sandbox;

    public ProgressHub Progress { get; } = new();

    /// <summary>
    /// A real inbox, wired the identical way <see cref="ChatRuntime.Inbox"/> is — but, unlike a human
    /// chat's, nothing drains it while the run is going. <c>SpawnedAgentRunner.RunAsync</c> drives one
    /// <c>ConversationOrchestrator.RunAsync</c> call straight through to completion with no
    /// <c>DrainInbox</c>/<c>OnMessageDelivered</c> wired (there is no <c>ChatPump</c> for a spawned run
    /// to begin with — a driven-to-one-result run has no turn boundary to wake a NEW turn at).
    /// <para>
    /// PLAN_20260902 wave 4 decision (found in wave 2): a correspondence reply that lands here while
    /// the run is still in progress simply waits, undrained, in this queue — it is not lost, only
    /// unread. Once <see cref="Finish"/> has written <c>Spawn.Outcome</c>, the session is an ordinary
    /// chat with its own <see cref="ChatRuntime"/> and pump, which will drain whatever is still queued
    /// here on the very next turn like any other pending item. The alternative — draining mid-run for
    /// symmetry with a live chat — was rejected for this wave: it would mean either a second
    /// concurrent loop over the same <c>Conversation</c> the running orchestrator already owns, or
    /// growing a mid-run pump for a single-shot run, and wave 4 has no reply tool yet to be the
    /// consumer of either. Wave 5's <c>reply_&lt;role&gt;</c> tool help text must say plainly that a
    /// spawned correspondent answers only after its run completes.
    /// </para>
    /// </summary>
    public ChatInbox Inbox { get; } = new();

    public BackgroundTaskRegistry Tasks { get; }

    public string ChatId => _chat.Id;
    public string Title => _chat.Title;
    public IAgentSession AgentSession { get; }

    /// <summary>This session's own event stream — the same class <see cref="ChatRuntime.Feed"/> uses
    /// (ADR_20260910-2 §4.1, wave 2: "у каждого сеанса — ChatRuntime и SpawnedSession одинаково —
    /// есть поток событий"). Public so the service's <c>ChatFeedWireSubscriber</c> can subscribe to it
    /// exactly as it subscribes a human chat's.</summary>
    public ChatFeed Feed { get; } = new();

    /// <summary>The orchestrator's own live conversation, attached once by <c>SpawnedAgentRunner</c>
    /// right after it is created — see <see cref="AttachConversation"/>. Backs
    /// <see cref="BuildSnapshot"/> until <see cref="Finish"/> writes the file; null before it is
    /// attached (a run without a session never calls in here).</summary>
    private IReadOnlyList<ChatMessage>? _conversation;

    private readonly Lock _liveGate = new();
    private readonly StringBuilder _liveContent = new();
    private readonly StringBuilder _liveReasoning = new();
    private int? _liveIndex;
    private int _bubbleSeq;

    /// <summary>The role's settings this run acts under, or null for the project's. See
    /// <see cref="ISpawnSessionHost.OpenSpawnedSession"/>.</summary>
    private readonly SPLA.Domain.Settings.ResolvedSettings? _settings;

    public SpawnedSession(ChatRegistry registry, ChatSession chat, SPLA.Domain.Settings.ResolvedSettings? settings = null)
    {
        _registry = registry;
        _runtime = registry.Runtime;
        _chat = chat;
        _settings = settings;
        Tasks = new BackgroundTaskRegistry(_lifetime.Token);

        // Its own shell, like any chat's — see ChatRuntime's own comment on why this must not be the
        // runtime's shared sandbox: a process a nested spawn starts must not outlive it with nothing
        // able to say otherwise.
        _sandbox = _runtime.Sandbox.ForChat();
        // The role's own shell_timeout_seconds on that shell, the same rule ChatRuntime follows.
        if (settings is { } role && role.ShellTimeoutSeconds != _runtime.Settings.ShellTimeoutSeconds &&
            _sandbox is SPLA.Domain.Host.PassthroughSandbox runSandbox)
            runSandbox.SetShellSilentIdle(role.ShellTimeoutSeconds > 0
                ? TimeSpan.FromSeconds(role.ShellTimeoutSeconds) : Timeout.InfiniteTimeSpan);

        AgentSession = new Domain.Agent.AgentSession(
            new KeyValueStore("session"), new CheckpointManager(), new SkillSession(),
            sandbox: _sandbox, background: this, chatId: chat.Id, settings: settings);
    }

    // ── ADR_20260910-2 wave 2: ISpawnedSession's Publish* seam ──────────────────────────────────

    public void AttachConversation(IReadOnlyList<ChatMessage> conversation) => _conversation = conversation;

    private void Emit(ChatEvent e) => Feed.Publish(e);
    private void Emit(ChatEvent e, Action mutate) => Feed.Publish(e, mutate);

    /// <summary>Starts a new streaming bubble — <see cref="ChatRuntime.NextBubbleIndex"/>'s own logic,
    /// copied rather than shared: a spawned session has no chat-wide bubble counter to share with
    /// anything else, only this one run.</summary>
    private int NextBubbleIndex()
    {
        var index = Interlocked.Increment(ref _bubbleSeq);
        lock (_liveGate)
        {
            _liveIndex = index;
            _liveContent.Clear();
            _liveReasoning.Clear();
        }
        return index;
    }

    private void ClearLive()
    {
        lock (_liveGate)
        {
            _liveIndex = null;
            _liveContent.Clear();
            _liveReasoning.Clear();
        }
    }

    private int _currentMsgIndex;

    public void PublishLlmTurnStart(IReadOnlyList<ChatMessage> context)
    {
        _currentMsgIndex = NextBubbleIndex();
        Emit(new ChatLlmCallStarted(_currentMsgIndex, context, null) { ChatId = ChatId });
    }

    public void PublishDelta(string chunk) =>
        Emit(new ChatDelta(_currentMsgIndex, chunk) { ChatId = ChatId },
            () => { lock (_liveGate) _liveContent.Append(chunk); });

    public void PublishReasoning(string chunk) =>
        Emit(new ChatReasoning(_currentMsgIndex, chunk) { ChatId = ChatId },
            () => { lock (_liveGate) _liveReasoning.Append(chunk); });

    public void PublishAssistantMessage(ChatMessage message) =>
        Emit(new ChatAssistantMessage(_currentMsgIndex, message) { ChatId = ChatId }, ClearLive);

    public void PublishAttempt(GenerationAttempt attempt) =>
        Emit(new ChatAttempt(_currentMsgIndex, attempt) { ChatId = ChatId });

    public void PublishToolStarted(ToolCall call) =>
        Emit(new ChatToolStarted(call) { ChatId = ChatId });

    public void PublishToolProgress(ToolCall call, ToolProgress progress) =>
        Emit(new ChatToolProgress(call, progress) { ChatId = ChatId });

    public void PublishToolResult(ToolCall call, ToolResult result) =>
        Emit(new ChatToolResult(call, result) { ChatId = ChatId });

    public void PublishLlmTurn(LlmTurnResult turn) =>
        Emit(new ChatLlmTurn(turn, null) { ChatId = ChatId });

    public void PublishNotice(string text) =>
        Emit(new ChatNotice(text) { ChatId = ChatId });

    /// <summary>Atomically captures this session's snapshot alongside <paramref name="attach"/> — the
    /// same tmux-style "снимок плюс поток" <see cref="ChatRuntime.SnapshotForOpen"/> gives a human
    /// chat (ADR_20260910-2 §4.4). Read from <see cref="_conversation"/> — the run's own in-memory
    /// messages — never from disk, which is empty until <see cref="Finish"/> runs.</summary>
    public ChatFeedSnapshot SnapshotForOpen(Action attach) => Feed.SnapshotUnderGate(BuildSnapshot, attach);

    /// <inheritdoc cref="IChatFeedSession.ResubscribeQueuedWithSnapshot"/>
    public (ChatFeedSnapshot Snapshot, IDisposable Subscription) ResubscribeQueuedWithSnapshot(
        Func<ChatEvent, Task> handler, Action onDetached)
        => Feed.SubscribeQueuedWithSnapshot(handler, onDetached, BuildSnapshot);

    private ChatRuntime.LivePartial? Live
    {
        get
        {
            lock (_liveGate)
            {
                if (_liveIndex is not { } index) return null;
                if (_liveContent.Length == 0 && _liveReasoning.Length == 0) return null;
                return new ChatRuntime.LivePartial(index, _liveContent.ToString(), _liveReasoning.ToString());
            }
        }
    }

    private ChatFeedSnapshot BuildSnapshot() => new(
        _conversation?.Where(m => m.Role != ChatRole.System).ToList() ?? new List<ChatMessage>(),
        Live,
        Progress.Trees.SelectMany(kv => kv.Value.Nodes
                .Where(n => n.State == SPLA.Domain.Models.ProgressState.Running)
                .Select(n => new ChatFeedProgressNode(kv.Key, n)))
            .ToList(),
        // A spawned session has no per-chat pending-ask surface yet — a run has no way to raise one
        // today (permission/clarify handlers come from the runner's caller, not from this session).
        // Deviation noted in the plan; revisit if a spawned run ever needs to ask.
        Array.Empty<PendingAsk>(),
        Tasks.All.Where(t => t.State == SPLA.Domain.Tools.BackgroundTaskState.Running).ToList());

    public void Finish(IReadOnlyList<ChatMessage> conversation, string? skillId, string mode,
        DateTimeOffset startedAt, string outcome, string? error)
    {
        // The run's own end of turn, on the SAME feed a watcher's snapshot subscription just read from
        // — a window attached mid-run sees the transcript complete instead of going quiet with no
        // signal at all (ADR_20260910-2 §4.8's "клиент... получает конец хода").
        Emit(new ChatTurnCompleted(outcome == "cancelled", error, skillId) { ChatId = ChatId });

        var saveToolCalls = (_settings ?? _runtime.Settings).SaveToolCalls;
        var saveAttempts = (_settings ?? _runtime.Settings).SaveAttempts;

        _chat.Messages = conversation
            .Where(m => m.Role != ChatRole.System)
            .Select(m => new ChatSessionMessage
            {
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content ?? "",
                Reasoning = string.IsNullOrEmpty(m.Reasoning) ? null : m.Reasoning,
                CreatedAt = m.CreatedAt,
                ToolCalls = saveToolCalls && m.ToolCalls?.Count > 0 ? m.ToolCalls : null,
                ToolCallId = saveToolCalls ? m.ToolCallId : null,
                Attempts = saveAttempts && m.Attempts?.Count > 0
                    ? m.Attempts.Select(a => new ChatSessionAttempt
                    {
                        Index = a.Index,
                        Outcome = a.Outcome.ToString(),
                        Content = a.Content,
                        Reasoning = a.Reasoning,
                        Note = a.Note,
                        Chars = a.Chars,
                        DurationMs = (long)a.Duration.TotalMilliseconds,
                        WaitMs = a.Wait is { } w ? (long)w.TotalMilliseconds : null,
                        WaitStated = a.WaitStated
                    }).ToList()
                    : null
            })
            .ToList();

        _chat.Kv = ((KeyValueStore)AgentSession.SessionKv).Snapshot();
        _chat.Spawn = new ChatSessionSpawnInfo
        {
            SkillId = skillId,
            Mode = mode,
            StartedAt = startedAt.UtcDateTime,
            FinishedAt = DateTime.UtcNow,
            Outcome = outcome,
            Error = error
        };

        _runtime.ChatManager.SaveChat(_chat);

        // Tells the registry this session's run is over: drops it from the "live spawned session"
        // table (so the NEXT chat.open loads the just-written file into an ordinary ChatRuntime) and
        // fires SpawnedClosed, which is what unhooks this session's ChatFeedWireSubscriber — the
        // symmetric close ChatRegistry.RuntimeClosed already gives a human chat (ADR_20260910-2 §4.8).
        _registry.NotifySpawnedFinished(this);
    }

    public void Dispose()
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        (_sandbox as IDisposable)?.Dispose();
    }
}
