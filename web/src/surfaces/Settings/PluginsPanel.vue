<template>
  <div class="s-panel" data-tab="plugins">
    <div class="s-head"><b>Plugins</b><span class="hint">{{ hint }}</span></div>
    <div class="pl-list">
      <div v-if="!plugins.length" class="notice">no plugins discovered</div>
      <!-- One collapsed row per plugin; click the row to expand its editors. Configured bits show
           as small summary text on the collapsed row so a glance tells what's customized. -->
      <div v-for="pl in plugins" :key="pl.id" class="pl-card" :class="{ open: isOpen(pl.id) }">
        <div class="pl-row" @click="toggle(pl.id)">
          <input type="checkbox" v-model="pl.enabled" @click.stop>
          <b class="pl-name">{{ pl.name || pl.id }}</b>
          <span class="ver">{{ pl.version || "" }} · {{ pl.id }}</span>
          <span v-if="!isOpen(pl.id)" class="pl-sum">{{ summary(pl) }}</span>
          <span class="grow"></span>
          <span class="state">{{ pl.state && pl.state !== "Enabled" ? (pl.stateReason || pl.state) : "" }}</span>
          <span class="chev">{{ isOpen(pl.id) ? "▾" : "▸" }}</span>
        </div>

        <div v-if="isOpen(pl.id)" class="pl-body">
          <label class="field col"><span>Custom prompt</span><textarea v-model="pl.customPrompt" rows="2"></textarea></label>
          <!-- A plugin with its own web settings module renders itself here; everything else falls
               back to the generic opaque JSON editor. The panel never branches on plugin id. -->
          <PluginWebSettings v-if="pl.webSettingsUrl" :plugin="pl" :ref="(el) => setWebRef(pl.id, el)" />
          <label v-else class="field col"><span>Settings (JSON)</span><textarea v-model="pl.settingsJson" class="mono" rows="4" spellcheck="false"></textarea></label>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import { projectEnvelope } from "../../state/project";
import type { PluginDto } from "../../protocol/types";
import PluginWebSettings from "./PluginWebSettings.vue";

const plugins = ref<PluginDto[]>([]);
const hint = ref("");
const webRefs = new Map<string, InstanceType<typeof PluginWebSettings>>();
const open = ref<Set<string>>(new Set());

function isOpen(id: string) { return open.value.has(id); }
function toggle(id: string) {
  const next = new Set(open.value);
  next.has(id) ? next.delete(id) : next.add(id);
  open.value = next;
}

/** Collapsed-row hint of what's already configured — never the values, just what exists. */
function summary(pl: PluginDto): string {
  const bits: string[] = [];
  if (pl.customPrompt?.trim()) bits.push(`prompt: ${pl.customPrompt.trim().slice(0, 40)}${pl.customPrompt.trim().length > 40 ? "…" : ""}`);
  const json = pl.settingsJson?.trim();
  if (json) bits.push(`settings: ${json.length} chars`);
  else if (pl.webSettingsUrl) bits.push("has settings UI");
  return bits.join(" · ");
}

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
  // Pull the edited JSON out of each plugin's own mounted module before sending. Modules exist only
  // for expanded cards; collapsed ones keep whatever settingsJson the server sent — unchanged.
  for (const pl of plugins.value) {
    const handle = webRefs.get(pl.id);
    if (handle) pl.settingsJson = handle.save() ?? undefined;
  }
  return new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => { offRes(); reject(new Error("save timed out")); }, 8000);
    const offRes = client.on("plugins.result", () => { clearTimeout(timer); offRes(); resolve(); });
    const ok = client.send("plugins.save", { plugins: plugins.value }, projectEnvelope());
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>

<style scoped>
/* Density-aware: gaps/radius follow the UI density vars. */
.pl-list { display: flex; flex-direction: column; gap: var(--gap, 8px); }
.pl-card { border: 1px solid var(--border); border-radius: var(--radius, 7px); background: var(--elevated); }
.pl-row { display: flex; align-items: center; gap: 8px; padding: 4px 8px; cursor: pointer; min-height: 26px; }
.pl-row:hover { background: color-mix(in srgb, var(--text) 4%, transparent); }
.pl-name { font-size: var(--fs-sm); }
.ver { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted); }
.pl-sum { font-size: var(--fs-xs); color: var(--muted); margin-left: 8px;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 45%; }
.grow { flex: 1; }
.state { font-size: var(--fs-xs); color: var(--danger, #f85149); }
.chev { color: var(--muted); font-size: var(--fs-xs); width: 12px; text-align: center; }
.pl-body { display: flex; flex-direction: column; gap: 6px; padding: 2px 8px 8px;
  border-top: 1px solid color-mix(in srgb, var(--border) 60%, transparent); }
</style>
