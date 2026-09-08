<!--
  The one "add another one of these" button. Every list in Settings ends with one, and ListPanel
  places it for you — reach for this directly only outside a list (a section that adds without
  showing its items). The ＋ glyph belongs to the button, not to the label: pass "New entry", not
  "＋ New entry", so every add button in the app reads the same way.

  Styling lives in app.css under ".list-add" alongside the list primitives, so the day the card
  design changes (boxes → separator lines), the add affordance changes with it in the same place.
-->
<template>
  <button type="button" class="btn ghost list-add" :disabled="disabled" @click="$emit('click', $event)">
    <span class="list-add-glyph" aria-hidden="true">＋</span><slot>{{ t(label) }}</slot>
  </button>
</template>

<script setup lang="ts">
import { t } from "../../i18n";
withDefaults(defineProps<{
  /** What is being added, e.g. "New entry", "MCP server". Ignored when the default slot is used. */
  label?: string;
  disabled?: boolean;
}>(), { label: "Add", disabled: false });

defineEmits<{ click: [MouseEvent] }>();
</script>
