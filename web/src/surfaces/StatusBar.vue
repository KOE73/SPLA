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
  <span
    v-if="ctxPercent != null"
    id="ctxBudget"
    :class="{ warn: ctxPercent >= 80, crit: ctxPercent >= 95 }"
    :title="ctxTitle"
  >ctx {{ ctxPercent }}%</span>
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
const ctxUsed = ref<number | null>(null);
const ctxWindow = ref<number | null>(null);

const ctxPercent = computed(() => {
  if (ctxUsed.value == null || !ctxWindow.value) return null;
  return Math.min(100, Math.round((ctxUsed.value / ctxWindow.value) * 100));
});
const ctxTitle = computed(() => {
  if (ctxUsed.value == null || !ctxWindow.value) return "";
  const base = `context: ${ctxUsed.value.toLocaleString()} of ${ctxWindow.value.toLocaleString()} tokens`;
  if ((ctxPercent.value ?? 0) >= 95) return base + " — almost full: start a new chat or the next request may fail";
  if ((ctxPercent.value ?? 0) >= 80) return base + " — getting full: consider a new chat soon";
  return base;
});

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
  // Another chat has a different history size — a stale percent is misleading until its first turn.
  ctxUsed.value = null;
  ctxWindow.value = null;
});
const offTokens = client.on("token.usage", p => {
  if (p.promptTokens != null || p.completionTokens != null)
    tokensText.value = "tokens in:" + (p.promptTokens ?? "?") + " out:" + (p.completionTokens ?? "?");
  // Context budget: prompt tokens vs the model's operative window (when the server knows it).
  // Warn well before the provider would reject the request — local runtimes often fail with an
  // opaque 500 instead of a clean "context exceeded" error.
  if (p.contextLength && p.promptTokens != null) {
    ctxUsed.value = p.promptTokens + (p.completionTokens ?? 0);
    ctxWindow.value = p.contextLength;
  }
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
