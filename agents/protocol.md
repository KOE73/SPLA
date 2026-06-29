# Wire Protocol & Event Registry

STOP — read this before adding, renaming, or removing any WebSocket message type, payload, or
client-side bus event. Message names are **soft strings**: a typo on either side fails silently
(the server logs `Unknown message type`, the client simply never reacts). This file is the registry
that keeps both sides honest.

## Source of truth

- **Wire message names**: `SPLA.Service.Contracts/Protocol.cs` → `MessageTypes` constants. These are
  authoritative. The C# side always references the constant, never a literal.
- **Payload shapes**: `SPLA.Service.Contracts/Payloads.cs`.
- **Envelope**: `ProtocolEnvelope` — `{ type, auth?, chatId?, requestId?, payload? }`. `type` selects
  the payload shape; `payload` rides as raw JSON so a client deserializes only shapes it knows.
- **Protocol version**: `ProtocolVersion.Current` (`"1"`). Bump when the envelope or a payload shape
  changes incompatibly.

The JS client (`SPLA.Service/WebClient/app.js`) uses the same strings as bare literals. When you add
a wire message, add the constant to `MessageTypes` **and** update the client **and** this table.

## Envelope fields

| Field | Meaning |
|-------|---------|
| `type` | One of the `MessageTypes` below. |
| `auth` | Identity/token. Ignored on loopback; reserved for network binds. |
| `chatId` | Which chat the message concerns, when applicable. |
| `requestId` | Correlates request/response pairs (permission, clarify). Same id out and back. |
| `payload` | Typed body for `type`, as raw JSON. |

## Client → Server

| Message | Const | Payload | Notes |
|---------|-------|---------|-------|
| `hello` | `Hello` | `HelloPayload` | First frame; server replies `welcome`. |
| `chat.list` | `ChatList` | — | Request the chat list. |
| `chat.open` | `ChatOpen` | `ChatOpenPayload` | Open a chat; reply `chat.opened`. |
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
| `connections.get` | `ConnectionsGet` | — | Reply `connections.result`. |
| `connections.save` | `ConnectionsSave` | `ConnectionsPayload` | Broadcasts `connections.result`. |
| `agent.get` | `AgentGet` | — | Reply `agent.result`. |
| `agent.save` | `AgentSave` | `AgentSettingsPayload` | Broadcasts `agent.result` **and** `appearance.changed`. |
| `plugins.get` | `PluginsGet` | — | Reply `plugins.result`. |
| `plugins.save` | `PluginsSave` | `PluginsPayload` | Broadcasts `plugins.result`. |

## Server → Client

| Message | Const | Payload | Fan-out | Notes |
|---------|-------|---------|---------|-------|
| `welcome` | `Welcome` | `WelcomePayload` | unicast | Carries project, connections, modes, theme/density. |
| `chat.list.result` | `ChatListResult` | `ChatListResultPayload` | broadcast | Every sidebar refreshes. |
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
| `agent.result` | `AgentResult` | `AgentSettingsPayload` | unicast/broadcast | Answer to get; broadcast after save. |
| `plugins.result` | `PluginsResult` | `PluginsPayload` | unicast/broadcast | Answer to get; broadcast after save. |
| `appearance.changed` | `AppearanceChanged` | `AppearanceChangedPayload` | broadcast | Theme/density; every window + native shell applies it. See [Service event bus](#domain-events-server-side). |
| `error` | `Error` | `ErrorPayload` | unicast | A handler threw. |

`watchers` = every connection currently watching that `chatId` (i.e. has it open). `broadcast` = every
connection. See `ConnectionHub.BroadcastAsync` / `BroadcastToWatchersAsync`.

## Domain events (server-side)

Cross-cutting state changes are not broadcast directly from the dispatch code. A mutator publishes a
typed `ServiceEvent` to `AgentRuntime.Events` (`SPLA.Service/ServiceEvents.cs`); a single subscriber
in `SplaServiceHost.Build` maps each event to a wire broadcast. To add one:

1. Add a `record X : ServiceEvent` in `ServiceEvents.cs`.
2. Publish it from the mutator: `runtime.Events.Publish(new X(...))`.
3. Add a `case X` in the host subscriber → `hub.BroadcastAsync(MessageTypes.…, payload)`.
4. Add the `MessageTypes` constant + payload + a row in the table above.
5. Register a **global** client reactor (`bus.on("…", …)`), not one buried in a surface.

Reference implementation: `AppearanceChanged` → `appearance.changed`.

## Client-local bus events (never hit the wire)

`app.js` fans every inbound message onto a synchronous bus (`Spla.makeBus`); surfaces also emit
purely local events. These are **not** protocol messages — they coordinate UI within one window. Keep
them out of `MessageTypes`, but list them here so names stay consistent:

| Event | Emitted by | Consumed by |
|-------|-----------|-------------|
| `conn` | `setConn` | status bar (connection dot). |
| `chat.current` | `chat.opened` handler | chat list (active highlight), chat log. |
| `appearance.changed` | welcome handler, appearance picker `onchange` | global appearance reactor (also a wire message — same name, applied uniformly). |
| `debug.open` | status-bar debug button | debug drawer. |
| `layout.request` / `layout.applied` | layout picker / `applyLayout` | layout machinery. |
| `local.userMsg` | composer | chat log (optimistic echo). |
| `chat.cleared` | chat-list delete | chat log. |
| `wire` | `send` (out) + `ws.onmessage` (in) | the **wire** debug surface (live protocol tap). |

## Debugging the wire

The `wire` surface (`?surface=wire`, or the 🔌 button on the native shell) is a passive tap: it
subscribes to the local `wire` event and logs every frame in/out with direction, type, and payload.
Use it to confirm a new message actually travels and carries the expected shape before chasing a
silent no-op caused by a name mismatch.
