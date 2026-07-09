using System.Collections.Concurrent;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// The server's shared set of open chats. A <see cref="ChatRuntime"/> is created once per chat and
/// shared across every client connection, so the agent that "sits on the project" has one consistent
/// state regardless of how many windows (or machines) are looking at it. Clients are just views.
/// </summary>
public sealed class ChatRegistry
{
    private readonly AgentRuntime _runtime;
    private readonly ConcurrentDictionary<string, ChatRuntime> _open = new();

    public ChatRegistry(AgentRuntime runtime) => _runtime = runtime;

    /// <summary>Opens (or returns the already-open) runtime for an existing chat; null if not found.</summary>
    public ChatRuntime? GetOrOpen(string chatId)
    {
        if (_open.TryGetValue(chatId, out var existing)) return existing;

        var session = _runtime.ChatManager.LoadChat(chatId);
        if (session == null) return null;

        return _open.GetOrAdd(chatId, _ => new ChatRuntime(_runtime, session));
    }

    /// <summary>Creates a new chat, opens its runtime, and returns it.</summary>
    public ChatRuntime CreateNew(string? title)
    {
        var session = _runtime.ChatManager.CreateNewChat(title);
        var runtime = new ChatRuntime(_runtime, session);
        _open[session.Id] = runtime;
        return runtime;
    }

    /// <summary>Renames a chat on disk and in any open runtime.</summary>
    public void Rename(string chatId, string title)
    {
        _runtime.ChatManager.RenameChat(chatId, title);
        if (_open.TryGetValue(chatId, out var open)) open.Session.Title = title;
    }

    /// <summary>Deletes a chat from disk and drops any open runtime.</summary>
    public void Delete(string chatId)
    {
        _runtime.ChatManager.DeleteChat(chatId);
        _open.TryRemove(chatId, out _);
    }

    /// <summary>All chats on disk, most-recent first, as wire summaries.</summary>
    public List<ChatSummaryDto> List()
        => _runtime.ChatManager.ListChats()
            .Select(c => new ChatSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                UpdatedAt = c.UpdatedAt.ToString("o")
            })
            .ToList();
}
