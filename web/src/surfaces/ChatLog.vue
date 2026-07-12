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
import PermissionAsk from "./PermissionAsk.vue";
import ClarifyAsk from "./ClarifyAsk.vue";

type Item =
  | { kind: "user"; key: string; text: string; images?: string[] }
  | { kind: "assistant"; key: string; msgIndex: number }
  | { kind: "tool" | "notice"; key: string; text: string }
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

function clearAll() {
  items.value = [];
  streams.forEach(s => clearTimeout(s.timer));
  streams.clear();
  bubbleRefs.clear();
}

function addAssistantItem(msgIndex: number) {
  items.value.push({ kind: "assistant", key: "asst" + msgIndex, msgIndex });
}

function startBubble(msgIndex: number) {
  addAssistantItem(msgIndex);
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
  if (message.reasoning) bubbleRefs.get(msgIndex)?.setReasoning(message.reasoning);
  nextTick(() => bubbleRefs.get(msgIndex)?.renderInto(message.content || "").then(scroll));
}

function openChat(p: { messages: { role: string; content?: string; reasoning?: string; images?: string[] }[] }) {
  clearAll();
  for (const m of p.messages) {
    if (m.role === "user") items.value.push({ kind: "user", key: nextKey(), text: m.content || "", images: m.images });
    else if (m.role === "assistant") {
      const msgIndex = -1 - items.value.length; // synthetic stable index for historical messages
      addAssistantItem(msgIndex);
      nextTick(() => {
        const b = bubbleRefs.get(msgIndex);
        b?.setReasoning(m.reasoning || "");
        b?.renderInto(m.content || "");
      });
    } else if (m.role === "tool") {
      items.value.push({ kind: "tool", key: nextKey(), text: "← tool result (" + (m.content || "").length + " chars)" });
    }
  }
  scroll();
}

function onPermissionDecide(requestId: string, decision: string) {
  client.send("permission.decision", { decision }, { requestId });
  items.value = items.value.filter(i => !(i.kind === "permission" && i.requestId === requestId));
}
function onClarifyChoose(requestId: string, choice: string | null) {
  client.send("clarify.choice", { choice }, { requestId });
  items.value = items.value.filter(i => !(i.kind === "clarify" && i.requestId === requestId));
}

// ── Wiring ───────────────────────────────────────────────────────────────────
const offs = [
  client.on("chat.opened", openChat),
  uiBus.on("local.userMsg", (p: unknown) => {
    const { text, images } = p as { text: string; images?: string[] };
    items.value.push({ kind: "user", key: nextKey(), text, images });
    scroll();
  }),
  client.on("llm.turn.start", p => startBubble(p.msgIndex)),
  client.on("delta", p => appendDelta(p.msgIndex, p.text)),
  client.on("reasoning", p => { bubbleRefs.get(p.msgIndex)?.appendReasoning(p.text); scroll(); }),
  client.on("assistant.message", p => finalizeBubble(p.msgIndex, p.message)),
  client.on("tool.started", p => { items.value.push({ kind: "tool", key: nextKey(), text: "→ " + p.toolCall.name }); scroll(); }),
  client.on("tool.result", p => { items.value.push({ kind: "tool", key: nextKey(), text: "← " + p.toolName + " (" + (p.result || "").length + " chars)" }); scroll(); }),
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
