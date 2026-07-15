import type { VueComponent } from "dockview-vue";
import type { Component } from "vue";
import ChatSurface from "../surfaces/ChatSurface.vue";
import WorkspaceShell from "../surfaces/Workspace/WorkspaceShell.vue";
import Terminal from "../surfaces/Terminal.vue";
import Debug from "../surfaces/Debug.vue";
import Wire from "../surfaces/Wire.vue";
import BrowserScreencast from "../surfaces/BrowserScreencast.vue";
import DockTab from "./DockTab.vue";

// Navigation is NOT a dock panel — it's the fixed left column (see AppShell). Only chat and the tool
// surfaces live inside dockview, on the right.
export type PanelKind = "chat" | "workspace" | "ssh" | "browserScreencast" | "debug" | "wire";

// Which panels are tools (everything the top strip can open/hide). Chat is the always-present base.
export const toolKinds: PanelKind[] = ["workspace", "ssh", "browserScreencast", "debug", "wire"];

export interface PanelDefinition {
  id: string;
  kind: PanelKind;
  title: string;
  icon: string;
  component: VueComponent;
  singleton: boolean;
  protected: boolean;
  defaultWidth?: number;
  minimumWidth?: number;
  maximumWidth?: number;
}

// dockview-vue's public VueComponent constructor type is narrower than Vue's SFC DefineComponent
// type even though the runtime accepts SFCs directly. Keep the compatibility cast in one place.
const dockComponent = (component: Component) => component as unknown as VueComponent;

export const panelCatalog: Record<PanelKind, PanelDefinition> = {
  chat: { id: "chat", kind: "chat", title: "Chat", icon: "💬", component: dockComponent(ChatSurface), singleton: true, protected: true, minimumWidth: 320 },
  workspace: { id: "workspace", kind: "workspace", title: "Workspace", icon: "◫", component: dockComponent(WorkspaceShell), singleton: true, protected: false },
  // NOT a singleton: each SSH terminal is its own panel (id "ssh:<host>:<n>", host in params) —
  // the operator routinely holds several sessions to the same or different hosts.
  ssh: { id: "ssh", kind: "ssh", title: "SSH", icon: "⌨", component: dockComponent(Terminal), singleton: false, protected: false, defaultWidth: 480 },
  browserScreencast: { id: "browserScreencast", kind: "browserScreencast", title: "Browser Lab", icon: "🌐", component: dockComponent(BrowserScreencast), singleton: true, protected: false, defaultWidth: 640 },
  debug: { id: "debug", kind: "debug", title: "Debug", icon: "🧠", component: dockComponent(Debug), singleton: true, protected: false, defaultWidth: 420 },
  wire: { id: "wire", kind: "wire", title: "Wire", icon: "🔌", component: dockComponent(Wire), singleton: true, protected: false, defaultWidth: 420 },
};

export const dockComponents = Object.fromEntries(
  Object.values(panelCatalog).map(panel => [panel.kind, panel.component]),
) as Record<string, VueComponent>;

export const dockTabComponents: Record<string, VueComponent> = { splaTab: dockComponent(DockTab) };
