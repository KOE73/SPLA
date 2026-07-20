// Protocol types — mirrors SPLA.Service.Contracts/Protocol.cs and Payloads.cs.
// Filled in incrementally as each surface is migrated; unknown payloads stay `unknown`, never `any`.

export interface WireFrame {
  dir: "in" | "out";
  type: string;
  payload: unknown;
  chatId?: string;
  projectId?: string;
  requestId?: string;
  ts: number;
}

export interface Envelope<P = unknown> {
  type: string;
  payload: P;
  chatId?: string;
  /** Which project this message concerns; absent = the connection's default project. See
   * AgentRuntimeRegistry.DefaultProjectId server-side — single-project usage never sets this. */
  projectId?: string;
  requestId?: string;
}

export interface ChatMessage {
  msgId?: string;
  role: "user" | "assistant" | "tool";
  content?: string;
  reasoning?: string;
  /** ISO-8601 UTC creation time; absent on chats saved before timestamps existed. */
  createdAt?: string;
  images?: string[];
  toolCalls?: ToolCallDto[];
  toolCallId?: string;
}

export interface ToolCallDto {
  id: string;
  name: string;
  arguments: string;
}

export interface ToolProgressDetail {
  label: string;
  value: string;
}

export interface ChatSummary {
  id: string;
  title?: string;
}

export interface ChatOpenedPayload {
  chatId: string;
  title?: string;
  messages: ChatMessage[];
  mode?: string;
  connectionId?: string;
}

export interface ConnectionDto {
  id: string;
  clientId?: string;
  name?: string;
  provider?: string;
  endpoint?: string;
  model?: string;
  apiKey?: string;
  lockModel?: boolean;
  swapModel?: boolean;
}

export interface ConnHealth {
  ok: boolean | null;
  error?: string;
}

export interface AgentResultPayload {
  mode?: string;
  modes?: string[];
  permRead?: string; permWrite?: string; permShell?: string; permInternet?: string;
  customPrompt?: string;
  loopGuard?: boolean; loopGuardRepeats?: number;
  theme?: string; density?: string;
  themes?: string[]; densities?: string[];
  canPersist?: boolean;
}

export interface PluginDto {
  id: string;
  name?: string;
  version?: string;
  state?: string;
  stateReason?: string;
  enabled?: boolean;
  customPrompt?: string;
  /** Opaque settings blob as JSON (the host converts to/from the YAML stored in the .spla file). */
  settingsJson?: string;
  /** URL of the plugin's prebuilt web settings module (dynamically imported), or absent when the
   * plugin has none — the panel falls back to the generic JSON editor. */
  webSettingsUrl?: string;
}

/** Contract a plugin's web settings module must export — see web_settings_entry in meta.yaml. */
export interface PluginSettingsMountApi {
  /** Current opaque settings blob as JSON, or null when none. */
  getJson(): string | null;
  /** Generic RPC into the host/plugin backend, e.g. invoke("plugin.action", {...}). */
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
}
export interface PluginSettingsHandle {
  /** Returns the edited settings serialized back to JSON. Called when the host Saves. */
  save(): string | null;
  destroy?(): void;
}
export type PluginSettingsMount = (el: HTMLElement, api: PluginSettingsMountApi) => PluginSettingsHandle;

export interface PluginsResultPayload {
  plugins: PluginDto[];
  canPersist?: boolean;
  restartToApply?: boolean;
}

export interface ConnectionsResultPayload {
  connections: ConnectionDto[];
  canPersist?: boolean;
}

export interface ConnectionsHealthPayload {
  statuses: ({ id: string } & ConnHealth)[];
}

export interface ConnectionModelsResultPayload {
  id: string;
  models?: string[];
  error?: string;
}

export interface ConnectionTestResultPayload {
  id: string;
  reply?: string;
  error?: string;
}

export interface ConnectionSwapModelResultPayload {
  id: string;
  model?: string;
  error?: string;
}

export interface ConnectionPingResultPayload {
  id: string;
  ok: boolean;
  error?: string;
}

export interface TokenUsageScope {
  promptTokens: number;
  completionTokens: number;
  turns: number;
  totalTokens: number;
}

export interface UsageResultPayload {
  session: TokenUsageScope;
  project: TokenUsageScope;
  machine: TokenUsageScope;
}

export interface ContextLine {
  index: number;
  msgId: string;
  approxTokens: number;
  source: string;
  preview: string;
  full?: string;
  inContext: boolean;
}

/** debug.request "kind" determines which of these shapes comes back; only one set of fields is present. */
export interface DebugSnapshotPayload {
  contextLines?: ContextLine[];
  totalCount?: number;
  contextCount?: number;
  approxTokens?: number;
  contextIsLive?: boolean;
  entries?: { key: string; value: string }[];
  text?: string;
}

// ── Secret store ─────────────────────────────────────────────────────────────
/** One entry: key + field NAMES (user, password, token, private_key, …) — never values. */
export interface SecretEntryDto {
  key: string;
  fields: string[];
}

export interface SecretListResultPayload {
  machine: SecretEntryDto[];
  project: SecretEntryDto[];
  projectOpen: boolean;
  error?: string;
}

