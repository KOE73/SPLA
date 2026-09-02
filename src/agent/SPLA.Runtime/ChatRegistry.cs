using System.Collections.Concurrent;
using SPLA.Domain.Interfaces;
namespace SPLA.Runtime;

/// <summary>
/// The server's shared set of open chats. A <see cref="ChatRuntime"/> is created once per chat and
/// shared across every client connection, so the agent that "sits on the project" has one consistent
/// state regardless of how many windows (or machines) are looking at it. Clients are just views.
/// <para>
/// Also this project's <see cref="ISpawnSessionHost"/> — see <c>docs/adr/ADR_20260902_core_session-unification.md</c>
/// §2.1. Injected into <see cref="AgentRuntime.SpawnedRunner"/> by <see cref="AgentRuntimeRegistry.Build"/>
/// right after both objects exist (the constructor order that already separates them there — see
/// <c>AgentRuntime.cs:366</c>'s own comment — is why this is a settable property on the runner rather
/// than a constructor parameter).
/// </para>
/// </summary>
public sealed class ChatRegistry : IDisposable, ISpawnSessionHost
{
    private readonly AgentRuntime _runtime;
    private readonly ConcurrentDictionary<string, ChatRuntime> _open = new();

    public ChatRegistry(AgentRuntime runtime)
    {
        _runtime = runtime;

        // The library's guard against being rebuilt under a running procedure. It has existed
        // unassigned outside tests, which was harmless while the only trigger was a file changing —
        // another save re-reads everything anyway. It stops being harmless now that the source LIST
        // is editable: a person adds a folder while some chat is mid-skill, and without this the
        // rebuild would land on top of that procedure. Any open chat counts, because the fond is
        // shared and the one running it may not be the one you are looking at.
        _runtime.SkillLibrary.IsSkillActive = () => _open.Values.Any(c => c.ActiveSkillId is not null);

        // Attached here rather than by whoever built us, for the reason trap 1 of
        // PLAN_20260902 exists: wiring that asks its callers to run in a particular order gets it
        // from one of them and silently not from the rest. That already happened once —
        // ChatRegistry.RuntimeOpened went unattached under `spla serve` because ServeCommand opened
        // the project before SplaServiceHost.Build. Doing it in the constructor is what makes a
        // spawn from `spla chat`, from a batch run, and from the service behave identically instead
        // of only the service getting a real session.
        _runtime.SpawnedRunner.AttachSessionHost(this);
    }

    /// <summary>The project runtime these chats belong to.</summary>
    public AgentRuntime Runtime => _runtime;

    /// <summary>
    /// Fires exactly once per <see cref="ChatRuntime"/>, the moment it is constructed — never on a
    /// cache hit. This is the one place that can wire something to a chat's whole life rather than to
    /// one turn or one connection: a per-chat progress subscription (live delivery of a background
    /// task's ticks, not just its final result) is the reason this exists.
    /// </summary>
    public event Action<ChatRuntime>? RuntimeOpened;

    /// <summary>
    /// Fires exactly once per <see cref="ChatRuntime"/>, right before it is disposed — the symmetric
    /// counterpart to <see cref="RuntimeOpened"/>. Recorded as a debt at the end of wave A: nothing
    /// could hang its own life on a chat's death, and the wave B pump needs exactly that (it must not
    /// outlive the chat it wakes). Fired before <c>Dispose()</c> so a subscriber's own cleanup can still
    /// touch the runtime (e.g. unsubscribe from <c>Inbox.Enqueued</c>) while it is still valid.
    /// </summary>
    public event Action<ChatRuntime>? RuntimeClosed;

    /// <summary>Opens (or returns the already-open) runtime for an existing chat; null if not found.</summary>
    public ChatRuntime? GetOrOpen(string chatId)
    {
        if (_open.TryGetValue(chatId, out var existing)) return existing;

        var session = _runtime.ChatManager.LoadChat(chatId);
        if (session == null) return null;

        var created = false;
        var runtime = _open.GetOrAdd(chatId, _ => { created = true; return new ChatRuntime(_runtime, session); });
        if (created) RuntimeOpened?.Invoke(runtime);
        return runtime;
    }

    /// <summary>Creates a new chat, opens its runtime, and returns it.</summary>
    public ChatRuntime CreateNew(string? title)
    {
        var session = _runtime.ChatManager.CreateNewChat(title);
        var runtime = new ChatRuntime(_runtime, session);
        _open[session.Id] = runtime;
        RuntimeOpened?.Invoke(runtime);
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
        RuntimeOpened?.Invoke(runtime);
        return runtime;
    }

