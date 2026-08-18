/**
 * Per-chat state, and the ONE place the server's chat-scoped events are dispatched.
 *
 * Why this exists. Every chat-scoped frame carries the chat it belongs to, but the log, the composer
 * and the status line each subscribed to the socket directly and each decided for itself whether to
 * check that id. Most did not, so a background chat's turn rendered into the visible chat's log and
 * overwrote its status line — and every new handler was one more chance to forget the check. Here the
 * check happens once, before any consumer sees anything: an event without a resolvable chat is
 * dropped, and everything else lands in ITS OWN session. Components read a session; they never
 * subscribe to chat traffic and never look at "the current chat" while handling an event.
 *
 * Memory. A session has a cheap head (flags, counters, draft) kept for every chat the window has
 * touched, and an expensive log (messages, tool cards) kept only where it is needed: the chat on
 * screen, chats with a turn in flight, and a couple of recently visited ones. Evicting a log drops
 * the watch with it, so the server stops sending that chat's stream. Coming back re-opens the chat —
 * the server is the source of truth for history anyway.
 */
import { reactive } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "./store";
import type { ChatDoubt, ToolSetState } from "../protocol/types";
import type { ToolCallState } from "../surfaces/ToolCard.vue";

export type LogItem =
  | { kind: "user"; key: string; text: string; images?: string[]; msgId?: string; createdAt?: string | number }
  | { kind: "assistant"; key: string; msgIndex: number; text: string; reasoning: string;
      msgId?: string; createdAt?: string | number;
      /** Generations the repetition guard threw away before this bubble got its real answer — the
       *  streamed text visible up to that point belonged to one of these, not to the final message.
       *  content/reasoning are the abandoned generation's own text, present live (from `llm.attempt`)
       *  and after reopening a chat saved with `agent: save_attempts` on. */
      attempts?: { index: number; note?: string; content?: string; reasoning?: string }[] }
  | { kind: "tool" | "notice"; key: string; text: string }
  | { kind: "toolcall"; key: string; call: ToolCallState }
  | { kind: "permission"; key: string; requestId: string; toolName: string; argumentsText?: string }
  | { kind: "clarify"; key: string; requestId: string; question: string;
      options?: { label: string; description?: string }[] };

export interface ChatSession {
  chatId: string;
  projectId: string | null;

  // ── Head: cheap, kept for every chat this window has touched ──────────────
  turnActive: boolean;
  mode: string;
  modelId: string;
  /** The two per-turn knobs, as this chat currently runs. `reasoning` is the scalar grammar
   *  ("" | "off" | "on" | an effort word | "budget:N"); what the model will accept is a separate,
   *  provider-described question the status bar asks for on its own. */
  temperature: number | null;
  reasoning: string;
  activeSkill: string | null;
  toolSets: ToolSetState[];
  doubt: ChatDoubt | null;
  lastPrompt: number | null;
  lastCompletion: number | null;
  ctxUsed: number | null;
  ctxWindow: number | null;
  /** Unsent composer text and images — per chat, or switching away loses them (and, worse, could
   *  send an image attached in one chat to another). */
  draft: string;
  attachments: string[];

  // ── Log: expensive, evictable ─────────────────────────────────────────────
  /** False when the log has never been loaded, or was dropped to save memory. */
  logLoaded: boolean;
  items: LogItem[];
  /** Bubbles opened by llm.turn.start that no assistant.message has closed. A turn that dies in the
   *  provider call leaves one behind — it is the empty box that used to be a failure's only trace. */
  pending: number[];
  /** Live tool calls by id, so tool.progress/result can mutate the card in place. */
  calls: Record<string, ToolCallState>;
}

/** How many chats keep their log. A weak client keeps fewer; a chat mid-turn is never counted out. */
const LOG_BUDGET = (() => {
  const mem = (navigator as { deviceMemory?: number }).deviceMemory;
  return mem != null && mem <= 4 ? 2 : 4;
})();

const sessions = reactive(new Map<string, ChatSession>());
/** Most-recently-focused first — the eviction order. */
const recency: string[] = [];
let keySeq = 0;
const nextKey = () => "i" + (keySeq++);

function blank(chatId: string): ChatSession {
  return {
    chatId,
    projectId: store.currentProjectId,
    turnActive: false,
    mode: "",
    modelId: "",
    temperature: null,
    reasoning: "",
    activeSkill: null,
    toolSets: [],
    doubt: null,
    lastPrompt: null,
    lastCompletion: null,
    ctxUsed: null,
    ctxWindow: null,
    draft: "",
    attachments: [],
    logLoaded: false,
    items: [],
    pending: [],
    calls: {}
  };
}

