<template>
  <div class="screencast-surface">
    <form class="browser-bar" @submit.prevent="navigate">
      <button type="button" title="Reload" @click="navigate">↻</button>
      <input v-model="address" aria-label="Browser address" placeholder="Enter URL" />
      <button type="submit">Go</button>
      <span class="state">{{ state }}</span>
    </form>
    <div ref="viewport" class="browser-viewport" tabindex="0" @keydown.prevent="keyFrame" @wheel.prevent="wheelFrame">
      <img v-if="frame" :src="frame" draggable="false" alt="Browser screencast"
           @pointerdown.prevent="pointerFrame('pointerDown', $event)"
           @pointermove.prevent="pointerFrame('pointerMove', $event)"
           @pointerup.prevent="pointerFrame('pointerUp', $event)" />
      <div v-else class="browser-empty">{{ error || "Starting experimental browser…" }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";

const panelId = "browser-screencast-main";
const viewport = ref<HTMLDivElement | null>(null);
const address = ref("about:blank");
const frame = ref("");
const state = ref("connecting");
const error = ref("");
const disposers: Array<() => void> = [];

function projectExtra() { return store.currentProjectId ? { projectId: store.currentProjectId } : undefined; }
function sendInput(inputType: string, data: object) {
  client.send("plugin.panel.input", { panelId, inputType, data }, projectExtra());
}
function navigate() { sendInput("navigate", { url: address.value }); }

function framePoint(event: PointerEvent) {
  const image = event.currentTarget as HTMLImageElement;
  const bounds = image.getBoundingClientRect();
  return {
    x: (event.clientX - bounds.left) * image.naturalWidth / bounds.width,
    y: (event.clientY - bounds.top) * image.naturalHeight / bounds.height,
    button: event.button,
  };
}
function pointerFrame(inputType: string, event: PointerEvent) {
  viewport.value?.focus();
  if (inputType === "pointerDown") (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
  sendInput(inputType, framePoint(event));
}
function keyFrame(event: KeyboardEvent) {
  if (event.key.length === 1 && !event.ctrlKey && !event.altKey && !event.metaKey) {
    sendInput("text", { text: event.key });
    return;
  }
  const modifiers = [event.ctrlKey && "Control", event.altKey && "Alt", event.shiftKey && "Shift", event.metaKey && "Meta"].filter(Boolean);
  const key = [...modifiers, event.key].join("+");
  sendInput("key", { key });
}
function wheelFrame(event: WheelEvent) {
  sendInput("wheel", { deltaX: event.deltaX, deltaY: event.deltaY });
}

onMounted(() => {
  let opened = false;
  const open = () => {
    if (opened || !client.send("plugin.panel.open", { panelId, panelType: "browser.screencast", parameters: {} }, projectExtra())) return;
    opened = true;
  };
  if (store.connected) open();
  disposers.push(client.on("conn", event => { if (event.on) open(); }));
  disposers.push(client.on("plugin.panel.opened", event => {
    if (event.panelId === panelId) state.value = "live";
  }));
  disposers.push(client.on("plugin.panel.event", event => {
    if (event.panelId !== panelId) return;
    if (event.eventType === "frame" && event.data?.base64) {
      frame.value = `data:${event.data.mimeType || "image/jpeg"};base64,${event.data.base64}`;
      if (event.data.url) address.value = event.data.url;
    } else if (event.eventType === "error") {
      error.value = event.data?.message || "Browser error";
      state.value = "error";
    }
  }));
  const resizeObserver = new ResizeObserver(entries => {
    const bounds = entries[0]?.contentRect;
    if (bounds && bounds.width > 0 && bounds.height > 0)
      sendInput("resize", { width: Math.max(320, Math.round(bounds.width)), height: Math.max(200, Math.round(bounds.height)) });
  });
  if (viewport.value) resizeObserver.observe(viewport.value);
  disposers.push(() => resizeObserver.disconnect());
});

onBeforeUnmount(() => {
  client.send("plugin.panel.close", { panelId }, projectExtra());
  disposers.forEach(dispose => dispose());
});
</script>

<style scoped>
.screencast-surface { display: flex; flex-direction: column; width: 100%; height: 100%; min-height: 0; background: #111; }
.browser-bar { display: flex; align-items: center; gap: 5px; height: 32px; padding: 3px 6px; background: var(--panel); border-bottom: 1px solid var(--border); }
.browser-bar input { flex: 1; min-width: 80px; height: 24px; padding: 2px 7px; color: var(--text); background: var(--bg); border: 1px solid var(--border); border-radius: 4px; }
.browser-bar button { height: 24px; color: var(--text); background: transparent; border: 1px solid var(--border); border-radius: 4px; }
.state { color: var(--muted); font-size: var(--fs-xs); }
.browser-viewport { flex: 1; min-height: 0; display: grid; place-items: center; overflow: hidden; }
.browser-viewport img { display: block; max-width: 100%; max-height: 100%; object-fit: contain; user-select: none; }
.browser-empty { color: var(--muted); }
</style>
