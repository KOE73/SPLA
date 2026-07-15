import { shallowReactive } from "vue";
import type { DockviewApi, IDockviewPanel, SerializedDockview } from "dockview-vue";
import { panelCatalog, type PanelKind } from "./panelCatalog";
import { store } from "../state/store";

// v2: navigation left the dock (it's the fixed left column now), so old v1 layouts are incompatible.
const storageKey = "spla.dock.layout.v2";

export const dockState = shallowReactive({
  api: null as DockviewApi | null,
  activePanelId: null as string | null,
  maximized: false,
  chatOnly: false,   // tools hidden — chat maximized across the whole right area
});

function addPanel(kind: PanelKind, referencePanel?: string, direction: "left" | "right" | "above" | "below" | "within" = "right") {
  const api = dockState.api;
  if (!api) return;
  const definition = panelCatalog[kind];
  const existing = api.getPanel(definition.id);
  if (existing) {
    existing.api.group.api.setVisible(true);
    existing.api.setActive();
    return;
  }

  api.addPanel({
    id: definition.id,
    component: definition.kind,
    title: `${definition.icon} ${definition.title}`,
    tabComponent: "splaTab",
    initialWidth: definition.defaultWidth,
    minimumWidth: definition.minimumWidth ?? 220,
    maximumWidth: definition.maximumWidth,
    renderer: "always",
    position: referencePanel ? { referencePanel, direction } : undefined,
  });
}

export function initializeDock(api: DockviewApi) {
  dockState.api = api;
  let restored = false;
  const saved = localStorage.getItem(storageKey);
  if (saved) {
    try {
      api.fromJSON(JSON.parse(saved) as SerializedDockview);
      restored = true;
    } catch {
      localStorage.removeItem(storageKey);
    }
  }

  if (!restored) addPanel("chat");
  if (!api.getPanel("chat")) addPanel("chat"); // chat is the always-present base

  dockState.activePanelId = api.activePanel?.id ?? "chat";
  api.onDidActivePanelChange(event => { dockState.activePanelId = event.panel?.id ?? null; });
  api.onDidMaximizedGroupChange(event => {
    dockState.maximized = event.isMaximized;
    if (!event.isMaximized) dockState.chatOnly = false;
  });
  api.onDidLayoutChange(() => {
    localStorage.setItem(storageKey, JSON.stringify(api.toJSON()));
    syncChatHeader();
  });
  api.onDidRemovePanel(panel => {
    if (panelCatalog[panel.id as PanelKind]?.protected)
      queueMicrotask(() => addPanel(panel.id as PanelKind));
  });
  syncChatHeader();
}

// The chat panel is the always-present base — a tab bar over it just says "Chat", which is noise.
// Hide the header of the group that holds chat WHEN chat is alone in it (a group with tabs beside
// chat still needs its header to switch between them). dockview has no per-group hide-header API, so
// we tag the group's DOM element and let CSS collapse the header (see app.css .dv-no-header).
function syncChatHeader() {
  const api = dockState.api;
  const chat = api?.getPanel("chat");
  const group = chat?.api.group;
  if (!group) return;
  const alone = group.panels.length === 1;
  for (const el of api!.groups.map(g => g.element))
    el.classList.toggle("dv-no-header", el === group.element && alone);
}

/// Open (or reveal) a tool. All tools share ONE group to the right of chat, so opening a second tool
/// adds it as a TAB in that group rather than splitting off a new column.
export function openPanel(kind: PanelKind) {
  if (dockState.chatOnly) showTools();
  const existingTool = firstTool();
  if (existingTool) addPanel(kind, existingTool.id, "within");
  else addPanel(kind, dockState.api?.getPanel("chat")?.id, "right");
}

