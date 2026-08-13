<!--
  The chat log — now a pure view over the session's items. It subscribes to nothing: every server
  event is placed into its chat's session by state/chatSessions, so a background chat's turn cannot
  appear here however busy it is. Switching chats swaps which list is rendered; it no longer destroys
  the other chat's streaming state.
-->
<template>
  <div style="display: contents">
    <component
      :is="itemComponent(item)"
      v-for="item in items"
      :key="item.key"
      v-bind="itemProps(item)"
      @decide="onPermissionDecide"
      @choose="onClarifyChoose"
      @rewind="onRewind"
      @fork="onFork"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, watch } from "vue";
import { client } from "../protocol/SplaClient";
import { useChat } from "../state/chatContext";
import { dropItem, type LogItem } from "../state/chatSessions";
import { uiBus } from "../state/uiBus";
import UserBubble from "./UserBubble.vue";
import AssistantBubble from "./AssistantBubble.vue";
import ToolLine from "./ToolLine.vue";
import ToolCard from "./ToolCard.vue";
import PermissionAsk from "./PermissionAsk.vue";
import ClarifyAsk from "./ClarifyAsk.vue";

const chat = useChat();
const items = computed<LogItem[]>(() => chat.session.value?.items ?? []);

// This component renders directly into the #log slot owned by ChatSurface, which already carries the
// flex/scroll CSS — `display:contents` on our own root keeps us out of the box tree so .msg children
// become real flex items of #log rather than of an extra inner div.
const logEl = () => document.getElementById("log");

function itemComponent(item: LogItem) {
  switch (item.kind) {
    case "user": return UserBubble;
    case "assistant": return AssistantBubble;
    case "tool": case "notice": return ToolLine;
    case "toolcall": return ToolCard;
    case "permission": return PermissionAsk;
    case "clarify": return ClarifyAsk;
  }
}
function itemProps(item: LogItem) {
  const { key, kind, ...rest } = item;
  return rest;
}

// ── Scrolling ────────────────────────────────────────────────────────────────
// Follow the stream only while the reader is already at the bottom. Scrolling up is a deliberate act
// — reading what happened while the answer keeps coming — and yanking the view back down every time
// a token arrives makes that impossible. Once they return to the bottom, following resumes on its own.
const STICK_PX = 80;

/** Whether the view is tied to the bottom. The reader owns this: scrolling away turns it off,
 *  scrolling back turns it on. Nothing else may set it — the point is that the code never decides
 *  to drag someone back down while they are reading. */
let following = true;

function atBottom(el: HTMLElement) {
  return el.scrollHeight - el.scrollTop - el.clientHeight <= STICK_PX;
}

function toBottom() {
  const el = logEl();
  if (el) el.scrollTop = el.scrollHeight;
}

function follow() {
  if (following) nextTick(toBottom);
}

function onScroll() {
  const el = logEl();
  if (el) following = atBottom(el);
}

// New entries are one signal; text growing inside the bubble that is streaming is the other, and it
// has no event of its own — so while a turn runs we look, cheaply, on a timer. A deep watch over the
// whole log would walk every message on every token.
let ticker = 0;
watch(() => items.value.length, follow);
watch(() => chat.session.value?.turnActive, active => {
  clearInterval(ticker);
  if (active) ticker = window.setInterval(follow, 120);
}, { immediate: true });

// A chat that has just been opened starts at the bottom — that is where the conversation is — and
// following starts on again, because this is a fresh look at a different chat.
watch(() => chat.chatId.value, () => {
  following = true;
  nextTick(toBottom);
}, { immediate: true });

onMounted(() => logEl()?.addEventListener("scroll", onScroll, { passive: true }));
onUnmounted(() => {
  clearInterval(ticker);
  logEl()?.removeEventListener("scroll", onScroll);
});

// ── Answering the chat's questions ───────────────────────────────────────────
function onPermissionDecide(requestId: string, decision: string) {
  client.send("permission.decision", { decision }, { requestId });
  const s = chat.session.value;
  if (s) dropItem(s, i => i.kind === "permission" && i.requestId === requestId);
}
function onClarifyChoose(requestId: string, choice: string | null) {
  client.send("clarify.choice", { choice }, { requestId });
  const s = chat.session.value;
  if (s) dropItem(s, i => i.kind === "clarify" && i.requestId === requestId);
}

// Rewind: a user bubble also passes its text (it goes back to the composer and the message itself is
// removed); an assistant bubble is kept and only what follows it is discarded. The server answers
// with chat.opened, which rebuilds the log. The chat is the surface's, so no id is passed here — that
// is exactly the mix-up this arrangement removes.
function onRewind(msgId: string, text?: string) {
  if (!msgId) return;
  if (text !== undefined) uiBus.emit("composer.set", { text });
  chat.send("chat.rewind", { msgId, before: text !== undefined });
}
function onFork(msgId: string) {
  if (msgId) chat.send("chat.fork", { msgId });
}
</script>
