<!--
  The one image viewer. Mounted once in LayoutHost and driven by state/lightbox.ts — any surface that
  shows a picture opens it with openLightbox()/openChatImage(), none mounts its own.

  Opens fitted to the window. Wheel zooms around the cursor, drag pans, a click (or "1:1") toggles
  between fitted and actual pixels, ←/→ step through the set, Esc or a click on the backdrop closes.
-->
<template>
  <div v-if="current" class="lightbox" role="dialog" aria-modal="true" @click.self="closeLightbox()">
    <div ref="stage" class="lb-stage" :class="{ panning: drag != null, zoomed: !fitted }"
         @click.self="closeLightbox()" @wheel.prevent="onWheel">
      <img
        :key="current.url"
        :src="current.url"
        :alt="current.label || ''"
        class="lb-img"
        :style="imgStyle"
        draggable="false"
        @load="onLoad"
        @pointerdown="onPointerDown"
        @pointermove="onPointerMove"
        @pointerup="onPointerUp"
        @pointercancel="drag = null"
      >
    </div>

    <button v-if="count > 1" class="lb-nav lb-prev" :title="t('Previous image')" @click="step(-1)">‹</button>
    <button v-if="count > 1" class="lb-nav lb-next" :title="t('Next image')" @click="step(1)">›</button>

    <footer class="lb-bar">
      <span class="lb-caption" :title="current.label">{{ current.label || t('Image') }}</span>
      <span v-if="count > 1" class="lb-count">{{ lightbox.index + 1 }} / {{ count }}</span>
      <span v-if="natural.w" class="lb-dims">{{ natural.w }}×{{ natural.h }} · {{ Math.round(scale * 100) }}%</span>
      <span class="lb-spacer" />
      <button class="lb-btn" :class="{ on: fitted }" :title="t('Fit to window')" @click="fit()">{{ t('Fit') }}</button>
      <button class="lb-btn" :class="{ on: !fitted && scale === 1 }" :title="t('Actual size')" @click="actual()">1:1</button>
      <button class="lb-btn" :title="t('Close (Esc)')" @click="closeLightbox()">✕</button>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, reactive, ref, watch } from "vue";
import { t } from "../i18n";
import { closeLightbox, lightbox } from "../state/lightbox";

const stage = ref<HTMLElement | null>(null);
const current = computed(() => lightbox.images[lightbox.index] ?? null);
const count = computed(() => lightbox.images.length);

const natural = reactive({ w: 0, h: 0 });
/** Pixels of screen per pixel of image, and where the image's top-left sits in the stage. */
const scale = ref(1);
const pos = reactive({ x: 0, y: 0 });
const fitted = ref(true);
const drag = ref<{ id: number; x: number; y: number; px: number; py: number; moved: boolean } | null>(null);

const imgStyle = computed(() => ({
  width: natural.w ? natural.w + "px" : undefined,
  height: natural.h ? natural.h + "px" : undefined,
  transform: `translate(${pos.x}px, ${pos.y}px) scale(${scale.value})`
}));

function stageSize() {
  const el = stage.value;
  return { w: el?.clientWidth ?? window.innerWidth, h: el?.clientHeight ?? window.innerHeight };
}

function fitScale() {
  const { w, h } = stageSize();
  if (!natural.w || !natural.h) return 1;
  // Never blown up past its own pixels: a small icon fitted to a big window is a blur, not a view.
  return Math.min(1, w / natural.w, h / natural.h);
}

function centerAt(s: number) {
  const { w, h } = stageSize();
  scale.value = s;
  pos.x = (w - natural.w * s) / 2;
  pos.y = (h - natural.h * s) / 2;
}

function fit() { fitted.value = true; centerAt(fitScale()); }
function actual() { fitted.value = false; centerAt(1); }

function onLoad(e: Event) {
  const img = e.target as HTMLImageElement;
  natural.w = img.naturalWidth;
  natural.h = img.naturalHeight;
  fit();
}

/** Zooms by `factor` keeping the image point under (cx, cy) — stage coordinates — where it is. */
function zoomAt(factor: number, cx: number, cy: number) {
  const next = Math.min(20, Math.max(0.05, scale.value * factor));
  const k = next / scale.value;
  pos.x = cx - (cx - pos.x) * k;
  pos.y = cy - (cy - pos.y) * k;
  scale.value = next;
  fitted.value = false;
}

function onWheel(e: WheelEvent) {
  const r = stage.value?.getBoundingClientRect();
  if (!r) return;
  zoomAt(e.deltaY < 0 ? 1.2 : 1 / 1.2, e.clientX - r.left, e.clientY - r.top);
}

