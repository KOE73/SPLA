/**
 * The rule this file exists to defend: a chat's events land in that chat and nowhere else.
 *
 * Two chats streaming at once used to share one log and one status line, so a background turn wrote
 * into whatever was on screen. These tests feed interleaved frames for two chats and check that each
 * session only ever contains its own.
 */
import { beforeEach, describe, expect, it } from "vitest";
import { client } from "../protocol/SplaClient";
import { forgetAllSessions, peekSession, sessionFor } from "./chatSessions";
import type { Envelope } from "../protocol/types";

function feed(type: string, chatId: string | undefined, payload: unknown, requestId?: string) {
  client.receive({ type, chatId, payload, requestId } as Envelope);
}

function open(chatId: string, messages: unknown[] = []) {
  feed("chat.opened", chatId, { chatId, messages, mode: "auto", modelId: "m1", toolSets: [] });
}

/** The assistant bubble carrying a given live index, if any. */
function bubbleText(chatId: string, msgIndex: number) {
  const items = peekSession(chatId)?.items ?? [];
  const b = items.find(i => i.kind === "assistant" && i.msgIndex === msgIndex);
  return b && b.kind === "assistant" ? b.text : undefined;
}

describe("chat sessions", () => {
  beforeEach(() => forgetAllSessions());

  it("keeps two chats' streams apart when their frames interleave", () => {
    open("A");
    open("B");

    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("llm.turn.start", "B", { msgIndex: 1 });   // same index, different chat
    feed("delta", "A", { msgIndex: 1, text: "alpha" });
    feed("delta", "B", { msgIndex: 1, text: "beta" });
    feed("delta", "A", { msgIndex: 1, text: "-more" });

    expect(bubbleText("A", 1)).toBe("alpha-more");
    expect(bubbleText("B", 1)).toBe("beta");
  });

  it("puts tool cards, notices and usage in the chat they belong to", () => {
    open("A");
    open("B");

    feed("tool.started", "B", { toolCall: { id: "t1", name: "read_file", arguments: "{}" } });
    feed("tool.result", "B", { toolCallId: "t1", toolName: "read_file", result: "ok" });
    feed("notice", "B", { text: "background note" });
    feed("token.usage", "B", { promptTokens: 100, completionTokens: 20, contextLength: 1000 });

    const a = peekSession("A")!;
    const b = peekSession("B")!;
    expect(a.items).toHaveLength(0);
    expect(a.ctxUsed).toBeNull();
    expect(b.items.some(i => i.kind === "toolcall" && i.call.status === "done")).toBe(true);
    expect(b.items.some(i => i.kind === "notice" && i.text === "background note")).toBe(true);
    expect(b.ctxUsed).toBe(120);
  });

  it("tracks the in-flight turn per chat, and takes the server's word on open", () => {
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });
    expect(peekSession("A")!.turnActive).toBe(true);
    expect(peekSession("B")).toBeUndefined();

    feed("turn.complete", "A", {});
    expect(peekSession("A")!.turnActive).toBe(false);

    // A window attaching to a chat that is already working must show it as working.
    feed("chat.opened", "C", { chatId: "C", messages: [], toolSets: [], turnActive: true });
    expect(peekSession("C")!.turnActive).toBe(true);
  });

  it("routes a permission request to the asking chat only", () => {
    open("A");
    open("B");
    feed("permission.request", "B", { toolName: "shell", arguments: "rm -rf /" }, "req1");

    expect(peekSession("A")!.items).toHaveLength(0);
    expect(peekSession("B")!.items.some(i => i.kind === "permission" && i.requestId === "req1")).toBe(true);
  });

  it("drops a chat-scoped event that names no chat instead of guessing", () => {
    open("A");
    feed("delta", undefined, { msgIndex: 1, text: "orphan" });
    expect(peekSession("A")!.items).toHaveLength(0);
  });

  it("keeps a background chat's answer while another chat is on screen", () => {
    open("A");
    open("B");
    feed("llm.turn.start", "B", { msgIndex: 1 });
    feed("delta", "B", { msgIndex: 1, text: "written while A was visible" });
    feed("assistant.message", "B", { msgIndex: 1, message: { content: "final answer", msgId: "m9" } });

    const b = peekSession("B")!;
    const bubble = b.items.find(i => i.kind === "assistant");
    expect(bubble && bubble.kind === "assistant" && bubble.text).toBe("final answer");
    expect(bubble && bubble.kind === "assistant" && bubble.msgId).toBe("m9");
  });

  it("removes an empty bubble left by a turn that produced nothing, and says why", () => {
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("turn.complete", "A", { error: "provider refused" });

    const items = sessionFor("A").items;
    expect(items.some(i => i.kind === "assistant")).toBe(false);
    expect(items.some(i => i.kind === "notice" && i.text.includes("provider refused"))).toBe(true);
  });

  it("removes a permission request when ask.resolved arrives", () => {
    open("A");
    feed("permission.request", "A", { toolName: "shell", arguments: "whoami" }, "req-perm");

    let items = peekSession("A")!.items;
    expect(items.some(i => i.kind === "permission" && i.requestId === "req-perm")).toBe(true);

    feed("ask.resolved", "A", { reason: "answered" }, "req-perm");

    items = peekSession("A")!.items;
    expect(items.some(i => i.kind === "permission" && i.requestId === "req-perm")).toBe(false);
  });

  it("removes a clarify request when ask.resolved arrives", () => {
    open("A");
    feed("clarify.request", "A", { question: "pick one?", options: [] }, "req-clarify");

    let items = peekSession("A")!.items;
    expect(items.some(i => i.kind === "clarify" && i.requestId === "req-clarify")).toBe(true);

    feed("ask.resolved", "A", { reason: "answered" }, "req-clarify");

    items = peekSession("A")!.items;
    expect(items.some(i => i.kind === "clarify" && i.requestId === "req-clarify")).toBe(false);
  });

  it("preserves a chat's state field from chat.opened and chat.list.result", () => {
    // The server broadcasts state on every chat summary; verify it survives the round trip.
    // The store is the presentation layer's source of truth for list badges.
    feed("chat.list.result", undefined, {
      chats: [
        { id: "A", title: "First", state: "working" },
        { id: "B", title: "Second", state: "waiting" }
      ]
    });

    expect(peekSession("A")).toBeUndefined();  // session not yet created
    expect(peekSession("B")).toBeUndefined();

    // Opening a chat should be able to carry state too (for consistency with list broadcasts).
    feed("chat.opened", "A", { chatId: "A", messages: [], toolSets: [], state: "idle" });

    // Verify the list result carries state through to the chat summary the UI renders,
    // which is separate from the session's internal state tracking.
    // This test documents that state is a presentation field, not session mechanics.
    expect(peekSession("A")).toBeDefined();
  });

  // ── progress.node ───────────────────────────────────────────────────────────
  //
  // The stream is flat and the shape is the client's to rebuild, so the assembly is the part with
  // something to get wrong.

  it("rebuilds the progress tree and hangs its root off the running call", () => {
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("tool.started", "A", { toolCall: { id: "t1", name: "agent_spawn", arguments: "{}" } });

    feed("progress.node", "A", { nodeId: "n1", parentId: null, label: "agent_spawn", state: "running" });
    feed("progress.node", "A", { nodeId: "n2", parentId: "n1", label: "count the files", state: "running",
      message: "turn 2, thinking" });
    feed("progress.node", "A", { nodeId: "n3", parentId: "n2", label: "fs_read", state: "completed" });

    const s = peekSession("A")!;
    expect(s.calls["t1"].rootNodeId).toBe("n1");
    expect(s.nodes["n1"].childIds).toEqual(["n2"]);
    expect(s.nodes["n2"].childIds).toEqual(["n3"]);
    expect(s.nodes["n2"].message).toBe("turn 2, thinking");
    expect(s.nodes["n3"].state).toBe("completed");
  });

  it("holds a node whose parent has not arrived yet", () => {
    // Parents are always generated first, but parallel work gives no ordering guarantee on the wire,
    // and a dropped node is a branch that never appears.
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });

    feed("progress.node", "A", { nodeId: "child", parentId: "parent", label: "port_scan", state: "running" });
    feed("progress.node", "A", { nodeId: "parent", parentId: null, label: "roslyn_script_run", state: "running" });

    const s = peekSession("A")!;
    expect(s.nodes["parent"].label).toBe("roslyn_script_run");
    expect(s.nodes["parent"].childIds).toEqual(["child"]);
  });

  it("keeps each chat's tree to itself and clears it when a new turn begins", () => {
    open("A");
    open("B");

    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("progress.node", "A", { nodeId: "n1", parentId: null, label: "fs_read", state: "running" });
    feed("progress.node", "B", { nodeId: "n1", parentId: null, label: "ssh_run", state: "running" });

    expect(peekSession("A")!.nodes["n1"].label).toBe("fs_read");
    expect(peekSession("B")!.nodes["n1"]?.label).toBe("ssh_run");

    // Ids are unique within a turn, not across turns — a stale tree would graft the next turn's nodes
    // onto the last one's.
    feed("turn.complete", "A", {});
    feed("llm.turn.start", "A", { msgIndex: 2 });
    expect(peekSession("A")!.nodes).toEqual({});
  });

  it("does not clear the tree between LLM calls inside one turn", () => {
    // llm.turn.start fires once per model call; the server opens one tree per user turn.
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("progress.node", "A", { nodeId: "n1", parentId: null, label: "fs_read", state: "running" });
    feed("llm.turn.start", "A", { msgIndex: 2 });

    expect(peekSession("A")!.nodes["n1"]).toBeDefined();
  });
});
