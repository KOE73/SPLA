import type { BoundarySlot, Point, Rect } from "../../geometry/types.js";
import type { DiagramDocument } from "../../model/document.js";
import type { DiagramElement } from "../../model/types.js";

/**
 * What a renderer is told about the world outside the element it draws.
 * Presentation state only — never the model's own data.
 */
export interface RenderContext {
  readonly doc: DiagramDocument;
  readonly selectedId: string | null;
  readonly dropTargetId: string | null;
  isCollapsed(el: DiagramElement): boolean;
  /** View highlighting, 0..1 (R-VIEW-03/04). */
  opacity(el: DiagramElement): number;
  /** Whether the element is hidden because an ancestor is collapsed. */
  isHidden(el: DiagramElement): boolean;
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
}
