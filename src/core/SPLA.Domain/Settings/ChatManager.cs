using SPLA.Domain.Models;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Settings;

/// <summary>
/// Where a chat id currently resolves on disk — the distinction correspondence (PLAN_20260902 wave 4)
/// needs and <see cref="ChatManager.LoadChat"/> alone cannot give: that method walks active-then-archived
/// and hands back the same non-null <see cref="ChatSession"/> either way, so a caller that only calls it
/// cannot tell "sleeping" from "gone quiet on purpose" — and the text of a correspondent's notice is
/// different news for each (ADR_20260827-2 §2.4).
/// </summary>
public enum ChatLocation { Active, Archived, Missing }

public class ChatManager
{
    private readonly ResolvedSettings _settings;
    private readonly string _chatsDir;
    private readonly string _archivedDir;
    private readonly string _summariesDir;
    private readonly string _backupsDir;
    private readonly string? _chatImagesDir;

    /// <summary>The project's per-role instance counters — source of <see cref="ChatSession.AsInstance"/>.
    /// Lazy because a project may be listed, deleted or inspected without ever creating a chat, and the
    /// counter file should not appear in <c>.spla/</c> until someone is actually named.</summary>
    private readonly Lazy<RoleInstanceCounters> _instances;

    /// <summary>Role a chat is publicly named under when it has none of its own — the same fallback
    /// <c>Correspond</c> applies, so such a chat is <c>agent_&lt;n&gt;</c> everywhere
    /// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.1).</summary>
    public const string DefaultPublicRole = "agent";

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeConverter(new ChatSessionImageConverter())
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .WithTypeConverter(new ChatSessionImageConverter())
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    public ChatManager(ResolvedSettings settings)
    {
        _settings = settings;

        // Chat history lives in project buckets; where those physically are (workspace .spla/
        // vs global ~/.spla) is the project backend's decision, not ours. The manager still does
        // raw file IO, so it maps each bucket to a host directory — a virtual backend would make
        // history unavailable until a bucket-native rewrite (later phase).
        var project = _settings.Project;
        _chatsDir = project.GetBucket("chats").MapToHostDirectory()
            ?? throw new InvalidOperationException("Chat history needs a disk-backed project backend.");
        _summariesDir = project.GetBucket("summaries").MapToHostDirectory()!;
        _backupsDir = project.GetBucket("backups").MapToHostDirectory()!;
        // Sidecar image attachments (see SPLA.Runtime.ChatImages) — best-effort here since a virtual
        // project backend may not have one; delete-cleanup simply skips it in that case.
        _chatImagesDir = project.GetBucket("chat-images").MapToHostDirectory();

        // Archived chats live in a subfolder of the same bucket. Deliberately a subfolder, not a
        // sibling bucket: ListChats() globs _chatsDir non-recursively, so archived chats are
        // automatically excluded from it without any extra filtering.
        _archivedDir = Path.Combine(_chatsDir, "archived");

        // The counter file sits in the project's runtime area next to the token tally, not in the
        // chats bucket: it outlives every chat in there, including the deleted ones, and putting it
        // among them would invite exactly the "just count the folder" reasoning it exists to refuse.
        var runtimeDir = project.GetBucket(SPLA.Domain.Project.IProjectBackend.RootBucket).MapToHostDirectory()
            ?? Path.Combine(_settings.WorkspacePath, ".spla");
        _instances = new Lazy<RoleInstanceCounters>(() => new RoleInstanceCounters(
            Path.Combine(runtimeDir, RoleInstanceCounters.FileName),
            SalvageInstanceFloor));

        ConfigLoader.TryHideDirectory(Path.GetDirectoryName(_chatsDir)!);
    }

    /// <summary>Highest instance number visible on disk per role, for the one case where the counter
    /// file is unreadable. Only ever a floor: chats that were deleted took their numbers with them and
    /// no scan can see those — which is the whole reason the counter is a file in the first place.</summary>
    private IEnumerable<KeyValuePair<string, int>> SalvageInstanceFloor()
    {
        foreach (var session in ListChatsIn(_chatsDir).Concat(ListChatsIn(_archivedDir)))
            if (session.AsInstance is > 0)
                yield return new KeyValuePair<string, int>(
                    string.IsNullOrWhiteSpace(session.As) ? DefaultPublicRole : session.As!,
                    session.AsInstance.Value);
    }

