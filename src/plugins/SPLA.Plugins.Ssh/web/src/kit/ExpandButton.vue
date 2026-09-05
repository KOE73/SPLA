<!-- Caret toggle for a collapsible card: ▸ closed, ▾ open. v-model:open is the whole contract — the
     caret never owns the open state, so the card header can drive it from a click anywhere. -->
<template>
  <IconGlyphButton variant="plain" :on="open" :title="open ? 'Collapse' : 'Expand'"
                   @click="$emit('update:open', !open)">{{ open ? "▾" : "▸" }}</IconGlyphButton>
</template>

<script setup lang="ts">
import IconGlyphButton from "./IconGlyphButton.vue";
defineProps<{ open: boolean }>();
defineEmits<{ "update:open": [boolean] }>();
</script>
