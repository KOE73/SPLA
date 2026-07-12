import { shallowReactive } from "vue";
import type { DockviewApi, IDockviewPanel, SerializedDockview } from "dockview-vue";
import { panelCatalog, type PanelKind } from "./panelCatalog";

const storageKey = "spla.dock.layout.v1";

export const dockState = shallowReactive({
  api: null as DockviewApi | null,
  activePanelId: null as string | null,
  maximized: false,
});

function addPanel(kind: PanelKind, referencePanel?: string, direction: "left" | "right" | "above" | "below" | "within" = "right") {
  const api = dockState.api;
  if (!api) return;
  const definition = panelCatalog[kind];
  const existing = api.getPanel(definition.id);
  if (existing) {
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

  if (!restored) {
    addPanel("navigation");
    addPanel("chat", "navigation", "right");
    queueMicrotask(() => api.getPanel("navigation")?.api.group.api.setSize({ width: panelCatalog.navigation.defaultWidth ?? 240 }));
  }
  if (!api.getPanel("navigation")) addPanel("navigation", api.panels[0]?.id, "left");
  if (!api.getPanel("chat")) addPanel("chat", api.panels[0]?.id, "right");

  dockState.activePanelId = api.activePanel?.id ?? "chat";
  api.onDidActivePanelChange(event => { dockState.activePanelId = event.panel?.id ?? null; });
  api.onDidMaximizedGroupChange(event => { dockState.maximized = event.isMaximized; });
  api.onDidLayoutChange(() => localStorage.setItem(storageKey, JSON.stringify(api.toJSON())));
  api.onDidRemovePanel(panel => {
    if (panelCatalog[panel.id as PanelKind]?.protected)
      queueMicrotask(() => addPanel(panel.id as PanelKind));
  });
}

export function openPanel(kind: PanelKind) {
  const reference = dockState.api?.getPanel("chat")?.id ?? dockState.api?.activePanel?.id;
  addPanel(kind, reference, kind === "navigation" ? "left" : "right");
}

export function toggleMaximize() {
  const api = dockState.api;
  if (!api) return;
  if (api.hasMaximizedGroup()) api.exitMaximizedGroup();
  else api.activePanel?.api.maximize();
}

export async function popoutActivePanel() {
  const api = dockState.api;
  if (api?.activePanel) await api.addPopoutGroup(api.activePanel);
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
  addPanel("navigation");
  addPanel("chat", "navigation", "right");
}

export function isProtected(panel?: IDockviewPanel) {
  return !!panel && !!panelCatalog[panel.id as PanelKind]?.protected;
}
