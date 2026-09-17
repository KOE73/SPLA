using SPLA.Domain.Agent;
using System;
using System.Collections.Concurrent;

namespace SPLA.Plugins.Geometry.Session;

/// <summary>
/// Maps each chat's <see cref="IAgentSession"/> to its own markup session, exactly the way
/// <c>BrowserSessionRegistry</c> does and for the same reason: <c>ISplaPlugin.Initialize</c> runs
/// once per process, so tool instances are shared across every chat and per-chat isolation has to
/// happen at call time, keyed by the ambient session reference.
/// <para>
/// One session per chat. Opening a frame in a chat that already has one <b>replaces</b> it silently —
/// the working unit is a single frame, marked up and handed on, frame after frame (ADR §3.5).
/// </para>
/// <para>
/// Known limitation, the same one the browser plugin carries: nothing calls <see cref="Remove"/>
/// when a chat closes, so a chat's last frame stays in memory until the host process exits.
/// </para>
/// </summary>
internal static class GeometrySessionRegistry
{
    private static readonly ConcurrentDictionary<IAgentSession, GeometrySession> Sessions = new();

    /// <summary>Installs <paramref name="geometry"/> as this chat's session, disposing whatever it
    /// replaces.</summary>
    public static void Set(IAgentSession session, GeometrySession geometry)
    {
        if (Sessions.TryGetValue(session, out var previous) && !ReferenceEquals(previous, geometry))
            previous.Dispose();
        Sessions[session] = geometry;
    }

    /// <summary>Raised with a chat id after a step of the markup loop has been rendered and stored.
    /// The panel is pushed to rather than polling: one event per step of a loop a human drives is
    /// nothing like a stream, and a timer would burn for nothing between steps (plan §2.1).</summary>
    public static event Action<string>? Updated;

    /// <summary>Says that <paramref name="session"/>'s markup moved on. A chat with no id (a bare CLI
    /// entry point) has no panel to tell.</summary>
    public static void NotifyUpdated(IAgentSession session)
    {
        if (session.ChatId is not { Length: > 0 } chatId) return;
        // A viewer that throws is a viewer's problem; the markup loop carries on.
        try { Updated?.Invoke(chatId); } catch { /* ignored */ }
    }

    /// <summary>The markup session of the chat with this id, together with the agent session that
    /// owns it — the panel needs both: the geometry for the text, the agent session for the blob ring
    /// the renders live in. Null when that chat has never opened a frame.</summary>
    public static (IAgentSession Owner, GeometrySession Geometry)? TryGetForChat(string chatId)
    {
        foreach (var (owner, geometry) in Sessions)
            if (string.Equals(owner.ChatId, chatId, StringComparison.Ordinal))
                return (owner, geometry);
        return null;
    }

    public static GeometrySession? TryGet(IAgentSession session)
        => Sessions.TryGetValue(session, out var geometry) ? geometry : null;

    public static bool Remove(IAgentSession session)
    {
        if (!Sessions.TryRemove(session, out var geometry)) return false;
        geometry.Dispose();
        return true;
    }
}
