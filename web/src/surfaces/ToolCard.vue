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

    <!-- What the single line above cannot say: nested and parallel work under this call. Only while
         running — a finished call's tree is its result, and the card already carries that. -->
    <div v-if="call.status === 'running' && hasBranch" class="tc-branch">
      <ProgressBranch :nodes="nodes" :parent-id="call.rootNodeId!" />
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

      <!-- One section per run. A plain agent_spawn has exactly one; agent_spawn_batch has as many as
           it was given tasks, and each is a separate conversation with its own outcome. -->
      <div v-for="(id, i) in call.runIds ?? []" :key="id" class="tc-section">
        <button v-if="!runs[id]" class="tc-section-title tc-link" :disabled="loading[id]"
                @click="loadRun(id)">
          {{ loading[id] ? "загрузка…" : runLabel(i) }}
        </button>
        <template v-else>
          <div v-if="!runs[id]!.found" class="tc-waiting">
            переписка недоступна — в журнале хранятся только последние запуски
          </div>
          <template v-else>
            <div class="tc-section-title">
              {{ runLabel(i) }} · {{ runs[id]!.outcome }} · {{ durationText(runs[id]!) }}
            </div>
            <div v-if="runs[id]!.error" class="tc-waiting">{{ runs[id]!.error }}</div>
            <div class="tc-pre tc-subagent-log">
              <div v-for="(m, n) in runs[id]!.messages" :key="n" class="tc-subagent-msg">
                <b>{{ m.role }}:</b> {{ m.content }}
              </div>
            </div>
          </template>
        </template>
      </div>
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
  /** The `progress.node` root this call opened, when the turn reported a tree. What hangs beneath it
   *  is everything the flat `progress` field above cannot express: a script's parallel children, a
   *  spawned sub-agent's whole run. */
  rootNodeId?: string;
  result?: string;
  /** Spawned runs this call produced, when it spawned any. Carried on the call rather than looked up
   *  from the node tree because the tree is cleared at the start of the next turn (see
   *  chatSessions.ts's progress.node handler) while the finished card stays in the log indefinitely.
   *
   *  A list because agent_spawn_batch is one call with several runs under it — each its own
   *  conversation with its own outcome, and presenting one of them as the call's would be the same
   *  confusion the per-run branches in the progress tree exist to prevent. */
  runIds?: string[];
}
</script>

<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from "vue";
import { useChat } from "../state/chatContext";
import { client } from "../protocol/SplaClient";
import type { SubagentResultPayload } from "../protocol/types";
import ProgressBranch from "./ProgressBranch.vue";

const props = defineProps<{ call: ToolCallState }>();
const expanded = ref(false);

// Taken from the surface rather than passed down: the chat is ambient here (see chatContext), and
// threading the node map through ChatLog's generic v-bind would put it on every other item type too.
const chat = useChat();
const nodes = computed(() => chat.session.value?.nodes ?? {});
const hasBranch = computed(() => {
  const id = props.call.rootNodeId;
  return id != null && (nodes.value[id]?.childIds.length ?? 0) > 0;
});

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

// ── Sub-agent transcripts: fetched on demand, cached per run ──────────────────
// Nothing streams here. The run is over and sitting in the server's ring by the time anyone asks, so
// it is one request/response round trip — the same idiom fs.read uses in useFsBrowser.ts.
//
// On demand rather than with the card, because a transcript is the whole conversation a sub-agent
// had: opening every one of them alongside the result would spend a lot to answer a question hardly
// anybody asks. Keyed by run id so a batch's several runs are fetched and kept independently.
const runs = ref<Record<string, SubagentResultPayload>>({});
const loading = ref<Record<string, boolean>>({});

async function loadRun(id: string) {
  if (runs.value[id] || loading.value[id]) return;
  loading.value = { ...loading.value, [id]: true };
  try {
    const result = await client.invoke<SubagentResultPayload>("subagent.get", { runId: id });
    runs.value = { ...runs.value, [id]: result };
  } finally {
    loading.value = { ...loading.value, [id]: false };
  }
}

/** "показать переписку суб-агента" for a lone run; numbered once there is more than one to tell apart. */
function runLabel(index: number) {
  const total = props.call.runIds?.length ?? 0;
  return total > 1 ? `переписка суб-агента ${index + 1} из ${total}` : "показать переписку суб-агента";
}

function durationText(run: SubagentResultPayload) {
  if (!run.startedAt || !run.finishedAt) return "";
  const ms = Date.parse(run.finishedAt) - Date.parse(run.startedAt);
  if (!Number.isFinite(ms) || ms < 0) return "";
  const s = Math.round(ms / 1000);
  return s < 60 ? s + "s" : Math.floor(s / 60) + "m " + (s % 60) + "s";
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

.tc-branch { padding: 0 10px 6px 14px; }

.tc-body { border-top: 1px solid var(--border); padding: 6px 10px 8px; display: flex; flex-direction: column; gap: 8px; }
.tc-section-title { font-size: var(--fs-xs); font-weight: 700; text-transform: uppercase;
  letter-spacing: .05em; color: var(--muted); margin-bottom: 3px; }
.tc-pre { margin: 0; white-space: pre-wrap; overflow-wrap: anywhere; max-height: 40vh; overflow: auto;
  background: var(--code-bg, transparent); border: 1px solid var(--border); border-radius: var(--radius-sm);
  padding: 6px 8px; font-size: var(--fs-xs); }
.tc-waiting { color: var(--muted); font-style: italic; font-size: var(--fs-xs); }
.tc-link { background: none; border: none; padding: 0; cursor: pointer; color: var(--muted); }
.tc-link:hover { color: var(--accent); }
.tc-subagent-log { display: flex; flex-direction: column; gap: 6px; }
.tc-subagent-msg { white-space: pre-wrap; overflow-wrap: anywhere; }
.tc-subagent-msg b { text-transform: uppercase; font-size: var(--fs-xs); color: var(--muted); margin-right: 4px; }
</style>
