import type { DiagramCanvas, Selection } from "../canvas/DiagramCanvas.js";
import type { DiagramEdge, DiagramElement } from "../model/types.js";
import { isContainer } from "../model/types.js";
import { edgeTypes } from "../canvas/render/edgeStyles.js";
import { el, replaceChildren } from "../util/dom.js";

/** Semantic types offered in the inspector (R-INSP-05). */
const TYPE_OPTIONS: ReadonlyArray<readonly [string, string]> = [
  ["concept", "💡 Concept (Концепт)"],
  ["component", "📦 Component"],
  ["service", "⚙️ Service"],
  ["security-component", "🛡️ Security"],
  ["tool", "🔧 Tool"],
  ["database", "💾 Storage / DB"],
  ["external-system", "🌐 External"],
  ["note", "📝 Note / Текст"],
  ["boundary", "🔲 Boundary"],
  ["subsystem", "🏛 Subsystem"],
];

const EDGE_TYPE_LABELS: Readonly<Record<string, string>> = {
  call: "Вызов (Call)",
  "data-flow": "Данные (Data)",
  event: "Событие (Event)",
  security: "Доступ (Security)",
  storage: "Хранилище (Storage)",
  extends: "Наследование (extends)",
  implements: "Реализация (implements)",
  realizes: "Реализация (realizes)",
  composes: "Композиция (composes)",
};

export interface InspectorHost {
  readonly canvas: DiagramCanvas;
  /** Apply a field edit and mark the document dirty. */
  editField(apply: () => void, options?: { rerender?: boolean; reselect?: boolean }): void;
  deleteSelection(): void;
  addEdgeFromSelection(targetId: string, type: string, label: string): void;
  deleteEdge(edgeId: string): void;
}

/**
 * The right-hand properties panel.
 *
 * Rebuilt from the model on selection change, and updated in place while a
 * gesture is running so the coordinate readout tracks the drag.
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
      const edge = doc.edge(selection.id);
      if (edge === undefined) {
        this.renderEmpty();
        return;
      }
      this.renderEdge(edge);
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
    this.badge.textContent = `${container ? "CONTAINER" : "NODE"}: ${element.type}`;
    this.badge.className = `badge ${container ? "badge-zone" : "badge-node"}`;

    const coords = el("span", { class: "mono coords", text: formatGeometry(element) });
    this.coordsNode = coords;

    replaceChildren(
      this.body,
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
      this.descriptionField(element),
      this.codeRefField(element),
      container ? null : this.connectionsPanel(element),
      this.deleteButton(container),
    );
  }

  private renderEdge(edge: DiagramEdge): void {
    this.badge.textContent = `EDGE: ${edge.type}`;
    this.badge.className = "badge badge-edge";

    replaceChildren(
      this.body,
      el("div", { class: "field-row" }, [
        el("span", { class: "mono muted", text: `ID: ${edge.id}` }),
        el("span", { class: "mono coords", text: `FROM: ${edge.from} TO: ${edge.to}` }),
      ]),
      el("label", { class: "field" }, [
        el("span", { class: "field-label", text: "Подпись связи" }),
        el("input", {
          type: "text",
          value: edge.label,
          on: {
            input: (e) => {
              const value = (e.target as HTMLInputElement).value;
              this.host.editField(() => {
                edge.label = value;
              });
            },
          },
        }),
      ]),
      el("label", { class: "field" }, [
        el("span", { class: "field-label", text: "Тип связи" }),
        this.select(edgeTypeOptions(), edge.type, (value) => {
          this.host.editField(() => {
            edge.type = value;
          }, { reselect: true });
        }),
      ]),
      el("div", { class: "field" }, [
        el("button", {
          class: "btn btn-danger full",
          text: "Удалить связь",
          on: { click: () => this.host.deleteEdge(edge.id) },
        }),
      ]),
    );
  }

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

  private typeField(element: DiagramElement): HTMLElement {
    const known = TYPE_OPTIONS.some(([value]) => value === element.type);
    const options = known
      ? TYPE_OPTIONS
      : [...TYPE_OPTIONS, [element.type, `❓ ${element.type}`] as const];

    return el("label", { class: "field" }, [
      el("span", { class: "field-label", text: "Тип элемента" }),
      this.select(options, element.type, (value) => {
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

  private connectionsPanel(element: DiagramElement): HTMLElement {
    const doc = this.host.canvas.model;
    if (doc === null) return el("div");

    const outgoing = doc.outgoingEdges(element.id);
    const others = doc.leaves().filter((n) => n.id !== element.id);

    const list = el(
      "div",
      { class: "edge-list" },
      outgoing.length === 0
        ? [el("div", { class: "muted italic", text: "Нет исходящих связей" })]
        : outgoing.map((edge) => {
            const target = doc.element(edge.to);
            return el("div", { class: "edge-row" }, [
              el("span", { class: "edge-row-label", text: `➔ ${target?.label ?? edge.to}` }),
              el("span", { class: "mono muted", text: `(${edge.type})` }),
              el("button", {
                class: "btn-icon danger",
                text: "✕",
                title: "Удалить связь",
                on: { click: () => this.host.deleteEdge(edge.id) },
              }),
            ]);
          }),
    );

    const targetSelect = this.select(
      [["", "— Куда вести —"], ...others.map((n) => [n.id, n.label] as const)],
      "",
      () => undefined,
    );
    const typeSelect = this.select(edgeTypeOptions(), "call", () => undefined);
    const labelInput = el("input", { type: "text", placeholder: "Подпись (опция)" });

    return el("div", { class: "panel-section" }, [
      el("div", { class: "field-row" }, [
        el("span", { class: "field-label", text: "Связи блока (исходящие)" }),
        el("span", { class: "mono muted", text: String(outgoing.length) }),
      ]),
      list,
      el("div", { class: "panel" }, [
        el("div", { class: "field-label accent", text: "Добавить новую связь:" }),
        el("div", { class: "grid-2" }, [targetSelect, typeSelect]),
        el("div", { class: "field-row gap" }, [
          labelInput,
          el("button", {
            class: "btn btn-primary",
            text: "Связать",
            on: {
              click: () => {
                if (targetSelect.value === "") return;
                this.host.addEdgeFromSelection(
                  targetSelect.value,
                  typeSelect.value,
                  labelInput.value,
                );
              },
            },
          }),
        ]),
      ]),
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

  private select(
    options: ReadonlyArray<readonly [string, string]>,
    selected: string,
    onChange: (value: string) => void,
  ): HTMLSelectElement {
    const node = el(
      "select",
      {
        on: {
          change: (e) => onChange((e.target as HTMLSelectElement).value),
        },
      },
      options.map(([value, label]) => {
        const option = el("option", { value, text: label });
        if (value === selected) option.selected = true;
        return option;
      }),
    );
    return node;
  }
}

function edgeTypeOptions(): ReadonlyArray<readonly [string, string]> {
  return edgeTypes().map((type) => [type, EDGE_TYPE_LABELS[type] ?? type] as const);
}

function formatGeometry(element: DiagramElement): string {
  return `X:${Math.round(element.x)} Y:${Math.round(element.y)} (${Math.round(element.width)}×${Math.round(element.height)})`;
}
