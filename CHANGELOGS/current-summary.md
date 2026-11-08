# Summary — unreleased

<!-- covers: 2026-11-08 -->

The prose account of the current cycle: what changed and why it matters, organised by theme rather
than by date. Rewritten from scratch before each push — never appended to. On release it is frozen
into `CHANGELOGS/<version>.md` and this file starts empty again.

The `covers:` marker on the first line records the latest `current-log.md` date this text accounts
for. CI compares the two: if the log has moved on, this summary is stale and is left out of the
release rather than published as if it were current.

---

Windows hosts now use DPAPI secret storage by default. Invalid or unavailable backends stop settings
resolution instead of silently selecting plaintext. Development and tests can explicitly select the
file backend; existing plaintext stores are neither migrated nor read as an encrypted-store fallback.

Claude models through OpenRouter can request prompt caching, enabled by default with a per-model
opt-out. Stable chat and spawned-run identities accompany cache requests, and the status bar displays
provider-reported cache reads and writes for the last call.

Chats and roles share a model picker grouped by connection, and spawned agents use the model selected
by their role. The project also includes builder and tester roles for compact build/test reports and
portable SeMaps MCP configuration; its own MCP endpoint is configured on port 60777.

