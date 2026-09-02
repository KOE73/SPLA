using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Interfaces;
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
/// </summary>
internal sealed class SpawnedSession : ISpawnedSession, IBackgroundTaskHost
{
    private readonly ChatRegistry _registry;
    private readonly AgentRuntime _runtime;
    private readonly ChatSession _chat;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SPLA.Domain.Host.ISandbox _sandbox;

    public ProgressHub Progress { get; } = new();
    public ChatInbox Inbox { get; } = new();
    public BackgroundTaskRegistry Tasks { get; }

    public string ChatId => _chat.Id;
    public IAgentSession AgentSession { get; }

    public SpawnedSession(ChatRegistry registry, ChatSession chat)
    {
        _registry = registry;
        _runtime = registry.Runtime;
        _chat = chat;
        Tasks = new BackgroundTaskRegistry(_lifetime.Token);

        // Its own shell, like any chat's — see ChatRuntime's own comment on why this must not be the
        // runtime's shared sandbox: a process a nested spawn starts must not outlive it with nothing
        // able to say otherwise.
        _sandbox = _runtime.Sandbox.ForChat();

        AgentSession = new Domain.Agent.AgentSession(
            new KeyValueStore("session"), new CheckpointManager(), new SkillSession(),
            sandbox: _sandbox, background: this, chatId: chat.Id);
    }

    public void Finish(IReadOnlyList<ChatMessage> conversation, string? skillId, string mode,
        DateTimeOffset startedAt, string outcome, string? error)
    {
        var saveToolCalls = _runtime.Settings.SaveToolCalls;
        var saveAttempts = _runtime.Settings.SaveAttempts;

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
                        DurationMs = (long)a.Duration.TotalMilliseconds
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

        // If nobody ever peeked at this chat while it ran, nothing is cached and this is a no-op. If a
        // client DID call chat.open/chat.watch on it mid-run (its file existed from the moment
        // OpenSpawnedSession created it), ChatRegistry would otherwise keep serving that now-stale
        // ChatRuntime forever — Finish wrote straight to disk, bypassing it entirely. Evicting here is
        // safe: no turn ever ran on that cached instance (SpawnedAgentRunner drives its own orchestrator,
        // never ChatRuntime.SendAsync), so there is nothing live to interrupt, only a stale snapshot to
        // drop. The next chat.send/chat.open reloads the finished file fresh.
        _registry.EvictCachedRuntime(_chat.Id);
    }

    public void Dispose()
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        (_sandbox as IDisposable)?.Dispose();
    }
}
