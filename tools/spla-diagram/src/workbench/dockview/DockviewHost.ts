import { createDockview, type DockviewApi } from "dockview-core";
import { PanelService } from "./PanelService.js";
import { WorkspaceLayoutService } from "./WorkspaceLayoutService.js";
import type { DiagramEditor } from "../../editor/DiagramEditor.js";
import {
  CanvasPanel,
  PropertiesPanel,
  RelationsPanel,
  FiltersPanel,
  StylesPanel,
  CatalogPanel,
  BasePanel,
} from "../panels/index.js";

export class DockviewHost {
  readonly dockview: DockviewApi;
  readonly panelService: PanelService;
  readonly layoutService: WorkspaceLayoutService;

  constructor(
    container: HTMLElement,
    private readonly editor: DiagramEditor,
  ) {
    this.panelService = new PanelService();
    this.layoutService = new WorkspaceLayoutService();

    // 1. Initialize Fullscreen Dockview Host
    this.dockview = createDockview(container, {
      createComponent: (options) => {
        return this.panelService.createRenderer(options.name);
      },
    });

    // 2. Connect PanelService
    this.panelService.init({ dockview: this.dockview, container });

    // 3. Register all Panel Descriptors
    this.registerPanels();

    // 4. Connect Layout Service
    this.layoutService.init(
      { dockview: this.dockview, container },
      () => this.setupDefaultLayout(),
    );

    const restored = this.layoutService.loadLayout();
    if (!restored) {
      this.setupDefaultLayout();
    }

    // Guard: Ensure diagram is always present
    if (!this.dockview.getPanel("diagram")) {
      this.setupDefaultLayout();
    }

    // 5. Persist layout changes
    this.dockview.onDidLayoutChange(() => {
      this.layoutService.saveLayout();
    });

    // Re-dock diagram if accidentally closed
    this.dockview.onDidRemovePanel((e) => {
      if (e.id === "diagram") {
        setTimeout(() => {
          if (!this.dockview.getPanel("diagram")) {
            this.dockview.addPanel({
              id: "diagram",
              component: "diagram",
              title: "Диаграмма",
              minimumWidth: 200,
              minimumHeight: 150,
            });
          }
        }, 50);
      }
    });
  }

  private registerPanels(): void {
    this.panelService.register({
      id: "diagram",
      title: "Диаграмма",
      minWidth: 200,
      minHeight: 150,
      createRenderer: () => new CanvasPanel(this.editor),
    });

    this.panelService.register({
      id: "catalog",
      title: "Каталог схем",
      minWidth: 100,
      minHeight: 80,
      createRenderer: () => new CatalogPanel(this.editor, () => (this.editor as any).catalog ?? []),
    });

    this.panelService.register({
      id: "properties",
      title: "Свойства",
      minWidth: 100,
      minHeight: 80,
      createRenderer: () => new PropertiesPanel(this.editor),
    });

    this.panelService.register({
      id: "relations",
      title: "Связи",
      minWidth: 100,
      minHeight: 80,
      createRenderer: () => new RelationsPanel(this.editor),
    });

    this.panelService.register({
      id: "filters",
      title: "Фильтры",
      minWidth: 100,
      minHeight: 80,
      createRenderer: () => new FiltersPanel(this.editor),
    });

    this.panelService.register({
      id: "styles",
      title: "Стили",
      minWidth: 100,
      minHeight: 80,
      createRenderer: () => new StylesPanel(this.editor),
    });

    this.panelService.register({
      id: "base",
      title: "База сущностей",
      minWidth: 100,
      minHeight: 80,
      createRenderer: () => new BasePanel(this.editor),
    });
  }

  private setupDefaultLayout(): void {
    this.dockview.clear();

    // 1. Central Diagram Panel
    const diagram = this.dockview.addPanel({
      id: "diagram",
      component: "diagram",
      title: "Диаграмма",
      minimumWidth: 200,
      minimumHeight: 150,
    });
    diagram.group.locked = "no-drop-target";

    // 2. Left Sidebar: Catalog
    this.dockview.addPanel({
      id: "catalog",
      component: "catalog",
      title: "Каталог схем",
      position: {
        direction: "left",
        referencePanel: diagram,
      },
      initialWidth: 240,
      minimumWidth: 100,
      minimumHeight: 80,
    });

    // 3. Right Sidebar Group: Properties, Relations, Filters, Styles, Base
    const rightGroup = this.dockview.addPanel({
      id: "properties",
      component: "properties",
      title: "Свойства",
      position: {
        direction: "right",
        referencePanel: diagram,
      },
      initialWidth: 320,
      minimumWidth: 100,
      minimumHeight: 80,
    });

    this.dockview.addPanel({
      id: "relations",
      component: "relations",
      title: "Связи",
      position: {
        direction: "within",
        referencePanel: rightGroup,
      },
      minimumWidth: 100,
      minimumHeight: 80,
    });

    this.dockview.addPanel({
      id: "filters",
      component: "filters",
      title: "Фильтры",
      position: {
        direction: "within",
        referencePanel: rightGroup,
      },
      minimumWidth: 100,
      minimumHeight: 80,
    });

    this.dockview.addPanel({
      id: "styles",
      component: "styles",
      title: "Стили",
      position: {
        direction: "within",
        referencePanel: rightGroup,
      },
      minimumWidth: 100,
      minimumHeight: 80,
    });

    this.dockview.addPanel({
      id: "base",
      component: "base",
      title: "База сущностей",
      position: {
        direction: "within",
        referencePanel: rightGroup,
      },
      minimumWidth: 100,
      minimumHeight: 80,
    });

    rightGroup.api.setActive();
    diagram.api.setActive();
  }
}
