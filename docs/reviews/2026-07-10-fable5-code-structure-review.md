# SPLA — god-классы, длинные файлы, структура (2026-07-10, ревизия 2)

Тематическое приложение к [общему обзору](2026-07-10-fable5-architecture-review.md).
Фокус: god-классы (осмысленность + план разбиения), файлы >200–300 строк (partial/извлечения по
логическим границам), структура папок. Ревизия 2 учитывает, что за день закрыты два из трёх
god-классов первой ревизии и добавлены новые файлы (auth, observability).

Метод: `wc -l` по всем `.cs`/`.ts`/`.vue`/`.axaml` вне `bin/obj/node_modules`, ручной разбор
ответственностей крупнейших файлов. Плагины — по размеру.

---

## 1. God-классы

### 1.1 Закрыто за день (было в первой ревизии)

- **`PluginManager`** (был 370 строк, 5 ответственностей) — **декомпозирован ровно по плану**:
  `PluginDiscovery` (stateless: директории+meta.yaml → `List<PluginDescriptor>` + dependency-
  fixpoint, тестируем без DLL), `PluginAssemblyLoader` (рефлексия + `PluginLoadContext`,
  владеет ALC-lifetime), `PluginManager` (214 строк — тонкий фасад + runtime-реестр). Образцовое
  исполнение: теперь discovery тестируется без файловой системы, а collectible-ALC (долг №8) —
  локальная правка в `PluginAssemblyLoader`, не трогающая discovery.
- **`SPLA.CLI/Program.cs`** (было 344 строки top-level) — **распилен**: 37 строк диспетчера +
  `Cli/` (`CliBootstrap`, `ServeCommand`, `ChatCommands`, `InteractiveRepl`, `ConsoleHandlers`).

### 1.2 `SplaServiceHost` (517 строк) — **осталось; вырос за счёт новых фич**

[src/service/SPLA.Service/Hosting/SplaServiceHost.cs](../../src/service/SPLA.Service/Hosting/SplaServiceHost.cs).
Частично разобран (извлечены `ConfigureAuthentication`, `WireRuntimeEvents`, `HandleWebSocketAsync`,
`LoadOrCreateCertificate`, `IsSameHostOrigin`), но за день добавились три auth-режима, HTTPS/cert-
генерация и wiring stats-плоскости — файл вырос. `Build` всё ещё ~150 строк и мешает несколько тем.
План:
- `ServiceEndpoints.Map(app, registry, ...)` — health/login/assets/chat-image/plugin-assets
  (сейчас 5+ инлайновых `MapGet` внутри `Build`).
- `StatsPlane.Wire(app, options, hub)` — collector + gaugeTimer + StatsHub + StatsEndpoints
  (сейчас ~20 строк инлайн в `Build`).
- `Build` оставить как оркестратор: builder → auth → kestrel → endpoints → stats → ws.
Риск низкий; `ServiceOptions` (record, 60 строк доков) — оставить как есть, это хорошая
самодокументированная точка конфигурации.

### 1.3 Пограничные (не god, под наблюдением)

- `PermissionManager` (174) — длинный `CheckPermission`, но **одна** ответственность и линейная
  матрица. Трогать только вместе с целевой моделью безопасности, не ради длины.
- `McpHost` (267) — реестр + исполнение + телеметрия + help. Когезивен; `GetToolHelp`
  (форматирование YAML-подобного вывода, ~70 строк) можно вынести в `ToolHelpFormatter`.
- `ConversationOrchestrator` (302) — единственный агентный цикл, намеренно цельный. Не делить.
- `ClientConnection` (448) — уже транспорт-only; оставшееся — связная тема (framing+auth+
  correlation+turn-plumbing). Дальнейшее дробление даст мало.
- `LocalAccountService` (224) — новый; одна тема (политика учёток над `IUserStore`), когезивен,
  трогать не нужно.
- `TelemetryCollector` (255) — новый; один тип (in-process потребитель сигналов), когезивен.

---

## 2. Файлы длиннее 200–300 строк — план по логическим границам

