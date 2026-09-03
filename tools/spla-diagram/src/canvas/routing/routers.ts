/**
 * Manhattan-style edge routers: orthogonal ladder, and the two "tree" shapes
 * used by use-case flows (horizontal) and inheritance/org hierarchies
 * (vertical).
 *
 * ADR_20260903 §2.7: a router is a pure function of (ports, obstacle rects).
 * Nothing here is cached or stored — the caller recomputes a route whenever
 * geometry changes and throws the old one away. That is what lets routing
 * modes evolve (this file today, obstacle-avoidance later per §2.8) without
 * ever touching a saved file or migrating data.
 *
 * Endpoint-only: none of these routers walk around *other* blocks (§2.8,
 * deferred stream H). They only reason about their own two rectangles
 * (fromRect/toRect) plus the shape-aware insets/marker offsets already
 * computed by the caller — same contract BezierRouter uses.
 */
import type { Point, Side } from "../../geometry/types.js";
import type { EdgeRouter, Route, RouteRequest } from "./EdgeRouter.js";

/** Local to this file: EdgeRouter.ts / diagram-constants.ts are off-limits while
 * a parallel change is in flight there, so the fillet radius for the
 * orthogonal router's rounded corners lives here instead of DIAGRAM_CONFIG. */
const ORTHOGONAL_FILLET_RADIUS = 10;

function offsetPoint(p: Point, side: Side, distance: number): Point {
  if (distance <= 0) return p;
  switch (side) {
    case "north": return { x: p.x, y: p.y - distance };
    case "south": return { x: p.x, y: p.y + distance };
    case "west":  return { x: p.x - distance, y: p.y };
    case "east":  return { x: p.x + distance, y: p.y };
  }
}

function isHorizontal(side: Side): boolean {
  return side === "east" || side === "west";
}

/** Euclidean length of a straight segment; used to pick the longest one for labelAt. */
function segLength(a: Point, b: Point): number {
  return Math.hypot(b.x - a.x, b.y - a.y);
}

function midpoint(a: Point, b: Point): Point {
  return { x: (a.x + b.x) / 2, y: (a.y + b.y) / 2 };
}

/**
 * Picks the label anchor as the midpoint of the longest straight segment in
 * a polyline, rather than the geometric midpoint of the whole path.
 *
 * The geometric midpoint of a Manhattan ladder often lands exactly on a
 * corner or a very short jog segment (e.g. the vertical riser in a tree
 * router can be a handful of pixels tall when the two blocks are almost
 * level). A label centred there crowds the elbow and reads as attached to
 * the wrong segment. Anchoring to the longest run instead keeps the label on
 * open, unambiguous stretch of line, mirroring how a human would place it.
 */
function labelOnLongestSegment(points: readonly Point[]): Point {
  const fallback = points[0] ?? { x: 0, y: 0 };
  if (points.length < 2) return fallback;
  let bestA: Point = fallback;
  let bestB: Point = points[1] ?? fallback;
  let bestLen = -1;
  for (let i = 0; i + 1 < points.length; i++) {
    const a = points[i];
    const b = points[i + 1];
    if (!a || !b) continue;
    const len = segLength(a, b);
    if (len > bestLen) {
      bestLen = len;
      bestA = a;
      bestB = b;
    }
  }
  return midpoint(bestA, bestB);
}

/** Builds an SVG polyline path from a point list, guarding against NaN by
 * collapsing any degenerate (non-finite) coordinate to its neighbour. */
function safePolylinePath(points: readonly Point[]): string {
  const cleaned = points.map((p, i) => {
    if (Number.isFinite(p.x) && Number.isFinite(p.y)) return p;
    // Degenerate input (e.g. coincident rects producing a 0/0 direction):
    // fall back to the previous good point so the path stays drawable
    // instead of emitting "NaN" into the `d` attribute, which silently
    // erases the whole line.
    const prev = points[i - 1];
    return prev ?? { x: 0, y: 0 };
  });
  const first = cleaned[0];
  if (!first) return "";
  const rest = cleaned.slice(1);
  return `M ${first.x} ${first.y}` + rest.map((p) => ` L ${p.x} ${p.y}`).join("");
}

/**
 * Rounds the corners of a Manhattan polyline with a fixed-radius fillet,
 * replacing each interior vertex with a short arc. Falls back to a sharp
 * corner (radius 0 for that vertex) when either adjacent segment is shorter
 * than the fillet would need — avoids overlapping arcs on tightly packed
 * ladders.
 */
