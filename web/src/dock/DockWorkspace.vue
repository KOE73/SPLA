<template>
  <div class="dock-shell">
    <DockToolbar />
    <DockviewVue class="dockview-theme-spla" :components="dockComponents" :tab-components="dockTabComponents" @ready="ready" />
  </div>
</template>

<script setup lang="ts">
import { onUnmounted } from "vue";
import { DockviewVue } from "dockview-vue";
import "dockview-vue/dist/styles/dockview.css";
import DockToolbar from "./DockToolbar.vue";
import { initializeDock, openPanel, openSshTerminal } from "./dockController";
import { dockComponents, dockTabComponents } from "./panelCatalog";
import { client } from "../protocol/SplaClient";
import { projectEnvelope } from "../state/project";

function ready(event: { api: Parameters<typeof initializeDock>[0] }) {
  initializeDock(event.api);
  const requestedPanel = new URLSearchParams(location.search).get("panel");
  if (requestedPanel === "browserScreencast") openPanel("browserScreencast");
}

// "Watch the agent live": when the agent opens an SSH session, auto-attach a terminal panel so the
// operator SEES the commands and output appear — not silently somewhere. We only auto-open each
// session once (tracked by id); if the operator closes the panel it does not pop back.
const autoOpened = new Set<string>();
const offChanged = client.on("ssh.sessions.changed", () => {
  client.send("ssh.sessions.get", undefined, projectEnvelope());
});
const offSessions = client.on("ssh.sessions.result", p => {
  for (const s of p.sessions) {
    if (s.openedBy !== "agent" || autoOpened.has(s.id)) continue;
    autoOpened.add(s.id);
    openSshTerminal({ session: s.id });
  }
});
onUnmounted(() => { offChanged(); offSessions(); });
// Catch a session the agent opened before this mounted, and re-sync after a reconnect.
client.send("ssh.sessions.get", undefined, projectEnvelope());
const offConn = client.on("conn", c => { if (c.on) client.send("ssh.sessions.get", undefined, projectEnvelope()); });
onUnmounted(offConn);
</script>

<style>
.dock-shell { height: 100%; min-width: 0; min-height: 0; display: flex; flex-direction: column; background: var(--bg); }
.dock-shell > .dockview-theme-spla { flex: 1; min-height: 0; }
.dockview-theme-spla,
.dockview-theme-spla > .dv-shell { width: 100%; height: 100%; }
.dockview-theme-spla,
.dockview-theme-spla .dockview-theme-abyss {
  --dv-background-color: var(--bg);
  --dv-group-view-background-color: var(--bg);
  --dv-paneview-active-outline-color: var(--accent);
  --dv-tabs-and-actions-container-background-color: var(--panel);
  --dv-activegroup-visiblepanel-tab-background-color: var(--elevated);
  --dv-activegroup-hiddenpanel-tab-background-color: var(--panel);
  --dv-inactivegroup-visiblepanel-tab-background-color: var(--panel);
  --dv-inactivegroup-hiddenpanel-tab-background-color: var(--panel);
  --dv-tab-divider-color: var(--border);
  --dv-activegroup-visiblepanel-tab-color: var(--text);
  --dv-activegroup-hiddenpanel-tab-color: var(--muted);
  --dv-inactivegroup-visiblepanel-tab-color: var(--muted);
  --dv-inactivegroup-hiddenpanel-tab-color: var(--muted);
  --dv-separator-border: var(--border);
  --dv-sash-color: var(--border);
  --dv-active-sash-color: var(--accent);
  --dv-scrollbar-background-color: var(--muted);
  --dv-context-menu-background-color: var(--panel);
  --dv-context-menu-color: var(--text);
  --dv-paneview-header-border-color: var(--border);
  --dv-drag-over-background-color: color-mix(in srgb, var(--accent) 20%, transparent);
  --dv-icon-hover-background-color: var(--accent-soft);
  --dv-floating-box-shadow: 0 4px 18px color-mix(in srgb, #000 35%, transparent);
}
.dockview-theme-spla .dv-tabs-and-actions-container { height: 27px; font-size: var(--fs-xs); }
/* Chat-only group: no tab bar at all (dockController tags the group when chat sits alone in it). */
.dockview-theme-spla .dv-no-header > .dv-tabs-and-actions-container,
.dockview-theme-spla .dv-no-header.dv-groupview > .dv-tabs-and-actions-container { display: none; }
.dockview-theme-spla .dv-content-container { min-width: 0; min-height: 0; }
</style>
