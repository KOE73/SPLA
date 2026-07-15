<template>
  <!-- Two nested elements on purpose: .settings-surface gives the solo window a full-height
       flex-column frame; .settings-shell inside it is the actual flex-row two-pane layout
       (nav | main). They must stay on separate elements — merging them collapses the row
       layout because .settings-surface's flex-direction:column wins the cascade. -->
  <div class="settings-surface">
    <div class="settings-shell">
      <nav class="settings-nav">
        <div class="nav-section">Settings</div>
        <template v-for="t in TABS" :key="t.id">
          <div class="nav-item" :class="{ on: tab === t.id }" @click="tab = t.id">
            <span class="nav-ic">{{ t.icon }}</span>{{ t.label }}
          </div>
          <!-- Second-level tabs: one per ENABLED plugin that ships its own settings UI. Driven live
               by plugins.result, so toggling a plugin in the Plugins list adds/removes its tab. -->
          <template v-if="t.id === 'plugins'">
            <div v-for="pl in pluginTabs" :key="pl.id" class="nav-item sub"
                 :class="{ on: tab === plTab(pl.id) }" @click="tab = plTab(pl.id)">
              <span class="nav-ic">└</span>{{ pl.name || pl.id }}
            </div>
          </template>
        </template>
      </nav>
      <div class="settings-main">
        <ConnectionsPanel :class="{ on: tab === 'connections' }" ref="connectionsRef" />
        <AgentPanel :class="{ on: tab === 'agent' }" ref="agentRef" />
        <PluginsPanel :class="{ on: tab === 'plugins' }" ref="pluginsRef" />
        <SecretsPanel :class="{ on: tab === 'secrets' }" />
        <AppearancePanel :class="{ on: tab === 'appearance' }" />
        <UsagePanel :class="{ on: tab === 'usage' }" />
        <PluginSettingsTab v-for="pl in pluginTabs" :key="pl.id" :plugin="pl"
                           :class="{ on: tab === plTab(pl.id) }" :ref="(el) => setPlRef(pl.id, el)" />
        <div class="settings-bar">
          <span class="grow"></span>
          <button v-if="saveable" class="btn save" :disabled="saving" @click="onSave">{{ saveLabel }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from "vue";
import { client } from "../../protocol/SplaClient";
import { projectEnvelope } from "../../state/project";
import type { PluginDto } from "../../protocol/types";
import ConnectionsPanel from "./ConnectionsPanel.vue";
import AgentPanel from "./AgentPanel.vue";
import PluginsPanel from "./PluginsPanel.vue";
import PluginSettingsTab from "./PluginSettingsTab.vue";
import SecretsPanel from "./SecretsPanel.vue";
import AppearancePanel from "./AppearancePanel.vue";
import UsagePanel from "./UsagePanel.vue";

const TABS = [
  { id: "connections", label: "Connections", icon: "⇄" },
  { id: "agent", label: "Agent", icon: "◎" },
  { id: "plugins", label: "Plugins", icon: "⬡" },
  { id: "secrets", label: "Secrets", icon: "🔑" },
  { id: "appearance", label: "Appearance", icon: "◈" },
  { id: "usage", label: "Usage", icon: "Σ" }
] as const;

// Tab id is either a fixed TABS id or "plugin:<id>" for a plugin's second-level tab.
const tab = ref<string>(new URLSearchParams(location.search).get("tab") || "connections");

function plTab(id: string) { return `plugin:${id}`; }

/** Enabled plugins that ship their own settings module — each gets a second-level tab. */
const pluginTabs = ref<PluginDto[]>([]);
const offPlugins = client.on("plugins.result", p => {
  pluginTabs.value = (p.plugins || []).filter(pl => pl.enabled !== false && pl.webSettingsUrl);
});
onUnmounted(offPlugins);

// If the selected plugin tab disappears (plugin disabled), fall back to the Plugins list.
watch(pluginTabs, tabs => {
  if (tab.value.startsWith("plugin:") && !tabs.some(pl => plTab(pl.id) === tab.value))
    tab.value = "plugins";
});

const connectionsRef = ref<InstanceType<typeof ConnectionsPanel>>();
const agentRef = ref<InstanceType<typeof AgentPanel>>();
const pluginsRef = ref<InstanceType<typeof PluginsPanel>>();
const plRefs = new Map<string, InstanceType<typeof PluginSettingsTab>>();
function setPlRef(id: string, el: unknown) {
  if (el) plRefs.set(id, el as InstanceType<typeof PluginSettingsTab>);
  else plRefs.delete(id);
}

const saving = ref(false);
const saveLabel = ref("Save");
const saveable = computed(() =>
  tab.value.startsWith("plugin:") || !["secrets", "appearance", "usage"].includes(tab.value));

async function onSave() {
  const panels: Record<string, { save: () => Promise<void> } | undefined> =
    { connections: connectionsRef.value, agent: agentRef.value, plugins: pluginsRef.value };
  const panel = tab.value.startsWith("plugin:")
    ? plRefs.get(tab.value.slice("plugin:".length))
    : panels[tab.value];
  if (!panel) return;
  saving.value = true;
  try {
    await panel.save();
    saveLabel.value = "Saved ✓";
  } catch (e) {
    saveLabel.value = "Save failed";
    console.error("settings save failed", e);
  } finally {
    saving.value = false;
    setTimeout(() => { saveLabel.value = "Save"; }, 1200);
  }
}

function fetchAll() {
  client.send("connections.get", undefined, projectEnvelope());
  client.send("agent.get", undefined, projectEnvelope());
  client.send("plugins.get", undefined, projectEnvelope());
}
client.on("welcome", fetchAll);
fetchAll();
</script>

<style scoped>
.nav-item.sub { padding-left: 22px; font-size: var(--fs-sm); }
.nav-item.sub .nav-ic { color: var(--muted); }
</style>
