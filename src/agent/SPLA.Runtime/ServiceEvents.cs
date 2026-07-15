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
