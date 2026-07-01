<template>
  <Teleport to="body">
    <div ref="popEl" class="models-popup" :style="popStyle">
      <div
        v-for="m in models"
        :key="m"
        class="model-item"
        :style="itemStyle(m)"
        :title="swap && !locked ? 'Hot-swap: unload current + load this model' : undefined"
        @click="onClick(m)"
      >{{ m }}</div>
      <div v-if="locked" class="model-item" style="opacity:0.5;cursor:default;font-style:italic;">
        🔒 model is locked — view only
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, onMounted, onBeforeUnmount, ref } from "vue";
import type { CSSProperties } from "vue";

const props = defineProps<{
  models: string[];
  anchor: HTMLElement;
  locked: boolean;
  swap: boolean;
  current?: string;
}>();
const emit = defineEmits<{ pick: [model: string]; swap: [model: string]; close: [] }>();

const popEl = ref<HTMLElement>();
const top = ref(0);
const left = ref<number | null>(null);
const popStyle = computed<CSSProperties>(() => ({
  top: top.value + "px",
  left: (left.value ?? 0) + "px",
  visibility: left.value === null ? "hidden" : undefined
}));

function itemStyle(m: string) {
  return m === props.current ? { fontWeight: "600" } : {};
}

function onClick(m: string) {
  if (props.locked) return;
  if (props.swap) emit("swap", m);
  else emit("pick", m);
}

function position() {
  const rect = props.anchor.getBoundingClientRect();
  top.value = rect.bottom + 2;
  const popW = popEl.value?.offsetWidth ?? 240;
  let l = rect.right - popW;
  if (l < 4) l = Math.min(rect.left, window.innerWidth - popW - 4);
  left.value = Math.max(4, l);
}

function onOutsideClick(e: MouseEvent) {
  if (popEl.value && !popEl.value.contains(e.target as Node) && e.target !== props.anchor) emit("close");
}

onMounted(() => {
  position();
  // defer so the click that opened the popup doesn't immediately close it
  setTimeout(() => document.addEventListener("click", onOutsideClick), 0);
});
onBeforeUnmount(() => document.removeEventListener("click", onOutsideClick));
</script>
