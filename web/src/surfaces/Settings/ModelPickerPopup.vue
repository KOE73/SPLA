<template>
  <Teleport to="body">
    <div ref="popEl" class="models-popup" :style="popStyle" @keydown="onKeydown">
      <div class="model-filter">
        <input
          ref="filterEl"
          v-model="filter"
          type="text"
          placeholder="gemma*free"
          spellcheck="false"
        />
        <span class="model-count">{{ shown.length }} / {{ models.length }}</span>
        <span class="model-hint" :title="hint">?</span>
      </div>
      <div ref="listEl" class="model-list">
        <div
          v-for="(m, i) in shown"
          :key="m"
          class="model-item"
          :data-active="i === active ? '1' : undefined"
          :style="itemStyle(m)"
          :title="swap && !locked ? 'Hot-swap: unload current + load this model' : undefined"
          @click="onClick(m)"
          @mousemove="active = i"
        >{{ m }}</div>
        <div v-if="!shown.length" class="model-empty">nothing matches the filter</div>
        <div v-if="locked" class="model-item" style="opacity:0.5;cursor:default;font-style:italic;">
          🔒 model is locked — view only
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onBeforeUnmount, ref, watch } from "vue";
import type { CSSProperties } from "vue";

const props = defineProps<{
  models: string[];
  anchor: HTMLElement;
  locked: boolean;
  swap: boolean;
  current?: string;
}>();
const emit = defineEmits<{ pick: [model: string]; swap: [model: string]; close: [] }>();

const hint =
  "Filter: * matches any substring (gemma*3*free). " +
  "A space separates terms — all must match, in any order (gemma free). " +
  "Case-insensitive, matched against the whole id.";

const popEl = ref<HTMLElement>();
const filterEl = ref<HTMLInputElement>();
const listEl = ref<HTMLElement>();
const filter = ref("");
const active = ref(0);

function termToRegex(term: string) {
  // escape regex metachars, then turn * into .*; terms are implicitly *term*
  const body = term.replace(/[.+?^${}()|[\]\\]/g, "\\$&").replace(/\*/g, ".*");
  return new RegExp(body, "i");
}

const matchers = computed(() =>
  filter.value.trim().split(/\s+/).filter(Boolean).map(termToRegex)
);
const shown = computed(() =>
  matchers.value.length ? props.models.filter(m => matchers.value.every(r => r.test(m))) : props.models
);

watch(shown, () => { active.value = 0; });

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

function move(delta: number) {
  if (!shown.value.length) return;
  active.value = (active.value + delta + shown.value.length) % shown.value.length;
  nextTick(() => {
    listEl.value?.querySelector<HTMLElement>('.model-item[data-active="1"]')
      ?.scrollIntoView({ block: "nearest" });
  });
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === "ArrowDown") { e.preventDefault(); move(1); }
  else if (e.key === "ArrowUp") { e.preventDefault(); move(-1); }
  else if (e.key === "Enter") {
    e.preventDefault();
    const m = shown.value[active.value];
    if (m) onClick(m);
  } else if (e.key === "Escape") { e.preventDefault(); emit("close"); }
}

function position(flip = true) {
  const rect = props.anchor.getBoundingClientRect();
  const el = popEl.value;
  const popW = el?.offsetWidth ?? 380;
  const popH = el?.offsetHeight ?? 360;
  // flip above the anchor when there is no room below; while the user drags the
  // resize handle we only clamp, so the popup doesn't jump out from under the cursor
  const noRoomBelow = rect.bottom + 2 + popH > window.innerHeight - 4;
  top.value = flip && noRoomBelow && rect.top - 2 - popH > 4
    ? rect.top - 2 - popH
    : Math.max(4, Math.min(top.value || rect.bottom + 2, window.innerHeight - popH - 4));
  let l = flip ? rect.right - popW : (left.value ?? rect.right - popW);
  if (l < 4) l = Math.min(rect.left, window.innerWidth - popW - 4);
  left.value = Math.max(4, Math.min(l, Math.max(4, window.innerWidth - popW - 4)));
}

function onOutsideClick(e: MouseEvent) {
  if (popEl.value && !popEl.value.contains(e.target as Node) && e.target !== props.anchor) emit("close");
}

let ro: ResizeObserver | undefined;
const onWindowResize = () => position();

onMounted(() => {
  position();
  filterEl.value?.focus();
  ro = new ResizeObserver(() => position(false));
  if (popEl.value) ro.observe(popEl.value);
  window.addEventListener("resize", onWindowResize);
  // defer so the click that opened the popup doesn't immediately close it
  setTimeout(() => document.addEventListener("click", onOutsideClick), 0);
});
onBeforeUnmount(() => {
  document.removeEventListener("click", onOutsideClick);
  window.removeEventListener("resize", onWindowResize);
  ro?.disconnect();
});
</script>
