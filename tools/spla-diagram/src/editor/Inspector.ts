import type { DiagramCanvas, Selection } from "../canvas/DiagramCanvas.js";
import type { StyleLibrary } from "../model/StyleLibrary.js";
import type { StyleTarget } from "../model/style-types.js";
import type { DiagramEdge, DiagramElement } from "../model/types.js";
import { isContainer } from "../model/types.js";
import { el, replaceChildren } from "../util/dom.js";
import { select, type Option } from "./fields.js";
import { blockPreview, edgePreview } from "./style-preview.js";

export interface InspectorHost {
  readonly canvas: DiagramCanvas;
  /** The style library, so the inspector can offer what actually exists. */
  readonly styles: StyleLibrary;
  /** Apply a field edit and mark the document dirty. */
  editField(apply: () => void, options?: { rerender?: boolean; reselect?: boolean }): void;
  deleteSelection(): void;
  addEdgeFromSelection(targetId: string, type: string, label: string): void;
  deleteEdge(edgeId: string): void;
  /** Switch the right-hand panel to the Styles tab with this style open. */
  openStyleTab(styleId: string): void;
  openTab(tab: "properties" | "edges" | "styles" | "base"): void;
}

/**
 * The right-hand properties panel.
 *
 * Rebuilt from the model on selection change, and updated in place while a
 * gesture is running so the coordinate readout tracks the drag.
 *
 * It no longer paints anything itself. The three inline colour fields that used
 * to live here are gone: an element's look is a *named style*, and the choice
 * offered here is which one it wears — so that changing how records look is one
 * edit rather than three hundred.
 */
export class Inspector {
  private coordsNode: HTMLElement | null = null;

  constructor(
    private readonly badge: HTMLElement,
    private readonly body: HTMLElement,
    private readonly host: InspectorHost,
  ) {}

  render(selection: Selection | null): void {
    this.coordsNode = null;
    const doc = this.host.canvas.model;

    if (selection === null || doc === null) {
      this.renderEmpty();
      return;
    }

    if (selection.kind === "edge") {
      let edge = doc.edge(selection.id);
      let isVisibleOnCanvas = true;

      if (edge === undefined) {
        // Look up in v2Bundle.relations
        const v2Relations =
          (doc.raw as any)?.v2Bundle?.relations?.relations ||
          (doc.raw as any)?.v2Bundle?.relations ||
          [];
        const rel = Array.isArray(v2Relations)
          ? v2Relations.find((r: any) => r.id === selection.id)
          : undefined;

        if (rel) {
          isVisibleOnCanvas = false;
          edge = {
            id: rel.id,
            from: rel.from,
            to: rel.to,
            type: rel.type || rel.relation || "relates",
            label: rel.label || "",
            styleId: rel.styleId,
          };
        } else if (selection.id.startsWith("ghost_")) {
          const parts = selection.id.split("_");
          if (parts.length >= 3 && parts[1] && parts[2]) {
            isVisibleOnCanvas = false;
            edge = {
              id: selection.id,
              from: parts[1],
              to: parts[2],
              type: "call",
              label: "",
            };
          }
        }
      }

      if (edge === undefined) {
        this.renderEmpty();
        return;
      }
      this.renderEdge(edge, isVisibleOnCanvas);
      return;
    }

    const element = doc.element(selection.id);
    if (element === undefined) {
      this.renderEmpty();
      return;
    }
    this.renderElement(element);
  }

  /** Live coordinate readout during a drag or resize (R-INSP-03). */
  updateGeometry(element: DiagramElement): void {
    if (this.coordsNode === null) return;
    this.coordsNode.textContent = formatGeometry(element);
  }

  private renderEmpty(): void {
    this.badge.textContent = "Ничего не выбрано";
    this.badge.className = "badge";
    replaceChildren(
      this.body,
      el("div", { class: "inspector-empty" }, [
        el("p", { class: "inspector-empty-icon", text: "👆" }),
        el("p", {
          text:
            "Кликните на любой компонент или зону. Названия, типы, описания и связи правятся прямо здесь.",
        }),
      ]),
    );
  }

