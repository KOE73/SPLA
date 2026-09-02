/**
 * Pure helpers over the chat list's tree shape (chatTree.ts) — wave 7's client-side counterpart to
 * the service's `RuntimeProjections.List` (PLAN_20260902, ADR_20260827-2 §2.5). Both walkers must
 * agree on what a tree looks like: a spawned session anywhere at any depth, and a title lookup that
 * degrades to the id itself for an orphaned/deleted parent rather than throwing or guessing.
 */
import { describe, expect, it } from "vitest";
import { collectSpawned, titleOf } from "./chatTree";
import type { ChatSummary } from "../protocol/types";

function chat(id: string, extra: Partial<ChatSummary> = {}): ChatSummary {
  return { id, title: `title-${id}`, ...extra };
}

describe("collectSpawned", () => {
  it("returns nothing for a flat list with no spawned sessions", () => {
    const list = [chat("A"), chat("B")];
    expect(collectSpawned(list)).toEqual([]);
  });

  it("finds a top-level spawned entry", () => {
    const spawned = chat("S", { origin: "spawned" });
    const list = [chat("A"), spawned];
    expect(collectSpawned(list)).toEqual([spawned]);
  });

  it("finds a spawned session nested under its parent", () => {
    const spawned = chat("S", { origin: "spawned", parent: "A" });
    const list = [chat("A", { children: [spawned] })];
    expect(collectSpawned(list)).toEqual([spawned]);
  });

  it("walks arbitrarily deep nesting, depth-first", () => {
    const leaf = chat("leaf", { origin: "spawned", parent: "mid" });
    const mid = chat("mid", { origin: "spawned", parent: "top", children: [leaf] });
    const top = chat("top", { children: [mid] });

    expect(collectSpawned([top])).toEqual([mid, leaf]);
  });

  it("collects every child across multiple parents and multiple roots", () => {
    const s1 = chat("s1", { origin: "spawned", parent: "A" });
    const s2 = chat("s2", { origin: "spawned", parent: "B" });
    const list = [chat("A", { children: [s1] }), chat("B", { children: [s2] })];

    expect(collectSpawned(list)).toEqual([s1, s2]);
  });

  it("does not mistake a human chat carrying a role (`as`) for a spawned one", () => {
    // A plain human chat can be role-narrowed (wave 5б's role-narrowed standing chat) without ever
    // being spawned — `origin` is the only thing that means "spawned", not `as`.
    const list = [chat("A", { as: "reviewer" })];
    expect(collectSpawned(list)).toEqual([]);
  });

  it("appends into a caller-supplied accumulator rather than replacing it", () => {
    const spawned = chat("S", { origin: "spawned" });
    const acc: ChatSummary[] = [chat("already-there", { origin: "spawned" })];
    const result = collectSpawned([spawned], acc);

    expect(result).toBe(acc);
    expect(result.map(c => c.id)).toEqual(["already-there", "S"]);
  });
});

describe("titleOf", () => {
  it("finds a top-level chat's title", () => {
    const list = [chat("A", { title: "Parent chat" })];
    expect(titleOf(list, "A")).toBe("Parent chat");
  });

  it("finds a nested chat's title without needing to know its depth", () => {
    const leaf = chat("leaf", { title: "Leaf chat" });
    const list = [chat("top", { children: [chat("mid", { children: [leaf] })] })];
    expect(titleOf(list, "leaf")).toBe("Leaf chat");
  });

  it("falls back to the id when the chat is not found anywhere in the tree", () => {
    const list = [chat("A", { children: [chat("B")] })];
    expect(titleOf(list, "no-such-chat")).toBe("no-such-chat");
  });

  it("falls back to the id when a chat exists but has no title", () => {
    const list = [{ id: "A" } as ChatSummary];
    expect(titleOf(list, "A")).toBe("A");
  });

  it("does not confuse an id that happens to equal another chat's title", () => {
    // titleOf's early-return check ("found !== id") is subtle: this pins the found title winning
    // even when it looks the same shape as an id, and that the search does not stop early on a
    // sibling that isn't the target.
    const target = chat("target", { title: "some title" });
    const list = [chat("other", { children: [target] }), chat("target-decoy")];
    expect(titleOf(list, "target")).toBe("some title");
  });
});
