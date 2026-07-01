<template>
  <div ref="el" class="text-editor" />
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from "vue";
import { EditorView, basicSetup } from "codemirror";
import { EditorState } from "@codemirror/state";
import { markdown } from "@codemirror/lang-markdown";
import { json } from "@codemirror/lang-json";
import { yaml } from "@codemirror/lang-yaml";
import { oneDark } from "@codemirror/theme-one-dark";

const props = defineProps<{
  text: string;
  lang: string;
  readOnly: boolean;
  docId: string;
  dark: boolean;
}>();

const emit = defineEmits<{
  (e: "save", v: { docId: string; text: string }): void;
  (e: "update", text: string): void;
}>();

const el = ref<HTMLElement>();
let view: EditorView | undefined;
// Prevents triggering a save during programmatic text replacement (file switch).
let _loading = false;

function langExtension() {
  switch (props.lang) {
    case "md": return markdown();
    case "json":
    case "jsonl": return json();
    case "yaml": return yaml();
    default: return [];
  }
}

function buildExtensions() {
  return [
    basicSetup,
    langExtension(),
    ...(props.dark ? [oneDark] : []),
    EditorState.readOnly.of(props.readOnly),
    EditorView.updateListener.of(u => {
      if (u.docChanged && !_loading) {
        emit("update", u.state.doc.toString());
      }
    }),
  ];
}

function createView(doc: string) {
  if (view) view.destroy();
  view = new EditorView({
    parent: el.value!,
    state: EditorState.create({ doc, extensions: buildExtensions() }),
  });
  el.value!.addEventListener("focusout", onBlur);
}

function onBlur() {
  if (_loading || props.readOnly || !view) return;
  emit("save", { docId: props.docId, text: view.state.doc.toString() });
}

onMounted(() => createView(props.text));
onUnmounted(() => { view?.destroy(); view = undefined; });

// New file selected: replace doc content without triggering save.
watch([() => props.text, () => props.docId], ([newText]) => {
  if (!view) return;
  _loading = true;
  view.dispatch({
    changes: { from: 0, to: view.state.doc.length, insert: newText },
    selection: { anchor: 0 },
  });
  _loading = false;
});

// Mode or language changed: rebuild editor preserving current content.
watch([() => props.readOnly, () => props.lang, () => props.dark], () => {
  if (!view) return;
  const current = view.state.doc.toString();
  createView(current);
});
</script>

<style scoped>
.text-editor {
  flex: 1;
  min-height: 0;
  overflow: auto;
}
.text-editor :deep(.cm-editor) {
  height: 100%;
  font-family: var(--mono);
  font-size: var(--fs-sm);
}
.text-editor :deep(.cm-scroller) {
  overflow: auto;
}
</style>
