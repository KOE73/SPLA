<template>
  <div class="msg assistant">
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

const bodyEl = ref<HTMLElement>();
const reasoningText = ref("");
const hasReasoning = computed(() => !!reasoningText.value.trim().length);

async function renderInto(text: string) {
  if (bodyEl.value) await renderMarkdown(bodyEl.value, text);
}
function setReasoning(text: string) { reasoningText.value = text || ""; }
function appendReasoning(chunk: string) { reasoningText.value += chunk; }

defineExpose({ renderInto, setReasoning, appendReasoning });
</script>
