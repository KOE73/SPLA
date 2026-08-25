import { DiagramCanvas, type Selection } from "../canvas/DiagramCanvas.js";
import {
  CenterPortAssigner,
  DiscretePortAssigner,
  UniformPortAssigner,
} from "../canvas/ports/assigners.js";
import { DiagramDocument } from "../model/document.js";
import { parseDocument, serializeDocument } from "../model/wire.js";
import type { WireDocument } from "../model/wire-types.js";
import type { DiagramElement } from "../model/types.js";
import { snap } from "../geometry/rect.js";
import { History } from "./History.js";
import { Inspector, type InspectorHost } from "./Inspector.js";
import { drawioFileName, exportDrawio } from "./io/drawio.js";
import { HttpModelStore, download, readJsonFile, type ModelStore } from "./io/transfer.js";
import { el, replaceChildren } from "../util/dom.js";

export interface CatalogEntry {
  readonly id: string;
  readonly file: string;
  readonly title: string;
  readonly subtitle?: string;
  readonly icon?: string;
  readonly theme?: string;
}

export interface DiagramEditorOptions {
  catalog?: readonly CatalogEntry[];
  store?: ModelStore;
}

/** How long a text field must be quiet before its edits become a history step. */
const FIELD_EDIT_QUIET_MS = 600;

type Slot =
  | "canvas" | "views" | "catalog" | "custom-catalog" | "custom-catalog-section"
  | "inspector-badge" | "inspector-body" | "title" | "dirty" | "zoom"
  | "json-modal" | "json-text" | "file-input" | "drop-hint" | "sidebar";

/**
 * The application: a canvas plus everything around it.
 *
 * Owns the things a reusable canvas must not know about — where models come
 * from, where they are saved, what undo means, what the toolbar looks like.
 * All of it talks to the canvas through its public API and its events.
 */
export class DiagramEditor implements InspectorHost {
  readonly canvas: DiagramCanvas;

  private readonly root: HTMLElement;
  private readonly slots = new Map<Slot, HTMLElement>();
  private readonly history = new History();
  private readonly inspector: Inspector;
  private readonly store: ModelStore;

  private catalog: readonly CatalogEntry[];
  private currentEntry: CatalogEntry | null = null;
  private dirty = false;

  /** State before the current burst of typing, held until the burst settles. */
  private fieldEditSnapshot: string | null = null;
  private fieldEditTimer: number | null = null;

  constructor(root: HTMLElement, options: DiagramEditorOptions = {}) {
    this.root = root;
    this.catalog = options.catalog ?? [];
    this.store = options.store ?? new HttpModelStore("./");

    for (const node of root.querySelectorAll<HTMLElement>("[data-slot]")) {
      const name = node.dataset.slot as Slot | undefined;
      if (name !== undefined) this.slots.set(name, node);
    }

    this.canvas = new DiagramCanvas(this.slot("canvas"));
    this.inspector = new Inspector(
      this.slot("inspector-badge"),
      this.slot("inspector-body"),
      this,
    );

    this.bindCanvas();
    this.bindActions();
    this.bindKeyboard();
    this.bindFiles();
    this.renderCatalog();

    const first = this.catalog[0];
    if (first !== undefined) void this.openCatalogEntry(first);
    else this.showCatalogEmptyState();
  }

  // ---------------------------------------------------------------- wiring

  private slot(name: Slot): HTMLElement {
    const node = this.slots.get(name);
    if (node === undefined) throw new Error(`Editor markup is missing [data-slot="${name}"]`);
    return node;
  }

  private bindCanvas(): void {
    this.canvas.events.on("select", (selection) => {
      // Moving on commits whatever was being typed, so the step belongs to the
      // element it was typed into.
      this.flushFieldEdit();
      this.inspector.render(selection);
      this.syncToolbar(selection);
    });

    this.canvas.events.on("gesturestart", () => {
      this.flushFieldEdit();
      this.history.begin(this.snapshot());
    });

    this.canvas.events.on("gestureend", () => {
      if (this.history.end(this.snapshot())) this.syncToolbar(this.canvas.selected);
    });

    this.canvas.events.on("modelchange", () => {
      this.markDirty();
      const element = this.canvas.selectedElement();
      if (element !== null) this.inspector.updateGeometry(element);
    });

    this.canvas.events.on("collapse", () => {
      this.inspector.render(this.canvas.selected);
    });

    this.canvas.events.on("viewport", (state) => {
      this.slot("zoom").textContent = `${Math.round(state.zoom * 100)}%`;
    });
  }

