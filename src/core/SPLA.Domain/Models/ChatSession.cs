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
    /// This chat's ordinal among all chats the project has ever created for <see cref="As"/> —
    /// handed out once, at creation, by a monotonic per-role counter the project owns
    /// (<c>SPLA.Domain.Settings.RoleInstanceCounters</c>), and thereafter the property of this chat
    /// (<c>docs/adr/ADR_20260906_core_one-address.md</c> §2.1).
    /// <para>Together with the role it forms the chat's <b>public name</b> — <c>architect_2</c> — the
    /// one string that stands for this chat in the chat directory, in <c>agent_correspond</c>, in the
    /// name of the reply tool pointed at it, in the log and on the correspondence graph. That is
    /// precisely what <see cref="Id"/> is not: the raw identifier stays inside the app forever, so
    /// that nothing outward-facing is welded to how sessions happen to be filed (ADR §2.2, §3).</para>
    /// <para>Never re-used and never re-issued: delete or archive the chat and the number goes with
    /// it, because by then it is already written down in other people's sessions. A chat created
    /// without a role still gets a number — under the role <c>agent</c>, matching the fallback
    /// <c>Correspond</c> already applies, so such a chat is publicly <c>agent_&lt;n&gt;</c>.</para>
    /// <para>Null for every session written before this wave; <see cref="Settings.ChatManager"/> mints
    /// one on first load rather than leaving the chat nameless.</para>
    /// </summary>
    [YamlMember(Alias = "as_instance")]
    public int? AsInstance { get; set; }

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

    /// <summary>Cumulative prompt tokens across all messages in this chat, computed at save time.
    /// Null if no message in the history reported usage; a number (including 0) if at least one message
    /// did report it. Carries the lifetime total rather than recomputing it from messages on every
    /// chat list render, which would be wasteful for long histories. Absent/null for every session file
    /// written before this field was added, falling back to message-by-message summation in the projection
    /// (<c>docs/adr/ADR_20260827_core_config-versioning.md</c>: absence of a new key is not an error).</summary>
    [YamlMember(Alias = "prompt_tokens_total")]
    public int? PromptTokensTotal { get; set; }

    /// <summary>Cumulative completion tokens across all messages in this chat, computed at save time.
    /// Null if no message in the history reported usage; a number (including 0) if at least one message
    /// did report it. Carries the lifetime total rather than recomputing it from messages on every
    /// chat list render, which would be wasteful for long histories. Absent/null for every session file
    /// written before this field was added, falling back to message-by-message summation in the projection
    /// (<c>docs/adr/ADR_20260827_core_config-versioning.md</c>: absence of a new key is not an error).</summary>
    [YamlMember(Alias = "completion_tokens_total")]
    public int? CompletionTokensTotal { get; set; }

    /// <summary>
    /// An independent copy of this session, identity included — the caller gives the copy its own
    /// <see cref="Id"/>, <see cref="Title"/> and <see cref="AsInstance"/>.
    ///
    /// <para>This is what lets a chat be duplicated from the live object instead of from its file
    /// (<c>ChatManager.DuplicateChat</c>). The old route wrote the chat out and read it straight back,
    /// which made forking depend on the history surviving a YAML round-trip — one binary byte in a
    /// tool result and the fork of a perfectly healthy on-screen chat failed.</para>
    ///
    /// <para>Deep on purpose: every list and every nested object is re-made, so nothing an edit does
    /// to one chat afterwards can be seen by the other. That is the whole reason this is not
    /// <c>MemberwiseClone</c> alone.</para>
    /// </summary>
    public ChatSession Clone()
    {
        var copy = (ChatSession)MemberwiseClone();
        copy.Model = Model?.Clone();
        copy.Agent = Agent?.Clone();
        copy.Context = Context?.Clone();
        copy.Messages = Messages.Select(m => m.Clone()).ToList();
        copy.Kv = new Dictionary<string, string>(Kv);
        copy.Doubt = Doubt.Select(d => d.Clone()).ToList();
        copy.Correspondences = Correspondences?.Select(c => c.Clone()).ToList();
        copy.Spawn = Spawn?.Clone();
        return copy;
    }
}

