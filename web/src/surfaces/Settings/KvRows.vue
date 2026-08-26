<!--
  A repeatable key/value list — MCP server env vars and HTTP headers. The value half is always a
  CredentialField: these are secret:/env: references, never plaintext, the same rule every other
  credential-bearing field in Settings follows (see ConnectionCard.vue's API key field).

  Controlled once at mount from `rows`, then owns its own local list and emits the whole record back
  on every edit — the parent is expected to hand this a fresh element each time it re-renders the row
  from scratch (see McpPanel's merge logic), not to push live updates into an open row.
-->
<template>
  <div class="kv-rows">
    <div v-for="(row, i) in local" :key="row._key" class="kv-row">
      <input v-model="row.key" placeholder="KEY" class="mono kv-key" @change="emitRows">
      <CredentialField
        :model-value="row.value"
        none-label="(none)"
        create-field="value"
        :create-scope="scope"
        @update:model-value="v => { row.value = v; emitRows(); }"
      />
      <button class="btn ghost" type="button" title="Remove" @click="removeRow(i)">✕</button>
    </div>
    <button class="btn ghost" type="button" @click="addRow">＋ add</button>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { SecretScopeId } from "../../protocol/types";
import CredentialField from "../../secrets/CredentialField.vue";

const props = withDefaults(defineProps<{
  rows?: Record<string, string>;
  scope?: SecretScopeId | "";
}>(), { scope: "" });

const emit = defineEmits<{ (e: "update:rows", rows: Record<string, string>): void }>();

let seq = 0;
function toLocal(rows?: Record<string, string>) {
  return Object.entries(rows || {}).map(([key, value]) => ({ _key: `k${seq++}`, key, value }));
}

const local = ref(toLocal(props.rows));

function emitRows() {
  const out: Record<string, string> = {};
  for (const row of local.value) if (row.key.trim()) out[row.key.trim()] = row.value;
  emit("update:rows", out);
}

function addRow() {
  local.value = [...local.value, { _key: `k${seq++}`, key: "", value: "" }];
}

function removeRow(i: number) {
  local.value = local.value.filter((_, idx) => idx !== i);
  emitRows();
}
</script>

<style scoped>
.kv-rows { display: flex; flex-direction: column; gap: 4px; }
.kv-row { display: flex; align-items: center; gap: 6px; }
.kv-key { width: 10em; height: 26px; padding: 2px 7px; color: var(--text); background: var(--bg);
  border: 1px solid var(--border); border-radius: 5px; font-size: var(--fs-sm); }
</style>
