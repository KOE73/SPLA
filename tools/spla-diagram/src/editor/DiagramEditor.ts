import { DiagramCanvas, type Selection } from "../canvas/DiagramCanvas.js";
import {
  CenterPortAssigner,
  DiscretePortAssigner,
  UniformPortAssigner,
} from "../canvas/ports/assigners.js";
import { DiagramDocument } from "../model/document.js";
import { StyleLibrary } from "../model/StyleLibrary.js";
import { builtinStyleSheet } from "../model/style-defaults.js";
import type { WireStyleSheet } from "../model/style-types.js";
import { parseDocument, serializeDocument } from "../model/wire.js";
import type { WireDocument } from "../model/wire-types.js";
import type { DiagramElement } from "../model/types.js";
import { snap } from "../geometry/rect.js";
import { History } from "./History.js";
import { Inspector, type InspectorHost } from "./Inspector.js";
import { StyleEditor } from "./StyleEditor.js";
import { StyleList, type StylePanelHost } from "./StyleList.js";
import { BasePanel } from "./BasePanel.js";
import { drawioFileName, exportDrawio } from "./io/drawio.js";
import {
  HttpModelStore,
  HttpStyleStore,
  download,
  readJsonFile,
  type ModelStore,
  type StyleStore,
} from "./io/transfer.js";
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
  styleStore?: StyleStore;
}

/** How long a text field must be quiet before its edits become a history step. */
const FIELD_EDIT_QUIET_MS = 600;

const INSPECTOR_WIDTH_KEY = "spla-diagram:inspector-width";
const MIN_INSPECTOR_WIDTH = 320;
const STYLE_LIST_WIDTH_KEY = "spla-diagram:style-list-width";
const MIN_STYLE_LIST_WIDTH = 180;
const MIN_STYLE_EDITOR_WIDTH = 220;

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

type Slot =
  | "canvas" | "views" | "tags" | "catalog" | "custom-catalog" | "custom-catalog-section"
  | "inspector-badge" | "inspector-body" | "title" | "dirty" | "zoom"
  | "json-modal" | "json-text" | "file-input" | "drop-hint" | "sidebar"
  | "styles-body" | "style-list" | "style-editor" | "style-pane-resizer"
  | "inspector" | "inspector-resizer"
  | "tab-base" | "base-search" | "base-body" | "base-list";

type Tab = "properties" | "styles" | "base";

/**
 * One history entry.
 *
 * Styles travel with the document because they are edited from the same panel
 * and belong to the same "what I just did". Snapshotting only the document —
 * which is what this used to do — meant Ctrl+Z after recolouring a style undid
 * whatever came *before* it and threw the style edit away without a trace.
 */
interface Snapshot {
  doc: WireDocument;
  styles: WireStyleSheet;
}

/**
 * The application: a canvas plus everything around it.
 *
 * Owns the things a reusable canvas must not know about — where models come
 * from, where they are saved, what undo means, what the toolbar looks like.
 * All of it talks to the canvas through its public API and its events.
 */
export class DiagramEditor implements InspectorHost, StylePanelHost {
  readonly canvas: DiagramCanvas;

  private readonly root: HTMLElement;
  private readonly slots = new Map<Slot, HTMLElement>();
  private readonly history = new History();
  private readonly inspector: Inspector;
  private readonly styleList: StyleList;
  private readonly styleEditor: StyleEditor;
  private readonly basePanel: BasePanel;
  private readonly store: ModelStore;
  private readonly styleStore: StyleStore;

  private catalog: readonly CatalogEntry[];
  private currentEntry: CatalogEntry | null = null;
  private dirty = false;
  /**
   * Tracked apart from `dirty` because the two have different destinations and
   * different failure modes: a model may be bound to no file at all while the
   * styles edited through it are perfectly saveable.
   */
  private stylesDirty = false;
  /** Pending coalesced canvas redraw after a burst of style edits. */
  private redrawHandle: number | null = null;
  private styleLibrary: StyleLibrary;

  /** State before the current burst of typing, held until the burst settles. */
  private fieldEditSnapshot: string | null = null;
  private fieldEditTimer: number | null = null;

