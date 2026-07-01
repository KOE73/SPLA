<template>
  <div class="forms-root" ref="formEl" @focusout="onContainerBlur">
    <div v-if="loading" class="forms-state">Loading schema…</div>
    <div v-else-if="schemaError" class="forms-state forms-err">{{ schemaError }}</div>
    <div v-else-if="!dataSchemaObj" class="forms-state forms-warn">
      No JSON Forms schema for this file.<br>
      Switch to the <strong>Text</strong> editor.
    </div>
    <JsonForms
      v-else
      :schema="dataSchemaObj"
      :uischema="uiSchemaObj ?? undefined"
      :data="formData"
      :renderers="renderers"
      :readonly="readOnly"
      @change="onChange"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { JsonForms } from "@jsonforms/vue";
import { vanillaRenderers } from "@jsonforms/vue-vanilla";
import type { UISchemaElement, JsonSchema as JFSchema } from "@jsonforms/core";
import "@jsonforms/vue-vanilla/vanilla.css";
import { client } from "../../../protocol/SplaClient";
import type { SchemaResultPayload } from "../../../protocol/types";
import { resolveSchemaName } from "./formsRegistry";
import { entry as tableArrayEntry } from "./TableArrayRenderer.vue";

const props = defineProps<{ text: string; readOnly: boolean; docId: string }>();
const emit = defineEmits<{
  (e: "save", payload: { docId: string; text: string }): void;
}>();

const formEl = ref<HTMLElement>();
const loading = ref(false);
const schemaError = ref<string>();
const dataSchemaObj = ref<JFSchema | null>(null);
const uiSchemaObj = ref<UISchemaElement | null>(null);
const formData = ref<Record<string, unknown>>({});
const currentSchemaName = ref<string | null>(null);  // kept for schema loading, not serialized

const renderers = [tableArrayEntry, ...vanillaRenderers];

// ── JSONL conversion ─────────────────────────────────────────────────────────
// Format: flat array — each line is a JSON object {field, type, desc?, agg?, note?}

function jsonlToObject(jsonl: string): Record<string, unknown> {
  const fields: unknown[] = [];
  for (const line of jsonl.split("\n")) {
    const t = line.trim();
    if (!t) continue;
    try {
      fields.push(JSON.parse(t) as unknown);
    } catch { /* skip invalid lines */ }
  }
  return { fields };
}

function objectToJsonl(obj: Record<string, unknown>): string {
  return ((obj.fields ?? []) as Record<string, unknown>[])
    .map((f) => JSON.stringify(f))
    .join("\n");
}

// ── Schema loading ───────────────────────────────────────────────────────────

/**
 * JsonForms validates with AJV pinned to draft-07. A data schema declaring
 * `$schema: draft/2020-12` makes AJV throw ("no schema with key or ref …"),
 * which crashes the whole <JsonForms> render into a blank pane. We don't rely on
 * any 2020-12-only keywords, so strip the meta-schema declaration defensively for
 * every provider's schema.
 */
function sanitizeSchema(raw: unknown): JFSchema {
  const s = raw as Record<string, unknown>;
  if (s && typeof s === "object") delete s.$schema;
  return s as JFSchema;
}

async function loadSchema(name: string) {
  loading.value = true;
  schemaError.value = undefined;
  dataSchemaObj.value = null;
  uiSchemaObj.value = null;
  try {
    const result = await client.invoke<SchemaResultPayload>("schema.get", { name });
    if (result?.error) {
      schemaError.value = result.error;
    } else {
      dataSchemaObj.value = result?.dataSchema ? sanitizeSchema(JSON.parse(result.dataSchema)) : null;
      uiSchemaObj.value   = result?.uiSchema   ? (JSON.parse(result.uiSchema)   as UISchemaElement) : null;
    }
  } catch (e) {
    schemaError.value = `Failed to load schema: ${e}`;
  } finally {
    loading.value = false;
  }
}

