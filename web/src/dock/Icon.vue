<!--
  Tool-strip icon set. Each icon is a standalone, editable file under assets/icons/<name>.svg (open
  it directly in Inkscape/Illustrator to restyle) — this component just strips the outer <svg> tag
  and re-renders the inner markup inside our own chrome (fixed 16×16, stroke=currentColor) so every
  icon stays visually consistent regardless of how its file is authored.
-->
<template>
  <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor"
       stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
    <g v-html="inner" />
  </svg>
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = defineProps<{ name: string }>();

// Raw file contents, e.g. '<svg viewBox="0 0 24 24" ...>...markup...</svg>\n'.
const rawIcons = import.meta.glob("../assets/icons/*.svg", { eager: true, query: "?raw", import: "default" }) as Record<string, string>;

// Keyed by filename without extension — "../assets/icons/ssh.svg" → "ssh".
const bodies: Record<string, string> = {};
for (const [path, raw] of Object.entries(rawIcons)) {
  const name = path.split("/").pop()!.replace(/\.svg$/, "");
  bodies[name] = raw.replace(/^[\s\S]*?<svg[^>]*>/, "").replace(/<\/svg>\s*$/, "");
}

const inner = computed(() => bodies[props.name] ?? "");
</script>
