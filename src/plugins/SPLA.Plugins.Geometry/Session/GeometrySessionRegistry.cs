using SPLA.Domain.Agent;
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

    public static GeometrySession? TryGet(IAgentSession session)
        => Sessions.TryGetValue(session, out var geometry) ? geometry : null;

    public static bool Remove(IAgentSession session)
    {
        if (!Sessions.TryRemove(session, out var geometry)) return false;
        geometry.Dispose();
        return true;
    }
}