  constructor(root: HTMLElement, options: DiagramEditorOptions = {}) {
    this.root = root;
    this.catalog = options.catalog ?? [];
    this.store = options.store ?? new HttpModelStore("./");
    this.styleStore = options.styleStore ?? new HttpStyleStore("./");

    for (const node of root.querySelectorAll<HTMLElement>("[data-slot]")) {
      const name = node.dataset.slot as Slot | undefined;
      if (name !== undefined) this.slots.set(name, node);
    }

    // The built-in sheet stands in until the real one arrives: the canvas is
    // constructed synchronously and must never exist without a library, and a
    // library that fails to load is not a reason to show a grey diagram.
    this.styleLibrary = StyleLibrary.parse(builtinStyleSheet());

    this.canvas = new DiagramCanvas(this.slot("canvas"), { styles: this.styleLibrary });
    this.inspector = new Inspector(
      this.slot("inspector-badge"),
      this.slot("inspector-body"),
      this,
    );
    this.styleList = new StyleList(this.slot("style-list"), this);
    this.styleEditor = new StyleEditor(this.slot("style-editor"), this);
    this.basePanel = new BasePanel(
      this.slot("base-search") as HTMLInputElement,
      this.slot("base-list"),
      this
    );

    this.bindCanvas();
    this.bindActions();
    this.bindKeyboard();
    this.bindFiles();
    this.bindInspectorResize();
    this.bindStyleListResize();
    this.renderCatalog();
    this.setTab("properties");

    void this.start();
  }

