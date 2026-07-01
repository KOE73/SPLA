<template>
  <div class="editor-host">
    <!-- ── Toolbar ───────────────────────────────────────────────────────── -->
    <div class="editor-toolbar">
      <div class="seg-toggle" title="View: read-only | Edit: autosave on focus-out">
        <button :class="{ on: mode === 'view' }" @click="mode = 'view'">View</button>
        <button :class="{ on: mode === 'edit' }" @click="mode = 'edit'">Edit</button>
      </div>
      <!-- Editor-type toggle is only meaningful for jsonl -->
      <div v-if="isJsonl" class="seg-toggle" title="Forms: structured (Phase 2) | Text: raw JSONL">
        <button :class="{ on: editorType === 'text' }" @click="editorType = 'text'">Text</button>
        <button :class="{ on: editorType === 'forms' }" @click="editorType = 'forms'">Forms</button>
      </div>
      <span class="editor-status">{{ statusText }}</span>
    </div>

    <!-- ── Editor area ───────────────────────────────────────────────────── -->
    <div class="editor-area" :class="{ split: isSplit }">
      <!-- Forms editor (jsonl + forms mode) -->
      <FormsEditor
        v-if="showForms"
        :text="text"
        :readOnly="isReadOnly"
        :docId="docId"
        @save="$emit('save', $event)"
      />
      <!-- Text / CodeMirror (all non-forms, or md in edit mode) -->
      <TextEditor
        v-if="showTextEditor"
        :text="text"
        :lang="lang"
        :readOnly="isReadOnly"
        :docId="docId"
        :dark="dark"
        @save="$emit('save', $event)"
        @update="liveText = $event"
      />
      <!-- Markdown preview (always shown for md; split in edit, full in view) -->
      <MarkdownPreview v-if="isMd" :text="liveText" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import TextEditor from "./editors/TextEditor.vue";
import FormsEditor from "./editors/FormsEditor.vue";
import MarkdownPreview from "./editors/MarkdownPreview.vue";

const props = defineProps<{
  text: string;
  contentType: string;
  docId: string;
  dark: boolean;
}>();

const emit = defineEmits<{
  (e: "save", v: { docId: string; text: string }): void;
}>();

const mode = ref<"view" | "edit">("view");
const editorType = ref<"text" | "forms">("text");
// Live text tracks keystrokes in the editor so the md preview stays fresh.
const liveText = ref(props.text);

watch(() => props.text, v => { liveText.value = v; });

const isMd    = computed(() => props.contentType === "md");
const isJsonl = computed(() => props.contentType === "jsonl");
const lang    = computed(() => props.contentType || "txt");

const isReadOnly     = computed(() => mode.value === "view");
const showForms      = computed(() => isJsonl.value && editorType.value === "forms");
const showTextEditor = computed(() => !showForms.value && !(isMd.value && mode.value === "view"));
const isSplit        = computed(() => isMd.value && mode.value === "edit");

const statusText = computed(() => {
  const modeLabel = mode.value === "edit" ? "edit (autosave)" : "view";
  const edLabel = showForms.value ? "forms" : lang.value;
  return `${edLabel} · ${modeLabel}`;
});
</script>

<style scoped>
.editor-host {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
  min-width: 0;
}

.editor-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 5px 10px;
  border-bottom: 1px solid var(--border);
  background: var(--panel);
  flex-shrink: 0;
}

.seg-toggle {
  display: flex;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  overflow: hidden;
}
.seg-toggle button {
  background: transparent;
  color: var(--muted);
  border: none;
  padding: 3px 10px;
  font-size: var(--fs-sm);
  cursor: pointer;
}
.seg-toggle button.on {
  background: var(--accent-soft);
  color: var(--accent);
}
.seg-toggle button:hover:not(.on) {
  background: color-mix(in srgb, var(--text) 6%, transparent);
}

.editor-status {
  margin-left: auto;
  font-size: var(--fs-xs);
  color: var(--muted);
}

.editor-area {
  display: flex;
  flex: 1;
  min-height: 0;
  overflow: hidden;
}
.editor-area.split > :deep(*) {
  flex: 1;
  min-width: 0;
}
.editor-area.split > :deep(*:not(:last-child)) {
  border-right: 1px solid var(--border);
}
</style>
