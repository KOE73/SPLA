# Интеграция с SPLA

`SPLA.Plugins.OneC` — самодостаточный DLL-плагин: анализ-tools для агента + web-панель «1C
Configuration Browser».

## Как подключается

1.  Плагин указан в `SPLA.slnx` и собирается в `plugins/onec/` рядом с каждым хостом (CLI, UI,
    Service) — см. target `CopyPlugin` в `SPLA.Plugins.OneC.csproj`.
2.  `meta.yaml` объявляет `entry_point: SPLA.Plugins.OneC.dll` и `web_settings_entry: web/dist/settings.js`.
3.  `OneCPlugin.Initialize` открывает индекс `onec.sqlite` (в runtime-каталоге проекта, историческое
    расположение `.spla/onec.sqlite`) и регистрирует агент-tools из `Tools/`.
4.  `OneCPlugin` реализует `ISplaPluginAction`: web-панель дергает backend через канал `plugin.action`
    (actions: `overview`, `search`, `object`, `graph`, `formatters`, `format`, `rebuild`).

## Разделение слоёв

*   **Данные/логика** (`Storage/`, `Indexing/`, `Graph/`, `Models/`, `Context/`, `Web/`) —
    UI-независимы, без графических зависимостей.
*   **Презентация** — web (`web/`, Vue + Cytoscape), грузится host'ом в рантайме и не импортируется в
    сборку плагина.

> Ранее презентация жила в `SPLA.Plugins.OneC.Avalonia` поверх `SPLA.Plugins.Host.Avalonia`
> (`IAvaloniaPlugin`, `NativeWebView`). Оба проекта удалены. Незакрытые задачи миграции — в `TODO.md`.
