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
import type { Point, Rect, Side } from "../../geometry/types.js";
import type { EdgeRouter, Route, RouteRequest } from "./EdgeRouter.js";

/** Local to this file: EdgeRouter.ts / diagram-constants.ts are off-limits while
 * a parallel change is in flight there, so the fillet radius for the
 * orthogonal router's rounded corners lives here instead of DIAGRAM_CONFIG. */
const ORTHOGONAL_FILLET_RADIUS = 10;

/**
 * Obstacle-avoidance and boundary-hug constants (ADR_20260903 §2.8, first
 * approximation). Local to this file for the same reason as the fillet
 * radius above.
 */
/** How far a detour clears an obstacle's bounding box (px). */
const OBSTACLE_DETOUR_MARGIN = 16;
/** Minimum perpendicular clearance a segment must keep from a container
 * boundary it runs parallel to (px) — below this it reads as "on" the line. */
const BOUNDARY_GAP = 10;
/** Minimum overlap along the shared axis before a near-parallel segment
 * counts as "running alongside" a boundary edge rather than just grazing it
 * on the way through. */
const BOUNDARY_OVERLAP_MIN = 4;
/** Shrink applied to obstacle rects before testing for interior crossings,
 * so a segment that merely touches an obstacle's edge (e.g. a port on its
 * boundary) is not flagged as passing through it. */
const OBSTACLE_EPS = 0.5;

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

// ------------------------------------------------------- obstacle avoidance

function isFinitePoint(p: Point): boolean {
  return Number.isFinite(p.x) && Number.isFinite(p.y);
}

function isFiniteRect(r: Rect): boolean {
  return Number.isFinite(r.x) && Number.isFinite(r.y) && Number.isFinite(r.width) && Number.isFinite(r.height);
}

/** Whether `point` lies within `rect` (inclusive, with a small tolerance). */
function rectContainsPoint(rect: Rect, point: Point, eps = 0.5): boolean {
  return (
    point.x >= rect.x - eps && point.x <= rect.x + rect.width + eps &&
    point.y >= rect.y - eps && point.y <= rect.y + rect.height + eps
  );
}

function sameRect(a: Rect, b: Rect, eps = 0.01): boolean {
  return Math.abs(a.x - b.x) < eps && Math.abs(a.y - b.y) < eps &&
    Math.abs(a.width - b.width) < eps && Math.abs(a.height - b.height) < eps;
}

/**
 * Obstacles that are really "this edge's own ends" in disguise: the two
 * endpoint rects themselves, or any rect one of the endpoints sits inside
 * (a collapsed/parent container the port is drawn against). A route has to
 * be allowed to leave and arrive through those — they are not "someone
 * else's block" (task brief, §obstacles).
 */
function isEndpointRelated(rect: Rect, fromRect: Rect, toRect: Rect, pFrom: Point, pTo: Point): boolean {
  if (!isFiniteRect(rect)) return true; // degenerate: treat as unusable, drop it from consideration
  if (sameRect(rect, fromRect) || sameRect(rect, toRect)) return true;
  if (rectContainsPoint(rect, pFrom) || rectContainsPoint(rect, pTo)) return true;
  return false;
}

/** Does the closed axis-aligned segment a→b pass *through the interior* of
 * `rect` (touching an edge does not count)? Both routers here only ever
 * build axis-aligned (Manhattan) segments, so this simple case-split is
 * exact — no general line/rect clipping needed. */
function segmentCrossesRectInterior(a: Point, b: Point, rect: Rect): boolean {
  if (!isFinitePoint(a) || !isFinitePoint(b) || !isFiniteRect(rect)) return false;
  const innerX0 = rect.x + OBSTACLE_EPS;
  const innerX1 = rect.x + rect.width - OBSTACLE_EPS;
  const innerY0 = rect.y + OBSTACLE_EPS;
  const innerY1 = rect.y + rect.height - OBSTACLE_EPS;
  if (innerX0 >= innerX1 || innerY0 >= innerY1) return false; // too thin to have an interior worth avoiding

  if (a.y === b.y) {
    // Horizontal segment.
    if (a.y <= innerY0 || a.y >= innerY1) return false;
    const lo = Math.min(a.x, b.x);
    const hi = Math.max(a.x, b.x);
    return lo < innerX1 && hi > innerX0;
  }
  if (a.x === b.x) {
    // Vertical segment.
    if (a.x <= innerX0 || a.x >= innerX1) return false;
    const lo = Math.min(a.y, b.y);
    const hi = Math.max(a.y, b.y);
    return lo < innerY1 && hi > innerY0;
  }
  // Non-axis-aligned segment (shouldn't occur for these routers): skip
  // rather than guess — a false negative here just falls back to the base
  // route, which is never worse than the pre-avoidance behaviour.
  return false;
}

