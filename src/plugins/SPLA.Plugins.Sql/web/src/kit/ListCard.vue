<!--
  One card inside a ListPanel: a header (always visible) plus a body that is only in the DOM while
  open. Mirrors web/src/components/list/ListCard.vue. The header slot gets `open` so it can draw its
  own caret; clicking the header toggles unless `no-toggle-on-click` is set. No chrome here — the
  border, background and radius are kit.css's three variables.
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
