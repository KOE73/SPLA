import type { BoundarySlot, Point, Rect, Side } from "../../geometry/types.js";
import type { DiagramDocument } from "../../model/document.js";
import type { ResolvedBlockStyle } from "../../model/StyleLibrary.js";
import type { DiagramElement } from "../../model/types.js";
import type { PaintRegistry } from "./PaintRegistry.js";

/**
 * What a renderer is told about the world outside the element it draws.
 * Presentation state only — never the model's own data.
 */
export interface RenderContext {
  readonly doc: DiagramDocument;
  /** The element that drives the inspector when several are selected. */
  readonly selectedId: string | null;
  readonly dropTargetId: string | null;
  readonly ghostNodeId?: string | null;
  isSelected(el: DiagramElement): boolean;
  isCollapsed(el: DiagramElement): boolean;
  /** View highlighting, 0..1 (R-VIEW-03/04). */
  opacity(el: DiagramElement): number;
  /** Whether the element is hidden because an ancestor is collapsed. */
  isHidden(el: DiagramElement): boolean;

  /**
   * The element's look, fully resolved: no optional fields, no inheritance
   * left to chase, no lookup for the renderer to get wrong.
   *
   * A renderer must not reach for the style library itself. Resolution is one
   * decision — styleId, then type, then the per-kind default — and it lives in
   * one place so that "why is this box grey" has one answer.
   */
  styleOf(el: DiagramElement): ResolvedBlockStyle;

  /**
   * Turns a resolved paint or arrow head into something an SVG attribute can
   * hold, materialising gradients and markers into `<defs>` on the way.
   */
  readonly paints: PaintRegistry;
}

/**
 * How one kind of thing looks and where lines attach to it.
 *
 * The containment tree is pure geometry; a renderer decides everything visual
 * about its own type and nothing about anyone else's. Adding a new type means
 * registering a renderer — no change anywhere else in the library.
 */
export interface ElementRenderer {
  /** Build the element's group from scratch. */
  create(el: DiagramElement, ctx: RenderContext): SVGGElement;

  /**
   * Bring an existing group up to date.
   *
   * Today every renderer may simply rebuild — that is what the original code
   * did on every frame. The method exists so that incremental rendering can be
   * added later inside renderers, without touching the code that calls them.
   */
  update(g: SVGGElement, el: DiagramElement, ctx: RenderContext): void;

  /**
   * Optional decoration drawn above every element rather than inside this
   * one's group — a resize grip that must stay grabbable even where a
   * neighbouring element overlaps this one's corner (R-REND-01).
   *
   * Returning null, or omitting the method, means the renderer needs none.
   */
  overlay?(el: DiagramElement, ctx: RenderContext): SVGGElement | null;

  /**
   * The rectangle the element actually occupies on screen, which is not always
   * its model rectangle — a collapsed container draws only its header.
   */
  visibleRect(el: DiagramElement, ctx: RenderContext): Rect;

  /**
   * Where a boundary slot lands on this shape.
   *
   * Port assignment works in shape-independent terms ("east side, 30% along");
   * turning that into a point is the shape's job. A box interpolates along an
   * edge, an ellipse walks an arc — same assignment algorithm, both correct.
   */
  pointAt(rect: Rect, slot: BoundarySlot): Point;

  /**
   * Safe corner inset (px) for this shape on the given side.
   * Tells port assigners and routers where the straight segment ends and the corner
   * or curvature begins.
   */
  cornerInset?(side: Side, style?: ResolvedBlockStyle): number;
}
