<template>
  <div :class="kind === 'notice' ? 'notice' : 'tool'">{{ text }}</div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ text: string; kind?: "tool" | "notice" }>(), { kind: "tool" });
</script>
