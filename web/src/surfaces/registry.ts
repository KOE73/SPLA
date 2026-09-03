import type { Component } from "vue";
import Settings from "./Settings/Settings.vue";
import ChatList from "./ChatList.vue";
import StatusBar from "./StatusBar.vue";
import Filters from "./Filters.vue";
import Composer from "./Composer.vue";
import Debug from "./Debug.vue";
import Wire from "./Wire.vue";
import ChatLog from "./ChatLog.vue";
import ChatSurface from "./ChatSurface.vue";
import WorkspaceShell from "./Workspace/WorkspaceShell.vue";
import Terminal from "./Terminal.vue";
import TaskPanel from "./TaskPanel.vue";
import Hub from "./Hub.vue";
import SessionsPanel from "./SessionsPanel.vue";

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
  // A tear-off window onto ONE chat — "open in a separate window" (PLAN_20260902 wave 7) is a filter
  // on this same project connection, never a second agent instance (a window is "a view onto a
  // project, holds nothing" — RegistryProtocol.cs's own words for ParticipantKind.Window). main.ts
  // reads ?chat=<id> and opens it on THIS window's own connection, independent of whatever chat any
  // other window has open.
  chatSurface: ChatSurface,
  workspace: WorkspaceShell,
  terminal: Terminal,
  taskPanel: TaskPanel,
  // Served by the registry hub rather than an agent, and speaks the registry protocol instead of the
  // chat one — see main.ts, which skips the chat socket entirely for this surface.
  hub: Hub,
  sessions: SessionsPanel,
};
