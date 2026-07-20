<template>
  <div ref="mountEl" class="pl-web-settings"></div>
</template>

<script setup lang="ts">
// Mounts a plugin's own prebuilt web settings module (see web_settings_entry in meta.yaml) into this
// slot at runtime. This component — and the rest of web/ — never imports or knows the plugin's code
// at build time; only the URL and the PluginSettingsMount contract (web/src/protocol/types.ts).
import { onBeforeUnmount, onMounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { PluginDto, PluginSettingsHandle, PluginSettingsMount } from "../../protocol/types";

const props = defineProps<{ plugin: PluginDto }>();
const mountEl = ref<HTMLElement | null>(null);
let handle: PluginSettingsHandle | null = null;

onMounted(async () => {
  if (!props.plugin.webSettingsUrl || !mountEl.value) return;
  try {
    const mod = await import(/* @vite-ignore */ props.plugin.webSettingsUrl);
    const mount = mod.mount as PluginSettingsMount;
    handle = mount(mountEl.value, {
      getJson: () => props.plugin.settingsJson ?? null,
      invoke: (type, payload) => client.invoke(type, payload)
    });
  } catch (e) {
    console.error("failed to load plugin web settings:", props.plugin.id, e);
  }
});

onBeforeUnmount(() => handle?.destroy?.());

/** Returns the edited settings as JSON; falls back to the unedited blob if the module failed to load. */
function save(): string | null {
  return handle ? handle.save() : (props.plugin.settingsJson ?? null);
}

defineExpose({ save });
</script>
