<template>
  <div class="ask">
    <div class="q">{{ question }}</div>
    <div class="opts">
      <button v-for="o in options" :key="o.label" class="btn" :title="o.description" @click="$emit('choose', requestId, o.label)">{{ o.label }}</button>
      <button class="btn ghost" @click="$emit('choose', requestId, null)">Skip</button>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ requestId: string; question: string; options?: { label: string; description?: string }[] }>();
defineEmits<{ choose: [requestId: string, choice: string | null] }>();
</script>
