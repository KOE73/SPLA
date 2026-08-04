using System.Collections.Generic;
using System.Text.Json;

namespace SPLA.Service.Contracts;

// ──────────────────────────────────────────────────────────────────────────
//  Shared data shapes
// ──────────────────────────────────────────────────────────────────────────

/// <summary>A tool call as seen on the wire — the function name and its raw JSON arguments.</summary>
public sealed class ToolCallDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// One rendered chat message. Mirrors the agent's domain message but only the fields a client needs
/// to display it — role, the markdown/plain content, optional reasoning, and tool calls.
/// </summary>
public sealed class ChatMessageDto
{
    public string MsgId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Reasoning { get; set; }
    /// <summary>ISO-8601 UTC creation time, or null when unknown (old chat files).</summary>
    public string? CreatedAt { get; set; }
    public List<ToolCallDto>? ToolCalls { get; set; }
    public string? ToolCallId { get; set; }
    public bool IsEphemeral { get; set; }

    /// <summary>URLs of attached images (e.g. /chat-image/&lt;chatId&gt;/&lt;file&gt; on reopen, or data URLs
    /// for a freshly sent message). Null when the message has no images.</summary>
    public List<string>? Images { get; set; }
}

/// <summary>A chat in the list view.</summary>
public sealed class ChatSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>One selectable option in a clarify request.</summary>
public sealed class ClarifyOptionDto
{
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>One model entry a chat can be pointed at, flattened out of the connection tree but
/// keeping its owner so a picker can group by connection. <see cref="Id"/> is the model entry's id —
/// what a chat stores — not the provider's model string, which travels in <see cref="Model"/>.</summary>
public sealed class ConnectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>The provider's own model string, for display next to the name.</summary>
    public string? Model { get; set; }

    /// <summary>Owning connection — the picker's grouping key and its health key.</summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>Owning connection's label. Never the provider name: two connections may share one.</summary>
    public string ConnectionName { get; set; } = string.Empty;

    public string? Provider { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────
//  Client → Server
// ──────────────────────────────────────────────────────────────────────────

/// <summary>First message a client sends; the server replies with <see cref="WelcomePayload"/>.</summary>
public sealed class HelloPayload
{
    public string? ClientName { get; set; }
    public string? ClientVersion { get; set; }
    public string ProtocolVersion { get; set; } = Contracts.ProtocolVersion.Current;
}

public sealed class ChatOpenPayload
{
    public string ChatId { get; set; } = string.Empty;
}

public sealed class ChatNewPayload
{
    public string? Title { get; set; }
}

/// <summary>Rewind a chat to a message: everything after it is discarded. With
/// <see cref="Before"/> the anchor message itself is removed too (used when a user message is
/// pulled back into the composer). The server answers with a fresh chat.opened snapshot.</summary>
public sealed class ChatRewindPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string MsgId { get; set; } = string.Empty;
    public bool Before { get; set; }
}

/// <summary>Fork a chat into a new one, keeping messages up to and including
/// <see cref="MsgId"/> (or the whole chat when null). The server opens the fork.</summary>
public sealed class ChatForkPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string? MsgId { get; set; }
}

public sealed class ChatRenamePayload
{
    public string ChatId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public sealed class ChatDeletePayload
{
    public string ChatId { get; set; } = string.Empty;
}

public sealed class ChatSendPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    /// <summary>Optional attached images as data URLs (data:image/png;base64,…) for vision models.</summary>
    public List<string>? Images { get; set; }
}

/// <summary>Which chat a window has focused. Sent as <see cref="MessageTypes.FocusSet"/> and echoed
/// back to all connections as <see cref="MessageTypes.FocusChanged"/> so tear-off windows can follow.</summary>
public sealed class FocusPayload
{
    public string ChatId { get; set; } = string.Empty;
}

/// <summary>Changes a chat's behaviour: its mode and/or which model entry it runs on. Null fields are left as-is.</summary>
public sealed class ChatSettingsPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string? Mode { get; set; }
    public string? ModelId { get; set; }
}

/// <summary>Answer to a <see cref="PermissionRequestPayload"/>; correlated by envelope RequestId.</summary>
public sealed class PermissionDecisionPayload
{
    /// <summary>One of "allowOnce", "allowRemember", "deny".</summary>
    public string Decision { get; set; } = "deny";
}

/// <summary>Answer to a <see cref="ClarifyRequestPayload"/>; correlated by envelope RequestId.</summary>
public sealed class ClarifyChoicePayload
{
    /// <summary>The chosen option label, or null to let the agent decide.</summary>
    public string? Choice { get; set; }
}

/// <summary>One editable LLM connection (full shape, unlike the id+name <see cref="ConnectionDto"/>
/// used for pickers). Carried both ways: server→client for the editor, client→server on save.</summary>
public sealed class ConnectionEditDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Provider { get; set; }
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }

    /// <summary>Account-management credential (management / admin key). Never used for inference.</summary>
    public string? AdminKey { get; set; }

    public bool SwapModel { get; set; }

    /// <summary>The models selected under this connection.</summary>
    public List<ModelEditDto> Models { get; set; } = new();
}