function countCrossings(points: readonly Point[], obstacles: readonly Rect[]): number {
  let count = 0;
  for (let i = 0; i + 1 < points.length; i++) {
    const a = points[i];
    const b = points[i + 1];
    if (!a || !b) continue;
    for (const rect of obstacles) {
      if (segmentCrossesRectInterior(a, b, rect)) count++;
    }
  }
  return count;
}

function pathLength(points: readonly Point[]): number {
  let total = 0;
  for (let i = 0; i + 1 < points.length; i++) {
    const a = points[i];
    const b = points[i + 1];
    if (!a || !b) continue;
    total += segLength(a, b);
  }
  return total;
}

interface Bbox { minX: number; minY: number; maxX: number; maxY: number }

function unionBbox(rects: readonly Rect[]): Bbox | null {
  let out: Bbox | null = null;
  for (const r of rects) {
    if (!isFiniteRect(r)) continue;
    const b: Bbox = { minX: r.x, minY: r.y, maxX: r.x + r.width, maxY: r.y + r.height };
    out = out === null ? b : {
      minX: Math.min(out.minX, b.minX), minY: Math.min(out.minY, b.minY),
      maxX: Math.max(out.maxX, b.maxX), maxY: Math.max(out.maxY, b.maxY),
    };
  }
  return out;
}

/**
 * Four candidate detours around the bounding box of the obstacles the base
 * route collides with — go around above, below, left of, or right of the
 * blocking cluster. Each is a simple 4-point "bus" jog, not a shortest-path
 * search: cheap, deterministic, and good enough for the common case of one
 * or two blocks sitting in the way (task brief: "не переусложняй").
 */
function detourCandidates(pFrom: Point, pTo: Point, bbox: Bbox): Point[][] {
  const gateTop = bbox.minY - OBSTACLE_DETOUR_MARGIN;
  const gateBottom = bbox.maxY + OBSTACLE_DETOUR_MARGIN;
  const gateLeft = bbox.minX - OBSTACLE_DETOUR_MARGIN;
  const gateRight = bbox.maxX + OBSTACLE_DETOUR_MARGIN;
  return [
    [pFrom, { x: pFrom.x, y: gateTop }, { x: pTo.x, y: gateTop }, pTo],
    [pFrom, { x: pFrom.x, y: gateBottom }, { x: pTo.x, y: gateBottom }, pTo],
    [pFrom, { x: gateLeft, y: pFrom.y }, { x: gateLeft, y: pTo.y }, pTo],
    [pFrom, { x: gateRight, y: pFrom.y }, { x: gateRight, y: pTo.y }, pTo],
  ];
}

/**
 * Picks the best of the base route and a handful of detours around whatever
 * it collides with, by (fewest obstacle crossings, then shortest, then
 * fewest bends). Pure and stateless per ADR_20260903 §2.7/§2.8: no search
 * state survives the call, and worst case (nothing clears the obstacle) it
 * simply returns the base route rather than nothing.
 */
function avoidObstacles(
  base: Point[],
  pFrom: Point,
  pTo: Point,
  fromRect: Rect,
  toRect: Rect,
  obstacles: readonly Rect[] | undefined,
): Point[] {
  if (!obstacles || obstacles.length === 0) return base;
  const relevant = obstacles.filter((r) => !isEndpointRelated(r, fromRect, toRect, pFrom, pTo));
  if (relevant.length === 0) return base;

  const baseCrossings = countCrossings(base, relevant);
  if (baseCrossings === 0) return base;

  const colliding = relevant.filter((r) => {
    for (let i = 0; i + 1 < base.length; i++) {
      const a = base[i];
      const b = base[i + 1];
      if (a && b && segmentCrossesRectInterior(a, b, r)) return true;
    }
    return false;
  });
  const bbox = unionBbox(colliding.length > 0 ? colliding : relevant);
  if (bbox === null) return base;

  const candidates = [base, ...detourCandidates(pFrom, pTo, bbox)];
  let best = base;
  let bestScore: [number, number, number] = [baseCrossings, pathLength(base), base.length];
  for (const candidate of candidates) {
    if (candidate.some((p) => !isFinitePoint(p))) continue;
    const score: [number, number, number] = [countCrossings(candidate, relevant), pathLength(candidate), candidate.length];
    if (
      score[0] < bestScore[0] ||
      (score[0] === bestScore[0] && score[1] < bestScore[1]) ||
      (score[0] === bestScore[0] && score[1] === bestScore[1] && score[2] < bestScore[2])
    ) {
      best = candidate;
      bestScore = score;
    }
  }
  return best;
}

