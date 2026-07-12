using System.Text.Json;

namespace SPLA.Service.Contracts;

/// <summary>
/// Version of the wire protocol. Bumped when the envelope or a payload shape changes in a way
/// older clients cannot tolerate. A client compares this against what the server reports in
/// <see cref="WelcomePayload.ProtocolVersion"/> and may refuse to proceed on a mismatch.
/// </summary>
public static class ProtocolVersion
{
    public const string Current = "1";
}

/// <summary>
/// Every message — both directions — is one of these on the wire. <see cref="Type"/> selects the
/// payload shape; <see cref="Payload"/> carries it as raw JSON so a client only deserializes the
/// shapes it knows. <see cref="Auth"/> rides on every message (not just the handshake) so identity
/// and, later, capability checks can be enforced per-message without a protocol change. For now the
/// server ignores it beyond an optional connect token.
/// </summary>
public sealed class ProtocolEnvelope
{
    /// <summary>One of the <see cref="MessageTypes"/> constants.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Identity/authorization envelope. Reserved: the server is allow-all at this stage.</summary>
    public AuthInfo? Auth { get; set; }

    /// <summary>Which chat this message concerns, when applicable.</summary>
    public string? ChatId { get; set; }

    /// <summary>Which project this message concerns. Null means "this connection's default
    /// project" (the one the service was started against) — a single-project client never has to
    /// set this. A multi-project client (project browser, several open project windows over the
    /// same socket) sets it explicitly on every project- or chat-scoped message; the server never
    /// remembers a "current project" for the connection, so there is no state to drift.</summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Correlation id for request/response pairs that need one — permission and clarify round-trips
    /// carry the same RequestId on the outgoing request and the incoming answer.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>The typed body for <see cref="Type"/>, as raw JSON.</summary>
    public JsonElement? Payload { get; set; }
}

/// <summary>
/// Identity/authorization carried on every message. At this stage only <see cref="Token"/> may be
/// checked, and only when the server is bound to a non-loopback address. <see cref="ActorId"/> and
/// capability negotiation are reserved for later stages — the fields exist now so the protocol does
/// not have to change when auth grows from "allow-all on localhost" to shared-secret to per-actor.
/// </summary>
public sealed class AuthInfo
{
    /// <summary>Shared connect secret. Ignored on loopback; required when bound to a network address (future).</summary>
    public string? Token { get; set; }

    /// <summary>Who the client claims to be. Server-assigned in <see cref="WelcomePayload"/>; reserved.</summary>
    public string? ActorId { get; set; }
}

/// <summary>The set of <see cref="ProtocolEnvelope.Type"/> values, grouped by direction.</summary>
public static class MessageTypes
{
    // ── Client → Server ──────────────────────────────────────────────────
    public const string Hello = "hello";
    public const string ChatList = "chat.list";
    public const string ChatOpen = "chat.open";
    /// <summary>Registers this connection as a watcher of a chat without the side effects of
    /// <see cref="ChatOpen"/> (no chat.opened echo) — used by solo windows (e.g. a tear-off debug
    /// panel) that follow the main window's focus and need turn/tool events but never "open" a chat.</summary>
    public const string ChatWatch = "chat.watch";
    public const string ChatNew = "chat.new";
    public const string ChatRename = "chat.rename";
    public const string ChatDelete = "chat.delete";
    public const string ChatSend = "chat.send";
    public const string ChatSettings = "chat.settings";
    /// <summary>A window tells the service which chat it has focused; the service echoes
    /// <see cref="FocusChanged"/> to every connection so auxiliary windows (e.g. a tear-off debug
    /// panel) can follow the active chat across separate sockets.</summary>
    public const string FocusSet = "focus.set";
    public const string Cancel = "cancel";
    public const string PermissionDecision = "permission.decision";
    public const string ClarifyChoice = "clarify.choice";
    public const string DebugRequest = "debug.request";

