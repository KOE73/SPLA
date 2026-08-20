// Protocol types — mirrors SPLA.Service.Contracts/Protocol.cs and Payloads.cs.
// Filled in incrementally as each surface is migrated; unknown payloads stay `unknown`, never `any`.

export interface WireFrame {
  dir: "in" | "out";
  type: string;
  payload: unknown;
  chatId?: string;
  requestId?: string;
  ts: number;
}

export interface Envelope<P = unknown> {
  type: string;
  payload: P;
  chatId?: string;
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
  /** Generations the repetition guard threw away before this message was produced. Only present
   *  when the project had `agent: save_attempts` on when the chat was saved. */
  attempts?: AttemptDto[];
}

/** One abandoned generation as stored on a message — see server `AttemptDto`. */
export interface AttemptDto {
  index: number;
  outcome: string;
  note?: string;
  chars: number;
  durationMs: number;
  content?: string;
  reasoning?: string;
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
  /** A turn is running in this chat right now — including one started by another window. */
  turnActive?: boolean;
  /** The chat's operational state: "idle" | "working" | "waiting" | "stalled".
   *  - idle: nothing running
   *  - working: a turn is running and making progress
   *  - waiting: the agent is blocked on a person (permission or clarification request)
   *  - stalled: a turn is registered but nothing has happened for ~10 minutes (model may have stopped halfway) */
  state?: string;
}

export interface ChatOpenedPayload {
  chatId: string;
  title?: string;
  messages: ChatMessage[];
  mode?: string;
  modelId?: string;
  /** The chat's effective sampling temperature (its own override, else the project default). */
  temperature?: number;
  /** The chat's effective reasoning selection: "" | "off" | "on" | an effort word | "budget:N". */
  reasoning?: string;
  /** The skill running in this chat, if any — the unload control keys off this. */
  activeSkillId?: string | null;
  doubt?: ChatDoubt;
  /** Tool sets this chat can see, raised or merely announced. */
  toolSets?: ToolSetState[];
  /** Whether a turn was already running when this chat was opened — so a window attaching mid-turn
   *  (or a reload) shows Stop rather than an input that looks ready. */
  turnActive?: boolean;
  /** The chat's operational state: "idle" | "working" | "waiting" | "stalled". */
  state?: string;
}

/**
 * What a model will let a caller do with its reasoning channel, as its provider describes it.
 *
 * There is no cross-vendor standard, so this carries three independent axes rather than one kind:
 * a switch (`mandatory` / `defaultEnabled`), a graded scale (`efforts`), and a token budget. A model
 * may have several at once — LM Studio reports Qwen3.8 as `["off","low","medium","xhigh","on"]`, a
 * switch and a scale in one list, with no `"high"` in it. The words are the provider's own and are
 * shown verbatim; nothing here is normalized to a vocabulary the UI made up.
 *
 * `known: false` means nobody described this model — most OpenAI-compatible servers. That is not the
 * same as "cannot reason", and it is why the lever is withheld rather than guessed: a LocalAI-hosted
 * Gemma accepts `reasoning_effort` it does not implement and answers with an endless "0.5-0.5-0.5".
 */
export interface ReasoningCapabilityDto {
  known: boolean;
  supported: boolean;
  /** The model always reasons; "off" is not on offer. */
  mandatory: boolean;
  defaultEnabled: boolean;
  /** Graded effort words, in the provider's vocabulary and order. */
  efforts: string[];
  defaultEffort?: string | null;
  supportsTokenBudget: boolean;
  minTokenBudget?: number | null;
  maxTokenBudget?: number | null;
}

export interface ChatReasoningResult {
  chatId: string;
  modelId?: string | null;
  reasoning: ReasoningCapabilityDto;
}

/** One tool set as a chat sees it. `level` is the standing permission, `by` is who raised it here
 * (empty when merely announced), `disclosed` is whether its tools are in the context right now —
 * which is what the status bar weights by, since that is what actually costs. */
export interface ToolSetState {
  setId: string;
  by: "skill" | "agent" | "user" | "";
  reason?: string | null;
  level: string;
  disclosed: boolean;
}

/** One editable connection: transport + credentials, owning its model entries. */
export interface ConnectionDto {
  id: string;
  clientId?: string;
  name?: string;
  provider?: string;
  endpoint?: string;
  /** A `secret:`/`env:` REFERENCE, never key material — the browser holds pointers only. Blank on
   *  save means "leave the stored credential alone" (see `apiKeyIsLiteral`). */
  apiKey?: string;
  /** Account-management credential (management / admin key). Never used for inference. */
  adminKey?: string;
  /** Server→client: the project still holds a pasted plaintext key, which was withheld from us.
   *  Client→server: we are not touching it — keep it. Clearing this while sending a blank `apiKey`
   *  is how a credential gets removed. */
  apiKeyIsLiteral?: boolean;
  /** As `apiKeyIsLiteral`, for the admin key. */
  adminKeyIsLiteral?: boolean;
  swapModel?: boolean;
  models: ModelEntryDto[];
}

