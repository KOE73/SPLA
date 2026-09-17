using System.Collections.Concurrent;
using SPLA.Domain.Interfaces;
namespace SPLA.Runtime;

/// <summary>
/// The server's shared set of open chats. A <see cref="ChatRuntime"/> is created once per chat and
/// shared across every client connection, so the agent that "sits on the project" has one consistent
/// state regardless of how many windows (or machines) are looking at it. Clients are just views.
/// <para>
/// Also this project's <see cref="ISpawnSessionHost"/> — see <c>docs/adr/ADR_20260902_core_session-unification.md</c>
/// §2.1. Attached to <see cref="AgentRuntime.SpawnedRunner"/> in this class's own constructor, so every
/// way of building a registry gets it rather than whichever caller remembered — see the comment there.
/// </para>
/// </summary>
public sealed class ChatRegistry : IDisposable, ISpawnSessionHost
{
    private readonly AgentRuntime _runtime;
    private readonly ConcurrentDictionary<string, ChatRuntime> _open = new();

    /// <summary>Spawned sessions whose one run is still going — ADR_20260910-2 wave 2. A session lives
    /// here from <see cref="OpenSpawnedSession"/> until <see cref="SpawnedSession.Finish"/> calls
    /// <see cref="NotifySpawnedFinished"/>; never touched by <see cref="_open"/> or its eviction, since
    /// a spawned session is never a <see cref="ChatRuntime"/> (see <see cref="SpawnedSession"/>'s own
    /// comment on why that boundary is deliberate). What lets <see cref="GetOrOpen"/> refuse to load
    /// the still-empty file for an id that is live here, and what <see cref="PeekSpawned"/> gives
    /// chat.open/chat.watch/chat.send to attach to the live feed instead.</summary>
    private readonly ConcurrentDictionary<string, SpawnedSession> _spawnedOpen = new();

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

    /// <summary>
    /// Fires exactly once per <see cref="SpawnedSession"/>, from <see cref="OpenSpawnedSession"/> —
    /// the spawned counterpart of <see cref="RuntimeOpened"/> (ADR_20260910-2 §4.8: "ChatRegistry
    /// объявляет «сеанс открыт/закрыт» для обоих видов, включая порождённый"). What lets the service
    /// wire a <c>ChatFeedWireSubscriber</c> onto a spawned session's <see cref="SpawnedSession.Feed"/>
    /// with the same code a human chat's <see cref="RuntimeOpened"/> already uses.</summary>
    public event Action<SpawnedSession>? SpawnedOpened;

    /// <summary>Fires exactly once per <see cref="SpawnedSession"/>, from
    /// <see cref="NotifySpawnedFinished"/> — the symmetric close. See <see cref="SpawnedOpened"/>.</summary>
    public event Action<SpawnedSession>? SpawnedClosed;

    /// <summary>The live session for a spawned chat whose run has not finished yet, or null — never
    /// loads from disk (same contract as <see cref="Peek"/>). What <c>chat.open</c>/<c>chat.watch</c>/
    /// <c>chat.send</c> check before falling through to <see cref="GetOrOpen"/>, so they attach to the
    /// run's own live feed instead of a <see cref="ChatRuntime"/> wrapping the still-empty file.</summary>
    public SpawnedSession? PeekSpawned(string chatId) => _spawnedOpen.TryGetValue(chatId, out var s) ? s : null;

    /// <summary>Opens (or returns the already-open) runtime for an existing chat; null if not found,
    /// archived, OR a spawned session whose run is still going (see <see cref="PeekSpawned"/> — that id
    /// belongs to a live <see cref="SpawnedSession"/>, and loading its file here would hand back a
    /// <see cref="ChatRuntime"/> wrapping an empty conversation, the bug ADR_20260910-2 describes). The
    /// soft-link liveness call correspondence uses (ADR_20260827-2 §2.4): asking wakes a sleeping chat
    /// in the same call — but never an archived one, which must stay closed exactly as
    /// <see cref="Archive"/>'s own comment claims (see KNOWN_ISSUES.md, resolved 2026-09-03: this used
    /// to load an archived chat's file anyway, silently un-archiving it in practice). A caller that
    /// needs to tell "archived" from "missing" apart — e.g. to show a different notice — still wants
    /// <see cref="Locate"/> instead.</summary>
    public ChatRuntime? GetOrOpen(string chatId)
    {
        if (_open.TryGetValue(chatId, out var existing)) return existing;

        if (_spawnedOpen.ContainsKey(chatId)) return null;

        if (_runtime.ChatManager.Locate(chatId) != SPLA.Domain.Settings.ChatLocation.Active) return null;

        var session = _runtime.ChatManager.LoadChat(chatId);
        if (session == null) return null;

        var created = false;
        var runtime = _open.GetOrAdd(chatId, _ => { created = true; return new ChatRuntime(_runtime, session, this); });
        if (created) RuntimeOpened?.Invoke(runtime);
        return runtime;
    }

    /// <summary>Where a chat id currently resolves on disk, without opening or resurrecting anything —
    /// see <see cref="ChatLocation"/>. What lets a correspondence's dead-link check
    /// (<see cref="ChatRuntime.RefreshCorrespondences"/>) tell "archived" from "deleted" apart, which
    /// <see cref="GetOrOpen"/> alone cannot (it would happily load an archived chat's file).</summary>
    public SPLA.Domain.Settings.ChatLocation Locate(string chatId) => _runtime.ChatManager.Locate(chatId);