// ── Schema editor ──────────────────────────────────────────────────────────
export interface SchemaResultPayload {
  name: string;
  dataSchema?: string;   // raw JSON string (JSON Schema 2020-12)
  uiSchema?: string;     // raw JSON string (JSON Forms UISchema)
  error?: string;
}

// ── Workspace filesystem browser ──────────────────────────────────────────────
export interface FsNode {
  ref: string;
  label: string;
  kind: "folder" | "leaf";
  contentType?: string;
  sizeBytes?: number;
  modified?: string;
}

export interface FsBrowseResultPayload {
  nodes: FsNode[];
}

export interface FsReadResultPayload {
  ref: string;
  text: string;
  contentType?: string;
  error?: string;
}

export interface FsWriteResultPayload {
  ref: string;
  ok: boolean;
  error?: string;
}

// ── Project browser ─────────────────────────────────────────────────────────
export interface ProjectDescriptor {
  id: string;
  name?: string;
  manifestPath?: string;
  lastOpened?: string;
}

export interface ProjectListResultPayload {
  projects: ProjectDescriptor[];
}

export interface ProjectContextPayload {
  projectId: string;
  projectName?: string;
  workspacePath?: string;
  connections?: ConnectionDto[];
  modes?: string[];
  defaultMode?: string;
  theme?: string;
  density?: string;
}

// ── Live SSH picker (ssh.sessions.get → ssh.sessions.result) ──────────────────
export interface SshHostDto {
  name: string;
  host?: string;
  port?: number;
  isDefault?: boolean;
  description?: string;
}

export interface SshSessionDto {
  /** Addressable id, host#N. */
  id: string;
  host: string;
  /** "agent" or "human". */
  openedBy: string;
  /** How many terminals are attached. */
  viewers: number;
}

export interface SshSessionsResultPayload {
  hosts: SshHostDto[];
  /** Every live session in the project's hub — agent- and human-opened. */
  sessions: SshSessionDto[];
  /** Terminals this client connection has open (views over sessions). */
  terminals: { terminalId: string; host: string; sessionId: string }[];
}

// ── Events the server pushes unprompted (subscribe via client.on) ──────────────
export interface ServerEvents {
  /** Local-only: emitted by SplaClient itself on socket open/close, never sent by the server. */
  "conn": { on: boolean; text?: string };
  "welcome": {
    theme?: string; density?: string; projectId?: string; projectName?: string; workspacePath?: string;
    modes?: string[]; defaultMode?: string;
    connections?: { id: string; name?: string }[];
    /** Authenticated user (server mode). Empty on local/embedded — the identity badge stays hidden. */
    userKey?: string; userName?: string;
  };
  "appearance.changed": { theme?: string; density?: string };
  "chat.opened": ChatOpenedPayload;
  "chat.list.result": { chats: ChatSummary[] };
  "chat.cleared": Record<string, never>;
  "chat.current": ChatOpenedPayload;
  "focus.changed": { chatId: string };
  "token.usage": { promptTokens?: number; completionTokens?: number; contextLength?: number };
  "delta": { msgIndex: number; text: string };
  "reasoning": { msgIndex: number; text: string };
  "llm.turn.start": { msgIndex: number };
  "assistant.message": { msgIndex: number; message: ChatMessage };
  /** User message accepted by the server. Text is present so server-initiated turns can render
   * without a local echo; ordinary composer turns use it only as a fallback. */
  "user.message": { msgId: string; createdAt?: string; text?: string };
  "turn.complete": { cancelled?: boolean; error?: string };
  "tool.started": { toolCall: ToolCallDto };
  "tool.progress": { toolCallId?: string; toolName: string; current: number; total: number; fraction?: number | null; message?: string | null; details?: ToolProgressDetail[] | null };
  "tool.result": { toolCallId: string; toolName: string; result: string };
  "notice": { text: string };
  "error": { message: string };
  "permission.request": { toolName: string; arguments?: string };
  "clarify.request": { question: string; options?: { label: string; description?: string }[] };
  "connections.result": ConnectionsResultPayload;
  "connections.health": ConnectionsHealthPayload;
  "connection.models.result": ConnectionModelsResultPayload;
  "connection.ping.result": ConnectionPingResultPayload;
  "connection.test.result": ConnectionTestResultPayload;
  "connection.swap_model.result": ConnectionSwapModelResultPayload;
  "agent.result": AgentResultPayload;
  "plugins.result": PluginsResultPayload;
  "usage.result": UsageResultPayload;
  "system.register_association.result": { ok?: boolean; message?: string };
  "secret.result": SecretListResultPayload;
  "schema.result": SchemaResultPayload;
  "debug.snapshot": DebugSnapshotPayload;
  "local.userMsg": { text: string; images?: string[] };
  "project.list.result": ProjectListResultPayload;
  "project.context": ProjectContextPayload;
  // Live SSH terminal (phase B)
  "terminal.opened": { terminalId: string; host: string; sessionId: string };
  "terminal.data": { terminalId: string; data: string };
  "terminal.closed": { terminalId: string; reason?: string };
  "ssh.sessions.result": SshSessionsResultPayload;
  "ssh.sessions.changed": Record<string, never>;
  "plugin.panel.opened": { panelId: string };
  "plugin.panel.event": { panelId: string; eventType: string; data?: { base64?: string; mimeType?: string; url?: string; message?: string } };
}
