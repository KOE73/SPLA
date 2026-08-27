import { bottom, right } from "../../geometry/rect.js";
import type { Point, Rect, Side } from "../../geometry/types.js";

export interface RouteRequest {
  readonly from: Point;
  readonly to: Point;
  readonly fromSide: Side;
  readonly toSide: Side;
  readonly fromRect: Rect;
  readonly toRect: Rect;
}

export interface Route {
  /** SVG path data. */
  readonly path: string;
  /** Where the edge's label belongs. */
  readonly labelAt: Point;
}

/**
 * Turns two attachment points into a drawn line.
 *
 * Isolated behind an interface because this is the piece most likely to be
 * replaced: orthogonal routing, obstacle avoidance and bundling are all
 * different implementations that the rest of the canvas need not know about.
 */
export interface EdgeRouter {
  readonly id: string;
  route(req: RouteRequest): Route;
}

/**
 * A cubic curve that leaves and enters along the axis of the chosen sides,
 * collapsing to a straight line when the two ends already face each other
 * squarely.
 *
 * The control points are anchored to their *own* end and pushed outward
 * along that end's side normal — never toward a shared midpoint. That is
 * what keeps the curve from crossing into either box: a point pushed away
 * from its own boundary can only leave the box behind it, whereas the
 * original design (control points meeting at the span's midpoint) had no
 * such guarantee. When the pair is far apart along its perpendicular axis
 * this reads as a gentle exit-and-entry curve; when it is nearly aligned
 * (the common case for two elements inside the same container) the
 * perpendicular offset is small enough that the curve is visually
 * indistinguishable from — and below a hard threshold, literally is — a
 * straight line. Distance does the "near vs. far" classification on its
 * own: two elements in different containers are, in a fixed-coordinate
 * diagram, always far apart on at least one axis.
 */
export class BezierRouter implements EdgeRouter {
  readonly id = "bezier";

  /** Below this perpendicular offset, curving would add nothing but noise. */
  private static readonly STRAIGHT_THRESHOLD = 6;
  /** How far a control point is pushed outward, capped so short hops stay gentle. */
  private static readonly MAX_HANDLE = 60;

  route(req: RouteRequest): Route {
    const { from, to, fromSide, toSide, fromRect, toRect } = req;
    const horizontal = fromSide === "east" || fromSide === "west";

    if (horizontal) {
      // Both ends sit on a vertical (east/west) side, so a horizontal run is
      // available exactly when the boxes share a band of y.
      const y = sharedRun(
        fromRect.y, bottom(fromRect), from.y, fromRect.height,
        toRect.y, bottom(toRect), to.y, toRect.height,
      );
      if (y !== null) return straightLine({ x: from.x, y }, { x: to.x, y });

      if (Math.abs(to.y - from.y) <= BezierRouter.STRAIGHT_THRESHOLD) {
        return straightLine(from, to);
      }
      const handle = Math.min(Math.abs(to.x - from.x) / 2, BezierRouter.MAX_HANDLE);
      const c1x = from.x + (fromSide === "east" ? handle : -handle);
      const c2x = to.x + (toSide === "east" ? handle : -handle);
      return {
        path: `M ${from.x} ${from.y} C ${c1x} ${from.y}, ${c2x} ${to.y}, ${to.x} ${to.y}`,
        labelAt: { x: (from.x + to.x) / 2, y: (from.y + to.y) / 2 - 6 },
      };
    }

    // Both ends sit on a horizontal (north/south) side: a vertical run is
    // available when the boxes share a band of x — a class directly beneath
    // the interface it implements, say.
    const x = sharedRun(
      fromRect.x, right(fromRect), from.x, fromRect.width,
      toRect.x, right(toRect), to.x, toRect.width,
    );
    if (x !== null) return straightLine({ x, y: from.y }, { x, y: to.y });

    if (Math.abs(to.x - from.x) <= BezierRouter.STRAIGHT_THRESHOLD) {
      return straightLine(from, to);
    }
    const handle = Math.min(Math.abs(to.y - from.y) / 2, BezierRouter.MAX_HANDLE);
    const c1y = from.y + (fromSide === "south" ? handle : -handle);
    const c2y = to.y + (toSide === "south" ? handle : -handle);
    return {
      path: `M ${from.x} ${from.y} C ${from.x} ${c1y}, ${to.x} ${c2y}, ${to.x} ${to.y}`,
      labelAt: { x: (from.x + to.x) / 2 + 8, y: (from.y + to.y) / 2 },
    };
  }
}

/**
 * The coordinate of a straight axis-aligned run between the two ends, or null
 * when the boxes share no band on that axis.
 *
 * Both ends already sit on sides perpendicular to the run, so sliding an end
 * along the axis keeps it on the side the port assigner chose: only its
 * position along that side moves, never which side it is. The end on the
 * *smaller* box keeps its assigned position, and the larger box's end is the
 * one that slides — a short side has only one sensible place to leave from,
 * while a long side has room to spare and its midpoint is arbitrary. That is
 * what lets several classes each drop straight down onto the one wide
 * interface above them instead of converging on its centre.
 */
function sharedRun(
  fromMin: number, fromMax: number, fromPos: number, fromExtent: number,
  toMin: number, toMax: number, toPos: number, toExtent: number,
): number | null {
  const lo = Math.max(fromMin, toMin);
  const hi = Math.min(fromMax, toMax);
  if (lo > hi) return null;
  const anchor = fromExtent <= toExtent ? fromPos : toPos;
  return Math.min(Math.max(anchor, lo), hi);
}

function straightLine(from: Point, to: Point): Route {
  return {
    path: `M ${from.x} ${from.y} L ${to.x} ${to.y}`,
    labelAt: { x: (from.x + to.x) / 2, y: (from.y + to.y) / 2 - 6 },
  };
}