// -------------------------------------------------------- boundary hugging

/**
 * Nudges interior segments that run parallel to, and within `BOUNDARY_GAP`
 * of, a container's border away from it — so the line reads as crossing
 * into the zone rather than merging with its outline (task brief, issue 1).
 *
 * Only *interior* segments (both endpoints are corner points the router
 * invented, not port attachment points) are ever moved: the first and last
 * segment touch `pFrom`/`pTo` exactly where the caller placed the port, and
 * moving those would detach the line from the shape it is meant to leave.
 */
function avoidBoundaryHug(points: readonly Point[], boundaries: readonly Rect[] | undefined): Point[] {
  if (!boundaries || boundaries.length === 0 || points.length < 4) return points.slice();
  const out = points.map((p) => ({ ...p }));

  for (let i = 1; i + 2 < out.length; i++) {
    const a = out[i];
    const b = out[i + 1];
    if (!a || !b || !isFinitePoint(a) || !isFinitePoint(b)) continue;

    if (a.y === b.y) {
      // Horizontal segment: check against top/bottom edges of each boundary.
      let y = a.y;
      for (const rect of boundaries) {
        if (!isFiniteRect(rect)) continue;
        const lo = Math.min(a.x, b.x);
        const hi = Math.max(a.x, b.x);
        const overlap = Math.min(hi, rect.x + rect.width) - Math.max(lo, rect.x);
        if (overlap < BOUNDARY_OVERLAP_MIN) continue;
        for (const edgeY of [rect.y, rect.y + rect.height]) {
          const dist = Math.abs(y - edgeY);
          if (dist < BOUNDARY_GAP) {
            const dir = y >= edgeY ? 1 : -1; // push further to whichever side it already leans
            y = edgeY + dir * BOUNDARY_GAP;
          }
        }
      }
      if (Number.isFinite(y)) {
        a.y = y;
        b.y = y;
      }
    } else if (a.x === b.x) {
      // Vertical segment: check against left/right edges of each boundary.
      let x = a.x;
      for (const rect of boundaries) {
        if (!isFiniteRect(rect)) continue;
        const lo = Math.min(a.y, b.y);
        const hi = Math.max(a.y, b.y);
        const overlap = Math.min(hi, rect.y + rect.height) - Math.max(lo, rect.y);
        if (overlap < BOUNDARY_OVERLAP_MIN) continue;
        for (const edgeX of [rect.x, rect.x + rect.width]) {
          const dist = Math.abs(x - edgeX);
          if (dist < BOUNDARY_GAP) {
            const dir = x >= edgeX ? 1 : -1;
            x = edgeX + dir * BOUNDARY_GAP;
          }
        }
      }
      if (Number.isFinite(x)) {
        a.x = x;
        b.x = x;
      }
    }
  }
  return out;
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
    const { from, to, fromSide, toSide, fromRect, toRect } = req;
    const fromOffset = req.fromMarkerOffset ?? 0;
    const toOffset = req.toMarkerOffset ?? 0;

    const pFrom = offsetPoint(from, fromSide, fromOffset);
    const pTo = offsetPoint(to, toSide, toOffset);

    const base = buildOrthogonalPath(pFrom, fromSide, pTo, toSide);
    const routed = avoidObstacles(base, pFrom, pTo, fromRect, toRect, req.obstacles);
    const points = avoidBoundaryHug(routed, req.boundaries);
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
    const { from, to, fromSide, toSide, fromRect, toRect } = req;
    const fromOffset = req.fromMarkerOffset ?? 0;
    const toOffset = req.toMarkerOffset ?? 0;

    const pFrom = offsetPoint(from, fromSide, fromOffset);
    const pTo = offsetPoint(to, toSide, toOffset);

    const base = buildTreePath(pFrom, pTo, "x");
    const routed = avoidObstacles(base, pFrom, pTo, fromRect, toRect, req.obstacles);
    const points = avoidBoundaryHug(routed, req.boundaries);
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
    const { from, to, fromSide, toSide, fromRect, toRect } = req;
    const fromOffset = req.fromMarkerOffset ?? 0;
    const toOffset = req.toMarkerOffset ?? 0;

    const pFrom = offsetPoint(from, fromSide, fromOffset);
    const pTo = offsetPoint(to, toSide, toOffset);

    const base = buildTreePath(pFrom, pTo, "y");
    const routed = avoidObstacles(base, pFrom, pTo, fromRect, toRect, req.obstacles);
    const points = avoidBoundaryHug(routed, req.boundaries);
    return {
      path: safePolylinePath(points),
      labelAt: labelOnLongestSegment(points),
      ...endLabels(pFrom, fromSide, pTo, toSide),
    };
  }
}
