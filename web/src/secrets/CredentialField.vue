<!--
  The one control anything that needs a credential embeds: pick an existing entry from the store,
  or create one right here — no separate trip to the Secrets panel, and no plugin ever writing its
  own credential form. Its value is a REFERENCE (`secret:<scope>:<key>`); the credential itself
  never passes through the consumer, which is the whole point of handing this component out from
  the host instead of letting each panel re-implement it.

  Entries are grouped by scope because the scope is part of the identity: two entries may
  legitimately share a name, and only the reference tells them apart.
-->
<template>
  <div class="cred-field">
    <div class="row">
      <select :value="modelValue" @change="pick(($event.target as HTMLSelectElement).value)">
        <option v-if="allowNone" value="">{{ noneLabel }}</option>
        <optgroup v-for="s in SCOPES" :key="s.id" :label="s.label">
          <option v-for="e in entriesOf(s.id)" :key="e.reference" :value="e.reference">
            {{ e.key }}{{ e.fields.length ? ` · ${e.fields.join(", ")}` : "" }}
          </option>
        </optgroup>
      </select>

      <button class="btn ghost" type="button" :class="{ on: mode === 'create' }"
              :title="mode === 'create' ? 'Cancel' : 'Create a new entry in the secret store'"
              @click="toggle('create')">{{ mode === "create" ? "cancel" : "＋ new" }}</button>
      <button v-if="selected?.canManage" class="btn ghost" type="button" :class="{ on: mode === 'edit' }"
              title="Edit this entry's fields" @click="toggle('edit')">edit</button>

      <span v-if="dangling" class="warn" :title="modelValue">
        ⚠ {{ modelValue }} — no such entry (deleted, or in a scope you cannot see)
      </span>
      <span v-else-if="selected && !selected.canManage" class="hint">read-only — usable, not editable</span>
      <span v-else-if="error" class="warn">{{ error }}</span>
    </div>

    <SecretEntryEditor v-if="mode === 'create'" mode="create" :scope="createScope"
                       @created="onCreated" @cancel="mode = ''" />
    <SecretEntryEditor v-else-if="mode === 'edit' && selected" mode="edit" :scope="selected.scope"
                       :entry-key="selected.key" :fields="selected.fields" />
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import type { SecretScopeId } from "../protocol/types";
import SecretEntryEditor from "./SecretEntryEditor.vue";
import { SCOPES, ensureLoaded, entriesOf, findByReference, secrets } from "./store";

const props = withDefaults(defineProps<{
  /** `secret:<scope>:<key>` — empty means none chosen. */
  modelValue?: string;
  allowNone?: boolean;
  noneLabel?: string;
  /** Scope pre-selected when creating; "" makes the user choose. */
  createScope?: SecretScopeId | "";
}>(), { modelValue: "", allowNone: true, noneLabel: "(none)", createScope: "" });

const emit = defineEmits<{ (e: "update:modelValue", reference: string): void }>();

const mode = ref<"" | "create" | "edit">("");
const error = ref("");

ensureLoaded().catch(e => { error.value = e instanceof Error ? e.message : String(e); });

const selected = computed(() => (props.modelValue ? findByReference(props.modelValue) : undefined));
/** A reference pointing at nothing is worth saying out loud — silently showing an empty select is
 * how a broken binding survives until the connection fails at use time. */
const dangling = computed(() => !!props.modelValue && secrets.loaded && !selected.value);

function pick(reference: string) {
  emit("update:modelValue", reference);
  if (mode.value === "edit") mode.value = "";
}

function toggle(m: "create" | "edit") { mode.value = mode.value === m ? "" : m; }

function onCreated(reference: string) {
  emit("update:modelValue", reference);
  mode.value = "";
}
</script>

<style scoped>
.cred-field { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.row { display: flex; align-items: center; gap: 6px; flex-wrap: wrap; }
select { height: 26px; padding: 2px 7px; color: var(--text); background: var(--bg);
  border: 1px solid var(--border); border-radius: 5px; font-family: var(--mono); font-size: var(--fs-sm); }
.hint { font-size: var(--fs-xs); color: var(--muted); }
.warn { font-size: var(--fs-xs); color: var(--danger, #f85149); }
.on { color: var(--text); }
</style>
