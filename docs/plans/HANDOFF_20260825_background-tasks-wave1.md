# HANDOFF 2026-08-25 — фон инструмента, волна 1, продолжение в новом чате

Ветка `work`. Ничего не закоммичено — все изменения в рабочей копии.

## Текущее состояние (проверено только что)

- `dotnet build SPLA.slnx` — чисто.
- `dotnet test tests/SPLA.Tests` — **1081/1081**.
- `npm run typecheck` (web/) — чисто (`vue-tsc`).
- `npm run test` (web/) — **17/17**.

Всё зелёное. Можно продолжать или коммитить как есть.

## Что сделано в этой сессии

### Волна 0 плана `PLAN_20260824-2_core_background-tool-calls.md` — закрыта целиком
0.1 (ChatRuntime : IDisposable), 0.2 (пер-чатный sandbox/shell), 0.3 (ProgressHub), 0.5
(SupportsBackground в метаданных), 0.6 (ChatInbox + DrainInbox в оркестраторе), 0.7 (протокол
task.list/state/cancel, отвечает пустотой) — все сделаны и покрыты тестами.

0.4 (подписка на прогресс с хода на чат) была **сознательно отложена** в волну 1, а затем **сделана
там же**, см. ниже — с найденным по пути багом.

### Волна 1 — в основном сделана
- **1.1 BackgroundTaskRegistry** — `src/core/SPLA.Domain/Tools/BackgroundTaskRegistry.cs`. Лимит 8
  живых задач/чат, привязка к `_chatLifetime` токену чата.
- **1.2 BackgroundStage** — `src/core/SPLA.MCP.Core/Pipeline/Stages/BackgroundStage.cs`, ступень 550.
  Деградирует в синхронный вызов, если у сессии нет `IBackgroundTaskHost` (суб-агенты — намеренно).
  `PermissionScope`/`ClarifyScope` внутри отсоединённой задачи подменены на авто-отказ.
- **IBackgroundTaskHost** — `src/core/SPLA.Domain/Agent/AgentSessionScope.cs`. `ChatRuntime`
  реализует его сам (Tasks/Progress/Inbox), передаётся в `AgentSession` как `background: this`.
- **1.3 схема** — `McpHost.WithBackgroundParameter`: добавляет nullable `background` в schema только
  инструментам с `SupportsBackground=true`; для `StrictSchema` кладёт в `required` (паттерн cwd/
  code_page). Наружу (MCP-голове) флаг **не** раскрывается (`GetToolDefinitionsFor`).
