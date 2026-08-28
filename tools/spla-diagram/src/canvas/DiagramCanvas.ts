import { elementRect, isContainer } from "../model/types.js";
import type { DiagramEdge, DiagramElement } from "../model/types.js";
import type { DiagramDocument } from "../model/document.js";
import { StyleLibrary } from "../model/StyleLibrary.js";
import { builtinStyleSheet } from "../model/style-defaults.js";
import { PaintRegistry } from "./render/PaintRegistry.js";
import type { Point, Rect } from "../geometry/types.js";
import { Emitter } from "../util/emitter.js";
import { createDefs } from "./defs.js";
import { clear, setAttrs, svg, text } from "./svg.js";
import { Viewport, type ViewportState } from "./Viewport.js";
import { BoxRenderer } from "./render/BoxRenderer.js";
import { ContainerRenderer } from "./render/ContainerRenderer.js";
import type { ElementRenderer, RenderContext } from "./render/ElementRenderer.js";
import { TypeRegistry } from "./render/TypeRegistry.js";
import { DIM } from "./render/styles.js";
import { dashArray, textAttrs } from "./render/textAttrs.js";
import { marquee, resizeHandles, selectionOutline } from "./render/handles.js";
import { CenterPortAssigner } from "./ports/assigners.js";
import { portKey, type PortAssigner, type PortRequest } from "./ports/PortAssigner.js";
import { BezierRouter, type EdgeRouter } from "./routing/EdgeRouter.js";
import { EDGE_ATTR } from "../interaction/roles.js";
import { InteractionController } from "../interaction/InteractionController.js";

export type SelectionKind = "zone" | "node" | "edge";

/** Named here so a host can build a toggle without importing the style module. */
export type EdgeFamily = "structure" | "flow";

export interface Selection {
  readonly id: string;
  readonly kind: SelectionKind;
}

export interface CanvasEvents {
  /** Selection changed, including to nothing. */
  select: Selection | null;
  /** The model was mutated by a canvas interaction. */
  modelchange: { reason: string };
  /** A direct-manipulation gesture began; a good moment to snapshot. */
  gesturestart: { reason: string };
  /** A gesture finished. */
  gestureend: { reason: string };
  viewport: ViewportState;
  collapse: { id: string; collapsed: boolean };
}

export interface DiagramCanvasOptions {
  registry?: TypeRegistry;
  portAssigner?: PortAssigner;
  router?: EdgeRouter;
  /** Grid step for snapping, 0 disables. */
  gridStep?: number;
  /** The look of everything. Defaults to the built-in library. */
  styles?: StyleLibrary;
}

/**
 * The reusable half of the library: everything needed to show a diagram and
 * manipulate it directly, and nothing about where the model came from or where
 * it is saved.
 *
 * Knows nothing about toolbars, inspectors, catalogs, `/api/save` or undo. It
 * reports what happened through events and lets a host decide what that means.
 */
export class DiagramCanvas {
  readonly events = new Emitter<CanvasEvents>();
  readonly viewport: Viewport;
  readonly registry: TypeRegistry;

  /** Snapping step used by interactions, in model units. */
  gridStep: number;
  /** Whether dragging a container carries its contents (R-EDIT-03). */
  containerDrag = true;

  /**
   * Families of edge currently *not* drawn. Empty means everything shows.
   *
   * In `model-core-full.json` 98 of 119 edges are `implements`/`extends`. That
   * thicket makes the canvas unreadable no matter how the lines are coloured —
   * the only thing that helps is being able to switch the structural family off
   * and look at what happens at runtime.
   */
  hiddenEdgeFamilies = new Set<EdgeFamily>();

  private readonly host: HTMLElement;
  private readonly svgEl: SVGSVGElement;
  private readonly viewportGroup: SVGGElement;
  private readonly zonesLayer: SVGGElement;
  private readonly edgesLayer: SVGGElement;
  private readonly nodesLayer: SVGGElement;
  private readonly overlayLayer: SVGGElement;

  private readonly interaction: InteractionController;

