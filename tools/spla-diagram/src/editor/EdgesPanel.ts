import type { DiagramEdge, DiagramElement } from "../model/types.js";
import { el, replaceChildren } from "../util/dom.js";
import { select, type Option } from "./fields.js";
import type { DiagramEditor } from "./DiagramEditor.js";

export interface KnownRelation {
  id: string;
  from: string;
  to: string;
  type: string;
  label: string;
  visible: boolean;
  edge?: DiagramEdge;
}

export class EdgesPanel {
  private filter = "";
  private editingId: string | null = null;

  constructor(
    private readonly body: HTMLElement,
    private readonly host: DiagramEditor,
  ) {}

  render(): void {
    const doc = this.host.canvas.model;
    const selection = this.host.canvas.selected;

    if (selection === null || doc === null) {
      replaceChildren(
        this.body,
        el("div", { class: "inspector-empty" }, [
          el("p", { class: "inspector-empty-icon", text: "🔗" }),
          el("p", { text: "Выберите блок на схеме, чтобы просмотреть и настроить его связи." }),
        ]),
      );
      return;
    }

    if (selection.kind === "edge") {
      const edge = doc.edge(selection.id);
      if (edge === undefined) {
        replaceChildren(this.body, el("div", { class: "muted italic", text: "Связь не найдена" }));
        return;
      }
      this.renderSingleEdge(edge);
      return;
    }

    const element = doc.element(selection.id);
    if (element === undefined) {
      replaceChildren(this.body, el("div", { class: "muted italic", text: "Элемент не найден" }));
      return;
    }

    this.renderElementEdges(element);
  }

  private renderSingleEdge(edge: DiagramEdge): void {
    const doc = this.host.canvas.model;
    if (!doc) return;

    const fromEl = doc.element(edge.from);
    const toEl = doc.element(edge.to);

    const typeSelect = select(
      [
        ["call", "Вызов (call)"],
        ["implements", "Реализует (implements)"],
        ["composes", "Компонует (composes)"],
        ["extends", "Расширяет (extends)"],
        ["event", "Событие (event)"],
        ["storage", "Хранилище (storage)"],
        ["relates", "Связан (relates)"],
      ],
      edge.type,
      (newType) => {
        edge.type = newType;
        (this.host as any).commit("edit-edge-type");
        this.host.canvas.render();
      },
    );

    const labelInput = el("input", {
      type: "text",
      value: edge.label || "",
      placeholder: "Подпись связи…",
      on: {
        change: (e) => {
          edge.label = (e.target as HTMLInputElement).value;
          (this.host as any).commit("edit-edge-label");
          this.host.canvas.render();
        },
      },
    });

    replaceChildren(
      this.body,
      el("div", { class: "panel-section" }, [
        el("div", { class: "field-row" }, [
          el("span", { class: "field-label accent", text: "Параметры связи" }),
          el("span", { class: "mono muted", text: edge.id }),
        ]),
        el("div", { class: "field-row" }, [
          el("span", { class: "muted", text: "Откуда:" }),
          el("span", { class: "input-strong", text: fromEl?.label || edge.from }),
        ]),
        el("div", { class: "field-row" }, [
          el("span", { class: "muted", text: "Куда:" }),
          el("span", { class: "input-strong", text: toEl?.label || edge.to }),
        ]),
        el("label", { class: "field" }, [
          el("span", { class: "field-label", text: "Тип связи" }),
          typeSelect,
        ]),
        el("label", { class: "field" }, [
          el("span", { class: "field-label", text: "Подпись" }),
          labelInput,
        ]),
        el("button", {
          class: "btn btn-danger full",
          text: "Удалить связь",
          on: {
            click: () => {
              doc.removeEdge(edge.id);
              (this.host as any).commit("delete-edge");
              this.host.canvas.select(null);
            },
          },
        }),
      ]),
    );
  }

