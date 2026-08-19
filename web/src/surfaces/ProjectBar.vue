<!--
  Project/server status bar — the sidebar footer, pinned under the chat list.

  Everything here is a property of the CONNECTION or the PROJECT, never of a chat. That is the whole
  point of the split: this bar is not remounted when the user switches chats and has no chat-scoped
  subscriptions, so it physically cannot flicker with a background chat's traffic — which is exactly
  what it used to do while these indicators lived in the chat's status line.

  It is as narrow as the sidebar (170px at the minimum, see AppShell), so the layout is fixed and
  every element degrades by shrinking its own label rather than by moving: detail goes to the title.
-->
<template>
  <div class="project-bar">
    <!-- Non-main build running: shout it before anything else, so it can't be mistaken for main. -->
    <div v-if="store.branch" class="pb-branch" :title="`Running a build published from '${store.branch}', not main.`">
      {{ store.branch }}
    </div>

    <!-- Row 1 — where I am. -->
    <div class="pb-row">
      <button class="pb-project" :title="projectTitle" @click="store.projectPickerOpen = true">
        <span class="pb-ico">📂</span>
        <span class="pb-text">{{ store.currentProjectName || "SPLA" }}</span>
      </button>
      <span v-if="store.userName" class="pb-user" :title="store.userName">
        <span class="pb-ico">👤</span>
        <span class="pb-text">{{ store.userName }}</span>
      </span>
    </div>

    <!-- Row 2 — how it feels. -->
    <div class="pb-row pb-health">
      <span class="pb-dot" :class="{ on: store.connected }" :title="connText">●</span>

      <!-- Silent while every connection answers: a row of green ticks is noise, one red one is news. -->
      <button
        v-if="healthCounts.bad || healthCounts.unknown"
        class="pb-chip"
        :class="{ bad: healthCounts.bad > 0 }"
        :title="healthTitle"
        @click="openSettings"
      >{{ healthLabel }}</button>

      <span v-if="projectTokens != null" class="pb-chip pb-tokens" :title="tokensTitle">
        {{ formatCompact(projectTokens) }}
      </span>

      <span class="pb-spacer"></span>

      <button class="pb-icon-btn" title="Settings" @click="openSettings">
        <Icon name="settings" :size="19" :weight="2" />
      </button>
      <button class="pb-icon-btn subtle" title="Debug" @click="uiBus.emit('debug.open')">
        <Icon name="debug" :size="16" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { uiBus } from "../state/uiBus";
import { formatCompact } from "../util/format";
import Icon from "../dock/Icon.vue";
import type { TokenUsageScope } from "../protocol/types";

const connText = ref("connecting…");
const health = ref<{ id: string; ok: boolean | null; error?: string }[]>([]);
const usage = ref<TokenUsageScope | null>(null);

const projectTitle = computed(() => {
  const name = store.currentProjectName || "SPLA";
  return store.workspacePath && store.workspacePath !== name
    ? `${name}\n${store.workspacePath}\n\nClick to switch project`
    : `${name}\n\nClick to switch project`;
});

const healthCounts = computed(() => {
  let ok = 0, bad = 0, unknown = 0;
  for (const h of health.value) {
    if (h.ok === true) ok++;
    else if (h.ok === false) bad++;
    else unknown++;
  }
  return { ok, bad, unknown };
});

/** "1✗" once something is down, "?2" while nobody has answered yet — never both, never verbose. */
const healthLabel = computed(() => {
  const c = healthCounts.value;
  return c.bad ? `${c.bad}✗` : `?${c.unknown}`;
});

const healthTitle = computed(() => {
  const lines = health.value
    .filter(h => h.ok !== true)
    .map(h => `· ${h.id} — ${h.ok === false ? (h.error || "unreachable") : "not checked yet"}`);
  return ["Connections:", `${healthCounts.value.ok} reachable of ${health.value.length}`, ...lines,
    "", "Click to open settings."].join("\n");
});

const projectTokens = computed(() => usage.value?.totalTokens ?? null);

const tokensTitle = computed(() => {
  const u = usage.value;
  if (!u) return "";
  return [
    "Tokens spent in this project (all chats, since it was created):",
    `in: ${u.promptTokens.toLocaleString()}`,
    `out: ${u.completionTokens.toLocaleString()}`,
    `turns: ${u.turns.toLocaleString()}`
  ].join("\n");
});

function openSettings() {
  window.open("/?surface=settings", "spla-settings", "width=640,height=720,resizable=yes");
}

/** The tally is project-scoped, so it is re-read whenever the focused project changes. */
function fetchUsage() {
  client.send("usage.get");
}

const offs = [
  client.on("conn", p => { connText.value = p.text || (p.on ? "connected" : "disconnected"); }),
  // The tally is asked for on welcome, not on "conn": the connected signal is emitted while the
  // welcome frame is still being dispatched, so the project this window belongs to is not settled
  // yet — a tear-off window carrying ?project= would have asked about the default project instead.
  client.on("welcome", fetchUsage),
  // The server sends cached health right after the handshake, so the dot is honest from the start
  // without this bar pinging anything itself.
  client.on("connections.health", p => { health.value = p.statuses || []; }),
  client.on("usage.result", p => { usage.value = p.project; })
];

onMounted(() => { if (store.connected) fetchUsage(); });
onUnmounted(() => offs.forEach(o => o()));
</script>

<style scoped>
.project-bar {
  flex-shrink: 0;
  border-top: 1px solid var(--border);
  padding: 5px var(--pad);
  background: var(--panel);
  font-size: var(--fs-sm);
  color: var(--muted);
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.pb-row {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}

.pb-ico { flex: 0 0 auto; font-size: 11px; opacity: 0.85; }

.pb-text {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.pb-project {
  display: flex;
  align-items: center;
  gap: 4px;
  min-width: 0;
  flex: 1 1 auto;
  font: inherit;
  color: var(--text);
  background: transparent;
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  padding: 1px 4px;
  cursor: pointer;
}
.pb-project:hover { background: color-mix(in srgb, var(--text) 6%, transparent); border-color: var(--border); }

.pb-user {
  display: flex;
  align-items: center;
  gap: 4px;
  min-width: 0;
  flex: 0 1 auto;
  max-width: 45%;
}

.pb-dot { flex: 0 0 auto; color: var(--danger, #d05); font-size: 10px; }
.pb-dot.on { color: var(--ok, #3c9); }

.pb-chip {
  flex: 0 0 auto;
  font: inherit;
  font-size: var(--fs-xs);
  color: var(--muted);
  background: transparent;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: 0 4px;
  cursor: default;
}
.pb-chip.bad { color: var(--danger, #d05); border-color: color-mix(in srgb, var(--danger, #d05) 45%, transparent); cursor: pointer; }

.pb-tokens { border-color: transparent; }

.pb-spacer { flex: 1 1 auto; }

.pb-icon-btn {
  flex: 0 0 auto;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  padding: 1px;
  cursor: pointer;
  color: var(--muted);
  border-radius: var(--radius-sm);
}
.pb-icon-btn:hover { color: var(--accent); }
.pb-icon-btn.subtle { opacity: 0.6; }
.pb-icon-btn.subtle:hover { opacity: 1; }

.pb-branch {
  flex-shrink: 0;
  background: #c00;
  color: #ff0;
  font-weight: 700;
  font-size: var(--fs-xs);
  text-align: center;
  padding: 2px 4px;
  border-radius: var(--radius-sm);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