    /// <summary>
    /// Takes the next public-name ordinal for <paramref name="role"/> (or for <c>agent</c> when the
    /// chat has no role of its own) and persists the counter before returning
    /// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.1).
    /// <para>Public because minting is no longer only a creation-time act: a role-less chat is numbered
    /// the first time it becomes someone's correspondent, and that moment is known to
    /// <c>ChatRuntime</c>, not here. The counter itself stays the project's — one sequence per role,
    /// wherever the request comes from.</para>
    /// </summary>
    public int NextInstanceNumber(string? role) =>
        _instances.Value.Next(string.IsNullOrWhiteSpace(role) ? DefaultPublicRole : role!);

    public string GenerateChatId()
    {
        return DateTime.Now.ToString("yyyy-MM-dd_HHmm") + "-" + Guid.NewGuid().ToString("N").Substring(0, 4);
    }

    private string GetChatFilePath(string id) => Path.Combine(_chatsDir, $"{id}.yaml");
    private string GetArchivedFilePath(string id) => Path.Combine(_archivedDir, $"{id}.yaml");
    public string GetSummaryFilePath(string id) => Path.Combine(_summariesDir, $"{id}.md");

    /// <summary>Finds a chat's yaml wherever it currently lives — active or archived — or null.</summary>
    private string? FindChatFilePath(string id)
    {
        var active = GetChatFilePath(id);
        if (File.Exists(active)) return active;
        var archived = GetArchivedFilePath(id);
        return File.Exists(archived) ? archived : null;
    }

    /// <summary>
    /// Creates an ordinary chat. <paramref name="role"/> sets <c>as:</c> at the moment of creation
    /// (rather than being patched on afterward) so that a caller opening a <see cref="ChatRuntime"/>
    /// immediately on the returned session — <c>agent_correspond</c>'s on-demand correspondent chat,
    /// PLAN_20260902 wave 5б — sees the role from its very first line: <c>ChatRuntime</c> resolves its
    /// role's settings once, in its constructor, so a role stamped on <see cref="ChatSession.As"/>
    /// after that runtime already exists would narrow nothing until the chat was closed and reopened.
    /// Same reasoning <see cref="CreateSpawnedChat"/> already follows for a spawned session's own role.
    /// </summary>
    /// <param name="origin">"cli" for a chat <c>spla chat run</c> created (locally or handed over to a
    /// live instance — see <c>RemoteChatRun</c>), null for one a human opened directly. Distinct from
    /// <see cref="ChatSession.Origin"/>'s "spawned" value; a scripted run is not a spawned sub-agent.</param>
    public ChatSession CreateNewChat(string? title = null, string? role = null, string? origin = null)
    {
        var chat = new ChatSession
        {
            Id = GenerateChatId(),
            Title = title ?? "New Chat",
            Workspace = _settings.WorkspacePath,
            // Live reference into the project's model list (seeded with the default entry).
            ModelId = _settings.DefaultModel?.Id,
            // Per-chat behaviour knobs only — endpoint/model come from the connection.
            Model = new SplaLlmSection
            {
                Temperature = _settings.Temperature,
                ReasoningLevel = _settings.ReasoningLevel
            },
            Agent = new SplaAgentSection
            {
                Mode = _settings.Mode.ToString()
            },
            Origin = origin,
            As = role,
            // A chat that is created BY being addressed gets its public name in the same breath
            // (ADR_20260906 §2.1): a role chat exists because someone asked for that role, so "created"
            // and "addressed" are one moment here.
            //
            // A chat with no role is the opposite case and gets nothing yet — its number is minted
            // lazily, the first time it actually becomes someone's correspondent (ChatRuntime's
            // EnsureAsInstance). Numbering every human chat at creation would spend the `agent`
            // sequence on the dozens of chats that never talk to anyone, and the first real
            // correspondent of the project would introduce himself as `agent_412`. It also makes old
            // and new role-less chats behave identically, since neither is numbered until addressed.
            AsInstance = string.IsNullOrWhiteSpace(role) ? null : _instances.Value.Next(role!)
        };

        SaveChat(chat);
        return chat;
    }