  private doc: DiagramDocument | null = null;
  private styleLibrary: StyleLibrary;
  private paintRegistry!: PaintRegistry;
  private selection: Selection | null = null;
  /**
   * Everything currently selected, including the primary. Group move and group
   * resize operate on this set; the inspector still follows `selection`, since
   * editing many elements at once is a different feature.
   */
  private selectionIds = new Set<string>();
  private collapsed = new Set<string>();
  private activeViewId: string | null = null;
  /**
   * Style tags currently isolated. Empty means no isolation.
   *
   * Not a second highlighting system: a view's `highlightZones`/`highlightNodes`
   * name elements one by one, by hand. A tag names a *domain* — it lives on the
   * style the elements already wear (`WireStyle.tags`), so isolating "llm" dims
   * everything except what a style already colours as belonging to it. No
   * separate per-element tag field exists or is needed; the style a thing wears
   * already says what it is.
   *
   * A set, not one tag: a style can carry several tags at once (`WireStyle.tags`
   * always was an array), because the same subdomain often belongs to more than
   * one classification worth isolating separately — a security-relevant zone
   * inside the LLM domain is both "llm" and "security". Selecting several tags
   * shows the union: anything wearing *any* of them.
   */
  private activeTags = new Set<string>();

  /** Rubber band in model coordinates while a selection sweep is running. */
  marqueeRect: Rect | null = null;

  private portAssigner: PortAssigner;
  private router: EdgeRouter;

  /** Set while a node is dragged over a container. */
  dropTargetId: string | null = null;

  constructor(host: HTMLElement, options: DiagramCanvasOptions = {}) {
    this.host = host;
    this.gridStep = options.gridStep ?? 10;
    this.portAssigner = options.portAssigner ?? new CenterPortAssigner();
    this.router = options.router ?? new BezierRouter();
    this.registry = options.registry ?? defaultRegistry();

    this.zonesLayer = svg("g", { class: "spla-layer-zones" });
    this.edgesLayer = svg("g", { class: "spla-layer-edges" });
    this.nodesLayer = svg("g", { class: "spla-layer-nodes" });
    this.overlayLayer = svg("g", { class: "spla-layer-overlay" });

    this.viewportGroup = svg("g", { class: "spla-viewport" }, [
      this.zonesLayer,
      this.edgesLayer,
      this.nodesLayer,
      this.overlayLayer,
    ]);

    // Held rather than inlined: gradients and arrow heads are created on demand
    // as styles ask for them, so something has to own the block they land in.
    const defs = createDefs();
    this.paintRegistry = new PaintRegistry(defs);
    this.styleLibrary =
      options.styles ?? StyleLibrary.parse(builtinStyleSheet());

    this.svgEl = svg("svg", { class: "spla-canvas", xmlns: "http://www.w3.org/2000/svg" }, [
      defs,
      this.viewportGroup,
    ]);

    host.classList.add("spla-canvas-host");
    host.appendChild(this.svgEl);

    this.viewport = new Viewport(this.viewportGroup);
    this.viewport.changed.on("change", (state) => this.events.emit("viewport", state));

    this.interaction = new InteractionController(this, host);
  }

  // ------------------------------------------------------------ public API

  setModel(doc: DiagramDocument): void {
    this.doc = doc;
    this.selection = null;
    this.selectionIds.clear();
    this.collapsed = new Set();
    this.activeViewId = doc.views[0]?.id ?? null;
    this.render();
    this.fit();
    this.events.emit("select", null);
  }

  /**
   * Swap in a rebuilt model while keeping the presentation state: viewport,
   * collapse, active view, and the selection re-resolved by id.
   *
   * This is what undo needs — the model is rebuilt from a snapshot, but the
   * user must not be thrown back to a different zoom or lose their place
   * (R-HIST-07).
   */
  replaceModel(doc: DiagramDocument): void {
    this.doc = doc;
    const alive = (id: string): boolean =>
      doc.element(id) !== undefined || doc.edge(id) !== undefined;

    for (const id of [...this.selectionIds]) {
      if (!alive(id)) this.selectionIds.delete(id);
    }
    if (this.selection !== null && !alive(this.selection.id)) {
      this.selection = this.resolveSelection([...this.selectionIds].at(-1) ?? null);
    }
    this.render();
    this.events.emit("select", this.selection);
  }

  get model(): DiagramDocument | null {
    return this.doc;
  }

  /** The library every element's look is resolved through. */
  get styles(): StyleLibrary {
    return this.styleLibrary;
  }

  /**
   * Swap the library, or redraw after editing a style inside it.
   *
   * Call this after any style edit: a style is shared by everything wearing it,
   * so there is no such thing as repainting one element — which is exactly the
   * property that made styles worth building.
   */
  setStyles(library?: StyleLibrary): void {
    if (library !== undefined) this.styleLibrary = library;
    this.render();
  }

