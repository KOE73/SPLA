<!--
  One assistant message. Driven by props now, not by imperative calls from the log: the text lives in
  the chat's session, so a bubble can be unmounted (chat switched away) and remounted with everything
  it had. It decides only WHEN to re-render markdown — debounced, because a streaming answer changes
  many times a second and parsing on every token is what made long answers crawl.
-->
<template>
  <div class="msg assistant">
    <MsgActions
      :msg-id="msgId" :created-at="createdAt"
      @copy="copy" @rewind="$emit('rewind', msgId!)" @fork="$emit('fork', msgId!)"
    />
    <div class="role">assistant</div>
    <details class="reasoning" :hidden="!hasReasoning">
      <summary>reasoning</summary>
      <div class="rbody">{{ reasoning }}</div>
    </details>
    <div v-if="attempts && attempts.length" class="attempts">
      <details v-for="a in attempts" :key="a.index" class="attempt-note">
        <summary>попытка {{ a.index }} отброшена{{ a.note ? ": " + a.note : "" }}</summary>
        <div v-if="a.reasoning" class="rbody">рассуждение: {{ a.reasoning }}</div>
        <div v-if="a.content" class="rbody">ответ: {{ a.content }}</div>
      </details>
    </div>
    <div ref="bodyEl" class="body"></div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { renderMarkdown } from "../composables/useMarkdown";
import MsgActions from "./MsgActions.vue";

const props = withDefaults(defineProps<{
  msgIndex: number;
  text?: string;
  reasoning?: string;
  msgId?: string;
  createdAt?: string | number;
  attempts?: { index: number; note?: string; content?: string; reasoning?: string }[];
}>(), { text: "", reasoning: "" });

defineEmits<{ (e: "rewind", msgId: string): void; (e: "fork", msgId: string): void }>();

const bodyEl = ref<HTMLElement>();
const hasReasoning = computed(() => !!(props.reasoning || "").trim().length);

const RENDER_DEBOUNCE_MS = 70;
let timer = 0;
let rendered = "";

async function render() {
  if (!bodyEl.value || props.text === rendered) return;
  rendered = props.text;
  await renderMarkdown(bodyEl.value, props.text);
}

watch(() => props.text, () => {
  clearTimeout(timer);
  timer = window.setTimeout(render, RENDER_DEBOUNCE_MS);
});

onMounted(render);
onUnmounted(() => clearTimeout(timer));

function copy() {
  // Rich copy (markdown as text/plain + rendered HTML) when the browser allows it; plain otherwise.
  const html = bodyEl.value?.innerHTML;
  if (html && typeof ClipboardItem !== "undefined" && navigator.clipboard?.write) {
    navigator.clipboard.write([new ClipboardItem({
      "text/plain": new Blob([props.text], { type: "text/plain" }),
      "text/html": new Blob([html], { type: "text/html" }),
    })]).catch(() => navigator.clipboard?.writeText(props.text).catch(() => {}));
  } else {
    navigator.clipboard?.writeText(props.text).catch(() => {});
  }
}
</script>
