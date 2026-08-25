import type { Point, Rect, Side } from "./types.js";

export function center(r: Rect): Point {
  return { x: r.x + r.width / 2, y: r.y + r.height / 2 };
}

export function right(r: Rect): number {
  return r.x + r.width;
}

export function bottom(r: Rect): number {
  return r.y + r.height;
}

export function area(r: Rect): number {
  return r.width * r.height;
}

export function containsPoint(r: Rect, p: Point): boolean {
  return p.x >= r.x && p.x <= right(r) && p.y >= r.y && p.y <= bottom(r);
}

/**
 * Strict containment: `inner` lies entirely within `outer`.
 * Touching edges count as contained; identical rectangles do too, so callers
 * that must exclude self-containment compare identity separately (R-CONT-02).
 */
export function containsRect(outer: Rect, inner: Rect): boolean {
  return (
    inner.x >= outer.x &&
    inner.y >= outer.y &&
    right(inner) <= right(outer) &&
    bottom(inner) <= bottom(outer)
  );
}

export function union(a: Rect, b: Rect): Rect {
  const x = Math.min(a.x, b.x);
  const y = Math.min(a.y, b.y);
  return {
    x,
    y,
    width: Math.max(right(a), right(b)) - x,
    height: Math.max(bottom(a), bottom(b)) - y,
  };
}

export function expand(r: Rect, by: number): Rect {
  return { x: r.x - by, y: r.y - by, width: r.width + by * 2, height: r.height + by * 2 };
}

/**
 * Which side of `from` faces `to`.
 *
 * The rule is the one the original renderer used (R-REND-10): whichever axis
 * has the larger separation wins. It is deliberately crude — it produces
 * stable, predictable results, which matters more here than optimal routing.
 */
export function facingSide(from: Rect, to: Rect): Side {
  const a = center(from);
  const b = center(to);
  const dx = b.x - a.x;
  const dy = b.y - a.y;

  if (Math.abs(dx) > Math.abs(dy)) {
    return dx >= 0 ? "east" : "west";
  }
  return dy >= 0 ? "south" : "north";
}

export function oppositeSide(side: Side): Side {
  switch (side) {
    case "north":
      return "south";
    case "south":
      return "north";
    case "east":
      return "west";
    case "west":
      return "east";
  }
}

/**
 * The point at parameter `t` (0..1) along one side of a rectangle, measured
 * clockwise so that the parameter runs in a consistent rotational direction on
 * every side. Used by the default box shape to turn a BoundarySlot into a point.
 */
export function pointOnSide(r: Rect, side: Side, t: number): Point {
  const clamped = Math.min(1, Math.max(0, t));
  switch (side) {
    case "north":
      return { x: r.x + r.width * clamped, y: r.y };
    case "east":
      return { x: right(r), y: r.y + r.height * clamped };
    case "south":
      return { x: right(r) - r.width * clamped, y: bottom(r) };
    case "west":
      return { x: r.x, y: bottom(r) - r.height * clamped };
  }
}

/** Snap a value to a grid step. A step of 0 or less disables snapping. */
export function snap(value: number, step: number): number {
  if (step <= 0) return value;
  return Math.round(value / step) * step;
}
