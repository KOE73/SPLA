<template>
  <div class="plugin-panel">
    <div v-if="error" class="failed">{{ t('This panel failed to load.') }} <code>{{ error }}</code></div>
    <div ref="mountEl" class="mount"></div>
  </div>
</template>

<script setup lang="ts">
// One dock component for EVERY plugin-supplied panel. It knows a URL and the PluginPanelMount
// contract (protocol/types.ts) and nothing else — no plugin is imported at build time, and none is
// named anywhere in web/. Registered once under the component name "pluginPanel", so a layout
// restored from localStorage mounts a plugin panel even before plugins.result has arrived: the URL
// rides in the panel's own params, which dockview serialises.
import { onBeforeUnmount, onMounted, ref, watch } from "vue";
import { client } from "../protocol/SplaClient";
import { store } from "../state/store";
import { t } from "../i18n";
import type { PluginPanelHandle, PluginPanelMount } from "../protocol/types";

const props = defineProps<{ params: { params?: { panelUrl?: string; pluginId?: string } } }>();

const mountEl = ref<HTMLElement | null>(null);
const error = ref<string | null>(null);
let handle: PluginPanelHandle | null = null;

const chatHandlers = new Set<(chatId: string | null) => void>();
// Unconditional: in dockview, mounted IS open. No "is it visible" guard — that guard is what left
// the debug panel showing the first chat's data forever while its title updated correctly.
const stopChatWatch = watch(() => store.currentChat, id => {
  for (const fn of chatHandlers) { try { fn(id); } catch (e) { console.error("plugin panel chat handler", e); } }
});

onMounted(async () => {
  const url = props.params?.params?.panelUrl;
  if (!url || !mountEl.value) { error.value = "no panel URL"; return; }
  try {
    const mod = await import(/* @vite-ignore */ url);
    const mount = mod.mount as PluginPanelMount;
    if (typeof mount !== "function") throw new Error("module exports no mount()");
    handle = mount(mountEl.value, {
      send: (type, payload) => client.send(type, payload),
      on: (type, fn) => client.on(type as never, fn as never),
      invoke: (type, payload) => client.invoke(type, payload),
      t,
      currentChatId: () => store.currentChat,
      onChatChange: fn => { chatHandlers.add(fn); return () => chatHandlers.delete(fn); },
    });
  } catch (e) {
    error.value = String(e);
    console.error("failed to load plugin panel:", props.params?.params?.pluginId, e);
  }
});

onBeforeUnmount(() => {
  stopChatWatch();
  chatHandlers.clear();
  handle?.destroy?.();
});
</script>

<style scoped>
.plugin-panel { height: 100%; min-height: 0; display: flex; flex-direction: column; overflow: auto; }
.mount { flex: 1; min-height: 0; }
.failed { padding: 8px 10px; color: var(--muted); font-size: var(--fs-sm); }
.failed code { color: var(--text); }
</style>