  private bindActions(): void {
    this.root.addEventListener("click", (e) => {
      const target = e.target;
      if (!(target instanceof Element)) return;
      const actionNode = target.closest<HTMLElement>("[data-action]");
      if (actionNode === null) return;
      const action = actionNode.dataset.action;
      if (action !== undefined) void this.runAction(action);
    });

    this.root.addEventListener("change", (e) => {
      const target = e.target;
      if (target instanceof HTMLInputElement) {
        const toggle = target.dataset.toggle;
        if (toggle !== undefined) this.applyToggle(toggle, target.checked);
        return;
      }
      if (target instanceof HTMLSelectElement && target.dataset.select === "ports") {
        this.applyPortAssigner(target.value);
      }
    });
  }

  private async runAction(action: string): Promise<void> {
    switch (action) {
      case "create-node": return this.createNode();
      case "create-zone": return this.createZone();
      case "delete": return this.deleteSelection();
      case "undo": return this.undo();
      case "redo": return this.redo();
      case "save": return this.save();
      case "export-drawio": return this.exportDrawio();
      case "fit": return this.canvas.fit();
      case "zoom-in": return this.canvas.zoomBy(1.2);
      case "zoom-out": return this.canvas.zoomBy(0.8);
      case "zoom-reset": return this.canvas.resetZoom();
      case "toggle-sidebar": return this.toggleSidebar();
      case "open-file": return this.slot("file-input").click();
      case "toggle-json": return this.toggleJsonModal();
      case "copy-json": return this.copyJson();
      case "apply-json": return this.applyJson();
      default: return;
    }
  }

  private applyToggle(name: string, on: boolean): void {
    switch (name) {
      case "grid":
        this.slot("canvas").classList.toggle("with-grid", on);
        return;
      case "snap":
        this.canvas.gridStep = on ? 10 : 0;
        return;
      case "container-drag":
        this.canvas.containerDrag = on;
        return;
      default:
        return;
    }
  }

  /**
   * Swap how edge ends are placed along a side.
   *
   * "center" is the default and reproduces the original renderer: every end
   * sits in the middle of the facing side, so several edges between the same
   * pair overlap. The others spread them out, ordering both ends by the same
   * key so the lines stay parallel instead of crossing.
   *
   * Nothing is stored in the model — placement is a pure function of it, which
   * is why this can be switched freely and why the JSON never learns about it.
   */
  private applyPortAssigner(id: string): void {
    switch (id) {
      case "uniform":
        this.canvas.setPortAssigner(new UniformPortAssigner());
        return;
      case "discrete":
        this.canvas.setPortAssigner(new DiscretePortAssigner());
        return;
      default:
        this.canvas.setPortAssigner(new CenterPortAssigner());
        return;
    }
  }

  private bindKeyboard(): void {
    window.addEventListener("keydown", (e) => {
      const target = e.target;
      if (target instanceof HTMLElement) {
        const tag = target.tagName;
        if (tag === "INPUT" || tag === "TEXTAREA" || target.isContentEditable) return;
      }

      const mod = e.ctrlKey || e.metaKey;
      const key = e.key.toLowerCase();

      if (mod && key === "z") {
        e.preventDefault();
        if (e.shiftKey) this.redo();
        else this.undo();
      } else if (mod && key === "y") {
        e.preventDefault();
        this.redo();
      } else if (e.key === "Delete" || e.key === "Backspace") {
        if (this.canvas.selected === null) return;
        e.preventDefault();
        this.deleteSelection();
      }
    });
  }

  private bindFiles(): void {
    const input = this.slot("file-input") as unknown as HTMLInputElement;
    input.addEventListener("change", () => {
      const file = input.files?.[0];
      if (file !== undefined) void this.loadFile(file);
      input.value = "";
    });

    const hint = this.slot("drop-hint");
    window.addEventListener("dragover", (e) => {
      e.preventDefault();
      hint.hidden = false;
    });
    window.addEventListener("dragleave", (e) => {
      if (e.relatedTarget === null) hint.hidden = true;
    });
    window.addEventListener("drop", (e) => {
      e.preventDefault();
      hint.hidden = true;
      const file = e.dataTransfer?.files[0];
      if (file === undefined) return;
      if (!file.name.endsWith(".json")) {
        this.notify("Перетащите файл с расширением .json");
        return;
      }
      void this.loadFile(file);
    });
  }