/** The session for a chat, created (as a bare head) if this window has not seen it before. */
export function sessionFor(chatId: string): ChatSession {
  let s = sessions.get(chatId);
  if (!s) { s = blank(chatId); sessions.set(chatId, s); }
  return s;
}

/** The session for a chat, or undefined — for readers that must not create one as a side effect. */
export function peekSession(chatId: string | null): ChatSession | undefined {
  return chatId ? sessions.get(chatId) : undefined;
}

/** Marks a chat as the one on screen and prunes logs that no longer fit the budget. */
export function focusSession(chatId: string): ChatSession {
  const s = sessionFor(chatId);
  const at = recency.indexOf(chatId);
  if (at >= 0) recency.splice(at, 1);
  recency.unshift(chatId);
  evict();
  return s;
}

/**
 * Drops the logs of chats that are neither on screen, nor working, nor recent enough to matter.
 * Dropping a log also drops the watch: from that moment the server stops streaming that chat here,
 * which is the point — an unwatched chat is exactly one whose events we have nowhere to put.
 */
function evict() {
  let kept = 0;
  for (const chatId of recency) {
    const s = sessions.get(chatId);
    if (!s?.logLoaded) continue;
    const mustKeep = chatId === recency[0] || s.turnActive || kept < LOG_BUDGET;
    if (mustKeep) { kept++; continue; }
    dropLog(s);
  }
}

function dropLog(s: ChatSession) {
  s.logLoaded = false;
  s.items = [];
  s.pending = [];
  s.calls = {};
  client.send("chat.unwatch", { chatId: s.chatId }, s.projectId ? { projectId: s.projectId } : undefined);
}

// ── Log helpers ──────────────────────────────────────────────────────────────

function addCall(s: ChatSession, call: ToolCallState): ToolCallState {
  s.items.push({ kind: "toolcall", key: nextKey(), call });
  // Take the reactive proxy back, so later mutations re-render.
  const stored = (s.items[s.items.length - 1] as { call: ToolCallState }).call;
  if (call.callId) s.calls[call.callId] = stored;
  return stored;
}

/** Fallback for events that arrive without a usable toolCallId. */
function lastRunningByName(s: ChatSession, name: string): ToolCallState | undefined {
  for (let i = s.items.length - 1; i >= 0; i--) {
    const it = s.items[i];
    if (it.kind === "toolcall" && it.call.status === "running" && it.call.name === name) return it.call;
  }
  return undefined;
}

function bubble(s: ChatSession, msgIndex: number) {
  return s.items.find((i): i is Extract<LogItem, { kind: "assistant" }> =>
    i.kind === "assistant" && i.msgIndex === msgIndex);
}

/** Drops a bubble that never received text or reasoning. True if one was actually removed.
 *  A bubble carrying attempt markers is NOT empty even though its text is: the guard cleared that
 *  text on purpose, and the markers are the only remaining trace of the generations that were thrown
 *  away. Dropping it would erase the explanation in exactly the case that needs one. */
function discardEmptyBubble(s: ChatSession, msgIndex: number): boolean {
  const b = bubble(s, msgIndex);
  if (!b || b.text || b.reasoning || b.attempts?.length) return false;
  s.items = s.items.filter(i => !(i.kind === "assistant" && i.msgIndex === msgIndex));
  return true;
}

export function addNotice(s: ChatSession, text: string) {
  s.items.push({ kind: "notice", key: nextKey(), text });
}

export function addLocalUserMessage(s: ChatSession, text: string, images?: string[]) {
  s.items.push({ kind: "user", key: nextKey(), text, images, createdAt: Date.now() });
}

export function dropItem(s: ChatSession, predicate: (i: LogItem) => boolean) {
  s.items = s.items.filter(i => !predicate(i));
}

// ── The demultiplexer ────────────────────────────────────────────────────────

/**
 * Chat-scoped events are only ever applied to the session named by the envelope. There is no
 * "fall back to whatever chat is open": that fallback is precisely how a background chat's stream
 * ended up in the visible log, and an event we cannot place is better dropped than misfiled.
 */
function on<P>(type: string, apply: (s: ChatSession, payload: P) => void) {
  client.on(type as never, ((payload: P, env: { chatId?: string }) => {
    if (!env.chatId) {
      console.warn("chat-scoped event without chatId — dropped:", type);
      return;
    }
    apply(sessionFor(env.chatId), payload);
  }) as never);
}

