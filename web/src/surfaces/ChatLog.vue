<template>
  <div style="display: contents">
    <component
      :is="itemComponent(item)"
      v-for="item in items"
      :key="item.key"
      v-bind="itemProps(item)"
      :ref="item.kind === 'assistant' ? (el => setBubbleRef(item.msgIndex, el)) : undefined"
      @decide="onPermissionDecide"
      @choose="onClarifyChoose"
      @rewind="onRewind"
      @fork="onFork"
    />
  </div>
</template>

<script setup lang="ts">
import { nextTick, onUnmounted, ref, watch } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { uiBus } from "../state/uiBus";
import UserBubble from "./UserBubble.vue";
import AssistantBubble from "./AssistantBubble.vue";
import ToolLine from "./ToolLine.vue";
import ToolCard, { type ToolCallState } from "./ToolCard.vue";
import PermissionAsk from "./PermissionAsk.vue";
import ClarifyAsk from "./ClarifyAsk.vue";

type Item =
  | { kind: "user"; key: string; text: string; images?: string[]; msgId?: string; createdAt?: string | number }
  | { kind: "assistant"; key: string; msgIndex: number; msgId?: string; createdAt?: string | number }
  | { kind: "tool" | "notice"; key: string; text: string }
  | { kind: "toolcall"; key: string; call: ToolCallState }
  | { kind: "permission"; key: string; requestId: string; toolName: string; argumentsText?: string }
  | { kind: "clarify"; key: string; requestId: string; question: string; options?: { label: string; description?: string }[] };

interface AssistantBubbleHandle {
  renderInto(text: string): Promise<void>;
  setReasoning(text: string): void;
  appendReasoning(chunk: string): void;
}

const items = ref<Item[]>([]);
// This component renders directly into the #log slot owned by ChatSurface,
// which already carries the flex/scroll CSS — `display:contents` on our own root keeps us out
// of the box tree so .msg children become real flex items of #log, not of an extra inner div.
const logEl = () => document.getElementById("log");
let keySeq = 0;
const nextKey = () => "i" + (keySeq++);

// per-msgIndex streaming state — mirrors the old `B` object in app.js
const streams = new Map<number, { raw: string; timer: number }>();
const bubbleRefs = new Map<number, AssistantBubbleHandle>();
function setBubbleRef(msgIndex: number, el: unknown) {
  if (el) bubbleRefs.set(msgIndex, el as AssistantBubbleHandle);
  else bubbleRefs.delete(msgIndex);
}

function itemComponent(item: Item) {
  switch (item.kind) {
    case "user": return UserBubble;
    case "assistant": return AssistantBubble;
    case "tool": case "notice": return ToolLine;
    case "toolcall": return ToolCard;
    case "permission": return PermissionAsk;
    case "clarify": return ClarifyAsk;
  }
}
function itemProps(item: Item) {
  const { key, kind, ...rest } = item;
  return rest;
}

function scroll() {
  nextTick(() => { const el = logEl(); if (el) el.scrollTop = el.scrollHeight; });
}

// Live/loaded tool calls by id — tool.progress and tool.result mutate the card in place.
const toolCalls = new Map<string, ToolCallState>();
function addToolCall(call: ToolCallState) {
  const reactiveItem: Item = { kind: "toolcall", key: nextKey(), call };
  items.value.push(reactiveItem);
  // grab the reactive proxy back so later mutations trigger re-render
  const stored = (items.value[items.value.length - 1] as { call: ToolCallState }).call;
  if (call.callId) toolCalls.set(call.callId, stored);
  return stored;
}

function clearAll() {
  renderedChatId = null;
  items.value = [];
  toolCalls.clear();
  streams.forEach(s => clearTimeout(s.timer));
  streams.clear();
  bubbleRefs.clear();
}

function addAssistantItem(msgIndex: number, msgId?: string, createdAt?: string | number) {
  items.value.push({ kind: "assistant", key: "asst" + msgIndex, msgIndex, msgId, createdAt });
}