  private renderElement(element: DiagramElement): void {
    const container = isContainer(element);
    const count = this.host.canvas.selectedIds.size;

    this.badge.textContent =
      count > 1
        ? `ВЫБРАНО: ${count}`
        : `${container ? "CONTAINER" : "NODE"}: ${element.type}`;
    this.badge.className = `badge ${container ? "badge-zone" : "badge-node"}`;

    const coords = el("span", { class: "mono coords", text: formatGeometry(element) });
    this.coordsNode = coords;

    replaceChildren(
      this.body,
      count > 1
        ? el("div", {
            class: "panel panel-info",
            text:
              `Выбрано элементов: ${count}. Перемещение и изменение размера ` +
              `работают на всю группу; поля ниже правят только «${element.label}».`,
          })
        : null,
      el("div", { class: "field-row" }, [
        el("span", { class: "mono muted", text: `ID: ${element.id}` }),
        coords,
      ]),
      this.labelField(element),
      el("div", { class: "grid-2" }, [
        this.typeField(element),
        this.parentField(element),
      ]),
      container ? this.containerPanel(element) : null,
      this.blockStylePicker(element),
      this.descriptionField(element),
      this.codeRefField(element),
      container ? null : el("div", { class: "panel-section" }, [
        el("button", {
          class: "btn full",
          text: `Связи блока (${this.host.canvas.model?.outgoingEdges(element.id).length ?? 0}) →`,
          title: "Перейти на вкладку «Связи» для детальной настройки",
          on: { click: () => this.host.openTab("edges") },
        }),
      ]),
      this.deleteButton(container),
    );
  }