client.on("chat.opened", (p, env) => {
  const s = sessionFor(p.chatId || env.chatId || "");
  s.projectId = store.currentProjectId;
  s.items = [];
  s.pending = [];
  s.calls = {};
  s.logLoaded = true;
  s.mode = p.mode || s.mode;
  s.modelId = p.modelId || "";
  s.temperature = p.temperature ?? null;
  s.reasoning = p.reasoning ?? "";
  s.activeSkill = p.activeSkillId || null;
  s.toolSets = p.toolSets || [];
  s.doubt = p.doubt ?? null;
  s.turnActive = !!p.turnActive;
  // Another chat has a different history size — a stale percentage is a lie until its first turn.
  s.lastPrompt = s.lastCompletion = s.ctxUsed = s.ctxWindow = null;

  for (const m of p.messages) {
    if (m.role === "user") {
      s.items.push({ kind: "user", key: nextKey(), text: m.content || "",
        images: m.images, msgId: m.msgId, createdAt: m.createdAt });
    } else if (m.role === "assistant") {
      // A degenerate-turn record has blank content/reasoning and exists purely for its attempts —
      // still worth a bubble, since dropping it would erase the only trace of what happened.
      if (m.content || m.reasoning || m.attempts?.length)
        // Historical bubbles get negative indices: the server's live indices are positive, so the two
        // can never collide inside one session.
        s.items.push({ kind: "assistant", key: nextKey(), msgIndex: -1 - s.items.length,
          text: m.content || "", reasoning: m.reasoning || "", msgId: m.msgId, createdAt: m.createdAt,
          attempts: m.attempts?.map(a => ({ index: a.index, note: a.note, content: a.content, reasoning: a.reasoning })) });
      for (const tc of m.toolCalls || [])
        addCall(s, { callId: tc.id, name: tc.name, argumentsText: tc.arguments, status: "done" });
    } else if (m.role === "tool") {
      const call = m.toolCallId ? s.calls[m.toolCallId] : undefined;
      if (call) call.result = m.content || "";
      else s.items.push({ kind: "tool", key: nextKey(),
        text: "← tool result (" + (m.content || "").length + " chars)" });
    }
  }
});

on("user.message", (s, p: { msgId: string; text?: string; createdAt?: string }) => {
  // Attach the server MsgId to the composer's optimistic echo. A server-initiated startup turn has
  // no echo, so its optional text becomes a real bubble instead.
  for (let i = s.items.length - 1; i >= 0; i--) {
    const it = s.items[i];
    if (it.kind === "user" && !it.msgId) {
      it.msgId = p.msgId;
      if (p.createdAt) it.createdAt = p.createdAt;
      return;
    }
  }
  if (p.text !== undefined)
    s.items.push({ kind: "user", key: nextKey(), text: p.text, msgId: p.msgId, createdAt: p.createdAt });
});

on("llm.turn.start", (s, p: { msgIndex: number }) => {
  s.turnActive = true;
  s.items.push({ kind: "assistant", key: "a" + p.msgIndex, msgIndex: p.msgIndex, text: "", reasoning: "",
    createdAt: Date.now() });
  s.pending.push(p.msgIndex);
});

on("delta", (s, p: { msgIndex: number; text: string }) => {
  const b = bubble(s, p.msgIndex);
  if (b) b.text += p.text;
});

on("reasoning", (s, p: { msgIndex: number; text: string }) => {
  const b = bubble(s, p.msgIndex);
  if (b) b.reasoning += p.text;
});

on("llm.attempt", (s, p: { msgIndex: number; index: number; note?: string; content?: string; reasoning?: string }) => {
  const b = bubble(s, p.msgIndex);
  if (!b) return;
  // Everything streamed into this bubble so far belonged to the generation that just got thrown
  // away — clear it so the retry (or the notice, if this was the last one) starts from empty instead
  // of the reader seeing a loop's tail glued to the real answer. The text is not lost: it rides along
  // on the marker itself (p.content/p.reasoning), which is what lets the reader open it back up.
  b.text = "";
  b.reasoning = "";
  (b.attempts ??= []).push({ index: p.index, note: p.note, content: p.content, reasoning: p.reasoning });
});

on("assistant.message", (s, p: { msgIndex: number;
  message: { content?: string; reasoning?: string; msgId?: string; createdAt?: string } }) => {
  s.pending = s.pending.filter(i => i !== p.msgIndex);
  let b = bubble(s, p.msgIndex);
  if (!b) {
    s.items.push({ kind: "assistant", key: "a" + p.msgIndex, msgIndex: p.msgIndex, text: "", reasoning: "" });
    b = bubble(s, p.msgIndex)!;
  }
  b.text = p.message.content || "";
  if (p.message.reasoning) b.reasoning = p.message.reasoning;
  if (p.message.msgId) b.msgId = p.message.msgId;
  if (p.message.createdAt) b.createdAt = p.message.createdAt;
});