    // ── Settings: connections editor (client → server) ───────────────────
    /// <summary>Ask for the editable connection list.</summary>
    public const string ConnectionsGet = "connections.get";
    // ── Connection diagnostics (client → server) ─────────────────────────
    /// <summary>Quick reachability check — GET /models on the endpoint.</summary>
    public const string ConnectionPing = "connection.ping";
    /// <summary>Fetch available model ids from the endpoint's /models API.</summary>
    public const string ConnectionModels = "connection.models";
    /// <summary>Send a short test message and return the reply.</summary>
    public const string ConnectionTest = "connection.test";
    /// <summary>Unload the current model and load a new one via the management API (LM Studio only).</summary>
    public const string ConnectionSwapModel = "connection.swap_model";
    /// <summary>Replace the connection list (persisted to the .spla project when there is one).</summary>
    public const string ConnectionsSave = "connections.save";
    /// <summary>Ask for the editable agent settings (default mode + permission overrides).</summary>
    public const string AgentGet = "agent.get";
    /// <summary>Save agent settings (persisted to the .spla project when there is one).</summary>
    public const string AgentSave = "agent.save";
    /// <summary>Ask for the discovered plugins and their enable/prompt/settings state.</summary>
    public const string PluginsGet = "plugins.get";
    /// <summary>Save plugin enable flags, custom prompts and opaque settings blobs.</summary>
    public const string PluginsSave = "plugins.save";
    /// <summary>Invoke an ad-hoc action on a plugin's web settings UI (e.g. "Test Connection").
    /// Body is <see cref="PluginActionPayload"/>; answer is <see cref="PluginActionResult"/>.</summary>
    public const string PluginAction = "plugin.action";
    /// <summary>Persist UI appearance (theme/density). Auto-sent on change — appearance has no Save step.
    /// Body is <see cref="AppearanceChangedPayload"/>; the server persists and broadcasts <see cref="AppearanceChanged"/>.</summary>
    public const string AppearanceSave = "appearance.save";
    /// <summary>Ask for the current token usage totals (session/project/machine).</summary>
    public const string UsageGet = "usage.get";
    /// <summary>Register the .spla file extension with this app in Windows Explorer (Windows only,
    /// per-user registry, no admin rights needed).</summary>
    public const string SystemRegisterAssociation = "system.register_association";

    // ── Schema editor (client → server) ─────────────────────────────────
    /// <summary>Resolve a named JSON schema (data + UI) from the registry — used by the Forms editor.</summary>
    public const string SchemaGet = "schema.get";

    // ── Project browser (client → server) ─────────────────────────────────
    /// <summary>List every known project (registry), most recently opened first.</summary>
    public const string ProjectList = "project.list";
    /// <summary>Same as <see cref="ProjectList"/>, explicitly ordered by recency (convenience for
    /// a "recent projects" picker that wants the same data as <see cref="ProjectList"/>).</summary>
    public const string ProjectRecent = "project.recent";
    /// <summary>Opens (building the runtime on first touch) a known project by id.</summary>
    public const string ProjectOpen = "project.open";
    /// <summary>Creates a new project (writes its manifest) and opens it.</summary>
    public const string ProjectCreate = "project.create";

    // ── Workspace filesystem browser (client → server) ───────────────────
    /// <summary>List children of a directory (null parentRef = workspace root).</summary>
    public const string FsBrowse = "fs.browse";
    /// <summary>Read the text content of a file by its ref (absolute path).</summary>
    public const string FsRead = "fs.read";
    /// <summary>Write (autosave) text content back to a file.</summary>
    public const string FsWrite = "fs.write";

    // ── Live SSH terminal (client → server) ──────────────────────────────
    // Phase B of the "live console": a raw xterm terminal bridged to an SSH pty. Per-connection,
    // bidirectional — the human types freely (no read-only guard: their own supervised session).
    /// <summary>Open a terminal to a configured SSH host. Body <see cref="TerminalOpenPayload"/>.</summary>
    public const string TerminalOpen = "terminal.open";
    /// <summary>Human keystrokes into the pty. Body <see cref="TerminalInputPayload"/>.</summary>
    public const string TerminalInput = "terminal.input";
    /// <summary>Terminal resized in the browser. Body <see cref="TerminalResizePayload"/>.</summary>
    public const string TerminalResize = "terminal.resize";
    /// <summary>Close a terminal. Body <see cref="TerminalClosePayload"/>.</summary>
    public const string TerminalClose = "terminal.close";

