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
- **Envelope**: `ProtocolEnvelope` — `{ type, auth?, chatId?, projectId?, requestId?, payload? }`.
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
| `projectId` | Which project the message concerns. Null = this connection's default project; the server keeps no "current project" state, so a multi-project client sets it per message. |
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
| `chat.list` | `ChatList` | — | Request the chat list. |
| `chat.open` | `ChatOpen` | `ChatOpenPayload` | Open a chat; reply `chat.opened`. |
| `chat.watch` | `ChatWatch` | `ChatOpenPayload` | Watch a chat (turn/tool events) without the `chat.opened` echo — for tear-off/aux windows. |
| `chat.new` | `ChatNew` | `ChatNewPayload` | Create + open; also broadcasts `chat.list.result`. |
| `chat.rename` | `ChatRename` | `ChatRenamePayload` | Broadcasts `chat.list.result`. |
| `chat.delete` | `ChatDelete` | `ChatDeletePayload` | Broadcasts `chat.list.result`. |
| `chat.send` | `ChatSend` | `ChatSendPayload` | Runs a turn; streams to watchers. |
| `chat.settings` | `ChatSettings` | `ChatSettingsPayload` | Change mode/connection; echoes `chat.opened`. |
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
| `agent.get` | `AgentGet` | — | Reply `agent.result`. |
| `agent.save` | `AgentSave` | `AgentSettingsPayload` | Mode + permission overrides. Broadcasts `agent.result`. |
| `plugins.get` | `PluginsGet` | — | Reply `plugins.result`. |
| `plugins.save` | `PluginsSave` | `PluginsPayload` | Broadcasts `plugins.result`. |
| `plugin.action` | `PluginAction` | `PluginActionPayload` | Invoke a plugin web-settings action; reply `plugin.action.result`. |
| `usage.get` | `UsageGet` | — | Reply `usage.result`. |
| `appearance.save` | `AppearanceSave` | `AppearanceChangedPayload` | Auto-sent on change (no Save step). Persists `ui:` + broadcasts `appearance.changed`. |
| `system.register_association` | `SystemRegisterAssociation` | — | Register the `.spla` extension (Windows, per-user). Reply `system.register_association.result`. |
| `schema.get` | `SchemaGet` | `SchemaGetPayload` | Resolve a named JSON schema (Forms editor); reply `schema.result`. |
| `fs.browse` | `FsBrowse` | `FsBrowsePayload` | List a workspace directory; reply `fs.browse.result`. |
| `fs.read` | `FsRead` | `FsReadPayload` | Read a file by ref; reply `fs.read.result`. |
| `fs.write` | `FsWrite` | `FsWritePayload` | Autosave a file; reply `fs.write.result`. |

## Server → Client

| Message | Const | Payload | Fan-out | Notes |
|---------|-------|---------|---------|-------|
| `welcome` | `Welcome` | `WelcomePayload` | unicast | Default project, connections, modes, theme/density, protocol version, identity. |
| `project.list.result` | `ProjectListResult` | `ProjectListResultPayload` | unicast | Answer to `project.list`/`project.recent`. |
| `project.context` | `ProjectContext` | `ProjectContextPayload` | unicast | Answer to `project.open`/`project.create`. |
| `chat.list.result` | `ChatListResult` | `ChatListResultPayload` | broadcast (project) | Every sidebar in that project refreshes. |
| `chat.opened` | `ChatOpened` | `ChatOpenedPayload` | unicast | Full chat state on open. |
| `llm.turn.start` | `LlmTurnStart` | `DeltaPayload` | watchers | New assistant message index. |
| `delta` | `Delta` | `DeltaPayload` | watchers | Streamed assistant text chunk. |
| `reasoning` | `Reasoning` | `ReasoningPayload` | watchers | Streamed reasoning chunk. |
| `assistant.message` | `AssistantMessage` | `AssistantMessagePayload` | watchers | Final assistant message. |
| `tool.started` | `ToolStarted` | `ToolStartedPayload` | watchers | A tool call began. |
| `tool.progress` | `ToolProgress` | `ToolProgressPayload` | watchers | Throttled progress ticks. |
| `tool.result` | `ToolResult` | `ToolResultPayload` | watchers | A tool call finished. |
| `notice` | `Notice` | `NoticePayload` | watchers | Inline notice. |
| `token.usage` | `TokenUsage` | `TokenUsagePayload` | watchers | Per-turn token counts. |
| `turn.complete` | `TurnComplete` | `TurnCompletePayload` | watchers | Turn ended; re-enable input. |
| `permission.request` | `PermissionRequest` | `PermissionRequestPayload` | unicast | To the initiating client (by `requestId`). |
| `clarify.request` | `ClarifyRequest` | `ClarifyRequestPayload` | unicast | To the initiating client (by `requestId`). |
| `debug.snapshot` | `DebugSnapshot` | `DebugSnapshotPayload` | unicast | Answer to `debug.request`. |
| `focus.changed` | `FocusChanged` | `FocusPayload` | broadcast | Tear-off windows follow the active chat. |
| `connections.result` | `ConnectionsResult` | `ConnectionsPayload` | unicast/broadcast | Answer to get; broadcast after save. |
| `connections.health` | `ConnectionsHealth` | health snapshot | unicast/broadcast (project) | Cached on get; re-pinged on startup/get/save. |
| `connection.ping.result` | `ConnectionPingResult` | diag result | unicast | Answer to `connection.ping`. |
| `connection.models.result` | `ConnectionModelsResult` | diag result | unicast | Answer to `connection.models`. |
| `connection.test.result` | `ConnectionTestResult` | diag result | unicast | Answer to `connection.test`. |
| `connection.swap_model.result` | `ConnectionSwapModelResult` | `ConnectionSwapModelResult` | unicast | Answer to `connection.swap_model`. |
| `agent.result` | `AgentResult` | `AgentSettingsPayload` | unicast/broadcast | Answer to get; broadcast after save. |
| `plugins.result` | `PluginsResult` | `PluginsPayload` | unicast/broadcast | Answer to get; broadcast after save. |
| `plugin.action.result` | `PluginActionResult` | `PluginActionResultPayload` | unicast | Answer to `plugin.action`. |
| `usage.result` | `UsageResult` | usage totals | unicast/broadcast (project) | Answer to `usage.get`; also broadcast after each turn's token accounting. |
| `appearance.changed` | `AppearanceChanged` | `AppearanceChangedPayload` | broadcast | Theme/density; every window applies it. See [Domain events](#domain-events-server-side). |
| `system.register_association.result` | `SystemRegisterAssociationResult` | result | unicast | Answer to `system.register_association`. |
| `schema.result` | `SchemaResult` | `SchemaResultPayload` | unicast | Answer to `schema.get`. |
| `fs.browse.result` | `FsBrowseResult` | `FsBrowseResultPayload` | unicast | Answer to `fs.browse`. |
| `fs.read.result` | `FsReadResult` | `FsReadResultPayload` | unicast | Answer to `fs.read`. |
| `fs.write.result` | `FsWriteResult` | `FsWriteResultPayload` | unicast | Answer to `fs.write`. |
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
`ServiceEvent` to `AgentRuntime.Events` (`src/service/SPLA.Service/ServiceEvents.cs`); a single
subscriber in `SplaServiceHost.Build` maps each event to a wire broadcast, scoped by project id. To
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

## Debugging the wire

The Wire surface (`web/src/surfaces/Wire.vue`) is a passive tap: it subscribes to the client's wire
listener and logs every frame in/out with direction, type, and payload. Use it to confirm a new
message actually travels and carries the expected shape before chasing a silent no-op caused by a
name mismatch.
