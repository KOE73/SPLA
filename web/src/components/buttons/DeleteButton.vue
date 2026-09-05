<!-- The one and only "get rid of this" affordance in the app: a trash can. There is no second
     button for the same act — no ✕, no "✕ Remove", no bespoke .del class — because "remove from
     the list" and "delete" read as one thing to the person clicking, and two glyphs for it only
     ask them which is which.

     The glyph carries U+FE0E (text presentation), so it renders as a thin outline in the UI font
     and takes the surrounding text's size, weight and color — not as a heavy colored emoji.

     Leave `title` alone by default. The row this button sits in already says what it deletes, so a
     per-site "Delete this connection" buys a sighted reader nothing and costs a dictionary entry
     that can drift out of sync with the row it names. Pass a title only when the button cannot be
     read from its place: two trashes of different meaning in one card (a connection and one of its
     models), or a consequence worth warning about before the click. -->
<template>
  <IconGlyphButton variant="danger" :title="title" :disabled="disabled" @click="$emit('click', $event)">🗑︎</IconGlyphButton>
</template>

<script setup lang="ts">
import IconGlyphButton from "./IconGlyphButton.vue";
withDefaults(defineProps<{ title?: string; disabled?: boolean }>(), { title: "Delete", disabled: false });
defineEmits<{ click: [MouseEvent] }>();
</script>