/// <summary>One editable model entry under a connection. <see cref="Id"/> is ours and globally
/// unique; <see cref="Model"/> is the provider's string sent on the wire.</summary>
public sealed class ModelEditDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Model { get; set; }
    public int? ContextLength { get; set; }
}

/// <summary>Request to hot-swap the loaded model on a connection via the management API (LM Studio).</summary>
public sealed class ConnectionSwapModelRequest
{
    public string Id { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string ModelKey { get; set; } = string.Empty;
}

/// <summary>Result of a model swap — new model name on success, error message on failure.</summary>
public sealed class ConnectionSwapModelResult
{
    public string Id { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Error { get; set; }
}

/// <summary>The full connection list for the editor. <see cref="ConnectionsGet"/> answer and
/// <see cref="ConnectionsSave"/> body; also broadcast as <see cref="MessageTypes.ConnectionsResult"/>.</summary>
public sealed class ConnectionsPayload
{
    public List<ConnectionEditDto> Connections { get; set; } = new();

    /// <summary>False when there is no .spla project to persist into (edits then live only in-memory).</summary>
    public bool CanPersist { get; set; }

    /// <summary>Set when a save was refused; the list then echoes back what is still in effect.
    /// Null on a successful save and on plain reads.</summary>
    public string? Error { get; set; }
}

/// <summary>Asks for the account/model figures behind one model entry — the "i" popup next to the
/// model picker. Identified by model id because that is what a chat holds; the server walks up to the
/// owning connection for the credential-scoped half.</summary>
public sealed class ProviderInfoRequest
{
    public string ModelId { get; set; } = string.Empty;
}

/// <summary>One provider-reported figure. Mirrors the domain's ProviderFact — the generic middle
/// layer, so a counter this build has never heard of still reaches the UI.</summary>
public sealed class ProviderFactDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    /// <summary>counter | money | percent | duration | timestamp | text — drives formatting.</summary>
    public string Kind { get; set; } = "text";
    /// <summary>normal | warn | critical — drives the dot next to the picker.</summary>
    public string Severity { get; set; } = "normal";
    public DateTimeOffset ObservedAt { get; set; }
    public DateTimeOffset? ResetsAt { get; set; }
}

/// <summary>A titled group of facts, with an optional link to the provider's own dashboard.</summary>
public sealed class ProviderFactSectionDto
{
    public string Title { get; set; } = string.Empty;
    public List<ProviderFactDto> Facts { get; set; } = new();
    public string? DeepLink { get; set; }
}

/// <summary>What the "i" popup shows. Sections are ordered connection-first, then model.</summary>
public sealed class ProviderInfoResult
{
    public string ModelId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public string ConnectionName { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public List<ProviderFactSectionDto> Sections { get; set; } = new();
    /// <summary>Set when nothing could be read at all (unknown model id, provider offline).</summary>
    public string? Error { get; set; }
}

/// <summary>Health state for one connection. <see cref="Ok"/> is null when not yet checked.</summary>
public sealed class ConnectionHealthDto
{
    public string? Id { get; set; }
    /// <summary>null = unchecked, true = reachable, false = unreachable.</summary>
    public bool? Ok { get; set; }
    public string? Error { get; set; }
}

/// <summary>Snapshot of health for all configured connections.
/// Broadcast as <see cref="MessageTypes.ConnectionsHealth"/>.</summary>
public sealed class ConnectionsHealthPayload
{
    public List<ConnectionHealthDto> Statuses { get; set; } = new();
}

/// <summary>Request body for connection diagnostics (ping / models / test).</summary>
public sealed class ConnectionDiagRequest
{
    /// <summary>Correlation key — echoed back in the result so the UI can match card to response.</summary>
    public string? Id { get; set; }
    public string? Provider { get; set; }
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
}

public sealed class ConnectionPingResultPayload
{
    public string? Id { get; set; }
    public bool Ok { get; set; }
    public string? Error { get; set; }
}

public sealed class ConnectionModelsResultPayload
{
    public string? Id { get; set; }
    public List<string> Models { get; set; } = new();
    public string? Error { get; set; }
}

public sealed class ConnectionTestResultPayload
{
    public string? Id { get; set; }
    public string? Reply { get; set; }
    public string? Error { get; set; }
}

/// <summary>Editable agent settings: the default mode for new chats and the four permission-effect
/// overrides. <see cref="AgentGet"/> answer / <see cref="AgentSave"/> body / broadcast result.
/// Permission values: "allow" | "ask" | "deny"; null/empty = use the mode's default.</summary>
public sealed class AgentSettingsPayload
{
    public string Mode { get; set; } = string.Empty;
    /// <summary>Available modes (server-provided, for the picker). Ignored on save.</summary>
    public List<string> Modes { get; set; } = new();
    public string? PermRead { get; set; }
    public string? PermWrite { get; set; }
    public string? PermShell { get; set; }
    public string? PermInternet { get; set; }
    /// <summary>Free-text appended to the system prompt — stored in .spla agent: custom_prompt.</summary>
    public string? CustomPrompt { get; set; }
    /// <summary>Anti-repeat guard: stop the turn when the model repeats the same tool call N times.
    /// Stored in .spla agent: loop_guard / loop_guard_repeats. Default off.</summary>
    public bool? LoopGuard { get; set; }
    public int? LoopGuardRepeats { get; set; }
    // UI appearance — stored in .spla ui: section
    public string Theme { get; set; } = "dark";
    public string Density { get; set; } = "norm";
    public List<string> Themes { get; set; } = new();
    public List<string> Densities { get; set; } = new();
    /// <summary>False when there is no .spla project to persist into (server-set; ignored on save).</summary>
    public bool CanPersist { get; set; }
}

/// <summary>Result of registering the .spla file association — answer to <see cref="MessageTypes.SystemRegisterAssociation"/>.</summary>
public sealed class SystemRegisterAssociationResultPayload
{
    public bool Ok { get; set; }
    public string? Message { get; set; }
}

/// <summary>One plugin's editable state. The settings blob is opaque to host and client alike — it
/// travels as a JSON string (<see cref="SettingsJson"/>) and is never interpreted here; the host
/// converts it to/from the YAML-backed mapping in ConfigLoader.</summary>
public sealed class PluginEditDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Version { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// How far this plugin's tool set may reach the model: <c>disabled</c>, <c>skill_demand</c>,
    /// <c>agent_demand</c>, <c>enabled</c>. This is the DISCLOSURE decision, distinct from
    /// <see cref="Enabled"/>, which is delivery — whether the assembly is loaded at all.
    /// <para>Absent (null) means no explicit level: the set follows <see cref="Enabled"/>, which is
    /// how every project written before tool sets keeps behaving as it did.</para>
    /// </summary>
    public string? Level { get; set; }

