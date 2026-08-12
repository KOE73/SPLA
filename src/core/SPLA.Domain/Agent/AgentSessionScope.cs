using SPLA.Domain.Host;

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

    /// <summary>Tool sets raised in this chat. The level that permits raising them at all is a
    /// project setting; this is what is armed here and now.</summary>
    IToolSetSession ToolSets { get; }

    /// <summary>This chat's queue of images a tool wants injected into the model's context on the
    /// next turn (e.g. a browser screenshot). See <see cref="IPendingImageSink"/>.</summary>
    IPendingImageSink Images { get; }

    /// <summary>The host boundary (files, shell, capability gate) this chat's tools act through.
    /// See <see cref="ISandbox"/>. Local chats share a passthrough sandbox; server chats get a
    /// scoped, sandboxed one — tools never know the difference.</summary>
    ISandbox Sandbox { get; }

    /// <summary>Whether this chat has taken in anything from a source nobody named. One bit, raised
    /// by arrivals from the open web and never lowered by anything automatic — see
    /// <see cref="Security.ChatDoubt"/>.</summary>
    Security.ChatDoubt Doubt { get; }
}

/// <summary>Plain bundle of the per-chat agent dependencies. Used by the UI chat VM and by
/// spawned sub-agents to open an <see cref="AgentSessionScope"/> over an isolated state set.</summary>
public sealed class AgentSession : IAgentSession
{
    public AgentSession(IKeyValueStore sessionKv, MarkManager checkpoint, ISkillSession skills,
        IBlobStore? blobs = null, IPendingImageSink? images = null, ISandbox? sandbox = null,
        IToolSetSession? toolSets = null, Security.ChatDoubt? doubt = null)
    {
        Doubt = doubt ?? new Security.ChatDoubt();
        SessionKv = sessionKv;
        Checkpoint = checkpoint;
        Skills = skills;
        ToolSets = toolSets ?? new ToolSetSession();
        Blobs = blobs ?? new BlobStore();
        Images = images ?? new PendingImageSink();
        Sandbox = sandbox ?? PassthroughSandbox.Default;
    }

    public IKeyValueStore SessionKv { get; }
    public IBlobStore Blobs { get; }
    public MarkManager Checkpoint { get; }
    public ISkillSession Skills { get; }
    public IToolSetSession ToolSets { get; }
    public IPendingImageSink Images { get; }
    public ISandbox Sandbox { get; }
    public Security.ChatDoubt Doubt { get; }
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
