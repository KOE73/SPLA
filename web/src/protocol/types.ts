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
  role: "user" | "assistant" | "tool";
  content?: string;
  reasoning?: string;
  images?: string[];
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
  settingsYaml?: string;
  /** URL of the plugin's prebuilt web settings module (dynamically imported), or absent when the
   * plugin has none — the panel falls back to the generic YAML editor. */
  webSettingsUrl?: string;
}

/** Contract a plugin's web settings module must export — see web_settings_entry in meta.yaml. */
export interface PluginSettingsMountApi {
  /** Current opaque settings blob as YAML, or null when none. */
  getYaml(): string | null;
  /** Generic RPC into the host/plugin backend, e.g. invoke("plugin.action", {...}). */
  invoke<R = unknown>(type: string, payload?: unknown): Promise<R>;
}
export interface PluginSettingsHandle {
  /** Returns the edited settings serialized back to YAML. Called when the host Saves. */
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

// ── Events the server pushes unprompted (subscribe via client.on) ──────────────
export interface ServerEvents {
  /** Local-only: emitted by SplaClient itself on socket open/close, never sent by the server. */
  "conn": { on: boolean; text?: string };
  "welcome": {
    theme?: string; density?: string; projectId?: string; projectName?: string; workspacePath?: string;
    modes?: string[]; defaultMode?: string;
    connections?: { id: string; name?: string }[];
  };
  "appearance.changed": { theme?: string; density?: string };
  "chat.opened": ChatOpenedPayload;
  "chat.list.result": { chats: ChatSummary[] };
  "chat.cleared": Record<string, never>;
  "chat.current": ChatOpenedPayload;
  "focus.changed": { chatId: string };
  "token.usage": { promptTokens?: number; completionTokens?: number };
  "delta": { msgIndex: number; text: string };
  "reasoning": { msgIndex: number; text: string };
  "llm.turn.start": { msgIndex: number };
  "assistant.message": { msgIndex: number; message: ChatMessage };
  "turn.complete": { cancelled?: boolean; error?: string };
  "tool.started": { toolCall: { name: string } };
  "tool.result": { toolName: string; result: string };
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
  "schema.result": SchemaResultPayload;
  "debug.snapshot": DebugSnapshotPayload;
  "local.userMsg": { text: string; images?: string[] };
  "project.list.result": ProjectListResultPayload;
  "project.context": ProjectContextPayload;
}