  private renderElementEdges(element: DiagramElement): void {
    const doc = this.host.canvas.model;
    if (!doc) return;

    const canvasEdges = doc.edges.filter((e) => e.from === element.id || e.to === element.id);
    const v2Relations =
      (doc.raw as any)?.v2Bundle?.relations?.relations ||
      (doc.raw as any)?.v2Bundle?.relations ||
      [];
    const rawEntityId = (element.raw as any)?._entity?.id;

    const allKnown = new Map<string, KnownRelation>();

    for (const edge of canvasEdges) {
      allKnown.set(edge.id, {
        id: edge.id,
        from: edge.from,
        to: edge.to,
        type: edge.type,
        label: edge.label || "",
        visible: true,
        edge,
      });
    }

    if (Array.isArray(v2Relations)) {
      for (const rel of v2Relations) {
        const fromMatch = rel.from === element.id || (rawEntityId && rel.from === rawEntityId);
        const toMatch = rel.to === element.id || (rawEntityId && rel.to === rawEntityId);
        if (fromMatch || toMatch) {
          const canvasFrom = fromMatch ? element.id : rel.from;
          const canvasTo = toMatch ? element.id : rel.to;

          const existing = [...allKnown.values()].find(
            (k) => k.from === canvasFrom && k.to === canvasTo && k.type === (rel.type || rel.relation),
          );
          if (!existing) {
            const relId = rel.id || `rel_${canvasFrom}_${canvasTo}_${rel.type || "rel"}`;
            allKnown.set(relId, {
              id: relId,
              from: canvasFrom,
              to: canvasTo,
              type: rel.type || rel.relation || "relates",
              label: rel.label || "",
              visible: false,
            });
          }
        }
      }
    }

    const relationsList = [...allKnown.values()];
    const visibleCount = relationsList.filter((r) => r.visible).length;
    const totalCount = relationsList.length;

    const needle = this.filter.trim().toLowerCase();
    const filtered = relationsList.filter((r) => {
      if (needle === "") return true;
      const otherId = r.from === element.id ? r.to : r.from;
      const otherEl = doc.element(otherId);
      const otherName = otherEl?.label || otherId;
      return (
        otherName.toLowerCase().includes(needle) ||
        r.type.toLowerCase().includes(needle) ||
        r.label.toLowerCase().includes(needle)
      );
    });

    const rows = filtered.map((item) => this.renderRelationRow(element, item));

    const filterInput = el("input", {
      type: "text",
      placeholder: "Поиск связей…",
      value: this.filter,
      on: {
        input: (e) => {
          this.filter = (e.target as HTMLInputElement).value;
          this.render();
        },
      },
    });

    const addSection = this.renderAddSection(element);

    replaceChildren(
      this.body,
      el("div", { class: "panel-section" }, [
        el("div", { class: "field-row" }, [
          el("span", { class: "field-label accent", text: "Связи блока" }),
          el("span", { class: "mono chip", text: `${totalCount} доступно / ${visibleCount} на схеме` }),
        ]),
        filterInput,
        el(
          "div",
          { class: "edge-list", attrs: { style: "max-height: 280px; gap: 6px;" } },
          rows.length === 0
            ? [el("div", { class: "muted italic", text: "Связи не найдены" })]
            : rows,
        ),
        addSection,
      ]),
    );
  }