  // ----------------------------------------------------------- model loading

  private async openCatalogEntry(entry: CatalogEntry): Promise<void> {
    try {
      const wire = await this.store.load(entry.file);
      this.currentEntry = entry;
      this.loadWire(wire, entry.title);
      this.highlightCatalog(entry.id);
    } catch (err) {
      // A failed fetch is surfaced rather than papered over with a stale
      // embedded copy of the model, which is what the original did (D-11).
      this.notify(`Не удалось открыть «${entry.title}»: ${(err as Error).message}`);
    }
  }

  private async loadFile(file: File): Promise<void> {
    try {
      const wire = await readJsonFile(file);
      this.currentEntry = null;
      this.loadWire(wire, file.name);
      this.addCustomCatalogEntry(file.name, wire);
    } catch (err) {
      this.notify((err as Error).message);
    }
  }

  private loadWire(wire: WireDocument, title: string): void {
    // Anything half-typed belongs to the model being replaced, not the new one.
    if (this.fieldEditTimer !== null) window.clearTimeout(this.fieldEditTimer);
    this.fieldEditTimer = null;
    this.fieldEditSnapshot = null;

    const doc = parseDocument(wire);
    this.canvas.setModel(doc);
    this.history.reset(this.snapshot());
    this.dirty = false;
    this.slot("dirty").hidden = true;
    this.slot("title").textContent = title;
    this.renderViews(doc);
    this.syncToolbar(null);
  }

  private snapshot(): string {
    const doc = this.canvas.model;
    return doc === null ? "" : JSON.stringify(serializeDocument(doc));
  }

  private restore(snapshot: string): void {
    if (snapshot === "") return;
    const doc = parseDocument(JSON.parse(snapshot) as WireDocument);
    this.canvas.replaceModel(doc);
    this.markDirty();
    this.syncToolbar(this.canvas.selected);
  }

  // ---------------------------------------------------------------- catalog

  private renderCatalog(): void {
    const host = this.slot("catalog");
    replaceChildren(
      host,
      ...this.catalog.map((entry) =>
        el(
          "button",
          {
            class: "catalog-item",
            dataset: { catalogId: entry.id },
            on: { click: () => void this.openCatalogEntry(entry) },
          },
          [
            el("span", { class: `catalog-icon theme-${entry.theme ?? "blue"}`, text: entry.icon ?? "📄" }),
            el("span", { class: "catalog-text sidebar-label" }, [
              el("span", { class: "catalog-title", text: entry.title }),
              el("span", { class: "catalog-subtitle", text: entry.subtitle ?? "" }),
            ]),
          ],
        ),
      ),
    );
  }

  private showCatalogEmptyState(): void {
    replaceChildren(
      this.slot("catalog"),
      el("div", {
        class: "catalog-empty",
        text: "Каталог схем не загружен. Откройте JSON-файл вручную.",
      }),
    );
  }

  private addCustomCatalogEntry(name: string, wire: WireDocument): void {
    this.slot("custom-catalog-section").hidden = false;
    this.slot("custom-catalog").appendChild(
      el(
        "button",
        {
          class: "catalog-item",
          on: {
            click: () => {
              this.currentEntry = null;
              this.loadWire(wire, name);
            },
          },
        },
        [
          el("span", { class: "catalog-icon theme-blue", text: "📄" }),
          el("span", { class: "catalog-text sidebar-label" }, [
            el("span", { class: "catalog-title", text: name }),
            el("span", { class: "catalog-subtitle", text: "Пользовательский файл" }),
          ]),
        ],
      ),
    );
  }

  private highlightCatalog(id: string): void {
    for (const node of this.slot("catalog").querySelectorAll<HTMLElement>("[data-catalog-id]")) {
      node.classList.toggle("is-active", node.dataset.catalogId === id);
    }
  }

  private renderViews(doc: DiagramDocument): void {
    const host = this.slot("views");
    const views = doc.views.length > 0
      ? doc.views
      : [{ id: "all", name: "Все элементы", icon: "🏛", description: "", highlightZones: [], highlightNodes: [] }];

    replaceChildren(
      host,
      ...views.map((view) =>
        el("button", {
          class: `view-btn${view.id === this.canvas.activeView ? " is-active" : ""}`,
          title: view.description,
          text: `${view.icon} ${view.name}`,
          dataset: { viewId: view.id },
          on: {
            click: () => {
              this.canvas.setView(view.id);
              this.syncViews();
            },
          },
        }),
      ),
    );
    this.syncViews();
  }