    /// <summary>Effective state name (Enabled / DisabledByUser / DisabledByDependency / LoadError…). Read-only.</summary>
    public string? State { get; set; }
    public string? StateReason { get; set; }
    public string? CustomPrompt { get; set; }
    /// <summary>The plugin's opaque settings blob serialized to JSON (null when none).</summary>
    public string? SettingsJson { get; set; }
    /// <summary>URL of the plugin's prebuilt web settings module (see <c>web_settings_entry</c> in
    /// meta.yaml), or null when the plugin has none — the client falls back to the generic JSON editor.</summary>
    public string? WebSettingsUrl { get; set; }
}

/// <summary>Invokes an ad-hoc action on a plugin's web settings UI (e.g. "Test Connection").
/// <see cref="MessageTypes.PluginAction"/> request body.</summary>
public sealed class PluginActionPayload
{
    public string PluginId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    /// <summary>Action-specific argument, serialized to JSON by the caller; opaque to the host.</summary>
    public string? ValueJson { get; set; }
}

/// <summary>Answer to <see cref="MessageTypes.PluginAction"/>.</summary>
public sealed class PluginActionResultPayload
{
    public bool Ok { get; set; }
    /// <summary>JSON-serialized result on success, or an error message in <see cref="Error"/> on failure.</summary>
    public string? ResultJson { get; set; }
    public string? Error { get; set; }
}

/// <summary>The discovered plugins. <see cref="PluginsGet"/> answer / <see cref="PluginsSave"/> body /
/// broadcast result.</summary>
public sealed class PluginsPayload
{
    public List<PluginEditDto> Plugins { get; set; } = new();
    public bool CanPersist { get; set; }

    /// <summary>True when enable/disable takes effect only on the next service start (plugins load once).</summary>
    public bool RestartToApply { get; set; }
}

/// <summary>
/// One switchable capability — a built-in <c>core.*</c> feature or a skill. Plugins keep their own
/// richer <see cref="PluginEditDto"/> (settings blob, web settings URL), but all three render through
/// the same row in the Capabilities settings group, so the shared shape lives here.
/// </summary>
public sealed class CapabilityDto
{
    public string Id { get; set; } = string.Empty;
    /// <summary>"builtin" or "skill".</summary>
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>Resolved state name — Available / MissingPrerequisites / DisabledByUser /
    /// DisabledByTrust / Superseded for skills, Enabled / DisabledByUser for built-ins. Read-only.</summary>
    public string? State { get; set; }
    public string? StateReason { get; set; }

    /// <summary>Provider id for a skill ("project", "machine", "plugin:network"); null for built-ins.</summary>
    public string? Source { get; set; }
    public string? SourceLabel { get; set; }

