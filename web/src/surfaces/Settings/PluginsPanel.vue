<template>
  <div class="s-panel" data-tab="plugins">
    <div class="s-head"><b>{{ t('Plugins') }}</b><span class="hint">{{ t(hint) }}</span></div>
    <!-- No add button: plugins are discovered on disk, not created here — hence no `add-label`. -->
    <ListPanel :empty="!plugins.length" :empty-text="t('no plugins discovered')">
      <!-- One collapsed row per plugin; click the row to expand its editors. Configured bits show
           as small summary text on the collapsed row so a glance tells what's customized. -->
      <ListCard v-for="pl in plugins" :key="pl.id" :open="isOpen(pl.id)" @update:open="toggle(pl.id)"
                :data-plugin-id="pl.id">
        <template #head>
          <input type="checkbox" v-model="pl.enabled" @click.stop>
          <b class="pl-name">{{ pl.name || pl.id }}</b>
          <span class="ver">{{ pl.version || "" }} · {{ pl.id }}</span>
          <span v-if="!isOpen(pl.id)" class="pl-sum">{{ summary(pl) }}</span>
          <span class="grow"></span>
          <span class="state">{{ pl.state && pl.state !== "Enabled" ? (pl.stateReason || pl.state) : "" }}</span>
          <ExpandButton :open="isOpen(pl.id)" @update:open="toggle(pl.id)" />
        </template>

        <template #body>
          <!-- Two separate decisions, and the wording has to keep them apart: the checkbox above is
               DELIVERY (is the assembly loaded at all), this is DISCLOSURE (how much of the set the
               model is shown before it is needed). -->
          <label class="field col">
            <span>{{ t('Tools in context') }}</span>
            <select v-model="pl.level" :disabled="pl.enabled === false">
              <option value="">{{ t('follow the enable flag') }}</option>
              <option value="enabled">{{ t('always — full definitions in every request') }}</option>
              <option value="agent_demand">{{ t('announced — one line; the agent loads it when needed') }}</option>
              <option value="skill_demand">{{ t('on skill demand — nothing until a skill requires it') }}</option>
              <option value="disabled">{{ t('never — the set does not exist for the model') }}</option>
            </select>
          </label>
          <label class="field col"><span>{{ t('Custom prompt') }}</span><textarea v-model="pl.customPrompt" rows="2"></textarea></label>
          <!-- A plugin with its own web settings module renders itself here; everything else falls
               back to the generic opaque JSON editor. The panel never branches on plugin id. -->
          <PluginWebSettings v-if="pl.webSettingsUrl" :plugin="pl" :ref="(el) => setWebRef(pl.id, el)" />
          <label v-else class="field col"><span>{{ t('Settings (JSON)') }}</span><textarea v-model="pl.settingsJson" class="mono" rows="4" spellcheck="false"></textarea></label>
        </template>
      </ListCard>
    </ListPanel>
  </div>
</template>

<script setup lang="ts">
import { t } from "../../i18n";
import { onUnmounted, ref } from "vue";
import { client } from "../../protocol/SplaClient";
import type { PluginDto } from "../../protocol/types";
import PluginWebSettings from "./PluginWebSettings.vue";
import ListPanel from "../../components/list/ListPanel.vue";
import ListCard from "../../components/list/ListCard.vue";
import ExpandButton from "../../components/buttons/ExpandButton.vue";

/** Collapsed-row wording for a level the user set explicitly. */
const LEVEL_LABELS: Record<string, string> = {
  enabled: "always",
  agent_demand: "announced",
  skill_demand: "on skill demand",
  disabled: "never"
};

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
  if (pl.level) bits.push(`tools: ${LEVEL_LABELS[pl.level] ?? pl.level}`);
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
  if (p.canPersist === false) bits.push(t("no .spla project — session-only"));
  if (p.restartToApply) bits.push(t("enable/disable applies on next launch"));
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
    const ok = client.send("plugins.save", { plugins: plugins.value });
    if (!ok) { clearTimeout(timer); offRes(); reject(new Error("socket closed")); }
  });
}

defineExpose({ save });
</script>

<style scoped>
/* .pl-list/.pl-card/.pl-row/.pl-body/.chev are gone — ListPanel/ListCard (app.css) draw the list,
   the card and the caret; only content specific to a plugin row stays here. */
.pl-name { font-size: var(--fs-sm); }
.ver { font-family: var(--mono); font-size: var(--fs-xs); color: var(--muted); }
.pl-sum { font-size: var(--fs-xs); color: var(--muted); margin-left: 8px;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 45%; }
.grow { flex: 1; }
.state { font-size: var(--fs-xs); color: var(--danger, #f85149); }
</style>
