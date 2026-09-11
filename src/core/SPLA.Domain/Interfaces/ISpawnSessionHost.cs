using SPLA.Domain.Agent;
using SPLA.Domain.Llm;
using SPLA.Domain.Models;

namespace SPLA.Domain.Interfaces;

/// <summary>
/// What <c>SPLA.Agent.SpawnedAgentRunner</c> needs from a real chat host to open a spawned session on
/// disk — see <c>docs/adr/ADR_20260902_core_session-unification.md</c> §2.1/§5. Same shape as
/// <see cref="Agent.IBackgroundTaskHost"/> and for the same reason: <c>SPLA.Runtime</c> (its
/// <c>ChatRegistry</c>) is the only place that can implement this, and <c>SPLA.Agent</c> must not
/// reference it back, so the seam is a Domain interface, injected once the runtime that implements it
/// exists (<c>AgentRuntimeRegistry.Build</c>, mirroring <c>AgentRuntime.cs</c>'s own construction of
/// <c>SpawnedAgentRunner</c>).
/// <para>
/// Optional on <c>SpawnedAgentRunner</c>: a runner built without one (a bare CLI or worker entry
/// point, or a test) keeps the pre-wave-2 in-memory-only behaviour rather than throwing.
/// </para>
/// </summary>
public interface ISpawnSessionHost
{
    /// <summary>
    /// Creates a spawned session on disk — <c>origin: spawned</c>, <c>parent: parentChatId</c>,
    /// <c>as: role</c> — and returns a handle scoped to the one run that session will ever drive.
    /// <paramref name="parentChatId"/> is null for a spawn with no chat behind it; <paramref name="role"/>
    /// is null for a run with no role.
    /// <paramref name="settings"/> is what the run acts under when it is not the project's own — the
    /// role's resolved settings. Carried on the session's <see cref="IAgentSession.Settings"/>, where
    /// the tool host, tool-set levels and question timeouts read them, and used for what the session
    /// saves. Null = the project's.
    /// </summary>
    ISpawnedSession OpenSpawnedSession(string? parentChatId, string? role, Settings.ResolvedSettings? settings = null);

    /// <summary>
    /// Trims finished spawned sessions down to <paramref name="keep"/> most-recently-finished on disk.
    /// Never touches one whose run is still in progress (trap 5). <c>keep &lt; 0</c> disables trimming;
    /// <c>keep == 0</c> keeps none. Called once a run finishes.
    /// </summary>
    void TrimSpawnedRetention(int keep);
}

/// <summary>
/// One spawned session's handle: a real chat id, a real file, and this session's own per-chat
/// capabilities (KV, skills, sandbox, doubt, <see cref="IBackgroundTaskHost"/>) — everything
/// <c>ChatRegistry</c> already gives a human chat, minus the turn-pump machinery a driven-to-one-result
/// run has no use for. Disposed by the caller once the run ends (releases the session's own sandbox).
/// </summary>
public interface ISpawnedSession : IDisposable
{
    string ChatId { get; }

    /// <summary>This session's own agent state — open an <see cref="AgentSessionScope"/> over this
    /// while driving the run, exactly as a human chat's turn does over its own.</summary>
    IAgentSession AgentSession { get; }

    /// <summary>
    /// Persists the finished conversation (the leading system message is stripped the same way a
    /// human chat's is) and this run's outcome, and clears the in-progress flag — the signal
    /// <c>chat.send</c> and retention both key off (see <see cref="ChatSessionSpawnInfo.Outcome"/>).
    /// Call exactly once, when the run ends (success, failure, or cancellation).
    /// </summary>
    void Finish(IReadOnlyList<ChatMessage> conversation, string? skillId, string mode,
        DateTimeOffset startedAt, string outcome, string? error);

    // ── ADR_20260910-2 wave 2 — this session's own event stream ─────────────────────────────────
    // "SpawnedSession получает поток... Снимок во время прогона — из памяти прогона (переписка
    // оркестратора), не из файла." (PLAN_20260910-2, "Волна 2").
    //
    // The closed ChatEvent set itself lives in SPLA.Runtime (see ChatEvents.cs's own comment on why),
    // and SPLA.Agent — where SpawnedAgentRunner builds these payloads as it drives the orchestrator —
    // must never reference SPLA.Runtime. So the seam here is not "hand back an AgentCallbacks" (that
    // type lives in SPLA.Agent itself and Domain cannot name it either) but a small set of Publish*
    // calls, each typed only in payloads Domain already knows. SPLA.Runtime's SpawnedSession implements
    // these by folding them onto the same ChatFeed/ChatEvent adapter ChatRuntime.SendAsync builds.

    /// <summary>Gives this session the live <see cref="Conversation"/> the orchestrator is filling in,
    /// so a snapshot taken mid-run (a watcher attaching, a reconnect) reads the run's own in-memory
    /// messages rather than the file <see cref="Finish"/> has not written yet. The list reference must
    /// stay backed by the same growing collection the orchestrator appends to — call once, right after
    /// the conversation is created, before the orchestrator runs.</summary>
    void AttachConversation(IReadOnlyList<ChatMessage> conversation);

    /// <summary>An LLM call is about to be made — starts a new streaming bubble, the same way
    /// <c>ChatRuntime.NextBubbleIndex</c> does for a human chat's turn.</summary>
    void PublishLlmTurnStart(IReadOnlyList<ChatMessage> context);

    /// <summary>A chunk of assistant answer text for the bubble the last <see cref="PublishLlmTurnStart"/> opened.</summary>
    void PublishDelta(string chunk);

    /// <summary>A chunk of reasoning/chain-of-thought text for the same bubble.</summary>
    void PublishReasoning(string chunk);

    /// <summary>The fully assembled assistant message for the current bubble.</summary>
    void PublishAssistantMessage(ChatMessage message);

    /// <summary>A generation attempt the repetition guard abandoned mid-stream.</summary>
    void PublishAttempt(GenerationAttempt attempt);

    /// <summary>A tool call is about to run.</summary>
    void PublishToolStarted(ToolCall call);

    /// <summary>A running top-level tool call reported progress.</summary>
    void PublishToolProgress(ToolCall call, ToolProgress progress);

    /// <summary>A tool call finished, outcome included.</summary>
    void PublishToolResult(ToolCall call, ToolResult result);

    /// <summary>The whole provider-reported outcome of one LLM call — token counters included.</summary>
    void PublishLlmTurn(LlmTurnResult turn);

    /// <summary>An ephemeral notice for whoever is watching. Never sent to the model.</summary>
    void PublishNotice(string text);
}