    /// <summary>
    /// Creates a spawned session on disk: same shape as <see cref="CreateNewChat"/>, plus
    /// <c>origin/parent/as</c> and an in-progress <see cref="ChatSessionSpawnInfo"/> (<c>outcome</c>
    /// null). Title is left at the "New Chat" default so <see cref="SaveChat"/>'s own
    /// first-message auto-title applies to it exactly like a human chat's.
    /// See <c>docs/adr/ADR_20260902_core_session-unification.md</c> §2.1.
    /// </summary>
    public ChatSession CreateSpawnedChat(string? parentChatId, string? role, string? skillId, string mode)
    {
        var chat = new ChatSession
        {
            Id = GenerateChatId(),
            Title = "New Chat",
            Workspace = _settings.WorkspacePath,
            ModelId = _settings.DefaultModel?.Id,
            Model = new SplaLlmSection
            {
                Temperature = _settings.Temperature,
                ReasoningLevel = _settings.ReasoningLevel
            },
            Agent = new SplaAgentSection
            {
                Mode = _settings.Mode.ToString()
            },
            Origin = "spawned",
            Parent = parentChatId,
            As = role,
            // No instance number, deliberately. A spawned run has no public address to be reached at:
            // its address is the link to its parent, it never appears in the chat directory, and
            // nobody outside can open a correspondence with it. Minting a number here would burn one
            // out of the role's sequence for a chat no one can ever say the name of (ADR_20260906 §2.2).
            AsInstance = null,
            Spawn = new ChatSessionSpawnInfo
            {
                SkillId = skillId,
                Mode = mode,
                StartedAt = DateTime.UtcNow,
                Outcome = null
            }
        };

        SaveChat(chat);
        return chat;
    }

    /// <summary><c>true</c> for a session <see cref="CreateSpawnedChat"/> made.</summary>
    public static bool IsSpawned(ChatSession session) =>
        string.Equals(session.Origin, "spawned", StringComparison.Ordinal);

    public void SaveChat(ChatSession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        // Auto-generate title from first user message if title is default
        if (session.Title == "New Chat" && session.Messages.Any(m => m.Role == "user"))
        {
            var firstUserMsg = session.Messages.First(m => m.Role == "user").Content;
            session.Title = Regex.Replace(firstUserMsg.Split('\n')[0], @"[^\w\s-]", "").Trim();
            if (session.Title.Length > 30) session.Title = session.Title.Substring(0, 30) + "...";
            if (string.IsNullOrWhiteSpace(session.Title)) session.Title = "Chat";
        }

        var yaml = ToYaml(session);
        WriteAtomic(GetChatFilePath(session.Id), yaml);
    }

    /// <summary>
    /// Writes through a temporary file and renames it into place, so a reader never sees a partial
    /// one.
    ///
    /// <para>A plain <see cref="File.WriteAllText(string,string)"/> truncates first: for as long as
    /// the write takes, anyone reading gets an empty or half-written file. <see cref="ListChats"/>
    /// skips whatever it cannot parse, so the visible symptom is a chat missing from the sidebar for
    /// one refresh — rare, silent, and impossible to reproduce on demand. Turns save while windows
    /// list, so this is an ordinary interleaving rather than an unlucky one.</para>
    ///
    /// <para>The rename is atomic on both Windows and POSIX; a crash mid-write leaves the previous
    /// version intact and a stray <c>.tmp</c>, which nothing reads.</para>
    /// </summary>
    private static void WriteAtomic(string path, string content)
    {
        var temp = path + ".tmp";
        File.WriteAllText(temp, content);
        File.Move(temp, path, overwrite: true);
    }

    /// <summary>Where <paramref name="id"/> currently lives, without loading or parsing it — a plain
    /// file-existence check against both directories, deliberately independent of
    /// <see cref="LoadChat"/>'s active-then-archived fallback. See <see cref="ChatLocation"/>.</summary>
    public ChatLocation Locate(string id)
    {
        if (File.Exists(GetChatFilePath(id))) return ChatLocation.Active;
        if (File.Exists(GetArchivedFilePath(id))) return ChatLocation.Archived;
        return ChatLocation.Missing;
    }

    /// <summary>
    /// Serializes a chat and strips the characters YAML has no way to carry raw: the C0 controls
    /// other than tab/newline. They reach a chat when a tool result carries binary — a gzip blob in
    /// an ssh/docker screen, say — and YamlDotNet writes them into the literal block verbatim, which
    /// makes the file unreadable on the next load ("did not find expected key"). The chat kept
    /// working from memory while every path that re-reads it from disk (fork, duplicate, list) died,
    /// so the damage stayed invisible until someone forked.
    ///
    /// <para>Dropping them loses nothing a conversation needs: a control byte is not text, and the
    /// alternative — an unparsable history file — loses the whole chat.</para>
    /// </summary>
    private static string ToYaml(ChatSession session) => StripControlChars(Serializer.Serialize(session));

    /// <summary>Same cleanup on the way in, so a file already corrupted by an earlier write still
    /// loads (and is written back clean by the next save) instead of being lost.</summary>
    private static string StripControlChars(string text)
    {
        if (!text.Any(IsIllegal)) return text;
        var sb = new System.Text.StringBuilder(text.Length);
        foreach (var c in text)
            if (!IsIllegal(c)) sb.Append(c);
        return sb.ToString();

        static bool IsIllegal(char c) => c < ' ' && c != '\n' && c != '\t' && c != '\r';
    }

