import type { Point, Rect, Side } from "../../geometry/types.js";
import { DIAGRAM_CONFIG } from "../../constants/diagram-constants.js";

export interface RouteRequest {
  readonly from: Point;
  readonly to: Point;
  readonly fromSide: Side;
  readonly toSide: Side;
  readonly fromRect: Rect;
  readonly toRect: Rect;
  /** Shape-aware corner inset for the starting element (px) */
  readonly fromInset?: number;
  /** Shape-aware corner inset for the target element (px) */
  readonly toInset?: number;
  /** Marker length at the start of the line (px) */
  readonly fromMarkerOffset?: number;
  /** Marker length at the end of the line (px) */
  readonly toMarkerOffset?: number;
}

export interface Route {
  /** SVG path data. */
  readonly path: string;
  /** Where the edge's label belongs. */
  readonly labelAt: Point;
}

export interface EdgeRouter {
  readonly id: string;
  route(req: RouteRequest): Route;
}

function offsetPoint(p: Point, side: Side, distance: number): Point {
  if (distance <= 0) return p;
  switch (side) {
    case "north": return { x: p.x, y: p.y - distance };
    case "south": return { x: p.x, y: p.y + distance };
    case "west":  return { x: p.x - distance, y: p.y };
    case "east":  return { x: p.x + distance, y: p.y };
  }
}

export class BezierRouter implements EdgeRouter {
  readonly id = "bezier";

  /** Below this perpendicular offset, curving would add nothing but noise. */
  private static readonly STRAIGHT_THRESHOLD = DIAGRAM_CONFIG.routing.bezierStraightThreshold;
  /** How far a control point is pushed outward, capped so short hops stay gentle. */
  private static readonly MAX_HANDLE = DIAGRAM_CONFIG.routing.bezierMaxHandle;

  route(req: RouteRequest): Route {
    const { from, to, fromSide, toSide } = req;
    const fromOffset = req.fromMarkerOffset ?? 0;
    const toOffset = req.toMarkerOffset ?? 0;

    const pFrom = offsetPoint(from, fromSide, fromOffset);
    const pTo = offsetPoint(to, toSide, toOffset);

    const horizontal = fromSide === "east" || fromSide === "west";

    if (horizontal) {
      if (Math.abs(pTo.y - pFrom.y) <= BezierRouter.STRAIGHT_THRESHOLD) {
        return straightLine(pFrom, pTo);
      }
      const handle = Math.min(Math.abs(pTo.x - pFrom.x) / 2, BezierRouter.MAX_HANDLE);
      const c1x = pFrom.x + (fromSide === "east" ? handle : -handle);
      const c2x = pTo.x + (toSide === "east" ? handle : -handle);
      return {
        path: `M ${pFrom.x} ${pFrom.y} C ${c1x} ${pFrom.y}, ${c2x} ${pTo.y}, ${pTo.x} ${pTo.y}`,
        labelAt: { x: (pFrom.x + pTo.x) / 2, y: (pFrom.y + pTo.y) / 2 - 6 },
      };
    }

    // Vertical (north/south)
    if (Math.abs(pTo.x - pFrom.x) <= BezierRouter.STRAIGHT_THRESHOLD) {
      return straightLine(pFrom, pTo);
    }
    const handle = Math.min(Math.abs(pTo.y - pFrom.y) / 2, BezierRouter.MAX_HANDLE);
    const c1y = pFrom.y + (fromSide === "south" ? handle : -handle);
    const c2y = pTo.y + (toSide === "south" ? handle : -handle);
    return {
      path: `M ${pFrom.x} ${pFrom.y} C ${pFrom.x} ${c1y}, ${pTo.x} ${c2y}, ${pTo.x} ${pTo.y}`,
      labelAt: { x: (pFrom.x + pTo.x) / 2 + 8, y: (pFrom.y + pTo.y) / 2 },
    };
  }
}

function straightLine(from: Point, to: Point): Route {
  return {
    path: `M ${from.x} ${from.y} L ${to.x} ${to.y}`,
    labelAt: { x: (from.x + to.x) / 2, y: (from.y + to.y) / 2 - 6 },
  };
}
