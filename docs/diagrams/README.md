# SPLA Architecture Diagrams

The diagram workspace: architecture models and the editor they are built in.
Layout is hand-made only — there is no auto-layout and there will not be one.

Russian version — [`README_RU.md`](README_RU.md).

---

## 📁 What lives here

| Path | What it is |
|---|---|
| [`app/`](app/index.html) | built editor application, served under `/app/` |
| [`catalog.json`](catalog.json) | what appears in the sidebar — **one entry per view** |
| [`styles.json`](styles.json) | shared style library for every project |
| [`projects/`](projects/) | the models themselves: entities, relations, texts, views |
| [`server.go`](server.go), [`run.cmd`](run.cmd) | local server, port `8777` |
| [`mapping/`](mapping/) | leftovers of the v1 generator, kept as reading material only |

The application is built from sources in [`tools/spla-diagram`](../../tools/spla-diagram);
after editing them run `npm run build:app` there.

```bash
docs/diagrams/run.cmd
```

Then open <http://localhost:8777/app/>.

---

## 📐 Where things are specified

| Document | Answers |
|---|---|
| [`tools/spla-diagram/docs/CONTRACT.md`](../../tools/spla-diagram/docs/CONTRACT.md) | what is valid on disk: fields, types, invariants (contract **v3**) |
| [`projects/AGENTS.md`](projects/AGENTS.md) | how to work with it: who writes what, pitfalls, what does not exist yet |
| [`tools/spla-diagram/docs/API.md`](../../tools/spla-diagram/docs/API.md) | what a host must serve and accept |
| [`ADR_20260831`](../adr/ADR_20260831_diagrams_text-provenance-and-view-axes.md) | why the format looks like this |

In short: a project is a directory holding a registry of entities and relations,
a vocabulary of relation types, texts (with per-field provenance) and **views**.
A view stores geometry and declares an **axis** — what nesting a block inside a
rectangle actually asserts.

---

## 🧭 Axes: one model, different questions

The same codebase can be laid out in different ways, and that is not decoration
but a **choice of the question the diagram answers**. In v3 the choice is
explicit: the view's `axis` field. Views on different axes may legitimately put
one node in different containers — that is exactly what axes are for.

### `axis_subsystem` — the semantic atlas

**Question:** "how is the system arranged by meaning — and does everything have a
place?"

1. **Grouping by meaning only.** A container is an architectural role, not a
   namespace, assembly or folder. The namespace stays in `codeRef` as reference.
2. **Completeness is mandatory.** Every entity is present, with no "misc" bucket.
   Completeness is the second point of the diagram: what fits nowhere shows up.
3. **Exactly one place per entity on this axis.** One class — one node — one
   container. Appearing twice on one axis almost always means the class does two
   unrelated things.
4. **Nesting at most two levels deep.** Beyond that the diagram stops reading.
5. **Containers do not overlap**, and a node lies entirely inside its own.

On this axis the diagram reads as an audit: a bloated container is an overloaded
subsystem; an entity that is hard to place is a candidate for rethinking.

**Examples:** `projects/core/`, `projects/full_core/`.

### `axis_turn` — the route

**Question:** "what happens during one turn, from message to answer?"

Completeness is **not** wanted here — only the participants of the route, laid
out along it. A node means a *step*, so one participant may legitimately appear
both on the way out and on the way back.

**Example:** `projects/spla_system/views/v_turn.view.json`.

### `axis_security_zone` — trust boundaries

**Question:** "where does trust end, and which call crosses the border?"

A rectangle is an island; the edge between rectangles is the unit everything is
accounted on — grants and refusals attach to edges, not to participants.

**Example:** `projects/spla_system/views/v_zones.view.json`.

### `axis_process` — where code runs

**Question:** "which process does each node physically execute in?"

Client, service, plugin load context, child process, network.

**Example:** `projects/spla_system/views/v_process.view.json`.

### Anti-pattern — the namespace grid ⛔

A layout where a container is a namespace and nodes are sorted alphabetically
inside it. It looks like a complete map but **answers no architectural
question**: it retells the directory tree, which the IDE already shows.

Mixing it with a meaningful layout in one view is worse still: some classes sit
in the namespace grid, others are pulled into semantic zones, and the reader
cannot tell a complete map from a slice. If both meaning and code structure are
needed, that is **two views on two axes**, not two layers in one.

---

## ⚙️ How the model is kept current

Today — **by hand**, and that is worth knowing up front.

The v1 generator ([`tools/spla-arch`](../../tools/spla-arch/), Go) is out of
service: the `model-*.json` files it produced have been deleted, and the orange
"UNPLACED" zone does not exist in v2/v3 — an entity reaches the canvas only by
being pulled from the registry. Its successor
[`tools/spla-atlas`](../../tools/spla-atlas/) (Roslyn) is **not written**: only
the task description is there.

Which means:

- the registry (`entities.json`, `relations.json`) is edited as text or by a
  one-off script, and every `codeRef` must be **checked against a real file** —
  otherwise the diagram lies from day one;
- texts are edited in the editor or by hand;
- geometry only in the editor, by the owner.

The full list of what the contract promises but the code does not do yet is in
[`projects/AGENTS.md` §5](projects/AGENTS.md).

---

## 🖱 What the editor does

- Pan & zoom, dragging blocks and zones; a zone drags by its header and carries
  its contents, a block re-parents when dropped into a zone.
- Inspector: name, kind, `codeRef`, description and `doc`, editable in place.
- Style panel over the shared `styles.json`.
- Export to `.drawio` — valid XML preserving geometry and metadata.
- Opening an arbitrary JSON by button or drag & drop.

**What it deliberately does not do:** place coordinates. Not on open, not "to
recover". Hand-made layout was lost once to a file reverted to `git HEAD`; the
whole format is built so that this cannot happen again.
