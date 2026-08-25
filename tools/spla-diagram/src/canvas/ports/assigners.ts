import { facingSide } from "../../geometry/rect.js";
import type { BoundarySlot, Side } from "../../geometry/types.js";
import {
  compareRequests,
  portKey,
  slotAlongSide,
  type PortAssigner,
  type PortKey,
  type PortRequest,
} from "./PortAssigner.js";

/**
 * Every end sits in the middle of the side that faces the other element.
 *
 * This reproduces the original renderer exactly (R-REND-10) and is the default,
 * so that porting the library did not change a single pixel. Its weakness is
 * the one worth knowing: several edges between the same pair overlap perfectly.
 */
export class CenterPortAssigner implements PortAssigner {
  readonly id = "center";

  assign(requests: readonly PortRequest[]): Map<PortKey, BoundarySlot> {
    const out = new Map<PortKey, BoundarySlot>();
    for (const req of requests) {
      out.set(portKey(req.edgeId, req.end), {
        side: facingSide(req.ownerRect, req.otherRect),
        t: 0.5,
      });
    }
    return out;
  }
}

/**
 * Spread the ends that share a side evenly along it.
 *
 * n ends land at 1/(n+1), 2/(n+1) … so the group stays centred and no end sits
 * on a corner. Adding an edge nudges its neighbours — acceptable, because edge
 * appearance is derived, while node placement is the part that is sacred.
 */
export class UniformPortAssigner implements PortAssigner {
  readonly id = "uniform";

  assign(requests: readonly PortRequest[]): Map<PortKey, BoundarySlot> {
    return distribute(requests, (index, count) => (index + 1) / (count + 1));
  }
}

/**
 * Place ends on a fixed grid of slots along the side, Visio-style.
 *
 * Ends keep their spacing regardless of how many there are, so a long side does
 * not bunch its connections in the middle. The group is centred on the side.
 */
export class DiscretePortAssigner implements PortAssigner {
  readonly id = "discrete";

  constructor(private readonly step = 20) {}

  assign(requests: readonly PortRequest[]): Map<PortKey, BoundarySlot> {
    return distribute(requests, (index, count, sideLength) => {
      const span = Math.min(this.step * (count - 1), Math.max(sideLength - this.step, 0));
      const gap = count > 1 ? span / (count - 1) : 0;
      const start = (sideLength - span) / 2;
      return sideLength <= 0 ? 0.5 : (start + gap * index) / sideLength;
    });
  }
}

type Placement = (index: number, count: number, sideLength: number) => number;

function distribute(
  requests: readonly PortRequest[],
  place: Placement,
): Map<PortKey, BoundarySlot> {
  const groups = new Map<string, { side: Side; items: PortRequest[] }>();

  for (const req of requests) {
    const side = facingSide(req.ownerRect, req.otherRect);
    const key = `${req.ownerId}#${side}`;
    const group = groups.get(key);
    if (group === undefined) groups.set(key, { side, items: [req] });
    else group.items.push(req);
  }

  const out = new Map<PortKey, BoundarySlot>();
  for (const { side, items } of groups.values()) {
    items.sort((a, b) => compareRequests(a, b, side));
    const first = items[0];
    if (first === undefined) continue;

    const horizontal = side === "north" || side === "south";
    const sideLength = horizontal ? first.ownerRect.width : first.ownerRect.height;

    items.forEach((req, i) => {
      const fraction = place(i, items.length, sideLength);
      out.set(portKey(req.edgeId, req.end), { side, t: slotAlongSide(side, fraction) });
    });
  }
  return out;
}
