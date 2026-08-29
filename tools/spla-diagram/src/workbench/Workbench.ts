import { el } from "../util/dom.js";
import { DiagramEditor, type DiagramEditorOptions } from "../editor/DiagramEditor.js";
import { CommandRegistry } from "./commands/CommandRegistry.js";
import { createBuiltinCommands } from "./commands/builtinCommands.js";
import type { CommandContext, SelectionService } from "./commands/types.js";
import { ShortcutManager } from "./commands/ShortcutManager.js";
import { Ribbon } from "./ribbon/Ribbon.js";
import { createDefaultRibbonSpec } from "./ribbon/RibbonModel.js";
import { DockviewHost } from "./dockview/DockviewHost.js";

/**
 * Workbench architecture root.
 *
 * Integrates:
 * - Ribbon (top command surface)
 * - Command Center (centralized actions)
 * - Fullscreen Dockview (multi-zone dockable panels around central diagram)
 * - Shortcut Manager (global hotkeys)
 * - Theme/Status/Persistence synchronization
 */
export class Workbench {
  readonly root: HTMLElement;
  readonly editor: DiagramEditor;
  readonly commands: CommandRegistry;
  readonly shortcuts: ShortcutManager;
  readonly ribbon: Ribbon;
  readonly dockviewHost: DockviewHost;

  constructor(
    private readonly hostElement: HTMLElement,
    options: DiagramEditorOptions,
  ) {
    // 1. Root Workbench Shell
    const root = el("div", {
      class: "workbench-shell",
      attrs: {
        style:
          "display: flex; flex-direction: column; width: 100%; height: 100%; overflow: hidden; background: var(--bg); color: var(--text);",
      },
    });
    this.root = root;

    // 2. Hidden host for DiagramEditor backend logic
    const editorMount = el("div", { attrs: { style: "display: none;" } });
    root.appendChild(editorMount);
    this.editor = new DiagramEditor(editorMount, options);

    // 3. Ribbon Container (top)
    const ribbonContainer = el("div", {
      class: "workbench-ribbon-container",
      attrs: { style: "flex-shrink: 0; z-index: 50;" },
    });

    // 4. Fullscreen Dockview Host (main area under Ribbon)
    const dockviewContainer = el("div", {
      class: "workbench-dockview-host dockview-theme-dark",
      attrs: {
        style:
          "flex: 1; min-width: 0; min-height: 0; width: 100%; height: 100%; position: relative; overflow: hidden;",
      },
    });

    root.appendChild(ribbonContainer);
    root.appendChild(dockviewContainer);

    // 5. Initialize Command Center
    this.commands = new CommandRegistry();
    this.commands.registerAll(createBuiltinCommands());

    // 6. Initialize Fullscreen Dockview Host
    this.dockviewHost = new DockviewHost(dockviewContainer, this.editor);

    // 7. Connect Command Context Provider
    this.commands.setContextProvider(() => this.createCommandContext());

    // 8. Initialize Global Shortcut Manager
    this.shortcuts = new ShortcutManager(this.commands, () => this.createCommandContext());

    // 9. Initialize Ribbon
    const ribbonSpec = createDefaultRibbonSpec();
    this.ribbon = new Ribbon(
      ribbonSpec,
      this.commands,
      () => this.createCommandContext(),
    );
    ribbonContainer.appendChild(this.ribbon.element);

    // 10. Bind events
    this.bindEvents();

    // Auto layout canvas on window resize
    window.addEventListener("resize", () => {
      this.editor.canvas.render();
    });

    // 11. Mount to host
    this.hostElement.appendChild(root);
  }

  private bindEvents(): void {
    // Re-evaluate ribbon and commands on selection change
    this.editor.canvas.events.on("select", () => {
      this.commands.notifyStateChanged();
    });

    // Re-evaluate on model/history change
    this.editor.canvas.events.on("modelchange", () => {
      this.commands.notifyStateChanged();
    });

    // Re-evaluate on panel visibility change
    this.dockviewHost.panelService.onPanelStateChange(() => {
      this.commands.notifyStateChanged();
      this.editor.canvas.render();
    });
  }

  private createCommandContext(): CommandContext {
    const canvas = this.editor.canvas;
    const selection = canvas.selected;

    const selectionService: SelectionService = {
      current: selection,
      ids: canvas.selectedIds,
      hasSelection: selection !== null,
      select: (id) => canvas.select(id),
      clear: () => canvas.select(null),
    };

    return {
      editor: this.editor,
      canvas,
      document: canvas.model,
      styles: this.editor.styles,
      history: this.editor.history,
      selection: selectionService,
      panels: this.dockviewHost.panelService,
      workspace: this.dockviewHost.layoutService,
      io: this.editor,
      dataLang: this.editor.dataLang,
    };
  }
}
