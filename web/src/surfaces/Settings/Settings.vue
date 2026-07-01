<template>
  <!-- Two nested elements on purpose: .settings-surface gives the solo window a full-height
       flex-column frame; .settings-shell inside it is the actual flex-row two-pane layout
       (nav | main). They must stay on separate elements — merging them collapses the row
       layout because .settings-surface's flex-direction:column wins the cascade. -->
  <div class="settings-surface">
    <div class="settings-shell">
      <nav class="settings-nav">
        <div class="nav-section">Settings</div>
        <div v-for="t in TABS" :key="t.id" class="nav-item" :class="{ on: tab === t.id }" @click="tab = t.id">
          <span class="nav-ic">{{ t.icon }}</span>{{ t.label }}
        </div>
      </nav>
      <div class="settings-main">
        <ConnectionsPanel :class="{ on: tab === 'connections' }" ref="connectionsRef" />
        <AgentPanel :class="{ on: tab === 'agent' }" ref="agentRef" />
        <PluginsPanel :class="{ on: tab === 'plugins' }" ref="pluginsRef" />
        <AppearancePanel :class="{ on: tab === 'appearance' }" />
        <UsagePanel :class="{ on: tab === 'usage' }" />
        <div class="settings-bar">
          <span class="grow"></span>
          <button v-if="tab !== 'appearance' && tab !== 'usage'" class="btn save" :disabled="saving" @click="onSave">{{ saveLabel }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { client } from "../../protocol/SplaClient";
import ConnectionsPanel from "./ConnectionsPanel.vue";
import AgentPanel from "./AgentPanel.vue";
import PluginsPanel from "./PluginsPanel.vue";
import AppearancePanel from "./AppearancePanel.vue";
import UsagePanel from "./UsagePanel.vue";

const TABS = [
  { id: "connections", label: "Connections", icon: "⇄" },
  { id: "agent", label: "Agent", icon: "◎" },
  { id: "plugins", label: "Plugins", icon: "⬡" },
  { id: "appearance", label: "Appearance", icon: "◈" },
  { id: "usage", label: "Usage", icon: "Σ" }
] as const;

const tab = ref<typeof TABS[number]["id"]>(
  (new URLSearchParams(location.search).get("tab") as typeof TABS[number]["id"]) || "connections"
);

const connectionsRef = ref<InstanceType<typeof ConnectionsPanel>>();
const agentRef = ref<InstanceType<typeof AgentPanel>>();
const pluginsRef = ref<InstanceType<typeof PluginsPanel>>();

const saving = ref(false);
const saveLabel = ref("Save");

async function onSave() {
  const panels: Partial<Record<typeof TABS[number]["id"], { save: () => Promise<void> } | undefined>> =
    { connections: connectionsRef.value, agent: agentRef.value, plugins: pluginsRef.value };
  const panel = panels[tab.value];
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
  client.send("connections.get");
  client.send("agent.get");
  client.send("plugins.get");
}
client.on("welcome", fetchAll);
fetchAll();
</script>
