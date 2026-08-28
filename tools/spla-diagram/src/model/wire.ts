import { area, center, containsPoint, containsRect } from "../geometry/rect.js";
import type { Rect } from "../geometry/types.js";
import { DiagramDocument } from "./document.js";
import { DEFAULT_STYLE_IDS, type StyleLibrary } from "./StyleLibrary.js";
import type { WireStyle } from "./style-types.js";
import type {
  DiagramEdge,
  DiagramElement,
  DiagramMetadata,
  DiagramView,
} from "./types.js";
import { elementRect } from "./types.js";
import type {
  WireDocument,
  WireEdge,
  WireNode,
  WireView,
  WireZone,
  WireZoneStyle,
} from "./wire-types.js";

/**
 * The only place that knows the JSON contract.
 *
 * Everything above works with `DiagramDocument`. When the contract changes
 * (docs/CONTRACT_V2.md), this file changes and nothing else does.
 */

const rawViews = new WeakMap<DiagramView, WireView>();

// ------------------------------------------------------------------ parsing

/**
 * @param styles When given, inline zone colours are migrated into it and the
 *   elements come back wearing a `styleId`. When omitted, inline colours are
 *   left alone in `raw` and ride through a save untouched — so a caller with no
 *   style library can still open and re-save a model without damaging it.
 */
export function parseDocument(wire: WireDocument, styles?: StyleLibrary): DiagramDocument {
  const zones = wire.zones ?? [];
  const nodes = wire.nodes ?? [];

  const zoneElements = zones.map((z, i) => zoneToElement(z, i, styles));
  const nodeElements = nodes.map((n, i) => nodeToElement(n, i));

  linkZoneHierarchy(zoneElements);
  linkNodes(nodeElements, zoneElements, nodes);

  const roots = [...zoneElements, ...nodeElements].filter((el) => el.parent === null);

  return new DiagramDocument({
    metadata: parseMetadata(wire),
    views: (wire.views ?? []).map(parseView),
    roots,
    edges: (wire.edges ?? []).map(parseEdge),
    raw: wire as unknown as Record<string, unknown>,
  });
}

function parseMetadata(wire: WireDocument): DiagramMetadata {
  const m = wire.metadata ?? {};
  return { ...m, title: m.title ?? "Схема без названия" };
}

function parseView(w: WireView): DiagramView {
  const view: DiagramView = {
    id: w.id,
    name: w.name ?? w.id,
    icon: w.icon ?? "🔹",
    description: w.description ?? "",
    highlightZones: w.highlightZones ?? [],
    highlightNodes: w.highlightNodes ?? [],
  };
  rawViews.set(view, w);
  return view;
}

function parseEdge(w: WireEdge): DiagramEdge {
  return {
    id: w.id,
    from: w.from,
    to: w.to,
    label: w.label ?? "",
    type: w.type ?? "call",
    ...(w.styleId === undefined ? {} : { styleId: w.styleId }),
  };
}

function zoneToElement(z: WireZone, order: number, styles?: StyleLibrary): DiagramElement {
  const styleId =
    z.styleId ??
    (z.style === undefined || styles === undefined
      ? undefined
      : migrateInlineZoneStyle(z.style, styles, z.name ?? z.id));

  return {
    id: z.id,
    kind: "zone",
    type: z.type ?? "boundary",
    label: z.name ?? z.id,
    ...(z.semanticId === undefined ? {} : { semanticId: z.semanticId }),
    tags: z.tags ?? [],
    metadata: z.metadata ?? {},
    ...(styleId === undefined ? {} : { styleId }),
    x: z.x,
    y: z.y,
    width: z.width,
    height: z.height,
    parent: null,
    children: [],
    wireOrder: order,
    raw: z as unknown as Record<string, unknown>,
  };
}

