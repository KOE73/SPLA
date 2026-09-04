<!-- Trash icon. The one and only delete affordance — every list row that can remove its own item
     uses this, never a raw ✕ or a bespoke .del class. -->
<template>
  <IconGlyphButton variant="danger" :title="title" :disabled="disabled" @click="$emit('click', $event)">🗑</IconGlyphButton>
</template>

<script setup lang="ts">
import IconGlyphButton from "./IconGlyphButton.vue";
withDefaults(defineProps<{ title?: string; disabled?: boolean }>(), { title: "Delete", disabled: false });
defineEmits<{ click: [MouseEvent] }>();
</script>
