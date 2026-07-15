using System.Collections.Concurrent;
namespace SPLA.Runtime;

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

    /// <summary>The project runtime these chats belong to.</summary>
    public AgentRuntime Runtime => _runtime;

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

    /// <summary>Forks a chat into a new one: duplicates it on disk, optionally truncated at the
    /// message with <paramref name="msgId"/> (inclusive), and opens the copy. Null if the source
    /// chat is unknown.</summary>
    public ChatRuntime? Fork(string chatId, string? msgId)
    {
        var source = GetOrOpen(chatId);
        if (source == null) return null;
        // Sync the file the duplicate is made from; refuse to fork mid-turn (half-written history).
        if (!source.TrySaveIdle()) return null;

        var copy = _runtime.ChatManager.DuplicateChat(chatId);
        if (msgId != null)
        {
            var keep = source.PersistedCountUpTo(msgId);
            if (keep >= 0 && keep < copy.Messages.Count)
                copy.Messages.RemoveRange(keep, copy.Messages.Count - keep);
        }
        copy.Title = source.Title + " (fork)";
        _runtime.ChatManager.SaveChat(copy);

        // Sidecar images are stored per chat id — copy the referenced files so pictures survive the fork.
        try
        {
            var srcDir = ChatImages.Dir(_runtime.Settings.Project, chatId);
            var dstDir = ChatImages.Dir(_runtime.Settings.Project, copy.Id);
            var wanted = copy.Messages.Where(m => m.Images != null).SelectMany(m => m.Images!).ToHashSet();
            foreach (var name in wanted)
            {
                var src = Path.Combine(srcDir, name);
                if (!File.Exists(src)) continue;
                Directory.CreateDirectory(dstDir);
                File.Copy(src, Path.Combine(dstDir, name), overwrite: true);
            }
        }
        catch { /* missing images must not break the fork */ }

        var runtime = new ChatRuntime(_runtime, copy);
        _open[copy.Id] = runtime;
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

}
