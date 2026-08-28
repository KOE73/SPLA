import type { Endpoint, Paint } from "../../model/StyleLibrary.js";
import { paintKey } from "../../model/StyleLibrary.js";
import type { EndShape } from "../../model/style-types.js";
import { svg } from "../svg.js";

/**
 * Materialises paints and arrow heads into `<defs>`, on demand.
 *
 * SVG cannot express a gradient or an arrow head inline: both have to exist as
 * a referenced definition first. The old code dealt with that by shipping a
 * fixed list of eight markers with the colours baked in — which is why the head
 * of an edge and its line were two independent colours that had to be kept in
 * agreement by hand, and why "a red dashed arrow" was not expressible without
 * editing the library.
 *
 * Here a definition is created the first time something asks for it and named
 * after what it contains, so asking twice returns the same id and the defs
 * block stays as small as the set of looks actually in use.
 */
export class PaintRegistry {
  private readonly defs: SVGDefsElement;
  private readonly seen = new Set<string>();

  constructor(defs: SVGDefsElement) {
    this.defs = defs;
  }

  /** A value for `fill=` / `stroke=`: a colour, or a url() into defs. */
  fill(paint: Paint): string {
    if (paint.kind === "solid") return paint.color;

    const id = `spla-p-${hash(paintKey(paint))}`;
    if (!this.seen.has(id)) {
      this.seen.add(id);
      this.defs.appendChild(
        paint.kind === "linear" ? linearGradient(id, paint) : radialGradient(id, paint),
      );
    }
    return `url(#${id})`;
  }

  /**
   * A value for `marker-start=` / `marker-end=`, or null when the end carries
   * no head.
   *
   * `lineColor` is what the head falls back to, so a head follows its line
   * unless the style deliberately says otherwise. Markers inherit the default
   * `markerUnits="strokeWidth"`, which is what the original eight were
   * calibrated against: `size` is measured in stroke widths, so a head keeps
   * its proportion to the line it sits on rather than swelling on hairlines.
   */
  marker(end: Endpoint, lineColor: string): string | null {
    if (end.shape === "none") return null;
    const color = end.color ?? lineColor;
    const size = end.size;
    const id = `spla-m-${hash(`${end.shape}|${size}|${color}`)}`;
    if (!this.seen.has(id)) {
      this.seen.add(id);
      this.defs.appendChild(markerFor(id, end.shape, size, color));
    }
    return `url(#${id})`;
  }
}

// ------------------------------------------------------------------ gradients

function linearGradient(
  id: string,
  paint: Extract<Paint, { kind: "linear" }>,
): SVGElement {
  // Angle in degrees clockwise from left-to-right, expressed as the unit vector
  // across the element's own box, so one gradient definition serves boxes of
  // any size.
  const rad = (paint.angle * Math.PI) / 180;
  const dx = Math.cos(rad);
  const dy = Math.sin(rad);
  return svg(
    "linearGradient",
    {
      id,
      x1: round(0.5 - dx / 2),
      y1: round(0.5 - dy / 2),
      x2: round(0.5 + dx / 2),
      y2: round(0.5 + dy / 2),
    },
    paint.stops.map((s) =>
      svg("stop", {
        offset: s.offset,
        "stop-color": s.color,
        "stop-opacity": s.opacity,
      }),
    ),
  );
}

function radialGradient(
  id: string,
  paint: Extract<Paint, { kind: "radial" }>,
): SVGElement {
  return svg(
    "radialGradient",
    { id, cx: 0.5, cy: 0.5, r: 0.6 },
    paint.stops.map((s) =>
      svg("stop", {
        offset: s.offset,
        "stop-color": s.color,
        "stop-opacity": s.opacity,
      }),
    ),
  );
}

// -------------------------------------------------------------------- markers

interface ShapeSpec {
  /** Path inside a 12×12 box, pointing right. */
  readonly path: string;
  /** Where the line's end lands along that box. */
  readonly refX: number;
  /** Filled with the head colour, or hollow with the page behind it. */
  readonly hollow?: boolean;
  /** Stroked outline only — no closed body to fill. */
  readonly open?: boolean;
}

/**
 * Every head is drawn in the same 12×12 box pointing right, so `orient` and
 * `refX` behave identically across shapes and a style can swap one for another
 * without the line appearing to change length.
 */
const SHAPES: Readonly<Record<Exclude<EndShape, "none">, ShapeSpec>> = {
  arrow: { path: "M 0 1.5 L 12 6 L 0 10.5 z", refX: 11 },
  "arrow-open": { path: "M 0.5 1 L 11 6 L 0.5 11", refX: 11, open: true },
  triangle: { path: "M 0 1 L 12 6 L 0 11 z", refX: 11.5 },
  "triangle-hollow": { path: "M 0.75 1.5 L 11 6 L 0.75 10.5 z", refX: 11, hollow: true },
  diamond: { path: "M 0 6 L 6 2 L 12 6 L 6 10 z", refX: 1 },
  "diamond-hollow": { path: "M 0.75 6 L 6 2.5 L 11.25 6 L 6 9.5 z", refX: 1.5, hollow: true },
  circle: { path: "M 2 6 a 4 4 0 1 0 8 0 a 4 4 0 1 0 -8 0", refX: 10 },
  "circle-hollow": { path: "M 2.5 6 a 3.5 3.5 0 1 0 7 0 a 3.5 3.5 0 1 0 -7 0", refX: 10, hollow: true },
  bar: { path: "M 10 1 L 10 11", refX: 10, open: true },
};

function markerFor(id: string, shape: Exclude<EndShape, "none">, size: number, color: string): SVGElement {
  const spec = SHAPES[shape];
  const stroked = spec.hollow === true || spec.open === true;
  return svg(
    "marker",
    {
      id,
      viewBox: "0 0 12 12",
      refX: spec.refX,
      refY: 6,
      markerWidth: size,
      markerHeight: size,
      // Reversed automatically at the start of a path, so one definition serves
      // both ends and a head at `from` points back the way it should.
      orient: "auto-start-reverse",
    },
    [
      svg("path", {
        d: spec.path,
        // A hollow head is filled with the canvas colour rather than "none":
        // "none" would let the line show through the middle of the triangle,
        // which is precisely the artefact hollow heads exist to avoid.
        fill: spec.open === true ? "none" : spec.hollow === true ? "#ffffff" : color,
        stroke: stroked ? color : null,
        "stroke-width": stroked ? 1.4 : null,
        "stroke-linecap": spec.open === true ? "round" : null,
        "stroke-linejoin": "round",
      }),
    ],
  );
}

// ----------------------------------------------------------------- utilities

/**
 * A short, stable name for a spec.
 *
 * Not a security hash — it only has to make two different looks land on two
 * different ids within one document, and to give the same look the same id
 * across re-renders so defs stop growing.
 */
function hash(input: string): string {
  let h = 2166136261;
  for (let i = 0; i < input.length; i += 1) {
    h ^= input.charCodeAt(i);
    h = Math.imul(h, 16777619);
  }
  return (h >>> 0).toString(36);
}

function round(n: number): number {
  return Math.round(n * 1000) / 1000;
}
