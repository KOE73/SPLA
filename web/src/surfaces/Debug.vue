<template>
  <div class="debug-surface">
    <header>
      <b>{{ t('Debug') }}</b>
      <span v-if="chatLabel" class="chat-label" :title="'chat: ' + (store.currentChat ?? '')">{{ chatLabel }}</span>
      <button class="refresh" :title="t('Refresh now')" @click="reload">⟳</button>
      <button v-if="!solo" class="filter" @click="close">{{ t('close') }}</button>
    </header>
    <div class="tabs">
      <button v-for="tab in TABS" :key="tab.kind" class="tab" :class="{ on: activeKind === tab.kind }" @click="request(tab.kind)">{{ tab.label }}</button>
    </div>
    <div id="debugBody" :class="{ 'ctx-mode': !!snapshot?.contextLines }">
      <ContextTable v-if="snapshot?.contextLines" :snapshot="snapshot" />
      <!-- What has actually moved between perimeters. Nothing here is refused: this is the record
           the decision to start refusing will be made from, which is why it shows traffic and not
           rules. -->
      <template v-if="snapshot?.edges">
        <div v-if="!snapshot.edges.length" class="edge-empty">
          {{ t('Nothing has crossed a perimeter yet in this process.') }}
        </div>
        <template v-else>
          <div class="kv-head">
            <span class="e-move">{{ t('movement') }}</span><span class="e-eff">{{ t('effect') }}</span>
            <span class="e-n">{{ t('calls') }}</span><span class="v">{{ t('last tool') }}</span>
          </div>
          <div v-for="(e, i) in snapshot.edges" :key="i" class="kv-row">
            <span class="e-move" :class="{ outward: e.outward }">{{ e.source }} → {{ e.sink }}</span>
            <span class="e-eff">{{ e.effect }}</span>
            <span class="e-n">{{ e.calls }}</span>
            <span class="v">{{ e.lastTool }}</span>
          </div>
        </template>
      </template>
      <template v-else-if="snapshot?.entries">
        <div v-if="!snapshot.entries.length">{{ t('(empty)') }}</div>
        <!-- Origin is its own column, never folded into the value: the question this view has to
             answer at a glance is "which of these came from outside", and a label buried in text is
             a label nobody scans for. -->
        <div class="kv-head">
          <span class="k">{{ t('key') }}</span><span class="o">{{ t('origin') }}</span><span class="v">{{ t('value') }}</span>
        </div>
        <div v-for="(e, i) in snapshot.entries" :key="i" class="kv-row">
          <span class="k">{{ e.key }}</span>
          <span class="o" :class="{ doubtful: e.doubtful }" :title="e.doubtful ? 'from a source nobody named' : ''">
            {{ e.origin ?? "—" }}
          </span>
          <span class="v">{{ e.value }}</span>
        </div>
      </template>
      <!-- Composition manifest: what the agent's context is made of, and who contributed each piece.
           Bodies are collapsed — the question this view answers first is "why is this here and what
           does it cost", not "what does it say". -->
      <template v-else-if="snapshot?.segments?.length">
        <div class="manifest-head">
          {{ snapshot.segments.length }} contributions · ~{{ snapshot.approxTokens }} tokens (estimate)
        </div>
        <div v-for="(s, i) in snapshot.segments" :key="i" class="manifest-item">
          <button class="manifest-row" :class="{ failed: !!s.problem }" @click="toggle(i)">
            <span class="caret">{{ open.has(i) ? "▾" : "▸" }}</span>
            <span class="who">{{ s.contributor }}</span>
            <span class="what">{{ s.source }}</span>
            <span class="place">{{ s.placement }}</span>
            <span class="cost">~{{ s.approxTokens }}</span>
          </button>
          <pre v-if="open.has(i)" class="manifest-body">{{ s.body }}</pre>
        </div>
      </template>
      <pre v-else-if="snapshot?.text != null" style="white-space: pre-wrap">{{ snapshot.text }}</pre>
    </div>
  </div>
</template>

<script setup lang="ts">
import { t } from "../i18n";
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { uiBus } from "../state/uiBus";
import { findChat } from "../state/chatTree";
import type { DebugSnapshotPayload } from "../protocol/types";
import ContextTable from "./ContextTable.vue";