// ── Reactive text / docId handling ──────────────────────────────────────────

watch(
  [() => props.text, () => props.docId],
  async ([newText, newDocId], oldVals) => {
    const prevDocId = oldVals?.[1];
    const docChanged = newDocId !== prevDocId;
    const name = resolveSchemaName(newText);

    if (docChanged || name !== currentSchemaName.value) {
      currentSchemaName.value = name;
      if (name) {
        await loadSchema(name);
      } else {
        dataSchemaObj.value = null;
        uiSchemaObj.value = null;
        schemaError.value = undefined;
      }
    }

    formData.value = jsonlToObject(newText);
  },
  { immediate: true }
);

// ── Save / change handling ────────────────────────────────────────────────────

let saveTimer: ReturnType<typeof setTimeout> | null = null;

function onChange({ data }: { data: unknown }) {
  if (props.readOnly) return;
  formData.value = data as Record<string, unknown>;
  if (saveTimer) clearTimeout(saveTimer);
  saveTimer = setTimeout(saveNow, 800);
}

function saveNow() {
  if (props.readOnly) return;
  emit("save", { docId: props.docId, text: objectToJsonl(formData.value) });
}

function onContainerBlur(e: FocusEvent) {
  const container = formEl.value;
  if (!container) return;
  if (!e.relatedTarget || !container.contains(e.relatedTarget as Node)) {
    if (saveTimer) { clearTimeout(saveTimer); saveTimer = null; }
    saveNow();
  }
}
</script>

<style scoped>
/* ── Root ───────────────────────────────────────────────────────────────────── */
.forms-root {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
  overflow: auto;
  padding: var(--pad);
}

/* ── State messages ─────────────────────────────────────────────────────────── */
.forms-state {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  color: var(--muted);
  font-size: var(--fs-sm);
}
.forms-err  { color: #e55; }
.forms-warn { flex-direction: column; line-height: 1.7; }
.forms-warn strong { color: var(--text); }

/* ── Override @jsonforms/vue-vanilla with SPLA tokens ───────────────────────── */
:deep(.vertical-layout),
:deep(.horizontal-layout),
:deep(.group-layout) {
  display: flex;
  flex-direction: column;
  gap: 12px;
  width: 100%;
}
:deep(.horizontal-layout) { flex-direction: row; flex-wrap: wrap; }

:deep(.group-layout) {
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: var(--pad);
}

:deep(.label-element) {
  font-size: var(--fs-sm);
  font-weight: 600;
  color: var(--text);
  margin-bottom: 4px;
}

:deep(.control) { display: flex; flex-direction: column; gap: 4px; }
:deep(.control > label) { font-size: var(--fs-sm); color: var(--muted); }
:deep(.control input),
:deep(.control textarea),
:deep(.control select) {
  background: var(--panel);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  color: var(--text);
  font-family: var(--font);
  font-size: var(--fs);
  padding: 4px 8px;
  outline: none;
  width: 100%;
  box-sizing: border-box;
}
:deep(.control input:focus),
:deep(.control textarea:focus),
:deep(.control select:focus) { border-color: var(--accent); }

/* ── Array controls ─────────────────────────────────────────────────────────── */
:deep(.array-list-toolbar) { display: flex; justify-content: flex-end; margin-bottom: 6px; }
:deep(.array-list-item) {
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: var(--pad);
  margin-bottom: 6px;
  background: var(--panel);
}
:deep(.array-list-item-toolbar) { display: flex; justify-content: flex-end; gap: 4px; margin-top: 6px; }
:deep(.button) {
  background: var(--panel);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  color: var(--text);
  font-size: var(--fs-sm);
  padding: 2px 8px;
  cursor: pointer;
}
:deep(.button:hover) { background: var(--accent); color: #fff; border-color: var(--accent); }

/* ── Validation hints ────────────────────────────────────────────────────────── */
:deep(.input-description),
:deep(.error) { font-size: var(--fs-xs); color: #e55; }
</style>