/**
 * Turn one zone's inline colours into a reference to a named style.
 *
 * Colours identify the style; border width does not. `spla-arch` used to thin a
 * nested zone's border by 0.5 per level, so matching on width too would mint a
 * near-duplicate style for every depth of every theme — on
 * `model-core-full.json` that was 23 styles where 14 are meant. The generator
 * has since stopped varying width by depth (nesting reads from the geometry
 * anyway), so preserving that variance here would only carry the old artefact
 * forward under a new name.
 *
 * A dash pattern does survive, because unlike a half-pixel of border it is a
 * deliberate signal — `zone.unplaced` is dashed precisely to be noticed.
 *
 * Anything matching nothing gets its own style rather than being rounded to the
 * nearest one: a hand-picked colour in a committed model is a decision someone
 * made, and losing it silently on first open is not a migration.
 */
function migrateInlineZoneStyle(
  inline: WireZoneStyle,
  styles: StyleLibrary,
  zoneLabel: string,
): string | undefined {
  const fill = inline.fill;
  const stroke = inline.stroke;
  const headerBg = inline.headerBg;
  if (fill === undefined && stroke === undefined && headerBg === undefined) return undefined;

  const themeId = findByColors(styles, fill, stroke, headerBg);
  const width = inline.strokeWidth;
  const dash = inline.strokeDasharray;

  if (themeId !== null) {
    const theme = styles.resolveBlock(themeId);
    const dashDiffers = dash !== undefined && dash !== "" && dash !== theme.border.dash;
    if (!dashDiffers) return themeId;

    const variantId = `${themeId}.dashed`;
    if (!styles.has(variantId)) {
      styles.put({
        id: variantId,
        name: `${styles.get(themeId)?.name ?? themeId} · пунктир`,
        appliesTo: "block",
        basedOn: themeId,
        border: { dash },
      });
    }
    return variantId;
  }

  // Named after the zone that carried the colours, not after a hex value: this
  // style is about to sit in a list of fifty, and "Импортирован (#faf5ff)"
  // tells the reader nothing about when to reach for it.
  const id = styles.freeId(`zone.${slug(zoneLabel)}`);
  const style: WireStyle = {
    id,
    name: `Зона · ${zoneLabel}`,
    appliesTo: "block",
    // Inherits the container look — radius, no shadow, header height — and
    // overrides only the colours it came for. Without this the migrated zone
    // would fall through to the block fallback instead and quietly turn into a
    // node-shaped box: the same three colours, but the wrong corner radius and
    // a drop shadow no zone has ever had.
    basedOn: DEFAULT_STYLE_IDS.zone,
    description:
      `Создан автоматически из inline-цветов зоны «${zoneLabel}» при переносе ` +
      `раскраски в библиотеку стилей. Переименуйте, если он описывает не одну зону, а роль.`,
    tags: ["импорт"],
    ...(fill === undefined ? {} : { fill }),
    border: {
      ...(stroke === undefined ? {} : { color: stroke }),
      ...(width === undefined ? {} : { width }),
      ...(dash === undefined || dash === "" ? {} : { dash }),
    },
    ...(headerBg === undefined ? {} : { header: { fill: headerBg } }),
  };
  styles.put(style);
  return id;
}

/**
 * The block style whose resolved solid colours match all three given ones.
 *
 * Several styles can share a palette — `default.zone` and `zone.slate` are the
 * same three colours by design — so a plain first-match would migrate a slate
 * zone onto the fallback style and quietly couple it to whatever the fallback
 * later becomes. A named style always wins over a `default.*` one; only if
 * nothing named matches does the fallback answer.
 */
function findByColors(
  styles: StyleLibrary,
  fill: string | undefined,
  stroke: string | undefined,
  headerBg: string | undefined,
): string | null {
  let fallback: string | null = null;
  for (const entry of styles.list("block")) {
    const r = styles.resolveBlock(entry.id);
    if (r.fill.kind !== "solid" || r.header.fill.kind !== "solid") continue;
    if (fill !== undefined && !sameColor(r.fill.color, fill)) continue;
    if (stroke !== undefined && !sameColor(r.border.color, stroke)) continue;
    if (headerBg !== undefined && !sameColor(r.header.fill.color, headerBg)) continue;
    if (entry.id.startsWith("default.")) {
      fallback ??= entry.id;
      continue;
    }
    return entry.id;
  }
  return fallback;
}