    /// <summary>Normalised subject words. Skills only — what the panel's facets filter on, and the
    /// only place a person can see the vocabulary drifting.</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>Unsatisfied requirements, so the panel can name them and offer to enable the owning
    /// plugin instead of just reporting that something is missing.</summary>
    public List<string> MissingTools { get; set; } = new();
    public List<string> MissingFeatures { get; set; } = new();
    public List<string> MissingPlugins { get; set; } = new();

    /// <summary>Ids this capability depends on (built-ins only) — enabling it pulls them in.</summary>
    public List<string> Requires { get; set; } = new();
}

/// <summary>Where the listed skills came from — shown as group headers, and as guidance on where to
/// drop a new skill file.</summary>
public sealed class SkillSourceDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Trust { get; set; } = "Trusted";

    /// <summary>How much of this source reaches the model unasked — OutOfCatalog / Findable /
    /// InCatalog / OnShelf. Read-only here: the level is a deployment decision in <c>.spla</c>, not a
    /// panel toggle, and showing it is how a person understands why a skill is switched on and still
    /// never chosen.</summary>
    public string Level { get; set; } = "OnShelf";

    /// <summary>Filesystem location for folder-backed sources; null for anything else.</summary>
    public string? Path { get; set; }
}

/// <summary>Every known skill with its source and state. <see cref="MessageTypes.SkillsGet"/> answer /
/// <see cref="MessageTypes.SkillsSave"/> body / broadcast result.</summary>
public sealed class SkillsPayload
{
    public List<CapabilityDto> Skills { get; set; } = new();
    public List<SkillSourceDto> Sources { get; set; } = new();
    public bool CanPersist { get; set; }
}

/// <summary>The built-in <c>core.*</c> capabilities. <see cref="MessageTypes.FeaturesGet"/> answer /
/// <see cref="MessageTypes.FeaturesSave"/> body / broadcast result.</summary>
public sealed class FeaturesPayload
{
    public List<CapabilityDto> Features { get; set; } = new();
    public bool CanPersist { get; set; }

    /// <summary>Always true: feature tools are registered once at startup, so a change to the set
    /// takes effect on the next service start. Skills, by contrast, apply immediately.</summary>
    public bool RestartToApply { get; set; } = true;
}

/// <summary>Asks the server for a debug snapshot. <see cref="Kind"/> selects which inspector view.</summary>
public sealed class DebugRequestPayload
{
    /// <summary>One of <see cref="DebugKinds"/>.</summary>
    public string Kind { get; set; } = string.Empty;
}

public static class DebugKinds
{
    public const string KvSession = "kv.session";
    public const string KvProject = "kv.project";
    public const string Blobs = "blobs";
    public const string LastContext = "context.last";
    public const string Prompt = "prompt";
}

/// <summary>One known project as the picker/tree sees it — enough to list and choose, no store opened.</summary>
public sealed class ProjectDescriptorDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? ManifestPath { get; set; }
    public string? LastOpened { get; set; }
}

/// <summary>Opens a known project by id (<see cref="MessageTypes.ProjectOpen"/>).</summary>
public sealed class ProjectOpenPayload
{
    public string ProjectId { get; set; } = string.Empty;
}

