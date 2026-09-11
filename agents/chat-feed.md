# The Session Event Stream (`ChatFeed`)

STOP — read this before touching `ChatFeed`, `ChatEvents.cs`, a feed subscriber,
`ChatFeedWireSubscriber`, or anything about how a turn's events leave a chat. Describes what exists
now, per [`agents/documentation.md`](documentation.md)'s rule that "how something works now" belongs
in `agents/`, never in a design document.

Full reasoning: [`ADR_20260910-2_agent_chat-feed`](../docs/adr/ADR_20260910-2_agent_chat-feed.md)
(the "смысловой терминал" frame — ssh analogy, snapshot+stream, why publish must not wait). Work log,
wave by wave, including every deviation from the ADR's plan:
[`PLAN_20260910-2_agent_chat-feed`](../docs/plans/PLAN_20260910-2_agent_chat-feed.md).

## The shape

Every session — a human `ChatRuntime` and a spawned `SpawnedSession` alike — owns one `ChatFeed`
(`src/agent/SPLA.Runtime/ChatFeed.cs`), reachable as `.Feed`. A session **publishes to it always and
the same way**, no matter who started the turn or who, if anyone, is watching. Nothing about who
called `SendAsync` decides who sees the result — that used to be true (`ChatTurnDriver`,
`ConsoleHandlers`, `BatchRunner`, `SpawnedAgentRunner` each built their own `AgentCallbacks`) and was
exactly the defect ADR §2 names: a spawned chat's own window had nothing to subscribe to.

