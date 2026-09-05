<template>
  <div class="s-panel" data-tab="connections">
    <div class="s-head">
      <b>{{ t('Connections') }}</b>
      <RefreshButton :title="t('Re-check all endpoints')" @click="recheck" />
      <span class="hint">{{ t(hint) }}</span>
    </div>

    <!--
      One section per scope, in merge order — a connection is defined by the file it lives in, and a
      flat list cannot say which of these keys are yours and which arrived with the repository.
      Each section owning its own add button is also what answers "into which scope" without an extra
      question: you add in the section you meant.
    -->
    <section v-for="s in SCOPES" :key="s.scope" class="conn-scope" :data-scope="s.scope">
      <div class="conn-scope-head">
        <b>{{ t(s.title) }}</b>
        <span class="conn-scope-where">{{ t(s.where) }}</span>
      </div>

      <ListPanel
        :empty="!grouped[s.scope].length"
        :empty-text="t(s.empty)"
        :add-label="t(s.add)"
        @add="addConnection(s.scope)"
      >
        <ConnectionCard
          v-for="conn in grouped[s.scope]"
          :key="conn.clientId || conn.id"
          :conn="conn"
          :health="health[conn.id]"
          @remove="remove(conn)"
        />
      </ListPanel>
    </section>
  </div>
</template>

<script setup lang="ts">
import { t } from "../../i18n";
import { computed, onUnmounted, reactive, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { ConnectionDto, ConnHealth } from "../../protocol/types";
import ConnectionCard from "./ConnectionCard.vue";
import ListPanel from "../../components/list/ListPanel.vue";
import RefreshButton from "../../components/buttons/RefreshButton.vue";
import { uuid } from "../../util/uuid";

const KNOWN_DEFAULT_EP = "http://127.0.0.1:1234/v1";

/** The three layers, least authoritative first — the same order they merge in, so what shadows what
 *  reads top-to-bottom. Wording says where the file is, because that IS what a scope means. */
const SCOPES = [
  {
    scope: "shared",
    title: "Shared",
    where: "connections.shared.yaml — administered, shared between people",
    empty: "No shared connections.",
    add: "Shared connection"
  },
  {
    scope: "user",
    title: "Mine",
    where: "~/.spla/connections.yaml — yours, never committed, in every project you open",
    empty: "None yet. Put a connection here and every project sees it.",
    add: "My connection"
  },
  {
    scope: "project",
    title: "This project",
    where: "the project's .spla — travels with the repository",
    empty: "No connections declared by this project.",
    add: "Project connection"
  }
] as const;

type Scope = (typeof SCOPES)[number]["scope"];

const conns = ref<ConnectionDto[]>([]);
const health = reactive<Record<string, ConnHealth>>({});
const hint = ref("");

/** An entry that never said where it lives is a project one — the layer everything was in before
 *  scopes existed, and the same fallback the server applies. */
const scopeOf = (c: ConnectionDto): Scope =>
  SCOPES.some(s => s.scope === c.scope) ? (c.scope as Scope) : "project";

const grouped = computed(() => {
  const out = { shared: [], user: [], project: [] } as Record<Scope, ConnectionDto[]>;
  for (const c of conns.value) out[scopeOf(c)].push(c);
  return out;
});

function addConnection(scope: Scope) {
  conns.value.push({
    id: "", clientId: uuid(), name: "", provider: "lmstudio", scope,
    endpoint: KNOWN_DEFAULT_EP, apiKey: "", models: []
  });
}

function remove(conn: ConnectionDto) {
  const i = conns.value.indexOf(conn);
  if (i >= 0) conns.value.splice(i, 1);
}

function applyResult(connections: ConnectionDto[]) {
  conns.value = connections.map(c => ({
    ...c,
    clientId: c.id || uuid(),
    models: (c.models || []).map(m => ({ ...m, clientId: m.id || uuid() }))
  }));
}

const offResult = client.on("connections.result", p => {
  applyResult(p.connections || []);
  // A refused save (duplicate model id) echoes the list still in effect — say so instead of
  // letting the editor silently snap back to the old values.
  // "No project" no longer means "nothing can be saved": it means the project section cannot be,
  // while the two layers above it are files of their own and save either way.
  hint.value = p.error ? p.error
    : p.canPersist === false ? t("no .spla project — anything under “This project” is session-only")
    : "";
});
const offHealth = client.on("connections.health", p => {
  for (const s of p.statuses || []) health[s.id] = { ok: s.ok, error: s.error };
});
onUnmounted(() => { offResult(); offHealth(); });

function recheck() { client.send("connections.get", undefined); }

/** Send + wait for the broadcast result that confirms the save actually landed (or timeout). */
function save(): Promise<void> {
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { off(); reject(new Error("save timed out")); }, 8000);
    const off = client.on("connections.result", () => { clearTimeout(timer); off(); resolve(); });
    if (!client.send("connections.save", { connections: conns.value })) {
      clearTimeout(timer); off(); reject(new Error("socket closed"));
    }
  });
}

defineExpose({ save });
</script>
