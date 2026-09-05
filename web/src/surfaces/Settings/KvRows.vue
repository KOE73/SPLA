<!--
  A repeatable key/value list — MCP server env vars and HTTP headers. The value half is always a
  CredentialField: these are secret:/env: references, never plaintext, the same rule every other
  credential-bearing field in Settings follows (see ConnectionCard.vue's API key field).

  Controlled once at mount from `rows`, then owns its own local list and emits the whole record back
  on every edit — the parent is expected to hand this a fresh element each time it re-renders the row
  from scratch (see McpPanel's merge logic), not to push live updates into an open row.
-->
<template>
  <!-- A ListPanel drawn tight: the rows are single lines, not cards, so the panel keeps the shared
       add-below-the-last-row placement while .kv-rows pulls the spacing back in. -->
  <ListPanel class="kv-rows" :empty="!local.length" :empty-text="t('none')" :add-label="addLabel" @add="addRow">
    <div v-for="(row, i) in local" :key="row._key" class="kv-row">
      <input v-model="row.key" :placeholder="t('KEY')" class="mono kv-key" @change="emitRows">
      <CredentialField
        :model-value="row.value"
        :none-label="t('(none)')"
        create-field="value"
        :create-scope="scope"
        @update:model-value="v => { row.value = v; emitRows(); }"
      />
      <DeleteButton @click="removeRow(i)" />
    </div>
  </ListPanel>
</template>

<script setup lang="ts">
import { t } from "../../i18n";
import { ref } from "vue";
import type { SecretScopeId } from "../../protocol/types";
import CredentialField from "../../secrets/CredentialField.vue";
import ListPanel from "../../components/list/ListPanel.vue";
import DeleteButton from "../../components/buttons/DeleteButton.vue";

const props = withDefaults(defineProps<{
  rows?: Record<string, string>;
  scope?: SecretScopeId | "";
  /** Bare noun for the add button — what one row of THIS list is ("Variable", "Header"). */
  addLabel?: string;
}>(), { scope: "", addLabel: "Row" });

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
/* Overrides the list gap only — the panel is still a ListPanel, so the add button and the empty
   state come from the kit. */
.kv-rows { gap: 4px; }
.kv-row { display: flex; align-items: center; gap: 6px; }
.kv-key { width: 10em; height: 26px; padding: 2px 7px; color: var(--text); background: var(--bg);
  border: 1px solid var(--border); border-radius: 5px; font-size: var(--fs-sm); }
</style>
