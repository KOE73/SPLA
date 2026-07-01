<template>
  <div class="s-panel" data-tab="plugins">
    <div class="s-head"><b>Plugins</b><span class="hint">{{ hint }}</span></div>
    <div class="pl-list">
      <div v-if="!plugins.length" class="notice">no plugins discovered</div>
      <div v-for="pl in plugins" :key="pl.id" class="conn-card">
        <div class="conn-head">
          <label class="pl-enable">
            <input type="checkbox" v-model="pl.enabled">
            <b>{{ pl.name || pl.id }}</b>
            <span class="ver">{{ pl.version || "" }} · {{ pl.id }}</span>
          </label>
          <span class="state">{{ pl.state && pl.state !== "Enabled" ? (pl.stateReason || pl.state) : "" }}</span>
        </div>
        <label class="field col"><span>Custom prompt</span><textarea v-model="pl.customPrompt" rows="2"></textarea></label>

        <!-- A plugin with its own web settings module renders itself here; everything else falls
             back to the generic opaque YAML editor. The panel never branches on plugin id. -->
        <PluginWebSettings v-if="pl.webSettingsUrl" :plugin="pl" :ref="(el) => setWebRef(pl.id, el)" />
        <label v-else class="field col"><span>Settings (YAML)</span><textarea v-model="pl.settingsYaml" class="mono" rows="4" spellcheck="false"></textarea></label>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { PluginDto } from "../../protocol/types";
import PluginWebSettings from "./PluginWebSettings.vue";

const plugins = ref<PluginDto[]>([]);
const hint = ref("");
const webRefs = new Map<string, InstanceType<typeof PluginWebSettings>>();

function setWebRef(id: string, el: unknown) {
  if (el) webRefs.set(id, el as InstanceType<typeof PluginWebSettings>);
  else webRefs.delete(id);
}

const off = client.on("plugins.result", p => {
  plugins.value = p.plugins || [];
  webRefs.clear();
  const bits: string[] = [];
  if (p.canPersist === false) bits.push("no .spla project — session-only");
  if (p.restartToApply) bits.push("enable/disable applies on next launch");
  hint.value = bits.join(" · ");
});
onUnmounted(off);

function save(): Promise<void> {
  // Pull the edited YAML out of each plugin's own mounted module before sending.
  for (const pl of plugins.value) {
    const handle = webRefs.get(pl.id);
    if (handle) pl.settingsYaml = handle.save() ?? undefined;
  }
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("plugins.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("plugins.save", { plugins: plugins.value });
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>
