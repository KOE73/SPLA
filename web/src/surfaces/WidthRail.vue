<!--
  The reading measure, as a thing you can grab. Chat text is centred at --content-w rather than run
  edge-to-edge, and this handle is how a person disagrees with the default.

  It sits ON the line above the composer, at the right edge of the column, as a small triangle
  pointing up out of that line — so its position always states where the column's edge currently is,
  and it is visible without having to be hunted for. Control is explicit and nothing else: press,
  move, release. No hover-follow, no snapping back. Double-click restores the default.
-->
<template>
  <div
    ref="el"
    class="width-handle"
    :class="{ dragging }"
    title="Drag to set the reading width — double-click to reset"
    @pointerdown="onDown"
    @dblclick="reset"
  ></div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

const KEY = "spla.contentWidth";
const DEFAULT = 900;
const MIN = 480;

const el = ref<HTMLElement>();
const dragging = ref(false);

function apply(px: number) {
  document.documentElement.style.setProperty("--content-w", `${Math.round(px)}px`);
}
function current(): number {
  return parseInt(getComputedStyle(document.documentElement).getPropertyValue("--content-w")) || DEFAULT;
}

onMounted(() => {
  const saved = Number(localStorage.getItem(KEY));
  if (saved >= MIN) apply(saved);
});

/**
 * Press starts a drag and nothing else does. The pointer is CAPTURED by the handle, so every move
 * and the release are delivered here even when the pointer has long left the 12 px triangle — which
 * is the whole difference between a handle and a thing that merely reacts to being hovered. Both
 * listeners hang off the handle itself (capture retargets to it) and both come off again on release,
 * so nothing survives the gesture.
 */
function onDown(e: PointerEvent) {
  const handle = el.value;
  const host = handle?.offsetParent as HTMLElement | null;
  if (!handle || !host) return;

  e.preventDefault();                 // no text selection, no native drag
  dragging.value = true;
  handle.setPointerCapture(e.pointerId);

  const move = (ev: PointerEvent) => {
    const box = host.getBoundingClientRect();
    // Width is twice the distance from the centre line, so the column stays centred as it grows.
    const half = ev.clientX - (box.left + box.width / 2);
    apply(Math.max(MIN, Math.min(box.width, half * 2)));
  };

  const end = (ev: PointerEvent) => {
    // Unbind FIRST. The previous version released the capture first, through a stale
    // `e.currentTarget` — which the DOM nulls out once dispatch ends, so the release threw and the
    // three removes below it never ran. The move listener survived the gesture and the column then
    // followed the bare pointer forever, settling on nothing: exactly the "reacts to hover, won't
    // fix" this handle is supposed to be the opposite of. Nothing here can throw before the unbind.
    handle.removeEventListener("pointermove", move);
    handle.removeEventListener("pointerup", end);
    handle.removeEventListener("pointercancel", end);
    dragging.value = false;
    handle.releasePointerCapture?.(ev.pointerId);
    localStorage.setItem(KEY, String(current()));
  };

  handle.addEventListener("pointermove", move);
  handle.addEventListener("pointerup", end);
  handle.addEventListener("pointercancel", end);
}

function reset() {
  apply(DEFAULT);
  localStorage.setItem(KEY, String(DEFAULT));
}
</script>
