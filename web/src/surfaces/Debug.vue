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
        <div v-for="(e, i) in snapshot.entries" :key="i" class="kv-row">
          <span class="k">{{ e.key }}</span><span class="v">{{ e.value }}</span>
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

const offSnapshot = client.on("debug.snapshot", p => { snapshot.value = p; });
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
