# Summary — unreleased

<!-- covers: 2026-08-28 -->

The prose account of the current cycle: what changed and why it matters, organised by theme rather
than by date. Rewritten from scratch before each push — never appended to. On release it is frozen
into `CHANGELOGS/<version>.md` and this file starts empty again.

The `covers:` marker on the first line records the latest `current-log.md` date this text accounts
for. CI compares the two: if the log has moved on, this summary is stale and is left out of the
release rather than published as if it were current.

---

### Documents & Spreadsheets

Word documents (`.docx`) can now be extracted into markdown, plain text, or structured JSON AST trees without Microsoft Office dependencies, enabling the model to read complex document hierarchies. Spreadsheets (`.xlsx`, `.csv`) support row appends and inspection keyed strictly by column header names rather than cell coordinates.

### Detached Background Tasks & Live Progress

Tool calls can opt into asynchronous background execution (`background: true`). The agent gets a task identifier immediately while the background execution delivers its outcome upon completion into the chat session. Progress nodes are namespace-isolated and persist seamlessly across turn boundaries.

### Interactive Architecture Diagrams

The diagramming subsystem transitioned from legacy monolithic files to an authoritative multi-file project format (`project.json`, `entities.json`, `relations.json`, `text.ru.json`, and views). The visualizer interface introduces a dedicated Filters panel (perspectives, multi-select tag chips with count indicators, structural edge visibility toggles), always-visible interactive save indicators, and in-place contextual confirmation dialogs preventing loss of unsaved changes when loading models.