/// Open an SSH terminal panel. Either ATTACH to an existing session (`session` = host#N — watch the
/// agent / reattach) or open a fresh session on a host (undefined = plugin's default host). Unlike
/// the singleton tools, every call adds a new panel — sessions are the unit, terminals are views,
/// and the operator may hold several. host/session ride in the panel's params (dockview serialises
/// them, so a restored layout reconnects to the right target). Attaching to an already-open session
/// twice would just add a second viewer, so we reveal an existing panel for that session instead.
let sshSeq = 0;
export function openSshTerminal(opts: { host?: string; session?: string } = {}) {
  const api = dockState.api;
  if (!api) return;
  if (dockState.chatOnly) showTools();

  if (opts.session) {
    const existing = api.panels.find(p =>
      (p.params as { session?: string } | undefined)?.session === opts.session);
    if (existing) { existing.api.setActive(); return; }
  }

  const label = opts.session ?? opts.host ?? "SSH";
  const id = `ssh:${label}:${Date.now().toString(36)}-${sshSeq++}`;
  const ref = firstTool() ?? api.getPanel("chat");
  api.addPanel({
    id,
    component: "ssh",
    title: `⌨ ${label}`,
    tabComponent: "splaTab",
    params: { host: opts.host, session: opts.session },
    initialWidth: panelCatalog.ssh.defaultWidth,
    minimumWidth: 220,
    renderer: "always",
    position: ref ? { referencePanel: ref.id, direction: firstTool() ? "within" : "right" } : undefined,
  });
}

/// Give the active tool panel the whole right area (left column is untouched — it lives outside the dock).
export function toggleMaximize() {
  const api = dockState.api;
  if (!api) return;
  if (api.hasMaximizedGroup()) { api.exitMaximizedGroup(); return; }
  const target = api.activePanel && api.activePanel.id !== "chat" ? api.activePanel : firstTool();
  target?.api.maximize();
}

/// Hide the tools (maximize chat so it fills the right area) or bring them back.
export function toggleChatOnly() {
  const api = dockState.api;
  if (!api) return;
  if (dockState.chatOnly || api.hasMaximizedGroup()) { showTools(); return; }
  if (!firstTool()) return; // nothing to hide
  api.getPanel("chat")?.api.maximize();
  dockState.chatOnly = true;
}

function showTools() {
  const api = dockState.api;
  if (api?.hasMaximizedGroup()) api.exitMaximizedGroup();
  dockState.chatOnly = false;
}

function firstTool(): IDockviewPanel | undefined {
  return dockState.api?.panels.find(p => p.id !== "chat");
}

export function hasTools(): boolean {
  return !!firstTool();
}

// Panel kinds that have a standalone solo surface (?surface=<name>) for native tear-off windows.
const soloSurfaceFor: Record<string, string> = { workspace: "workspace", debug: "debug", wire: "wire" };

export async function popoutActivePanel() {
  const api = dockState.api;
  const panel = api?.activePanel;
  if (!api || !panel) return;

  // Embedded in Avalonia: WebView2 can't host dockview's window.open popout windows — ask the
  // native shell (WebViewBridge) for a real OS window running the same surface solo. The dock's
  // copy is removed: for SSH the detached window opens its own pty, keeping both would double the
  // sessions; for singleton tools it would just duplicate the view.
  const webview = window.chrome?.webview;
  if (webview) {
    const q = new URLSearchParams();
    if (store.currentProjectId) q.set("project", store.currentProjectId);

    let surface: string | undefined;
    let title: string = panel.title ?? panel.id;
    if (panel.id === "ssh" || panel.id.startsWith("ssh:")) {
      surface = "terminal";
      // Prefer the live session id (Terminal.vue writes it into params on terminal.opened) so the
      // detached window ATTACHES to the same session rather than opening a second one.
      const params = (panel as unknown as { params?: { host?: string; session?: string } }).params;
      if (params?.session) { q.set("session", params.session); title = "SSH: " + params.session; }
      else if (params?.host) { q.set("host", params.host); title = "SSH: " + params.host; }
    } else {
      surface = soloSurfaceFor[panel.id];
    }
    if (!surface) return; // chat and unmapped panels don't detach

    webview.postMessage({ kind: "openWindow", surface, query: q.toString(), title });
    api.removePanel(panel);
    return;
  }

  await api.addPopoutGroup(panel);
}

export function closeActivePanel() {
  const api = dockState.api;
  const panel = api?.activePanel;
  if (!api || !panel || panelCatalog[panel.id as PanelKind]?.protected) return;
  api.removePanel(panel);
}

export function resetDock() {
  const api = dockState.api;
  if (!api) return;
  localStorage.removeItem(storageKey);
  api.clear();
  dockState.chatOnly = false;
  addPanel("chat");
}

export function isProtected(panel?: IDockviewPanel) {
  return !!panel && !!panelCatalog[panel.id as PanelKind]?.protected;
}