function onPointerDown(e: PointerEvent) {
  if (e.button !== 0) return;
  (e.target as HTMLElement).setPointerCapture(e.pointerId);
  drag.value = { id: e.pointerId, x: e.clientX, y: e.clientY, px: pos.x, py: pos.y, moved: false };
}

function onPointerMove(e: PointerEvent) {
  const d = drag.value;
  if (!d || d.id !== e.pointerId) return;
  const dx = e.clientX - d.x, dy = e.clientY - d.y;
  if (!d.moved && Math.hypot(dx, dy) < 4) return;
  d.moved = true;
  pos.x = d.px + dx;
  pos.y = d.py + dy;
}

/** A press that did not move is a click: it toggles fitted ↔ 1:1 around the clicked point. */
function onPointerUp(e: PointerEvent) {
  const d = drag.value;
  drag.value = null;
  if (!d || d.moved) return;
  if (!fitted.value) { fit(); return; }
  const r = stage.value?.getBoundingClientRect();
  if (!r) { actual(); return; }
  const cx = e.clientX - r.left, cy = e.clientY - r.top;
  zoomAt(1 / scale.value, cx, cy);
}

function step(delta: number) {
  if (count.value < 2) return;
  lightbox.index = (lightbox.index + delta + count.value) % count.value;
}

// A new picture starts unknown: its size arrives with its load event, which fits it.
watch(() => current.value?.url, () => { natural.w = natural.h = 0; fitted.value = true; });

function onKey(e: KeyboardEvent) {
  if (!current.value) return;
  const handled = (() => {
    switch (e.key) {
      case "Escape": closeLightbox(); return true;
      case "ArrowLeft": step(-1); return true;
      case "ArrowRight": step(1); return true;
      case "0": case "f": fit(); return true;
      case "1": actual(); return true;
      case "+": case "=": { const { w, h } = stageSize(); zoomAt(1.2, w / 2, h / 2); return true; }
      case "-": { const { w, h } = stageSize(); zoomAt(1 / 1.2, w / 2, h / 2); return true; }
    }
    return false;
  })();
  // Capture phase and stopped: the viewer sits over everything, so the composer or an overlay
  // underneath must not also act on the same Escape or arrow key.
  if (handled) { e.preventDefault(); e.stopPropagation(); }
}

function onResize() { if (fitted.value) nextTick(fit); }

onMounted(() => {
  window.addEventListener("keydown", onKey, true);
  window.addEventListener("resize", onResize);
});
onUnmounted(() => {
  window.removeEventListener("keydown", onKey, true);
  window.removeEventListener("resize", onResize);
});
</script>

<style scoped>
.lightbox {
  position: fixed; inset: 0; z-index: 400;
  display: flex; flex-direction: column;
  background: color-mix(in srgb, var(--bg) 20%, rgba(0, 0, 0, .82));
  color: var(--text);
}
.lb-stage { position: relative; flex: 1 1 auto; overflow: hidden; cursor: zoom-out; }
.lb-img {
  position: absolute; left: 0; top: 0; transform-origin: 0 0; max-width: none;
  cursor: zoom-in; user-select: none; touch-action: none;
  image-rendering: auto; box-shadow: 0 4px 24px rgba(0, 0, 0, .5);
  background: repeating-conic-gradient(color-mix(in srgb, var(--border) 60%, transparent) 0 25%, transparent 0 50%) 0 0 / 16px 16px;
}
.lb-stage.zoomed .lb-img { cursor: grab; }
.lb-stage.panning .lb-img { cursor: grabbing; }

.lb-nav {
  position: absolute; top: 50%; transform: translateY(-50%);
  width: 44px; height: 64px; border: 1px solid var(--border); border-radius: var(--radius-sm);
  background: color-mix(in srgb, var(--panel) 80%, transparent); color: var(--text);
  font-size: 28px; line-height: 1; cursor: pointer; opacity: .75;
}
.lb-nav:hover { opacity: 1; color: var(--accent); }
.lb-prev { left: 12px; }
.lb-next { right: 12px; }

.lb-bar {
  flex: 0 0 auto; display: flex; align-items: center; gap: 10px; flex-wrap: wrap;
  padding: 6px 12px; background: var(--panel); border-top: 1px solid var(--border);
  font-size: var(--fs-sm);
}
.lb-caption { font-weight: 600; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 50%; }
.lb-count, .lb-dims { color: var(--muted); font-size: var(--fs-xs); white-space: nowrap; }
.lb-spacer { flex: 1 1 auto; }
.lb-btn {
  background: none; border: 1px solid var(--border); border-radius: var(--radius-sm);
  color: var(--muted); font: inherit; font-size: var(--fs-xs); padding: 2px 8px; cursor: pointer;
}
.lb-btn:hover { color: var(--text); }
.lb-btn.on { color: var(--accent); border-color: var(--accent); }
</style>
