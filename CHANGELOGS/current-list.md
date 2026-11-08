# Changes — unreleased

The scannable list: one line per change, no dates, no detail. Derived from the entry headings in
`current-log.md` rather than written independently, so the two cannot drift apart. Frozen into
`CHANGELOGS/<version>.md` on release, then started empty again.

This list and the summary are what CI puts into the release body; the full log stays in the
repository and is linked from it.

**Covers work since `v0.2.7`**, frozen in [`CHANGELOGS/v0.2.7.md`](v0.2.7.md).

---

- Windows now defaults to DPAPI secret storage and refuses unavailable or invalid backends instead of falling back to plaintext.
- Claude models through OpenRouter can use prompt caching, with cache read/write token counts shown for each chat.
- Roles use the same connection-grouped model picker as chats, and spawned agents honor the role's selected model.
- The project includes builder and tester roles for concise command reports, plus shared SeMaps MCP configuration.
