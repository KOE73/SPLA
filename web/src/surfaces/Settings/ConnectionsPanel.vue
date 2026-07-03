<template>
  <div class="s-panel" data-tab="connections">
    <div class="s-head">
      <b>Connections</b>
      <button class="btn ghost conn-recheck" title="Re-check all endpoints" @click="recheck">↻</button>
      <span class="hint">{{ hint }}</span>
    </div>
    <div class="conn-list">
      <ConnectionCard
        v-for="(conn, i) in conns"
        :key="conn.clientId || conn.id"
        :conn="conn"
        :health="health[conn.id]"
        @remove="conns.splice(i, 1)"
      />
      <button class="btn ghost" @click="addConnection">+ Add connection</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, reactive, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { ConnectionDto, ConnHealth } from "../../protocol/types";
import ConnectionCard from "./ConnectionCard.vue";
import { uuid } from "../../util/uuid";

const KNOWN_DEFAULT_EP = "http://127.0.0.1:1234/v1";

const conns = ref<ConnectionDto[]>([]);
const health = reactive<Record<string, ConnHealth>>({});
const hint = ref("");

function addConnection() {
  conns.value.push({ id: "", clientId: uuid(), name: "", provider: "lmstudio", endpoint: KNOWN_DEFAULT_EP, model: "", apiKey: "" });
}

function applyResult(connections: ConnectionDto[]) {
  conns.value = connections.map(c => ({ ...c, clientId: c.id || uuid() }));
}

const offResult = client.on("connections.result", p => {
  applyResult(p.connections || []);
  hint.value = p.canPersist === false ? "no .spla project — session-only" : "";
});
const offHealth = client.on("connections.health", p => {
  for (const s of p.statuses || []) health[s.id] = { ok: s.ok, error: s.error };
});
onUnmounted(() => { offResult(); offHealth(); });

function recheck() { client.send("connections.get"); }

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
