# Log — unreleased

The detailed record, appended to as work happens and grouped by date. Never reordered, never
rewritten — this is the working log, not a narrative. Frozen into `CHANGELOGS/<version>.md` on
release, then started empty again.

An entry earns its place if someone outside your own head would notice the change. Time spent is
not the test: an hour's fix that changes visible behaviour gets an entry, two days of refactoring
that changes nothing observable does not. See the changelog rules in `agents/release.md`.

Each entry is **a bold sentence saying what changed**, optionally followed by the detail. The bold
sentences are what `current-list.md` is built from, which is why they have to stand on their own.

> **Entries for `v0.2.8`** are frozen in
> [`CHANGELOGS/v0.2.8.md`](v0.2.8.md). Everything here is unreleased.

---

## 2026-11-11

**VisionAgent and LogSentry now start with the default Windows secret store.**
Both register DPAPI before resolving project settings. The runtime embedding examples show the same setup.

**Server builds and publishes use the DPAPI backend produced by the selected build configuration.**
Custom MSBuild output paths are supported, and DPAPI read warnings are sent to the server logger.
