<!--
  A settings list: zero or more item cards, then the "add" affordance — always in that order, always
  in that place. Mirrors web/src/components/list/ListPanel.vue. This is the ONE place that decides
  where the add button sits relative to the items. (The bug it exists to prevent: an "+ Add" drawn
  above the list, so a freshly added item appears out of sight below the fold instead of right next
  to the button that made it.)

  `add-label` is the thing being added ("Connection", "Host") — the ＋ is the button's, not the
  label's. Omit it for a list nothing can be added to. The kit's stylesheet ships with this
  component, so a panel imports the list and gets the whole look with it.
-->
<template>
  <div class="list-panel">
    <div v-if="empty" class="list-empty"><slot name="empty">{{ emptyText }}</slot></div>
    <slot />
    <AddButton v-if="addLabel" :label="addLabel" :disabled="addDisabled" @click="$emit('add')" />
  </div>
</template>

<script setup lang="ts">
import AddButton from "./AddButton.vue";

withDefaults(defineProps<{
  empty: boolean;
  emptyText?: string;
  /** What the button adds, e.g. "Connection". Omit for a list nothing can be added to. */
  addLabel?: string;
  addDisabled?: boolean;
}>(), { emptyText: "Nothing here yet.", addLabel: "", addDisabled: false });

defineEmits<{ add: [] }>();
</script>

<!-- Unscoped on purpose: these are the host's own class names and must not be rewritten into
     data-v selectors, or slotted content and the host's own restyles would stop matching. -->
<style src="./kit.css"></style>
