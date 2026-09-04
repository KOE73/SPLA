/**
 * Pure helpers over the correspondence graph's wire shape (`CorrespondenceEdgeDto` —
 * PLAN_20260902 wave 7б, ADR_20260827-2 §2.5's last row). The edges themselves come straight off
 * the wire; this module answers the one question the graph exists to answer: which roles are
 * imbalanced — a role that only ever sends, and a role nobody answers.
 */
import type { CorrespondenceEdgeDto } from "../protocol/types";

/** Per-role traffic totals the imbalance view is built from. */
export interface RoleTraffic {
  role: string;
  /** Replies this role sent, across every edge where it appears as either side. */
  sent: number;
  /** Estimated token volume of everything this role sent. */
  sentVolume: number;
  /** Replies this role received back. */
  received: number;
  /** Estimated token volume of everything this role received. */
  receivedVolume: number;
}

/**
 * Rolls every edge's two directional counters up into one row per role. A role can appear as the
 * initiator on one edge and the correspondent on another (e.g. an architect who calls a writer and
 * is also called by a reviewer), so totals are accumulated across ALL edges the role touches, not
 * read off a single edge.
 */
export function rollUpByRole(edges: CorrespondenceEdgeDto[]): RoleTraffic[] {
  const byRole = new Map<string, RoleTraffic>();
  const get = (role: string): RoleTraffic => {
    let row = byRole.get(role);
    if (!row) { row = { role, sent: 0, sentVolume: 0, received: 0, receivedVolume: 0 }; byRole.set(role, row); }
    return row;
  };

  for (const e of edges) {
    const from = get(e.fromRole);
    from.sent += e.repliesFromInitiator;
    from.sentVolume += e.volumeFromInitiator;
    from.received += e.repliesFromCorrespondent;
    from.receivedVolume += e.volumeFromCorrespondent;

    const to = get(e.toRole);
    to.sent += e.repliesFromCorrespondent;
    to.sentVolume += e.volumeFromCorrespondent;
    to.received += e.repliesFromInitiator;
    to.receivedVolume += e.volumeFromInitiator;
  }

  return [...byRole.values()];
}

/** A role that has sent at least one reply but never received one back — "only ever talks". */
export function onlyTalks(edges: CorrespondenceEdgeDto[]): RoleTraffic[] {
  return rollUpByRole(edges).filter(r => r.sent > 0 && r.received === 0);
}

/** A role that has received at least one reply but never sent one back — "nobody answers... except
 *  this one only answers and never speaks first" reversed: this is the one nobody hears FROM. Kept
 *  distinct from {@link onlyTalks} because the two are not opposite ends of the same edge — a third
 *  role entirely can be the one always answered and never initiating. */
export function neverReplies(edges: CorrespondenceEdgeDto[]): RoleTraffic[] {
  return rollUpByRole(edges).filter(r => r.received > 0 && r.sent === 0);
}

// ── Drawing the thing ────────────────────────────────────────────────────────
// Everything below turns the wire's edge list into coordinates. It is deliberately pure and lives
// here rather than in the view: a layout is arithmetic, and arithmetic is the part worth testing.

/** One arrow in the drawn graph: a role pair, both directions rolled together. */
export interface RolePairEdge {
  from: string;
  to: string;
  /** Replies (and estimated tokens) travelling from → to. */
  sent: number;
  sentVolume: number;
  /** …and back again. */
  back: number;
  backVolume: number;
  /** How many distinct chat pairs this one arrow stands for — several chats can share two roles. */
  pairs: number;
}

/**
 * Collapses the wire's per-CHAT edges into per-ROLE-PAIR arrows. The raw list has one row per
 * (fromChat, toChat, topic), so the same two roles talking in three different chats read as three
 * identical-looking rows — which is exactly what made the text list confusing. A pair is
 * direction-normalised (a↔b is one arrow, counted both ways) so it is drawn once.
 */
