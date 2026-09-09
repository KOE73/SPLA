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
    <div class="role">{{ t('assistant') }}</div>
    <details class="reasoning" :hidden="!hasReasoning">
      <summary>{{ t('reasoning') }}</summary>
      <div class="rbody">{{ reasoning }}</div>
    </details>
    <div v-if="attempts && attempts.length" class="attempts">
      <!-- Every rate limit in a turn is the same event happening again, so it gets ONE line that is
           rewritten as the attempts accumulate. A row per attempt buried the answer under a stack of
           near-identical ones, each with an expander over nothing. -->
      <div v-if="paced" class="attempt-note flat">{{ paced }}</div>
      <template v-for="a in discarded" :key="a.index">
        <!-- An expander is offered only when there is something behind it. -->
        <details v-if="a.content || a.reasoning" class="attempt-note">
          <summary>{{ discardedLine(a) }}</summary>
          <div v-if="a.reasoning" class="rbody">{{ t('reasoning:') }} {{ a.reasoning }}</div>
          <div v-if="a.content" class="rbody">{{ t('answer:') }} {{ a.content }}</div>
        </details>
        <div v-else class="attempt-note flat">{{ discardedLine(a) }}</div>
      </template>
    </div>
    <div ref="bodyEl" class="body"></div>
  </div>
</template>

<script setup lang="ts">
import { t } from "../i18n";
import { computed, onMounted, onUnmounted, ref, watch } from "vue";
import { renderMarkdown } from "../composables/useMarkdown";
import MsgActions from "./MsgActions.vue";

const props = withDefaults(defineProps<{
  msgIndex: number;
  text?: string;
  reasoning?: string;
  msgId?: string;
  createdAt?: string | number;
  attempts?: { index: number; outcome?: string; note?: string; chars?: number; durationMs?: number;
    waitMs?: number | null; waitStated?: boolean; content?: string; reasoning?: string }[];
}>(), { text: "", reasoning: "" });

type Attempt = NonNullable<typeof props.attempts>[number];

/** Seconds, at one decimal and never "0.0s" — a refusal that took 40ms should read as fast, not free. */
function secs(ms: number): string {
  return ms < 100 ? `${Math.round(ms)} ${t('ms')}` : `${(ms / 1000).toFixed(1)} ${t('s')}`;
}

/** The single rewritten line for every rate limit in this turn, or "" when there was none.
 *  <p>Composed here rather than echoed from the attempt's `note`, which the engine writes in English:
 *  the number matters enough to say in the reader's own language. Whose figure the wait is gets said
 *  out loud — a delay the provider named is a fact to sit out, ours is a guess. */
const paced = computed(() => {
  const limited = (props.attempts || []).filter(a => a.outcome === "RateLimited");
  if (!limited.length) return "";

  const last = limited[limited.length - 1];
  const spent = secs(limited.reduce((total, a) => total + (a.durationMs || 0), 0));
  const stats = t("{n} refused, {spent} spent", { n: limited.length, spent });

  // No wait on the last one means nothing followed it — the attempts or the budget ran out.
  if (!last.waitMs) return `${t("rate limit — gave up")} · ${stats}`;

  const wait = secs(last.waitMs);
  const head = last.waitStated
    ? t("rate limit — the provider asked for {wait}", { wait })
    : t("rate limit — retrying in {wait}", { wait });
  return `${head} · ${stats}`;
});

/** Attempts that produced text and were thrown away for what it became — one row each, because each
 *  one holds a different generation worth opening. */
const discarded = computed(() => (props.attempts || []).filter(a => a.outcome !== "RateLimited"));

function discardedLine(a: Attempt): string {
  const size = a.chars ? t("{chars} chars", { chars: a.chars }) : "";
  const took = a.durationMs ? secs(a.durationMs) : "";
  const stats = [size, took].filter(Boolean).join(", ");
  const head = t("attempt {n} discarded", { n: a.index });
  return [a.note ? `${head}: ${a.note}` : head, stats].filter(Boolean).join(" · ");
}

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
