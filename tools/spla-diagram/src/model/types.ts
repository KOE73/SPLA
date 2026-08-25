import type { Rect } from "../geometry/types.js";
import type { WireMetadata, WireZoneStyle } from "./wire-types.js";

/**
 * The in-memory model: one element shape, one containment tree.
 *
 * On the wire, zones and nodes are two flat arrays whose shapes are nearly
 * identical and whose nesting is implied by geometry. In memory they are one
 * kind of thing in a real tree, because every consumer above this layer —
 * rendering, interaction, export — wants a parent and children, not a guess.
 */

export type ElementKind = "zone" | "node";

export interface ElementStyle extends WireZoneStyle {}

export interface DiagramElement {
  readonly id: string;
  /**
   * Whether this element came from `zones` or `nodes` on the wire, and which
   * array it goes back to on save. It also selects a default renderer, and it
   * is what `views` still discriminates on (highlightZones vs highlightNodes).
   */
  readonly kind: ElementKind;
  /** Free-form type string. Selects a renderer and a style; never an enum. */
  type: string;
  /** Unified caption: `zone.name` or `node.label` on the wire. */
  label: string;
  semanticId?: string;
  tags: string[];
  metadata: WireMetadata;
  style?: ElementStyle;

  /** Absolute model coordinates. Contract v1 stores these directly. */
  x: number;
  y: number;
  width: number;
  height: number;

  parent: DiagramElement | null;
  readonly children: DiagramElement[];

  /**
   * Position this element had in its wire array when loaded, so that saving
   * reproduces the file's original ordering instead of the tree's traversal
   * order. Without it the first save of an untouched model would produce a
   * whole-file diff. New elements get Infinity and are appended.
   */
  wireOrder: number;
  /**
   * The object this element was parsed from, kept so that fields this library
   * does not model survive a load/save round trip untouched.
   */
  readonly raw?: Record<string, unknown>;

  /**
   * How this element's parentage arrived, so that saving an untouched model
   * reproduces the file byte for byte. A node whose containment was inferred
   * from geometry must not silently gain a `zone` field just because it was
   * opened — that would rewrite files nobody edited.
   */
  origin?: {
    /** Whether the wire object carried a `zone` key at all. */
    readonly zoneDeclared: boolean;
    /** Parent resolved at load time; a change from it means a real edit. */
    readonly parentId: string | null;
  };
}

export interface DiagramEdge {
  readonly id: string;
  from: string;
  to: string;
  label: string;
  type: string;
}

export interface DiagramView {
  readonly id: string;
  name: string;
  icon: string;
  description: string;
  highlightZones: string[];
  highlightNodes: string[];
}

export interface DiagramMetadata {
  title: string;
  layout?: string;
  description?: string;
  [key: string]: unknown;
}

export function elementRect(el: DiagramElement): Rect {
  return { x: el.x, y: el.y, width: el.width, height: el.height };
}

/** A container is an element that may hold children. Today: zones only. */
export function isContainer(el: DiagramElement): boolean {
  return el.kind === "zone";
}
