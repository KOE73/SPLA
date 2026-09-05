<!--
  The one "add another one of these" button. ListPanel places it for you, always last. The ＋ glyph
  belongs to the button, not to the label: pass "Connection", not "＋ Add connection", so every add
  button in the app reads the same way. The "btn ghost" classes are the host's own (app.css) and are
  what makes this button identical to the core ones; ".list-add" carries the standalone fallback.
-->
<template>
  <button type="button" class="btn ghost list-add" :disabled="disabled" @click="$emit('click', $event)">
    <span class="list-add-glyph" aria-hidden="true">＋</span><slot>{{ label }}</slot>
  </button>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  /** What is being added, e.g. "Connection", "Host". Ignored when the default slot is used. */
  label?: string;
  disabled?: boolean;
}>(), { label: "Add", disabled: false });

defineEmits<{ click: [MouseEvent] }>();
</script>
