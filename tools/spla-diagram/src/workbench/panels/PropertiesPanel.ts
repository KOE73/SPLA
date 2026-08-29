import type { IContentRenderer, GroupPanelPartInitParameters } from "dockview-core";
import type { DiagramEditorFacade } from "../commands/types.js";
import { Inspector, type InspectorHost } from "../../editor/Inspector.js";
import { el } from "../../util/dom.js";

export class PropertiesPanel implements IContentRenderer {
  readonly element: HTMLElement;
  private readonly badge: HTMLElement;
  private readonly body: HTMLElement;
  private readonly inspector: Inspector;

  constructor(private readonly editor: DiagramEditorFacade & InspectorHost) {
    this.badge = el("span", { class: "badge", text: "Ничего не выбрано" });
    this.body = el("div", { class: "inspector-body", attrs: { style: "padding: 12px; overflow-y: auto; height: 100%;" } });

    const head = el("div", { class: "inspector-head" }, [
      el("h2", { text: "Свойства", attrs: { style: "font-size: 13px; margin: 0;" } }),
      this.badge,
    ]);

    this.element = el("div", { class: "inspector", attrs: { style: "width: 100%; height: 100%; display: flex; flex-direction: column; overflow: hidden; background: var(--panel);" } }, [
      head,
      this.body,
    ]);

    this.inspector = new Inspector(this.badge, this.body, editor);

    editor.canvas.events.on("select", (selection) => {
      this.inspector.render(selection);
    });

    editor.canvas.events.on("modelchange", () => {
      const element = editor.canvas.selectedElement();
      if (element !== null) this.inspector.updateGeometry(element);
    });

    editor.canvas.events.on("collapse", () => {
      this.inspector.render(editor.canvas.selected);
    });
  }

  init(_params: GroupPanelPartInitParameters): void {
    this.inspector.render(this.editor.canvas.selected);
  }

  onShow(): void {
    this.inspector.render(this.editor.canvas.selected);
  }
}