/** One model under a connection. `id` is ours and globally unique; `model` is the provider's string. */
export interface ModelEntryDto {
  id: string;
  clientId?: string;
  name?: string;
  model?: string;
  contextLength?: number;
}

/** A model entry flattened for pickers, keeping its owning connection for grouping. */
export interface ModelPickDto {
  id: string;
  name: string;
  model?: string;
  connectionId: string;
  connectionName: string;
  provider?: string;
}

/** One provider-reported figure. `kind` drives formatting, `severity` drives the dot. */
export interface ProviderFactDto {
  key: string;
  label: string;
  value: string;
  unit?: string;
  kind?: "counter" | "money" | "percent" | "duration" | "timestamp" | "text";
  severity?: "normal" | "warn" | "critical";
  observedAt?: string;
  resetsAt?: string;
}

export interface ProviderFactSectionDto {
  title: string;
  facts: ProviderFactDto[];
  deepLink?: string;
}

export interface ProviderInfoResultPayload {
  modelId: string;
  connectionId: string;
  connectionName: string;
  provider?: string;
  sections: ProviderFactSectionDto[];
  error?: string;
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
  loopGuard?: boolean; loopGuardRepeats?: number; saveToolCalls?: boolean; saveAttempts?: boolean;
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
  /** How far this plugin's tool set may reach the model: "disabled" | "skill_demand" |
   * "agent_demand" | "enabled". Empty/absent = follows `enabled`, which is what every project
   * written before tool sets does. Delivery (`enabled`) and disclosure (`level`) are separate
   * decisions — see agents/toolsets.md. */
  level?: string;
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
  /**
   * Mounts the host's credential control into a plugin-owned element: pick an existing secret-store
   * entry or create one, yielding a `secret:<scope>:<key>` reference through `onChange`. Handed out
   * rather than reimplemented per plugin — that is what keeps `secret.*` (and any chance of a value
   * escaping into a settings blob) out of plugin code entirely.
   */
  mountCredentialField(el: HTMLElement, opts: CredentialFieldOptions): CredentialFieldHandle;
}