  private renderEdge(edge: DiagramEdge, isVisibleOnCanvas = true): void {
    const doc = this.host.canvas.model;
    const fromEl = doc?.element(edge.from);
    const toEl = doc?.element(edge.to);

    this.badge.textContent = isVisibleOnCanvas ? `EDGE: ${edge.type}` : `GHOST EDGE: ${edge.type}`;
    this.badge.className = `badge ${isVisibleOnCanvas ? "badge-edge" : "badge-node"}`;

    const statusToggle = el("div", {
      class: "field-row",
      attrs: {
        style:
          "background: var(--panel-alt); padding: 8px 12px; border-radius: 8px; border: 1px solid var(--line); margin-bottom: 8px; justify-content: space-between; align-items: center;",
      },
    }, [
      el("span", { text: "Отображение на схеме:", attrs: { style: "font-weight: 600; font-size: 12px;" } }),
      el("div", {
        class: "pill-group",
        attrs: { style: "display: flex; gap: 4px;" },
      }, [
        el("button", {
          class: `btn btn-small ${isVisibleOnCanvas ? "btn-primary" : ""}`,
          text: "● Включена",
          attrs: { style: isVisibleOnCanvas ? "font-weight: 700;" : "opacity: 0.7;" },
          on: {
            click: () => {
              if (!isVisibleOnCanvas && doc) {
                doc.addEdge({
                  id: edge.id,
                  from: edge.from,
                  to: edge.to,
                  type: edge.type,
                  label: edge.label,
                  styleId: edge.styleId,
                });
                (this.host as any).commit("show-edge");
                this.renderEdge(edge, true);
              }
            },
          },
        }),
        el("button", {
          class: `btn btn-small ${!isVisibleOnCanvas ? "btn-secondary" : ""}`,
          text: "○ Скрыта (теневая)",
          attrs: { style: !isVisibleOnCanvas ? "font-weight: 700; border-color: var(--accent); color: var(--accent);" : "opacity: 0.7;" },
          on: {
            click: () => {
              if (isVisibleOnCanvas && doc) {
                doc.removeEdge(edge.id);
                (this.host as any).commit("hide-edge");
                this.renderEdge(edge, false);
              }
            },
          },
        }),
      ]),
    ]);

    replaceChildren(
      this.body,
      statusToggle,
      el("div", { class: "field-row" }, [
        el("span", { class: "mono muted", text: `ID: ${edge.id}` }),
        el("span", { class: "mono coords", text: `${fromEl?.label || edge.from} ➔ ${toEl?.label || edge.to}` }),
      ]),
      el("label", { class: "field" }, [
        el("span", { class: "field-label", text: "Подпись связи" }),
        el("input", {
          type: "text",
          value: edge.label,
          placeholder: "Подпись связи…",
          on: {
            input: (e) => {
              const value = (e.target as HTMLInputElement).value;
              edge.label = value;
              if (isVisibleOnCanvas) {
                this.host.editField(() => {
                  edge.label = value;
                }, { rerender: true });
              } else {
                const v2Rels = (doc?.raw as any)?.v2Bundle?.relations?.relations;
                const r = Array.isArray(v2Rels) ? v2Rels.find((x: any) => x.id === edge.id) : null;
                if (r) r.label = value;
                this.host.canvas.render();
              }
            },
          },
        }),
      ]),
      el("label", { class: "field" }, [
        el("span", { class: "field-label", text: "Тип связи" }),
        select(this.edgeTypeOptions(edge.type), edge.type, (value) => {
          edge.type = value;
          if (isVisibleOnCanvas) {
            this.host.editField(() => {
              edge.type = value;
            }, { rerender: true, reselect: true });
          } else {
            const v2Rels = (doc?.raw as any)?.v2Bundle?.relations?.relations;
            const r = Array.isArray(v2Rels) ? v2Rels.find((x: any) => x.id === edge.id) : null;
            if (r) {
              r.type = value;
              r.relation = value;
            }
            this.host.canvas.render();
          }
        }),
      ]),
      this.edgeStylePicker(edge),
      el("div", { class: "field" }, [
        el("button", {
          class: "btn btn-danger full",
          text: "Удалить связь",
          on: {
            click: () => {
              if (isVisibleOnCanvas) {
                this.host.deleteEdge(edge.id);
              }
              const v2Rels = (doc?.raw as any)?.v2Bundle?.relations?.relations;
              if (Array.isArray(v2Rels)) {
                const idx = v2Rels.findIndex((x: any) => x.id === edge.id);
                if (idx >= 0) v2Rels.splice(idx, 1);
              }
              (this.host as any).commit("delete-edge");
              this.host.canvas.select(null);
            },
          },
        }),
      ]),
    );
  }

  // ----------------------------------------------------------------- fields

  private labelField(element: DiagramElement): HTMLElement {
    return el("label", { class: "field" }, [
      el("span", { class: "field-label", text: "Название / Заголовок" }),
      el("input", {
        class: "input-strong",
        type: "text",
        value: element.label,
        on: {
          input: (e) => {
            const value = (e.target as HTMLInputElement).value;
            this.host.editField(() => {
              element.label = value;
            }, { rerender: true });
          },
        },
      }),
    ]);
  }

  /**
   * Semantic type, offered from the style library.
   *
   * The list used to be a hardcoded table of ten types with hand-written icons.
   * With a live library that is a second source of truth: a style added to
   * `styles.json` was invisible here, and a type listed here with no style
   * behind it silently fell back to the default. The library answers both.
   */
  private typeField(element: DiagramElement): HTMLElement {
    return el("label", { class: "field" }, [
      el("span", { class: "field-label", text: "Тип элемента" }),
      select(this.typeOptions(element.type), element.type, (value) => {
        this.host.editField(
          () => {
            element.type = value;
            element.metadata.type = value;
          },
          { rerender: true, reselect: true },
        );
      }),
    ]);
  }

  private typeOptions(current: string): Option[] {
    const lib = this.host.styles;
    const options: Option[] = lib.list("block").map((entry) => {
      const glyph = lib.resolveBlock(entry.id).icon.glyph;
      return [entry.id, `${glyph} ${entry.name}`] as Option;
    });
    // A model may name a type nobody wrote a style for; it stays selectable so
    // that opening such an element does not silently retype it.
    if (!options.some(([value]) => value === current)) options.push([current, `❓ ${current}`]);
    return options;
  }

