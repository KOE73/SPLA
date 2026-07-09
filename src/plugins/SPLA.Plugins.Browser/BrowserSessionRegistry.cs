using SPLA.Domain.Agent;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SPLA.Plugins.Browser;

/// <summary>
/// Maps each chat's <see cref="IAgentSession"/> to its own <see cref="BrowserSessionManager"/>.
/// <para>
/// Unlike <c>SqlConnectionRegistry</c> (constructed once in <c>SqlPlugin.Initialize</c> and shared
/// by all chats — fine for a stateless-ish resource like a DB connection pool), a browser is a
/// stateful, exclusive resource: two chats must never share one set of tabs. But <c>ISplaPlugin.
/// Initialize</c> only runs once per process (<c>PluginManager.LoadPlugins</c> is called once by
/// <c>AgentRuntime</c>, not per chat) — so tool instances are themselves shared across every chat.
/// Per-chat isolation therefore has to happen at call time, the same way <see cref="DataChannel"/>
/// (via <see cref="AgentSessionScope.Current"/>) resolves the right chat's blob store. Each tool
/// resolves its session lazily here, keyed by the ambient <see cref="IAgentSession"/> reference
/// (stable and unique for the lifetime of a chat).
/// </para>
/// <para>
/// Known limitation (documented in the plugin plan, Wave 2 item): nothing currently calls
/// <see cref="Remove"/> when a chat is closed, so a chat that never calls <c>browser_stop</c> leaks
/// its browser process until the host process exits.
/// </para>
/// </summary>
internal static class BrowserSessionRegistry
{
    private static readonly ConcurrentDictionary<IAgentSession, BrowserSessionManager> Sessions = new();

    public static BrowserSessionManager GetOrCreate(IAgentSession session)
        => Sessions.GetOrAdd(session, static _ => new BrowserSessionManager());

    public static BrowserSessionManager? TryGet(IAgentSession session)
        => Sessions.TryGetValue(session, out var mgr) ? mgr : null;

    public static async Task<bool> RemoveAsync(IAgentSession session)
    {
        if (!Sessions.TryRemove(session, out var mgr)) return false;
        await mgr.DisposeAsync();
        return true;
    }
}
