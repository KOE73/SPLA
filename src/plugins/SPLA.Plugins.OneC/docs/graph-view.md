# Визуализация графа

Плагин `SPLA.Plugins.OneC` предоставляет визуальное отображение связей конфигурации 1С в web-панели
«1C Configuration Browser».

## Архитектура визуализации

Тяжёлые графовые движки на C# не используются. Вместо этого:

1.  **`Web/OneCBrowserActions.Graph(...)` (C#)**: формирует JSON-подграф из SQLite-базы на основе
    выбранного объекта, режима (`dependencies`, `references`, `dataflow`), глубины и лимита рёбер.
    Внутри использует `Graph/OneCGraphBuilder` и `Graph/CytoscapeGraphAdapter.ToJson`.
2.  **`web/src/BrowserPanel.vue` (Vue + Cytoscape.js)**: получает JSON через канал `plugin.action`
    (action `graph`) и рисует граф в Cytoscape — цвета узлов зависят от типа объекта 1С, цвета рёбер
    от типа связи. Обеспечивает drag/zoom/fit.
3.  **`Graph/CytoscapeGraphAdapter.ToHtmlDocument(...)`**: дополнительно умеет отдать полностью
    самодостаточную HTML-страницу с графом (для «открыть граф в новой вкладке» / офлайн-экспорта).
4.  **`Assets/onec_graph.html`**: встроенный ресурс — более богатый интерактивный вьюер (фильтры по
    типам связей, тултипы, легенда), рассчитанный на мост `window.loadGraph(json)`.

> Историческая версия использовала `Avalonia.WebView` в проекте `SPLA.Plugins.OneC.Avalonia`.
> Он удалён; вся визуализация теперь web-only. Оставшиеся задачи по интеграции — в `TODO.md`.

## Взаимодействие backend ↔ web

*   **web → C#**: панель вызывает `invoke("plugin.action", { pluginId: "onec", action, valueJson })`.
    `OneCPlugin.InvokeActionAsync` маршрутизирует action и возвращает JSON.
*   **C# → web**: результат `graph` содержит `{ nodes, edges }`, который скармливается Cytoscape.

## Режимы просмотра (ViewMode)

*   **`Dependencies`**: исходящие связи объекта (кого он использует).
*   **`References`**: входящие связи (кто использует его).
*   **`DataFlow`**: только потоки данных (`writes`, `reads`, `queries`). Самый полезный режим, чтобы
    понять, какие регистры двигает документ и откуда берёт данные.
