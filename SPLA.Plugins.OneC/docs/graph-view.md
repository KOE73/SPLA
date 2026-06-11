# Визуализация графа

`SPLA.Tools.OneC` предоставляет возможность визуального отображения связей конфигурации 1С.

## Архитектура визуализации

Мы не используем тяжелые графовые движки на C#. Вместо этого мы используем связку:

1.  **`GraphDataBuilder.cs` (C#)**: Формирует JSON-представление подграфа из SQLite-базы на основе выбранного объекта, режима (Dependencies, References, DataFlow) и глубины.
2.  **`Assets/onec_graph.html` (HTML/JS)**: Встроенный HTML-файл, содержащий [Cytoscape.js](https://js.cytoscape.org/). Он отрисовывает граф, применяет стили (цвета узлов зависят от типа объекта 1С, цвета рёбер от типа связи) и обеспечивает интерактивность (drag, zoom, фильтрация).
3.  **`WebView` (UI-слой)**: `SPLA.UI.Avalonia` использует `Avalonia.WebView` для отображения `onec_graph.html`.

## Взаимодействие C# и JS

*   **C# -> JS**: C# вызывает функцию `window.loadGraph(jsonString)` в JavaScript, передавая новые данные графа при смене выбранного объекта или параметров просмотра.
*   **JS -> C#**: JavaScript использует мост WebView (через `window.chrome.webview.postMessage` на Windows или `window.webkit.messageHandlers` на macOS/Linux), чтобы отправлять события в C#:
    *   `modeChanged`
    *   `depthChanged`
    *   `objectSelected`
    *   `objectDoubleClicked`

## Режимы просмотра (ViewMode)

*   **`Dependencies`**: Показывает исходящие связи объекта (кого он использует).
*   **`References`**: Показывает входящие связи объекта (кто использует его).
*   **`DataFlow`**: Показывает только потоки данных (связи `writes`, `reads`, `queries`). Это самый полезный режим для понимания, какие регистры двигает документ и откуда он берет данные.
