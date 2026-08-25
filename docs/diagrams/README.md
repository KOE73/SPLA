# SPLA Semantic Architecture Diagram (MVP)

An interactive architecture diagram visualizer and editor without unpredictable auto-layout, based on the **Semantic Diagram JSON → SVG/HTML Canvas + Draw.io XML Export** workflow.

## 📁 Prototype Files
1. **[`index.html`](file:///C:/GitKOE/SPLA/docs/diagram-mvp/index.html)** — Standalone interactive visualizer web application. Open directly in any modern browser (double click or drag-and-drop).
2. **[`model.json`](file:///C:/GitKOE/SPLA/docs/diagram-mvp/model.json)** (and `schemas/*.json`) — Semantic JSON models representing system architecture, subsystems, metadata, and spatial coordinates.
3. **[`README_RU.md`](file:///C:/GitKOE/SPLA/docs/diagram-mvp/README_RU.md)** — Russian version of this documentation.

---

## 🚀 Key Features

### 1. Strict Coordinate Control (Zero Auto-Layout)
- Every component and zone has deterministic coordinates (`x`, `y`, `width`, `height`).
- Modifying labels, metadata, or relationships **never breaks** or reshuffles the spatial layout.

### 2. Semantic Rectangles (Zones & Containers)
- Zones (e.g. *Client Layer*, *Core & Runtime Subsystem*, *Tool Dispatch & MCP Host*, *Storage & Persistence*) are first-class semantic objects with `type`, `semanticId`, and `tags`, rather than decorative background shapes.

### 3. Dynamic Multi-Schema & Data-Driven Views
- **Schema Switcher**: Switch seamlessly between different diagrams (e.g. *SPLA System Architecture*, *LLM Pipeline & Composition*, *Plugins & Tools*).
- **Data-Driven View Buttons**: View preset buttons in the top navigation are generated dynamically from the active JSON's `"views"` definition.
- **Open Local File**: Easily load any external `.json` diagram model via drag-and-drop or file picker.

### 4. Rich Object Inspector (Right Sidebar)
- Clicking any node or zone opens an inspector panel displaying:
  - Entity name and type badge
  - Semantic ID (`zone.core.runtime`, etc.)
  - Codebase references (`codeRef`), e.g., `src/SPLA.Core/Runtime/ChatTurnPump.cs`
  - Responsibilities list
  - Semantic tags

### 5. Smooth Navigation
- Intuitive Pan & Zoom (mouse wheel zoom, drag canvas, fit-to-screen, and reset controls).

### 6. Bidirectional Export & Interoperability
- **Export to `.drawio`**: Generates valid Diagrams.net XML with complete geometry and custom object metadata (`<Object as="data">`). Open in draw.io / Visio for manual refinement.
- **Live JSON Editor**: View and modify the underlying schema on the fly with live canvas re-rendering.

---

## 🧭 Layout Variants

The same codebase can be laid out on the canvas in different ways. A layout is not decoration — it is a **chosen point of view** that decides which question the diagram answers. Each model declares its variant in `metadata.layout`.

### Variant A — Semantic Atlas (`layout: "semantic-atlas"`)

**Answers:** *how is the system organized by meaning — and is everything actually in its place?*

Rules:

1. **Grouping by meaning only.** A container is an architectural role (LLM Pipeline, Tool Pipeline, Security & Capability, Agent Memory…), never a namespace, assembly, or folder. The namespace survives in `metadata.codeRef` as a reference, but never drives placement.
2. **Completeness is mandatory.** Every entity in the codebase appears — no exceptions, no "misc" bucket. Completeness is what gives the diagram its second meaning: you can see whether everything found a home.
3. **Exactly one place per entity.** One class, one node, one container. A class appearing twice is forbidden: it breaks the diagram's premise and almost always means the class does two unrelated things and should be split in code.
4. **Nested containers.** A major role (`type: "boundary"`) holds sub-roles (`type: "component"`), which hold nodes. Depth stops at two levels — beyond that the diagram stops being readable.
5. **Dense grid inside a container.** Within a sub-role, nodes are laid out alphabetically in a grid: ordering carries no meaning there, membership does.
6. **Containers never overlap.** Boundary boxes stay disjoint, and every node lies fully inside its own container.

**Diagnostic value.** Read as an audit: a bloated container means an overloaded subsystem; an entity hard to assign to any role is a candidate for rethinking; a class pulled toward two roles is a candidate for splitting.

**Example:** [`model-core-full.json`](model-core-full.json) — 246 entities from `src/core`, 13 semantic boundaries, 17 nested sub-roles.

### Variant B — Message Flow (`layout: "message-flow"`)

**Answers:** *what happens to a single message from user input to reply?*

Completeness is **not** required — it is actively harmful. Only the participants along the route appear, ordered left to right along the path; everything else is hidden. Edges are numbered by step. The same participant may appear on both the outbound and return leg — legitimate here, because a node means *a step in the route*, not *a class*.

**Example:** [`model-llm-pipeline.json`](model-llm-pipeline.json).

### Variant C — Subsystem Overview (`layout: "subsystem-overview"`)

**Answers:** *which large parts make up the system, and who talks to whom?*

A node is an entire subsystem, not a class. Dozens of classes collapse into one box. This is the variant for explaining the system to someone seeing it for the first time.

**Example:** [`model.json`](model.json), [`model-core.json`](model-core.json).

---

## ⚙️ How the atlas stays current

The atlas is not drawn by hand — it is **generated** by
[`tools/spla-arch`](../../tools/spla-arch/AGENTS.md) from three inputs:

```text
   discovered entities        rules for one point of view        json schema
   (extracted from code)  +   (mapping/<variant>/*.map.json)  +  (model-*.json)
                                        │
                                        ▼
                             deterministic verification
                                        │
                  ┌─────────────────────┼─────────────────────┐
                  ▼                     ▼                     ▼
               broken                 extra                missing
                  └─────────────────────┴─────────────────────┘
                                        │
                                        ▼
                       agent fixes the RULES or the SCHEMA
```

The key split: **rules are hand-authored and durable, coordinates are derived
and disposable.** The rules file is essentially a cache of decisions already
made about meaning, so the classification is not reinvented on every run. Rule
sets are per point of view: `semantic-atlas` has one set, `message-flow`
another — there is no shared set, because they group by different criteria.

New code places itself for as long as the rules can manage it:

1. name rule → 2. regex → 3. path prefix (longest wins) → 4. **file-mate
inheritance** (one file is one unit of meaning) → 5. if nothing claims it, into
the orange `UNPLACED` zone on the canvas.

That orange zone is the agent's work queue. A lockfile `<id>.known.json` keeps
a snapshot of the previous run, so the tool can tell "new and unclaimed" from
"new and auto-placed — check whether that is right".

```bash
go run . build --mapping ../../docs/diagrams/mapping/semantic-atlas/core.map.json --repo ../..
```

```bash
go run . verify --dir ../../docs/diagrams --repo ../..
```

`verify` writes nothing — it only answers whether code, rules, and schema still
agree. It demands completeness **only** from `semantic-atlas`: for a
`message-flow`, incompleteness is the design, not a defect.

### Anti-variant — Namespace Grid ⛔

A layout where the container is a namespace and nodes inside are alphabetical. It looks like a complete map but **answers no architectural question** — it restates the directory tree already visible in the IDE. It is especially harmful when mixed with a meaningful layout in one file: some classes sit in the namespace grid, others are pulled out into semantic zones, and the reader cannot tell whether they are looking at a full map or a slice.

If both meaning and code structure are needed, that is **two separate files**, not two layers in one.
