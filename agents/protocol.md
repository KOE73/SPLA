# Wire Protocol & Event Registry

STOP — read this before adding, renaming, or removing any WebSocket message type, payload, or
client-side bus event. Message names are **soft strings**: a typo on either side fails silently
(the server logs `Unknown message type`, the client simply never reacts). This file is the registry
that keeps both sides honest.

> **Drift guard:** `ProtocolDocTests` (in `tests/SPLA.Tests`) asserts that every `MessageTypes`
> constant string appears somewhere in this file. If you add a constant without documenting it here,
> that test goes red. Keep the tables below complete.

## Source of truth

- **Wire message names**: `src/service/SPLA.Service.Contracts/Protocol.cs` → `MessageTypes`
  constants. These are authoritative. The C# side always references the constant, never a literal.
- **Payload shapes**: `src/service/SPLA.Service.Contracts/Payloads.cs`.
- **Envelope**: `ProtocolEnvelope` — `{ type, auth?, chatId?, requestId?, payload? }`.
  `type` selects the payload shape; `payload` rides as raw JSON so a client deserializes only shapes
  it knows.
- **Protocol version**: `ProtocolVersion.Current` (`"1"`), echoed in `WelcomePayload.ProtocolVersion`.
  Bump when the envelope or a payload shape changes incompatibly; a client may refuse a mismatch.
- **Server handlers**: dispatch is a registry, not a switch — `src/service/SPLA.Service/Protocol/`
  (`MessageRouter` + `Handlers/*`). A new message type is a new (or extended) `IMessageHandler`,
  never an edit to `ClientConnection`. Handshake (`Hello`) and the token/auth gate stay in
  `ClientConnection` as connection-level concerns.

The TypeScript client (`web/src/protocol/SplaClient.ts`, types in `web/src/protocol/types.ts`) uses
the same strings. `send()`/`invoke()` emit outbound frames; inbound frames fan onto a typed event
bus (`on(type, handler)`), with `ServerEvents` in `types.ts` mapping each server message to its
payload type. When you add a wire message, add the constant to `MessageTypes` **and** update the TS
client/types **and** this table.

## Envelope fields

| Field | Meaning |
|-------|---------|
| `type` | One of the `MessageTypes` below. |
| `auth` | `AuthInfo` (token + reserved actor id). Only the token is checked, and only when a connect token is configured; on loopback it is ignored. |
| `chatId` | Which chat the message concerns, when applicable. |
| ~~`projectId`~~ | **Removed.** A project is a property of the CONNECTION, not of a message — see below. |
| `requestId` | Correlates request/response pairs (permission, clarify, and any `invoke()` RPC). Same id out and back. |
| `payload` | Typed body for `type`, as raw JSON. |

## Client → Server

