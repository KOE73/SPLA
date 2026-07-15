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

/// <summary>A project connection (endpoint/model bundle) a chat can be pointed at.</summary>
public sealed class ConnectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
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

/// <summary>Changes a chat's behaviour: its mode and/or which connection it uses. Null fields are left as-is.</summary>
public sealed class ChatSettingsPayload
{
    public string ChatId { get; set; } = string.Empty;
    public string? Mode { get; set; }
    public string? ConnectionId { get; set; }
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
    public string? Model { get; set; }
    public bool LockModel { get; set; }
    public bool SwapModel { get; set; }
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
/// travels as a YAML string (<see cref="SettingsYaml"/>) and is never interpreted here.</summary>
public sealed class PluginEditDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Version { get; set; }
    public bool Enabled { get; set; } = true;
    /// <summary>Effective state name (Enabled / DisabledByUser / DisabledByDependency / LoadError…). Read-only.</summary>
    public string? State { get; set; }
    public string? StateReason { get; set; }
    public string? CustomPrompt { get; set; }
    /// <summary>The plugin's opaque settings blob serialized to YAML (null when none).</summary>
    public string? SettingsYaml { get; set; }
    /// <summary>URL of the plugin's prebuilt web settings module (see <c>web_settings_entry</c> in
    /// meta.yaml), or null when the plugin has none — the client falls back to the generic YAML editor.</summary>
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
    public string? ConnectionId { get; set; }
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

/// <summary>Server ack of the user message that started the current turn — carries the MsgId the
/// conversation assigned, so the client's local echo can become a rewind/fork anchor.</summary>
public sealed class UserMessagePayload
{
    public string MsgId { get; set; } = string.Empty;
    public string? CreatedAt { get; set; }
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

public sealed class DebugSegmentDto
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
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
/// fields; an entry is a named record like user+password or a lone token). <see cref="Scope"/> is
/// "project" or "machine". Field values travel browser→server only; never echoed back or logged.</summary>
public sealed class SecretSetPayload
{
    public string Key { get; set; } = string.Empty;
    /// <summary>Field name → value. E.g. { "user": "koe", "password": "…" }.</summary>
    public Dictionary<string, string> Fields { get; set; } = [];
    public string Scope { get; set; } = "machine";
}

/// <summary><see cref="MessageTypes.SecretDelete"/> — remove a whole entry, or one field of it when
/// <see cref="Field"/> is set (deleting the last field removes the entry).</summary>
public sealed class SecretDeletePayload
{
    public string Key { get; set; } = string.Empty;
    public string? Field { get; set; }
    public string Scope { get; set; } = "machine";
}

/// <summary>One entry in a listing: key + field NAMES, never values.</summary>
public sealed class SecretEntryDto
{
    public string Key { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = [];
}

/// <summary><see cref="MessageTypes.SecretResult"/> — entries per scope (keys + field names, NEVER
/// values). <see cref="ProjectOpen"/> is false when no project is open (project-scope edits are then
/// disabled in the UI).</summary>
public sealed class SecretListResultPayload
{
    public List<SecretEntryDto> Machine { get; set; } = [];
    public List<SecretEntryDto> Project { get; set; } = [];
    public bool ProjectOpen { get; set; }
    public string? Error { get; set; }
}
