/**
 * The visual vocabulary: how each semantic type looks.
 *
 * These are data, not behaviour, so they are plain records. A type that needs
 * more than fill/stroke/icon — a cylinder, a UML class with compartments —
 * gets its own renderer in the registry instead of an entry here.
 */

export interface NodeStyle {
  readonly fill: string;
  readonly stroke: string;
  readonly strokeWidth: number;
  readonly icon: string;
}

const NODE_STYLES: Readonly<Record<string, NodeStyle>> = {
  concept: { fill: "#fefce8", stroke: "#fef08a", strokeWidth: 1.8, icon: "💡" },
  note: { fill: "#fef9c3", stroke: "#fde047", strokeWidth: 1.5, icon: "📝" },
  component: { fill: "#ffffff", stroke: "#cbd5e1", strokeWidth: 1.5, icon: "📦" },
  service: { fill: "#ffffff", stroke: "#93c5fd", strokeWidth: 1.5, icon: "⚙️" },
  "security-component": { fill: "#fff1f2", stroke: "#fca5a5", strokeWidth: 2, icon: "🛡️" },
  tool: { fill: "#ffffff", stroke: "#bfdbfe", strokeWidth: 1.5, icon: "🔧" },
  database: { fill: "#ffffff", stroke: "#d8b4fe", strokeWidth: 1.5, icon: "💾" },
  "external-system": { fill: "#fffbeb", stroke: "#fde68a", strokeWidth: 1.5, icon: "🌐" },
};

export const DEFAULT_NODE_STYLE: NodeStyle = {
  fill: "#ffffff",
  stroke: "#cbd5e1",
  strokeWidth: 1.5,
  icon: "📄",
};

export function nodeStyle(type: string): NodeStyle {
  return NODE_STYLES[type] ?? DEFAULT_NODE_STYLE;
}

export interface ZoneStyleDefaults {
  readonly fill: string;
  readonly stroke: string;
  readonly strokeWidth: number;
  readonly strokeDasharray: string;
  readonly headerBg: string;
}

export const ZONE_DEFAULTS: ZoneStyleDefaults = {
  fill: "#f8fafc",
  stroke: "#cbd5e1",
  strokeWidth: 2,
  strokeDasharray: "none",
  headerBg: "#e2e8f0",
};

/**
 * Height of a container's header, and therefore the height it renders at when
 * collapsed (R-REND-04, R-REND-06).
 *
 * One constant. The original had 34 in the renderer and 36 in hit testing and
 * fit-to-view — that was D-01.
 */
export const HEADER_HEIGHT = 34;

/** Opacity applied to elements outside the active view (R-VIEW-03/04/05). */
export const DIM = {
  zone: 0.25,
  node: 0.2,
  edge: 0.15,
} as const;
