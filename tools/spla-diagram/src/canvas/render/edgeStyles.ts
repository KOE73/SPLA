/**
 * Two families of connection, distinguishable at a glance:
 *
 *   STRUCTURE — how the code is assembled. Grey, triangular heads, static.
 *               Extracted automatically by the generator.
 *   FLOW      — what happens at runtime. Coloured, arrow heads.
 *               Authored by hand.
 */

export type EdgeFamily = "structure" | "flow";

export interface EdgeStyle {
  readonly stroke: string;
  readonly strokeWidth: number;
  readonly dash: string;
  readonly marker: string;
  readonly family: EdgeFamily;
  readonly title: string;
}

const EDGE_STYLES: Readonly<Record<string, EdgeStyle>> = {
  extends: {
    stroke: "#475569", strokeWidth: 1.5, dash: "none",
    marker: "url(#spla-triangle-hollow)", family: "structure", title: "Наследование класса",
  },
  implements: {
    stroke: "#64748b", strokeWidth: 1.5, dash: "8,4",
    marker: "url(#spla-triangle-solid)", family: "structure", title: "Реализация интерфейса",
  },
  realizes: {
    stroke: "#64748b", strokeWidth: 1.5, dash: "8,4",
    marker: "url(#spla-triangle-solid)", family: "structure", title: "Реализация интерфейса",
  },
  composes: {
    stroke: "#0f766e", strokeWidth: 1.8, dash: "none",
    marker: "url(#spla-diamond-solid)", family: "structure", title: "Владение / композиция",
  },
  call: {
    stroke: "#94a3b8", strokeWidth: 1.5, dash: "4,4",
    marker: "url(#spla-arrow)", family: "flow", title: "Вызов",
  },
  "data-flow": {
    stroke: "#3b82f6", strokeWidth: 2, dash: "none",
    marker: "url(#spla-arrow-data)", family: "flow", title: "Поток данных",
  },
  event: {
    stroke: "#ea580c", strokeWidth: 1.8, dash: "2,3",
    marker: "url(#spla-arrow-open)", family: "flow", title: "Событие / уведомление",
  },
  security: {
    stroke: "#f43f5e", strokeWidth: 1.5, dash: "3,3",
    marker: "url(#spla-arrow-security)", family: "flow", title: "Проверка прав / аудит",
  },
  storage: {
    stroke: "#a855f7", strokeWidth: 2, dash: "none",
    marker: "url(#spla-arrow-storage)", family: "flow", title: "Запись / чтение хранилища",
  },
};

export const DEFAULT_EDGE_STYLE: EdgeStyle = {
  stroke: "#cbd5e1",
  strokeWidth: 1.5,
  dash: "none",
  marker: "url(#spla-arrow)",
  family: "flow",
  title: "Связь",
};

export function edgeStyle(type: string): EdgeStyle {
  return EDGE_STYLES[type] ?? DEFAULT_EDGE_STYLE;
}

export function edgeTypes(): string[] {
  return Object.keys(EDGE_STYLES);
}
