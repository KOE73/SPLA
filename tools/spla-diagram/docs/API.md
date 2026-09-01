# SPLA Diagram Host & Backend API Specification

This document defines the host/backend communication contract for the SPLA Architecture Visualizer & Editor (`@spla/diagram`). Any backend implementation (Go server, Node.js server, VSCode extension WebView, Electron, CLI embedded server, Cloud) must fulfill these requirements to ensure full compatibility for both reading and writing models.

**Scope.** This file covers transport only — which paths a host serves and accepts. The *shape* of every file listed here is normative in [`CONTRACT.md`](CONTRACT.md) (model contract **v3**); the workflow around them is in [`docs/diagrams/projects/AGENTS.md`](../../../docs/diagrams/projects/AGENTS.md). A host never needs to understand the contents — it moves bytes.

---

## 1. Architectural Overview

The editor operates over a project workspace containing:
1. **Catalog**: `catalog.json` listing available projects/views.
2. **Global Styles**: `styles.json` defining color schemes, strokes, typography, and badges.
3. **Projects**: `projects/<project_id>/` directories holding:
   - `project.json` — Project manifest
   - `entities.json` — Catalog of code entities / types
   - `relations.json` — Catalog of code relations & dependencies
   - `relation-types.json` — Authored vocabulary of relation types
   - `containers.json` — Container membership rules
   - `text.<lang>.json` — Localized texts with per-field provenance
   - `views/<view_id>.view.json` — Classification axis, geometry, zones, node placements

```
Workspace Root (e.g. docs/diagrams/)
├── app/                      Editor web application bundle
├── catalog.json              Diagram / view registry — one entry per view
├── styles.json               Shared style library
└── projects/
    └── <project_id>/
        ├── project.json
        ├── entities.json
        ├── relations.json
        ├── relation-types.json
        ├── containers.json
        ├── text.ru.json
        └── views/
            └── <view_id>.view.json
```

---

## 2. Read Requirements (Static / GET)

The backend must serve static JSON documents and application assets over HTTP or an equivalent virtual transport.

### 2.1. Endpoints & Paths

| Path / URL | Method | Content-Type | Description |
|---|---|---|---|
| `/app/` | `GET` | `text/html` | Entry point for the editor application |
| `/app/assets/*` | `GET` | `application/javascript`, `text/css` | Bundled JavaScript, CSS, and media |
| `/catalog.json` | `GET` | `application/json` | Catalog of available diagrams and views |
| `/styles.json` | `GET` | `application/json` | Shared style stylesheet (returns 404 if not created yet; client falls back to built-in styles) |
| `/projects/<project_id>/project.json` | `GET` | `application/json` | Project manifest |
| `/projects/<project_id>/entities.json` | `GET` | `application/json` | Catalog of all entities |
| `/projects/<project_id>/relations.json` | `GET` | `application/json` | Catalog of all relations |
| `/projects/<project_id>/relation-types.json` | `GET` | `application/json` | Relation type vocabulary (404 tolerated: the client falls back to an empty vocabulary) |
| `/projects/<project_id>/text.<lang>.json` | `GET` | `application/json` | Localized strings (e.g. `text.ru.json`) |
| `/projects/<project_id>/views/<view_id>.view.json` | `GET` | `application/json` | Axis, placement & geometry layout |

`containers.json` is part of the project on disk but is **not requested by the current client** — a host still serves it like any other file.

### 2.2. Base URL Handling
- When served from `/app/`, relative model paths resolve relative to `../` (workspace root).
- In development mode (Vite dev server), model paths resolve relative to `./` (mounted public root).

### 2.3. Caching

Model files are edited on disk while the editor is open, so a host **MUST NOT** let the browser reuse a cached copy without revalidating: serve every `.json` with `Cache-Control: no-cache` (or an equivalent validator). A stale `entities.json` does not fail loudly — the diagram simply renders raw ids in place of names, which reads as a broken model rather than a caching problem. Application assets under `/app/assets/*` are content-hashed and may be cached normally.

---

## 3. Write Requirements (Save API)

The backend must provide a mechanism to persist updated JSON files back to the workspace.

### 3.1. Save Endpoint: `POST /api/save`

- **URL Pattern**: `/api/save?file=<relative_path>`
- **Method**: `POST`
- **Headers**: `Content-Type: application/json`
- **Request Body**: Valid JSON payload formatted with 2-space indentation.

#### Query Parameter `file`
- Contains a workspace-relative path (e.g. `projects/llm_pipeline/views/v_main.view.json`, `styles.json`, `catalog.json`).
- Path separators may be `/` (standardized by client) or `\` (Windows).

### 3.2. Response Status Codes

| HTTP Status | Condition | Body / Reason |
|---|---|---|
| `200 OK` | File successfully written to disk | `OK` or `{ "status": "ok" }` |
| `400 Bad Request` | Missing `file` param, invalid extension, or directory traversal attempt | Error message string |
| `405 Method Not Allowed` | Method is not `POST` | `Method not allowed` |
| `500 Internal Server Error` | File system I/O error or permission failure | Error message string |

### 3.3. Security & Path Traversal Rules
1. **Canonicalization**: The server must clean the requested path (e.g. via `filepath.Clean`).
2. **Directory Traversal Protection**: Paths starting with `..`, containing `../` or `..\`, or resolving outside the workspace root **MUST** be rejected with HTTP 400.
3. **Absolute Path Protection**: Absolute paths (e.g. `/etc/passwd`, `C:\Windows\...`) **MUST** be rejected with HTTP 400.
4. **Extension Whitelist**: Only `.json` files are permitted to be written via `/api/save`.
5. **Auto Directory Creation**: If parent subdirectories do not exist (e.g. `projects/<project_id>/views/`), the server **MUST** create them automatically before writing the file.

### 3.4. What the editor actually writes

One user-initiated save issues up to three `POST /api/save` calls, in this order:

| File | When |
|---|---|
| `views/<view_id>.view.json` | always |
| `projects/<project_id>/entities.json` | only if the canvas gained a block absent from the registry (appended with `origin: "authored"`) |
| `projects/<project_id>/text.<lang>.json` | only if a name or description changed; values the user edited are re-stamped `authored`, untouched ones keep their loaded provenance, so an idle save produces an empty diff |

`relations.json`, `relation-types.json`, `containers.json` and `project.json` are **never written by the editor**. A host that makes those files read-only loses nothing.

---

## 4. Alternative Host Adapters (VSCode / Electron / Node)

When embedding `@spla/diagram` in non-HTTP hosts (e.g., VSCode Extension WebView, Electron IPC):

1. **Implement `ModelStore`**:
   ```typescript
   export interface ModelStore {
     load(file: string): Promise<WireDocument>;
     save(target: SaveTarget, wire: WireDocument): Promise<void>;
   }
   ```
2. **Implement `StyleStore`**:
   ```typescript
   export interface StyleStore {
     load(): Promise<WireStyleSheet>;
     save(sheet: WireStyleSheet): Promise<void>;
   }
   ```
3. **Implement `CatalogStore`**:
   ```typescript
   export interface CatalogStore {
     load(): Promise<CatalogEntry[]>;
     save(catalog: CatalogEntry[]): Promise<void>;
   }
   ```

Any host fulfilling these three interfaces will provide complete visualizer and editor functionality without requiring a running Go HTTP server.
