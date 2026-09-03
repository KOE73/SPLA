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
