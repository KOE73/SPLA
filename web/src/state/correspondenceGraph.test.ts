/**
 * Pure helpers over the correspondence graph (correspondenceGraph.ts) — wave 7б's client-side
 * imbalance view (PLAN_20260902, ADR_20260827-2 §2.5's last row: "первый вид, на котором видно
 * перекос — роль, которая только говорит, и роль, которую никто не слушает").
 */
import { describe, expect, it } from "vitest";
import { neverReplies, onlyTalks, rollUpByRole } from "./correspondenceGraph";
import type { CorrespondenceEdgeDto } from "../protocol/types";

function edge(extra: Partial<CorrespondenceEdgeDto> = {}): CorrespondenceEdgeDto {
  return {
    fromChatId: "A", fromRole: "architect",
    toChatId: "B", toRole: "writer",
    topic: "design review",
    repliesFromInitiator: 0, volumeFromInitiator: 0,
    repliesFromCorrespondent: 0, volumeFromCorrespondent: 0,
    ...extra,
  };
}

describe("rollUpByRole", () => {
  it("sums a single edge's two directions onto its two roles", () => {
    const edges = [edge({ repliesFromInitiator: 3, volumeFromInitiator: 30, repliesFromCorrespondent: 1, volumeFromCorrespondent: 10 })];
    const rows = rollUpByRole(edges);

    const architect = rows.find(r => r.role === "architect")!;
    expect(architect).toEqual({ role: "architect", sent: 3, sentVolume: 30, received: 1, receivedVolume: 10 });

    const writer = rows.find(r => r.role === "writer")!;
    expect(writer).toEqual({ role: "writer", sent: 1, sentVolume: 10, received: 3, receivedVolume: 30 });
  });

  it("accumulates across multiple edges touching the same role from different sides", () => {
    const edges = [
      edge({ fromRole: "architect", toRole: "writer", repliesFromInitiator: 2, repliesFromCorrespondent: 0 }),
      edge({ fromRole: "reviewer", toRole: "architect", repliesFromInitiator: 5, repliesFromCorrespondent: 1 }),
    ];
    const architect = rollUpByRole(edges).find(r => r.role === "architect")!;

    // architect: sent 2 (as initiator of edge 1) + 1 (as correspondent of edge 2) = 3
    // architect: received 0 (edge 1) + 5 (as correspondent of edge 2) = 5
    expect(architect.sent).toBe(3);
    expect(architect.received).toBe(5);
  });

  it("returns one row per role with no duplicates", () => {
    const edges = [edge(), edge({ toRole: "writer", topic: "another topic" })];
    const rows = rollUpByRole(edges);
    expect(rows.map(r => r.role).sort()).toEqual(["architect", "writer"]);
  });
});

describe("onlyTalks", () => {
  it("flags a role that has sent replies but never received any", () => {
    const edges = [edge({ repliesFromInitiator: 4, repliesFromCorrespondent: 0 })];
    const flagged = onlyTalks(edges);
    expect(flagged.map(r => r.role)).toEqual(["architect"]);
  });

  it("does not flag a role once at least one reply comes back", () => {
    const edges = [edge({ repliesFromInitiator: 4, repliesFromCorrespondent: 1 })];
    expect(onlyTalks(edges)).toEqual([]);
  });

  it("does not flag a role that has sent nothing at all", () => {
    const edges = [edge({ repliesFromInitiator: 0, repliesFromCorrespondent: 0 })];
    expect(onlyTalks(edges)).toEqual([]);
  });
});

describe("neverReplies", () => {
  it("flags a role that has received replies but never sent any back", () => {
    const edges = [edge({ repliesFromInitiator: 6, repliesFromCorrespondent: 0 })];
    const flagged = neverReplies(edges);
    expect(flagged.map(r => r.role)).toEqual(["writer"]);
  });

  it("does not flag a role once it has answered even once", () => {
    const edges = [edge({ repliesFromInitiator: 6, repliesFromCorrespondent: 1 })];
    expect(neverReplies(edges)).toEqual([]);
  });

  it("keeps onlyTalks and neverReplies as independent, non-overlapping questions", () => {
    // architect only sends, writer only receives — both flags fire, on different roles.
    const edges = [edge({ fromRole: "architect", toRole: "writer", repliesFromInitiator: 3, repliesFromCorrespondent: 0 })];
    expect(onlyTalks(edges).map(r => r.role)).toEqual(["architect"]);
    expect(neverReplies(edges).map(r => r.role)).toEqual(["writer"]);
  });
});