export interface CredentialFieldOptions {
  /** Current reference, or empty. */
  value?: string;
  onChange(reference: string): void;
  /** Offer "(none)" for consumers that can also work without a credential. Default true. */
  allowNone?: boolean;
  noneLabel?: string;
  createScope?: SecretScopeId | "";
}
export interface CredentialFieldHandle {
  setValue(reference: string): void;
  destroy(): void;
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

/**
 * One switchable capability — a built-in `core.*` feature or a skill. Plugins keep the richer
 * PluginDto above (settings blob, web settings URL), but all three render through the same row
 * component in the Capabilities settings group, so the common shape is shared.
 */
export interface CapabilityDto {
  id: string;
  /**
   * A skill's full address, `branch:id` — what identifies it, since two branches may hold the same
   * name. Absent for built-ins. Key rows and saves on this, never on `id`: two editions of one name
   * would otherwise share a row and a switch.
   */
  address?: string;
  kind: "builtin" | "skill";
  name: string;
  description?: string;
  enabled?: boolean;
  /** Available / MissingPrerequisites / DisabledByUser / DisabledByTrust — read-only. */
  state?: string;
  stateReason?: string;
  /** Provider id for a skill ("project", "machine", "plugin:network"); absent for built-ins. */
  source?: string;
  sourceLabel?: string;
  /** Normalised subject words. Skills only — what the panel's facets filter on. */
  tags?: string[];
  missingTools?: string[];
  missingFeatures?: string[];
  /** Plugins owning the missing tools — the panel offers to enable them. */
  missingPlugins?: string[];
  /** Built-in ids this one depends on; enabling it pulls them in. */
  requires?: string[];
}

export interface SkillSourceDto {
  id: string;
  label: string;
  trust: string;
  /** OutOfCatalog / Findable / InCatalog / OnShelf — how much of this source the model is told
   *  about unasked. Editable for your own branches, read-only for prescribed ones. */
  level?: string;
  /** Filesystem location for folder-backed sources; absent for anything else. */
  path?: string;
  /** Project / Machine / Granted / Deployment — which layer declared it. */
  origin?: string;
  /** False when switched off. Off is not gone: an inherited branch can only be darkened. */
  enabled?: boolean;
  /** True when the row lives in your own store and can be edited or removed outright. */
  editable?: boolean;
  /** True when this location carries an explicit trust grant, as opposed to being trusted by
   *  default. Kept apart so the two never look alike. */
  trustGranted?: boolean;
}

export interface SkillsResultPayload {
  skills: CapabilityDto[];
  sources: SkillSourceDto[];
  canPersist?: boolean;
}

/** One branch as the panel edits it. Only the person's own branches are editable this way. */
export interface SkillSourceEditDto {
  /** Reusing a prescribed branch's id is how you override it — the only way to switch one off. */
  id: string;
  /** Absent for a pure override (a row that only says "off"). */
  path?: string;
  label?: string;
  /** OutOfCatalog / Findable / InCatalog / OnShelf — context economy, unrelated to trust. */
  level?: string;
  enabled?: boolean;
}

export interface SkillSourcesResultPayload {
  sources: SkillSourceEditDto[];
}

export interface FeaturesResultPayload {
  features: CapabilityDto[];
  canPersist?: boolean;
  /** Feature tools register once at startup, so a change applies on the next service start. */
  restartToApply?: boolean;
}

export interface ConnectionsResultPayload {
  connections: ConnectionDto[];
  canPersist?: boolean;
  /** Set when a save was refused (e.g. duplicate model id); the list echoes what is still in effect. */
  error?: string;
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
/** One contribution to the assembled agent context: who produced it, which of its pieces it is,
 *  where it is delivered ("prompt" | "turn" | "failed"), and a local token estimate for attribution. */
/** One row of a kv/blob debug view. `origin` is where the value came from; it gets its own column
 *  rather than being folded into the value, because a label mixed into text is a label nobody
 *  scans for. `doubtful` is the same bit that raises the chat's flag. */
/** Whether a chat has taken in anything from a source nobody named, and what did it. The causes
 *  travel with the flag because a bare red dot with no account of itself gets dismissed on reflex. */
export interface ChatDoubt {
  raised: boolean;
  causes: { zone: string; what: string; at: string }[];
}

/** One movement between perimeters and how much has gone along it. `outward` is not a verdict —
 *  nothing is refused yet — it marks the rows worth looking at first. */
export interface DebugEdge {
  source: string;
  sink: string;
  effect: string;
  calls: number;
  lastTool: string;
  outward: boolean;
}

export interface DebugKvEntry {
  key: string;
  value: string;
  origin?: string | null;
  doubtful?: boolean;
}

export interface DebugSegment {
  title: string;
  body: string;
  contributor: string;
  source: string;
  placement: string;
  approxTokens: number;
  problem?: string | null;
}

export interface DebugSnapshotPayload {
  contextLines?: ContextLine[];
  totalCount?: number;
  contextCount?: number;
  approxTokens?: number;
  contextIsLive?: boolean;
  entries?: DebugKvEntry[];
  edges?: DebugEdge[];
  segments?: DebugSegment[];
  text?: string;
}

// ── Secret store ─────────────────────────────────────────────────────────────
/** Where a secret lives. Always explicit — the server never picks one for you. */
export type SecretScopeId = "user" | "project" | "shared";

/** One entry: key + field NAMES (user, password, token, private_key, …) — never values.
 *  `scope` and `reference` travel with every entry so a picker can show WHERE a credential lives
 *  instead of leaving the user to guess between two identically named ones. */
export interface SecretEntryDto {
  key: string;
  fields: string[];
  scope: SecretScopeId;
  /** Ready-to-paste `secret:<scope>:<key>`. */
  reference: string;
  /** False when this user may use the entry but not change it (shared scope, ACL). */
  canManage: boolean;
}

export interface SecretListResultPayload {
  user: SecretEntryDto[];
  project: SecretEntryDto[];
  shared: SecretEntryDto[];
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
  connections?: ModelPickDto[];
  modes?: string[];
  defaultMode?: string;
  theme?: string;
  density?: string;
}

// ── Spawned sub-agent runs (subagent.get → subagent.result) ───────────────────
export interface SubagentResultPayload {
  /** False when the id fell out of the bounded in-memory ring, or never existed — a normal answer,
   *  not an error; every other field is left at its default in that case. */
  found: boolean;
  runId: string;
  label: string;
  skillId?: string;
  mode: string;
  startedAt: string;
  finishedAt: string;
  /** "completed" | "failed" | "cancelled". */
  outcome: string;
  error?: string;
  result: string;
  messages: ChatMessage[];
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
  /**
   * Local-only: emitted by SplaClient itself on socket open/close, never sent by the server.
   * `lost` separates "the agent has stopped" from "the socket dropped and we are retrying" — the
   * client cannot tell them apart in one event, so it says so after several failed attempts.
   * `attempts` is the consecutive failure count, zero once connected.
   */
  "conn": { on: boolean; text?: string; lost?: boolean; attempts?: number };
  "welcome": {
    theme?: string; density?: string; projectId?: string; projectName?: string; workspacePath?: string;
    modes?: string[]; defaultMode?: string;
    connections?: ModelPickDto[];
    /** Authenticated user (server mode). Empty on local/embedded — the identity badge stays hidden. */
    userKey?: string; userName?: string;
    /** Set only when this build was published from a branch other than main — draws the warning banner. */
    branch?: string;
  };
  "appearance.changed": { theme?: string; density?: string };
  "chat.opened": ChatOpenedPayload;
  "chat.reasoning.result": ChatReasoningResult;
  "chat.list.result": { chats: ChatSummary[] };
  "chat.cleared": Record<string, never>;
  "chat.current": ChatOpenedPayload;
  "focus.changed": { chatId: string };
  "token.usage": { promptTokens?: number; completionTokens?: number; contextLength?: number };
  "delta": { msgIndex: number; text: string };
  "reasoning": { msgIndex: number; text: string };
  "llm.turn.start": { msgIndex: number };
  /** A generation the repetition guard abandoned mid-stream — never sent for the successful attempt.
   *  Carries the abandoned content/reasoning so a reader can open it, not just the streamed text that
   *  was already visible before the guard cut it off. */
  "llm.attempt": { msgIndex: number; index: number; outcome: string; note?: string; chars: number;
    durationMs: number; content?: string; reasoning?: string };
  "assistant.message": { msgIndex: number; message: ChatMessage };
  /** User message accepted by the server. Text is present so server-initiated turns can render
   * without a local echo; ordinary composer turns use it only as a fallback. */
  "user.message": { msgId: string; createdAt?: string; text?: string };
  "turn.complete": { cancelled?: boolean; error?: string; activeSkillId?: string | null };
  /** A chat's active skill changed — after an explicit unload. */
  "chat.skill.state": { chatId: string; activeSkillId?: string | null };
  "chat.doubt.state": { chatId: string; doubt: ChatDoubt };
  /** A chat's raised tool sets changed — after a turn, or after an explicit lowering. */
  "chat.toolset.state": { chatId: string; sets?: ToolSetState[] };
  "tool.started": { toolCall: ToolCallDto };
  "tool.progress": { toolCallId?: string; toolName: string; current: number; total: number; fraction?: number | null; message?: string | null; details?: ToolProgressDetail[] | null };
  /** One node of the turn's progress tree, whole, on each change — the nested counterpart to
   *  `tool.progress`, which reports the top-level call only and so cannot show a script's parallel
   *  children or a spawned sub-agent's run at all.
   *
   *  A flat append-only stream, not a snapshot: keep what you are told, keyed by `nodeId`, and attach
   *  each node to `parentId` (null = top level). Hold a node whose parent has not arrived rather than
   *  dropping it — parallel work gives no ordering guarantee. Structural frames (a node's first
   *  appearance and its finish) are never throttled; the ticks between them are, per node. */
  "progress.node": { nodeId: string; parentId?: string | null; label: string; state: "running" | "completed" | "failed"; current?: number | null; total?: number | null; fraction?: number | null; message?: string | null; details?: ToolProgressDetail[] | null };
  "tool.result": { toolCallId: string; toolName: string; result: string };
  "notice": { text: string };
  "error": { message: string };
  "permission.request": { toolName: string; arguments?: string };
  "clarify.request": { question: string; options?: { label: string; description?: string }[] };
  /** An outstanding permission or clarify question is no longer outstanding — answered elsewhere,
   *  cancelled, or timed out. The envelope carries the requestId; this payload explains why. */
  "ask.resolved": { reason: string };
  "connections.result": ConnectionsResultPayload;
  "connections.health": ConnectionsHealthPayload;
  "connection.models.result": ConnectionModelsResultPayload;
  "connection.ping.result": ConnectionPingResultPayload;
  "connection.test.result": ConnectionTestResultPayload;
  "connection.swap_model.result": ConnectionSwapModelResultPayload;
  "provider.info.result": ProviderInfoResultPayload;
  "agent.result": AgentResultPayload;
  "plugins.result": PluginsResultPayload;
  "skills.result": SkillsResultPayload;
  "skills.sources.result": SkillSourcesResultPayload;
  "features.result": FeaturesResultPayload;
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
  "subagent.result": SubagentResultPayload;
}
