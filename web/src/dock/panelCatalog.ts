import type { VueComponent } from "dockview-vue";
import type { Component } from "vue";
import NavigationSurface from "../surfaces/NavigationSurface.vue";
import ChatSurface from "../surfaces/ChatSurface.vue";
import WorkspaceShell from "../surfaces/Workspace/WorkspaceShell.vue";
import Terminal from "../surfaces/Terminal.vue";
import Debug from "../surfaces/Debug.vue";
import Wire from "../surfaces/Wire.vue";
import BrowserScreencast from "../surfaces/BrowserScreencast.vue";
import DockTab from "./DockTab.vue";

export type PanelKind = "navigation" | "chat" | "workspace" | "ssh" | "browserScreencast" | "debug" | "wire";

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
  navigation: { id: "navigation", kind: "navigation", title: "Navigation", icon: "☰", component: dockComponent(NavigationSurface), singleton: true, protected: true, defaultWidth: 240, minimumWidth: 170, maximumWidth: 480 },
  chat: { id: "chat", kind: "chat", title: "Chat", icon: "💬", component: dockComponent(ChatSurface), singleton: true, protected: true, minimumWidth: 320 },
  workspace: { id: "workspace", kind: "workspace", title: "Workspace", icon: "◫", component: dockComponent(WorkspaceShell), singleton: true, protected: false },
  ssh: { id: "ssh", kind: "ssh", title: "SSH", icon: "⌨", component: dockComponent(Terminal), singleton: true, protected: false, defaultWidth: 480 },
  browserScreencast: { id: "browserScreencast", kind: "browserScreencast", title: "Browser Lab", icon: "🌐", component: dockComponent(BrowserScreencast), singleton: true, protected: false, defaultWidth: 640 },
  debug: { id: "debug", kind: "debug", title: "Debug", icon: "🧠", component: dockComponent(Debug), singleton: true, protected: false, defaultWidth: 420 },
  wire: { id: "wire", kind: "wire", title: "Wire", icon: "🔌", component: dockComponent(Wire), singleton: true, protected: false, defaultWidth: 420 },
};

export const dockComponents = Object.fromEntries(
  Object.values(panelCatalog).map(panel => [panel.kind, panel.component]),
) as Record<string, VueComponent>;

export const dockTabComponents: Record<string, VueComponent> = { splaTab: dockComponent(DockTab) };
