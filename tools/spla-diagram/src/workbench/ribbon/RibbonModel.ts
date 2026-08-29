import type { RibbonSpec } from "./types.js";

export function createDefaultRibbonSpec(): RibbonSpec {
  return {
    tabs: [
      // ----------------------------------------------------------- Главная
      {
        id: "home",
        title: "Главная",
        keyTip: "H",
        groups: [
          {
            id: "history",
            title: "История",
            items: [
              { type: "button", command: "edit.undo", size: "small" },
              { type: "button", command: "edit.redo", size: "small" },
            ],
          },
          {
            id: "create",
            title: "Создание",
            items: [
              { type: "button", command: "diagram.block.add", size: "large" },
              { type: "button", command: "diagram.zone.add", size: "large" },
            ],
          },
          {
            id: "edit",
            title: "Правка",
            items: [
              { type: "button", command: "edit.delete", size: "medium" },
            ],
          },
          {
            id: "file",
            title: "Файл",
            items: [
              { type: "button", command: "file.save", size: "large" },
              { type: "button", command: "file.open", size: "medium" },
              { type: "button", command: "file.export.drawio", size: "medium" },
            ],
          },
        ],
      },

      // ----------------------------------------------------------- Вставка
      {
        id: "insert",
        title: "Вставка",
        keyTip: "N",
        groups: [
          {
            id: "elements",
            title: "Фигуры и слои",
            items: [
              { type: "button", command: "diagram.block.add", size: "large" },
              { type: "button", command: "diagram.zone.add", size: "large" },
            ],
          },
          {
            id: "catalogs",
            title: "Каталоги",
            items: [
              { type: "button", command: "panel.base.toggle", size: "large", label: "База сущностей" },
              { type: "button", command: "panel.catalog.toggle", size: "large", label: "Каталог схем" },
            ],
          },
        ],
      },

      // --------------------------------------------------------- Диаграмма
      {
        id: "diagram",
        title: "Диаграмма",
        keyTip: "D",
        groups: [
          {
            id: "ports",
            title: "Связи",
            items: [
              {
                type: "select",
                command: "diagram.ports.set",
                label: "Причаливание",
                options: [
                  { value: "uniform", label: "равномерно" },
                  { value: "discrete", label: "по сетке" },
                  { value: "center", label: "в центр" },
                ],
                getValue: () => localStorage.getItem("spla.ports") || "uniform",
              },
              { type: "toggle", command: "view.edges.toggleStructure", size: "medium" },
            ],
          },
          {
            id: "model",
            title: "Модель",
            items: [
              { type: "button", command: "file.code.toggle", size: "medium" },
            ],
          },
        ],
      },

      // --------------------------------------------------------------- Вид
      {
        id: "view",
        title: "Вид",
        keyTip: "V",
        groups: [
          {
            id: "canvas_options",
            title: "Холст",
            items: [
              { type: "toggle", command: "view.grid.toggle", size: "small" },
              { type: "toggle", command: "view.snap.toggle", size: "small" },
            ],
          },
          {
            id: "zoom",
            title: "Масштаб",
            items: [
              { type: "button", command: "view.zoom.in", size: "small" },
              { type: "button", command: "view.zoom.out", size: "small" },
              { type: "button", command: "view.zoom.reset", size: "small" },
              { type: "button", command: "view.zoom.fit", size: "small" },
            ],
          },
          {
            id: "appearance",
            title: "Оформление",
            items: [
              {
                type: "select",
                command: "view.theme.set",
                label: "Тема",
                options: [
                  { value: "cream", label: "Кремовая (Cream)" },
                  { value: "dark", label: "Тёмная (Dark)" },
                  { value: "emerald", label: "Изумрудная (Emerald)" },
                  { value: "light", label: "Светлая (Light)" },
                ],
                getValue: () => localStorage.getItem("spla.theme") || "cream",
              },
              {
                type: "select",
                command: "view.lang.set",
                label: "Язык данных",
                options: [
                  { value: "ru", label: "RU" },
                  { value: "en", label: "EN" },
                ],
                getValue: (ctx) => ctx.dataLang || "ru",
              },
            ],
          },
          {
            id: "panels",
            title: "Панели",
            items: [
              { type: "toggle", command: "panel.properties.toggle", size: "small" },
              { type: "toggle", command: "panel.relations.toggle", size: "small" },
              { type: "toggle", command: "panel.filters.toggle", size: "small" },
              { type: "toggle", command: "panel.styles.toggle", size: "small" },
              { type: "toggle", command: "panel.catalog.toggle", size: "small" },
              { type: "toggle", command: "panel.base.toggle", size: "small" },
              { type: "separator" },
              { type: "button", command: "workspace.layout.reset", size: "small" },
            ],
          },
        ],
      },

      // ------------------------------------------------ Контекстные вкладки
      {
        id: "context_node",
        title: "Формат блока",
        keyTip: "B",
        contextual: "node",
        groups: [
          {
            id: "node_actions",
            title: "Элемент",
            items: [
              { type: "button", command: "panel.properties.toggle", size: "large", label: "Параметры" },
              { type: "button", command: "panel.relations.toggle", size: "large", label: "Связи" },
              { type: "button", command: "edit.delete", size: "medium" },
            ],
          },
        ],
      },
      {
        id: "context_zone",
        title: "Формат зоны",
        keyTip: "Z",
        contextual: "zone",
        groups: [
          {
            id: "zone_actions",
            title: "Зона",
            items: [
              { type: "button", command: "panel.properties.toggle", size: "large", label: "Параметры зоны" },
              { type: "button", command: "edit.delete", size: "medium" },
            ],
          },
        ],
      },
      {
        id: "context_edge",
        title: "Формат связи",
        keyTip: "E",
        contextual: "edge",
        groups: [
          {
            id: "edge_actions",
            title: "Связь",
            items: [
              { type: "button", command: "panel.properties.toggle", size: "large", label: "Свойства связи" },
              { type: "button", command: "edit.delete", size: "medium" },
            ],
          },
        ],
      },
    ],
  };
}
