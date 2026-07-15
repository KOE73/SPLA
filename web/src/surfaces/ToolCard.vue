<template>
  <div class="tool tool-card" :class="{ done: call.status === 'done', open: expanded }">
    <div class="tc-head" @click="expanded = !expanded">
      <span class="tc-status">
        <span v-if="call.status === 'running'" class="tc-spinner" />
        <template v-else>✓</template>
      </span>
      <span class="tc-arrow">{{ call.status === "running" ? "→" : "←" }}</span>
      <span class="tc-name">{{ call.name }}</span>
      <span class="tc-meta">
        <template v-if="call.status === 'done' && call.result != null">{{ formatSize(call.result.length) }} ·</template>
        {{ elapsedText }}
      </span>
      <span class="tc-chevron">{{ expanded ? "▾" : "▸" }}</span>
    </div>

    <!-- live progress: shown while running, collapsed away when done -->
    <div v-if="call.status === 'running' && call.progress" class="tc-progress">
      <div v-if="call.progress.fraction != null" class="tc-bar">
        <div class="tc-bar-fill" :style="{ width: Math.round(100 * (call.progress.fraction ?? 0)) + '%' }" />
      </div>
      <div v-if="call.progress.message" class="tc-progress-msg">{{ call.progress.message }}</div>
      <div v-if="call.progress.details?.length" class="tc-details">
        <span v-for="d in call.progress.details" :key="d.label" class="tc-detail">
          <b>{{ d.label }}:</b> {{ d.value }}
        </span>
      </div>
    </div>

    <div v-if="expanded" class="tc-body">
      <div class="tc-section">
        <div class="tc-section-title">параметры</div>
        <pre class="tc-pre">{{ prettyArgs }}</pre>
      </div>
      <div v-if="call.result != null" class="tc-section">
        <div class="tc-section-title">результат ({{ formatSize(call.result.length) }})</div>
        <pre class="tc-pre">{{ call.result }}</pre>
      </div>
      <div v-else-if="call.status === 'running'" class="tc-section tc-waiting">выполняется…</div>
    </div>
  </div>
</template>

<script lang="ts">
import type { ToolProgressDetail } from "../protocol/types";

export interface ToolCallState {
  callId: string;
  name: string;
  argumentsText: string;
  status: "running" | "done";
  /** epoch ms when tool.started arrived; absent for historical calls (no timing shown). */
  startedAt?: number;
  finishedAt?: number;
  progress?: { fraction?: number | null; message?: string | null; details?: ToolProgressDetail[] | null };
  result?: string;
}
</script>

<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from "vue";

const props = defineProps<{ call: ToolCallState }>();
const expanded = ref(false);

const prettyArgs = computed(() => {
  const raw = props.call.argumentsText || "";
  if (!raw.trim()) return "(нет)";
  try { return JSON.stringify(JSON.parse(raw), null, 2); } catch { return raw; }
});

// ── Elapsed clock: ticks every second while the call runs ─────────────────────
const now = ref(Date.now());
let timer = 0;
watch(() => props.call.status, s => {
  clearInterval(timer);
  if (s === "running" && props.call.startedAt) timer = window.setInterval(() => { now.value = Date.now(); }, 1000);
}, { immediate: true });
onUnmounted(() => clearInterval(timer));

const elapsedText = computed(() => {
  const { startedAt, finishedAt, status } = props.call;
  if (!startedAt) return "";
  const ms = (status === "done" ? (finishedAt ?? startedAt) : now.value) - startedAt;
  const s = Math.max(0, Math.round(ms / 1000));
  if (s < 60) return s + "s";
  return Math.floor(s / 60) + "m " + (s % 60) + "s";
});

function formatSize(chars: number) {
  return chars >= 10000 ? (chars / 1000).toFixed(1) + "k chars" : chars + " chars";
}
</script>

<style scoped>
.tool-card { padding: 0; min-width: 28ch; max-width: 80ch; }
.tc-head { display: flex; align-items: center; gap: 7px; padding: 5px 10px; cursor: pointer; user-select: none; }
.tc-head:hover { color: var(--text); }
.tc-status { width: 1em; text-align: center; color: var(--accent); }
.tc-name { font-weight: 600; }
.tc-meta { color: var(--muted); font-size: var(--fs-xs); margin-left: auto; white-space: nowrap; }
.tc-chevron { color: var(--muted); }

.tc-spinner { display: inline-block; width: .8em; height: .8em; border: 2px solid var(--border);
  border-top-color: var(--accent); border-radius: 50%; animation: tc-spin 0.8s linear infinite; vertical-align: -1px; }
@keyframes tc-spin { to { transform: rotate(360deg); } }

.tc-progress { padding: 0 10px 6px; }
.tc-bar { height: 4px; border-radius: 2px; background: var(--border); overflow: hidden; margin: 2px 0 4px; }
.tc-bar-fill { height: 100%; background: var(--accent); border-radius: 2px; transition: width .2s; }
.tc-progress-msg { font-size: var(--fs-xs); color: var(--muted); white-space: pre-wrap; }
.tc-details { display: flex; flex-wrap: wrap; gap: 4px 12px; font-size: var(--fs-xs); color: var(--muted); margin-top: 2px; }
.tc-detail b { font-weight: 600; color: var(--text); }

.tc-body { border-top: 1px solid var(--border); padding: 6px 10px 8px; display: flex; flex-direction: column; gap: 8px; }
.tc-section-title { font-size: var(--fs-xs); font-weight: 700; text-transform: uppercase;
  letter-spacing: .05em; color: var(--muted); margin-bottom: 3px; }
.tc-pre { margin: 0; white-space: pre-wrap; overflow-wrap: anywhere; max-height: 40vh; overflow: auto;
  background: var(--code-bg, transparent); border: 1px solid var(--border); border-radius: var(--radius-sm);
  padding: 6px 8px; font-size: var(--fs-xs); }
.tc-waiting { color: var(--muted); font-style: italic; font-size: var(--fs-xs); }
</style>
