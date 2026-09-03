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

    /// <summary>
    /// The role this session runs as, or null for a plain human chat. Named <c>as:</c> — deliberately
    /// not <c>role:</c> — because <see cref="ChatSessionMessage.Role"/> a few lines below is a mirror
    /// of the provider protocol's own field (<c>user</c>/<c>assistant</c>/<c>tool</c>) and must not be
    /// confused with it; a session file is read by eye exactly when something has already gone wrong,
    /// and the one moment that reading needs to be unambiguous is the moment two same-named fields
    /// would collide (<c>docs/adr/ADR_20260827-2_core_roles.md</c> §5, trap 4 in
    /// <c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c>).
    /// <para>Declared, read and round-tripped in this wave; nothing writes it yet.</para>
    /// </summary>
    [YamlMember(Alias = "as")]
    public string? As { get; set; }

    /// <summary>
    /// The chat id that spawned this session, or null for one a human opened directly. Together with
    /// <see cref="Origin"/> this is the entire difference between a chat and a spawned run once a role
    /// supplies everything else (<c>docs/adr/ADR_20260902_core_session-unification.md</c> §2.1).
    /// <para>Declared, read and round-tripped in this wave; nothing writes it yet.</para>
    /// </summary>
    [YamlMember(Alias = "parent")]
    public string? Parent { get; set; }

    /// <summary>
    /// <c>"human"</c> or <c>"spawned"</c>; null means human — the historical default, so every session
    /// file written before this field existed still means exactly what it always meant
    /// (<c>docs/adr/ADR_20260827_core_config-versioning.md</c>: absence of a new key is not an error,
    /// it is the first version).
    /// <para>Declared, read and round-tripped in this wave; nothing writes it yet.</para>
    /// </summary>
    [YamlMember(Alias = "origin")]
    public string? Origin { get; set; }

    /// <summary>
    /// Metadata about the one run <c>SpawnedAgentRunner</c> drove on this session, or null for a
    /// session a human opened directly. <see cref="ChatSessionSpawnInfo.Outcome"/> being null is the
    /// authoritative "a run is in progress" signal this wave uses for two things at once: refusing a
    /// human <c>chat.send</c> into it (ADR §2.2) and excluding it from retention trimming (ADR §2.3,
    /// trap 5 — a ring must never evict a session whose run has not finished).
    /// </summary>
    [YamlMember(Alias = "spawn")]
    public ChatSessionSpawnInfo? Spawn { get; set; }

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

    /// <summary>
    /// This chat's live correspondences (<c>SPLA.Runtime.Correspondence</c>), persisted so an exchange
    /// survives a restart instead of dying with the <c>ChatRuntime</c> that held it in memory
    /// (<c>docs/plans/PLAN_20260902_agent_roles-and-correspondence.md</c> "Волна 7б";
    /// <c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.3's "один стор" — this rides alongside
    /// <see cref="As"/>/<see cref="Parent"/>/<see cref="Origin"/> in the same session file rather than
    /// a second store). Null/absent for every session written before this wave, which loads exactly as
    /// it always did — no correspondences, nothing to restore
    /// (<c>docs/adr/ADR_20260827_core_config-versioning.md</c>: absence of a new key is not an error).
    /// <para>The project-wide "who talks to whom" graph (ADR §2.5's last row) is assembled by reading
    /// this field off every session on disk (<c>ChatManager</c>), never from open runtimes — see
    /// <c>SPLA.Domain.Settings.CorrespondenceGraph</c>.</para>
    /// </summary>
    [YamlMember(Alias = "correspondences")]
    public List<ChatSessionCorrespondence>? Correspondences { get; set; }
}

/// <summary>Persisted mirror of one <c>SPLA.Runtime.Correspondence</c> — see that class for what each
/// field means; this is only the on-disk shape (plain strings/ints, no enum, matching every other
/// session field's YAML-friendliness).</summary>
public class ChatSessionCorrespondence
{
    [YamlMember(Alias = "role")]
    public string Role { get; set; } = string.Empty;

    [YamlMember(Alias = "topic")]
    public string Topic { get; set; } = string.Empty;

    [YamlMember(Alias = "chat_id")]
    public string ChatId { get; set; } = string.Empty;

    /// <summary><c>"self"</c> or <c>"correspondent"</c> — mirrors <c>CorrespondenceInitiator</c>.</summary>
    [YamlMember(Alias = "initiator")]
    public string Initiator { get; set; } = "self";

    [YamlMember(Alias = "tool_name")]
    public string ToolName { get; set; } = string.Empty;

    [YamlMember(Alias = "last_reply_at")]
    public DateTimeOffset? LastReplyAt { get; set; }

    /// <summary>Lifetime replies THIS chat has sent through this address — see
    /// <c>Correspondence.Depth</c>'s own remark on why this is never reset, unlike <c>ChatPump</c>'s
    /// unrelated debounce counter of a similar name.</summary>
    [YamlMember(Alias = "depth")]
    public int Depth { get; set; }