function startBubble(msgIndex: number) {
  addAssistantItem(msgIndex, undefined, Date.now());
  streams.set(msgIndex, { raw: "", timer: 0 });
}
function appendDelta(msgIndex: number, text: string) {
  const s = streams.get(msgIndex);
  if (!s) return;
  s.raw += text;
  clearTimeout(s.timer);
  s.timer = window.setTimeout(() => { bubbleRefs.get(msgIndex)?.renderInto(s.raw).then(scroll); }, 70);
}
function finalizeBubble(msgIndex: number, message: { content?: string; reasoning?: string }) {
  let s = streams.get(msgIndex);
  if (!s) { addAssistantItem(msgIndex); s = { raw: "", timer: 0 }; streams.set(msgIndex, s); }
  clearTimeout(s.timer);
  // The finalized message carries the server-side MsgId — attach it so rewind/fork can anchor here.
  const item = items.value.find(i => i.kind === "assistant" && i.msgIndex === msgIndex);
  if (item && item.kind === "assistant") {
    const m = message as { msgId?: string; createdAt?: string };
    if (m.msgId) item.msgId = m.msgId;
    if (m.createdAt) item.createdAt = m.createdAt;
  }
  if (message.reasoning) bubbleRefs.get(msgIndex)?.setReasoning(message.reasoning);
  nextTick(() => bubbleRefs.get(msgIndex)?.renderInto(message.content || "").then(scroll));
}

// The chat this log is actually rendering. Rewind/fork must anchor to THIS id, never to
// store.currentChat — focus.changed from another window can retarget the store mid-click,
// which would send a destructive rewind to a chat the user never looked at.
let renderedChatId: string | null = null;

function openChat(p: { chatId?: string; messages: { msgId?: string; role: string; content?: string; reasoning?: string;
  createdAt?: string; images?: string[];
  toolCalls?: { id: string; name: string; arguments: string }[]; toolCallId?: string }[] }) {
  clearAll();
  renderedChatId = p.chatId ?? null;
  for (const m of p.messages) {
    if (m.role === "user") items.value.push({ kind: "user", key: nextKey(), text: m.content || "",
      images: m.images, msgId: m.msgId, createdAt: m.createdAt });
    else if (m.role === "assistant") {
      if (m.content || m.reasoning) {
        const msgIndex = -1 - items.value.length; // synthetic stable index for historical messages
        addAssistantItem(msgIndex, m.msgId, m.createdAt);
        nextTick(() => {
          const b = bubbleRefs.get(msgIndex);
          b?.setReasoning(m.reasoning || "");
          b?.renderInto(m.content || "");
        });
      }
      // historical tool calls become collapsed done-cards; the matching tool message fills in the result
      for (const tc of m.toolCalls || [])
        addToolCall({ callId: tc.id, name: tc.name, argumentsText: tc.arguments, status: "done" });
    } else if (m.role === "tool") {
      const call = m.toolCallId ? toolCalls.get(m.toolCallId) : undefined;
      if (call) call.result = m.content || "";
      else items.value.push({ kind: "tool", key: nextKey(), text: "← tool result (" + (m.content || "").length + " chars)" });
    }
  }
  scroll();
}

// Fallback matcher for events that arrive without a usable toolCallId.
function lastRunningByName(name: string): ToolCallState | undefined {
  for (let i = items.value.length - 1; i >= 0; i--) {
    const it = items.value[i];
    if (it.kind === "toolcall" && it.call.status === "running" && it.call.name === name) return it.call;
  }
  return undefined;
}

function onPermissionDecide(requestId: string, decision: string) {
  client.send("permission.decision", { decision }, { requestId });
  items.value = items.value.filter(i => !(i.kind === "permission" && i.requestId === requestId));
}
function onClarifyChoose(requestId: string, choice: string | null) {
  client.send("clarify.choice", { choice }, { requestId });
  items.value = items.value.filter(i => !(i.kind === "clarify" && i.requestId === requestId));
}

