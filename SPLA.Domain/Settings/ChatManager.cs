using SPLA.Domain.Models;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Settings;

public class ChatManager
{
    private readonly ResolvedSettings _settings;
    private readonly string _chatsDir;
    private readonly string _summariesDir;
    private readonly string _backupsDir;

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

        // If there is a project file, store in [workspace]/.spla/chats/
        // Otherwise store in ~/.spla/chats/
        var baseDir = _settings.ProjectName != null 
            ? Path.Combine(_settings.WorkspacePath, ".spla")
            : ConfigLoader.GetDefaultsDir();

        _chatsDir = Path.Combine(baseDir, "chats");
        _summariesDir = Path.Combine(baseDir, "summaries");
        _backupsDir = Path.Combine(baseDir, "backups");

        Directory.CreateDirectory(_chatsDir);
        Directory.CreateDirectory(_summariesDir);
        Directory.CreateDirectory(_backupsDir);

        ConfigLoader.TryHideDirectory(baseDir);
    }

    public string GenerateChatId()
    {
        return DateTime.Now.ToString("yyyy-MM-dd_HHmm") + "-" + Guid.NewGuid().ToString("N").Substring(0, 4);
    }

    private string GetChatFilePath(string id) => Path.Combine(_chatsDir, $"{id}.yaml");
    public string GetSummaryFilePath(string id) => Path.Combine(_summariesDir, $"{id}.md");

    public ChatSession CreateNewChat(string? title = null)
    {
        var chat = new ChatSession
        {
            Id = GenerateChatId(),
            Title = title ?? "New Chat",
            Workspace = _settings.WorkspacePath,
            // Live reference into the project's connection list (seeded with the default connection).
            ConnectionId = _settings.Connections.FirstOrDefault()?.Id,
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
        File.WriteAllText(GetChatFilePath(session.Id), yaml);
    }

    public ChatSession? LoadChat(string id)
    {
        var path = GetChatFilePath(id);
        if (!File.Exists(path)) return null;

        var yaml = File.ReadAllText(path);
        return Deserializer.Deserialize<ChatSession>(yaml);
    }

    public List<ChatSession> ListChats()
    {
        var chats = new List<ChatSession>();
        foreach (var file in Directory.GetFiles(_chatsDir, "*.yaml"))
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

    public void DeleteChat(string id)
    {
        var path = GetChatFilePath(id);
        if (File.Exists(path)) File.Delete(path);

        var summaryPath = GetSummaryFilePath(id);
        if (File.Exists(summaryPath)) File.Delete(summaryPath);
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
