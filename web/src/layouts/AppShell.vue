<!--
  Top-level app layout. Two regions:
    • a FIXED left navigation column — full height, never draggable/dockable, only width-resizable
      within a clamped range (persisted);
    • the right area (chat + dockable tools), whose own thin control strip lives inside DockWorkspace
      and therefore spans only this region.
-->
<template>
  <div class="app-shell">
    <aside class="left-nav" :style="{ width: navWidth + 'px' }">
      <NavigationSurface />
    </aside>
    <div class="nav-resizer" :class="{ dragging }" @pointerdown="startDrag" title="Drag to resize"></div>
    <div class="right-area">
      <DockWorkspace />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import NavigationSurface from "../surfaces/NavigationSurface.vue";
import DockWorkspace from "../dock/DockWorkspace.vue";

const MIN = 170, MAX = 480, KEY = "spla.leftnav.width";

const navWidth = ref(clamp(Number(localStorage.getItem(KEY)) || 240));
const dragging = ref(false);

function clamp(w: number) { return Math.min(MAX, Math.max(MIN, w)); }

function startDrag(e: PointerEvent) {
  dragging.value = true;
  const startX = e.clientX, startW = navWidth.value;
  const onMove = (ev: PointerEvent) => { navWidth.value = clamp(startW + (ev.clientX - startX)); };
  const onUp = () => {
    dragging.value = false;
    localStorage.setItem(KEY, String(navWidth.value));
    window.removeEventListener("pointermove", onMove);
    window.removeEventListener("pointerup", onUp);
  };
  window.addEventListener("pointermove", onMove);
  window.addEventListener("pointerup", onUp);
}
</script>

<style scoped>
.app-shell { display: flex; height: 100vh; min-height: 0; background: var(--bg); }
.left-nav { flex: 0 0 auto; height: 100%; min-height: 0; overflow: hidden; border-right: 1px solid var(--border); }
.nav-resizer { flex: 0 0 4px; cursor: col-resize; background: transparent; }
.nav-resizer:hover, .nav-resizer.dragging { background: var(--accent); }
.right-area { flex: 1 1 auto; min-width: 0; min-height: 0; display: flex; }
.right-area > * { flex: 1; min-width: 0; }
</style>