on("tool.started", (s, p: { toolCall: { id: string; name: string; arguments: string } }) => {
  s.turnActive = true;
  addCall(s, { callId: p.toolCall.id, name: p.toolCall.name, argumentsText: p.toolCall.arguments,
    status: "running", startedAt: Date.now() });
});

on("tool.progress", (s, p: { toolCallId?: string; toolName: string;
  fraction?: number; message?: string; details?: { label: string; value: string }[] }) => {
  const call = (p.toolCallId && s.calls[p.toolCallId]) || lastRunningByName(s, p.toolName);
  if (call) call.progress = { fraction: p.fraction, message: p.message, details: p.details };
});

on("tool.result", (s, p: { toolCallId: string; toolName: string; result?: string }) => {
  const call = s.calls[p.toolCallId] || lastRunningByName(s, p.toolName);
  if (call) { call.result = p.result || ""; call.status = "done"; call.finishedAt = Date.now(); }
  else addNotice(s, "← " + p.toolName + " (" + (p.result || "").length + " chars)");
});

on("notice", (s, p: { text: string }) => addNotice(s, p.text));

on("token.usage", (s, p: { promptTokens?: number; completionTokens?: number; contextLength?: number }) => {
  s.lastPrompt = p.promptTokens ?? null;
  s.lastCompletion = p.completionTokens ?? null;
  if (p.promptTokens != null) {
    s.ctxUsed = p.promptTokens + (p.completionTokens ?? 0);
    s.ctxWindow = p.contextLength ?? null;
  }
});

// Permission/clarify carry their correlation id on the envelope, not in the payload, so they are
// wired directly rather than through the payload-only helper above. The chatId check is the same
// one — and here it also matters for safety: a permission dialog must never appear over a chat other
// than the one whose tool is asking.
client.on("permission.request", (p, env) => {
  if (!env.chatId || !env.requestId) return;
  sessionFor(env.chatId).items.push({ kind: "permission", key: nextKey(), requestId: env.requestId,
    toolName: p.toolName, argumentsText: p.arguments });
});
client.on("clarify.request", (p, env) => {
  if (!env.chatId || !env.requestId) return;
  sessionFor(env.chatId).items.push({ kind: "clarify", key: nextKey(), requestId: env.requestId,
    question: p.question, options: p.options });
});

on("chat.skill.state", (s, p: { activeSkillId?: string | null }) => { s.activeSkill = p.activeSkillId || null; });
on("chat.toolset.state", (s, p: { sets?: ToolSetState[] }) => { s.toolSets = p.sets || []; });
on("chat.doubt.state", (s, p: { doubt: ChatDoubt }) => { s.doubt = p.doubt; });

on("turn.complete", (s, p: { error?: string; cancelled?: boolean; activeSkillId?: string | null }) => {
  s.turnActive = false;
  s.activeSkill = p.activeSkillId || null;

  // A cancelled or failed turn can strand cards mid-spin.
  for (const it of s.items)
    if (it.kind === "toolcall" && it.call.status === "running") {
      it.call.status = "done";
      it.call.finishedAt = Date.now();
    }

  // A bubble opened for a turn that produced nothing is not an answer, it is the shape of a failure.
  let discarded = false;
  for (const msgIndex of s.pending) discarded = discardEmptyBubble(s, msgIndex) || discarded;
  s.pending = [];

  if (p.error) addNotice(s, "⚠ Turn failed: " + p.error);
  else if (p.cancelled) addNotice(s, "⏹ Turn cancelled.");
  else if (discarded) addNotice(s, "⚠ The model returned no output for this turn.");

  // A chat that has stopped working no longer earns its log.
  evict();
});

/**
 * Connection-level errors carry no chat. They are shown in the chat on screen because that is where
 * the person is looking — the one deliberate exception to "never fall back to the current chat", and
 * it is safe precisely because the frame belongs to no chat at all.
 */
client.on("error", (p, env) => {
  const s = peekSession(env.chatId ?? store.currentChat);
  if (s) addNotice(s, "⚠ " + p.message);
});

/** A deleted chat leaves nothing behind. */
export function forgetSession(chatId: string) {
  sessions.delete(chatId);
  const at = recency.indexOf(chatId);
  if (at >= 0) recency.splice(at, 1);
}

/** Switching projects: none of these chats are reachable from the new one. */
export function forgetAllSessions() {
  sessions.clear();
  recency.length = 0;
}
