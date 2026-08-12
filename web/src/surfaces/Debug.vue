<template>
  <div class="debug-surface">
    <header><b>Debug</b><button v-if="!solo" class="filter" @click="close">close</button></header>
    <div class="tabs">
      <button v-for="t in TABS" :key="t.kind" class="tab" :class="{ on: activeKind === t.kind }" @click="request(t.kind)">{{ t.label }}</button>
    </div>
    <div id="debugBody" :class="{ 'ctx-mode': !!snapshot?.contextLines }">
      <ContextTable v-if="snapshot?.contextLines" :snapshot="snapshot" />
      <template v-else-if="snapshot?.entries">
        <div v-if="!snapshot.entries.length">(empty)</div>
        <!-- Origin is its own column, never folded into the value: the question this view has to
             answer at a glance is "which of these came from outside", and a label buried in text is
             a label nobody scans for. -->
        <div class="kv-head">
          <span class="k">key</span><span class="o">origin</span><span class="v">value</span>
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
import { onMounted, onUnmounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { uiBus } from "../state/uiBus";
import type { DebugSnapshotPayload } from "../protocol/types";
import ContextTable from "./ContextTable.vue";

// Standalone window (e.g. a tear-off ?surface=debug): no drawer chrome, auto-load immediately.
const solo = !!new URLSearchParams(location.search).get("surface");

const TABS = [
  { kind: "kv.session", label: "session kv" },
  { kind: "kv.project", label: "project kv" },
  { kind: "blobs", label: "blobs" },
  { kind: "context.last", label: "context" },
  { kind: "prompt", label: "prompt" }
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
const offOpen = uiBus.on("debug.open", () => {
  isOpen.value = true;
  document.getElementById("debug")?.classList.add("open");
  request("kv.session");
});

function watchAndReload() {
  if (store.currentChat) client.send("chat.watch", { chatId: store.currentChat }, { projectId: store.currentProjectId ?? undefined });
  reload();
}
const offWelcome = solo ? client.on("welcome", watchAndReload) : () => {};
const offFocus = solo ? client.on("focus.changed", watchAndReload) : () => {};
const offChatOpened = solo ? client.on("chat.opened", watchAndReload) : () => {};

onMounted(() => { if (solo) request("kv.session"); });
onUnmounted(() => { offSnapshot(); offToolResult(); offTurnComplete(); offOpen(); offWelcome(); offFocus(); offChatOpened(); });
</script>
