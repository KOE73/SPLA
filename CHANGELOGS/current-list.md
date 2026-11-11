# Changes — unreleased

The scannable list: one line per change, no dates, no detail. Derived from the entry headings in
`current-log.md` rather than written independently, so the two cannot drift apart. Frozen into
`CHANGELOGS/<version>.md` on release, then started empty again.

The release commit freezes this list, the summary and the log in the versioned changelog.
The release workflow publishes the separately authored versioned release notes.

**Covers work since `v0.2.8`**, frozen in [`CHANGELOGS/v0.2.8.md`](v0.2.8.md).

---

- VisionAgent and LogSentry now start with the default Windows secret store.
- Server builds and publishes use the DPAPI backend produced by the selected build configuration.
