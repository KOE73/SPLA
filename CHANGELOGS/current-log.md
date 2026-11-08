# Log — unreleased

The detailed record, appended to as work happens and grouped by date. Never reordered, never
rewritten — this is the working log, not a narrative. Frozen into `CHANGELOGS/<version>.md` on
release, then started empty again.

An entry earns its place if someone outside your own head would notice the change. Time spent is
not the test: an hour's fix that changes visible behaviour gets an entry, two days of refactoring
that changes nothing observable does not. See the changelog rules in `agents/release.md`.

Each entry is **a bold sentence saying what changed**, optionally followed by the detail. The bold
sentences are what `current-list.md` is built from, which is why they have to stand on their own.

> **Entries through 2026-09-16 were released as `v0.2.7`** and are frozen in
> [`CHANGELOGS/v0.2.7.md`](v0.2.7.md). Everything here is unreleased.

---

## 2026-10-15

**Windows now defaults to DPAPI secret storage and refuses unavailable or invalid backends instead of falling back to plaintext.**
Explicit `secrets.backend: file` remains available for development and tests. CLI, desktop and server
startup use the same selection policy; existing plaintext files are not read as a fallback.

## 2026-11-08

**Claude models through OpenRouter can use prompt caching, with cache read/write token counts shown for each chat.**
Caching is enabled by default and can be disabled per model with `prompt_cache: false` or in connection
settings. Chat and spawned-run identities accompany cache requests, and provider-reported cache
counters flow through token accounting to the status bar.

**Roles use the same connection-grouped model picker as chats, and spawned agents honor the role's selected model.**
The picker keeps the owning connection visible and preserves a configured model that is temporarily
unavailable. The role settings payload carries the model and connection metadata used by both pickers.

**The project includes builder and tester roles for concise command reports, plus shared SeMaps MCP configuration.**
The project manifest enables its MCP endpoint on port 60777 and includes both command-reporting roles.
