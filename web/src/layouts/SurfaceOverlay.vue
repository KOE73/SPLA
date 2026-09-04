<!--
  Full-screen layer for a surface opened over the app (see state/overlay.ts and
  ADR_20260904-3_web_settings-placement).

  It covers the shell rather than sitting inside the dock: settings are not a tool panel you arrange
  next to the chat, they are a place you go to and come back from. Hence exactly one way out — the
  back button, doubled by Escape — and no resize/dock affordances.
-->
<template>
  <div v-if="overlaySurface" class="surface-overlay">
    <header class="overlay-bar">
      <button class="overlay-back" @click="closeOverlay()" title="Escape">← Back to the app</button>
    </header>
    <div class="overlay-body">
      <component :is="surfaces[overlaySurface]" v-if="surfaces[overlaySurface]" />
      <div v-else class="surface-missing">no surface: {{ overlaySurface }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from "vue";
import { surfaces } from "../surfaces/registry";
import { overlaySurface, closeOverlay } from "../state/overlay";

// Escape closes it because nothing here is a pending transaction: reversible preferences apply
// instantly, and the sections that are transactional (connections, permissions) own their Save
// button. Leaving therefore cannot lose work — feedback "auto-apply vs Save".
function onKey(e: KeyboardEvent) {
  if (e.key === "Escape" && overlaySurface.value) { e.stopPropagation(); closeOverlay(); }
}
// Capture phase: a panel's own input must not swallow the one gesture out of here.
onMounted(() => window.addEventListener("keydown", onKey, true));
onUnmounted(() => window.removeEventListener("keydown", onKey, true));
</script>

<style scoped>
.surface-overlay {
  position: fixed; inset: 0; z-index: 300;
  display: flex; flex-direction: column;
  background: var(--bg);
}
.overlay-bar {
  flex: 0 0 auto; display: flex; align-items: center; gap: 8px;
  padding: 6px 10px; border-bottom: 1px solid var(--border); background: var(--panel);
}
.overlay-back {
  background: none; border: none; color: var(--muted); cursor: pointer;
  font: inherit; font-size: var(--fs-sm); padding: 2px 6px; border-radius: 4px;
}
.overlay-back:hover { color: var(--fg); background: var(--elevated); }
.overlay-body { flex: 1 1 auto; min-height: 0; overflow: hidden; }
.surface-missing { padding: 16px; color: var(--muted); }
</style>