  /**
   * Load the style library, then the first model.
   *
   * Ordered, not parallel: `parseDocument` migrates a zone's inline colours
   * into the library as it parses, so a model opened against the placeholder
   * library would mint its imported styles into a library about to be thrown
   * away — and the zones would come back wearing ids that no longer exist.
   */
  private async start(): Promise<void> {
    try {
      this.setStyleLibrary(StyleLibrary.parse(await this.styleStore.load()));
    } catch (err) {
      this.notify(
        `Библиотека стилей не загружена: ${(err as Error).message}\n` +
          "Используются встроенные стили. Сохранение стилей перезапишет файл на сервере.",
      );
    }
    this.styleList.render();

    const first = this.catalog[0];
    if (first !== undefined) await this.openCatalogEntry(first);
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
      case "tab-properties": return this.setTab("properties");
      case "tab-styles": return this.setTab("styles");
      case "tab-base": return this.setTab("base");
      default: return;
    }
  }

  /**
   * Swap which half of the right-hand panel is showing.
   *
   * Both halves stay in the DOM and are only hidden, so switching tabs cannot
   * touch the canvas selection — the properties form is still bound to whatever
   * is selected when the user comes back to it.
   */
  private setTab(tab: Tab): void {
    this.slot("inspector-body").hidden = tab !== "properties";
    this.slot("styles-body").hidden = tab !== "styles";
    this.slot("base-body").hidden = tab !== "base";
    this.slot("inspector-badge").hidden = tab !== "properties";

    for (const node of this.root.querySelectorAll<HTMLElement>("[data-tab]")) {
      node.classList.toggle("is-active", node.dataset.tab === tab);
    }

    if (tab === "properties") this.inspector.render(this.canvas.selected);
    else if (tab === "styles") this.styleList.render();
    else if (tab === "base") this.basePanel.render();
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
      case "structure-edges":
        // Not a style question, which is why no style can answer it: in
        // model-core-full.json 100 of 119 edges are implements/extends, and the
        // 19 that describe runtime behaviour are invisible inside them however
        // they are coloured. Presentation state only — nothing is written.
        this.canvas.setEdgeFamilyHidden("structure", !on);
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

  /**
   * Drag the strip between the canvas and the right panel to resize it.
   *
   * The panel was a fixed 380px, which was fine for a short property list but
   * cramped for the style editor's gradient stops and per-end arrow controls —
   * exactly the fields a wide monitor has room for. Width lives in
   * localStorage, not the model: it is a per-viewer convenience, the same
   * category as which sidebar is collapsed, not something a saved diagram
   * should carry.
   */
  private bindInspectorResize(): void {
    this.bindColumnResize({
      handle: this.slot("inspector-resizer"),
      panel: this.slot("inspector"),
      storageKey: INSPECTOR_WIDTH_KEY,
      min: MIN_INSPECTOR_WIDTH,
      max: () => this.maxInspectorWidth(),
      // The handle sits to the panel's left, so dragging it left (negative
      // delta) must widen the panel: growth is the inverse of pointer motion.
      grow: "left",
    });
  }

  /**
   * The same drag, one level in: the style list against its own editor form,
   * inside whatever width the inspector panel currently has. Two independent
   * resizers because the outer one trades the panel against the canvas, and
   * this one trades the list against the form — different questions, so a
   * width for the panel does not answer "how much of it goes to the list".
   */
  private bindStyleListResize(): void {
    const list = this.slot("style-list");
    this.bindColumnResize({
      handle: this.slot("style-pane-resizer"),
      panel: list,
      storageKey: STYLE_LIST_WIDTH_KEY,
      min: MIN_STYLE_LIST_WIDTH,
      max: () => this.slot("styles-body").getBoundingClientRect().width - MIN_STYLE_EDITOR_WIDTH,
      // Here the handle sits to the panel's right, so dragging it right
      // (positive delta) is what widens it — motion and growth agree.
      grow: "right",
    });
  }

  /**
   * One draggable column-width divider. `grow` says which side of the handle
   * the resized panel is on, which is the one thing that differs between the
   * two current uses — everything else (clamping, persistence, the dragging
   * class) is identical and not worth writing twice.
   */
  private bindColumnResize(options: {
    handle: HTMLElement;
    panel: HTMLElement;
    storageKey: string;
    min: number;
    max: () => number;
    grow: "left" | "right";
  }): void {
    const { handle, panel, storageKey, min, max, grow } = options;
    const sign = grow === "left" ? -1 : 1;

    const stored = Number(window.localStorage.getItem(storageKey));
    if (Number.isFinite(stored) && stored > 0) {
      panel.style.width = `${clamp(stored, min, max())}px`;
    }

    handle.addEventListener("pointerdown", (e) => {
      e.preventDefault();
      const startX = e.clientX;
      const startWidth = panel.getBoundingClientRect().width;
      handle.setPointerCapture(e.pointerId);
      handle.classList.add("is-dragging");

      const onMove = (move: PointerEvent): void => {
        const width = clamp(startWidth + sign * (move.clientX - startX), min, max());
        panel.style.width = `${width}px`;
      };
      const onUp = (): void => {
        handle.classList.remove("is-dragging");
        handle.releasePointerCapture(e.pointerId);
        handle.removeEventListener("pointermove", onMove);
        handle.removeEventListener("pointerup", onUp);
        window.localStorage.setItem(storageKey, panel.getBoundingClientRect().width.toFixed(0));
      };
      handle.addEventListener("pointermove", onMove);
      handle.addEventListener("pointerup", onUp);
    });
  }

  /** Leaves room for the sidebar and a usable sliver of canvas either way. */
  private maxInspectorWidth(): number {
    return Math.max(MIN_INSPECTOR_WIDTH, window.innerWidth - 480);
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

    // Parsed against the live library so that a model still carrying inline
    // zone colours is migrated into named styles as it loads (see wire.ts).
    const doc = parseDocument(wire, this.styleLibrary);
    this.canvas.setModel(doc);
    this.history.reset(this.snapshot());
    this.dirty = false;
    this.syncDirtyChip();
    this.slot("title").textContent = title;
    this.renderViews(doc);
    this.renderTags();
    this.syncToolbar(null);
    // Migration may have minted styles; the list must show them.
    this.styleList.render();
  }

  private snapshot(): string {
    const doc = this.canvas.model;
    if (doc === null) return "";
    const state: Snapshot = {
      doc: serializeDocument(doc),
      styles: this.styleLibrary.serialize(),
    };
    return JSON.stringify(state);
  }

  private restore(snapshot: string): void {
    if (snapshot === "") return;
    const state = JSON.parse(snapshot) as Snapshot;

    // The library is rebuilt, not mutated, so a style deleted since the
    // snapshot comes back and one added since it goes away. Everything holding
    // a library must therefore read it through `this.styles`, never cache it.
    this.styleLibrary = StyleLibrary.parse(state.styles);
    const doc = parseDocument(state.doc, this.styleLibrary);
    this.canvas.setStyles(this.styleLibrary);
    this.canvas.replaceModel(doc);
    this.renderTags();

    this.markDirty();
    this.markStylesDirty();
    this.styleList.setActive(this.styleEditor.openId);
    this.styleEditor.render();
    this.syncToolbar(this.canvas.selected);
  }

  private setStyleLibrary(library: StyleLibrary): void {
    this.styleLibrary = library;
    this.canvas.setStyles(library);
    this.renderTags();
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

  /**
   * The tag bar: one pill per domain tag currently worn by something on the
   * canvas, sourced entirely from style tags — there is no separate per-element
   * tag to maintain. Rebuilt whenever the set of tags in use can have changed:
   * on model load and after any style edit (a rename, a retag, a new style).
   *
   * Several pills can be active at once — a style can carry more than one tag
   * (a zone can be both "llm" and "security"), so isolating "one angle" and
   * "another angle" together and seeing their union is a real thing to want,
   * not just "pick one".
   */
  renderTags(): void {
    const host = this.slot("tags");
    const tags = this.canvas.tagsInUse();

    if (tags.length === 0) {
      replaceChildren(host);
      return;
    }

    replaceChildren(
      host,
      ...tags.map((tag) =>
        el("button", {
          class: "view-btn tag-btn",
          text: `#${tag}`,
          dataset: { tagId: tag },
          on: {
            click: () => {
              this.canvas.toggleHighlightTag(tag);
              this.syncTags();
            },
          },
        }),
      ),
      tags.length > 0
        ? el("button", {
            class: "btn-icon",
            title: "Снять всю подсветку по тегам",
            text: "✕",
            on: {
              click: () => {
                this.canvas.clearHighlightTags();
                this.syncTags();
              },
            },
          })
        : null,
    );
    this.syncTags();
  }

  private syncTags(): void {
    const active = this.canvas.highlightTags;
    for (const node of this.slot("tags").querySelectorAll<HTMLElement>("[data-tag-id]")) {
      node.classList.toggle("is-active", active.has(node.dataset.tagId ?? ""));
    }
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
      // A named style, not five inline colours. The slate theme is what the old
      // hardcoded literals here spelled out, so a new zone looks the same as
      // before while now being repaintable in one place.
      styleId: "zone.slate",
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

  // ------------------------------------------------------------- styles API

  /** The library everything on screen resolves its look through. */
  get styles(): StyleLibrary {
    return this.styleLibrary;
  }

  /**
   * A field edit inside a style.
   *
   * Deliberately the same machinery as `editField`: a burst of typing into a
   * colour box is one history step, and a style edit interleaved with a model
   * edit lands in the right order because both flush through the same timer.
   * The canvas is redrawn whole, because a style has no single owner — every
   * element wearing it changes at once, which is the property styles exist for.
   * Whole is expensive, though, so the redraw is coalesced to one frame: a
   * dragged colour picker fires this dozens of times a second, and on
   * `model-core-full.json` that is 428 nodes rebuilt per mouse move.
   */
  editStyle(apply: () => void): void {
    if (this.fieldEditSnapshot === null) {
      this.fieldEditSnapshot = this.snapshot();
    }

    apply();
    this.markStylesDirty();
    this.scheduleRedraw();

    if (this.fieldEditTimer !== null) window.clearTimeout(this.fieldEditTimer);
    this.fieldEditTimer = window.setTimeout(() => this.flushFieldEdit(), FIELD_EDIT_QUIET_MS);
  }

  /**
   * Redraw once for however many style edits arrived before the next frame.
   *
   * Only the drawing is delayed — the library, the dirty flag and the history
   * are all updated synchronously, so nothing can observe a stale model.
   */
  private scheduleRedraw(): void {
    if (this.redrawHandle !== null) return;
    this.redrawHandle = window.requestAnimationFrame(() => {
      this.redrawHandle = null;
      this.canvas.setStyles();
      // A retag is exactly the edit this bar exists to reflect, and the bar is
      // a cheap DOM diff — not worth a second coalescing path of its own.
      this.renderTags();
    });
  }

  private cancelScheduledRedraw(): void {
    if (this.redrawHandle === null) return;
    window.cancelAnimationFrame(this.redrawHandle);
    this.redrawHandle = null;
  }

  /** Create, clone, rename or delete: one discrete step, never coalesced. */
  commitStyle(apply: () => void): void {
    this.flushFieldEdit();
    apply();
    this.markStylesDirty();
    // Immediate, and any frame still pending is dropped: a discrete action must
    // not be overtaken by a redraw queued before it happened.
    this.cancelScheduledRedraw();
    this.canvas.setStyles();
    this.renderTags();
    this.history.push(this.snapshot());
    this.syncToolbar(this.canvas.selected);
  }

  openStyle(id: string | null): void {
    this.styleList.setActive(id);
    this.styleEditor.open(id);
  }

  /** From the inspector's "править стиль" button. */
  openStyleTab(styleId: string): void {
    this.setTab("styles");
    this.openStyle(styleId);
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

  /**
   * Save whatever is dirty — the model, the styles, or both.
   *
   * The two are reported separately on purpose. Styles are shared across every
   * model in the catalogue and are always bound to a file; a model opened by
   * hand is bound to none. Rolling both into one "не удалось сохранить" would
   * mean the user could not tell whether the style work they just did survived.
   */
  private async save(): Promise<void> {
    this.flushFieldEdit();
    const problems: string[] = [];
    let stylesSaved = false;

    if (this.stylesDirty) {
      try {
        await this.styleStore.save(this.styleLibrary.serialize());
        this.stylesDirty = false;
        stylesSaved = true;
      } catch (err) {
        problems.push(`Стили не сохранены: ${(err as Error).message}`);
      }
    }

    const doc = this.canvas.model;
    if (this.dirty && doc !== null) {
      if (this.currentEntry === null) {
        problems.push(
          "Схема загружена вручную и не привязана к файлу на сервере — она не сохранена.",
        );
      } else {
        try {
          await this.store.save({ file: this.currentEntry.file }, serializeDocument(doc));
          this.dirty = false;
        } catch (err) {
          problems.push(`Схема не сохранена: ${(err as Error).message}`);
        }
      }
    }

    this.syncDirtyChip();

    if (problems.length === 0) {
      this.flashSaved();
      return;
    }
    this.notify(
      [
        stylesSaved ? "Библиотека стилей сохранена." : null,
        ...problems,
        "Проверьте, запущен ли сервер.",
      ]
        .filter((line): line is string => line !== null)
        .join("\n"),
    );
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
    download(drawioFileName(doc), exportDrawio(doc, this.styleLibrary), "application/xml");
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
    this.syncDirtyChip();
  }

  private markStylesDirty(): void {
    this.stylesDirty = true;
    this.syncDirtyChip();
  }

  private syncDirtyChip(): void {
    this.slot("dirty").hidden = !this.dirty && !this.stylesDirty;
  }

  private syncToolbar(selection: Selection | null): void {
    this.setDisabled("undo", !this.history.canUndo);
    this.setDisabled("redo", !this.history.canRedo);
    this.setDisabled("delete", selection === null);
  }

  /** Whether the model or the style library has unsaved changes (R-STATE-01). */
  get hasUnsavedChanges(): boolean {
    return this.dirty || this.stylesDirty;
  }

  private setDisabled(action: string, disabled: boolean): void {
    const button = this.root.querySelector<HTMLButtonElement>(`[data-action="${action}"]`);
    if (button !== null) button.disabled = disabled;
  }

  notify(message: string): void {
    window.alert(message);
  }
}
