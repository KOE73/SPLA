<template>
  <div class="msg-actions">
    <span v-if="when" class="when" :title="when.abs">{{ when.abs }} · {{ when.rel }}</span>
    <button class="act" :title="copied ? 'Copied' : 'Copy'" @click="onCopy">{{ copied ? "✓" : "⧉" }}</button>
    <button v-if="msgId" class="act" title="Rewind to here — discard everything after" @click="$emit('rewind')">↩</button>
    <button v-if="msgId" class="act" title="Fork chat from here" @click="$emit('fork')">⑂</button>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";

const props = defineProps<{
  msgId?: string;
  /** ISO string (server) or epoch ms (local echo). */
  createdAt?: string | number;
}>();
const emit = defineEmits<{ (e: "rewind"): void; (e: "fork"): void; (e: "copy"): void }>();

const copied = ref(false);
function onCopy() {
  emit("copy");
  copied.value = true;
  setTimeout(() => { copied.value = false; }, 1200);
}

// "now" ticks are not needed: the tooltip is recomputed each hover because the parent re-renders
// rarely; a coarse relative label is fine for this purpose.
const when = computed(() => {
  if (props.createdAt == null) return null;
  const d = new Date(props.createdAt);
  if (isNaN(d.getTime())) return null;
  const abs = d.toLocaleString(undefined, {
    year: "numeric", month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit",
  });
  const s = Math.max(0, (Date.now() - d.getTime()) / 1000);
  const rel =
    s < 60 ? "just now" :
    s < 3600 ? `${Math.floor(s / 60)}m ago` :
    s < 86400 ? `${Math.floor(s / 3600)}h ago` :
    `${Math.floor(s / 86400)}d ago`;
  return { abs, rel };
});
</script>