function filletedPath(points: readonly Point[], radius: number): string {
  const cleaned = points.map((p) => (Number.isFinite(p.x) && Number.isFinite(p.y) ? p : { x: 0, y: 0 }));
  if (cleaned.length < 2) return safePolylinePath(cleaned);
  if (cleaned.length === 2 || radius <= 0) return safePolylinePath(cleaned);

  const first = cleaned[0];
  if (!first) return "";
  let d = `M ${first.x} ${first.y}`;
  for (let i = 1; i < cleaned.length - 1; i++) {
    const prev = cleaned[i - 1];
    const curr = cleaned[i];
    const next = cleaned[i + 1];
    if (!prev || !curr || !next) continue;

    const inLen = segLength(prev, curr);
    const outLen = segLength(curr, next);
    // Cap the fillet so it never eats more than half of either adjoining
    // segment — prevents the arc from overshooting into the next corner
    // on short jogs.
    const r = Math.min(radius, inLen / 2, outLen / 2);

    if (r <= 0.01 || inLen === 0 || outLen === 0) {
      d += ` L ${curr.x} ${curr.y}`;
      continue;
    }

    const inRatio = (inLen - r) / inLen;
    const outRatio = r / outLen;
    const enter = {
      x: prev.x + (curr.x - prev.x) * inRatio,
      y: prev.y + (curr.y - prev.y) * inRatio,
    };
    const exit = {
      x: curr.x + (next.x - curr.x) * outRatio,
      y: curr.y + (next.y - curr.y) * outRatio,
    };
    d += ` L ${enter.x} ${enter.y} Q ${curr.x} ${curr.y}, ${exit.x} ${exit.y}`;
  }
  const last = cleaned[cleaned.length - 1];
  if (last) d += ` L ${last.x} ${last.y}`;
  return d;
}

/** Same end-label placement logic as BezierRouter's private helper: a step
 * outward along the port's own side, then off to the side of the stroke. */
function endLabelPoint(anchor: Point, side: Side, along: number, perp: number): Point {
  const dir = sideVector(side);
  const normal = { x: -dir.y, y: dir.x };
  return {
    x: anchor.x + dir.x * along + normal.x * perp,
    y: anchor.y + dir.y * along + normal.y * perp,
  };
}

function sideVector(side: Side): Point {
  switch (side) {
    case "north": return { x: 0, y: -1 };
    case "south": return { x: 0, y: 1 };
    case "west":  return { x: -1, y: 0 };
    case "east":  return { x: 1, y: 0 };
  }
}

const END_LABEL_ALONG = 14;
const END_LABEL_PERP = 9;

function endLabels(pFrom: Point, fromSide: Side, pTo: Point, toSide: Side) {
  return {
    fromLabelAt: endLabelPoint(pFrom, fromSide, END_LABEL_ALONG, END_LABEL_PERP),
    toLabelAt: endLabelPoint(pTo, toSide, END_LABEL_ALONG, END_LABEL_PERP),
  };
}

/**
 * Manhattan "ladder": leaves `from` along its side's normal, arrives at `to`
 * along its side's normal, with a single perpendicular jog connecting the
 * two straight runs. Corners are filleted for readability (a sequence of
 * sharp right angles reads as jagged at small scale).
 *
 * Degenerate cases (coincident points, zero gap, opposite/same sides) all
 * fall out of the same construction: when the "jog" collapses to zero length
 * the fillet step naturally skips it (see `filletedPath`), and worst case the
 * router still emits a valid two-point path.
 */
export class OrthogonalRouter implements EdgeRouter {
  readonly id = "orthogonal";

  route(req: RouteRequest): Route {
    const { from, to, fromSide, toSide } = req;
    const fromOffset = req.fromMarkerOffset ?? 0;
    const toOffset = req.toMarkerOffset ?? 0;

    const pFrom = offsetPoint(from, fromSide, fromOffset);
    const pTo = offsetPoint(to, toSide, toOffset);

    const points = buildOrthogonalPath(pFrom, fromSide, pTo, toSide);
    const path = filletedPath(points, ORTHOGONAL_FILLET_RADIUS);

    return {
      path,
      labelAt: labelOnLongestSegment(points),
      ...endLabels(pFrom, fromSide, pTo, toSide),
    };
  }
}

/**
 * Builds the waypoint list for a generic (non-tree) orthogonal route: one
 * bend when the ports face compatibly, two bends (an S/Z jog) otherwise.
 * Kept separate from the class so the degenerate branches stay easy to read.
 */
