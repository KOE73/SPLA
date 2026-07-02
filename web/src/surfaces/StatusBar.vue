<template>
  <span><span id="dot" :class="{ on: store.connected }">●</span> <span id="conn">{{ connText }}</span></span>
  <span id="project">{{ project }}</span>
  <label>mode
    <select v-model="mode" :disabled="!store.currentChat" @change="onModeChange">
      <option v-for="m in modes" :key="m" :value="m">{{ m }}</option>
    </select>
  </label>
  <label>model
    <select v-model="connectionId" :disabled="!store.currentChat" @change="onConnectionChange">
      <option v-for="c in connPairs" :key="c.id" :value="c.id">{{ connEmoji(c.id) }}{{ c.label }}</option>
    </select>
    <span id="connHealthDot" class="conn-status" :class="healthClass" :title="healthTitle"></span>
  </label>
  <button class="filter" @click="openSettings">⚙</button>
  <button class="filter" @click="uiBus.emit('debug.open')">debug</button>
  <span id="tokens">{{ tokensText }}</span>
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { uiBus } from "../state/uiBus";
import type { ConnHealth } from "../protocol/types";

const connText = ref("connecting…");
const project = ref("");
const modes = ref<string[]>([]);
const mode = ref("");
const connPairs = ref<{ id: string; label: string }[]>([]);
const connectionId = ref("");
const connHealth = ref<Record<string, ConnHealth>>({});
const tokensText = ref("");

function connEmoji(id: string): string {
  const h = connHealth.value[id];
  return !h || h.ok == null ? "" : h.ok ? "🟢 " : "🔴 ";
}

const healthClass = computed(() => {
  const h = connHealth.value[connectionId.value];
  if (!h || h.ok == null) return "";
  return h.ok ? "ok" : "err";
});
const healthTitle = computed(() => {
  const h = connHealth.value[connectionId.value];
  if (!h || h.ok == null) return "";
  return h.ok ? "Reachable" : (h.error || "Unreachable");
});

function onModeChange() {
  if (store.currentChat) client.send("chat.settings", { chatId: store.currentChat, mode: mode.value }, { projectId: store.currentProjectId ?? undefined });
}
function onConnectionChange() {
  if (store.currentChat) client.send("chat.settings", { chatId: store.currentChat, connectionId: connectionId.value }, { projectId: store.currentProjectId ?? undefined });
}
function openSettings() {
  window.open("/?surface=settings", "spla-settings", "width=640,height=720,resizable=yes");
}

const offConn = client.on("conn", p => { connText.value = p.text || (p.on ? "connected" : "disconnected"); });
const offWelcome = client.on("welcome", p => {
  project.value = p.projectName || p.workspacePath || "";
  modes.value = p.modes || [];
  mode.value = p.defaultMode || "";
  connPairs.value = (p.connections || []).map(x => ({ id: x.id, label: x.name || x.id }));
});
const offChatOpened = client.on("chat.opened", p => {
  if (p.mode) mode.value = p.mode;
  connectionId.value = p.connectionId || "";
});
const offTokens = client.on("token.usage", p => {
  if (p.promptTokens != null || p.completionTokens != null)
    tokensText.value = "tokens in:" + (p.promptTokens ?? "?") + " out:" + (p.completionTokens ?? "?");
});
const offResult = client.on("connections.result", p => {
  connPairs.value = (p.connections || []).map(x => ({ id: x.id, label: x.name || x.model || x.id }));
});
const offHealth = client.on("connections.health", p => {
  connHealth.value = {};
  for (const s of p.statuses || []) connHealth.value[s.id] = { ok: s.ok, error: s.error };
});
onUnmounted(() => { offConn(); offWelcome(); offChatOpened(); offTokens(); offResult(); offHealth(); });
</script>