  private renderRelationRow(element: DiagramElement, item: KnownRelation): HTMLElement {
    const doc = this.host.canvas.model;
    if (!doc) return el("div");

    const isOutgoing = item.from === element.id;
    const otherId = isOutgoing ? item.to : item.from;
    const otherEl = doc.element(otherId);
    const otherName = otherEl?.label || otherId;

    const isEditing = this.editingId === item.id;

    const checkbox = el("input", {
      type: "checkbox",
      title: item.visible ? "Скрыть связь с холста" : "Отобразить связь на холсте",
      on: {
        change: (e) => {
          const checked = (e.target as HTMLInputElement).checked;
          if (checked) {
            doc.addEdge({
              id: item.id,
              from: item.from,
              to: item.to,
              type: item.type,
              label: item.label,
            });
            (this.host as any).commit("show-edge");
          } else {
            doc.removeEdge(item.id);
            (this.host as any).commit("hide-edge");
          }
          this.render();
        },
      },
    });
    (checkbox as HTMLInputElement).checked = item.visible;

    const labelSpan = el("span", {
      class: "edge-row-label",
      attrs: { style: "cursor: pointer; display: flex; align-items: center; gap: 4px;" },
      on: {
        click: () => {
          if (otherEl) {
            this.host.canvas.select(otherEl.id);
          }
        },
      },
    }, [
      el("span", { class: "mono", text: isOutgoing ? "➔" : "⬅" }),
      el("span", { class: "input-strong", text: otherName }),
      item.label ? el("span", { class: "muted", text: `«${item.label}»` }) : null,
    ]);

    const typeBadge = el("span", {
      class: "badge",
      text: item.type,
      title: "Тип связи",
    });

    const editBtn = el("button", {
      class: "btn-icon",
      text: "✏️",
      title: "Редактировать связь",
      on: {
        click: () => {
          this.editingId = isEditing ? null : item.id;
          this.render();
        },
      },
    });

    const deleteBtn = el("button", {
      class: "btn-icon danger",
      text: "✕",
      title: "Удалить связь",
      on: {
        click: () => {
          if (item.visible) {
            doc.removeEdge(item.id);
          }
          const v2Rels = (doc.raw as any)?.v2Bundle?.relations?.relations;
          if (Array.isArray(v2Rels)) {
            const idx = v2Rels.findIndex((r: any) => r.id === item.id);
            if (idx >= 0) v2Rels.splice(idx, 1);
          }
          (this.host as any).commit("delete-relation");
          this.render();
        },
      },
    });

    const row = el(
      "div",
      {
        class: `edge-row${item.visible ? "" : " is-hidden-edge"}`,
        attrs: { style: `opacity: ${item.visible ? "1" : "0.6"}; background: var(--panel-alt); padding: 6px 8px; border-radius: 6px;` },
      },
      [
        checkbox,
        labelSpan,
        typeBadge,
        editBtn,
        deleteBtn,
      ],
    );

    if (isEditing) {
      const typeSel = select(
        [
          ["call", "Вызов (call)"],
          ["implements", "Реализует (implements)"],
          ["composes", "Компонует (composes)"],
          ["extends", "Расширяет (extends)"],
          ["event", "Событие (event)"],
          ["storage", "Хранилище (storage)"],
          ["relates", "Связан (relates)"],
        ],
        item.type,
        () => undefined,
      );
      const lblInput = el("input", { type: "text", value: item.label, placeholder: "Подпись…" });

      const editBox = el("div", {
        attrs: { style: "display: flex; flex-direction: column; gap: 6px; padding: 8px; margin-top: 4px; background: var(--panel); border: 1px solid var(--line); border-radius: 6px;" },
      }, [
        el("div", { class: "grid-2" }, [typeSel, lblInput]),
        el("div", { attrs: { style: "display: flex; gap: 6px; justify-content: flex-end;" } }, [
          el("button", {
            class: "btn btn-small",
            text: "Отмена",
            on: {
              click: () => {
                this.editingId = null;
                this.render();
              },
            },
          }),
          el("button", {
            class: "btn btn-primary btn-small",
            text: "Сохранить",
            on: {
              click: () => {
                item.type = typeSel.value;
                item.label = lblInput.value;
                if (item.edge) {
                  item.edge.type = item.type;
                  item.edge.label = item.label;
                }
                this.editingId = null;
                (this.host as any).commit("update-relation");
                this.render();
              },
            },
          }),
        ]),
      ]);

      return el("div", { attrs: { style: "display: flex; flex-direction: column;" } }, [row, editBox]);
    }

    return row;
  }

  private renderAddSection(element: DiagramElement): HTMLElement {
    const doc = this.host.canvas.model;
    if (!doc) return el("div");

    const others = doc.leaves().filter((n) => n.id !== element.id);
    const targetSelect = select(
      [["", "— Куда вести —"], ...others.map((n) => [n.id, n.label] as Option)],
      "",
      () => undefined,
    );
    const typeSelect = select(
      [
        ["call", "Вызов (call)"],
        ["implements", "Реализует (implements)"],
        ["composes", "Компонует (composes)"],
        ["extends", "Расширяет (extends)"],
        ["event", "Событие (event)"],
        ["storage", "Хранилище (storage)"],
        ["relates", "Связан (relates)"],
      ],
      "call",
      () => undefined,
    );
    const labelInput = el("input", { type: "text", placeholder: "Подпись (опция)" });

    return el("div", { class: "panel", attrs: { style: "flex-direction: column; align-items: stretch; gap: 8px; margin-top: 8px;" } }, [
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
              targetSelect.value = "";
              labelInput.value = "";
              this.render();
            },
          },
        }),
      ]),
    ]);
  }
}
