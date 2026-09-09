<!--
  Side panel of active sessions (PLAN_20260902 wave 7; closes PLAN_20260820-2 stage 6's "Боковая
  панель активных подагентов"). A flat view over the same tree ChatList renders — role, parent,
  status, model, tokens — for exactly the case a nested indent answers badly: "which of my several
  spawned sessions needs me right now", across however many parents they are scattered under.

  Reads store.chats like ChatList does; nothing here is chat-scoped state of its own (root AGENTS.md's
  "Web UI: Chat-Scoped State" rule — this panel is project-scoped, not per-chat, so a plain computed
  over the global store is exactly right, not a violation of it).

  The correspondence graph section below (PLAN_20260902 wave 7б) lives here for the same reason: a
  project-wide "who talks to whom" view belongs beside the other project-scoped panel, not inside any
  one chat. `edges` is a local ref rather than something in state/store.ts because it is fetched once
  per panel mount and never keyed by a chat — see correspondenceGraph.ts for the pure imbalance math
  this view is built on.
-->
<template>
  <div class="sessions-panel">
    <div v-if="!sessions.length" class="sessions-empty">{{ t('No spawned sessions.') }}</div>
    <div v-for="s in sessions" :key="s.id" class="session-row" :class="{ active: s.id === store.currentChat }" @click="open(s.id)">
      <div class="session-head">
        <span class="session-role">{{ s.as || "agent" }}</span>
        <span v-if="s.state && s.state !== 'idle'" class="session-state" :class="`state-${s.state}`" :title="s.state">●</span>
        <span class="session-title">{{ s.title || s.id }}</span>
        <span class="x" :title="t('Open in a separate window')" @click.stop="openChatWindow(s.id, s.title || s.as || s.id)">⧉</span>
      </div>
      <div class="session-meta">
        <span class="session-parent" :title="'parent: ' + parentTitle(s)">↰ {{ parentTitle(s) }}</span>
        <span v-if="s.modelId" class="session-model" :title="'model: ' + s.modelId">{{ s.modelId }}</span>
        <span v-if="hasTokens(s)" class="session-tokens" :title="t('prompt / completion tokens')">
          {{ s.promptTokens ?? 0 }}↑ {{ s.completionTokens ?? 0 }}↓
        </span>
      </div>
    </div>

    <!--
      Correspondence graph (PLAN_20260902 wave 7б; ADR_20260827-2 §2.5's last row). Project-scoped,
      not per-chat, fetched once on mount — same reasoning as the rest of this panel (see the
      top-of-file note on the "Web UI: Chat-Scoped State" rule).
    -->
    <div class="graph-section">
      <div class="graph-title">{{ t('Who talks to whom') }}</div>
      <div v-if="!edges.length" class="sessions-empty">{{ t('No correspondences yet.') }}</div>
      <template v-else>
        <div v-if="imbalanced.length" class="graph-imbalance">
          <div v-for="r in imbalanced" :key="r.kind + r.role" class="imbalance-row" :title="r.detail">
            ⚠ {{ r.role }} — {{ r.label }}
          </div>
        </div>
        <CorrespondenceGraph :edges="edges" />
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { t } from "../i18n";
import { computed, onMounted, onUnmounted, ref } from "vue";
import { store } from "../state/store";
import { client } from "../protocol/SplaClient";
import type { ChatSummary, CorrespondenceEdgeDto } from "../protocol/types";
import { collectSpawned, titleOf } from "../state/chatTree";
import { openChat } from "../state/chatSessions";
import { neverReplies, onlyTalks } from "../state/correspondenceGraph";
import { openChatWindow, openPanel } from "../dock/dockController";
import CorrespondenceGraph from "./CorrespondenceGraph.vue";

const sessions = computed(() => collectSpawned(store.chats)
  .slice()
  .sort((a, b) => (b.updatedAt ?? "").localeCompare(a.updatedAt ?? "")));

function parentTitle(s: ChatSummary): string {
  return s.parent ? titleOf(store.chats, s.parent) : "—";
}

function hasTokens(s: ChatSummary): boolean {
  return s.promptTokens != null || s.completionTokens != null;
}

function open(chatId: string) {
  openChat(chatId);
  openPanel("chat");
}

// Project-wide, not per-chat state (see the file-top note) — a plain local ref is correct here,
// not a violation of the chat-scoped-state rule: nothing below is keyed by store.currentChat.
const edges = ref<CorrespondenceEdgeDto[]>([]);

const imbalanced = computed(() => {
  const rows: { kind: string; role: string; label: string; detail: string }[] = [];
  for (const r of onlyTalks(edges.value))
    rows.push({ kind: "talks", role: r.role, label: "only ever talks", detail: `sent ${r.sent} replies, received 0` });
  for (const r of neverReplies(edges.value))
    rows.push({ kind: "silent", role: r.role, label: "nobody hears back from", detail: `received ${r.received} replies, sent 0` });
  return rows;
});

const offGraph = client.on("correspondence.graph.result", p => { edges.value = p.edges || []; });
onUnmounted(offGraph);
onMounted(() => client.send("correspondence.graph.get"));
</script>

<style scoped>
.sessions-panel { display: flex; flex-direction: column; gap: 6px; padding: 8px; overflow: auto; height: 100%; }
.sessions-empty { color: var(--muted); font-size: var(--fs-sm); font-style: italic; padding: 8px; }

.session-row {
  border: 1px solid var(--border); border-radius: var(--radius-sm); padding: 6px 8px;
  cursor: pointer; display: flex; flex-direction: column; gap: 3px;
}
.session-row:hover { background: color-mix(in srgb, var(--text) 6%, transparent); }
.session-row.active { background: var(--accent-soft); border-color: var(--accent); }

.session-head { display: flex; align-items: center; gap: 6px; }
.session-role {
  flex-shrink: 0; font-size: var(--fs-xs); font-weight: 600;
  padding: 0 5px; border-radius: var(--radius-sm); background: var(--accent-soft); color: var(--accent);
}
.session-title { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: var(--fs-sm); }
.session-row .x { color: var(--muted); opacity: 0; font-size: var(--fs-xs); flex-shrink: 0; }
.session-row:hover .x { opacity: .8; }
.session-row .x:hover { color: var(--accent); }

.session-state { font-size: 6px; flex-shrink: 0; }
.session-state.state-working { color: var(--accent); animation: sp-pulse 1.6s ease-in-out infinite; }
@keyframes sp-pulse { 0%, 100% { opacity: .35; } 50% { opacity: 1; } }
.session-state.state-waiting { color: var(--danger); animation: sp-wait-pulse .8s ease-in-out infinite; }
@keyframes sp-wait-pulse { 0%, 100% { opacity: 1; } 50% { opacity: .5; } }
.session-state.state-stalled { color: var(--muted); opacity: .6; }

.session-meta { display: flex; gap: 10px; font-size: var(--fs-xs); color: var(--muted); flex-wrap: wrap; }
.session-parent, .session-model, .session-tokens { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.graph-section { display: flex; flex-direction: column; gap: 4px; margin-top: 8px; padding-top: 8px; border-top: 1px solid var(--border); }
.graph-title { font-size: var(--fs-xs); font-weight: 600; color: var(--muted); text-transform: uppercase; letter-spacing: .04em; }
.graph-imbalance { display: flex; flex-direction: column; gap: 2px; margin-bottom: 4px; }
.imbalance-row { font-size: var(--fs-xs); color: var(--danger); }
/* The edge list is drawn now — see CorrespondenceGraph.vue, which brings its own styles. */
</style>
