# web/ — the Vue client

Read the root `AGENTS.md` first. Wire messages and client bus events: [`agents/protocol.md`](../agents/protocol.md).

## Chat-scoped state (recurring bug — do not regress)

The composer input, Send/Stop button, and every other per-conversation UI state belong to the
**current chat**, not the window. In `web/src` any such state MUST live in `store.ts` keyed by
`chatId` (e.g. `store.turnActiveByChat`) and be read via a computed over `store.currentChat`.
Never hold it in a component-local `ref` — it leaks across chat switches (a running turn in chat A
locked input in chat B, twice). Server events must be applied by `env.chatId` from the envelope,
never to whatever chat happens to be open.