    /// <summary>Lifetime estimated token volume of every reply THIS chat has sent through this
    /// address — see <c>Correspondence.VolumeEstimate</c>'s own remark on why this is an honest
    /// estimate of the replies themselves, never a slice of a turn's real usage.</summary>
    [YamlMember(Alias = "volume_estimate")]
    public int VolumeEstimate { get; set; }
}

/// <summary>What <c>subagent.get</c>/<c>subagent.result</c> answer with, persisted on the session
/// itself now that <c>SpawnedRunLog</c> is gone (<c>docs/adr/ADR_20260902_core_session-unification.md</c>
/// §2.1). <see cref="Outcome"/> is null exactly while the run is in progress.</summary>
public class ChatSessionSpawnInfo
{
    [YamlMember(Alias = "skill")]
    public string? SkillId { get; set; }

    [YamlMember(Alias = "mode")]
    public string Mode { get; set; } = string.Empty;

    [YamlMember(Alias = "started_at")]
    public DateTime StartedAt { get; set; }

    [YamlMember(Alias = "finished_at")]
    public DateTime? FinishedAt { get; set; }

    /// <summary>"completed" | "failed" | "cancelled"; null while the run is still going.</summary>
    [YamlMember(Alias = "outcome")]
    public string? Outcome { get; set; }

    [YamlMember(Alias = "error")]
    public string? Error { get; set; }
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

    /// <summary>Tool calls the assistant requested on this message. Only written when the
    /// full tool trace is enabled — see <see cref="Settings.SplaAgentSection.SaveToolCalls"/>.</summary>
    [YamlMember(Alias = "tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }

    /// <summary>For a tool-result message, the id of the <see cref="ToolCall"/> it answers. Only
    /// written when the full tool trace is enabled.</summary>
    [YamlMember(Alias = "tool_call_id")]
    public string? ToolCallId { get; set; }

    /// <summary>Mirrors <see cref="SPLA.Domain.Models.ChatMessage.PeerFrom"/> across a save/load —
    /// the correspondent's role for a reply that arrived across a correspondence, so a reopened chat
    /// still renders it as speech rather than an ordinary human message
    /// (<c>docs/adr/ADR_20260827-2_core_roles.md</c> §2.5). Null for every ordinary message.</summary>
    [YamlMember(Alias = "peer_from")]
    public string? PeerFrom { get; set; }

    /// <summary>Mirrors <see cref="SPLA.Domain.Models.ChatMessage.PromptTokens"/>/<see
    /// cref="SPLA.Domain.Models.ChatMessage.CompletionTokens"/> across a save/load. Set only on an
    /// assistant message whose provider reported usage; null everywhere else (including every message
    /// from a provider that does not expose it — see <see cref="ChatMessage.PromptTokens"/>'s own
    /// remarks on why absence must stay absence rather than becoming a misleading 0). Wave 7's side
    /// panel (PLAN_20260902 "Волна 7") is what first reads this back out — before it, the two fields
    /// existed on the live domain message and were thrown away every time a chat was saved.</summary>
    [YamlMember(Alias = "prompt_tokens")]
    public int? PromptTokens { get; set; }

    [YamlMember(Alias = "completion_tokens")]
    public int? CompletionTokens { get; set; }

    /// <summary>Generations the repetition guard threw away before this message was produced. Only
    /// written when the full attempt trace is enabled — see
    /// <see cref="Settings.SplaAgentSection.SaveAttempts"/>. Null/empty otherwise, including for every
    /// message that never had any (the overwhelmingly common case).</summary>
    [YamlMember(Alias = "attempts")]
    public List<ChatSessionAttempt>? Attempts { get; set; }
}

/// <summary>
/// Persisted shape of one abandoned generation — see <see cref="SPLA.Domain.Llm.GenerationAttempt"/> for the
/// live/domain counterpart this mirrors. Kept as its own type rather than persisting the domain
/// record directly: YAML has no native duration, so <see cref="DurationMs"/> stands in for the
/// domain's <c>TimeSpan</c>, the same substitution the wire's <c>AttemptPayload</c> already makes.
/// </summary>
public class ChatSessionAttempt
{
    [YamlMember(Alias = "index")]
    public int Index { get; set; }

    [YamlMember(Alias = "outcome")]
    public string Outcome { get; set; } = string.Empty;

    /// <summary>The abandoned answer text — the whole point of saving this at all.</summary>
    [YamlMember(Alias = "content")]
    public string? Content { get; set; }

    [YamlMember(Alias = "reasoning")]
    public string? Reasoning { get; set; }

    [YamlMember(Alias = "note")]
    public string? Note { get; set; }

    [YamlMember(Alias = "chars")]
    public int Chars { get; set; }

    [YamlMember(Alias = "duration_ms")]
    public long DurationMs { get; set; }
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