  private edgeTypeOptions(current: string): Option[] {
    const options: Option[] = this.host.styles
      .list("edge")
      .map((entry) => [entry.id, entry.name] as Option);
    if (!options.some(([value]) => value === current)) options.push([current, `❓ ${current}`]);
    return options;
  }

  private parentField(element: DiagramElement): HTMLElement {
    const parent = element.parent;
    const label = isContainer(element)
      ? "Является зоной"
      : parent === null
        ? "Вне зон"
        : parent.label;

    return el("div", { class: "field" }, [
      el("span", { class: "field-label", text: "Контейнер (Зона)" }),
      el("div", { class: "readonly-box", text: label }),
    ]);
  }

  private containerPanel(element: DiagramElement): HTMLElement {
    const collapsed = this.host.canvas.isCollapsed(element);
    const count = element.children.filter((c) => !isContainer(c)).length;

    return el("div", { class: "panel panel-info" }, [
      el("span", { text: `📦 Вложенных компонентов: ${count}` }),
      el("button", {
        class: "btn btn-small",
        text: collapsed ? "Развернуть" : "Свернуть",
        on: { click: () => this.host.canvas.toggleCollapse(element.id) },
      }),
    ]);
  }

  // ---------------------------------------------------------- style picking

  private blockStylePicker(element: DiagramElement): HTMLElement {
    const lib = this.host.styles;
    const activeId = lib.blockStyleIdFor(element);
    const explicit = element.styleId !== undefined && lib.has(element.styleId);
    const source = explicit
      ? "задан явно"
      : lib.has(element.type)
        ? `по типу «${element.type}»`
        : "стиль по умолчанию";

    return this.stylePicker("block", activeId, source, explicit, {
      pick: (id) => {
        this.host.editField(
          () => {
            element.styleId = id;
          },
          { rerender: true, reselect: true },
        );
      },
      reset: () => {
        this.host.editField(
          () => {
            element.styleId = undefined;
          },
          { rerender: true, reselect: true },
        );
      },
    });
  }

  private edgeStylePicker(edge: DiagramEdge): HTMLElement {
    const lib = this.host.styles;
    const activeId = lib.edgeStyleIdFor(edge);
    const explicit = edge.styleId !== undefined && lib.has(edge.styleId);
    const source = explicit
      ? "задан явно"
      : lib.has(edge.type)
        ? `по типу «${edge.type}»`
        : "стиль по умолчанию";

    return this.stylePicker("edge", activeId, source, explicit, {
      pick: (id) => {
        this.host.editField(
          () => {
            edge.styleId = id;
          },
          { rerender: true, reselect: true },
        );
      },
      reset: () => {
        this.host.editField(
          () => {
            edge.styleId = undefined;
          },
          { rerender: true, reselect: true },
        );
      },
    });
  }

