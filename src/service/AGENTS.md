# src/service — service core & wire protocol

`SPLA.Service` (WebSocket host, per-project/per-chat runtimes, message handlers) and
`SPLA.Service.Contracts` (the wire protocol: `MessageTypes`, payloads). Read the root `AGENTS.md`
first, and `agents/protocol.md` before touching any message type.

## Invariants

- **A new message type is a new handler, never an edit to `ClientConnection`.** Dispatch is a
  registry (`Protocol/MessageRouter.cs`) of per-module `IMessageHandler`s (Project, Chat,
  Correlation, Connection, Settings, Workspace) acting through the `IClientSession` facade +
  `RequestContext`. Add the handler (or a case in the right module handler) and register it; the
  connection owns only transport, the auth/handshake cross-cut, correlation tables, and turn plumbing.
- **Every `MessageTypes` constant is part of a contract with the TS client** (`web/src/protocol`).
  Adding/renaming/removing one is a two-sided change and must be recorded in `agents/protocol.md`.
  The protocol is not yet versioned — until it is, assume old clients exist and avoid silent breaks.
- **Security enforcement lives here and in core, not in the tool layer.** The token gate (reject
  non-Hello before a successful handshake when a connect token is required) and the `/ws` Origin
  check are connection-level cross-cutting concerns — keep them in `ClientConnection`/`SplaServiceHost`,
  one place, not sprinkled per handler.
- **Project resolution is one point (`Resolve`).** In server mode a user must not open a project
  outside their own area; the ACL/path check belongs in `Resolve` (or the middleware that wraps it),
  so no individual handler can bypass it. Do not resolve a runtime by raw path in a handler.
- **`AgentRuntime` is per-project and process-wide; `ChatRuntime` is per-chat.** Shared, chat-agnostic
  state (LLM client, tools, plugins, prompt, project KV) goes on `AgentRuntime`; anything that must
  not be shared between chats (conversation, session KV, checkpoint) goes on `ChatRuntime`.

## Tests

Protocol tests open a real WebSocket. Bind an OS-assigned free port (`FreePort()` in
`MultiProjectProtocolTests`), never a hardcoded one — fixed ports collide with whatever else listens
on the machine.
