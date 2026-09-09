# Log — unreleased

The detailed record, appended to as work happens and grouped by date. Never reordered, never
rewritten — this is the working log, not a narrative. Frozen into `CHANGELOGS/<version>.md` on
release, then started empty again.

An entry earns its place if someone outside your own head would notice the change. Time spent is
not the test: an hour's fix that changes visible behaviour gets an entry, two days of refactoring
that changes nothing observable does not. See the changelog rules in `AGENTS.md`.

Each entry is **a bold sentence saying what changed**, optionally followed by the detail. The bold
sentences are what `current-list.md` is built from, which is why they have to stand on their own.

> **Entries through 2026-09-07 were released as `v0.2.6`** and are frozen in
> [`CHANGELOGS/v0.2.6.md`](v0.2.6.md). Everything here is unreleased.

---

## 2026-09-09

- **A connection model can be the project default, and its picker label is a valid CLI model name.**
  Set `default: true` on one model in a configuration layer; the most specific layer that names a
  default wins, while a layer with two defaults is rejected with their ids. New and spawned chats,
  role fallbacks and batch runs all use the resolved default. The status bar shows the qualified
  connection/model label and can copy that exact value for `spla chat run --model`; the settings
  editor preserves the flag without exposing another control.