    /// <summary>Renames a chat on disk and in any open runtime.</summary>
    public void Rename(string chatId, string title)
    {
        _runtime.ChatManager.RenameChat(chatId, title);
        if (_open.TryGetValue(chatId, out var open)) open.Session.Title = title;
    }

    /// <summary>Deletes a chat from disk and closes any open runtime.</summary>
    public void Delete(string chatId)
    {
        _runtime.ChatManager.DeleteChat(chatId);
        // Dropping the runtime out of the dictionary ends nothing that it holds open — that is the
        // whole shape of the observed leak: no chat, and its shell session still running.
        if (_open.TryRemove(chatId, out var closed))
        {
            RuntimeClosed?.Invoke(closed);
            closed.Dispose();
        }
    }

    /// <summary>Archives a chat: closes any open runtime for it (an archived chat must not have a
    /// live one, same as <see cref="Delete"/>), then moves its yaml aside on disk.</summary>
    public void Archive(string chatId)
    {
        if (_open.TryRemove(chatId, out var closed))
        {
            RuntimeClosed?.Invoke(closed);
            closed.Dispose();
        }
        _runtime.ChatManager.Archive(chatId);
    }

    /// <summary>Unarchives a chat: just moves its yaml back — nothing was open to reopen.</summary>
    public void Unarchive(string chatId) => _runtime.ChatManager.Unarchive(chatId);

    /// <summary>
    /// Closes every open chat. Called when the host stops, so that a shutdown does not leave live
    /// child processes behind for the OS to reap — or not.
    /// </summary>
    public void Dispose()
    {
        foreach (var chatId in _open.Keys.ToList())
            if (_open.TryRemove(chatId, out var open))
            {
                RuntimeClosed?.Invoke(open);
                open.Dispose();
            }
    }

    /// <summary>The already-open runtime for a chat, or null — never loads from disk. For callers that
    /// only want to ASK about live state (is a turn running?) and must not resurrect a closed chat to
    /// find out: the chat list projects this for every chat on disk, and loading them all would turn
    /// listing into a full read of the project's history.</summary>
    public ChatRuntime? Peek(string chatId) => _open.TryGetValue(chatId, out var c) ? c : null;

    // ── ISpawnSessionHost ────────────────────────────────────────────────────────────────────────
    // See docs/adr/ADR_20260902_core_session-unification.md §2.1/§2.3. A spawned session never enters
    // _open: SpawnedAgentRunner drives its one turn directly rather than through a ChatRuntime (which
    // is built for the turn-pump/rewind/fork/reconnect machinery a single driven-to-completion run has
    // no use for) — see SpawnedSession's own comment for why that is a deliberate line, not a shortcut.

    /// <summary>Creates the session on disk and returns a driver scoped to the one run it will make.</summary>
    public ISpawnedSession OpenSpawnedSession(string? parentChatId, string? role)
    {
        var chat = _runtime.ChatManager.CreateSpawnedChat(parentChatId, role, skillId: null, mode: "");
        return new SpawnedSession(this, chat);
    }

    /// <summary>
    /// Drops a cached <see cref="ChatRuntime"/> without touching its file — the counterpart a
    /// finished spawned session needs (see <see cref="SpawnedSession.Finish"/>) because
    /// <see cref="ISpawnedSession"/> writes straight through <c>ChatManager</c>, bypassing whatever a
    /// client's earlier <c>chat.open</c>/<c>chat.watch</c> may have cached here while the run was
    /// still going. A no-op when nothing is cached for the id.
    /// </summary>
    internal void EvictCachedRuntime(string chatId)
    {
        if (_open.TryRemove(chatId, out var stale))
        {
            RuntimeClosed?.Invoke(stale);
            stale.Dispose();
        }
    }

    /// <summary>
    /// Trims finished spawned sessions to <paramref name="keep"/> most-recently-updated, never
    /// touching one whose <see cref="SPLA.Domain.Models.ChatSessionSpawnInfo.Outcome"/> is still null
    /// (trap 5: a run in progress is never trimmable). Negative disables trimming; ordering is by
    /// <c>UpdatedAt</c>, which <c>ChatManager.SaveChat</c> stamps on every write, including the one
    /// <see cref="ISpawnedSession.Finish"/> just made.
    /// </summary>
    public void TrimSpawnedRetention(int keep)
    {
        if (keep < 0) return;

        var finished = _runtime.ChatManager.ListSpawnedChats()
            .Where(c => c.Spawn?.Outcome != null)
            .OrderByDescending(c => c.UpdatedAt)
            .ToList();

        foreach (var stale in finished.Skip(keep))
            _runtime.ChatManager.DeleteChat(stale.Id);
    }
}