    /// <summary>Creates a new chat, opens its runtime, and returns it. <paramref name="role"/> — see
    /// <see cref="SPLA.Domain.Settings.ChatManager.CreateNewChat"/> — stamps <c>as:</c> before the
    /// session ever reaches a <see cref="ChatRuntime"/> constructor, so a role passed here narrows
    /// this chat's tool surface and mode from its very first turn (PLAN_20260902 wave 5б).</summary>
    public ChatRuntime CreateNew(string? title, string? role = null, string? origin = null)
    {
        var session = _runtime.ChatManager.CreateNewChat(title, role, origin);
        var runtime = new ChatRuntime(_runtime, session, this);
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
        // Flush the live conversation into the session object the copy is made from, and refuse to
        // fork mid-turn (a half-written history is not something to hand anyone). The save that comes
        // with it is a side effect now, not the mechanism: the copy is taken from memory below.
        if (!source.TrySaveIdle()) return null;

        // From the open session, never from its file. Reading the chat back off disk to copy it made
        // forking a live chat depend on its history surviving a YAML round-trip - see
        // ChatManager.DuplicateChat(ChatSession).
        var copy = _runtime.ChatManager.DuplicateChat(source.Session);
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
            var wanted = copy.Messages.Where(m => m.Images != null)
                .SelectMany(m => m.Images!).Select(i => i.File).ToHashSet();
            foreach (var name in wanted)
            {
                var src = Path.Combine(srcDir, name);
                if (!File.Exists(src)) continue;
                Directory.CreateDirectory(dstDir);
                File.Copy(src, Path.Combine(dstDir, name), overwrite: true);
            }
        }
        catch { /* missing images must not break the fork */ }

        var runtime = new ChatRuntime(_runtime, copy, this);
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

    /// <summary>Reads an archived chat's file as data — history to look at, with no
    /// <see cref="ChatRuntime"/> created and nothing added to <c>_open</c>. Null unless the id is
    /// actually <see cref="ChatLocation.Archived"/>: an active chat has a real runtime and must be
    /// reached through <see cref="GetOrOpen"/>, not read behind its own back.
    ///
    /// <para>The symmetric counterpart of <see cref="Peek"/> — that one answers about a live chat
    /// without touching the disk, this one answers from the disk without waking anything. Together
    /// they are the two ways of asking about a chat you are not opening (PLAN_20260903 stage 1);
    /// <see cref="GetOrOpen"/> refusing an archived id is what makes this necessary rather than
    /// merely tidy.</para></summary>
    public SPLA.Domain.Models.ChatSession? ReadArchived(string chatId)
        => _runtime.ChatManager.Locate(chatId) == SPLA.Domain.Settings.ChatLocation.Archived
            ? _runtime.ChatManager.LoadChat(chatId)
            : null;

    // ── ISpawnSessionHost ────────────────────────────────────────────────────────────────────────
    // See docs/adr/ADR_20260902_core_session-unification.md §2.1/§2.3. A spawned session never enters
    // _open: SpawnedAgentRunner drives its one turn directly rather than through a ChatRuntime (which
    // is built for the turn-pump/rewind/fork/reconnect machinery a single driven-to-completion run has
    // no use for) — see SpawnedSession's own comment for why that is a deliberate line, not a shortcut.

    /// <summary>Creates the session on disk and returns a driver scoped to the one run it will make —
    /// tracked in <see cref="_spawnedOpen"/> for the run's whole life so <see cref="GetOrOpen"/>,
    /// <see cref="PeekSpawned"/> and the service's wire subscriber all agree this id is live (wave 2).</summary>
    public ISpawnedSession OpenSpawnedSession(string? parentChatId, string? role,
        SPLA.Domain.Settings.ResolvedSettings? settings = null)
    {
        var chat = _runtime.ChatManager.CreateSpawnedChat(parentChatId, role, skillId: null, mode: "");
        var session = new SpawnedSession(this, chat, settings);
        _spawnedOpen[chat.Id] = session;
        SpawnedOpened?.Invoke(session);
        return session;
    }

    /// <summary>
    /// Called exactly once by <see cref="SpawnedSession.Finish"/>, right after it has written the
    /// finished session to disk: drops the session out of <see cref="_spawnedOpen"/> (so the NEXT
    /// <c>chat.open</c> loads the just-written file into an ordinary <see cref="ChatRuntime"/> through
    /// <see cref="GetOrOpen"/>, the normal path) and fires <see cref="SpawnedClosed"/> — the signal the
    /// service's <c>ChatFeedWireSubscriber</c> unhooks on. Replaces the old <c>EvictCachedRuntime</c>:
    /// with <see cref="GetOrOpen"/> now refusing a still-live spawned id outright (see its own comment),
    /// nothing can ever cache a stale <see cref="ChatRuntime"/> for one in the first place, so there is
    /// no cache left to evict (PLAN_20260910-2 wave 2's "проверить и убрать").
    /// </summary>
    internal void NotifySpawnedFinished(SpawnedSession session)
    {
        if (_spawnedOpen.TryRemove(session.ChatId, out _))
            SpawnedClosed?.Invoke(session);
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
