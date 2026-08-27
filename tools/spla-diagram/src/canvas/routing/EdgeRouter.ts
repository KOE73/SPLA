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
 * A cubic curve that leaves and enters along the axis of the chosen sides.
 *
 * This is the original routing, preserved exactly: control points sit at the
 * midpoint of the span, so the curve bulges out of the source and into the
 * target without any awareness of what lies between them.
 */
export class BezierRouter implements EdgeRouter {
  readonly id = "bezier";

  route(req: RouteRequest): Route {
    const { from, to, fromSide } = req;
    const horizontal = fromSide === "east" || fromSide === "west";

    if (horizontal) {
      const midX = (from.x + to.x) / 2;
      return {
        path: `M ${from.x} ${from.y} C ${midX} ${from.y}, ${midX} ${to.y}, ${to.x} ${to.y}`,
        labelAt: { x: midX, y: (from.y + to.y) / 2 - 6 },
      };
    }

    const midY = (from.y + to.y) / 2;
    return {
      path: `M ${from.x} ${from.y} C ${from.x} ${midY}, ${to.x} ${midY}, ${to.x} ${to.y}`,
      labelAt: { x: (from.x + to.x) / 2 + 8, y: midY },
    };
  }
}

/**
 * A direct chord between the two attachment points, no detour.
 *
 * The default. `BezierRouter`'s S-curve assumes the pair sits roughly
 * opposite each other along the chosen axis; when the boxes are closer to
 * diagonal, its control points can swing the curve's tail across the target
 * box before it reaches the boundary, burying the arrowhead under the node's
 * own fill (nodes paint over the edge layer — see R-REND-10 history). A
 * straight line never backtracks past its own endpoint, so it cannot recreate
 * that failure, and for a fixed-coordinate diagram straight connectors read
 * fine: there is no auto-layout here to produce overlapping geometry a curve
 * would need to route around.
 */
export class StraightRouter implements EdgeRouter {
  readonly id = "straight";

  route(req: RouteRequest): Route {
    const { from, to } = req;
    return {
      path: `M ${from.x} ${from.y} L ${to.x} ${to.y}`,
      labelAt: { x: (from.x + to.x) / 2, y: (from.y + to.y) / 2 - 6 },
    };
  }
}