  private syncViews(): void {
    for (const node of this.slot("views").querySelectorAll<HTMLElement>("[data-view-id]")) {
      node.classList.toggle("is-active", node.dataset.viewId === this.canvas.activeView);
    }
  }

  // ---------------------------------------------------------------- editing

  private createNode(): void {
    const doc = this.canvas.model;
    if (doc === null) return;
    const at = this.canvas.viewCenter();
    const x = snap(at.x, 10);
    const y = snap(at.y, 10);

    const node: DiagramElement = {
      id: `node_${Date.now().toString(36)}`,
      kind: "node",
      type: "concept",
      label: "Новый блок",
      tags: [],
      metadata: { type: "Concept / Блок", description: "Пользовательский блок архитектуры." },
      x, y, width: 190, height: 60,
      parent: null,
      children: [],
      wireOrder: Number.POSITIVE_INFINITY,
    };

    const target = doc.containerAt({ x: x + 95, y: y + 30 });
    doc.add(node, target);
    this.commit("create-node");
    this.canvas.select(node.id);
  }

  private createZone(): void {
    const doc = this.canvas.model;
    if (doc === null) return;
    const at = this.canvas.viewCenter();
    const id = `zone_${Date.now().toString(36)}`;

    const zone: DiagramElement = {
      id,
      kind: "zone",
      type: "boundary",
      label: "Новая область / Слой",
      semanticId: `zone.${id}`,
      tags: [],
      metadata: { description: "Пользовательский логический контейнер." },
      x: snap(at.x, 10), y: snap(at.y, 10), width: 420, height: 300,
      style: {
        fill: "#f8fafc", stroke: "#94a3b8", strokeWidth: 2,
        strokeDasharray: "none", headerBg: "#e2e8f0",
      },
      parent: null,
      children: [],
      wireOrder: Number.POSITIVE_INFINITY,
    };

    doc.add(zone, null);
    this.commit("create-zone");
    this.canvas.select(zone.id);
  }

  deleteSelection(): void {
    const doc = this.canvas.model;
    const selection = this.canvas.selected;
    if (doc === null || selection === null) return;

    if (selection.kind === "edge") {
      doc.removeEdge(selection.id);
    } else {
      // Everything selected goes, not just the primary.
      for (const element of this.canvas.selectedElements()) {
        doc.remove(element);
      }
    }

    this.canvas.select(null);
    this.commit("delete");
  }

  deleteEdge(edgeId: string): void {
    const doc = this.canvas.model;
    if (doc === null) return;
    doc.removeEdge(edgeId);
    // The selection may have been the edge just removed; dropping it keeps the
    // inspector from binding to something that no longer exists (D-05).
    if (this.canvas.selected?.id === edgeId) this.canvas.select(null);
    this.commit("delete-edge");
    this.inspector.render(this.canvas.selected);
  }

  addEdgeFromSelection(targetId: string, type: string, label: string): void {
    const doc = this.canvas.model;
    const selection = this.canvas.selected;
    if (doc === null || selection === null || selection.kind === "edge") return;

    doc.addEdge({
      id: `edge_${Date.now().toString(36)}`,
      from: selection.id,
      to: targetId,
      label,
      type,
    });
    this.commit("add-edge");
    this.inspector.render(selection);
  }

  /**
   * Apply an inspector field edit.
   *
   * Typing produces one history step per burst, not one per keystroke: the
   * state before the first change is held, and committed once the field has
   * been quiet for a moment or something else needs the history. Before this,
   * text edits mutated the model without recording anything, so undo jumped
   * straight past them and silently discarded the typing (D-02).
   */
  editField(apply: () => void, options: { rerender?: boolean; reselect?: boolean } = {}): void {
    if (this.fieldEditSnapshot === null) {
      this.fieldEditSnapshot = this.snapshot();
    }

    apply();
    this.markDirty();
    if (options.rerender === true) this.canvas.render();
    if (options.reselect === true) this.inspector.render(this.canvas.selected);

    if (this.fieldEditTimer !== null) window.clearTimeout(this.fieldEditTimer);
    this.fieldEditTimer = window.setTimeout(() => this.flushFieldEdit(), FIELD_EDIT_QUIET_MS);
  }