/// <summary>Creates a new project from a manifest path (<see cref="MessageTypes.ProjectCreate"/>).</summary>
public sealed class ProjectCreatePayload
{
    public string ManifestPath { get; set; } = string.Empty;
    public string? Name { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────
//  Server → Client
// ──────────────────────────────────────────────────────────────────────────

/// <summary>Answer to <see cref="MessageTypes.ProjectList"/>/<see cref="MessageTypes.ProjectRecent"/>.</summary>
public sealed class ProjectListResultPayload
{
    public List<ProjectDescriptorDto> Projects { get; set; } = new();
}

/// <summary>
/// Full context for one project — the same fields <see cref="WelcomePayload"/> carries for the
/// connection's default project, keyed explicitly by <see cref="ProjectId"/> so a client juggling
/// several open projects over one socket can tell them apart. Answer to
/// <see cref="MessageTypes.ProjectOpen"/>/<see cref="MessageTypes.ProjectCreate"/>.
/// </summary>
public sealed class ProjectContextPayload
{
    public string ProjectId { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string? WorkspacePath { get; set; }
    public List<ConnectionDto> Connections { get; set; } = new();
    public string[] Modes { get; set; } = System.Array.Empty<string>();
    public string DefaultMode { get; set; } = string.Empty;
    public string Theme { get; set; } = "dark";
    public string Density { get; set; } = "norm";
}

public sealed class WelcomePayload
{
    public string ProtocolVersion { get; set; } = Contracts.ProtocolVersion.Current;
    public string ActorId { get; set; } = string.Empty;
    public string[] Capabilities { get; set; } = System.Array.Empty<string>();
    public string ServerName { get; set; } = "SPLA";

    /// <summary>The authenticated user (Negotiate/NTLM). Empty when the server runs without auth
    /// (loopback/embedded). UserKey is the stable SID; UserName is the human DOMAIN\user for display.</summary>
    public string UserKey { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    /// <summary>The connection's default project id — omit ProjectId on later messages to mean this one.</summary>
    public string ProjectId { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string? WorkspacePath { get; set; }

    /// <summary>Project connections a chat can point at, and the available agent modes — so a client
    /// can offer per-chat model/mode pickers without hardcoding them.</summary>
    public List<ConnectionDto> Connections { get; set; } = new();
    public string[] Modes { get; set; } = System.Array.Empty<string>();
    public string DefaultMode { get; set; } = string.Empty;

    /// <summary>UI appearance resolved from the project file — the client applies these on first
    /// connect so per-project themes load immediately without a separate get/result round-trip.</summary>
    public string Theme { get; set; } = "dark";
    public string Density { get; set; } = "norm";
}

public sealed class ChatListResultPayload
{
    public List<ChatSummaryDto> Chats { get; set; } = new();
}

/// <summary>The project's UI appearance after a change — broadcast as <see cref="MessageTypes.AppearanceChanged"/>.
/// Every window applies these; the native shell picks them up via the webview bridge.</summary>
public sealed class AppearanceChangedPayload
{
    public string Theme { get; set; } = "dark";
    public string Density { get; set; } = "norm";
}

/// <summary>Full state of a chat the client just opened (or created): its existing messages + settings.</summary>
public sealed class ChatOpenedPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<ChatMessageDto> Messages { get; set; } = new();
    public string Mode { get; set; } = string.Empty;
    public string? ModelId { get; set; }

    /// <summary>The skill running in this chat, or null. Sent on open so a window attaching to a chat
    /// that was left mid-skill can offer the way out immediately.</summary>
    public string? ActiveSkillId { get; set; }

    /// <summary>Tool sets this chat can see, raised or merely announced. Same reason as the active
    /// skill: a window attaching mid-conversation has to show what the model can currently reach.</summary>
    public List<ToolSetStateDto> ToolSets { get; set; } = new();
}

public sealed class DeltaPayload
{
    public int MsgIndex { get; set; }
    public string Text { get; set; } = string.Empty;
}

public sealed class ReasoningPayload
{
    public int MsgIndex { get; set; }
    public string Text { get; set; } = string.Empty;
}

/// <summary>Server event for the user message that started the current turn. <see cref="Text"/> lets
/// clients render server-initiated turns; ordinary client-initiated turns use it only as a fallback
/// because their optimistic local echo is already visible.</summary>
public sealed class UserMessagePayload
{
    public string MsgId { get; set; } = string.Empty;
    public string? CreatedAt { get; set; }
    public string? Text { get; set; }
}

public sealed class AssistantMessagePayload
{
    public int MsgIndex { get; set; }
    public ChatMessageDto Message { get; set; } = new();
}

public sealed class ToolStartedPayload
{
    public ToolCallDto ToolCall { get; set; } = new();
}

public sealed class ToolProgressPayload
{
    public string ToolCallId { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public long Current { get; set; }
    public long Total { get; set; }
    public double? Fraction { get; set; }
    public string? Message { get; set; }
    public List<ToolProgressDetailDto>? Details { get; set; }
}

public sealed class ToolProgressDetailDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class ToolResultPayload
{
    public string ToolCallId { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}

public sealed class NoticePayload
{
    public string Text { get; set; } = string.Empty;
}

public sealed class TokenUsagePayload
{
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }

    /// <summary>The model's operative context window in tokens, when known (connection override or
    /// provider-detected). Lets the client render prompt usage as a fraction of the window and warn
    /// before the provider rejects the request. Null = unknown.</summary>
    public int? ContextLength { get; set; }
}

/// <summary>One usage scope's running totals (session/project/machine), shaped for direct display.</summary>
public sealed class TokenUsageScopePayload
{
    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public long Turns { get; set; }
    public long TotalTokens { get; set; }
}

/// <summary>Answer to <see cref="Protocol.MessageTypes.UsageGet"/> — session (this process), project
/// (.spla/token-usage.json) and machine-global (~/.spla/token-usage.json) totals.</summary>
public sealed class UsageResultPayload
{
    public TokenUsageScopePayload Session { get; set; } = new();
    public TokenUsageScopePayload Project { get; set; } = new();
    public TokenUsageScopePayload Machine { get; set; } = new();
}

/// <summary>Sent when a chat's turn finishes (the agent loop returned), so the client can re-enable input.</summary>
public sealed class TurnCompletePayload
{
    public bool Cancelled { get; set; }
    public string? Error { get; set; }

    /// <summary>The skill still running once the turn ended, or null.
    /// <para>End of turn is exactly when a stuck skill becomes visible and actionable: the model was
    /// supposed to call <c>skill_deactivate</c> as its final step, and if it did not, the chat is now
    /// idle with a skill still pinned — index suppressed, a second activation refused. Reporting it
    /// here means the client can offer the unload control at the moment it is needed, with no event
    /// subscription to keep alive.</para></summary>
    public string? ActiveSkillId { get; set; }
}

/// <summary>Request to hand a skill to a chat — the person choosing, rather than the model.
/// Answered with <see cref="ChatSkillStatePayload"/>, or an error naming why not.</summary>
public sealed class ChatSkillActivatePayload
{
    public string ChatId { get; set; } = string.Empty;

    /// <summary>Skill id. May name a skill the model was never told about: an out-of-catalog source
    /// is invisible to the model and fully visible to its owner, and that is the point of it.</summary>
    public string SkillId { get; set; } = string.Empty;
}

/// <summary>Request to end the skill running in a chat — the user's way out when the model never
/// called <c>skill_deactivate</c>. Answered with <see cref="ChatSkillStatePayload"/>.</summary>
public sealed class ChatSkillDeactivatePayload
{
    public string ChatId { get; set; } = string.Empty;
}

/// <summary>A chat's active-skill state, broadcast to the chat's watchers after it changes so every
/// window on that chat agrees.</summary>
public sealed class ChatSkillStatePayload
{
    public string ChatId { get; set; } = string.Empty;
    public string? ActiveSkillId { get; set; }
}

/// <summary>
/// One tool set as a chat currently sees it. Sets the user levelled off are never sent — for this
/// chat they do not exist.
/// <para>Both halves travel together on purpose: <see cref="Level"/> is the standing permission and
/// <see cref="By"/> is who raised it here (empty when it is merely announced). The status bar weights
/// them by what they actually cost in context — a fully disclosed set carries all its definitions,
/// an announced one costs a single line.</para>
/// </summary>
public sealed class ToolSetStateDto
{
    public string SetId { get; set; } = string.Empty;

    /// <summary>"skill", "agent", "user" — or empty when the set is announced but not raised.</summary>
    public string By { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Level { get; set; } = string.Empty;

    /// <summary>True when the set's tools are in this chat's context right now.</summary>
    public bool Disclosed { get; set; }
}

/// <summary>Request to lower a tool set raised in a chat — the person's control, the reason
/// <c>toolset_deactivate</c> can stay a permission for the model rather than a duty.
/// Answered with <see cref="ChatToolSetStatePayload"/>.</summary>
public sealed class ChatToolSetDeactivatePayload
{
    public string ChatId { get; set; } = string.Empty;
    public string SetId { get; set; } = string.Empty;
}

/// <summary>A chat's tool sets, broadcast to its watchers after any change.</summary>
public sealed class ChatToolSetStatePayload
{
    public string ChatId { get; set; } = string.Empty;
    public List<ToolSetStateDto> Sets { get; set; } = new();
}

public sealed class PermissionRequestPayload
{
    public string ToolName { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

public sealed class ClarifyRequestPayload
{
    public string Question { get; set; } = string.Empty;
    public List<ClarifyOptionDto> Options { get; set; } = new();
}

/// <summary>
/// A debug snapshot the server produced in response to <see cref="DebugRequestPayload"/>. The shape
/// of <see cref="Entries"/>/<see cref="Text"/> depends on <see cref="Kind"/> — KV kinds fill Entries,
/// the prompt kind fills Text/Segments. Keeps debug a read-only protocol concern, available to any
/// client, not just the native debug windows.
/// </summary>
public sealed class DebugSnapshotPayload
{
    public string Kind { get; set; } = string.Empty;
    public List<DebugKvEntryDto>? Entries { get; set; }
    public List<DebugSegmentDto>? Segments { get; set; }
    public string? Text { get; set; }
    /// <summary>Structured context lines for the context.last view.</summary>
    public List<ContextLineDto>? ContextLines { get; set; }
    public int ContextCount { get; set; }
    public int TotalCount { get; set; }
    public int ApproxTokens { get; set; }
    /// <summary>True when ContextLines reflect an actual captured LLM request; false when they are a
    /// freshly-computed preview because no turn has run yet in this process.</summary>
    public bool ContextIsLive { get; set; }
}

/// <summary>One row in the context debug view.</summary>
public sealed class ContextLineDto
{
    public int Index { get; set; }
    public string MsgId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public string Full { get; set; } = string.Empty;
    public int ApproxTokens { get; set; }
    /// <summary>True = included in the context sent to LLM; false = exists in history but was not sent.</summary>
    public bool InContext { get; set; }
}

public sealed class DebugKvEntryDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// One contribution in the assembled agent context — a line of the composition manifest plus the
/// text it stands for. <see cref="Contributor"/> answers "who put this here", <see cref="Source"/>
/// answers "which of its pieces", and <see cref="ApproxTokens"/> is a local estimate for attribution
/// only (the authoritative figures come from the provider).
/// </summary>
public sealed class DebugSegmentDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Contributor { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    /// <summary>"prompt" (part of the system message) or "turn" (its own per-turn message).</summary>
    public string Placement { get; set; } = string.Empty;
    public int ApproxTokens { get; set; }
    /// <summary>Why this contribution is missing, when a contributor failed. Null when it is present.</summary>
    public string? Problem { get; set; }
}

public sealed class ErrorPayload
{
    public string Message { get; set; } = string.Empty;
    public string? Detail { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────
//  Schema editor  (schema.get / schema.result)
// ──────────────────────────────────────────────────────────────────────────

/// <summary>Client → server: resolve a named schema pair (data + UI) from the registry.</summary>
public sealed class SchemaGetPayload
{
    /// <summary>Data schema name, e.g. "sql-table@1". The server also resolves the matching UI schema.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>Answer to <see cref="MessageTypes.SchemaGet"/>.</summary>
public sealed class SchemaResultPayload
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Raw JSON of the data schema (JSON Schema 2020-12), or null when not found.</summary>
    public string? DataSchema { get; set; }
    /// <summary>Raw JSON of the UI schema (JSON Forms UISchema), or null when none registered.</summary>
    public string? UiSchema { get; set; }
    public string? Error { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────
//  Workspace filesystem browser  (fs.browse / fs.read / fs.write)
// ──────────────────────────────────────────────────────────────────────────

/// <summary>Client → server: list children of a directory.
/// <see cref="ParentRef"/> null means workspace root.</summary>
public sealed class FsBrowsePayload
{
    public string? ParentRef { get; set; }
}

/// <summary>Client → server: read a text file by its <see cref="Ref"/> (absolute path).</summary>
public sealed class FsReadPayload
{
    public string Ref { get; set; } = string.Empty;
}

/// <summary>Client → server: autosave text back to a file.</summary>
public sealed class FsWritePayload
{
    public string Ref { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

/// <summary>One node in the workspace file tree. <see cref="Ref"/> is an opaque handle
/// (currently an absolute FS path) understood by <see cref="FsReadPayload"/> and
/// <see cref="FsWritePayload"/>. Kind is "folder" | "leaf".</summary>
public sealed class FsNodeDto
{
    public string Ref { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    /// <summary>"folder" | "leaf"</summary>
    public string Kind { get; set; } = string.Empty;
    /// <summary>Content type hint for leaves: "md", "json", "jsonl", "yaml", "sql", "cs", "txt"…</summary>
    public string? ContentType { get; set; }
    public long? SizeBytes { get; set; }
    /// <summary>ISO-8601 last-write timestamp, or null when unknown.</summary>
    public string? Modified { get; set; }
}

/// <summary>Answer to <see cref="MessageTypes.FsBrowse"/>.</summary>
public sealed class FsBrowseResultPayload
{
    public List<FsNodeDto> Nodes { get; set; } = new();
}

/// <summary>Answer to <see cref="MessageTypes.FsRead"/>.</summary>
public sealed class FsReadResultPayload
{
    public string Ref { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? Error { get; set; }
}

/// <summary>Answer to <see cref="MessageTypes.FsWrite"/>.</summary>
public sealed class FsWriteResultPayload
{
    public string Ref { get; set; } = string.Empty;
    public bool Ok { get; set; }
    public string? Error { get; set; }
}

// ── Live SSH terminal (phase B) ──────────────────────────────────────────────
// A raw xterm terminal bridged to an SSH pty. terminalId scopes every message to one terminal so a
// connection can hold several. Payloads never carry credentials — the host is a configured name; the
// password is resolved server-side from the secret store, exactly as for the SSH tools.

/// <summary><see cref="MessageTypes.TerminalOpen"/> — open a terminal to a configured host.</summary>
public sealed class TerminalOpenPayload
{
    public string TerminalId { get; set; } = string.Empty;
    /// <summary>Configured host name; null uses the SSH plugin's default_host.</summary>
    public string? Host { get; set; }
    /// <summary>Existing session id (host#N) to ATTACH to instead of opening a new session —
    /// the "watch the agent's session / reattach" path. Wins over <see cref="Host"/>.</summary>
    public string? Session { get; set; }
    public int Cols { get; set; } = 120;
    public int Rows { get; set; } = 30;
}

/// <summary><see cref="MessageTypes.TerminalInput"/> — raw human keystrokes into the pty.</summary>
public sealed class TerminalInputPayload
{
    public string TerminalId { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

/// <summary><see cref="MessageTypes.TerminalResize"/> — terminal geometry changed.</summary>
public sealed class TerminalResizePayload
{
    public string TerminalId { get; set; } = string.Empty;
    public int Cols { get; set; }
    public int Rows { get; set; }
}

/// <summary><see cref="MessageTypes.TerminalClose"/> — close one terminal.</summary>
public sealed class TerminalClosePayload
{
    public string TerminalId { get; set; } = string.Empty;
}

/// <summary><see cref="MessageTypes.TerminalOpened"/> — the terminal is live.</summary>
public sealed class TerminalOpenedPayload
{
    public string TerminalId { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    /// <summary>The live session this terminal is a view of (host#N) — shown in the tab title.</summary>
    public string SessionId { get; set; } = string.Empty;
}

/// <summary><see cref="MessageTypes.TerminalData"/> — a chunk of raw pty output (ANSI included).</summary>
public sealed class TerminalDataPayload
{
    public string TerminalId { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

/// <summary><see cref="MessageTypes.TerminalClosed"/> — the terminal ended.</summary>
public sealed class TerminalClosedPayload
{
    public string TerminalId { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary><see cref="MessageTypes.SshSessionsResult"/> — the SSH picker's snapshot: configured
/// hosts, the agent's live shell sessions (host names), and this connection's open terminals.
/// Names only — never credentials.</summary>
public sealed class SshSessionsResultPayload
{
    public List<SshHostDto> Hosts { get; set; } = [];
    /// <summary>Every live session in the project's hub — agent-opened and human-opened alike.</summary>
    public List<SshSessionDto> Sessions { get; set; } = [];
    /// <summary>Terminals this client connection has open (views over sessions).</summary>
    public List<SshTerminalDto> Terminals { get; set; } = [];
}

public sealed class SshSessionDto
{
    /// <summary>Addressable id, host#N.</summary>
    public string Id { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    /// <summary>"agent" or "human".</summary>
    public string OpenedBy { get; set; } = string.Empty;
    /// <summary>How many terminals are currently attached.</summary>
    public int Viewers { get; set; }
}

/// <summary><see cref="MessageTypes.SshSessionClose"/> — end a live session for everyone.</summary>
public sealed class SshSessionClosePayload
{
    public string SessionId { get; set; } = string.Empty;
}

public sealed class SshHostDto
{
    public string Name { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int Port { get; set; } = 22;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
}

public sealed class SshTerminalDto
{
    public string TerminalId { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

public sealed class PluginPanelOpenPayload
{
    public string PanelId { get; set; } = string.Empty;
    public string PanelType { get; set; } = string.Empty;
    public Dictionary<string, string?> Parameters { get; set; } = [];
}

public sealed class PluginPanelInputPayload
{
    public string PanelId { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public JsonElement Data { get; set; }
}

public sealed class PluginPanelClosePayload
{
    public string PanelId { get; set; } = string.Empty;
}

public sealed class PluginPanelOpenedPayload
{
    public string PanelId { get; set; } = string.Empty;
}

public sealed class PluginPanelEventPayload
{
    public string PanelId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public object? Data { get; set; }
}

// ── Secret store ─────────────────────────────────────────────────────────────
/// <summary><see cref="MessageTypes.SecretSet"/> — store fields of an entry (merged over existing
/// fields; an entry is a named record like user+password or a lone token). Field values travel
/// browser→server only; never echoed back or logged.</summary>
public sealed class SecretSetPayload
{
    public string Key { get; set; } = string.Empty;
    /// <summary>Field name → value. E.g. { "user": "koe", "password": "…" }.</summary>
    public Dictionary<string, string> Fields { get; set; } = [];
    /// <summary>"user", "project" or "shared". <b>Required — there is no default.</b> The client must
    /// state where the secret goes; the server never picks for it, because a store that guesses
    /// eventually writes somewhere the user did not mean and reads back somebody else's value.</summary>
    public string Scope { get; set; } = string.Empty;
}

/// <summary><see cref="MessageTypes.SecretDelete"/> — remove a whole entry, or one field of it when
/// <see cref="Field"/> is set (deleting the last field removes the entry).</summary>
public sealed class SecretDeletePayload
{
    public string Key { get; set; } = string.Empty;
    public string? Field { get; set; }
    /// <summary>"user", "project" or "shared". Required — see <see cref="SecretSetPayload.Scope"/>.</summary>
    public string Scope { get; set; } = string.Empty;
}

/// <summary>One entry in a listing: key + field NAMES, never values. <see cref="Scope"/> travels
/// with every entry so a picker can show WHERE each credential lives instead of leaving the user to
/// guess between two identically named ones.</summary>
public sealed class SecretEntryDto
{
    public string Key { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = [];
    /// <summary>"user", "project" or "shared".</summary>
    public string Scope { get; set; } = string.Empty;
    /// <summary>Full reference to paste into config: <c>secret:&lt;scope&gt;:&lt;key&gt;</c>.</summary>
    public string Reference { get; set; } = string.Empty;
    /// <summary>True when this caller may overwrite or delete the entry (shared scope + ACL).</summary>
    public bool CanManage { get; set; } = true;
}

/// <summary><see cref="MessageTypes.SecretResult"/> — entries per scope (keys + field names, NEVER
/// values). <see cref="ProjectOpen"/> is false when no project is open (project-scope edits are then
/// disabled in the UI). Shared entries the caller may not even see are filtered out server-side.</summary>
public sealed class SecretListResultPayload
{
    public List<SecretEntryDto> User { get; set; } = [];
    public List<SecretEntryDto> Project { get; set; } = [];
    public List<SecretEntryDto> Shared { get; set; } = [];
    public bool ProjectOpen { get; set; }
    public string? Error { get; set; }
}