  /** Show or hide a whole family of connections, and redraw. */
  setEdgeFamilyHidden(family: EdgeFamily, hidden: boolean): void {
    if (hidden) this.hiddenEdgeFamilies.add(family);
    else this.hiddenEdgeFamilies.delete(family);
    this.render();
  }

  get activeView(): string | null {
    return this.activeViewId;
  }

  setView(viewId: string | null): void {
    this.activeViewId = viewId;
    this.render();
  }

  /** Tags currently isolated. Empty means no isolation. */
  get highlightTags(): ReadonlySet<string> {
    return this.activeTags;
  }

  /** Add or remove one tag from the isolated set, keeping the rest. */
  toggleHighlightTag(tag: string): void {
    if (this.activeTags.has(tag)) this.activeTags.delete(tag);
    else this.activeTags.add(tag);
    this.render();
  }

  clearHighlightTags(): void {
    if (this.activeTags.size === 0) return;
    this.activeTags.clear();
    this.render();
  }

  /**
   * Every tag worn by a style currently in use, sorted.
   *
   * "In use" — not every tag in the library — because a hundred styles will
   * carry tags for domains this particular diagram never touches, and a picker
   * offering those is a picker offering dead ends.
   */
  tagsInUse(): string[] {
    if (this.doc === null) return [];
    const tags = new Set<string>();
    for (const el of this.doc.elements()) {
      for (const t of this.styleLibrary.tagsOf(this.styleLibrary.blockStyleIdFor(el))) tags.add(t);
    }
    for (const edge of this.doc.edges) {
      for (const t of this.styleLibrary.tagsOf(this.styleLibrary.edgeStyleIdFor(edge))) tags.add(t);
    }
    return [...tags].sort((a, b) => a.localeCompare(b, "ru"));
  }

  select(id: string | null): void {
    this.selection = this.resolveSelection(id);
    this.selectionIds = this.selection === null ? new Set() : new Set([this.selection.id]);
    this.render();
    this.events.emit("select", this.selection);
  }

  /**
   * Add or remove one element from the selection, keeping the rest.
   *
   * The last element added becomes primary, so the inspector follows what the
   * user just touched.
   */
  toggleSelected(id: string): void {
    if (this.selectionIds.has(id)) {
      this.selectionIds.delete(id);
      if (this.selection?.id === id) {
        const next = [...this.selectionIds].at(-1) ?? null;
        this.selection = this.resolveSelection(next);
      }
    } else {
      this.selectionIds.add(id);
      this.selection = this.resolveSelection(id);
    }
    this.render();
    this.events.emit("select", this.selection);
  }

  /** Replace the whole selection at once, as a marquee sweep does. */
  selectMany(ids: readonly string[]): void {
    this.selectionIds = new Set(ids);
    this.selection = this.resolveSelection(ids.at(-1) ?? null);
    this.render();
    this.events.emit("select", this.selection);
  }

  private resolveSelection(id: string | null): Selection | null {
    if (id === null || this.doc === null) return null;
    const el = this.doc.element(id);
    if (el !== undefined) return { id, kind: el.kind };
    if (this.doc.edge(id) !== undefined) return { id, kind: "edge" };
    return null;
  }

  get selected(): Selection | null {
    return this.selection;
  }

  /** Ids of every selected element, primary included. */
  get selectedIds(): ReadonlySet<string> {
    return this.selectionIds;
  }

  /** The selected elements, skipping edges and anything already gone. */
  selectedElements(): DiagramElement[] {
    const doc = this.doc;
    if (doc === null) return [];
    const out: DiagramElement[] = [];
    for (const id of this.selectionIds) {
      const el = doc.element(id);
      if (el !== undefined) out.push(el);
    }
    return out;
  }

  /** Union of the visible rectangles of the selected elements. */
  selectionBounds(): Rect | null {
    const elements = this.selectedElements();
    if (elements.length === 0 || this.doc === null) return null;
    const ctx = this.context();
    let out: Rect | null = null;
    for (const el of elements) {
      if (ctx.isHidden(el)) continue;
      const r = this.registry.resolve(el).visibleRect(el, ctx);
      out = out === null ? r : unionRect(out, r);
    }
    return out;
  }

  selectedElement(): DiagramElement | null {
    if (this.selection === null || this.selection.kind === "edge") return null;
    return this.doc?.element(this.selection.id) ?? null;
  }

  selectedEdge(): DiagramEdge | null {
    if (this.selection === null || this.selection.kind !== "edge") return null;
    return this.doc?.edge(this.selection.id) ?? null;
  }

