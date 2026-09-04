<!-- Copies `text` to the clipboard. ⧉ alone for a tight inline slot (a chip), "⧉ label" when there's
     room — the same component either way, so every copy-ref affordance in the app looks and behaves
     the same. Shows "copied" in the title for a moment as the only feedback; nothing else changes. -->
<template>
  <IconGlyphButton variant="plain" :title="justCopied ? 'Copied!' : title" @click="copy">
    <template v-if="label">⧉ {{ label }}</template>
    <template v-else>⧉</template>
  </IconGlyphButton>
</template>

<script setup lang="ts">
import { ref } from "vue";
import IconGlyphButton from "./IconGlyphButton.vue";

const props = withDefaults(defineProps<{
  text: string;
  /** Optional visible label after the glyph, e.g. "ref". Omit for an icon-only chip button. */
  label?: string;
  title?: string;
}>(), { title: "Copy" });

const justCopied = ref(false);

function copy() {
  navigator.clipboard?.writeText(props.text).then(() => {
    justCopied.value = true;
    setTimeout(() => { justCopied.value = false; }, 1200);
  }).catch(() => {});
}
</script>
