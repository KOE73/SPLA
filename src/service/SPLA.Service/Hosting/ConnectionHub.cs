using System.Collections.Concurrent;

namespace SPLA.Service;

/// <summary>
/// The set of currently connected clients. Lets the server fan a message out to every window —
/// the basis for "clients are just views on one agent": when one client changes shared state
/// (creates/renames/deletes a chat), every other client is told, so all sidebars stay in sync.
/// </summary>
public sealed class ConnectionHub
{
    private readonly ConcurrentDictionary<ClientConnection, byte> _connections = new();

    public void Add(ClientConnection c) => _connections[c] = 0;
    public void Remove(ClientConnection c) => _connections.TryRemove(c, out _);

    /// <summary>Number of currently connected clients — surfaced as a live gauge on the stats plane.</summary>
    public int Count => _connections.Count;

    /// <summary>Sends one message to every connected client. Failures to a single client are ignored.
    /// Reserved for truly connection-level events (e.g. <see cref="Contracts.MessageTypes.FocusChanged"/>)
    /// that aren't scoped to any one project — most broadcasts should use
    /// <see cref="BroadcastToProjectAsync"/> instead so a client only looking at project A never sees
    /// project B's settings/connections/usage results.</summary>
    public Task BroadcastAsync(string type, object? payload)
        => Task.WhenAll(_connections.Keys.Select(c => c.TrySendAsync(type, payload)));

    /// <summary>Sends a message to every client that has touched <paramref name="projectId"/> (i.e. has
    /// sent at least one message scoped to it) — the project-level analogue of
    /// <see cref="BroadcastToWatchersAsync"/>, so project-scoped state (connections, agent settings,
    /// plugin list, usage, chat list, appearance) never leaks across projects sharing one hub.</summary>
    public Task BroadcastToProjectAsync(string projectId, string type, object? payload)
        => Task.WhenAll(_connections.Keys
            .Where(c => c.IsWatchingProject(projectId))
            .Select(c => c.TrySendAsync(type, payload)));

    /// <summary>Sends a message to every client currently watching <paramref name="chatId"/> (i.e. that
    /// has it open). This is how a turn's live stream reaches all windows on the same chat, not just
    /// the one that sent it — completing the "many windows, one agent" model.</summary>
    public Task BroadcastToWatchersAsync(string chatId, string type, object? payload)
        => Task.WhenAll(_connections.Keys
            .Where(c => c.IsWatching(chatId))
            .Select(c => c.TrySendAsync(type, payload, chatId)));
}