// Rewind: a user bubble also passes its text (goes back to the composer and the message itself is
// removed); an assistant bubble is kept and only what follows it is discarded. The server answers
// with chat.opened, which rebuilds the log.
function onRewind(msgId: string, text?: string) {
  if (!renderedChatId || !msgId) return;
  if (text !== undefined) uiBus.emit("composer.set", { text });
  client.send("chat.rewind", { chatId: renderedChatId, msgId, before: text !== undefined },
    { projectId: store.currentProjectId ?? undefined });
}
function onFork(msgId: string) {
  if (!renderedChatId || !msgId) return;
  client.send("chat.fork", { chatId: renderedChatId, msgId },
    { projectId: store.currentProjectId ?? undefined });
}

// ── Wiring ───────────────────────────────────────────────────────────────────
const offs = [
  client.on("chat.opened", openChat),
  uiBus.on("local.userMsg", (p: unknown) => {
    const { text, images } = p as { text: string; images?: string[] };
    items.value.push({ kind: "user", key: nextKey(), text, images, createdAt: Date.now() });
    scroll();
  }),
  // Attach the server MsgId to the composer's optimistic local echo. A server-initiated startup
  // turn has no local echo, so its optional text becomes a real user bubble here.
  client.on("user.message", (p, env) => {
    if (env.chatId && env.chatId !== renderedChatId) return;
    let attached = false;
    for (let i = items.value.length - 1; i >= 0; i--) {
      const it = items.value[i];
      if (it.kind === "user" && !it.msgId) {
        it.msgId = p.msgId;
        if (p.createdAt) it.createdAt = p.createdAt;
        attached = true;
        break;
      }
    }
    if (!attached && p.text !== undefined)
      items.value.push({ kind: "user", key: nextKey(), text: p.text, msgId: p.msgId, createdAt: p.createdAt });
    scroll();
  }),
  client.on("llm.turn.start", p => startBubble(p.msgIndex)),
  client.on("delta", p => appendDelta(p.msgIndex, p.text)),
  client.on("reasoning", p => { bubbleRefs.get(p.msgIndex)?.appendReasoning(p.text); scroll(); }),
  client.on("assistant.message", p => finalizeBubble(p.msgIndex, p.message)),
  client.on("tool.started", p => {
    addToolCall({ callId: p.toolCall.id, name: p.toolCall.name, argumentsText: p.toolCall.arguments,
      status: "running", startedAt: Date.now() });
    scroll();
  }),
  client.on("tool.progress", p => {
    const call = (p.toolCallId && toolCalls.get(p.toolCallId)) || lastRunningByName(p.toolName);
    if (call) call.progress = { fraction: p.fraction, message: p.message, details: p.details };
  }),
  client.on("tool.result", p => {
    const call = toolCalls.get(p.toolCallId) || lastRunningByName(p.toolName);
    if (call) { call.result = p.result || ""; call.status = "done"; call.finishedAt = Date.now(); }
    else items.value.push({ kind: "tool", key: nextKey(), text: "← " + p.toolName + " (" + (p.result || "").length + " chars)" });
    scroll();
  }),
  // A cancelled/failed turn can strand cards in "running" — stop their spinners.
  client.on("turn.complete", () => {
    for (const it of items.value)
      if (it.kind === "toolcall" && it.call.status === "running") { it.call.status = "done"; it.call.finishedAt = Date.now(); }
  }),
  client.on("notice", p => { items.value.push({ kind: "notice", key: nextKey(), text: p.text }); scroll(); }),
  client.on("error", p => { items.value.push({ kind: "notice", key: nextKey(), text: "⚠ " + p.message }); scroll(); }),
];
const offPermission = client.on("permission.request", (p, env) => {
  items.value.push({ kind: "permission", key: nextKey(), requestId: env.requestId!, toolName: p.toolName, argumentsText: p.arguments });
  scroll();
});
const offClarify = client.on("clarify.request", (p, env) => {
  items.value.push({ kind: "clarify", key: nextKey(), requestId: env.requestId!, question: p.question, options: p.options });
  scroll();
});

// Mirrors old chat.cleared handling — ChatList sets store.currentChat = null on delete.
watch(() => store.currentChat, v => { if (v === null) clearAll(); });

onUnmounted(() => { offs.forEach(o => o()); offPermission(); offClarify(); streams.forEach(s => clearTimeout(s.timer)); });
</script>
