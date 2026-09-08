<!-- Caret toggle for a collapsible row/section: ▸ closed, ▾ open. v-model:open is the whole contract —
     the caret never owns the open state itself, so a row can drive it from a click anywhere in its
     header (see CollapsibleRow) as well as from this button directly. -->
<template>
  <IconGlyphButton variant="plain" :on="open" :title="open ? 'Collapse' : 'Expand'"
                   @click="$emit('update:open', !open)">{{ open ? "▾" : "▸" }}</IconGlyphButton>
</template>

<script setup lang="ts">
import IconGlyphButton from "./IconGlyphButton.vue";
defineProps<{ open: boolean }>();
defineEmits<{ "update:open": [boolean] }>();
</script>
