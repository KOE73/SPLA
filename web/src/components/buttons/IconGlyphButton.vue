<!--
  The one base every small glyph button in the app is built from — trash, caret, copy, remove.
  Consumers never write button CSS; they pick a semantic wrapper (DeleteButton, ExpandButton,
  CopyButton, RemoveButton) and this is what those render. All styling lives in app.css under
  ".gbtn" so a single edit there restyles every glyph button in the app at once — see the
  "Shared glyph buttons" section.
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
  /** Hover/active color family. "danger" is for destructive actions (delete); "plain" for everything else. */
  variant?: "plain" | "danger";
  disabled?: boolean;
  /** Toggled-open visual state (carets, active filters). */
  on?: boolean;
}>(), { variant: "plain", disabled: false, on: false });

defineEmits<{ click: [MouseEvent] }>();
</script>