| Файл | Строк | Вердикт | Действие |
|------|-------|---------|----------|
| `SPLA.Service.Contracts/Payloads.cs` | 646 | **делить** | ~69 DTO в одном файле → разбить по модулям (`Payloads.Chat.cs`, `.Project.cs`, `.Settings.cs`, `.Workspace.cs`, `.Connection.cs`). Плоский bag DTO; partial по темам. Риск ~0. |
| `SplaServiceHost.cs` | 517 | **частично** | См. §1.2 — извлечь `ServiceEndpoints`/`StatsPlane`. |
| `LMStudio/LMStudioClient.cs` | 461 | **частично** | Уже `partial` (DTO вынесены). Остаётся дублирующаяся SSE-парс-логика в 3 методах — вынести `OpenAiStreamParser`; клиент станет ~250 строк. |
| `SPLA.CLI/Program.cs` | ~~344~~ 37 | ✅ сделано | Распилен в `Cli/`. |
| `Plugins/PluginManager.cs` | ~~370~~ 214 | ✅ сделано | Декомпозирован (§1.1). |
| `OneC/Storage/OneCIndexDatabase.cs` | 371 | наблюдать | Плагин; SQLite-доступ, когезивно. |
| `OneC.Avalonia/OneCOverviewPanel.axaml.cs` | 355 | наблюдать | UI code-behind плагина; вне фокуса ядра. |
| `Editor.Schema/WorkspaceShellView.axaml.cs` | 314 | наблюдать | UI code-behind; проверить, нет ли доменной логики (правило `agents/avalonia.md`) — если есть, в VM. |
| `Domain/Settings/ConfigLoader.cs` | 269 | **частично** | Каскад + scaffolding + FindProjectFile. Вынести `ConfigScaffolder` и `ProjectFileLocator` — останется чистый резолвер каскада. |
| `Auth/AuthPages.cs` | 265 | **частично** | HTML/CSS/JS трёх страниц в C#-строках. Вынести разметку в embedded-ресурсы по образцу `main_sys_prompt.md` — C# останется тонким шаблонизатором. Логика (экранирование) уже корректна. |
| `Observability/Collection/TelemetryCollector.cs` | 255 | наблюдать | Новый, когезивен (§1.3). Персист (`Save`/`TryLoad`) можно вынести в `StatsPersistence` при росте. |
| `Auth/AuthPages.cs`, `Auth/LocalAccountService.cs` (224) | 200+ | наблюдать | Новые auth-файлы; когезивны, кроме HTML-разметки выше. |
| `SPLA.Service.Contracts/Protocol.cs` | 222 | наблюдать | Реестр констант протокола; плоско по природе. |
| `ClientConnection.cs` | 448 | наблюдать | См. §1.3. |

**Приоритет действий**: `Payloads.cs` (тривиально, высокая читаемость), `SplaServiceHost`
(§1.2, тестируемость), `AuthPages` (разметка в ресурсы), `LMStudioClient` (устранить дубль SSE),
`ConfigLoader` (изоляция scaffolding). Остальные — «наблюдать».

---

## 3. Структура папок

Общая раскладка `src/` по слоям с направленными `AGENTS.md` — сильная. За день структура
улучшилась: `SPLA.CLI/Cli/`, `SPLA.Service/Auth/`, `SPLA.Service/Observability/`,
`SPLA.Observability/Collection/`, `MCP.Core/Plugins/` (после декомпозиции) — новые логические
подпапки. Остаточные точки:

### 3.1 `src/core/SPLA.MCP.Core/Tools` — 19 файлов плоско → **подпапки**

Смешаны семейства: память (`AgentMemory*` — 6 файлов), скиллы (`Skill*`, `AgentSpawn*`),
чекпойнты/метки (`ContextCheckpoint*`, `Mark*`), data-channel (`DataChannel`, `ImageView`, `KvGlob`),
help (`AgentInfo`). Предложение:
```
Tools/Memory/      AgentMemory{Set,Get,List,Delete,Clear,Helpers}
Tools/Skills/      Skill{Activate,Deactivate}, AgentSpawn{,Batch}, AgentClarify
Tools/Checkpoints/ ContextCheckpoint{Set,Restore}, Mark{Set,Rollback}
Tools/DataChannel/ DataChannel, ImageView, KvGlob
Tools/             AgentInfo (help-роутер, остаётся в корне)
```

