<!--
  One bordered card inside a ListPanel (or standalone) — the visual unit every "host row", "entry
  row" and "server row" in Settings collapsed into its own bespoke CSS before this existed. A card is
  always a header (always visible) plus an optional body (only in the DOM while open) — the header
  slot gets `open` so it can draw its own caret via ExpandButton, and clicking the header toggles by
  default unless `no-toggle-on-click` is set (rows with their own clickable controls in the header,
  e.g. a checkbox, want that off).
-->
<template>
  <div class="list-card" :class="{ open }">
    <div class="list-card-head" @click="onHeadClick"><slot name="head" :open="open" /></div>
    <div v-if="open && $slots.body" class="list-card-body"><slot name="body" /></div>
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  open?: boolean;
  noToggleOnClick?: boolean;
}>(), { open: false, noToggleOnClick: false });

const emit = defineEmits<{ "update:open": [boolean] }>();

function onHeadClick() {
  if (!props.noToggleOnClick) emit("update:open", !props.open);
}
</script>
