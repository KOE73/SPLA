<template>
  <div class="wire-surface">
  <header>
    <b>Wire</b>
    <input v-model="filter" class="wire-filter" placeholder="filter type/payload…" spellcheck="false">
    <button class="filter" :class="{ on: paused }" @click="paused = !paused">{{ paused ? "resume" : "pause" }}</button>
    <button class="filter" @click="clear">clear</button>
    <span class="hint">{{ total ? total + " frames" : "" }}</span>
  </header>
  <div ref="logEl" class="wire-log">
    <div
      v-for="(f, i) in frames"
      :key="i"
      class="wire-row"
      :class="[f.dir === 'in' ? 'in' : 'out', { hide: !matches(f) }]"
    >
      <span class="wt">{{ time(f.ts) }}</span>
      <span class="wd">{{ f.dir === "in" ? "←" : "→" }}</span>
      <span class="wtype">{{ f.type }}</span>
      <span v-if="meta(f)" class="wmeta">{{ meta(f) }}</span>
      <span v-if="payloadText(f)" class="wpay">{{ payloadText(f) }}</span>
    </div>
  </div>
  </div>
</template>

<script setup lang="ts">
import { nextTick, onUnmounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import type { WireFrame } from "../protocol/types";

const MAX = 500; // ring cap so a long session doesn't grow unbounded

const frames = ref<WireFrame[]>([]);
const filter = ref("");
const paused = ref(false);
const total = ref(0);
const logEl = ref<HTMLElement>();

function time(ts: number) { return new Date(ts).toLocaleTimeString(); }
function meta(f: WireFrame) { return [f.chatId && "chat=" + f.chatId, f.requestId && "req=" + f.requestId].filter(Boolean).join(" "); }
function payloadStr(f: WireFrame) { return f.payload == null ? "" : JSON.stringify(f.payload); }
function payloadText(f: WireFrame) {
  const s = payloadStr(f);
  return s.length > 300 ? s.slice(0, 300) + "…" : s;
}
function searchText(f: WireFrame) { return (f.type + " " + payloadStr(f)).toLowerCase(); }
function matches(f: WireFrame) {
  const q = filter.value.toLowerCase();
  return !q || searchText(f).includes(q);
}
function clear() { frames.value = []; total.value = 0; }

const off = client.onWire(frame => {
  if (paused.value) return;
  const atBottom = logEl.value ? logEl.value.scrollHeight - logEl.value.scrollTop - logEl.value.clientHeight < 40 : true;
  frames.value.push(frame);
  if (frames.value.length > MAX) frames.value.shift();
  total.value++;
  if (atBottom) nextTick(() => { if (logEl.value) logEl.value.scrollTop = logEl.value.scrollHeight; });
});
onUnmounted(off);
</script>