    public ChatSession? LoadChat(string id)
    {
        var path = FindChatFilePath(id);
        if (path == null) return null;

        var yaml = StripControlChars(File.ReadAllText(path));
        var session = Deserializer.Deserialize<ChatSession>(yaml);
        if (session != null) MintInstanceOnFirstLoad(session, path);
        return session;
    }

    /// <summary>
    /// Gives a session written before <see cref="ChatSession.AsInstance"/> existed its number, once,
    /// the first time it is loaded — and writes it back immediately, because a number that is not on
    /// disk is a number the next load hands out again to someone else. Struck at first load rather
    /// than at every load, and rather than in a migration pass over the folder: chats are numbered
    /// when they are actually reached, so a project full of years-old chats does not spend its whole
    /// role sequence the first time it is opened.
    ///
    /// <para>Only chats that ran <i>as</i> something are numbered. A plain human chat gets its number
    /// when someone first needs to address it, not retroactively; a spawned run gets none at all
    /// (see <see cref="CreateSpawnedChat"/>).</para>
    ///
    /// <para><b>The price, accepted knowingly.</b> The <c>tool_name</c> already saved on existing
    /// correspondences is <i>not</i> rewritten — trap 11 of <c>PLAN_20260906</c> forbids renaming after
    /// the fact, since that string is what the other side has been reading and quoting. So for a while
    /// an old <c>reply_architect_2</c> may point at a chat whose public name is now
    /// <c>architect_7</c>. The link still works (it is keyed by chat id, not by name); only the label
    /// disagrees, and only until that correspondence ends.</para>
    /// </summary>
    private void MintInstanceOnFirstLoad(ChatSession session, string path)
    {
        if (session.AsInstance is > 0) return;
        if (string.IsNullOrWhiteSpace(session.As)) return;
        if (IsSpawned(session)) return;

        session.AsInstance = _instances.Value.Next(session.As!);
        // Written back to the path it came from, not through SaveChat: that one always writes into the
        // active folder, which would quietly unarchive an archived chat just for being read.
        try { WriteAtomic(path, ToYaml(session)); }
        catch { /* The number is still in the counter, so it is spent, not re-used; the next load
                   simply mints a fresh one. Losing a read to a read-only file is the worse trade. */ }
    }

    /// <summary>Human-visible chats only — a spawned session is not a chat a person opened, and a
    /// batch of spawns must not pollute this list (ADR §2.1: "shown under their parent in the role→chat
    /// tree", not here — see wave 7). Use <see cref="ListSpawnedChats"/> to reach the ones this hides.</summary>
    public List<ChatSession> ListChats() => ListChatsIn(_chatsDir).Where(c => !IsSpawned(c)).ToList();

    /// <summary>Every spawned session on disk, most-recently-updated first — retention's own view,
    /// and the future tree view's (wave 7).</summary>
    public List<ChatSession> ListSpawnedChats() => ListChatsIn(_chatsDir).Where(IsSpawned).ToList();

    /// <summary>Both human-visible and spawned sessions in a single read of the catalog — for callers
    /// that need both categories and want to avoid reading the directory twice. The result is the same
    /// as <see cref="ListChatsIn"/> returns: most-recently-updated first, unsorted by origin. Callers
    /// must filter by <see cref="IsSpawned"/> as needed.</summary>
    public List<ChatSession> ListChatsAndSpawned() => ListChatsIn(_chatsDir);

    /// <summary>Chats moved aside by <see cref="Archive"/> — never mixed into <see cref="ListChats"/>
    /// since they live in a subfolder that its non-recursive glob does not see.</summary>
    public List<ChatSession> ListArchivedChats() => ListChatsIn(_archivedDir);

    private static List<ChatSession> ListChatsIn(string dir)
    {
        var chats = new List<ChatSession>();
        if (!Directory.Exists(dir)) return chats;
        foreach (var file in Directory.GetFiles(dir, "*.yaml"))
        {
            try
            {
                var yaml = StripControlChars(File.ReadAllText(file));
                var session = Deserializer.Deserialize<ChatSession>(yaml);
                if (session != null) chats.Add(session);
            }
            catch { /* Skip malformed files */ }
        }
        return chats.OrderByDescending(c => c.UpdatedAt).ToList();
    }