| Message | Const | Payload | Notes |
|---------|-------|---------|-------|
| `hello` | `Hello` | `HelloPayload` | First frame; server replies `welcome`. When a connect token is required, every other type is rejected until this succeeds. |
| `project.list` | `ProjectList` | — | Reply `project.list.result`. |
| `project.recent` | `ProjectRecent` | — | Reply `project.list.result`, ordered by recency. |
| `project.open` | `ProjectOpen` | `ProjectOpenPayload` | Open a project by id; reply `project.context`. |
| `project.create` | `ProjectCreate` | `ProjectCreatePayload` | Create + open; reply `project.context`. Server mode: created by name inside the user's area. |
| `instance.status` | `InstanceStatus` | — | Ask this process what it is doing right now; reply `instance.status.result`. |
| `instance.stop` | `InstanceStop` | `InstanceStopPayload` | Ask this process to shut down; reply `instance.status.result` (`Stopping: true` once underway, or a refusal naming why). `Force: true` cancels every running turn first. |
| `chat.list` | `ChatList` | — | Request the chat list. |
| `chat.open` | `ChatOpen` | `ChatOpenPayload` | Open a chat; reply `chat.opened`. |
| `chat.watch` | `ChatWatch` | `ChatOpenPayload` | Watch a chat (turn/tool events) without the `chat.opened` echo — for tear-off/aux windows. |
| `chat.unwatch` | `ChatUnwatch` | `ChatOpenPayload` | Stop receiving a chat's turn events. Client-driven: opening another chat is NOT enough, because a chat mid-turn keeps streaming into its own background session. |
| `chat.new` | `ChatNew` | `ChatNewPayload` | Create + open; also broadcasts `chat.list.result`. |
| `chat.rename` | `ChatRename` | `ChatRenamePayload` | Broadcasts `chat.list.result`. |
| `chat.delete` | `ChatDelete` | `ChatDeletePayload` | Broadcasts `chat.list.result`. |
| `chat.send` | `ChatSend` | `ChatSendPayload` | Runs a turn; streams to watchers. |
| `chat.settings` | `ChatSettings` | `ChatSettingsPayload` | Change mode/model entry, temperature or reasoning selection; echoes `chat.opened`. An empty `reasoning` string means "drop my override", which is why it is distinct from null. |
| `chat.reasoning.get` | `ChatReasoningGet` | `ChatReasoningRequest` | Ask what the chat's model takes on its reasoning channel; reply `chat.reasoning.result`. A request, not a field on `chat.opened`, because the answer comes from the provider over the network and opening a chat must not wait on one. |
| `chat.reasoning.result` | `ChatReasoningResult` | `ChatReasoningResult` | The advertised capability: the provider's own effort words, whether "off" is on offer, whether a token budget is. `known: false` = nobody described this model, and the lever stays unavailable rather than guessed. |
| `chat.rewind` | `ChatRewind` | `ChatRewindPayload` | Truncate a chat at/before a message; echoes `chat.opened`. |
| `chat.fork` | `ChatFork` | `ChatForkPayload` | Copy a chat at a message boundary; opens the fork and broadcasts `chat.list.result`. |
| `chat.skill.activate` | `ChatSkillActivate` | `ChatSkillActivatePayload` | Hand a skill to the chat because a person picked it; broadcasts `chat.skill.state` to watchers, or answers `error` with the reason. May name a skill the model was never told about — level hides from the model, not from its owner. |
| `chat.skill.deactivate` | `ChatSkillDeactivate` | `ChatSkillDeactivatePayload` | End the chat's running skill; broadcasts `chat.skill.state` to watchers. The user's exit when the model never calls `skill_deactivate`. |
| `chat.toolset.deactivate` | `ChatToolSetDeactivate` | `ChatToolSetDeactivatePayload` | Lower a tool set raised in a chat; broadcasts `chat.toolset.state` to watchers. The person's control — the model may release a set but is never told it must. |
| `chat.doubt.clear` | `ChatDoubtClear` | `ChatDoubtClearPayload` | Clear the chat's doubt flag; broadcasts `chat.doubt.state` to watchers. A person's decision only — the model has no tool for this, because a mark removable by what it guards against guards nothing. |
| `focus.set` | `FocusSet` | `FocusPayload` | Window focused a chat; echoes `focus.changed` to all. |
| `cancel` | `Cancel` | — (uses `chatId`) | Cancel the active turn of `chatId`. |
| `permission.decision` | `PermissionDecision` | `PermissionDecisionPayload` | Answer to `permission.request` (by `requestId`). |
| `clarify.choice` | `ClarifyChoice` | `ClarifyChoicePayload` | Answer to `clarify.request` (by `requestId`). |
| `debug.request` | `DebugRequest` | `DebugRequestPayload` | Ask for a debug snapshot (`DebugKinds`). |
| `connections.get` | `ConnectionsGet` | — | Reply `connections.result` (+ cached `connections.health`, then a re-ping broadcast). |
| `connections.save` | `ConnectionsSave` | `ConnectionsPayload` | Broadcasts `connections.result`; re-pings. |
| `connection.ping` | `ConnectionPing` | `ConnectionDiagRequest` | Reply `connection.ping.result`. |
| `connection.models` | `ConnectionModels` | `ConnectionDiagRequest` | Reply `connection.models.result`. |
| `connection.test` | `ConnectionTest` | `ConnectionDiagRequest` | Reply `connection.test.result`. |
| `connection.swap_model` | `ConnectionSwapModel` | `ConnectionSwapModelRequest` | Reply `connection.swap_model.result`; broadcasts `connections.result` on success. |
| `provider.info` | `ProviderInfo` | `ProviderInfoRequest` | Reply `provider.info.result`. Account/model figures for one model entry; never returns credential material. |
| `agent.get` | `AgentGet` | — | Reply `agent.result`. |
| `agent.save` | `AgentSave` | `AgentSettingsPayload` | Mode + permission overrides. Broadcasts `agent.result`. |
| `plugins.get` | `PluginsGet` | — | Reply `plugins.result`. |
| `plugins.save` | `PluginsSave` | `PluginsPayload` | Broadcasts `plugins.result`. |
| `plugin.action` | `PluginAction` | `PluginActionPayload` | Invoke a plugin web-settings action; reply `plugin.action.result`. |
| `skills.get` | `SkillsGet` | — | Reply `skills.result`. |
| `skills.save` | `SkillsSave` | `SkillsPayload` | Per-skill enable → `skills.items`, keyed by full address. Applies live. Broadcasts `skills.result`. |
| `skills.sources.get` | `SkillSourcesGet` | — | Reply `skills.sources.result`. Only the branches this person owns; prescribed ones are read-only and already travel with `skills.result`. |
| `skills.sources.save` | `SkillSourcesSave` | `SkillSourcesPayload` | Replaces the person's branch list in their own store — never the project file. Rebuilds sources live. Broadcasts `skills.result`. |
| `skills.source.trust` | `SkillSourceTrust` | `SkillSourceTrustPayload` | Grants or withdraws approval of one branch's contents, stored against the resolved folder. Separate from save on purpose: adding a folder and vouching for its text are different acts with different costs. Broadcasts `skills.result`. |
| `features.get` | `FeaturesGet` | — | Reply `features.result`. |
| `features.save` | `FeaturesSave` | `FeaturesPayload` | Built-in `core.*` set → `agent.capabilities`. Broadcasts `features.result`. |
| `mcp.get` | `McpGet` | — | Reply `mcp.result`. |
| `mcp.save` | `McpSave` | `McpSettingsPayload` | MCP-over-HTTP settings → `.spla` `mcp:` section. Broadcasts `mcp.result`. Takes effect on the next `spla serve` start. |
| `usage.get` | `UsageGet` | — | Reply `usage.result`. |
| `appearance.save` | `AppearanceSave` | `AppearanceChangedPayload` | Auto-sent on change (no Save step). Persists `ui:` + broadcasts `appearance.changed`. |
| `system.register_association` | `SystemRegisterAssociation` | — | Register the `.spla` extension (Windows, per-user). Reply `system.register_association.result`. |
| `schema.get` | `SchemaGet` | `SchemaGetPayload` | Resolve a named JSON schema (Forms editor); reply `schema.result`. |
| `fs.browse` | `FsBrowse` | `FsBrowsePayload` | List a workspace directory; reply `fs.browse.result`. |
| `fs.read` | `FsRead` | `FsReadPayload` | Read a file by ref; reply `fs.read.result`. |
| `fs.write` | `FsWrite` | `FsWritePayload` | Autosave a file; reply `fs.write.result`. |
| `terminal.open` | `TerminalOpen` | `TerminalOpenPayload` | Open a configured SSH host as an interactive terminal. |
| `terminal.input` | `TerminalInput` | `TerminalInputPayload` | Send human keystrokes to an SSH terminal. |
| `terminal.resize` | `TerminalResize` | `TerminalResizePayload` | Update terminal rows and columns. |
| `terminal.close` | `TerminalClose` | `TerminalClosePayload` | Close an SSH terminal. |
| `ssh.sessions.get` | `SshSessionsGet` | — | Snapshot for the SSH picker: configured hosts, every live session (host#N), this connection's open terminals. Reply `ssh.sessions.result`. |
| `ssh.session.close` | `SshSessionClose` | `SshSessionClosePayload` | Close one live SSH session for every viewer. |
| `secret.list` | `SecretList` | `SecretListPayload` | List secret entries (keys + field names only, never values); reply `secret.result`. |
| `secret.set` | `SecretSet` | `SecretSetPayload` | Create/merge a secret entry's fields; reply `secret.result`. |
| `secret.delete` | `SecretDelete` | `SecretDeletePayload` | Delete a whole entry or one field; reply `secret.result`. |
| `plugin.panel.open` | `PluginPanelOpen` | `PluginPanelOpenPayload` | Open an interactive session supplied by an enabled plugin panel provider. |
| `plugin.panel.input` | `PluginPanelInput` | `PluginPanelInputPayload` | Send opaque typed input to a plugin-owned panel session. |
| `plugin.panel.close` | `PluginPanelClose` | `PluginPanelClosePayload` | Close a plugin-owned panel session. |
| `subagent.get` | `SubagentGet` | `SubagentGetPayload` | Ask for one finished spawned run by id — the same id the run's progress ticks carried while it was live. Reply `subagent.result`. An unknown id (fallen out of the ring, or never existed) answers `found: false`, not an error. |
| `task.list` | `TaskList` | `TaskListPayload` | List a chat's background tool calls (`background: true`), running and recently finished. Reply `task.list.result`. See `docs/adr/ADR_20260824-2_core_background-tool-calls.md`. |
| `task.state` | `TaskState` | `TaskStatePayload` | Ask one task's current state — finished result if done, progress tail if still running. Reply `task.state.result`. An unknown task id answers `task: null`, not an error, the same way `subagent.get` treats an unknown run. |
| `task.cancel` | `TaskCancel` | `TaskCancelPayload` | Cancel a live background task. No reply — observe the effect through the next `task.list`/`task.state`, the same as `chat.unwatch`. |

## Server → Client

| Message | Const | Payload | Fan-out | Notes |
|---------|-------|---------|---------|-------|
| `welcome` | `Welcome` | `WelcomePayload` | unicast | Default project, connections, modes, theme/density, protocol version, identity, build branch. |
| `project.list.result` | `ProjectListResult` | `ProjectListResultPayload` | unicast | Answer to `project.list`/`project.recent`. |
| `project.context` | `ProjectContext` | `ProjectContextPayload` | unicast | Answer to `project.open`/`project.create`. |
| `instance.status.result` | `InstanceStatusResult` | `InstanceStatusPayload` | unicast | Answer to `instance.status`/`instance.stop` — a unicast reply to whoever asked, never fanned out to other clients. |
| `chat.list.result` | `ChatListResult` | `ChatListResultPayload` | broadcast (project) | Every sidebar in that project refreshes. |
| `chat.opened` | `ChatOpened` | `ChatOpenedPayload` | unicast | Full chat state on open. |
| `user.message` | `UserMessage` | `UserMessagePayload` | watchers | Accepted user message id/time; optional text renders server-initiated turns. |
| `llm.turn.start` | `LlmTurnStart` | `DeltaPayload` | watchers | New assistant message index. |
| `delta` | `Delta` | `DeltaPayload` | watchers | Streamed assistant text chunk. |
| `reasoning` | `Reasoning` | `ReasoningPayload` | watchers | Streamed reasoning chunk. |
| `llm.attempt` | `Attempt` | `AttemptPayload` | watchers | A generation the repetition guard abandoned mid-stream; never sent for the successful attempt. Carries the abandoned Content/Reasoning so a reader can open it live; `chat.opened`'s `ChatMessageDto.attempts` (`AttemptDto[]`) carries the same fields for a reopened chat, when `agent.save_attempts` was on when it was saved. |
| `assistant.message` | `AssistantMessage` | `AssistantMessagePayload` | watchers | Final assistant message. |
| `tool.started` | `ToolStarted` | `ToolStartedPayload` | watchers | A tool call began. |
| `tool.progress` | `ToolProgress` | `ToolProgressPayload` | watchers | Throttled progress ticks for the top-level call only. One bar, no nesting. |
| `progress.node` | `ProgressNode` | `ProgressNodePayload` | watchers | One node of the turn's progress tree, whole, on each change — the nested counterpart to `tool.progress`, carrying a script's parallel children and a spawned sub-agent's whole run. A flat append-only stream, not a snapshot: keep what you are told and attach each node to `parentId` (null = top level). Hold a node whose parent has not arrived rather than dropping it — parallel work gives no ordering guarantee. Structural frames (a node's first appearance and its finish) are never throttled; the ticks between them are, per node. Both this and `tool.progress` are sent; a client that wants one bar can ignore this. |
| `tool.result` | `ToolResult` | `ToolResultPayload` | watchers | A tool call finished. |
| `subagent.result` | `SubagentResult` | `SubagentResultPayload` | unicast | Answer to `subagent.get`: the finished run's transcript (`messages` reuses `ChatMessageDto`) plus its label, mode, outcome and timing. `found: false` when the id is not in the log. |
| `task.list.result` | `TaskListResult` | `TaskListResult` | unicast | Answer to `task.list`: this chat's background tasks as summary rows (id, tool, state, started-at). |
| `task.state.result` | `TaskStateResult` | `TaskStateResult` | unicast | Answer to `task.state`: the task's summary plus its result text once finished (`Result` null while running). `Task: null` for an unknown id. |
| `notice` | `Notice` | `NoticePayload` | watchers | Inline notice. |
| `token.usage` | `TokenUsage` | `TokenUsagePayload` | watchers | Per-turn token counts; `contextLength` (nullable) carries the model's operative window for the client's context-budget display. |
| `turn.complete` | `TurnComplete` | `TurnCompletePayload` | watchers | Turn ended; re-enable input. `activeSkillId` reports a skill still running — end of turn is when one the model forgot to close becomes actionable. |
| `chat.skill.state` | `ChatSkillState` | `ChatSkillStatePayload` | watchers | The chat's active skill changed (an explicit hand-out or unload). |
| `chat.toolset.state` | `ChatToolSetState` | `ChatToolSetStatePayload` | watchers | The chat's tool sets, raised or merely announced. Sent after every turn and after an explicit lowering; sets levelled off are never listed. |
| `chat.doubt.state` | `ChatDoubtState` | `ChatDoubtStatePayload` | watchers | Whether the chat has taken in content from a source nobody named, with the causes. Sent on clearing; the flag also rides `chat.opened`, since it survives a reload. |
| `permission.request` | `PermissionRequest` | `PermissionRequestPayload` | watchers | Outstanding permission question. Replayed to a client opening the chat while a question is still pending. |
| `clarify.request` | `ClarifyRequest` | `ClarifyRequestPayload` | watchers | Outstanding clarification question. Replayed to a client opening the chat while a question is still pending. |
| `ask.resolved` | `AskResolved` | `AskResolvedPayload` | watchers | An outstanding permission or clarify question was resolved (answered, cancelled, or timed out). Payload carries `Reason`. |
| `debug.snapshot` | `DebugSnapshot` | `DebugSnapshotPayload` | unicast | Answer to `debug.request`. |
| `focus.changed` | `FocusChanged` | `FocusPayload` | broadcast | Tear-off windows follow the active chat. |
| `connections.result` | `ConnectionsResult` | `ConnectionsPayload` | unicast/broadcast | Answer to get; broadcast after save. |
| `connections.health` | `ConnectionsHealth` | health snapshot | unicast/broadcast (project) | Cached on get; re-pinged on startup/get/save. |
| `connection.ping.result` | `ConnectionPingResult` | diag result | unicast | Answer to `connection.ping`. |
| `connection.models.result` | `ConnectionModelsResult` | diag result | unicast | Answer to `connection.models`. |
| `connection.test.result` | `ConnectionTestResult` | diag result | unicast | Answer to `connection.test`. |
| `connection.swap_model.result` | `ConnectionSwapModelResult` | `ConnectionSwapModelResult` | unicast | Answer to `connection.swap_model`. |
| `provider.info.result` | `ProviderInfoResult` | `ProviderInfoResult` | unicast | Answer to `provider.info`. Sections ordered connection-first, then model. |
| `agent.result` | `AgentResult` | `AgentSettingsPayload` | unicast/broadcast | Answer to get; broadcast after save. |
| `plugins.result` | `PluginsResult` | `PluginsPayload` | unicast/broadcast | Answer to get; broadcast after save. |
| `plugin.action.result` | `PluginActionResult` | `PluginActionResultPayload` | unicast | Answer to `plugin.action`. |
| `skills.result` | `SkillsResult` | `SkillsPayload` | unicast/broadcast | Answer to get; broadcast after any save AND unprompted whenever the fond is rebuilt — a file changed, a branch was added, a grant moved. Lists every skill with its address, source and resolved state, unavailable ones included. |
| `skills.sources.result` | `SkillSourcesResult` | `SkillSourcesPayload` | unicast | Answer to `skills.sources.get`. The editable half of the fond only. |
| `features.result` | `FeaturesResult` | `FeaturesPayload` | unicast/broadcast | Answer to get; broadcast after save. `restartToApply` is always true — feature tools register once at startup. |
| `mcp.result` | `McpResult` | `McpSettingsPayload` | unicast/broadcast | Answer to `mcp.get`; broadcast after `mcp.save`. |
| `usage.result` | `UsageResult` | usage totals | unicast/broadcast (project) | Answer to `usage.get`; also broadcast after each turn's token accounting. |
| `appearance.changed` | `AppearanceChanged` | `AppearanceChangedPayload` | broadcast | Theme/density; every window applies it. See [Domain events](#domain-events-server-side). |
| `system.register_association.result` | `SystemRegisterAssociationResult` | result | unicast | Answer to `system.register_association`. |
| `schema.result` | `SchemaResult` | `SchemaResultPayload` | unicast | Answer to `schema.get`. |
| `fs.browse.result` | `FsBrowseResult` | `FsBrowseResultPayload` | unicast | Answer to `fs.browse`. |
| `fs.read.result` | `FsReadResult` | `FsReadResultPayload` | unicast | Answer to `fs.read`. |
| `fs.write.result` | `FsWriteResult` | `FsWriteResultPayload` | unicast | Answer to `fs.write`. |
| `terminal.opened` | `TerminalOpened` | `TerminalOpenedPayload` | unicast | SSH terminal is ready. |
| `terminal.data` | `TerminalData` | `TerminalDataPayload` | unicast | Raw SSH terminal output. |
| `terminal.closed` | `TerminalClosed` | `TerminalClosedPayload` | unicast | SSH terminal ended or failed. |
| `ssh.sessions.result` | `SshSessionsResult` | `SshSessionsResultPayload` | unicast | Answer to `ssh.sessions.get` — names only, never credentials. |
| `ssh.sessions.changed` | `SshSessionsChanged` | — | broadcast (project) | The set of live SSH sessions changed; clients re-fetch. |
| `secret.result` | `SecretResult` | `SecretListResultPayload` | unicast | Answer to any `secret.*` request — entry keys + field names, never values. |
| `plugin.panel.opened` | `PluginPanelOpened` | `PluginPanelOpenedPayload` | unicast | Plugin panel session is ready. |
| `plugin.panel.event` | `PluginPanelEvent` | `PluginPanelEventPayload` | unicast | Opaque event emitted by a plugin-owned panel session. |
| `error` | `Error` | `ErrorPayload` | unicast | A handler threw, or a request was rejected. |

`watchers` = every connection currently watching that `chatId`. `broadcast` = every connection;
`broadcast (project)` = every connection that has touched that project. See
`ConnectionHub.BroadcastAsync` / `BroadcastToWatchersAsync` / `BroadcastToProjectAsync`.

## Capabilities

`WelcomePayload.Capabilities` carries the tokens the server granted this client
(`Capabilities.Chat`/`Debug`/`Manage`). Today the server grants the full set, but clients should
gate features on what they were granted, so a restricted grant (groups/roles, later) just works.

## Domain events (server-side)

Cross-cutting state changes are not broadcast directly from handler code. A mutator publishes a typed
`ServiceEvent` to `AgentRuntime.Events` (`src/agent/SPLA.Runtime/ServiceEvents.cs`); a single
subscriber in `src/service/SPLA.Service/Hosting/SplaServiceHost.cs` maps each event to a wire broadcast, scoped by project id. To
add one:

1. Add a `record X : ServiceEvent` in `ServiceEvents.cs`.
2. Publish it from the mutator: `runtime.Events.Publish(new X(...))`.
3. Add a `case X` in the host subscriber → `hub.BroadcastToProjectAsync(projectId, MessageTypes.…, payload)`.
4. Add the `MessageTypes` constant + payload + a row in the table above.
5. Register a client reactor via `client.on("…", …)`.

Reference implementation: `AppearanceChanged` → `appearance.changed`.

## Client-local events (never hit the wire)

`SplaClient` fans every inbound frame onto its typed bus (`on(type, handler)`); surfaces also emit
purely local UI events (e.g. `conn` for the connection dot). These are **not** protocol messages —
keep them out of `MessageTypes`. Every in/out frame is also mirrored to `onWire(...)` listeners.

## A connection has one project

There is no `projectId` on the envelope, and adding one back is not the fix for anything.

The server binds a connection to a project when the socket is established (the user's own default in
server mode, the process's project locally) and rebinds it **only** on `project.open`. Everything
else — chats, settings, secrets, plugins, usage — is implicitly about that project.

It used to be per message, and the cost was paid in two places. Clients had to remember the field on
every send, and forgetting it wrote silently into whichever project the connection defaulted to; the
web client carried a `projectEnvelope()` helper and a comment warning every settings surface to pass
it. And it made a local invariant into a fiction: a process has exactly one working directory, so a
window claiming to hold several projects at once was telling the truth about its runtimes and a lie
about anything resolved relative to `cwd`.

So: **a second project is a second connection.** Locally that means a second window; on a server it
means one socket walking between the user's own projects via `project.open`. Broadcast scoping
(`BroadcastToProjectAsync`) still exists and still works — `IsWatchingProject` is now simply "is this
the connection's project".

## The registry channel is a different protocol

Everything above is the chat protocol: `ProtocolEnvelope`, `MessageTypes`, one socket per client.
The registry hub does **not** speak it. It has its own tiny vocabulary in
`SPLA.Instances.Contracts/RegistryProtocol.cs` — `register`, `status`, `accepted`, `stop` — carried
as `RegistryFrame` over `/registry/ws`, with `GET /registry/instances` and `POST /registry/stop`
beside it.

That separation is deliberate and worth keeping. An instance registering with a hub is saying three
things — I exist, here is my address, here is what I am doing — and none of them need chats, tool
calls, permissions or projects. Making registration speak the chat protocol would drag this entire
contract into every instance and every observer, and would tie the hub's compatibility to a protocol
that changes for unrelated reasons.

So: **do not add registry messages to `MessageTypes`**, and do not route them through
`MessageRouter`. They are listed in `RegistryFrames`, served by
`SPLA.Service/Hosting/RegistryEndpoints.cs`, and consumed by `InstanceRegistrar` /
`RemoteInstanceRegistry`.

What *is* in the chat protocol is asking one instance directly what it is doing —
`instance.status` / `instance.stop`, in the tables above. Same question, different asker: the hub
learns it pushed over the registration channel, a client on the instance's own socket asks for it.

## Debugging the wire

The Wire surface (`web/src/surfaces/Wire.vue`) is a passive tap: it subscribes to the client's wire
listener and logs every frame in/out with direction, type, and payload. Use it to confirm a new
message actually travels and carries the expected shape before chasing a silent no-op caused by a
name mismatch.
