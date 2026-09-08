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
import ReplyOutLine from "./ReplyOutLine.vue";
import PermissionAsk from "./PermissionAsk.vue";
import ClarifyAsk from "./ClarifyAsk.vue";
import { isReplyCall } from "../state/replyCalls";

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
    // A reply/correspondence call reads as speech, not a tool invocation (PLAN_20260902 wave 7;
    // ADR_20260827-2 §2.5: "реплика рендерится как речь"). Everything else keeps the ordinary card.
    case "toolcall": return isReplyCall(item.call.name) ? ReplyOutLine : ToolCard;
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

// Staying at the bottom is answered by MEASUREMENT, not by guessing when the growing stops.
//
// The old version polled every 120 ms while a turn ran, and stopped the moment the turn ended. Every
// height change that landed after the last tick — the final tokens, a bubble reflowing once its
// markdown/mermaid finished, an image arriving with its real height, the composer growing a row and
// taking that space off the viewport — left the view short by exactly that much. Hence "always a
// little bit left to scroll" while sitting at the bottom.
//
// A ResizeObserver fires after layout, for every one of those causes, and only when something
// actually changed. Two boxes are watched: the content (it grows) and the scroll viewport itself
// (it shrinks when the composer or the settings drawer takes room). Re-pinning is a scroll, not a
// layout write, so it cannot feed itself.
let ro: ResizeObserver | null = null;
/** Fallback for environments without ResizeObserver — the old timer, and only there. */
let ticker = 0;

function pin() {
  if (following) toBottom();
}

watch(() => items.value.length, follow);
watch(() => chat.session.value?.turnActive, active => {
  clearInterval(ticker);
  if (active && !ro) ticker = window.setInterval(follow, 120);
}, { immediate: true });

// A chat that has just been opened starts at the bottom — that is where the conversation is — and
// following starts on again, because this is a fresh look at a different chat.
watch(() => chat.chatId.value, () => {
  following = true;
  nextTick(toBottom);
}, { immediate: true });

onMounted(() => {
  const el = logEl();
  el?.addEventListener("scroll", onScroll, { passive: true });
  if (el && typeof ResizeObserver !== "undefined") {
    ro = new ResizeObserver(pin);
    clearInterval(ticker);                             // the turnActive watch ran before mount, when
    ticker = 0;                                        // it could not yet know an observer was coming
    ro.observe(el);                                    // the viewport: composer/drawer take its room
    // The content lives in the centring wrapper ChatSurface puts inside #log; this component renders
    // into it with display:contents, so that wrapper is the box whose height tracks the conversation.
    if (el.firstElementChild) ro.observe(el.firstElementChild);
  }
});
onUnmounted(() => {
  clearInterval(ticker);
  ro?.disconnect();
  ro = null;
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
