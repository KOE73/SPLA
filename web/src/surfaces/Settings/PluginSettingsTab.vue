<!--
  Second-level Settings tab for ONE plugin's own settings UI. Fully generic: the host only knows the
  PluginDto and the PluginSettingsMount contract — the actual editor is the plugin's prebuilt module
  (web_settings_entry in its meta.yaml), dynamically imported by PluginWebSettings. The tab itself is
  created/removed by Settings.vue from plugins.result (enabled + has a web module), so toggling a
  plugin adds/removes its tab live. The dto arrives as a prop from Settings.vue — subscribing to
  plugins.result here would miss the broadcast that created this very tab.
-->
<template>
  <div class="s-panel" :data-tab="`plugin-${plugin.id}`">
    <div class="s-head">
      <b>{{ plugin.name || plugin.id }}</b>
      <span class="hint">{{ plugin.stateReason || "" }}</span>
    </div>
    <PluginWebSettings :key="mountKey" :plugin="plugin" ref="webRef" />
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { client } from "../../protocol/SplaClient";
import { projectEnvelope } from "../../state/project";
import type { PluginDto } from "../../protocol/types";
import PluginWebSettings from "./PluginWebSettings.vue";

const props = defineProps<{ plugin: PluginDto }>();

const webRef = ref<InstanceType<typeof PluginWebSettings>>();
// Remount the plugin module whenever a fresh dto object arrives (each plugins.result rebuilds the
// list) — otherwise an edit saved from the Plugins list would keep a stale editor here.
const mountKey = ref(0);
watch(() => props.plugin, () => mountKey.value++);

/** Pulls the edited YAML out of the mounted module and persists just this plugin's dto. */
function save(): Promise<void> {
  const pl = props.plugin;
  pl.settingsYaml = webRef.value?.save() ?? undefined;
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("plugins.result", () => { clearTimeout(timer); offRes(); resolve(); });
    if (!client.send("plugins.save", { plugins: [pl] }, projectEnvelope()))
      { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>
