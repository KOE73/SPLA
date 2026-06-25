namespace SPLA.Domain.Agent;

/// <summary>
/// The per-chat agent state that tools resolve at execution time: working memory, the
/// checkpoint/mark manager, and the active-skill session. Each chat owns its own instance;
/// nothing here is shared between chats.
/// </summary>
public interface IAgentSession
{
    /// <summary>This chat's session-scoped working memory (persisted with the chat).</summary>
    IKeyValueStore SessionKv { get; }

    /// <summary>This chat's transient data-channel store for bulk tool output (not persisted).</summary>
    IBlobStore Blobs { get; }

    /// <summary>This chat's checkpoint/mark manager (bound to this chat's conversation).</summary>
    MarkManager Checkpoint { get; }

    /// <summary>This chat's single active-skill session.</summary>
    ISkillSession Skills { get; }
}

/// <summary>Plain bundle of the three per-chat agent dependencies. Used by the UI chat VM and by
/// spawned sub-agents to open an <see cref="AgentSessionScope"/> over an isolated state set.</summary>
public sealed class AgentSession : IAgentSession
{
    public AgentSession(IKeyValueStore sessionKv, MarkManager checkpoint, ISkillSession skills, IBlobStore? blobs = null)
    {
        SessionKv = sessionKv;
        Checkpoint = checkpoint;
        Skills = skills;
        Blobs = blobs ?? new BlobStore();
    }

    public IKeyValueStore SessionKv { get; }
    public IBlobStore Blobs { get; }
    public MarkManager Checkpoint { get; }
    public ISkillSession Skills { get; }
}

/// <summary>
/// Ambient channel that carries the <see cref="IAgentSession"/> of the chat whose conversation
/// loop is currently running. A chat opens a scope with <see cref="Begin"/> around its run; tools
/// — however deep in the call stack and across async/parallel boundaries — read
/// <see cref="Current"/> to act on the right chat's state. Same <see cref="AsyncLocal{T}"/>
/// approach as <see cref="Tools.ClarifyScope"/> and <see cref="Tools.ProgressScope"/>, so
/// multiple chats can run concurrently in the background without their tool calls colliding.
/// When no scope is open, <see cref="Current"/> is <c>null</c>.
/// </summary>
public static class AgentSessionScope
{
    private static readonly AsyncLocal<IAgentSession?> _current = new();

    /// <summary>The agent session for the current async flow, or <c>null</c> when no scope is open.</summary>
    public static IAgentSession? Current => _current.Value;

    /// <summary>
    /// Routes agent-state access on the current async flow to <paramref name="session"/> until the
    /// returned handle is disposed. Nesting restores the previous session (e.g. a spawned sub-agent).
    /// </summary>
    public static IDisposable Begin(IAgentSession session)
    {
        var previous = _current.Value;
        _current.Value = session;
        return new Restore(previous);
    }

    private sealed class Restore : IDisposable
    {
        private readonly IAgentSession? _previous;
        public Restore(IAgentSession? previous) => _previous = previous;
        public void Dispose() => _current.Value = _previous;
    }
}
