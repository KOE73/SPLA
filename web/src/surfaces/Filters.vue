<template>
  <span>show:</span>
  <button v-for="f in FILTERS" :key="f.kind" class="filter" :class="{ on: !hidden.has(f.kind) }" @click="toggle(f.kind)">
    {{ f.label }}
  </button>
</template>

<script setup lang="ts">
import { reactive } from "vue";

const FILTERS = [
  { kind: "user", label: "me" },
  { kind: "assistant", label: "assistant" },
  { kind: "tool", label: "tools" },
  { kind: "reasoning", label: "reasoning" }
] as const;

function loadHidden(): Set<string> {
  try { return new Set(JSON.parse(localStorage.getItem("spla.hidden") || "[]")); }
  catch { return new Set(); }
}

const hidden = reactive(loadHidden());
applyBodyClasses();

function applyBodyClasses() {
  for (const f of FILTERS) document.body.classList.toggle("hide-" + f.kind, hidden.has(f.kind));
}

function toggle(kind: string) {
  if (hidden.has(kind)) hidden.delete(kind); else hidden.add(kind);
  localStorage.setItem("spla.hidden", JSON.stringify([...hidden]));
  applyBodyClasses();
}
</script>