  isCollapsed(el: DiagramElement): boolean {
    return this.collapsed.has(el.id);
  }

  toggleCollapse(id: string): void {
    if (this.collapsed.has(id)) this.collapsed.delete(id);
    else this.collapsed.add(id);
    this.render();
    this.events.emit("collapse", { id, collapsed: this.collapsed.has(id) });
  }

  fit(): void {
    this.viewport.fit(this.visibleBounds(), {
      width: this.host.clientWidth,
      height: this.host.clientHeight,
    });
  }

  zoomTo(zoom: number): void {
    this.viewport.zoomTo(zoom);
  }

  zoomBy(factor: number): void {
    this.viewport.zoomBy(factor);
  }

  /** Zoom by `factor`, keeping the model point under the given client coordinates fixed. */
  zoomAtClient(factor: number, clientX: number, clientY: number): void {
    const box = this.host.getBoundingClientRect();
    this.viewport.zoomAt({ x: clientX - box.left, y: clientY - box.top }, factor);
  }

  resetZoom(): void {
    this.viewport.reset();
  }

  setPortAssigner(assigner: PortAssigner): void {
    this.portAssigner = assigner;
    this.render();
  }

  setRouter(router: EdgeRouter): void {
    this.router = router;
    this.render();
  }

  /** Screen point (client coordinates) to model coordinates. */
  toModel(clientX: number, clientY: number): Point {
    const box = this.host.getBoundingClientRect();
    return this.viewport.toModel({ x: clientX - box.left, y: clientY - box.top });
  }

  /** The centre of the visible area, in model coordinates. */
  viewCenter(): Point {
    return this.viewport.toModel({
      x: this.host.clientWidth / 2,
      y: this.host.clientHeight / 2,
    });
  }

  notifyModelChanged(reason: string): void {
    this.render();
    this.events.emit("modelchange", { reason });
  }

  destroy(): void {
    this.interaction.destroy();
    this.svgEl.remove();
    this.events.clear();
  }

  // -------------------------------------------------------------- rendering

  render(): void {
    clear(this.zonesLayer);
    clear(this.edgesLayer);
    clear(this.nodesLayer);
    clear(this.overlayLayer);
    if (this.doc === null) return;

    const ctx = this.context();

    // Parents before children, so nested containers stack above their parent.
    for (const el of this.doc.elements()) {
      if (ctx.isHidden(el)) continue;
      const renderer = this.registry.resolve(el);
      const layer = isContainer(el) ? this.zonesLayer : this.nodesLayer;
      layer.appendChild(renderer.create(el, ctx));

      const overlay = renderer.overlay?.(el, ctx) ?? null;
      if (overlay !== null) this.overlayLayer.appendChild(overlay);
    }

    this.renderEdges(ctx);
    this.renderSelectionOverlay();
  }

  /**
   * Grips and outlines, drawn above everything.
   *
   * One element selected: grips sit on the element. Several: they sit on the
   * union of their boxes, with a dashed outline, and dragging one scales the
   * whole group.
   */
  private renderSelectionOverlay(): void {
    const scale = this.viewport.zoom;

    if (this.marqueeRect !== null) {
      this.overlayLayer.appendChild(marquee(this.marqueeRect, scale));
    }

    const elements = this.selectedElements();
    if (elements.length === 0) return;

    const bounds = this.selectionBounds();
    if (bounds === null) return;

    if (elements.length > 1) {
      this.overlayLayer.appendChild(selectionOutline(bounds, scale));
    } else if (this.collapsed.has(elements[0]!.id)) {
      // A collapsed container shows no grips: resizing it would change a height
      // that is not currently visible.
      return;
    }

    this.overlayLayer.appendChild(resizeHandles(bounds, scale));
  }