`AgentCallbacks` (the orchestrator's 11-hook interface) still exists — `ConversationOrchestrator` is
shared and I/O-agnostic and there was no reason to break it — but only a **session** builds one now,
as a private adapter "hooks → feed", inside `SendAsync`/the spawned run. Nobody outside a session
constructs `AgentCallbacks` for a real turn any more (test code that unit-tests the orchestrator
directly is the one carved-out exception — see plan wave 3's note).

## The closed event set

`ChatEvents.cs` (`src/agent/SPLA.Runtime`, not `SPLA.Domain` — see the file's own comment for why:
`ChatAskRaised`/`ChatAskResolved` need `PendingAsk`/`AskResolution`, declared in
`SPLA.Runtime/PendingAsks.cs`, so the set stays together in the lowest project that can reference
every payload). Every `ChatEvent` subtype:

`ChatTurnStarted`, `ChatTurnCompleted`, `ChatUserMessage`, `ChatLlmCallStarted`, `ChatDelta`,
`ChatReasoning`, `ChatAttempt`, `ChatAssistantMessage`, `ChatToolStarted`, `ChatToolProgress`,
`ChatToolResult`, `ChatProgressNode`, `ChatNotice`, `ChatLlmTurn`, `ChatAskRaised`, `ChatAskResolved`,
`ChatTaskChanged`.

The set is closed by design (ADR §4.2) — exactly what already reached the wire through the paths the
ADR counted, nothing new. **Events carry their own ids** (`msgId`, `callId`, a progress-tree node id)
— never a bubble index. `NextBubbleIndex` is a wire concept, assigned by `ChatFeedWireSubscriber`, not
the event's own identity.

## Who publishes

- **`ChatRuntime`** (`src/agent/SPLA.Runtime/ChatRuntime.cs`) — `Emit(ChatEvent)` /
  `Emit(ChatEvent, Action mutate)` wrap `Feed.Publish`. The chat itself declares turn start/end, the
  user message, asks raised/resolved (subscribed to `runtime.Asks.Asked/Resolved` in the constructor,
  filtered by `ChatId`), task changes (`Tasks.Changed`, also constructor-subscribed), and progress
  nodes (`Progress.NodeChanged`, also constructor-subscribed — for the chat's whole life, not one
  turn). `Emit(e, mutate)` folds a state change (appending to `_conversation`, growing the live
  partial, clearing it) into the **same** `ChatFeed._gate` critical section as the sequence bump and
  delivery — see "Snapshot + sequence" below for why that matters.
- **`SpawnedSession`** (`src/agent/SPLA.Runtime/SpawnedSession.cs`) — same `ChatFeed` class, but no
  `AgentCallbacks` of its own: `ISpawnedSession` lives in `SPLA.Domain`, which cannot reference
  `AgentCallbacks` (declared in `SPLA.Agent`), so the interface grew a set of `Publish*` methods
  (`PublishLlmTurnStart`, `PublishDelta`, `PublishAttempt`, `PublishAssistantMessage`,
  `PublishToolStarted`, `PublishToolProgress`, `PublishToolResult`, `PublishLlmTurn`, `PublishNotice`,
  …) typed only in Domain-visible payloads. `SpawnedAgentRunner` still builds the one `AgentCallbacks`
  the orchestrator needs (nothing else can), and each hook now does two things: marks the parent's
  progress tree (unchanged, pre-existing behaviour) **and** calls the matching `session.Publish*` —
  one call, two effects, not two separate `AgentCallbacks`.

Both types implement `IChatFeedSession { string ChatId; ChatFeed Feed; ResubscribeQueuedWithSnapshot(...); }`
— the minimal surface `ChatFeedWireSubscriber` needs to treat a human chat and a spawned run's one
turn identically.

## Subscription modes

- **`Subscribe(Action<ChatEvent> handler)`** — synchronous, in-process, in order. `Publish` calls the
  handler directly, inside its own lock. Right for a cheap in-process subscriber that wants to see
  every event before `SendAsync` returns and never wants to risk missing one: `ConsoleHandlers`
  (`SubscribeRich`/`SubscribeBasic`), `BatchRunner`, tests. A throwing handler is swallowed — a
  misbehaving subscriber must never break the chat that is publishing.
- **`SubscribeQueued(Func<ChatEvent, Task> handler, Action onDetached)`** — a bounded, per-subscriber
  queue plus one dedicated consumer `Task`. `Publish` only ever touches `Enqueue` (in-memory
  list/dictionary bookkeeping, no I/O, no `await`); the actual work (a WebSocket send) runs off the
  chat's own call stack, so a slow subscriber never slows the turn. This is what
  `ChatFeedWireSubscriber` uses.

### Lag rules (`SubscribeQueued` only)

- **Coalesce.** Consecutive `ChatDelta`/`ChatReasoning` for the same `MsgIndex` concatenate;
  `ChatToolProgress`/`ChatProgressNode` for the same call/node id keep only the latest — merged
  in-place, position unchanged. Key: `QueuedSubscription.CoalesceKeyFor`.
  A structural event is a **barrier**: it clears the coalescing table, so a delta after it can never
  merge back into a delta before it (otherwise "chunk A, an abandoned attempt that clears the bubble,
  chunk B" would deliver as "AB" ahead of the attempt, erasing the real answer on the client).
- **Never drop structural events.** Everything without a coalesce key (message, tool
  started/result, turn started/completed, ask raised/resolved, user message, task changed, attempt,
  llm-call-started, token usage, notice) piles up instead of being lost.
- **Overflow → detach.** Past `ChatFeed.StructuralOverflowThreshold` (512, generous — a normal turn
  produces a few dozen) UNDELIVERED structural events, the subscription unsubscribes itself and calls
  `onDetached`. Counted as undelivered until the handler has actually finished with an item — an
  in-flight event held by a stuck connection is exactly as undelivered as one still queued.

## Snapshot + sequence

`ChatFeed` holds `_gate` (a `Lock`) and `_sequence`. `Publish` runs the optional
`mutateUnderGate` (a chat's own state change — see "Who publishes") **before** bumping the sequence,
inside the same critical section as delivery. Three atomic combinations built on that gate, all on
`ChatFeed`:

| Method | Used by | Guarantee |
|---|---|---|
| `SubscribeWithSnapshot<T>(captureSnapshot, handler)` | `ChatRuntime.SubscribeWithSnapshot` | New in-process subscription + a caller snapshot, same gate acquisition. |
| `SnapshotUnderGate<T>(captureSnapshot, attach)` | `ChatRuntime`/`SpawnedSession.SnapshotForOpen` | Marks a connection a watcher (`attach`) and reads state, same gate acquisition — no new `IChatFeed` subscriber (the wire already has one permanent `ChatFeedWireSubscriber` per chat fanning out to whichever connections are marked watching). |
| `SubscribeQueuedWithSnapshot<T>(handler, onDetached, captureSnapshot)` | `IChatFeedSession.ResubscribeQueuedWithSnapshot` (wave 5) | Registers a **queued** subscription and captures the snapshot together — the overflow-detach resync path (below). |

`ChatFeedSnapshot` (messages, live partial, open progress nodes, pending asks, running tasks) is read
from the session's **own memory**, never from disk — a chat mid-turn has nothing worth reading on disk
yet, and a spawned session writes its file only once, in `Finish`.

**Why the third method exists (wave 5).** An overflow-detach used to call `SubscribeQueued` for a
fresh queue, then build the resync `chat.opened` **separately and later**, off the gate, once per
watching connection (`SnapshotForOpen`). Between those two moments an event could land in the new
queue (already subscribed) **and** in the snapshot (captured after it) — delivered twice.
`ResubscribeQueuedWithSnapshot` does both under one gate acquisition, so the snapshot returned is
exactly the state as of the sequence number the new queue starts delivering after. The one snapshot is
then reused for every watching connection (`ClientConnection.SendOpenedFromSnapshotAsync` /
`SendOpenedSpawnedFromSnapshotAsync`) instead of each connection re-querying — re-querying per
connection would reopen the same gap per connection. See `ChatFeedQueuedSubscriptionTests
.SubscribeQueuedWithSnapshot_never_double_delivers_against_a_concurrent_publisher`.

## The wire subscriber

`ChatFeedWireSubscriber` (`src/service/SPLA.Service/Hosting/ChatFeedWireSubscriber.cs`) — one instance
per chat, constructed on `ChatRegistry.RuntimeOpened`/`SpawnedOpened`, disposed on
`RuntimeClosed`/`SpawnedClosed`. Maps every `ChatEvent` onto the exact wire message types and payload
shapes the pre-stream code used to build (`ChatTurnDriver.BuildCallbacks`,
`SplaServiceHost.WireChatProgress` — both removed, folded in here); the wire protocol itself did not
change (`agents/protocol.md` still governs message names/shapes). It uses `SubscribeQueued`, throttles
`tool.progress`/`progress.node` at 120ms (structural frames excepted, same as before), does
`Turns.Touch` on every event, and re-subscribes + resyncs via `ResubscribeQueuedWithSnapshot` on
`onDetached`.

## Other subscribers

- **CLI console** (`ConsoleHandlers.SubscribeRich`/`SubscribeBasic`) — synchronous `Subscribe`, text
  render, two variants.
- **`chat run` / `BatchRunner`** — synchronous `Subscribe`, accumulates `answer`/`stream`/stats.
- **A spawn parent** — not a feed subscriber in the literal sense (`SPLA.Agent` cannot reference
  `ChatEvent`/`ChatFeed`); `SpawnedAgentRunner`'s hooks do the parent's progress-tree marking and the
  session's `Publish*` call together, from the same call site.
- **MCP progress projection**, **correspondence** (one chat as another's watcher) — same mechanism,
  different renders (ADR §4.6).

## What stays outside the feed

- **Project-level events** (`runtime.Events` — theme, skills, MCP servers) — not chat-scoped, see
  `agents/protocol.md`'s "Domain events (server-side)".
- **Persistence.** Writing the chat file (`Save()`) stays synchronous inside `SendAsync`/`Finish`, not
  a subscriber — a subscriber may lag or be detached, and a save must not follow those rules. See
  `docs/KNOWN_ISSUES.md`, "Чат сохраняется на диск только в конце успешного хода": the feed's snapshot
  makes a live watcher independent of when the file is written, but a process crash mid-turn still
  loses that turn — the feed lives in memory only.
- **Inbound commands** (send, cancel, answer a question) — not part of this stream; see
  `ChatHandlers`/`PendingAskStore`.

## Adding a new event

1. Add a `record ChatXxx(...) : ChatEvent` to `ChatEvents.cs`.
2. Publish it: a new `ChatRuntime.Emit(...)` call, or a new `IChatFeedSession.Publish*`/
   `SpawnedSession` method plus the matching `SpawnedAgentRunner` hook.
3. If it needs a coalesce/never-drop rule beyond "structural" (the default), extend
   `QueuedSubscription.CoalesceKeyFor`/`Merge` in `ChatFeed.cs`.
4. Map it in `ChatFeedWireSubscriber.OnEventAsync` to a `MessageTypes` constant + payload.
5. Document the wire message in [`agents/protocol.md`](protocol.md) (its own STOP rule) and add the TS
   client/types (`web/src/protocol/SplaClient.ts`, `types.ts`).
6. Wire a web reactor if the UI needs to react.
