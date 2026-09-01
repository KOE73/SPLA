# SPLA.Plugins.Documents — the native document backend

Read the parent [`src/plugins/AGENTS.md`](../AGENTS.md) first. The decision this plugin implements
is [`ADR_20260824_plugins_document-context`](../../../docs/adr/ADR_20260824_plugins_document-context.md);
the open work is in
[`PLAN_20260824_plugins_document-context`](../../../docs/plans/PLAN_20260824_plugins_document-context.md).

## The one rule: Context, not Artifact

Everything here answers **"what does this document say"**. Nothing here answers "what does it look
like".

- **Never add** a tool or a parameter that sets or reads a font, colour, size, alignment, border,
  merged cell, column width, print area or formula. That is the Artifact/Layout class, it needs its
  own API, and mixing it in produces an interface where half the arguments are always empty.
- **Never widen** a spreadsheet write beyond "append rows keyed by column header". No cell
  addresses, no "rewrite the sheet", no adding a column to an existing sheet — a sheet belongs to a
  person who did not ask for it to be restructured.
- **Never carry image bytes** into `ContextDocument`. Images are references; a document with forty
  screenshots must not turn one call into forty megabytes.

## Boundaries

- `SPLA.Documents.Model` is referenced, **not** shared with the host, and must never be added to
  `PluginLoadContext.SharedAssemblies`. That is safe only while no instance of its types crosses the
  plugin boundary — bytes plus a MIME type do. Keep it that way.
- Files are reached through `HostServices.Sandbox.Workspace` (see `Tools/DocumentsToolPaths.cs`).
  ClosedXML needs a real path, which comes from `MapPathToHost`; a workspace that refuses to map is
  a refusal, never a reason to touch `System.IO` directly.
- A second backend (pandoc, Aspose) is a **separate plugin folder** registering the same pairs, not
  a branch inside this one. Two backends claiming one pair is still an open question — see ADR §4.1.