function sameColor(a: string, b: string): boolean {
  return a.trim().toLowerCase() === b.trim().toLowerCase();
}

/** A short ascii-ish id fragment from a zone caption, which is usually Russian. */
function slug(label: string): string {
  const cleaned = label
    .toLowerCase()
    .replace(/\(.*?\)/g, "")
    .replace(/[^a-z0-9а-яё]+/gi, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 28);
  return cleaned === "" ? "imported" : cleaned;
}

function nodeToElement(n: WireNode, order: number): DiagramElement {
  return {
    id: n.id,
    kind: "node",
    type: n.type ?? "component",
    label: n.label ?? n.id,
    tags: n.tags ?? [],
    metadata: n.metadata ?? {},
    ...(n.styleId === undefined ? {} : { styleId: n.styleId }),
    x: n.x,
    y: n.y,
    width: n.width,
    height: n.height,
    parent: null,
    children: [],
    wireOrder: order,
    raw: n as unknown as Record<string, unknown>,
  };
}

/**
 * Nest zones inside zones by strict geometric containment (R-CONT-02).
 *
 * The innermost enclosing zone wins, decided by area rather than by array
 * order. Zones with identical rectangles do not nest, which keeps the relation
 * acyclic without a separate cycle check.
 */
function linkZoneHierarchy(zones: DiagramElement[]): void {
  for (const zone of zones) {
    const r = elementRect(zone);
    let best: DiagramElement | null = null;
    for (const candidate of zones) {
      if (candidate === zone) continue;
      const cr = elementRect(candidate);
      if (area(cr) <= area(r)) continue;
      if (!containsRect(cr, r)) continue;
      if (best === null || area(cr) < area(elementRect(best))) best = candidate;
    }
    if (best !== null) {
      zone.parent = best;
      best.children.push(zone);
    }
  }
}

/**
 * Attach nodes to zones.
 *
 * A declared `zone` that names an existing zone wins; otherwise the innermost
 * zone containing the node's centre does (R-CONT-01). Under the old
 * implementation the geometric branch resolved by first match over the zone
 * array, so for nested zones the winner depended on array order — that is D-04,
 * and choosing by area is its fix.
 */
function linkNodes(nodes: DiagramElement[], zones: DiagramElement[], wire: WireNode[]): void {
  const byId = new Map(zones.map((z) => [z.id, z]));

  nodes.forEach((node, i) => {
    const declared = wire[i]?.zone;
    let parent: DiagramElement | null =
      declared === undefined || declared === null ? null : byId.get(declared) ?? null;

    if (parent === null) {
      parent = innermostContaining(zones, center(elementRect(node)));
    }

    if (parent !== null) {
      node.parent = parent;
      parent.children.push(node);
    }

    node.origin = {
      zoneDeclared: wire[i] !== undefined && "zone" in wire[i]!,
      parentId: parent?.id ?? null,
    };
  });
}

function innermostContaining(
  zones: readonly DiagramElement[],
  point: { x: number; y: number },
): DiagramElement | null {
  let best: DiagramElement | null = null;
  let bestArea = Number.POSITIVE_INFINITY;
  for (const zone of zones) {
    const r: Rect = elementRect(zone);
    if (!containsPoint(r, point)) continue;
    const a = area(r);
    if (a < bestArea) {
      best = zone;
      bestArea = a;
    }
  }
  return best;
}

// --------------------------------------------------------------- serializing

