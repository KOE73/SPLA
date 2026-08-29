import type { IContentRenderer, GroupPanelPartInitParameters } from "dockview-core";
import type { DiagramEditor } from "../../editor/DiagramEditor.js";
import { StyleList, type StylePanelHost } from "../../editor/StyleList.js";
import { StyleEditor } from "../../editor/StyleEditor.js";
import { el } from "../../util/dom.js";

const STYLE_LIST_WIDTH_KEY = "spla-diagram:style-list-width";
const MIN_STYLE_LIST_WIDTH = 180;
const MIN_STYLE_EDITOR_WIDTH = 220;

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}

export class StylesPanel implements IContentRenderer {
  readonly element: HTMLElement;
  private readonly styleListMount: HTMLElement;
  private readonly styleEditorMount: HTMLElement;
  private readonly resizer: HTMLElement;
  private readonly styleList: StyleList;
  private readonly styleEditor: StyleEditor;

  constructor(editor: DiagramEditor & StylePanelHost) {
    this.styleListMount = el("div", {
      class: "style-list",
      attrs: { style: "width: 220px; flex-shrink: 0; display: flex; flex-direction: column; overflow: hidden; border-right: 1px solid var(--border);" },
    });

    this.resizer = el("div", {
      class: "style-pane-resizer",
      title: "Потяните, чтобы изменить ширину списка",
    });

    this.styleEditorMount = el("div", {
      class: "style-editor",
      attrs: { style: "flex: 1; overflow-y: auto; height: 100%;" },
    });

    const body = el(
      "div",
      {
        class: "inspector-body styles-pane",
        attrs: { style: "display: flex; flex-direction: row; height: 100%; overflow: hidden; padding: 0;" },
      },
      [this.styleListMount, this.resizer, this.styleEditorMount],
    );

    this.element = el(
      "div",
      {
        class: "inspector",
        attrs: { style: "width: 100%; height: 100%; display: flex; flex-direction: column; overflow: hidden; background: var(--panel);" },
      },
      [body],
    );

    this.styleList = new StyleList(this.styleListMount, editor);
    this.styleEditor = new StyleEditor(this.styleEditorMount, editor);

    this.bindResizer();
  }

  init(_params: GroupPanelPartInitParameters): void {
    this.render();
  }

  onShow(): void {
    this.render();
  }

  render(): void {
    this.styleList.render();
    this.styleEditor.render();
  }

  openStyle(id: string | null): void {
    this.styleList.setActive(id);
    this.styleEditor.open(id);
  }

  private bindResizer(): void {
    const list = this.styleListMount;
    const stored = Number(window.localStorage.getItem(STYLE_LIST_WIDTH_KEY));
    if (Number.isFinite(stored) && stored > 0) {
      list.style.width = `${clamp(stored, MIN_STYLE_LIST_WIDTH, 400)}px`;
    }

    this.resizer.addEventListener("pointerdown", (e) => {
      e.preventDefault();
      const startX = e.clientX;
      const startWidth = list.getBoundingClientRect().width;
      this.resizer.setPointerCapture(e.pointerId);
      this.resizer.classList.add("is-dragging");

      const onMove = (move: PointerEvent): void => {
        const maxWidth = this.element.getBoundingClientRect().width - MIN_STYLE_EDITOR_WIDTH;
        const width = clamp(startWidth + (move.clientX - startX), MIN_STYLE_LIST_WIDTH, Math.max(MIN_STYLE_LIST_WIDTH, maxWidth));
        list.style.width = `${width}px`;
      };

      const onUp = (): void => {
        this.resizer.classList.remove("is-dragging");
        this.resizer.releasePointerCapture(e.pointerId);
        this.resizer.removeEventListener("pointermove", onMove);
        this.resizer.removeEventListener("pointerup", onUp);
        window.localStorage.setItem(STYLE_LIST_WIDTH_KEY, list.getBoundingClientRect().width.toFixed(0));
      };

      this.resizer.addEventListener("pointermove", onMove);
      this.resizer.addEventListener("pointerup", onUp);
    });
  }
}
