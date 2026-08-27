namespace SPLA.Runtime;

/// <summary>
/// A domain event raised when some shared, process-wide state changes. Mutators publish these; they
/// do not know who is listening or how the news travels. The host subscribes one handler that maps
/// each event to a wire message and fans it out to every connected client.
/// <para>
/// This is the "say what changed, once" layer: a feature that mutates shared state publishes an event
/// and is done — it never touches the WebSocket dispatch or the connection list. Adding a new client
/// kind (browser, native shell, phone) requires zero changes here.
/// </para>
/// </summary>
public abstract record ServiceEvent;

/// <summary>The project's UI appearance changed. Every view applies it — web chrome and, via the
/// webview bridge, the native shell — regardless of which surface triggered the change.</summary>
public sealed record AppearanceChanged(string Theme, string Density) : ServiceEvent;

/// <summary>
/// The skill fond was rebuilt — a file changed under a branch, the source list was edited, or a
/// trust grant moved. Every open panel re-reads; nobody has to have asked.
///
/// <para><c>SkillLibrary.Reloaded</c> has fired into an empty room since it was written: zero
/// subscribers, so a folder watcher noticing a new skill reached no window at all. This is the wire
/// that was missing, and it matters more now that the list itself can change from the panel.</para>
/// </summary>
public sealed record SkillsChanged : ServiceEvent;

/// <summary>
/// A connected MCP server's status changed — it connected, disconnected, or its tool list changed.
/// Nobody asked for this one, the same way nobody asks for <see cref="SkillsChanged"/>: a background
/// connection thread (<see cref="McpClientManager"/>) mutates <see cref="SPLA.MCP.Core.McpHost"/> and
/// <see cref="SPLA.MCP.Core.ToolSets.ToolSetRegistry"/> on its own schedule, and every open panel
/// should re-read rather than poll.
/// </summary>
public sealed record McpServersChanged : ServiceEvent;

/// <summary>
/// The in-process event hub: components <see cref="Publish"/> domain events, subscribers react.
/// Lives on <see cref="AgentRuntime"/> (process-wide, chat-agnostic). Handlers run synchronously on
/// the publisher's thread and must not block — the broadcast subscriber fires its async sends and
/// returns immediately.
/// </summary>
public sealed class ServiceEvents
{
    private readonly List<Action<ServiceEvent>> _handlers = new();
    private readonly object _gate = new();

    public void Subscribe(Action<ServiceEvent> handler)
    {
        lock (_gate) _handlers.Add(handler);
    }

    public void Publish(ServiceEvent evt)
    {
        Action<ServiceEvent>[] snapshot;
        lock (_gate) snapshot = _handlers.ToArray();
        foreach (var h in snapshot) h(evt);
    }
}
