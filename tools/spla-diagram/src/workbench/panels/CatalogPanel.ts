import type { IContentRenderer, GroupPanelPartInitParameters } from "dockview-core";
import type { DiagramEditorFacade } from "../commands/types.js";
import type { CatalogEntry } from "../../editor/io/types.js";
import { el, replaceChildren } from "../../util/dom.js";

export class CatalogPanel implements IContentRenderer {
  readonly element: HTMLElement;
  private readonly catalogSlot: HTMLElement;
  private readonly customCatalogSlot: HTMLElement;
  private readonly customSection: HTMLElement;

  constructor(
    private readonly editor: DiagramEditorFacade,
    private readonly getCatalog: () => readonly CatalogEntry[],
  ) {
    this.catalogSlot = el("div");
    this.customCatalogSlot = el("div");
    this.customSection = el("div", { attrs: { hidden: "true" } }, [
      el("div", { class: "section-label sidebar-label", text: "Пользовательские" }),
      this.customCatalogSlot,
    ]);

    const scroll = el("div", { class: "sidebar-scroll", attrs: { style: "flex: 1; overflow-y: auto; padding: 8px;" } }, [
      el("div", { class: "section-label sidebar-label", text: "Каталог проекта" }),
      this.catalogSlot,
      this.customSection,
    ]);

    const hint = el("div", { class: "hint sidebar-label", attrs: { style: "padding: 8px; border-top: 1px solid var(--border);" } }, [
      el("div", { text: "• Зона тащится за шапку и несёт вложенное" }),
      el("div", { text: "• Блок при переносе в зону меняет родителя" }),
      el("div", { text: "• [ − / + ] в шапке сворачивает зону" }),
    ]);

    const foot = el("div", { class: "sidebar-foot", attrs: { style: "padding: 8px; border-top: 1px solid var(--border);" } }, [
      el("button", {
        class: "btn full",
        text: "📂 Открыть JSON…",
        on: { click: () => editor.openFile() },
      }),
    ]);

    this.element = el(
      "aside",
      {
        class: "sidebar",
        attrs: { style: "width: 100%; height: 100%; display: flex; flex-direction: column; overflow: hidden; background: var(--panel);" },
      },
      [scroll, hint, foot],
    );
  }

  init(_params: GroupPanelPartInitParameters): void {
    this.render();
  }

  onShow(): void {
    this.render();
  }

  render(): void {
    const catalog = this.getCatalog();
    if (catalog.length === 0) {
      replaceChildren(
        this.catalogSlot,
        el("div", { class: "catalog-empty", text: "Каталог схем не загружен." }),
      );
      return;
    }

    replaceChildren(
      this.catalogSlot,
      ...catalog.map((entry) =>
        el(
          "button",
          {
            class: "catalog-item",
            dataset: { catalogId: entry.id },
            on: {
              click: () => {
                void this.editor.openCatalogEntry(entry);
              },
            },
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
}
