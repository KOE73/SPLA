<!--
  A number that may be left unsaid. Empty input = inherit, and the placeholder carries the inherited
  value so an empty box reads as an answer rather than as a missing one. Same reasoning as
  TriStateField: "not stated" is a state of its own and has to be typeable.
-->
<template>
  <label class="field"><span>{{ label }}</span>
    <span class="num">
      <input type="number" :value="modelValue ?? ''" :min="min" :max="max" :step="step"
             :placeholder="placeholder ?? 'inherit'" @input="onInput" />
      <span v-if="unit" class="hint">{{ unit }}</span>
      <span v-if="hint" class="hint">{{ hint }}</span>
    </span>
  </label>
</template>

<script setup lang="ts">
defineProps<{
  label: string;
  modelValue?: number | null;
  /** What the empty box means, spelled out — defaults to "inherit". */
  placeholder?: string;
  unit?: string;
  hint?: string;
  min?: number;
  max?: number;
  step?: number;
}>();

const emit = defineEmits<{ "update:modelValue": [number | null] }>();

function onInput(e: Event) {
  const raw = (e.target as HTMLInputElement).value.trim();
  emit("update:modelValue", raw === "" ? null : Number(raw));
}
</script>

<style scoped>
.num { display: flex; align-items: center; gap: 8px; }
.num input { width: 7em; }
</style>