    // ── Server → Client ──────────────────────────────────────────────────
    public const string Welcome = "welcome";
    public const string ChatListResult = "chat.list.result";
    public const string ChatOpened = "chat.opened";
    public const string LlmTurnStart = "llm.turn.start";
    public const string Delta = "delta";
    public const string Reasoning = "reasoning";
    public const string AssistantMessage = "assistant.message";
    public const string ToolStarted = "tool.started";
    public const string ToolProgress = "tool.progress";
    public const string ToolResult = "tool.result";
    public const string Notice = "notice";
    public const string TokenUsage = "token.usage";
    public const string TurnComplete = "turn.complete";
    public const string PermissionRequest = "permission.request";
    public const string ClarifyRequest = "clarify.request";
    public const string DebugSnapshot = "debug.snapshot";
    /// <summary>Broadcast to all connections when a window changes the focused chat (see <see cref="FocusSet"/>).</summary>
    public const string FocusChanged = "focus.changed";
    /// <summary>The current connection list — answer to <see cref="ConnectionsGet"/> and broadcast to all
    /// after <see cref="ConnectionsSave"/> so every window's pickers refresh.</summary>
    public const string ConnectionsResult = "connections.result";
    // ── Connection diagnostics (server → client) ─────────────────────────
    public const string ConnectionPingResult      = "connection.ping.result";
    public const string ConnectionModelsResult    = "connection.models.result";
    public const string ConnectionSwapModelResult = "connection.swap_model.result";
    public const string ConnectionTestResult   = "connection.test.result";
    /// <summary>Health snapshot for all configured connections — broadcast on startup and on
    /// <see cref="ConnectionsGet"/> so every window can show live/dead indicators.</summary>
    public const string ConnectionsHealth = "connections.health";
    /// <summary>The current agent settings — answer to <see cref="AgentGet"/> and broadcast after <see cref="AgentSave"/>.</summary>
    public const string AgentResult = "agent.result";
    /// <summary>The current plugin list/state — answer to <see cref="PluginsGet"/> and broadcast after <see cref="PluginsSave"/>.</summary>
    public const string PluginsResult = "plugins.result";
    /// <summary>Answer to <see cref="PluginAction"/>.</summary>
    public const string PluginActionResult = "plugin.action.result";
    /// <summary>The project's UI appearance (theme/density) changed — broadcast to every window so all
    /// apply it uniformly, independent of which surface they show. A dedicated channel, not piggy-backed
    /// on <see cref="AgentResult"/>, so any view can react without understanding the settings editor.</summary>
    public const string AppearanceChanged = "appearance.changed";
    /// <summary>Token usage totals — answer to <see cref="UsageGet"/>.</summary>
    public const string UsageResult = "usage.result";
    /// <summary>Result of <see cref="SystemRegisterAssociation"/>.</summary>
    public const string SystemRegisterAssociationResult = "system.register_association.result";
    /// <summary>Answer to <see cref="SchemaGet"/>.</summary>
    public const string SchemaResult = "schema.result";

    /// <summary>Answer to <see cref="ProjectList"/>/<see cref="ProjectRecent"/>.</summary>
    public const string ProjectListResult = "project.list.result";
    /// <summary>Answer to <see cref="ProjectOpen"/>/<see cref="ProjectCreate"/> — the same shape
    /// <see cref="WelcomePayload"/> carries for the connection's default project, but for any
    /// project id, so a client can render a new project window without a second round trip.</summary>
    public const string ProjectContext = "project.context";

    /// <summary>Answer to <see cref="FsBrowse"/>.</summary>
    public const string FsBrowseResult = "fs.browse.result";
    /// <summary>Answer to <see cref="FsRead"/>.</summary>
    public const string FsReadResult = "fs.read.result";
    /// <summary>Answer to <see cref="FsWrite"/>.</summary>
    public const string FsWriteResult = "fs.write.result";

    // ── Live SSH terminal (server → client) ──────────────────────────────
    /// <summary>Terminal opened OK. Body <see cref="TerminalOpenedPayload"/>.</summary>
    public const string TerminalOpened = "terminal.opened";
    /// <summary>A chunk of raw pty output (ANSI included) for the terminal to render. Body <see cref="TerminalDataPayload"/>.</summary>
    public const string TerminalData = "terminal.data";
    /// <summary>Terminal closed (by request, disconnect, or error). Body <see cref="TerminalClosedPayload"/>.</summary>
    public const string TerminalClosed = "terminal.closed";

    public const string Error = "error";
}

/// <summary>
/// Capability tokens the server grants a client in <see cref="WelcomePayload.Capabilities"/>. Today
/// the server grants the full set, but clients are expected to look at what they were granted rather
/// than assume omnipotence — so that when groups/roles arrive, a restricted grant just works.
/// </summary>
public static class Capabilities
{
    public const string Chat = "chat";
    public const string Debug = "debug";
    public const string Manage = "manage";

    public static readonly string[] Full = { Chat, Debug, Manage };
}
