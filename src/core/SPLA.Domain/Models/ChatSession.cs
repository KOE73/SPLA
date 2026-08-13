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

    /// <summary>Id of the model entry this chat runs on (live reference into the project's model
    /// list). When the referenced entry is missing, the chat falls back to the first available one.
    /// <para>
    /// Points at the <i>leaf</i>: transport and credentials belong to the connection that owns the
    /// entry, and <c>connection_id</c> is reserved for that node. A chat picks a model, not a wire.
    /// </para></summary>
    [YamlMember(Alias = "model_id")]
    public string? ModelId { get; set; }

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

    /// <summary>
    /// What raised this chat's doubt flag, if anything. Persisted because the flag only goes up and
    /// nothing automatic takes it down — and a mark that a reload clears is a mark anyone can clear
    /// by closing the window.
    /// </summary>
    [YamlMember(Alias = "doubt")]
    public List<ChatSessionDoubt> Doubt { get; set; } = new();
}

/// <summary>One recorded arrival from a source nobody named.</summary>
public class ChatSessionDoubt
{
    /// <summary>Zone name as recorded, e.g. <c>internet</c>.</summary>
    [YamlMember(Alias = "zone")]
    public string Zone { get; set; } = string.Empty;

    /// <summary>What arrived — a URL, a tool name, a handle. Shown to the person deciding whether to
    /// clear the flag, which is the only reason it is kept.</summary>
    [YamlMember(Alias = "what")]
    public string What { get; set; } = string.Empty;

    [YamlMember(Alias = "at")]
    public DateTime At { get; set; } = DateTime.UtcNow;
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
