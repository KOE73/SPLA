<template>
  <span><span id="dot" :class="{ on: store.connected }">●</span> <span id="conn">{{ connText }}</span></span>
  <span id="project">{{ project }}</span>
  <label>mode
    <select v-model="mode" :disabled="!store.currentChat" @change="onModeChange">
      <option v-for="m in modes" :key="m" :value="m">{{ m }}</option>
    </select>
  </label>
  <label>model
    <select v-model="modelId" :disabled="!store.currentChat" @change="onModelChange">
      <optgroup v-for="g in groups" :key="g.connectionId" :label="connEmoji(g.connectionId) + g.connectionName">
        <option v-for="m in g.models" :key="m.id" :value="m.id">{{ modelLabel(m) }}</option>
      </optgroup>
    </select>
    <span id="connHealthDot" class="conn-status" :class="healthClass" :title="healthTitle"></span>
    <button
      ref="infoBtn"
      class="pi-btn"
      :class="alertClass"
      :disabled="!modelId"
      title="Provider and model details"
      @click.stop="toggleInfo"
    >i</button>
  </label>
  <button class="filter" @click="openSettings">⚙</button>
  <button class="filter" @click="uiBus.emit('debug.open')">debug</button>
  <span
    v-if="ctxUsed != null"
    id="ctxBudget"
    :class="{ warn: ctxPercent != null && ctxPercent >= 80, crit: ctxPercent != null && ctxPercent >= 95 }"
    :title="ctxTitle"
  >
    <span v-if="ctxPercent != null" class="ctx-bar"><span class="ctx-bar-fill" :style="{ width: ctxPercent + '%' }"></span></span>
    <span class="ctx-label">{{ ctxLabel }}</span>
  </span>

  <ProviderInfoPopup
    v-if="infoOpen && infoBtn && modelId"
    :model-id="modelId"
    :anchor="infoBtn"
    @close="infoOpen = false"
  />
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { uiBus } from "../state/uiBus";
import type { ConnHealth, ModelPickDto } from "../protocol/types";
import ProviderInfoPopup from "./ProviderInfoPopup.vue";

const connText = ref("connecting…");
const project = ref("");
const modes = ref<string[]>([]);
const mode = ref("");
const picks = ref<ModelPickDto[]>([]);
const modelId = ref("");
const connHealth = ref<Record<string, ConnHealth>>({});
const lastPrompt = ref<number | null>(null);
const lastCompletion = ref<number | null>(null);
const ctxUsed = ref<number | null>(null);
const ctxWindow = ref<number | null>(null);

const ctxPercent = computed(() => {
  if (ctxUsed.value == null || !ctxWindow.value) return null;
  return Math.min(100, Math.round((ctxUsed.value / ctxWindow.value) * 100));
});

/** Compact "18.5k" / "1.0M" form — the pill has no room for raw token counts. */
function formatCompact(n: number): string {
  if (n >= 1_000_000) return trimZero(n / 1_000_000) + "M";
  if (n >= 1_000) return trimZero(n / 1_000) + "k";
  return String(n);
}
function trimZero(v: number): string {
  return v.toFixed(1).replace(/\.0$/, "");
}

/** "18.5k / 1.0M (2%)" when the window is known, else just the raw count — degrades gracefully
 *  for providers/runtimes GetContextLengthAsync can't resolve a window for. */
const ctxLabel = computed(() => {
  if (ctxUsed.value == null) return "";
  const used = formatCompact(ctxUsed.value);
  return ctxWindow.value
    ? `${used} / ${formatCompact(ctxWindow.value)} (${ctxPercent.value}%)`
    : `${used} tokens`;
});

/** This is the LAST LLM call, not a running total for the conversation — the agent loop makes one
 *  call per tool round-trip, and each resends the full history, so the latest call's token counts
 *  ARE the current context occupancy. The exact in/out split lives here, in the tooltip, since the
 *  pill only has room for the compact size. */
const ctxTitle = computed(() => {
  if (ctxUsed.value == null) return "";
  const parts: string[] = [];
  if (lastPrompt.value != null) parts.push(`in: ${lastPrompt.value.toLocaleString()}`);
  if (lastCompletion.value != null) parts.push(`out: ${lastCompletion.value.toLocaleString()}`);
  let title = parts.length ? `last request — ${parts.join(", ")}` : "";
  if (ctxWindow.value) {
    title += (title ? "\n" : "") + `context: ${ctxUsed.value.toLocaleString()} of ${ctxWindow.value.toLocaleString()} tokens`;
    if ((ctxPercent.value ?? 0) >= 95) title += " — almost full: start a new chat or the next request may fail";
    else if ((ctxPercent.value ?? 0) >= 80) title += " — getting full: consider a new chat soon";
  }
  return title;
});

/** Model entries grouped by their owning connection — the picker's two levels. Grouped by connection
 *  NAME, never by provider: two connections to the same provider must stay distinguishable. */
