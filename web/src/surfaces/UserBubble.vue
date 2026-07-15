<template>
  <div class="msg user">
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

const props = defineProps<{ text: string; images?: string[]; msgId?: string; createdAt?: string | number }>();
defineEmits<{ (e: "rewind", msgId: string, text: string): void; (e: "fork", msgId: string): void }>();

function copy() {
  navigator.clipboard?.writeText(props.text).catch(() => {});
}
</script>
