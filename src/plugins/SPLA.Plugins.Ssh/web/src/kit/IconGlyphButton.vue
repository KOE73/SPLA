<!--
  The one base every small glyph button in this panel is built from. Mirrors
  web/src/components/buttons/IconGlyphButton.vue in the main client; all styling lives in kit.css
  under ".gbtn", so a restyle is one edit there and never a change in a panel.
-->
<template>
  <button
    type="button"
    class="gbtn"
    :class="[`gbtn-${variant}`, { on }]"
    :disabled="disabled"
    :title="title"
    @click.stop="$emit('click', $event)"
  ><slot /></button>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  title?: string;
  /** Hover/active colour family. "danger" for destructive actions, "plain" for everything else. */
  variant?: "plain" | "danger";
  disabled?: boolean;
  /** Toggled-open visual state (carets). */
  on?: boolean;
}>(), { variant: "plain", disabled: false, on: false });

defineEmits<{ click: [MouseEvent] }>();
</script>
