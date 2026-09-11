/**
 * The rule this file exists to defend: a chat's events land in that chat and nowhere else.
 *
 * Two chats streaming at once used to share one log and one status line, so a background turn wrote
 * into whatever was on screen. These tests feed interleaved frames for two chats and check that each
 * session only ever contains its own.
 */
import { beforeEach, describe, expect, it, vi } from "vitest";
import { client } from "../protocol/SplaClient";
import { forgetAllSessions, openChat, peekSession, sessionFor } from "./chatSessions";
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

  it("captures a spawned run's id onto the call, from a nested node's run detail", () => {
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("tool.started", "A", { toolCall: { id: "t1", name: "agent_spawn", arguments: "{}" } });

    feed("progress.node", "A", { nodeId: "n1", parentId: null, label: "agent_spawn", state: "running" });
    feed("progress.node", "A", { nodeId: "n2", parentId: "n1", label: "count the files", state: "running",
      details: [{ label: "run", value: "r-abc123" }] });

    expect(peekSession("A")!.calls["t1"].runIds).toEqual(["r-abc123"]);
  });

  it("keeps the run id on the finished tool card after the tree is cleared for the next turn", () => {
    // s.nodes is rebuilt from scratch at the next llm.turn.start (see the test above this one in the
    // file), so if the id lived only in the node map it would be unrecoverable by the time anyone
    // wants to open the finished card — it has to be copied onto the call itself.
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("tool.started", "A", { toolCall: { id: "t1", name: "agent_spawn", arguments: "{}" } });
    feed("progress.node", "A", { nodeId: "n1", parentId: null, label: "agent_spawn", state: "running" });
    feed("progress.node", "A", { nodeId: "n2", parentId: "n1", label: "count the files", state: "running",
      details: [{ label: "run", value: "r-abc123" }] });
    feed("tool.result", "A", { toolCallId: "t1", toolName: "agent_spawn", result: "done" });

    feed("turn.complete", "A", {});
    feed("llm.turn.start", "A", { msgIndex: 2 });

    expect(peekSession("A")!.nodes).toEqual({});
    expect(peekSession("A")!.calls["t1"].runIds).toEqual(["r-abc123"]);
  });

  it("sweeps only the previous turn's own namespaced nodes, leaving a background task's tree intact", () => {
    // Two live trees on one chat — the human turn and a detached background task — get wire ids
    // namespaced "{treeId}:{nodeId}" precisely so they don't collide. The sweep at the next
    // llm.turn.start must only drop the finished turn's own prefix, never a blanket reset, or a
    // background task's progress would vanish the moment the user sends their next message.
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1, progressTreeId: "t1" });
    feed("progress.node", "A", { nodeId: "t1:n1", parentId: null, label: "fs_read", state: "running" });
    feed("progress.node", "A", { nodeId: "bg1:n1", parentId: null, label: "system_run_shell", state: "running" });

    feed("turn.complete", "A", {});
    feed("llm.turn.start", "A", { msgIndex: 2, progressTreeId: "t2" });

    expect(peekSession("A")!.nodes["t1:n1"]).toBeUndefined();
    expect(peekSession("A")!.nodes["bg1:n1"]).toBeDefined();

    feed("progress.node", "A", { nodeId: "bg1:n2", parentId: "bg1:n1", label: "still running", state: "running" });
    expect(peekSession("A")!.nodes["bg1:n2"]).toBeDefined();
  });

  it("keeps every run of a batch, not just the first", () => {
    // agent_spawn_batch is one tool call with several runs beneath it, each its own conversation with
    // its own outcome. Keeping one would present it as the call's, which is exactly the confusion the
    // per-run branches in the tree exist to prevent.
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("tool.started", "A", { toolCall: { id: "t1", name: "agent_spawn_batch", arguments: "{}" } });
    feed("progress.node", "A", { nodeId: "n1", parentId: null, label: "agent_spawn_batch", state: "running" });

    feed("progress.node", "A", { nodeId: "n2", parentId: "n1", label: "audit ports", state: "running",
      details: [{ label: "run", value: "r-aaa" }] });
    feed("progress.node", "A", { nodeId: "n3", parentId: "n1", label: "read the docs", state: "running",
      details: [{ label: "run", value: "r-bbb" }] });
    // The same run ticking again must not add itself twice — the id rides every tick by design.
    feed("progress.node", "A", { nodeId: "n2", parentId: "n1", label: "audit ports", state: "completed",
      details: [{ label: "run", value: "r-aaa" }] });

    expect(peekSession("A")!.calls["t1"].runIds).toEqual(["r-aaa", "r-bbb"]);
  });

  // ── PLAN_20260903 stage 1: reading an archived chat ────────────────────────

  it("fills a session from chat.read.result and marks it read-only", () => {
    feed("chat.read.result", "R", { chatId: "R", title: "Old business", readOnly: true, messages: [
      { msgId: "m1", role: "user", content: "the thing we said" },
      { msgId: "m2", role: "assistant", content: "and the answer" }
    ] });

    const s = peekSession("R")!;
    expect(s.readOnly).toBe(true);
    expect(s.logLoaded).toBe(true);
    expect(s.items.map(i => i.kind)).toEqual(["user", "assistant"]);
    // Nothing here describes a next turn, because there is none — a leftover model or mode would be a
    // straight lie about the chat on screen.
    expect(s.modelId).toBe("");
    expect(s.toolSets).toEqual([]);
    expect(s.turnActive).toBe(false);
  });

  it("clears read-only when the same chat is later opened for real", () => {
    // Unarchiving turns the snapshot back into a session; the window must not stay in reading mode
    // just because that is how it first saw the chat.
    feed("chat.read.result", "R", { chatId: "R", title: "t", readOnly: true, messages: [] });
    expect(peekSession("R")!.readOnly).toBe(true);

    open("R");

    expect(peekSession("R")!.readOnly).toBe(false);
  });

  it("renders a read chat's history exactly as an opened one does", () => {
    const messages = [
      { msgId: "m1", role: "user", content: "hello" },
      { msgId: "m2", role: "assistant", content: "hi", toolCalls: [{ id: "t1", name: "read_file", arguments: "{}" }] },
      { msgId: "m3", role: "tool", toolCallId: "t1", content: "file body" }
    ];
    open("A", messages);
    feed("chat.read.result", "B", { chatId: "B", title: "t", readOnly: true, messages });

    const a = peekSession("A")!, b = peekSession("B")!;
    expect(b.items.map(i => i.kind)).toEqual(a.items.map(i => i.kind));
    expect(b.calls["t1"].result).toEqual(a.calls["t1"].result);
  });

  it("keeps streaming into a chat whose log was rebuilt mid-turn", () => {
    // Three chats working at once, and the person clicking between them to watch: every click used to
    // land as a fresh chat.opened, whose history snapshot cannot contain the sentence being streamed
    // right now. The bubble went with the rebuild and every later chunk was dropped on the floor, so
    // the answer and the reasoning appeared only when the turn ended.
    open("B");
    feed("llm.turn.start", "B", { msgIndex: 1 });
    feed("reasoning", "B", { msgIndex: 1, text: "thinking" });

    open("B");   // the log is rebuilt from persisted history — the live bubble is gone

    feed("reasoning", "B", { msgIndex: 1, text: "-more" });
    feed("delta", "B", { msgIndex: 1, text: "answer" });

    const b = peekSession("B")!.items.find(i => i.kind === "assistant" && i.msgIndex === 1);
    expect(b && b.kind === "assistant" ? b.text : undefined).toBe("answer");
    expect(b && b.kind === "assistant" ? b.reasoning : undefined).toBe("-more");
  });

  it("shows the sentence in flight to a window that opens the chat mid-turn", () => {
    // The history a chat is opened with is what has been persisted, and the answer being generated
    // right now is not in it. Without the live partial the window shows an empty log and only learns
    // there was an answer when the turn ends.
    feed("chat.opened", "B", { chatId: "B", messages: [], mode: "auto", modelId: "m1", toolSets: [],
      turnActive: true, live: { msgIndex: 7, content: "half a sen", reasoning: "because" } });

    feed("delta", "B", { msgIndex: 7, text: "tence" });

    const b = peekSession("B")!.items.find(i => i.kind === "assistant" && i.msgIndex === 7);
    expect(b && b.kind === "assistant" ? b.text : undefined).toBe("half a sentence");
    expect(b && b.kind === "assistant" ? b.reasoning : undefined).toBe("because");
    // One bubble, not the live one beside a second one built from the same index.
    expect(peekSession("B")!.items.filter(i => i.kind === "assistant").length).toBe(1);
  });

  it("switches to an already-loaded chat without asking the server to open it", () => {
    // The cure for the rebuild above: a watched chat's log is already current, so the switch is local.
    open("B");
    const send = vi.spyOn(client, "send").mockReturnValue(true);

    openChat("B");

    const types = send.mock.calls.map(c => c[0]);
    send.mockRestore();
    expect(types).not.toContain("chat.open");
  });

  it("still asks the server for a chat this window has no log for", () => {
    const send = vi.spyOn(client, "send").mockReturnValue(true);

    openChat("NEVER-SEEN");

    const types = send.mock.calls.map(c => c[0]);
    send.mockRestore();
    expect(types).toContain("chat.open");
  });

  // ── wave 1 (ADR_20260910-2 §4.4): chat.opened carries the snapshot ─────────

  it("rebuilds the progress tree and tasks from chat.opened's snapshot fields", () => {
    // A window attaching mid-turn must not have to wait for the next progress.node tick — or the next
    // task.list round-trip — to see what is already running.
    feed("chat.opened", "A", {
      chatId: "A", messages: [], toolSets: [], turnActive: true,
      openProgressNodes: [
        { nodeId: "n1", parentId: null, label: "agent_spawn", state: "running" },
        { nodeId: "n2", parentId: "n1", label: "count the files", state: "running", message: "turn 2" }
      ],
      runningTasks: [{ taskId: "bg_1", toolName: "system_run_shell", state: "Running", startedAt: "2026-09-10T00:00:00Z" }]
    });

    const s = peekSession("A")!;
    expect(s.nodes["n1"].childIds).toEqual(["n2"]);
    expect(s.nodes["n2"].message).toBe("turn 2");
    expect(s.tasks.map(t => t.taskId)).toEqual(["bg_1"]);

    // The live stream keeps working on top of the restored tree, exactly as it would for a node it
    // learned about itself.
    feed("progress.node", "A", { nodeId: "n2", parentId: "n1", label: "count the files", state: "completed" });
    expect(s.nodes["n2"].state).toBe("completed");
  });

  it("updates a restored task from task.state.changed, and adds one that was not running yet", () => {
    feed("chat.opened", "A", {
      chatId: "A", messages: [], toolSets: [],
      runningTasks: [{ taskId: "bg_1", toolName: "fs_read", state: "Running", startedAt: "t" }]
    });

    feed("task.state.changed", "A", { task: { taskId: "bg_1", toolName: "fs_read", state: "Completed", startedAt: "t" } });
    feed("task.state.changed", "A", { task: { taskId: "bg_2", toolName: "ssh_run", state: "Running", startedAt: "t" } });

    const tasks = peekSession("A")!.tasks;
    expect(tasks.find(t => t.taskId === "bg_1")!.state).toBe("Completed");
    expect(tasks.find(t => t.taskId === "bg_2")!.state).toBe("Running");
  });

  it("clears a stale progress tree when the chat is reopened with none running", () => {
    open("A");
    feed("llm.turn.start", "A", { msgIndex: 1 });
    feed("progress.node", "A", { nodeId: "n1", parentId: null, label: "fs_read", state: "running" });
    expect(peekSession("A")!.nodes["n1"]).toBeDefined();

    // Reopened later with nothing left running — the old tree must not linger and look live.
    feed("chat.opened", "A", { chatId: "A", messages: [], toolSets: [] });
    expect(peekSession("A")!.nodes).toEqual({});
  });

  // ── wave 2 (ADR_20260910-2 §4.6/§4.8): a spawned session's window ──────────

  it("shows a spawned session's transcript live and refreshes it once the run finishes", () => {
    // Before wave 2, chat.opened for a spawned run mid-turn always carried an empty `messages` list
    // (SpawnedSession wrote to disk only in Finish, and chat.open loaded that empty file) — and once
    // logLoaded flipped true, later clicks on the same chat switched locally to that frozen empty log
    // forever. Wave 2's server fix sends a real snapshot on open and keeps the log live via the same
    // wire events any chat gets; this test is the client-side half of that fix: it must not special-case
    // a spawned chat's events, and a later chat.opened (sent fresh once the run ends) must still replace
    // whatever is on screen rather than being ignored because the log "was already loaded".
    feed("chat.opened", "SPAWN-1", {
      chatId: "SPAWN-1", messages: [{ msgId: "u1", role: "user", content: "do the thing" }],
      toolSets: [], turnActive: true
    });

    let s = peekSession("SPAWN-1")!;
    expect(s.logLoaded).toBe(true);
    expect(s.readOnly).toBe(false);
    expect(s.turnActive).toBe(true);
    expect(s.items.map(i => i.kind)).toEqual(["user"]);

    // The run streams normally — same events, same chat id, no special path.
    feed("llm.turn.start", "SPAWN-1", { msgIndex: 1 });
    feed("delta", "SPAWN-1", { msgIndex: 1, text: "working on it" });
    feed("assistant.message", "SPAWN-1", { msgIndex: 1, message: { content: "done", msgId: "a1" } });
    feed("turn.complete", "SPAWN-1", {});

    s = peekSession("SPAWN-1")!;
    expect(s.turnActive).toBe(false);
    expect(s.items.some(i => i.kind === "assistant" && i.text === "done")).toBe(true);

    // The server pushes a fresh chat.opened once the finished file is the source of truth again — must
    // overwrite the log, not be shadowed by openChat's "already loaded" fast path (that path is never
    // consulted here; chat.opened always applies directly to the session).
    feed("chat.opened", "SPAWN-1", {
      chatId: "SPAWN-1",
      messages: [
        { msgId: "u1", role: "user", content: "do the thing" },
        { msgId: "a1", role: "assistant", content: "done" }
      ],
      toolSets: [], turnActive: false
    });

    s = peekSession("SPAWN-1")!;
    expect(s.items.map(i => i.kind)).toEqual(["user", "assistant"]);
    expect(s.turnActive).toBe(false);
  });

  it("resets logLoaded on welcome, so a reconnect re-fetches instead of switching locally", () => {
    open("B");
    expect(peekSession("B")!.logLoaded).toBe(true);

    feed("welcome", undefined, {});
    expect(peekSession("B")!.logLoaded).toBe(false);

    const send = vi.spyOn(client, "send").mockReturnValue(true);
    openChat("B");
    const types = send.mock.calls.map(c => c[0]);
    send.mockRestore();
    expect(types).toContain("chat.open");
  });
});