/// <summary>Persisted mirror of one <c>SPLA.Runtime.Correspondence</c> — see that class for what each
/// field means; this is only the on-disk shape (plain strings/ints, no enum, matching every other
/// session field's YAML-friendliness).</summary>
public class ChatSessionCorrespondence
{
    [YamlMember(Alias = "role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>Legacy field: the (role, topic) address wave 0 replaced with (role, instance_no) —
    /// see <c>docs/plans/PLAN_20260906_core_chat-directory-and-await.md</c> §3 wave 0. Kept purely so
    /// a session file written before the change still reads: on load, an entry with no
    /// <see cref="Purpose"/> falls back to this as its purpose text. Every entry written from here on
    /// mirrors <see cref="Purpose"/> into this field too, so anything still reading "topic" (the
    /// project-wide correspondence graph's display field) keeps working unchanged.</summary>
    [YamlMember(Alias = "topic")]
    public string Topic { get; set; } = string.Empty;

    /// <summary>Why this correspondence was opened — see <c>Correspondence.Purpose</c>. Absent on a
    /// session written before wave 0, in which case <see cref="Topic"/> is what carried this text.</summary>
    [YamlMember(Alias = "purpose")]
    public string? Purpose { get; set; }

    /// <summary>Which instance of <see cref="Role"/> this is — see <c>Correspondence.InstanceNo</c>.
    /// Absent on a session written before wave 0; <c>ChatRuntime</c>'s restore loop assigns one by
    /// order of appearance in this file when it is missing, per plan §3 wave 0's migration note.</summary>
    [YamlMember(Alias = "instance_no")]
    public int? InstanceNo { get; set; }

    [YamlMember(Alias = "chat_id")]
    public string ChatId { get; set; } = string.Empty;

    /// <summary><c>"self"</c> or <c>"correspondent"</c> — mirrors <c>CorrespondenceInitiator</c>.</summary>
    [YamlMember(Alias = "initiator")]
    public string Initiator { get; set; } = "self";

    /// <summary>Public name of the chat that put these two in touch, when a third one did — see
    /// <c>Correspondence.IntroducedBy</c> for why this is a field of its own rather than a third value
    /// of <see cref="Initiator"/>. Absent for a correspondence either side opened itself, which is
    /// every correspondence written before the introduction operation existed.</summary>
    [YamlMember(Alias = "introduced_by")]
    public string? IntroducedBy { get; set; }

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

    /// <summary>When this correspondence ended, or absent while it is still open — see
    /// <c>Correspondence.EndedAt</c>. An ended entry stays in this list on purpose
    /// (<c>docs/adr/ADR_20260904_core_history-vs-current.md</c> §2.1: archiving is a headstone, not a
    /// deletion), which is why a session file accumulates them and never drops one.</summary>
    [YamlMember(Alias = "ended_at")]
    public DateTimeOffset? EndedAt { get; set; }

    /// <summary><c>"archived"</c> or <c>"deleted"</c>, or absent while open — see
    /// <c>Correspondence.EndedReason</c>.</summary>
    [YamlMember(Alias = "ended_reason")]
    public string? EndedReason { get; set; }

    /// <summary>Independent copy — see <see cref="ChatSession.Clone"/>.</summary>
    public ChatSessionCorrespondence Clone() => (ChatSessionCorrespondence)MemberwiseClone();
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

    /// <summary>Independent copy — see <see cref="ChatSession.Clone"/>.</summary>
    public ChatSessionSpawnInfo Clone() => (ChatSessionSpawnInfo)MemberwiseClone();
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

    /// <summary>Independent copy — see <see cref="ChatSession.Clone"/>.</summary>
    public ChatSessionDoubt Clone() => (ChatSessionDoubt)MemberwiseClone();
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

    /// <summary>Images attached to this message: the sidecar file each one was written to under
    /// <c>.spla/chat-images/&lt;chatId&gt;/</c>, plus the name it was sent under. Only that lives in the
    /// chat YAML — the binary payload never bloats it. Null/empty for text-only messages.</summary>
    [YamlMember(Alias = "images")]
    public List<ChatSessionImage>? Images { get; set; }

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

    /// <summary>Mirrors <see cref="SPLA.Domain.Models.ChatMessage.ScopeMarker"/> across a save/load —
    /// see <c>docs/adr/ADR_20260911-2_agent_agents-md-scopes.md</c> §2.5. Written for every marker
    /// regardless of <see cref="Settings.SplaAgentSection.SaveToolCalls"/>. Null for every ordinary
    /// message.</summary>
    [YamlMember(Alias = "scope_marker")]
    public string? ScopeMarker { get; set; }

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

    /// <summary>Independent copy — see <see cref="ChatSession.Clone"/>. Tool calls and attempts are
    /// re-made rather than shared: a message is rebuilt wholesale on every save, so sharing them would
    /// not corrupt anything today — but a copy that is deep everywhere needs no such argument to stay
    /// true tomorrow.</summary>
    public ChatSessionMessage Clone()
    {
        var copy = (ChatSessionMessage)MemberwiseClone();
        copy.Images = Images?.Select(i => i.Clone()).ToList();
        copy.ToolCalls = ToolCalls == null ? null : new List<ToolCall>(ToolCalls);
        copy.Attempts = Attempts?.Select(a => a.Clone()).ToList();
        return copy;
    }
}

/// <summary>
/// One image on a persisted message: which sidecar file holds it, and what it was called.
/// <para>
/// Written as a bare file name when it has no name of its own and as <c>{file, label}</c> when it
/// does — see <see cref="ChatSessionImageConverter"/>. A chat written before names existed is
/// therefore still exactly the file it was, and stays that way as long as nobody names anything in
/// it: the format grew a shape rather than replacing the one on disk.
/// </para>
/// </summary>
public sealed class ChatSessionImage
{
    public string File { get; set; } = string.Empty;
    public string? Label { get; set; }

    public ChatSessionImage() { }
    public ChatSessionImage(string file, string? label = null) { File = file; Label = label; }

    public ChatSessionImage Clone() => (ChatSessionImage)MemberwiseClone();
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

    /// <summary>The pause taken before the next attempt, in milliseconds; null when none followed.
    /// See <see cref="SPLA.Domain.Llm.GenerationAttempt.Wait"/>.</summary>
    [YamlMember(Alias = "wait_ms")]
    public long? WaitMs { get; set; }

    /// <summary>Whether <see cref="WaitMs"/> is the provider's own figure — see
    /// <see cref="SPLA.Domain.Llm.GenerationAttempt.WaitStated"/>.</summary>
    [YamlMember(Alias = "wait_stated")]
    public bool WaitStated { get; set; }

    /// <summary>Independent copy — see <see cref="ChatSession.Clone"/>.</summary>
    public ChatSessionAttempt Clone() => (ChatSessionAttempt)MemberwiseClone();
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

    /// <summary>Independent copy — see <see cref="ChatSession.Clone"/>.</summary>
    public ChatSessionContext Clone() => new()
    {
        InstructionFiles = new List<string>(InstructionFiles),
        FilesMentioned = new List<string>(FilesMentioned),
        ChangedFiles = new List<string>(ChangedFiles),
        Commands = new List<string>(Commands)
    };
}
