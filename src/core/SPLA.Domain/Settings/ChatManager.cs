using SPLA.Domain.Models;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Settings;

public class ChatManager
{
    private readonly ResolvedSettings _settings;
    private readonly string _chatsDir;
    private readonly string _archivedDir;
    private readonly string _summariesDir;
    private readonly string _backupsDir;
    private readonly string? _chatImagesDir;

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
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

        ConfigLoader.TryHideDirectory(Path.GetDirectoryName(_chatsDir)!);
    }

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

    public ChatSession CreateNewChat(string? title = null)
    {
        var chat = new ChatSession
        {
            Id = GenerateChatId(),
            Title = title ?? "New Chat",
            Workspace = _settings.WorkspacePath,
            // Live reference into the project's model list (seeded with the default entry).
            ModelId = _settings.Models.FirstOrDefault()?.Id,
            // Per-chat behaviour knobs only — endpoint/model come from the connection.
            Model = new SplaLlmSection
            {
                Temperature = _settings.Temperature,
                ReasoningLevel = _settings.ReasoningLevel
            },
            Agent = new SplaAgentSection
            {
                Mode = _settings.Mode.ToString()
            }
        };

        SaveChat(chat);
        return chat;
    }

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

        var yaml = Serializer.Serialize(session);
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

    public ChatSession? LoadChat(string id)
    {
        var path = FindChatFilePath(id);
        if (path == null) return null;

        var yaml = File.ReadAllText(path);
        return Deserializer.Deserialize<ChatSession>(yaml);
    }

    public List<ChatSession> ListChats() => ListChatsIn(_chatsDir);

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
                var yaml = File.ReadAllText(file);
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

    public ChatSession DuplicateChat(string id, string? overrideModel = null)
    {
        var chat = LoadChat(id) ?? throw new Exception($"Chat {id} not found");
        
        chat.Id = GenerateChatId();
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
        var yaml = Serializer.Serialize(session);
        File.WriteAllText(path, yaml);
    }
}