- **1.4 доставка** — внутри `BackgroundStage.RunDetachedAsync`, формат
  `[Background task bg_N — tool — finished in Xs]`. Отмена/ошибка доставляются так же ("молчание
  хуже ошибки").
- **1.5 инструменты** — `task_list`/`task_output`/`task_cancel`,
  `src/core/SPLA.MCP.Core/Tools/BackgroundTaskTools.cs`, зарегистрированы в `AgentRuntime.cs` как
  `Feature("core.background_tasks", ...)`.
- **1.6 промпт** — `src/agent/SPLA.Agent/Features/CoreBackgroundTasks/prompt.md` +
  `CoreFeaturePrompts.cs` + `AgentFeatureCatalog.cs` (запись `core.background_tasks`).
- **1.7 кому дать SupportsBackground=true** — по прямому решению пользователя:
  `system_run_shell`, `agent_spawn`, `agent_spawn_batch`, `web_fetch`, `ssh_session_exec`.
- **Живая доставка прогресса фоновой задачи** (то, что было отложено как 0.4): подписка теперь
  **на уровне чата**, не на ходе — `ChatRegistry.RuntimeOpened` (новое событие) →
  `SplaServiceHost.WireChatProgress` подписывается на `chat.Progress.NodeChanged` один раз на всю
  жизнь чата и шлёт `progress.node` всем вотчерам. Старая пер-ходовая подписка в
  `ClientConnection.BuildCallbacks` (блок `OnProgressTree = tree => {...}`) **удалена** — иначе было
  бы двойное `progress.node` для обычных ходов.

### Найденный и починенный баг (не в плане, вскрылся по пути)
`ProgressTree` нумерует узлы локально (`n1`, `n2`...) **внутри каждого дерева отдельно**. Как только
появилось два живых дерева на чат одновременно (ход + фоновая задача), их id **коллизируют** на
проводе. Исправлено: на сервере узлы отдаются с префиксом `"{treeId}:{nodeId}"`
(`SplaServiceHost.WireChatProgress`), а `llm.turn.start` теперь несёт `progressTreeId` — новое поле
в `DeltaPayload` (C#) и в `web/src/protocol/types.ts`. Клиент (`chatSessions.ts`) на новом ходу
чистит из `nodes` только узлы **предыдущего** хода (по префиксу), а не всё разом — иначе дерево
фоновой задачи стиралось бы на каждом новом ходу человека.

**Важно про этот фикс**: он **не покрыт .NET/web юнит-тестами на новую логику** (только руками
проверена логика отката к старому поведению, когда `progressTreeId` отсутствует — это и оказался
баг, который поймал существующий тест `chatSessions.test.ts`, я его починил). Юнит-тест именно на
namespaced-id и на "фоновое дерево переживает сброс хода" **не написан** — это первое, что стоит
сделать в новой сессии.

## Что НЕ сделано / открыто

1. **1.8 Тесты** — покрытие есть точечно (BackgroundTaskRegistryTests 14, BackgroundStageTests 13,
   BackgroundSchemaExposureTests 7, ChatInboxTests, ProgressHubTests, DrainInbox-тесты в
   ConversationOrchestratorTests), но список ловушек из плана (§ловушки 1–18) **не пройден
   вручную по каждому пункту** — стоит сверить.
   Специально не хватает: теста на namespaced progress node id (см. выше), теста на то, что
   `task_cancel` реально останавливает `system_run_shell`/`ssh_session_exec` вживую (юнит-тесты на
   отмену есть только на уровне `BackgroundTaskRegistry`/`BackgroundStage` с фейковым инструментом).
2. **1.9 Документы** — `SPLA.slnx` содержит новые файлы? **Не проверено в этой сессии** — обязательно
   свериться (правило `feedback-slnx-sync` из памяти: новые файлы Pipeline/Stages/BackgroundStage.cs,
   Tools/BackgroundTaskTools.cs, Domain/Tools/{BackgroundTaskRegistry,ChatInbox,ProgressHub}.cs,
   Permissions/IToolArgumentPolicy.cs — должны быть в .slnx). `CHANGELOGS/current-log.md` и
   `current-list.md` **не обновлены** под работу этой сессии.
3. **Живая проверка через браузер** — не проверялось в этом заходе (только 0.7 проверялась живым
   WS в прошлой части сессии). Стоит поднять `spla serve` + `spla-web` и реально прогнать фон:
   запустить `system_run_shell` с `background:true`, посмотреть что модель получает
   `task bg_1 started`, что `task_list` его видит, что по завершении в чат падает синтетическое
   сообщение, и что `progress.node` для фоновой задачи реально доезжает до окна и не пропадает на
   следующем ходу.
4. **Волна 2** (пробуждение чата на простое, панель задач в UI) — не начата, это следующий кусок
   плана после волны 1.
5. **Контракт конвейера** (`ADR_20260824-3`) — `Post` (650) и `Timeout` (750) объявлены как места в
   enum, **не реализованы**. `IToolArgumentPolicy` — контракт есть
   (`src/core/SPLA.MCP.Core/Permissions/IToolArgumentPolicy.cs`), **ни одного модуля-реализации нет**
   (SQL-политика и т.п. — отдельная будущая работа, см. `IDEA_20260824-2`).
6. **MCP-клиент** — по решению пользователя, отдельная задача на будущую сессию (см. память
   `mcp-client-next-session.md`), не начата вообще.

## Как продолжить в новом чате

Рекомендуемый порядок:
1. Дописать unit-тест на namespaced progress node id + "фоновое дерево переживает сброс хода"
   (C# `WireChatProgress`-подобный тест невозможен без ConnectionHub-мока — реалистичнее
   TS-тест в `chatSessions.test.ts`: подать `progress.node` с `nodeId: "t1:n1"`, затем
   `llm.turn.start` с другим `progressTreeId`, затем ещё `progress.node` с `nodeId: "t1:n2"` —
   проверить, что `t1:n1` пережил сброс).
2. Живая проверка в браузере (см. п.3 выше) — критично, ни разу не гонялось руками для волны 1.
3. Свериться со `SPLA.slnx` и дописать `CHANGELOGS/`.
4. Пройтись по ловушкам плана (§ловушки 1–18 в `PLAN_20260824-2_core_background-tool-calls.md`) —
   вручную сверить каждую с тем, что реально в коде.
5. Спросить пользователя про коммит — он ничего не коммитил в этой сессии специально.

## Важные заметки о стиле работы в этой сессии

- Пользователь на низком уровне усилий/контекста, просил под конец **не задавать вопросов** и
  доводить до конца самостоятельно, с разумными решениями. Один раз воспользовался
  `AskUserQuestion` до этой просьбы — ответы: SupportsBackground → shell/spawn/web_fetch/ssh
  session-exec; живой прогресс — почини сейчас; после — остановиться и закоммитить (но потом
  попросил продолжать без вопросов, я комбинировал оба указания).
- Дважды пойман на когнитивной перегрузке пользователя ("нихера не понял", "я реально запутался") —
  в такие моменты нужно **резюмировать по-человечески**, без жаргона, что было/делалось/сделано/
  осталось — см. пример ответа в истории на "что у нас было в планах, что делали, что сделали".
