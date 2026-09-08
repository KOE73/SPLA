<!--
  A settings list: zero or more item cards, then the "add" affordance — always in that order, always
  in that place. This is the ONE place that decides where the add button sits relative to the items;
  a consumer supplies content (via the default slot) and the empty-state text, nothing about layout.
  (The bug this exists to prevent: an "+ Add" button drawn above the list, so a freshly added item
  appears out of sight below the fold instead of right next to the button that made it.)

  The add button is AddButton, so it cannot drift per panel; pass `add-label` as the thing being
  added ("New entry", "MCP server") — the ＋ is the button's, not the label's. Omit `add-label`
  entirely for a read-only list with nothing to add.
-->
<template>
  <div class="list-panel">
    <div v-if="empty" class="list-empty"><slot name="empty">{{ t(emptyText) }}</slot></div>
    <slot />
    <AddButton v-if="addLabel" :label="addLabel" :disabled="addDisabled" @click="$emit('add')" />
  </div>
</template>

<script setup lang="ts">
import { t } from "../../i18n";
import AddButton from "../buttons/AddButton.vue";

withDefaults(defineProps<{
  empty: boolean;
  emptyText?: string;
  /** What the button adds, e.g. "Secret". Omit for a list nothing can be added to. */
  addLabel?: string;
  addDisabled?: boolean;
}>(), { emptyText: "Nothing here yet.", addLabel: "", addDisabled: false });

defineEmits<{ add: [] }>();
</script>