  private context(): RenderContext {
    const doc = this.doc;
    if (doc === null) throw new Error("render context requested with no model");
    const view = this.activeViewId === null
      ? undefined
      : doc.views.find((v) => v.id === this.activeViewId);

    return {
      doc,
      selectedId: this.selection?.id ?? null,
      dropTargetId: this.dropTargetId,
      isSelected: (el) => this.selectionIds.has(el.id),
      isCollapsed: (el) => this.collapsed.has(el.id),
      isHidden: (el) => this.collapsedAncestor(el) !== null,
      opacity: (el) => {
        const viewOpacity = ((): number => {
          if (view === undefined) return 1;
          if (isContainer(el)) {
            if (view.highlightZones.length === 0) return 1;
            return view.highlightZones.includes(el.id) ? 1 : DIM.zone;
          }
          if (view.highlightNodes.length > 0) {
            return view.highlightNodes.includes(el.id) ? 1 : DIM.node;
          }
          if (view.highlightZones.length > 0) {
            const inHighlighted = doc
              .ancestors(el)
              .some((a) => view.highlightZones.includes(a.id));
            return inHighlighted ? 1 : DIM.node;
          }
          return 1;
        })();

        const tagOpacity = ((): number => {
          if (this.activeTags.size === 0) return 1;
          return this.hasAnyDomainTag(el, this.activeTags) ? 1 : isContainer(el) ? DIM.zone : DIM.node;
        })();

        // The dimmer of the two axes wins: a view and a tag can be active at
        // once, and either one asking for "not this" should be enough to grey
        // an element out.
        return Math.min(viewOpacity, tagOpacity);
      },
      styleOf: (el) => this.styleLibrary.blockStyle(el),
      paints: this.paintRegistry,
    };
  }

  /**
   * Whether an element belongs to a tagged domain — its own style, or any
   * ancestor zone's.
   *
   * A domain tag lives on one zone's style, not on every element inside it: a
   * node three levels deep does not carry its own copy of "llm", it belongs to
   * `block.llm` by containment, the same way it belongs to `block.llm`'s
   * colour without carrying that colour itself. Without the ancestor walk, the
   * root zone of a tagged domain would light up and everything nested inside
   * it — its own child zones, every node — would stay dimmed, which is the
   * opposite of what isolating a domain is for.
   */
  private hasAnyDomainTag(el: DiagramElement, tags: ReadonlySet<string>): boolean {
    const own = this.styleLibrary.tagsOf(this.styleLibrary.blockStyleIdFor(el));
    if (own.some((t) => tags.has(t))) return true;
    if (this.doc === null) return false;
    return this.doc.ancestors(el).some((a) =>
      this.styleLibrary.tagsOf(this.styleLibrary.blockStyleIdFor(a)).some((t) => tags.has(t)),
    );
  }

  /**
   * The outermost collapsed ancestor, which is the container actually visible
   * on screen when several nested containers are collapsed at once.
   */
  private collapsedAncestor(el: DiagramElement): DiagramElement | null {
    if (this.doc === null) return null;
    let found: DiagramElement | null = null;
    for (const ancestor of this.doc.ancestors(el)) {
      if (this.collapsed.has(ancestor.id)) found = ancestor;
    }
    return found;
  }

  /** Where an edge end attaches: the element itself, or the collapsed container hiding it. */
  private anchorFor(el: DiagramElement): { owner: DiagramElement; rect: Rect } {
    const hidden = this.collapsedAncestor(el);
    const owner = hidden ?? el;
    const renderer = this.registry.resolve(owner);
    return { owner, rect: renderer.visibleRect(owner, this.context()) };
  }

