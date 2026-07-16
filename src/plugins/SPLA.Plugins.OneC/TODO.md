# SPLA.Plugins.OneC — TODO (миграция Avalonia → web)

Панель `SPLA.Plugins.OneC.Avalonia` («1C Configuration Browser») перенесена в web. Ниже — что
уже сделано, что осталось доделать и что проверить на реальной конфигурации 1С (дома не тестировалось).

## Что сделано

- **Backend-действия** (`OneCPlugin : ISplaPluginAction`, `Web/OneCBrowserActions.cs`), вызываются
  из web через канал `plugin.action` (`pluginId: "onec"`):
  - `overview` — счётчики (объекты/связи/секции) + дерево иерархии `owns`.
  - `search` — поиск по подстроке (FullName + Name), до 80 результатов.
  - `object` — карточка объекта.
  - `graph` — узлы/рёбра для Cytoscape (режимы `dependencies` / `references` / `dataflow`, depth, limit).
  - `formatters` / `format` — экспорт контекста (FullName, Object card YAML, Graph summary, Selected relations).
  - `rebuild` — переиндексация каталога-дампа конфигурации 1С.
- **Перенесён UI-независимый код** из Avalonia в сам плагин:
  - `Graph/CytoscapeGraphAdapter.cs` (ToJson + ToHtmlDocument).
  - `Context/OneCContextFormatters.cs` (форматтеры экспорта контекста).
- **Web-бандл** `web/` (Vue 3 + Vite, как у SQL/SSH): `BrowserPanel.vue` + `TreeNode.vue`,
  сборка в `web/dist/settings.js`. Объявлен в `meta.yaml` как `web_settings_entry`.
- **Удалены** проекты `SPLA.Plugins.OneC.Avalonia` и `SPLA.Plugins.Host.Avalonia` (у Host.Avalonia
  не осталось потребителей), убраны из `SPLA.slnx`.
- **Починен `csproj`**: пути деплоя плагина исправлены со `..\SPLA.CLI` на `..\..\apps\SPLA.CLI`
  (та же ошибка, что была описана в SQL-плагине), добавлен копир `web/dist`.

## Осталось доделать

1. **Полноценная dock-панель.** Сейчас браузер открывается через `web_settings_entry`, т.е. в
   `Settings → Plugins → 1C`. Чтобы вернуть поведение Avalonia (отдельная панель в тулбаре
   воркспейса), нужен host-код в `web/`:
   - новый surface-компонент (напр. `web/src/surfaces/OneC/OneCBrowser.vue`, можно просто
     реэкспортировать логику из плагинного бандла или загрузить его так же, как `PluginWebSettings.vue`);
   - регистрация в `web/src/surfaces/registry.ts`, `web/src/dock/panelCatalog.ts` (новый `PanelKind`
     `"onec"`, добавить в `toolKinds`), иконка/заголовок;
   - кнопка открытия в тулбаре. Осторожно: dock-система хрупкая (см. заметки про регрессии заголовков
     и per-chat состояние) — тестировать открытие/закрытие/singleton.
2. **CSS web-бандла.** В lib-режиме Vite кладёт стили отдельным файлом `web/dist/onec-web.css`, а
   `PluginWebSettings.vue` подгружает только `settings.js` — стили сейчас могут не примениться
   (та же особенность у SQL-плагина). Варианты: инлайнить критические стили в компонент через JS,
   либо чтобы host подтягивал соседний `*.css`. Проверить визуально.
3. **Cytoscape по CDN.** `BrowserPanel.vue` грузит cytoscape с cdnjs. При офлайне / строгом CSP граф
   не отрисуется. Зашить cytoscape локально в бандл (`npm i cytoscape` + import) — тогда `ToHtmlDocument`
   в адаптере тоже стоит перевести на локальную копию (сейчас там `<script src=cdn>`).
4. **Insert into prompt.** В Avalonia была кнопка Insert (`Interaction.InsertIntoPrompt`). В контракте
   `MountApi` (`getYaml` + `invoke`) такого канала нет — оставлен только Copy (через
   `navigator.clipboard`). Добавить в host API метод вставки в композер или послать сообщение через RPC.
5. **Прогресс переиндексации.** Avalonia стримил прогресс (`IProgress<string>`). Web-действие
   `rebuild` пока возвращает только итоговый отчёт. Для живого прогресса нужен стрим-канал
   (напр. `tool.progress`-подобные события или отдельный `invoke`-поллинг).
6. **Путь дампа для rebuild.** Действие `rebuild` требует путь к каталогу-дампу (как и tool
   `onec_build_index`). Возможно, стоит запоминать последний путь в настройках плагина
   (`plugins.onec.settings`) и подставлять по умолчанию.
7. **Агент-tool `onec_open_graph`.** В Avalonia был tool, открывавший окно с графом (`OneCGraphOpenTool`).
   Он удалён вместе с проектом: в web «открыть окно» из tool'а нечем. Если нужно, чтобы агент мог
   попросить UI открыть граф по объекту — добавить событие/канал host → web (напр. broadcast, который
   web-панель слушает) и вернуть tool в web-плагин.
8. **Тема.** Компонент использует CSS-переменные с fallback-цветами. Свести палитру с темами host'а
   (см. как это делает SQL/SSH settings), чтобы граф и панель совпадали со светлой/тёмной темой.

## Проверить на реальной конфигурации 1С

- Собрать индекс (`rebuild` или tool `onec_build_index`) на настоящем дампе выгрузки конфигурации.
- Дерево `owns`: корректность корневых секций и вложенности (формы, табличные части, модули).
- Все три режима графа на «тяжёлом» объекте (напр. документ реализации) — размеры/усечение (limit).
- Экспорт контекста всеми форматтерами → Copy.
