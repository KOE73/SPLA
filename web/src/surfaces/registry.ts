import type { Component } from "vue";
import Settings from "./Settings/Settings.vue";
import ChatList from "./ChatList.vue";
import StatusBar from "./StatusBar.vue";
import Filters from "./Filters.vue";
import Composer from "./Composer.vue";
import Debug from "./Debug.vue";
import Wire from "./Wire.vue";
import ChatLog from "./ChatLog.vue";
import WorkspaceShell from "./Workspace/WorkspaceShell.vue";

// name → Vue component. Populated incrementally as each surface is migrated (Phases 3-7).
export const surfaces: Record<string, Component> = {
  settings: Settings,
  chatList: ChatList,
  statusBar: StatusBar,
  filters: Filters,
  composer: Composer,
  debug: Debug,
  wire: Wire,
  chatLog: ChatLog,
  workspace: WorkspaceShell,
};
