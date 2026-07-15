<template>
  <div class="msg assistant">
    <MsgActions
      :msg-id="msgId" :created-at="createdAt"
      @copy="copy" @rewind="$emit('rewind', msgId!)" @fork="$emit('fork', msgId!)"
    />
    <div class="role">assistant</div>
    <details class="reasoning" :hidden="!hasReasoning">
      <summary>reasoning</summary>
      <div class="rbody">{{ reasoningText }}</div>
    </details>
    <div ref="bodyEl" class="body"></div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { renderMarkdown } from "../composables/useMarkdown";
import MsgActions from "./MsgActions.vue";

defineProps<{ msgIndex: number; msgId?: string; createdAt?: string | number }>();
defineEmits<{ (e: "rewind", msgId: string): void; (e: "fork", msgId: string): void }>();

const bodyEl = ref<HTMLElement>();
const reasoningText = ref("");
const hasReasoning = computed(() => !!reasoningText.value.trim().length);
let rawText = "";   // markdown source of the current body — what Copy puts on the clipboard

async function renderInto(text: string) {
  rawText = text;
  if (bodyEl.value) await renderMarkdown(bodyEl.value, text);
}
function setReasoning(text: string) { reasoningText.value = text || ""; }
function appendReasoning(chunk: string) { reasoningText.value += chunk; }

function copy() {
  // Rich copy (markdown as text/plain + rendered HTML) when the browser allows it; plain otherwise.
  const html = bodyEl.value?.innerHTML;
  if (html && typeof ClipboardItem !== "undefined" && navigator.clipboard?.write) {
    navigator.clipboard.write([new ClipboardItem({
      "text/plain": new Blob([rawText], { type: "text/plain" }),
      "text/html": new Blob([html], { type: "text/html" }),
    })]).catch(() => navigator.clipboard?.writeText(rawText).catch(() => {}));
  } else {
    navigator.clipboard?.writeText(rawText).catch(() => {});
  }
}

defineExpose({ renderInto, setReasoning, appendReasoning });
</script>
