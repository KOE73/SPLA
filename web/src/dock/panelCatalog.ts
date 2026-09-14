import type { VueComponent } from "dockview-vue";
import type { Component } from "vue";
import ChatSurface from "../surfaces/ChatSurface.vue";
import WorkspaceShell from "../surfaces/Workspace/WorkspaceShell.vue";
import Terminal from "../surfaces/Terminal.vue";
import Debug from "../surfaces/Debug.vue";
import Wire from "../surfaces/Wire.vue";
import SessionsPanel from "../surfaces/SessionsPanel.vue";
import PluginPanel from "./PluginPanel.vue";
import DockTab from "./DockTab.vue";
import { builtInKinds, panelCatalog, pluginPanelComponent } from "./panelRegistry";

// Navigation is NOT a dock panel — it's the fixed left column (see AppShell). Only chat and the tool
// surfaces live inside dockview, on the right.
//
// This file is the WIRING half: which Vue surface each built-in kind renders. What panels exist, and
// how a plugin adds one, lives in panelRegistry.ts — which imports no component, so it says nothing
// about any particular panel and can be tested without a DOM.
export * from "./panelRegistry";

// dockview-vue's public VueComponent constructor type is narrower than Vue's SFC DefineComponent
// type even though the runtime accepts SFCs directly. Keep the compatibility cast in one place.
const dockComponent = (component: Component) => component as unknown as VueComponent;

const builtInComponents: Record<(typeof builtInKinds)[number], VueComponent> = {
  chat: dockComponent(ChatSurface),
  workspace: dockComponent(WorkspaceShell),
  ssh: dockComponent(Terminal),
  debug: dockComponent(Debug),
  wire: dockComponent(Wire),
  sessions: dockComponent(SessionsPanel),
};

for (const kind of builtInKinds) panelCatalog[kind].component = builtInComponents[kind];

export const dockComponents: Record<string, VueComponent> = {
  ...builtInComponents,
  // One entry for every plugin panel there will ever be — registered up front so it is present
  // before any plugins.result, and so dockview can restore a saved plugin panel from localStorage.
  [pluginPanelComponent]: dockComponent(PluginPanel),
};

export const dockTabComponents: Record<string, VueComponent> = { splaTab: dockComponent(DockTab) };
