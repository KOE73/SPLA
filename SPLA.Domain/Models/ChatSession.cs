using SPLA.Domain.Settings;
using YamlDotNet.Serialization;

namespace SPLA.Domain.Models;

public class ChatSession
{
    [YamlMember(Alias = "version")]
    public int Version { get; set; } = 1;

    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "title")]
    public string Title { get; set; } = "New Chat";

    [YamlMember(Alias = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [YamlMember(Alias = "updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [YamlMember(Alias = "workspace")]
    public string Workspace { get; set; } = string.Empty;

    /// <summary>Id of the project connection this chat uses (live reference into the project's
    /// connection list). When the referenced connection is missing, the chat falls back to the
    /// first available one.</summary>
    [YamlMember(Alias = "connection_id")]
    public string? ConnectionId { get; set; }

    /// <summary>Per-chat LLM behaviour knobs layered on the chosen connection: temperature and
    /// reasoning level. Endpoint/model come from the connection, not from here.</summary>
    [YamlMember(Alias = "model")]
    public SplaLlmSection? Model { get; set; }

    [YamlMember(Alias = "agent")]
    public SplaAgentSection? Agent { get; set; }

    [YamlMember(Alias = "messages")]
    public List<ChatSessionMessage> Messages { get; set; } = new();

    [YamlMember(Alias = "context")]
    public ChatSessionContext? Context { get; set; }

    /// <summary>Session-scoped agent working memory (key/value). Persisted with the chat.</summary>
    [YamlMember(Alias = "kv")]
    public Dictionary<string, string> Kv { get; set; } = new();
}

public class ChatSessionMessage
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8);

    [YamlMember(Alias = "role")]
    public string Role { get; set; } = string.Empty;

    [YamlMember(Alias = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [YamlMember(Alias = "content")]
    public string Content { get; set; } = string.Empty;

    [YamlMember(Alias = "reasoning")]
    public string? Reasoning { get; set; }

    /// <summary>Filenames of images attached to this message, stored as sidecar files under
    /// <c>.spla/chat-images/&lt;chatId&gt;/</c>. Only the filenames live in the chat YAML — the binary
    /// payload never bloats it. Null/empty for text-only messages.</summary>
    [YamlMember(Alias = "images")]
    public List<string>? Images { get; set; }
}

public class ChatSessionContext
{
    [YamlMember(Alias = "instruction_files")]
    public List<string> InstructionFiles { get; set; } = new();

    [YamlMember(Alias = "files_mentioned")]
    public List<string> FilesMentioned { get; set; } = new();

    [YamlMember(Alias = "changed_files")]
    public List<string> ChangedFiles { get; set; } = new();

    [YamlMember(Alias = "commands")]
    public List<string> Commands { get; set; } = new();
}