function buildOrthogonalPath(pFrom: Point, fromSide: Side, pTo: Point, toSide: Side): Point[] {
  // Coincident endpoints: nothing to draw but a point — return a 2-point
  // path so downstream path builders always have at least a segment.
  if (pFrom.x === pTo.x && pFrom.y === pTo.y) {
    return [pFrom, { x: pTo.x, y: pTo.y }];
  }

  const fromHoriz = isHorizontal(fromSide);
  const toHoriz = isHorizontal(toSide);

  if (fromHoriz && !toHoriz) {
    // Leave horizontally, arrive vertically: single elbow at (pTo.x, pFrom.y).
    const elbow = { x: pTo.x, y: pFrom.y };
    return [pFrom, elbow, pTo];
  }
  if (!fromHoriz && toHoriz) {
    // Leave vertically, arrive horizontally: single elbow at (pFrom.x, pTo.y).
    const elbow = { x: pFrom.x, y: pTo.y };
    return [pFrom, elbow, pTo];
  }
  if (fromHoriz && toHoriz) {
    // Both horizontal: jog at the horizontal midpoint between the two ports.
    const midX = (pFrom.x + pTo.x) / 2;
    return [pFrom, { x: midX, y: pFrom.y }, { x: midX, y: pTo.y }, pTo];
  }
  // Both vertical: jog at the vertical midpoint.
  const midY = (pFrom.y + pTo.y) / 2;
  return [pFrom, { x: pFrom.x, y: midY }, { x: pTo.x, y: midY }, pTo];
}

/**
 * "Tree" shape shared by both axis-specific routers below: leave straight
 * along the primary axis, run a perpendicular collector segment at the
 * halfway point between the two ports on that axis, then arrive straight
 * along the primary axis into the target. This is the classic use-case /
 * org-chart "bus" shape: every child hangs off one shared spine.
 */
function buildTreePath(pFrom: Point, pTo: Point, axis: "x" | "y"): Point[] {
  if (axis === "x") {
    // Horizontal travel: collector is a vertical segment at the midpoint x.
    const midX = (pFrom.x + pTo.x) / 2;
    if (pFrom.y === pTo.y) {
      // Already level: the "collector" would have zero length — a plain
      // straight line reads better than a needless jog.
      return [pFrom, pTo];
    }
    return [pFrom, { x: midX, y: pFrom.y }, { x: midX, y: pTo.y }, pTo];
  }
  // Vertical travel: collector is a horizontal segment at the midpoint y.
  const midY = (pFrom.y + pTo.y) / 2;
  if (pFrom.x === pTo.x) {
    return [pFrom, pTo];
  }
  return [pFrom, { x: pFrom.x, y: midY }, { x: pTo.x, y: midY }, pTo];
}

/**
 * "Bus" routing for left-to-right flows (use-case diagrams, pipelines):
 * leaves horizontally, a vertical collector sits halfway between the two
 * ports, arrives horizontally. No fillet — the flat "step" shape is the
 * point (distinguishes it visually from `orthogonal`'s rounded ladder).
 */
export class TreeHorizontalRouter implements EdgeRouter {
  readonly id = "tree-horizontal";

  route(req: RouteRequest): Route {
    const { from, to, fromSide, toSide } = req;
    const fromOffset = req.fromMarkerOffset ?? 0;
    const toOffset = req.toMarkerOffset ?? 0;

    const pFrom = offsetPoint(from, fromSide, fromOffset);
    const pTo = offsetPoint(to, toSide, toOffset);

    const points = buildTreePath(pFrom, pTo, "x");
    return {
      path: safePolylinePath(points),
      labelAt: labelOnLongestSegment(points),
      ...endLabels(pFrom, fromSide, pTo, toSide),
    };
  }
}

/**
 * "Bus" routing for top-to-bottom hierarchies (inheritance, org charts):
 * leaves vertically, a horizontal collector sits halfway between the two
 * ports, arrives vertically.
 */
export class TreeVerticalRouter implements EdgeRouter {
  readonly id = "tree-vertical";

  route(req: RouteRequest): Route {
    const { from, to, fromSide, toSide } = req;
    const fromOffset = req.fromMarkerOffset ?? 0;
    const toOffset = req.toMarkerOffset ?? 0;

    const pFrom = offsetPoint(from, fromSide, fromOffset);
    const pTo = offsetPoint(to, toSide, toOffset);

    const points = buildTreePath(pFrom, pTo, "y");
    return {
      path: safePolylinePath(points),
      labelAt: labelOnLongestSegment(points),
      ...endLabels(pFrom, fromSide, pTo, toSide),
    };
  }
}
