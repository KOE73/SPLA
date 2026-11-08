<template>
  <select :value="modelValue" class="model-select" :disabled="disabled"
          @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value); emit('change')">
    <option v-if="inheritLabel !== undefined" value="">{{ inheritLabel }}</option>
    <optgroup v-for="g in groups" :key="g.id" :label="healthEmoji(g.id) + g.name">
      <option v-for="m in g.models" :key="m.id" :value="m.id">{{ label(m) }}</option>
    </optgroup>
    <option v-if="modelValue && !models.some(m => m.id === modelValue)" :value="modelValue">
      {{ modelValue }} (?)
    </option>
  </select>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { ConnHealth, ModelPickDto } from "../../protocol/types";

const props = defineProps<{
  modelValue?: string;
  models: ModelPickDto[];
  disabled?: boolean;
  inheritLabel?: string;
  health?: Record<string, ConnHealth>;
}>();
const emit = defineEmits<{ 'update:modelValue': [value: string]; change: [] }>();

const groups = computed(() => {
  const result: { id: string; name: string; models: ModelPickDto[] }[] = [];
  for (const model of props.models) {
    let group = result.find(g => g.id === model.connectionId);
    if (!group) result.push(group = {
      id: model.connectionId, name: model.connectionName || model.connectionId, models: []
    });
    group.models.push(model);
  }
  return result;
});

function healthEmoji(connectionId: string): string {
  const health = props.health?.[connectionId];
  return !health || health.ok == null ? "" : health.ok ? "🟢 " : "🔴 ";
}

function label(model: ModelPickDto): string {
  const entry = model.model && model.model !== model.name
    ? `${model.name} · ${model.model}` : model.name;
  return `${model.default ? "★ " : ""}${model.connectionName || model.connectionId} | ${entry}`;
}
</script>
