import { svg } from "./svg.js";

interface MarkerSpec {
  id: string;
  path: string;
  fill?: string;
  stroke?: string;
  strokeWidth?: number;
  size: number;
  refX: number;
  viewBox: string;
}

const MARKERS: readonly MarkerSpec[] = [
  { id: "spla-arrow", path: "M 0 1.5 L 10 5 L 0 8.5 z", fill: "#94a3b8", size: 6, refX: 9, viewBox: "0 0 10 10" },
  { id: "spla-arrow-data", path: "M 0 1.5 L 10 5 L 0 8.5 z", fill: "#2563eb", size: 6, refX: 9, viewBox: "0 0 10 10" },
  { id: "spla-arrow-security", path: "M 0 1.5 L 10 5 L 0 8.5 z", fill: "#dc2626", size: 6, refX: 9, viewBox: "0 0 10 10" },
  { id: "spla-arrow-storage", path: "M 0 1.5 L 10 5 L 0 8.5 z", fill: "#9333ea", size: 6, refX: 9, viewBox: "0 0 10 10" },
  {
    id: "spla-arrow-open", path: "M 0 1 L 9 5 L 0 9", fill: "none",
    stroke: "#ea580c", strokeWidth: 1.6, size: 8, refX: 9, viewBox: "0 0 10 10",
  },
  {
    id: "spla-triangle-hollow", path: "M 0 1 L 11 6 L 0 11 z", fill: "#ffffff",
    stroke: "#475569", strokeWidth: 1.5, size: 11, refX: 11, viewBox: "0 0 12 12",
  },
  { id: "spla-triangle-solid", path: "M 0 1 L 11 6 L 0 11 z", fill: "#475569", size: 11, refX: 11, viewBox: "0 0 12 12" },
  { id: "spla-diamond-solid", path: "M 0 6 L 6 2 L 12 6 L 6 10 z", fill: "#0f766e", size: 12, refX: 1, viewBox: "0 0 12 12" },
];

/**
 * Arrow heads and the drop shadow.
 *
 * Ids are prefixed so that a page hosting more than one canvas, or hosting this
 * canvas next to other SVG, cannot collide. The original `#triangle` marker was
 * dropped: nothing referenced it and it was drawn with `currentColor` on a
 * fill-less path, so it would not have shown up anyway (D-10).
 */
export function createDefs(): SVGDefsElement {
  const markers = MARKERS.map((m) =>
    svg("marker", {
      id: m.id,
      viewBox: m.viewBox,
      refX: m.refX,
      refY: m.viewBox === "0 0 12 12" ? 6 : 5,
      markerWidth: m.size,
      markerHeight: m.size,
      orient: "auto-start-reverse",
    }, [
      svg("path", {
        d: m.path,
        fill: m.fill ?? "none",
        stroke: m.stroke ?? null,
        "stroke-width": m.strokeWidth ?? null,
      }),
    ]),
  );

  const shadow = svg("filter", {
    id: "spla-shadow", x: "-5%", y: "-5%", width: "115%", height: "115%",
  }, [
    svg("feDropShadow", { dx: 0, dy: 2, stdDeviation: 3, "flood-opacity": 0.08 }),
  ]);

  return svg("defs", {}, [...markers, shadow]);
}