export function serializeDocument(doc: DiagramDocument): WireDocument {
  const zones: Array<{ order: number; value: WireZone }> = [];
  const nodes: Array<{ order: number; value: WireNode }> = [];

  for (const el of doc.elements()) {
    if (el.kind === "zone") {
      zones.push({ order: el.wireOrder, value: elementToZone(el) });
    } else {
      nodes.push({ order: el.wireOrder, value: elementToNode(el) });
    }
  }

  const byOrder = (a: { order: number }, b: { order: number }): number => a.order - b.order;
  zones.sort(byOrder);
  nodes.sort(byOrder);

  const out: WireDocument = {
    // Unknown top-level keys ($schema, version, generator stamps) ride along.
    ...doc.raw,
    metadata: { ...doc.metadata },
    views: doc.views.map(serializeView),
    zones: zones.map((z) => z.value),
    nodes: nodes.map((n) => n.value),
    edges: doc.edges.map(serializeEdge),
  };
  return out;
}

function serializeView(view: DiagramView): WireView {
  const raw = rawViews.get(view);
  const out: WireView = { ...(raw ?? {}), id: view.id };
  if (raw?.name !== undefined || view.name !== view.id) out.name = view.name;
  if (raw?.icon !== undefined) out.icon = view.icon;
  if (raw?.description !== undefined) out.description = view.description;
  if (view.highlightZones.length > 0) out.highlightZones = view.highlightZones;
  if (view.highlightNodes.length > 0) out.highlightNodes = view.highlightNodes;
  return out;
}

function serializeEdge(edge: DiagramEdge): WireEdge {
  const out: WireEdge = { id: edge.id, from: edge.from, to: edge.to };
  if (edge.label !== "") out.label = edge.label;
  out.type = edge.type;
  if (edge.styleId !== undefined) out.styleId = edge.styleId;
  return out;
}

function elementToZone(el: DiagramElement): WireZone {
  const raw = (el.raw ?? {}) as Partial<WireZone>;
  const out: WireZone = {
    ...raw,
    id: el.id,
    name: el.label,
    type: el.type,
    x: round(el.x),
    y: round(el.y),
    width: round(el.width),
    height: round(el.height),
  };
  if (el.semanticId !== undefined) out.semanticId = el.semanticId;
  if (el.tags.length > 0) out.tags = el.tags;
  else delete out.tags;

  // Once a zone names a style, its inline colours are gone for good — keeping
  // both would recreate the two-sources-of-truth problem that styles exist to
  // end. A zone that was never migrated (no library at parse time) keeps
  // whatever `raw` carried, so a save is still lossless in that case.
  if (el.styleId !== undefined) {
    out.styleId = el.styleId;
    delete out.style;
  }

  // An element that never carried metadata does not acquire an empty object
  // just by being opened.
  if (Object.keys(el.metadata).length > 0 || "metadata" in raw) out.metadata = el.metadata;
  else delete out.metadata;
  return out;
}

function elementToNode(el: DiagramElement): WireNode {
  const raw = (el.raw ?? {}) as Partial<WireNode>;
  const out: WireNode = {
    ...raw,
    id: el.id,
    label: el.label,
    type: el.type,
    x: round(el.x),
    y: round(el.y),
    width: round(el.width),
    height: round(el.height),
  };
  // Containment is expressed on the wire only for nodes, and only as a hint;
  // zone nesting stays geometric because contract v1 has no field for it.
  //
  // The field is written when the file already had it, or when the parent has
  // actually changed since load. A node that relied on geometry and was never
  // moved keeps relying on it, so opening a model and saving it produces no
  // diff at all.
  const parentId = el.parent !== null && el.parent.kind === "zone" ? el.parent.id : null;
  const origin = el.origin;
  const moved = origin === undefined || origin.parentId !== parentId;
  if (origin?.zoneDeclared === true || moved) {
    out.zone = parentId;
  } else {
    delete out.zone;
  }

  if (el.tags.length > 0) out.tags = el.tags;
  else delete out.tags;
  if (el.styleId !== undefined) out.styleId = el.styleId;
  else delete out.styleId;
  // An element that never carried metadata does not acquire an empty object
  // just by being opened.
  if (Object.keys(el.metadata).length > 0 || "metadata" in raw) out.metadata = el.metadata;
  else delete out.metadata;
  return out;
}

function round(n: number): number {
  return Math.round(n * 1000) / 1000;
}