### 3.2 `src/plugins/SPLA.Plugins.Network/Tools` — 24 файла плоско → **подпапки**

Группы: DNS, HTTP, scan (`LanScan`, `PortScan`, `PortCheck`, `Arp`), probe (`Tcp/Udp/Smtp/Ssl`),
ping/route. Разложить в `Tools/{Dns,Http,Scan,Probe,Diagnostics}/`; `NetworkScanHelpers` — в корень.

### 3.3 `tests/SPLA.Tests` — 33 файла плоско → **подпапки по слою**

Сгруппировать зеркально коду: `Core/` (Permission, Context, KeyValue, Blob, Progress, Identity,
Skill*, Telemetry), `Agent/` (Orchestrator, WorkingMemory, SystemPrompt, Spawn, Clarify),
`Service/` (MultiProject, ProjectBroker/Provider, AgentRuntimeRegistry, ProtocolDoc, LocalAccount),
`Plugins/`, `Tools/` (Search*, ToolNameConvention). Проверить наличие шаблонного `UnitTest1.cs`
и удалить. Появились `LocalAccountServiceTests`, `TelemetryCollectorTests` — тем нужнее группировка.

### 3.4 `src/service/SPLA.Service` — корень разгружается → **закончить**

`Protocol/`, `Auth/`, `Observability/` уже выделены. В корне ещё соседствуют runtime (`AgentRuntime`,
`ChatRuntime`, `ChatRegistry`, `AgentRuntimeRegistry`) и ops (`SettingsOps`, `WorkspaceOps`,
`SchemaOps`, `SystemOps`, `ConnectionDiagOps`, `ChatImages`). Предложение: `Runtime/`, `Ops/`;
оставить транспорт/host (`ClientConnection`, `SplaServiceHost`, `ConnectionHub`, `AuthGate`,
`MessageRouter`) в корне.

### 3.5 Норм — трогать не нужно

`SPLA.Domain` разложен по темам (`Agent/`, `Host/`, `Project/`, `Settings/`, `Editor/`,
`Identity/`, `Secrets/`, `Models/`, `Tools/`, `Context/`, `Interfaces/`) — эталон.
`SPLA.MCP.BasicTools` (`FileSystem/`, `Network/`, `System/` + `Search/`) — хорошо.
`web/src` (`surfaces/`, `state/`, `protocol/`, `composables/`, `layouts/`, `util/`) — хорошо.

---

## 4. Сводный план (порядок по «выгода / риск»)

1. **Тривиально, риск ~0**: разбить `Payloads.cs` по модулям; подпапки в `MCP.Core/Tools`,
   `Network/Tools`, `tests/`; проверить/удалить `UnitTest1.cs`.
2. **Низкий риск, высокая выгода**: извлечь `ServiceEndpoints`/`StatsPlane` из `SplaServiceHost`;
   HTML auth-страниц в embedded-ресурсы; `ConfigScaffolder`/`ProjectFileLocator` из `ConfigLoader`.
3. **Средний риск**: `OpenAiStreamParser` из `LMStudioClient`; collectible ALC в
   `PluginAssemblyLoader` (долг №8 — теперь локальная правка после декомпозиции); `Runtime/`+`Ops/`
   группировка в `SPLA.Service`.
4. **Только вместе с профильным рефактором**: `PermissionManager` (с целевой моделью безопасности);
   `McpHost.GetToolHelp` → `ToolHelpFormatter` (при следующем касании help-системы).

Всё в §1–3 — структурные улучшения без изменения поведения; можно вести параллельно с
приоритетными архитектурными долгами (sandbox, projectId, ToolResult) — не конфликтуют.