// Standalone window (e.g. a tear-off ?surface=debug): no drawer chrome, auto-load immediately.
const solo = !!new URLSearchParams(location.search).get("surface");

const TABS = [
  { kind: "kv.session", label: "session kv" },
  { kind: "kv.project", label: "project kv" },
  { kind: "blobs", label: "blobs" },
  { kind: "edges", label: "edges" },
  { kind: "context.last", label: "context" },
  { kind: "prompt", label: "prompt" },
  // The full instance-tracking dump: what this process thinks it is doing, what its lock file
  // claims, and every question it is blocked on. A developer view, not a user one — it renders
  // through the generic key/value branch above precisely because completeness beats presentation.
  { kind: "instances", label: "instances" }
] as const;

const activeKind = ref<string>("kv.session");
const snapshot = ref<DebugSnapshotPayload | null>(null);
const isOpen = ref(false);
/** Which manifest rows are expanded. Reset on every fetch — row indexes are only meaningful
 *  for the snapshot they came from. */
const open = ref(new Set<number>());

function toggle(i: number) {
  const next = new Set(open.value);
  next.has(i) ? next.delete(i) : next.add(i);
  open.value = next;
}

// The tab/row label for whichever chat this panel is showing — same lookup ChatListItem draws its
// role badge and title from, so "which chat is this?" reads the same way here as it does in the list.
const chatLabel = computed(() => {
  const id = store.currentChat;
  if (!id) return "";
  const c = findChat(store.chats, id) ?? findChat(store.archivedChats, id);
  if (!c) return id;
  return c.as ? `${c.as}: ${c.title || c.id}` : c.title || c.id;
});

function request(kind: string) {
  activeKind.value = kind;
  client.send("debug.request", { kind }, store.currentChat ? { chatId: store.currentChat } : undefined);
}
function reload() { request(activeKind.value); }
function close() { isOpen.value = false; document.getElementById("debug")?.classList.remove("open"); }

let refreshTimer = 0;
function scheduleRefresh() {
  clearTimeout(refreshTimer);
  refreshTimer = window.setTimeout(() => { if (solo || isOpen.value) reload(); }, 400);
}

const offSnapshot = client.on("debug.snapshot", p => { snapshot.value = p; open.value = new Set(); });
const offToolResult = client.on("tool.result", scheduleRefresh);
const offTurnComplete = client.on("turn.complete", scheduleRefresh);
// A mode/model/reasoning change echoes back as chat.opened (see ChatHandlers.Settings) — refresh right
// then, before any turn runs, rather than waiting for tool.result/turn.complete to happen to fire.
const offChatSettingsEcho = client.on("chat.opened", (_p, env) => {
  if (env.chatId && env.chatId === store.currentChat) scheduleRefresh();
});
const offOpen = uiBus.on("debug.open", () => {
  isOpen.value = true;
  document.getElementById("debug")?.classList.add("open");
  request("kv.session");
});

// Follows whichever chat is on screen — an embedded drawer as much as a solo tear-off window. Without
// this the panel was static: opening it once and then switching chats kept showing the first chat's
// snapshot forever, because nothing ever asked again.
watch(() => store.currentChat, () => { if (solo || isOpen.value) reload(); });

// A tear-off panel follows the focused chat, so it watches one chat at a time — and must drop the
// previous one. Without that, a window left open all day accumulates watches and keeps receiving the
// turn events of every chat it has ever followed.
let watched: string | null = null;

function watchAndReload() {
  const next = store.currentChat;
  if (next !== watched) {
    if (watched) client.send("chat.unwatch", { chatId: watched });
    if (next) client.send("chat.watch", { chatId: next });
    watched = next;
  }
  reload();
}
const offWelcome = solo ? client.on("welcome", watchAndReload) : () => {};
const offFocus = solo ? client.on("focus.changed", watchAndReload) : () => {};
const offChatOpened = solo ? client.on("chat.opened", watchAndReload) : () => {};

onMounted(() => { if (solo) request("kv.session"); });
onUnmounted(() => {
  offSnapshot(); offToolResult(); offTurnComplete(); offChatSettingsEcho();
  offOpen(); offWelcome(); offFocus(); offChatOpened();
});
</script>
