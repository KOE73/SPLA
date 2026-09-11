<template>
  <!-- A compaction summary (ADR_20260911-3 §2.2) reads as a plate, not a human bubble: role is
       "user" on the wire, but nobody typed this and it deserves a collapsed-by-default look, not the
       ordinary actions (rewind/fork on it would be confusing — it stands for a whole hidden prefix,
       not a single turn a person could "take back"). -->
  <div v-if="compactSummary" class="msg compact-plate">
    <button class="plate-toggle" @click="expanded = !expanded">
      {{ expanded ? "▾" : "▸" }} {{ t("Compacted context") }}
    </button>
    <div v-if="expanded" class="body plain plate-body">{{ summaryText }}</div>
  </div>
  <div v-else class="msg user" :class="{ peer: !!peerFrom, compacted: !!compacted }">
    <!-- An incoming reply across a correspondence (ADR_20260827-2 §2.5) is, on the wire, an ordinary
         user-role message — but it must never READ as one: the model that sent it is not "you", and
         a person scrolling back must be able to tell at a glance. The label is the whole mechanism;
         everything else about the bubble (actions, rewind, fork) stays exactly what it is for any
         other message, because this really is one, just not theirs. -->
    <div v-if="peerFrom" class="peer-label">← from {{ peerFrom }}</div>
    <MsgActions
      :msg-id="msgId" :created-at="createdAt"
      @copy="copy" @rewind="$emit('rewind', msgId!, text)" @fork="$emit('fork', msgId!)"
    />
    <!-- The name under each picture is not decoration: it is what the model was shown in front of
         the image, so a reader scrolling back sees the same handle the prompt and the answer use. -->
    <div v-if="images?.length" class="attached">
      <figure v-for="(img, i) in images" :key="i" :title="img.label">
        <img :src="img.url">
        <figcaption v-if="img.label">{{ img.label }}</figcaption>
      </figure>
    </div>
    <div class="body plain">{{ text }}</div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { t } from "../i18n";
import MsgActions from "./MsgActions.vue";
import type { ImageRef } from "../protocol/types";

const props = defineProps<{
  text: string; images?: ImageRef[]; msgId?: string; createdAt?: string | number; peerFrom?: string;
  compacted?: boolean; compactSummary?: boolean;
}>();
defineEmits<{ (e: "rewind", msgId: string, text: string): void; (e: "fork", msgId: string): void }>();

const expanded = ref(false);
// The stored text carries the "--- Compacted context (summary) ---" marker line the server prefixes
// (ADR §2.2) — the plate's own label already says what this is, so strip it rather than show it twice.
const summaryText = computed(() => props.text.replace(/^--- Compacted context \(summary\) ---\n?/, ""));

function copy() {
  navigator.clipboard?.writeText(props.text).catch(() => {});
}
</script>