  private renderEdges(ctx: RenderContext): void {
    const doc = this.doc;
    if (doc === null) return;

    const view = this.activeViewId === null
      ? undefined
      : doc.views.find((v) => v.id === this.activeViewId);

    interface Resolved {
      edge: DiagramEdge;
      from: { owner: DiagramElement; rect: Rect };
      to: { owner: DiagramElement; rect: Rect };
    }

    const resolved: Resolved[] = [];
    for (const edge of doc.edges) {
      // Filtered before ports are assigned, not while drawing: a hidden edge
      // that still claimed a port would push the visible ones off centre for
      // no reason anyone could see.
      if (this.hiddenEdgeFamilies.has(this.styleLibrary.edgeStyle(edge).family)) continue;

      const fromEl = doc.element(edge.from);
      const toEl = doc.element(edge.to);
      // An edge naming a missing element is silently skipped (R-MODEL-05).
      if (fromEl === undefined || toEl === undefined) continue;

      const from = this.anchorFor(fromEl);
      const to = this.anchorFor(toEl);
      // Both ends collapsed into the same container: nothing to show.
      if (from.owner === to.owner) continue;

      resolved.push({ edge, from, to });
    }

    const requests: PortRequest[] = [];
    for (const r of resolved) {
      requests.push({
        edgeId: r.edge.id, end: "from", ownerId: r.from.owner.id,
        ownerRect: r.from.rect, otherRect: r.to.rect, edgeType: r.edge.type,
      });
      requests.push({
        edgeId: r.edge.id, end: "to", ownerId: r.to.owner.id,
        ownerRect: r.to.rect, otherRect: r.from.rect, edgeType: r.edge.type,
      });
    }
    const ports = this.portAssigner.assign(requests);

    for (const r of resolved) {
      const fromSlot = ports.get(portKey(r.edge.id, "from"));
      const toSlot = ports.get(portKey(r.edge.id, "to"));
      if (fromSlot === undefined || toSlot === undefined) continue;

      const fromPoint = this.registry.resolve(r.from.owner).pointAt(r.from.rect, fromSlot);
      const toPoint = this.registry.resolve(r.to.owner).pointAt(r.to.rect, toSlot);

      const route = this.router.route({
        from: fromPoint, to: toPoint,
        fromSide: fromSlot.side, toSide: toSlot.side,
        fromRect: r.from.rect, toRect: r.to.rect,
      });

      const style = this.styleLibrary.edgeStyle(r.edge);
      const viewHighlighted =
        view === undefined || view.highlightNodes.length === 0
          ? true
          : view.highlightNodes.includes(r.edge.from) && view.highlightNodes.includes(r.edge.to);
      // Either end, not both: unlike a view's "focus on this closed subsystem",
      // a tag is a flashlight on one domain — a call crossing its boundary is
      // exactly the kind of thing worth still seeing, not hiding.
      const tagHighlighted =
        this.activeTags.size === 0 ||
        this.hasAnyDomainTag(r.from.owner, this.activeTags) ||
        this.hasAnyDomainTag(r.to.owner, this.activeTags);

      const g = svg("g", {
        class: `spla-edge${this.selection?.id === r.edge.id ? " is-selected" : ""}`,
        [EDGE_ATTR]: r.edge.id,
        opacity: viewHighlighted && tagHighlighted ? 1 : DIM.edge,
      });

      g.appendChild(
        svg("path", {
          class: "spla-edge-line",
          d: route.path,
          fill: "none",
          stroke: style.line.color,
          "stroke-width": style.line.width,
          "stroke-dasharray": dashArray(style.line.dash),
          "stroke-opacity": style.line.opacity === 1 ? null : style.line.opacity,
          // The head follows the line's colour unless the style overrides it,
          // which is the whole reason markers are built on demand instead of
          // picked from a fixed list with colours baked in.
          "marker-start": ctx.paints.marker(style.source, style.line.color),
          "marker-end": ctx.paints.marker(style.target, style.line.color),
        }),
      );

      if (r.edge.label !== "" && style.label.show) {
        g.appendChild(
          text(
            {
              ...textAttrs(style.label),
              class: "spla-edge-label",
              x: route.labelAt.x,
              y: route.labelAt.y,
              "text-anchor": style.label.align,
            },
            r.edge.label,
          ),
        );
      }

      this.edgesLayer.appendChild(g);
    }
  }

  /** Bounds of everything, using visible rectangles so collapsed containers count small. */
  private visibleBounds(): Rect | null {
    if (this.doc === null) return null;
    const ctx = this.context();
    let out: Rect | null = null;
    for (const el of this.doc.elements()) {
      if (ctx.isHidden(el)) continue;
      const r = this.registry.resolve(el).visibleRect(el, ctx);
      out = out === null ? r : unionRect(out, r);
    }
    return out;
  }

  /** Used by the interaction controller. */
  elementRectOf(el: DiagramElement): Rect {
    return elementRect(el);
  }

  setDropTarget(id: string | null): void {
    if (this.dropTargetId === id) return;
    this.dropTargetId = id;
    this.render();
  }

  emitGestureStart(reason: string): void {
    this.events.emit("gesturestart", { reason });
  }

  emitGestureEnd(reason: string): void {
    this.events.emit("gestureend", { reason });
  }
}

function unionRect(a: Rect, b: Rect): Rect {
  const x = Math.min(a.x, b.x);
  const y = Math.min(a.y, b.y);
  return {
    x,
    y,
    width: Math.max(a.x + a.width, b.x + b.width) - x,
    height: Math.max(a.y + a.height, b.y + b.height) - y,
  };
}

export function defaultRegistry(): TypeRegistry {
  const registry = new TypeRegistry();
  const box: ElementRenderer = new BoxRenderer();
  const container: ElementRenderer = new ContainerRenderer();
  registry.registerDefault("node", box);
  registry.registerDefault("zone", container);
  return registry;
}

export { setAttrs };
