<!-- Reload-what-this-panel-shows. One glyph, one meaning: a panel header's "read it again" button,
     never a raw ↻ in panel markup. Pass `title` to say what gets re-read. -->
<template>
  <IconGlyphButton :title="title" :disabled="disabled" @click="$emit('click', $event)">↻</IconGlyphButton>
</template>

<script setup lang="ts">
import IconGlyphButton from "./IconGlyphButton.vue";
withDefaults(defineProps<{ title?: string; disabled?: boolean }>(), { title: "Refresh", disabled: false });
defineEmits<{ click: [MouseEvent] }>();
</script>
