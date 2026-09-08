<!-- The one and only "get rid of this" affordance: a trash can. Mirrors
     web/src/components/buttons/DeleteButton.vue in the main client. There is no second button for
     the same act — no ✕, no "✕ Remove", no bespoke .del class — because "remove from the list" and
     "delete" read as one thing to the person clicking, and two glyphs only ask them which is which.

     The glyph carries U+FE0E (text presentation), so it renders as a thin outline in the UI font and
     takes the surrounding text's size, weight and colour — not as a heavy colored emoji.

     `title` arrives already translated: this bundle's kit does not speak i18n, the panel does. Say
     what is deleted ("Delete this host"), not just "Delete". -->
<template>
  <IconGlyphButton variant="danger" :title="title" :disabled="disabled" @click="$emit('click', $event)">🗑︎</IconGlyphButton>
</template>

<script setup lang="ts">
import IconGlyphButton from "./IconGlyphButton.vue";
withDefaults(defineProps<{ title?: string; disabled?: boolean }>(), { title: "Delete", disabled: false });
defineEmits<{ click: [MouseEvent] }>();
</script>