  /**
   * Which named style this thing wears, and where that came from.
   *
   * The provenance line is the part that matters. "Задан явно" and "по типу"
   * look identical on the canvas but behave completely differently when the
   * type or the library changes, and without saying so the reset button below
   * would be a mystery — reset to *what*?
   */
  private stylePicker(
    target: StyleTarget,
    activeId: string | null,
    source: string,
    explicit: boolean,
    actions: { pick: (id: string) => void; reset: () => void },
  ): HTMLElement {
    const lib = this.host.styles;
    const rows = el("div", { class: "style-rows style-rows-compact" });

    const renderRows = (filter: string): void => {
      const entries = lib.list(target, filter);
      if (entries.length === 0) {
        replaceChildren(
          rows,
          el("div", { class: "muted italic style-empty", text: "Ничего не найдено" }),
        );
        return;
      }
      replaceChildren(
        rows,
        ...entries.map((entry) =>
          el(
            "div",
            {
              class: `style-row${entry.id === activeId ? " is-active" : ""}`,
              title: entry.style.description ?? "",
              on: { click: () => actions.pick(entry.id) },
            },
            [
              el("div", { class: "style-row-preview" }, [
                target === "block"
                  ? blockPreview(lib.resolveBlock(entry.id))
                  : edgePreview(lib.resolveEdge(entry.id)),
              ]),
              el("div", { class: "style-row-text" }, [
                el("span", { class: "style-row-name", text: entry.name }),
                el("span", { class: "mono muted style-row-id", text: entry.id }),
              ]),
            ],
          ),
        ),
      );
    };
    renderRows("");

    return el("div", { class: "panel-section" }, [
      el("div", { class: "field-row" }, [
        el("span", { class: "field-label accent", text: "Стиль" }),
        el("span", { class: "muted", text: source }),
      ]),
      activeId === null
        ? el("div", { class: "muted italic", text: "Библиотека стилей пуста" })
        : el("div", { class: "style-current" }, [
            el("div", { class: "style-row-preview" }, [
              target === "block"
                ? blockPreview(lib.resolveBlock(activeId))
                : edgePreview(lib.resolveEdge(activeId)),
            ]),
            el("div", { class: "style-row-text" }, [
              el("span", {
                class: "style-row-name",
                text: lib.get(activeId)?.name ?? activeId,
              }),
              el("span", { class: "mono muted style-row-id", text: activeId }),
            ]),
          ]),
      el("input", {
        class: "style-filter",
        type: "text",
        placeholder: "Фильтр стилей…",
        on: {
          input: (e) => renderRows((e.target as HTMLInputElement).value),
        },
      }),
      rows,
      el("div", { class: "field-row gap" }, [
        el("button", {
          class: "btn btn-small full",
          text: "Сбросить к типу",
          title: "Убрать явный стиль: элемент снова возьмёт стиль по своему типу",
          disabled: !explicit,
          on: { click: () => actions.reset() },
        }),
        el("button", {
          class: "btn btn-small full",
          text: "Править стиль",
          disabled: activeId === null,
          on: {
            click: () => {
              if (activeId !== null) this.host.openStyleTab(activeId);
            },
          },
        }),
      ]),
    ]);
  }

  // ------------------------------------------------------------------ rest

  private descriptionField(element: DiagramElement): HTMLElement {
    const value = typeof element.metadata.description === "string" ? element.metadata.description : "";
    return el("label", { class: "field" }, [
      el("span", { class: "field-label", text: "Суть / Описание" }),
      el("textarea", {
        rows: 3,
        value,
        placeholder: "Опишите назначение или архитектурную роль…",
        on: {
          input: (e) => {
            const next = (e.target as HTMLTextAreaElement).value;
            this.host.editField(() => {
              element.metadata.description = next;
            });
          },
        },
      }),
    ]);
  }

  private codeRefField(element: DiagramElement): HTMLElement {
    const value = typeof element.metadata.codeRef === "string" ? element.metadata.codeRef : "";
    return el("label", { class: "field" }, [
      el("span", { class: "field-label", text: "Файл / Класс (Code Ref)" }),
      el("input", {
        class: "mono input-code",
        type: "text",
        value,
        placeholder: "src/core/… (необязательно)",
        on: {
          input: (e) => {
            const next = (e.target as HTMLInputElement).value;
            this.host.editField(() => {
              element.metadata.codeRef = next;
            });
          },
        },
      }),
    ]);
  }

  private deleteButton(container: boolean): HTMLElement {
    return el("div", { class: "panel-section" }, [
      el("button", {
        class: "btn btn-danger full",
        text: `Удалить этот ${container ? "контейнер" : "блок"}`,
        on: { click: () => this.host.deleteSelection() },
      }),
    ]);
  }
}

function formatGeometry(element: DiagramElement): string {
  return `X:${Math.round(element.x)} Y:${Math.round(element.y)} (${Math.round(element.width)}×${Math.round(element.height)})`;
}
