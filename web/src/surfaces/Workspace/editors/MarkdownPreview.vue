<template>
  <div class="md-preview" v-html="html" />
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = defineProps<{ text: string }>();

const html = computed(() => {
  if (!props.text) return "";
  try {
    return window.marked?.parse(props.text, { gfm: true, breaks: true }) ?? escapeHtml(props.text);
  } catch {
    return escapeHtml(props.text);
  }
});

function escapeHtml(s: string) {
  return s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}
</script>

<style scoped>
.md-preview {
  flex: 1;
  min-height: 0;
  overflow: auto;
  padding: var(--pad);
  font-size: var(--fs);
  line-height: 1.6;
}
.md-preview :deep(h1),
.md-preview :deep(h2),
.md-preview :deep(h3) { color: var(--text); border-bottom: 1px solid var(--border); padding-bottom: 4px; }
.md-preview :deep(code) { background: var(--panel); border: 1px solid var(--border);
  border-radius: 3px; padding: 1px 4px; font-family: var(--mono); font-size: .9em; }
.md-preview :deep(pre) { background: var(--panel); border: 1px solid var(--border);
  border-radius: var(--radius-sm); padding: var(--pad); overflow: auto; }
.md-preview :deep(pre code) { border: none; background: none; padding: 0; }
.md-preview :deep(a) { color: var(--accent); }
.md-preview :deep(blockquote) { border-left: 3px solid var(--border); margin: 0;
  padding-left: var(--pad); color: var(--muted); }
.md-preview :deep(table) { border-collapse: collapse; width: 100%; }
.md-preview :deep(th),
.md-preview :deep(td) { border: 1px solid var(--border); padding: 4px 8px; }
.md-preview :deep(hr) { border: none; border-top: 1px solid var(--border); }
</style>
