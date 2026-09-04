<!--
  A settings list: zero or more item cards, then the "add" affordance — always in that order, always
  in that place. This is the ONE place that decides where "+ Add …" sits relative to the items; a
  consumer supplies content (via the default slot) and the empty-state text, nothing about layout.
  (The bug this exists to prevent: an "+ Add" button drawn above the list, so a freshly added item
  appears out of sight below the fold instead of right next to the button that made it.)
-->
<template>
  <div class="list-panel">
    <div v-if="empty" class="list-empty"><slot name="empty">{{ emptyText }}</slot></div>
    <slot />
    <button type="button" class="btn ghost list-add" @click="$emit('add')">{{ addLabel }}</button>
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  empty: boolean;
  emptyText?: string;
  addLabel: string;
}>(), { emptyText: "Nothing here yet." });

defineEmits<{ add: [] }>();
</script>