    /// <summary>Moves a chat's yaml into the <c>archived</c> subfolder. No-op if already there or
    /// missing. A plain atomic <see cref="File.Move"/> — the file is never rewritten.</summary>
    public void Archive(string id)
    {
        var path = GetChatFilePath(id);
        if (!File.Exists(path)) return;
        Directory.CreateDirectory(_archivedDir);
        File.Move(path, GetArchivedFilePath(id), overwrite: true);
    }

    /// <summary>Moves a chat's yaml back out of <c>archived</c>. No-op if it isn't archived.</summary>
    public void Unarchive(string id)
    {
        var path = GetArchivedFilePath(id);
        if (!File.Exists(path)) return;
        File.Move(path, GetChatFilePath(id), overwrite: true);
    }

    /// <summary>
    /// Removes a chat and everything hanging off it, wherever the chat currently lives (active or
    /// archived): the yaml itself, its summary, every backup snapshot (<c>backups/&lt;id&gt;_*.yaml</c>),
    /// and its <c>chat-images/&lt;id&gt;/</c> sidecar folder. Previously this only removed the yaml and
    /// summary, leaving backups and images orphaned forever — a known gap closed here rather than left
    /// for archived chats to inherit too.
    /// </summary>
    public void DeleteChat(string id)
    {
        var path = FindChatFilePath(id);
        if (path != null) File.Delete(path);

        var summaryPath = GetSummaryFilePath(id);
        if (File.Exists(summaryPath)) File.Delete(summaryPath);

        if (Directory.Exists(_backupsDir))
        {
            foreach (var backup in Directory.GetFiles(_backupsDir, $"{id}_*.yaml"))
            {
                try { File.Delete(backup); } catch { /* best-effort cleanup */ }
            }
        }

        if (_chatImagesDir != null)
        {
            var imagesDir = Path.Combine(_chatImagesDir, id);
            if (Directory.Exists(imagesDir))
            {
                try { Directory.Delete(imagesDir, recursive: true); } catch { /* best-effort cleanup */ }
            }
        }
    }

    public void RenameChat(string id, string newTitle)
    {
        var chat = LoadChat(id);
        if (chat != null)
        {
            chat.Title = newTitle;
            SaveChat(chat);
        }
    }

    /// <summary>Duplicates a chat read from disk. Prefer the overload taking a live
    /// <see cref="ChatSession"/> when the chat is already open — see it for why.</summary>
    public ChatSession DuplicateChat(string id, string? overrideModel = null)
        => DuplicateChat(LoadChat(id) ?? throw new Exception($"Chat {id} not found"), overrideModel);

    /// <summary>
    /// Duplicates a chat from the session object itself: the copy is a deep clone
    /// (<see cref="ChatSession.Clone"/>) given a new id, a new public number and its own title.
    ///
    /// <para>Taking the source in rather than an id is the point. An open chat lives in memory, and
    /// making its copy by writing it out and reading it straight back made duplication depend on the
    /// history surviving a YAML round-trip — so a single binary byte in a tool result (a gzip blob
    /// from an ssh screen) killed the fork of a chat that was on screen and working.</para>
    /// </summary>
    public ChatSession DuplicateChat(ChatSession source, string? overrideModel = null)
    {
        var chat = source.Clone();

        chat.Id = GenerateChatId();
        // A copy is a new chat, not a second face of the old one: it needs its own public name, or two
        // chats would answer to `architect_2` and every reply tool pointed at that name would be
        // ambiguous. Same for Fork, which comes through here (ChatRegistry.Fork).
        //
        // A copy of a chat that had no name yet stays nameless: it inherits the original's situation,
        // not the original's number, and will be minted on first address like any other (§2.1).
        chat.AsInstance = chat.AsInstance is > 0 ? NextInstanceNumber(chat.As) : null;
        chat.Title += " (Copy)";
        chat.CreatedAt = DateTime.UtcNow;
        chat.UpdatedAt = DateTime.UtcNow;

        if (overrideModel != null && chat.Model != null)
        {
            chat.Model.Model = overrideModel;
        }

        SaveChat(chat);
        return chat;
    }

    public void SaveSummary(string id, string markdownContent)
    {
        File.WriteAllText(GetSummaryFilePath(id), markdownContent);
    }

    public void SaveBackup(ChatSession session, string reason)
    {
        var safeReason = Regex.Replace(reason, @"[^\w-]+", "_").Trim('_');
        if (string.IsNullOrWhiteSpace(safeReason)) safeReason = "backup";

        var fileName = $"{session.Id}_{DateTime.Now:yyyy-MM-dd_HHmmss}_{safeReason}.yaml";
        var path = Path.Combine(_backupsDir, fileName);
        var yaml = ToYaml(session);
        File.WriteAllText(path, yaml);
    }
}
