# Summary — unreleased

<!-- covers: 2026-08-29 -->

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

### Interactive Architecture Diagrams & Workbench Architecture

The diagramming subsystem transitioned from legacy monolithic files to an authoritative multi-file project format (`project.json`, `entities.json`, `relations.json`, `text.<lang>.json`, and views). The editor migrated to a CAD/IDE-grade Workbench architecture featuring a compact Ribbon command surface with keytips/tooltips, a centralized command system, and a fullscreen Dockview grid. The central diagram canvas is pinned as a headerless workspace, while tool panels (Catalog, Properties, Relations, Filters, Styles, Base Entities) can be docked anywhere (left, right, bottom, top), grouped into tab sets, or detached as floating windows with automatic layout persistence. Potential and shadow relations (ghost edges) are distributed uniformly across boundary ports alongside active connections, preventing overlapping lines when exploring node relationships.