  /**
   * Turn a burst of typing into one history step.
   *
   * Called whenever something else is about to touch the history, so that a
   * half-finished edit can never end up straddling another action.
   */
  private flushFieldEdit(): void {
    if (this.fieldEditTimer !== null) {
      window.clearTimeout(this.fieldEditTimer);
      this.fieldEditTimer = null;
    }
    const before = this.fieldEditSnapshot;
    this.fieldEditSnapshot = null;
    if (before === null) return;

    const now = this.snapshot();
    if (before === now) return;

    // Seed the stack with the pre-edit state when this is the first change
    // after a load, so undo has somewhere to go back to.
    this.history.begin(before);
    if (this.history.end(now)) this.syncToolbar(this.canvas.selected);
  }

  private commit(reason: string): void {
    this.flushFieldEdit();
    this.canvas.notifyModelChanged(reason);
    this.history.push(this.snapshot());
    this.markDirty();
    this.syncToolbar(this.canvas.selected);
  }

  private undo(): void {
    this.flushFieldEdit();
    const snapshot = this.history.undo();
    if (snapshot !== null) this.restore(snapshot);
  }

  private redo(): void {
    this.flushFieldEdit();
    const snapshot = this.history.redo();
    if (snapshot !== null) this.restore(snapshot);
  }

  // ------------------------------------------------------------------- i/o

  private async save(): Promise<void> {
    this.flushFieldEdit();
    const doc = this.canvas.model;
    if (doc === null) return;
    if (this.currentEntry === null) {
      this.notify(
        "Эта схема загружена вручную и не привязана к файлу на сервере. Сохранение недоступно.",
      );
      return;
    }

    try {
      await this.store.save({ file: this.currentEntry.file }, serializeDocument(doc));
      this.dirty = false;
      this.slot("dirty").hidden = true;
      this.flashSaved();
    } catch (err) {
      this.notify(
        `Ошибка сохранения: ${(err as Error).message}\nПроверьте, запущен ли сервер.`,
      );
    }
  }

  private flashSaved(): void {
    const button = this.root.querySelector<HTMLElement>('[data-action="save"]');
    if (button === null) return;
    const previous = button.textContent;
    button.textContent = "✅ Сохранено";
    window.setTimeout(() => {
      button.textContent = previous;
    }, 2000);
  }

  private exportDrawio(): void {
    const doc = this.canvas.model;
    if (doc === null) return;
    download(drawioFileName(doc), exportDrawio(doc), "application/xml");
  }

  private toggleJsonModal(): void {
    const modal = this.slot("json-modal");
    const editor = this.slot("json-text") as unknown as HTMLTextAreaElement;
    const doc = this.canvas.model;

    if (modal.hidden) {
      editor.value = doc === null ? "" : JSON.stringify(serializeDocument(doc), null, 2);
      modal.hidden = false;
    } else {
      modal.hidden = true;
    }
  }

  private async copyJson(): Promise<void> {
    const editor = this.slot("json-text") as unknown as HTMLTextAreaElement;
    try {
      await navigator.clipboard.writeText(editor.value);
      this.notify("JSON скопирован в буфер обмена");
    } catch {
      this.notify("Не удалось получить доступ к буферу обмена");
    }
  }

  private applyJson(): void {
    const editor = this.slot("json-text") as unknown as HTMLTextAreaElement;
    try {
      const wire = JSON.parse(editor.value) as WireDocument;
      const title = wire.metadata?.title ?? "Пользовательская схема";
      this.currentEntry = null;
      this.loadWire(wire, title);
      this.slot("json-modal").hidden = true;
    } catch (err) {
      this.notify(`Ошибка в формате JSON: ${(err as Error).message}`);
    }
  }

  // ------------------------------------------------------------------- ui

  private toggleSidebar(): void {
    this.slot("sidebar").classList.toggle("is-collapsed");
  }

  private markDirty(): void {
    this.dirty = true;
    this.slot("dirty").hidden = false;
  }

  private syncToolbar(selection: Selection | null): void {
    this.setDisabled("undo", !this.history.canUndo);
    this.setDisabled("redo", !this.history.canRedo);
    this.setDisabled("delete", selection === null);
  }

  /** Whether the model has unsaved changes (R-STATE-01). */
  get hasUnsavedChanges(): boolean {
    return this.dirty;
  }

  private setDisabled(action: string, disabled: boolean): void {
    const button = this.root.querySelector<HTMLButtonElement>(`[data-action="${action}"]`);
    if (button !== null) button.disabled = disabled;
  }

  private notify(message: string): void {
    window.alert(message);
  }
}