export function aggregateByRolePair(edges: CorrespondenceEdgeDto[]): RolePairEdge[] {
  const byPair = new Map<string, RolePairEdge>();

  for (const e of edges) {
    // Normalised key so b→a lands on the same arrow as a→b; `flipped` says which way this row runs
    // relative to the stored orientation.
    const flipped = e.toRole < e.fromRole;
    const [from, to] = flipped ? [e.toRole, e.fromRole] : [e.fromRole, e.toRole];
    const key = `${from}\u0000${to}`;

    let row = byPair.get(key);
    if (!row) { row = { from, to, sent: 0, sentVolume: 0, back: 0, backVolume: 0, pairs: 0 }; byPair.set(key, row); }

    if (flipped) {
      row.back += e.repliesFromInitiator;
      row.backVolume += e.volumeFromInitiator;
      row.sent += e.repliesFromCorrespondent;
      row.sentVolume += e.volumeFromCorrespondent;
    } else {
      row.sent += e.repliesFromInitiator;
      row.sentVolume += e.volumeFromInitiator;
      row.back += e.repliesFromCorrespondent;
      row.backVolume += e.volumeFromCorrespondent;
    }
    row.pairs++;
  }

  return [...byPair.values()];
}

/** A placed node: the role, its traffic, and where its box goes. */
export interface GraphNode extends RoleTraffic {
  x: number;
  y: number;
}

/** A laid-out graph, in its own coordinate space — the view scales it with a viewBox. */
export interface GraphLayout {
  nodes: GraphNode[];
  edges: RolePairEdge[];
  width: number;
  height: number;
}

export const NODE_W = 150;
export const NODE_H = 46;
const COL_GAP = 90;
const ROW_GAP = 34;   // enough room for a stacked pair's own arrow to be read between the boxes
const PAD = 12;

/**
 * Places roles in columns by how far they sit from someone who only ever initiates.
 *
 * Not a force layout and not trying to be: with a handful of roles, "who starts, who is spoken to"
 * IS the shape worth seeing, and a deterministic left-to-right reading of it beats a prettier
 * arrangement that lands somewhere new every time the panel opens. Roles nobody initiates towards
 * (in-degree zero) start at the left; each role reached from them sits one column further right.
 * A pure cycle has no such starting point, so the busiest talker is used as one.
 */
export function layoutGraph(edges: CorrespondenceEdgeDto[]): GraphLayout {
  const traffic = rollUpByRole(edges);
  const pairs = aggregateByRolePair(edges);
  if (!traffic.length) return { nodes: [], edges: [], width: 0, height: 0 };

  // Direction for ranking is "who spoke first on this pair" — the side with more replies sent.
  const out = new Map<string, string[]>();
  const inDegree = new Map<string, number>(traffic.map(t => [t.role, 0]));
  for (const p of pairs) {
    const [from, to] = p.sent >= p.back ? [p.from, p.to] : [p.to, p.from];
    out.set(from, [...(out.get(from) ?? []), to]);
    inDegree.set(to, (inDegree.get(to) ?? 0) + 1);
  }

  const roots = traffic.filter(t => (inDegree.get(t.role) ?? 0) === 0).map(t => t.role);
  const seeds = roots.length
    ? roots
    : [traffic.slice().sort((a, b) => b.sent - a.sent)[0].role];   // all cycle: start at the loudest

  const depth = new Map<string, number>(seeds.map(r => [r, 0]));
  const queue = [...seeds];
  while (queue.length) {
    const role = queue.shift()!;
    for (const next of out.get(role) ?? []) {
      if (depth.has(next)) continue;                                // first path wins; no cycling
      depth.set(next, (depth.get(role) ?? 0) + 1);
      queue.push(next);
    }
  }
  for (const t of traffic) if (!depth.has(t.role)) depth.set(t.role, 0);   // unreachable: own column

  // Group by column, then spread each column vertically about a common middle.
  const columns = new Map<number, RoleTraffic[]>();
  for (const t of traffic) {
    const d = depth.get(t.role)!;
    columns.set(d, [...(columns.get(d) ?? []), t]);
  }
  const tallest = Math.max(...[...columns.values()].map(c => c.length));
  const height = PAD * 2 + tallest * NODE_H + (tallest - 1) * ROW_GAP;
  const colCount = columns.size;
  const width = PAD * 2 + colCount * NODE_W + (colCount - 1) * COL_GAP;

  const nodes: GraphNode[] = [];
  for (const [d, column] of [...columns.entries()].sort((a, b) => a[0] - b[0])) {
    const colHeight = column.length * NODE_H + (column.length - 1) * ROW_GAP;
    const top = (height - colHeight) / 2;
    const order = [...columns.keys()].sort((a, b) => a - b).indexOf(d);
    column.forEach((t, i) => nodes.push({
      ...t,
      x: PAD + order * (NODE_W + COL_GAP),
      y: top + i * (NODE_H + ROW_GAP)
    }));
  }

  return { nodes, edges: pairs, width, height };
}