const groups = computed(() => {
  const out: { connectionId: string; connectionName: string; models: ModelPickDto[] }[] = [];
  for (const p of picks.value) {
    let g = out.find(x => x.connectionId === p.connectionId);
    if (!g) out.push(g = { connectionId: p.connectionId, connectionName: p.connectionName || p.connectionId, models: [] });
    g.models.push(p);
  }
  return out;
});

/** Health is a property of the connection (endpoint + key), so the dot rides on the group. */
function connEmoji(connectionId: string): string {
  const h = connHealth.value[connectionId];
  return !h || h.ok == null ? "" : h.ok ? "🟢 " : "🔴 ";
}

/** Name plus the provider's model string, when they differ — the name alone hides which model it is. */
function modelLabel(m: ModelPickDto): string {
  return m.model && m.model !== m.name ? `${m.name} · ${m.model}` : m.name;
}

/** The health of the selected model's connection, not of the model itself. */
const currentConnectionId = computed(() =>
  picks.value.find(p => p.id === modelId.value)?.connectionId ?? "");

const healthClass = computed(() => {
  const h = connHealth.value[currentConnectionId.value];
  if (!h || h.ok == null) return "";
  return h.ok ? "ok" : "err";
});
const healthTitle = computed(() => {
  const h = connHealth.value[currentConnectionId.value];
  if (!h || h.ok == null) return "";
  return h.ok ? "Reachable" : (h.error || "Unreachable");
});

// ── Provider info ("i") ──────────────────────────────────────────────────────
// Passive by design: it never polls. It reads on open, and afterwards reflects whatever the last
// read reported — a badge that quietly hammered the provider on a timer would cost money on some of
// them and lie on the rest.
const infoBtn = ref<HTMLElement>();
const infoOpen = ref(false);
const worstSeverity = ref<"normal" | "warn" | "critical">("normal");

function toggleInfo() { infoOpen.value = !infoOpen.value; }

const alertClass = computed(() =>
  worstSeverity.value === "critical" ? "crit" : worstSeverity.value === "warn" ? "warn" : "");

function onModeChange() {
  if (store.currentChat) client.send("chat.settings", { chatId: store.currentChat, mode: mode.value }, { projectId: store.currentProjectId ?? undefined });
}
function onModelChange() {
  // A different model may sit behind a different key, so the previous badge says nothing about it.
  worstSeverity.value = "normal";
  infoOpen.value = false;
  if (store.currentChat) client.send("chat.settings", { chatId: store.currentChat, modelId: modelId.value }, { projectId: store.currentProjectId ?? undefined });
}
function openSettings() {
  window.open("/?surface=settings", "spla-settings", "width=640,height=720,resizable=yes");
}

const offConn = client.on("conn", p => { connText.value = p.text || (p.on ? "connected" : "disconnected"); });
const offWelcome = client.on("welcome", p => {
  project.value = p.projectName || p.workspacePath || "";
  modes.value = p.modes || [];
  mode.value = p.defaultMode || "";
  picks.value = p.connections || [];
});
const offChatOpened = client.on("chat.opened", p => {
  if (p.mode) mode.value = p.mode;
  modelId.value = p.modelId || "";
  // Another chat has a different history size — a stale percent is misleading until its first turn.
  lastPrompt.value = null;
  lastCompletion.value = null;
  ctxUsed.value = null;
  ctxWindow.value = null;
});
const offTokens = client.on("token.usage", p => {
  lastPrompt.value = p.promptTokens ?? null;
  lastCompletion.value = p.completionTokens ?? null;
  // Context occupancy: this call's prompt (+ completion, since it joins the history for the next
  // call) vs the model's operative window (when the server knows it). Warn well before the provider
  // would reject the request — local runtimes often fail with an opaque 500 instead of a clean
  // "context exceeded" error.
  if (p.promptTokens != null) {
    ctxUsed.value = p.promptTokens + (p.completionTokens ?? 0);
    ctxWindow.value = p.contextLength ?? null;
  }
});
// The editor broadcasts the connection TREE; the picker needs it flattened. Done here rather than
// asking the server for a second shape — the tree already carries everything the two levels need.
const offResult = client.on("connections.result", p => {
  picks.value = (p.connections || []).flatMap(c =>
    (c.models || []).map(m => ({
      id: m.id,
      name: m.name || m.model || m.id,
      model: m.model,
      connectionId: c.id,
      connectionName: c.name || c.id,
      provider: c.provider
    })));
});
// The badge follows whatever the last read reported — including a read triggered by another window.
const offInfo = client.on("provider.info.result", p => {
  if (p.modelId !== modelId.value) return;
  const all = (p.sections || []).flatMap(s => s.facts || []);
  worstSeverity.value = all.some(f => f.severity === "critical") ? "critical"
    : all.some(f => f.severity === "warn") ? "warn"
    : "normal";
});
const offHealth = client.on("connections.health", p => {
  connHealth.value = {};
  for (const s of p.statuses || []) connHealth.value[s.id] = { ok: s.ok, error: s.error };
});
onUnmounted(() => { offConn(); offWelcome(); offChatOpened(); offTokens(); offResult(); offHealth(); offInfo(); });
</script>
