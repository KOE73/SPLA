<template>
  <div class="msg user" :class="{ peer: !!peerFrom }">
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
    <div v-if="images?.length" style="display:flex;gap:6px;flex-wrap:wrap;margin-bottom:6px">
      <img v-for="(src, i) in images" :key="i" :src="src" style="max-width:160px;max-height:160px;border-radius:6px">
    </div>
    <div class="body plain">{{ text }}</div>
  </div>
</template>

<script setup lang="ts">
import MsgActions from "./MsgActions.vue";

const props = defineProps<{
  text: string; images?: string[]; msgId?: string; createdAt?: string | number; peerFrom?: string;
}>();
defineEmits<{ (e: "rewind", msgId: string, text: string): void; (e: "fork", msgId: string): void }>();

function copy() {
  navigator.clipboard?.writeText(props.text).catch(() => {});
}
</script>
