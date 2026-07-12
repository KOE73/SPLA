<template>
  <div class="dock-shell">
    <DockToolbar />
    <DockviewVue class="dockview-theme-spla" :components="dockComponents" :tab-components="dockTabComponents" @ready="event => initializeDock(event.api)" />
  </div>
</template>

<script setup lang="ts">
import { DockviewVue } from "dockview-vue";
import "dockview-vue/dist/styles/dockview.css";
import DockToolbar from "./DockToolbar.vue";
import { initializeDock } from "./dockController";
import { dockComponents, dockTabComponents } from "./panelCatalog";
</script>

<style>
.dock-shell { height: 100vh; min-width: 0; min-height: 0; display: flex; flex-direction: column; background: var(--bg); }
.dock-shell > .dockview-theme-spla { flex: 1; min-height: 0; }
.dockview-theme-spla,
.dockview-theme-spla > .dv-shell { width: 100%; height: 100%; }
.dockview-theme-spla {
  --dv-background-color: var(--bg);
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
  --dv-paneview-header-border-color: var(--border);
  --dv-drag-over-background-color: color-mix(in srgb, var(--accent) 20%, transparent);
  --dv-icon-hover-background-color: var(--accent-soft);
  --dv-floating-box-shadow: 0 4px 18px color-mix(in srgb, #000 35%, transparent);
}
.dockview-theme-spla .dv-tabs-and-actions-container { height: 27px; font-size: var(--fs-xs); }
.dockview-theme-spla .dv-content-container { min-width: 0; min-height: 0; }
</style>
