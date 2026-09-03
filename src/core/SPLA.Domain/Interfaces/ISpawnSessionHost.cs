using SPLA.Domain.Agent;
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
    /// is null in this wave (wave 3 wires it through <c>agent_spawn</c>).
    /// </summary>
    ISpawnedSession OpenSpawnedSession(string? parentChatId, string? role);

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
}
