<!--
  A yes/no setting that also has a third answer: say nothing and inherit whoever you inherit from.
  Three states, three words — never a checkbox, because an unchecked box and "not stated" look the
  same and mean different things (a role that says nothing about a switch is not a role that turned
  it off). `inheritLabel` should name the inherited VALUE, not the word "default", so the reader can
  see what saying nothing actually gets them.
-->
<template>
  <label class="field"><span>{{ label }}</span>
    <span class="tri">
      <select :value="wire" @change="onChange">
        <option value="">{{ inheritLabel }}</option>
        <option value="on">on</option>
        <option value="off">off</option>
      </select>
      <span v-if="hint" class="hint">{{ hint }}</span>
    </span>
  </label>
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = withDefaults(defineProps<{
  label: string;
  modelValue?: boolean | null;
  /** What "say nothing" resolves to, spelled out — e.g. "inherit (on)". */
  inheritLabel?: string;
  hint?: string;
}>(), { inheritLabel: "inherit", modelValue: null });

const emit = defineEmits<{ "update:modelValue": [boolean | null] }>();

const wire = computed(() =>
  props.modelValue === true ? "on" : props.modelValue === false ? "off" : "");

function onChange(e: Event) {
  const v = (e.target as HTMLSelectElement).value;
  emit("update:modelValue", v === "on" ? true : v === "off" ? false : null);
}
</script>

<style scoped>
.tri { display: flex; align-items: center; gap: 8px; }
</style>
