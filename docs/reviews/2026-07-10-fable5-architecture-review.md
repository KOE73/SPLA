# SPLA — независимый архитектурный обзор (2026-07-10, ревизия 2)

Автор: Claude (Fable 5), роль — senior software architect.
Метод: собственная модель системы построена по коду (ядро прочитано целиком: `SPLA.Domain`,
`SPLA.MCP.Core`, `SPLA.Agent`, `SPLA.MCP.BasicTools`, `SPLA.Service` включая новые `Auth/` и
`Observability/`, `SPLA.Server`/`SPLA.CLI`, `SPLA.Observability`), затем сверена с документацией
(`AGENTS.md`, `agents/*`, `docs/roadmap.md`, `docs/readme_auth.md`, `docs/readme_observability.md`).
Плагины — поверхностно. **Ревизия 2**: первая версия этого файла была написана утром; за день в
рабочей копии реализована часть её рекомендаций и добавлены две новые подсистемы (локальная auth,
observability-плоскость) — обзор перепроверен и обновлён под текущее дерево.

Тематические файлы этого обзора:

- **[2026-07-10-fable5-security-review.md](2026-07-10-fable5-security-review.md)** — модель
  разрешений, sandbox/gate, пути эскалации, оценка новой auth-подсистемы, целевая архитектура.
- **[2026-07-10-fable5-code-structure-review.md](2026-07-10-fable5-code-structure-review.md)** —
  god-классы, файлы длиннее 200–300 строк, структура папок.

---

## 1. Модель системы (реконструирована по коду)

SPLA — среда, в которой агент «живёт на проекте» и действует через типизированные инструменты.
Доктрина (задокументирована и реально применяется): интеллект выносится из модели в инструмент;
локальная модель — диспетчер над узкими детерминированными tools.

### Слои

1. **`src/core/SPLA.Domain`** — контракты без внешних зависимостей: host-граница
   (`ISandbox` = `IWorkspace` + `IShell?` + `ICapabilityGate`; реализации пока passthrough),
   ambient-scope'ы на `AsyncLocal` (`AgentSessionScope`, `ToolHostScope`, `ClarifyScope`,
   `ProgressScope`), проектные бэкенды (`IProjectBackend`/`IBucket`, `ServerProjectRoot`),
   каскад настроек (`ConfigLoader` → мутабельный `ResolvedSettings`), identity, secrets.
2. **`src/core/SPLA.MCP.Core`** — хост инструментов: `McpHost` (реестр, permission-enforcement,
   телеметрия), `PermissionManager` (режим × Scope/Effect/Risk + проектные overrides + remembered),
   плагинная система из трёх частей — `PluginDiscovery` (stateless: manifests → descriptors +
   dependency-fixpoint), `PluginAssemblyLoader` (изолированные `PluginLoadContext`),
   `PluginManager` (фасад + runtime-реестр), — `SkillManager`, агентные инструменты.
3. **`src/core/SPLA.Observability`** — `SplaTelemetry` (единые `ActivitySource`/`Meter` «SPLA»,
   ambient `SplaTelemetryContext` для атрибуции user/chat/project) + `Collection/TelemetryCollector`
   (in-process `MeterListener`/`ActivityListener`, per-minute бакеты, per-user агрегаты,
   JSON-персист) — «batteries-included» потребитель тех же сигналов, что уходят в OTLP.
4. **`src/agent/SPLA.Agent`** — единственный агентный цикл `ConversationOrchestrator`
   (контекст → working memory → фильтр tools по режиму → стрим → исполнение tool calls →
   анти-луп guard'ы → rollback по меткам), `SystemPromptBuilder` (сегментная сборка, WYSIWYG),
   `SpawnedAgentRunner`. **`SPLA.MCP.BasicTools`** — fs/shell/build/web-инструменты через
   `HostServices.Sandbox`.
5. **`src/service/SPLA.Service`** — `AgentRuntime` (стек одного проекта), `ChatRuntime`
   (per-chat VM с `_turnGate`), `AgentRuntimeRegistry` (ленивые runtime по projectId),
   `SplaServiceHost` (ASP.NET: `/ws`, три auth-режима, HTTPS c self-signed сертификатом,
   `/stats`-дашборд), `ClientConnection` (транспорт + auth-gate + correlation; бизнес-логика —
   в `Protocol/Handlers/*` через `MessageRouter`), `Auth/` (Negotiate | Local: PBKDF2-store,
   seed-admin, admin-панель, last-admin guard | None), `Observability/` (stats-страница,
   live-push, OTLP-экспорт). **`SPLA.Service.Contracts`** — wire-протокол: `MessageTypes`,
   ~69 DTO, `ProtocolVersion` в `Hello`/`Welcome`.
6. **Хосты**: `SPLA.CLI` (тонкий `Program.cs` + `Cli/` — bootstrap, REPL, chat-команды,
   `ServeCommand`), `SPLA.Server` (доменное/домашнее развёртывание; identity-провайдер —
   DLL по конфигу, не reference), `SPLA.UI.Avalonia` (WebView + дочерний `spla serve` на
   loopback через `EmbeddedServiceLauncher`), `web/` (Vue 3-клиент, встраивается в Service).
7. **Плагины**: Network, Browser (Playwright), Roslyn (`roslyn_script_run` — C#-скрипт дирижирует
   tools через `ctx.Run`), Sql, OneC (+ Avalonia-панели), Skills.Network (скиллы-markdown).

### Ключевые принятые решения

- Один агентный цикл для всех входов; клиенты тонкие, вся власть на стороне агента/сервиса.
- Embedded desktop = клиент поднимает собственный сервис; протокол один для всех.
- Permission-модель декларативная: `Scope/Effect/Risk` × режим × проектная политика × Ask.
- Sandbox — как шов: интерфейсы введены заранее, реализация passthrough.
- Все auth-режимы сходятся в один cookie `spla.auth` → downstream (в т.ч. `/ws`) одинаков.
- Телеметрия: hot path пишет в стандартные .NET-примитивы; потребители (локальный дашборд,
  OTLP) подключаются снаружи и независимо.
- Скиллы — данные; bulk-данные — по handle (DataChannel/blobs), не через окно модели.
- Документация «STOP-файлами» с директивными `AGENTS.md` по слоям; протокол защищён
  тестом-линтером (`ProtocolDocTests`).

### Дельта внутри 2026-07-10 (что сделано после первой ревизии обзора)

Проверено по рабочей копии — за день закрыты пункты первой ревизии:

- **`PluginManager` декомпозирован** на `PluginDiscovery`/`PluginAssemblyLoader`/фасад (был
  god-класс на 370 строк с пятью ответственностями; сейчас 214 + два независимо тестируемых
  компонента). Сделано ровно по предложенному плану.
- **CLI `Program.cs` распилен**: 344 строки top-level → 37 строк диспетчера + `Cli/`
  (`CliBootstrap`, `ServeCommand`, `ChatCommands`, `InteractiveRepl`, `ConsoleHandlers`).
- **Версия протокола добавлена**: `ProtocolVersion` в `Hello`/`Welcome` (долг №8 первой ревизии —
  наполовину закрыт; генерации TS-типов по-прежнему нет).
- **`SplaServiceHost` частично разобран** (`ConfigureAuthentication`, `WireRuntimeEvents`,
  `HandleWebSocketAsync` извлечены), но файл при этом вырос до 517 строк за счёт новых фич.

И добавлены две новые подсистемы (в первой ревизии их не было):

- **`--auth local`**: JSON-store пользователей, PBKDF2-хэши, seed-admin со случайным паролем,
  self-registration, admin-панель `/admin` c last-admin guard, self-service `/account`.
  Качество реализации хорошее (open-redirect guard, HTML-экранирование, честные 401/403 для API);
  замечания — в security-обзоре.
- **Observability-плоскость**: `/stats`-дашборд (KPI, sparkline'ы по минутным бакетам,
  live-feed по server-push), per-user срез для не-админов, JSON-персист бакетов,
  опциональный OTLP-экспорт (`--otlp-endpoint`). Подключено как слушатель — hot path не тронут.

**Не исправлено** (и потому остаётся ядром отчёта): серверный режим без исполнительной изоляции,
произвольный projectId, самодекларация Scope плагинами, строковый контракт результата
инструмента, потеря tool-linkage при персисте чата, вечный кэш runtime'ов, мёртвые loop-guard'ы.

---

## 2. Независимая оценка

### 2.1 Что сделано хорошо

1. **Единый агентный цикл** (`ConversationOrchestrator`, 302 строки, когезивный) — CLI/Server/UI
   реально стоят на одном стеке. Токен-учёт, progress-tree и rollback — в ядре, не в UI.
2. **Ambient-scope'ы на `AsyncLocal`** — per-chat контекст доходит до инструментов без изменения
   сигнатур; параллельные чаты изолированы. `ToolHostScope` даёт скриптам (`ctx.Run`) те же
   permissions и progress, что и прямым вызовам; `SplaTelemetryContext` тем же приёмом даёт
   per-user атрибуцию метрик без тегов на каждом call-site.
3. **Швы до механизмов**: `ISandbox`/`ICapabilityGate`/`IProjectBackend`/`IIdentityProvider`
   введены и покрыты seam-тестами до появления серверных реализаций. `SPLA.Server` не имеет
   platform-reference — Windows-identity загружается как DLL по конфигу.
4. **Сервисный слой**: `ClientConnection` (448 строк) владеет только транспортом/auth-gate/
   correlation; новый тип сообщения = новый handler через `MessageRouter`, ядро не трогается.
5. **Плагинная система после декомпозиции** — образец: discovery чист над FS+settings и
   тестируем без DLL; загрузка изолирована в `PluginLoadContext` с явным списком общих сборок;
   состояния зависимостей считаются fixpoint'ом и честно показываются пользователю.
6. **Наблюдаемость встроена правильно**: ядро пишет в стандартные примитивы, потребители
   (локальный `/stats` и OTLP) слушают снаружи; «over time» переживает рестарт; per-user срез
   уважает роль зрителя. Это редкая для проекта такого размера зрелость.
7. **Документация как контракт**: направленные `AGENTS.md` по слоям, «STOP-доки»,
   протокол-линтер (`ProtocolDocTests`), политика переводов; `agents/security.md` описывает
   *действительное* поведение, включая честные «Caveats» о дырах.
8. **Слоистый system prompt с сегментами** — «что видишь, то и отправлено»; правила
   непротиворечивости промпта (`sys_prompt_rules.md`).
9. **Скорость закрытия долгов**: значительная часть рекомендаций утренней ревизии реализована
   в тот же день, и реализована по предложенным планам, без срезания углов.

### 2.2 Где риски / что хрупко

**Главный системный риск — сервер опередил sandbox.** Многопользовательский доступ (Negotiate,
теперь ещё и Local с self-registration по умолчанию) реализован, а исполнение по-прежнему
passthrough: [ChatRuntime.cs:112](../../src/agent/SPLA.Runtime/ChatRuntime.cs) создаёт
`AgentSession` без параметра sandbox → `PassthroughSandbox.Default` (`AllowAllGate` + весь диск +
`powershell.exe` от сервисного аккаунта). Появление `--auth local` **расширило** аудиторию этого
риска: теперь на сервер попадают и не-доменные пользователи, причём self-registration включена
по умолчанию. `docs/readme_auth.md` честно предупреждает («run the server only where every
authenticated user is trusted»), но предупреждение — не контроль. Детали и целевая модель —
в [security-обзоре](2026-07-10-fable5-security-review.md).

**Хрупкие контракты (ломаются молча):**

- **Результат инструмента — строка**, признак ошибки — эвристика
  [ConversationOrchestrator.cs:215](../../src/agent/SPLA.Agent/ConversationOrchestrator.cs):
  `StartsWith("Error:") || Contains("exception")`. Файл с C#-кодом, прочитанный `fs_read`,
  «выглядит ошибкой»; ошибка в другом формате — успехом. Permission-отказы тоже кодируются
  строкой `"Error: …"`, неотличимой для модели от сбоя инструмента.
- **Анти-луп guard'ы мертвы**: `EnableLoopGuard = false` по умолчанию
  ([ConversationOrchestrator.cs:33](../../src/agent/SPLA.Agent/ConversationOrchestrator.cs)) и
  **ни один вызов в репозитории не включает его** (grep: только объявление и проверка). Реально
  работает лишь text-repeat guard. Для локальной слабой модели зацикливание на инструментах —
  основной режим отказа; защита от него написана, оттестирована и... выключена.
- **Потеря tool-linkage при персисте**: `ChatSessionMessage` хранит role/content/reasoning/images,
  но не `ToolCalls`/`ToolCallId` ([ChatRuntime.Save](../../src/agent/SPLA.Runtime/ChatRuntime.cs)).
  После рестарта возобновлённый чат «забывает», что делал инструментами — тихая деградация
  ключевой фичи «чат переживает рестарт» для любого агентного сценария.
- **TS-типы протокола дублируются вручную** (`web/src/protocol/types.ts`, 274 строки).
  `ProtocolVersion` теперь ловит мажорное расхождение, но не спасает от тихого drift'а полей.

**Жизненный цикл ресурсов:**

- `AgentRuntimeRegistry` кэширует runtime каждого открытого проекта навсегда (`GetOrAdd` без
  выселения). На сервере каждый пользователь при первом коннекте создаёт runtime (HttpClient,
  плагины, промпт, health-check) — неограниченный рост.
- `PluginLoadContext(isCollectible: false)` + перезагружаемый `LoadPlugins` → накопление
  load-контекстов при reload (декомпозиция плагинной системы сделала фикс дешёвым, но сам
  флаг не изменён).
- `LocalShell`: kill tree при отмене есть, но **нет таймаута и лимита объёма вывода** —
  зависшая команда держит turn бесконечно, мегабайтный stdout читается в память целиком.
- `ChatRuntime.Save()` каждый ход переписывает весь YAML чата — на длинных чатах O(n²)
  суммарной записи.

### 2.3 Что плохо масштабируется

- **`ResolvedSettings` — мутабельный god-object**: его live-ссылку читают `PermissionManager`,
  `PluginManager`, `ChatRuntime`, промпт-билдер; правки летят через `agent.save` откуда угодно.
  Любое изменение формы настроек трогает все слои. Roadmap D.7 (`IProject.GetStore<T>()`) —
  правильное лекарство.
- **System prompt заморожен на старте runtime**
  ([AgentRuntime.cs:136](../../src/service/SPLA.Service/AgentRuntime.cs)): смена
  скиллов/плагинов/инструкций требует пересоздания runtime. При этом `SpawnedAgentRunner`
  собирает промпт свежим на каждый spawn (тем же билдером) — поведение основного чата и
  sub-агента расходится.
- **PowerShell-only `LocalShell`** — блокирует Linux-сервер, который identity-слой и доки уже
  подразумевают.
- **Remembered-разрешения**: матч по точному JSON аргументов почти никогда не повторится, кроме
  `*`, который слишком широк; хранение на уровне runtime = «запомнить» действует на все чаты
  проекта (на shared-сервере — на всех пользователей).

### 2.4 5–10 изменений с наибольшим эффектом

1. **Per-user sandbox на сервере** (минимум: `Shell=null` + workspace, ограниченный user-area) —
   закрывает главный риск, шов готов.
2. **Валидация projectId по user-area** в `ClientConnection.Resolve` + тот же резолвер для
   `/chat-image`/`/plugin-assets` — одна точка, закрывает доступ к чужим проектам.
3. **Включить loop-guard'ы** (одна строка в `ChatRuntime`/`SpawnedAgentRunner` + настройка) —
   дешевле не бывает, а защищает основной режим отказа локальной модели.
4. **Типизированный `ToolResult`** (`Ok`/`Content`/`ErrorKind`) вместо строковой эвристики —
   предусловие и честных loop-guard'ов, и аудита, и UI-отображения отказов.
5. **Персист `tool_calls`/`tool_call_id` в чат-YAML** — «чат переживает рестарт» начинает
   работать для агентных сценариев.
6. **Запрет `Scope∈{Agent,Skill}` плагинным инструментам** при регистрации в `McpHost` —
   пара строк, закрывает худший вектор самодекларации.
7. **Таймаут + output-cap в `LocalShell`**; collectible `PluginLoadContext`.
8. **Дефолт нового проекта → `Research`** (сейчас `Edit`,
   [ConfigLoader.cs:59](../../src/core/SPLA.Domain/Settings/ConfigLoader.cs)).
9. **`IProject.GetStore<T>()`** (roadmap D.7) — распил `ResolvedSettings` до того, как серверный
   бэкенд зафиксирует его форму.
10. **Генерация TS-типов из `Payloads.cs`** — убирает ручной дубль протокола.

---

## 3. Соответствие кода и документации

Зона сильная: документация в этом репозитории — рабочий инструмент, а не витрина.

**Соответствует коду (выборочно проверено):**
- `agents/security.md` — матрица режимов совпадает с `PermissionManager` ветка-в-ветку; раздел
  «Caveats» честно фиксирует мёртвую Danger-ветку и unconditional-Allow `Scope=Agent`.
- `agents/structure.md` — слоёный `src/` описан точно, включая `MessageRouter`/handlers и
  двойную skills-copy механику Debug vs Publish (задокументирована как «known fragility» — честно).
- `agents/protocol.md` — защищён `ProtocolDocTests` (константа без доки роняет тест) — образцовый
  механизм против drift'а.
- `docs/readme_auth.md` и `docs/readme_observability.md` — **новые доки написаны в один день с
  кодом** и соответствуют ему (проверено по `AuthEndpoints`/`LocalAccountService`/
  `TelemetryCollector`/`StatsEndpoints`), включая честный абзац «Execution isolation … not yet».
- `docs/roadmap.md` — статусы («СДЕЛАНО») соответствуют коду.

**Найденные расхождения (мелкие):**
1. [agents/security.md:4](../../agents/security.md) ссылается на
   `src/core/SPLA.MCP.Core/Permissions/PermissionManager.cs` — фактический путь
   `src/core/SPLA.MCP.Core/...` (артефакт реорганизации src/). До сих пор не поправлено.
2. `SPLA.Plugins.Test` существует с `.csproj` и `meta.yaml`, но не включён в `SPLA.slnx` —
   «полуживой» артефакт, задокументированный, но не решённый.
3. `docs/TODO*.md` — шесть поколений TODO-файлов с пересекающимся содержимым; какие актуальны —
   из репозитория не выводится.
4. `agents/structure.md` не упоминает новые `SPLA.Service/Auth/`, `Observability/` и
   `SPLA.CLI/Cli/` (появились сегодня — обновить при коммите).

**Системный вывод**: механизм «док защищён тестом» доказал себя на протоколе — распространить
на permission-матрицу (тест, сверяющий `security.md` с `PermissionManager`) и на список
проектов в `structure.md` (сверка с `SPLA.slnx`).

---

## 4. Code quality — 10 важнейших технических долгов

Косметика исключена; сортировка по severity × стоимости промедления.

| # | Долг | Severity | Где |
|---|------|----------|-----|
| 1 | Сервер без исполнительной изоляции (sandbox не пробрасывается в чат); аудитория расширена `--auth local` + self-registration | **Critical** | `ChatRuntime`, `SplaServiceHost`, `PassthroughSandbox`, `AllowAllGate` |
| 2 | Произвольный projectId → чужие проекты и ресурсы (`Resolve`, `ProjectOpen`, `/chat-image?project=`) | **High** | `ClientConnection.Resolve`, `Protocol/Handlers/ProjectHandlers`, `SplaServiceHost` |
| 3 | Самодекларация `Scope/Effect/Risk` инструментами; `Scope=Agent` = always-allow; `Risk=Danger` не присвоен никому → Ask-ветка Agent-режима мертва | **High** | `PermissionManager`, `McpHost.RegisterTool`, `IMcpTool`, все плагины |
| 4 | Строковый контракт результата инструмента + эвристика ошибок | **High** | `IMcpTool`, `McpHost`, `ConversationOrchestrator` |
| 5 | Потеря `ToolCalls`/`ToolCallId` при персисте чата | **High** | `ChatSessionMessage`, `ChatRuntime.Save/ctor` |
| 6 | Loop-guard'ы выключены по умолчанию и не включаются нигде | **Medium-High** | `ConversationOrchestrator.EnableLoopGuard`, `ChatRuntime`, `SpawnedAgentRunner` |
| 7 | `ResolvedSettings` — мутабельный god-object, live-ссылки во всех слоях | **Medium-High** | `SPLA.Domain/Settings`, `PermissionManager`, `PluginManager`, промпт |
| 8 | Жизненный цикл: registry без выселения; non-collectible ALC при reload; shell без таймаута/лимита вывода; O(n²) персист чата | **Medium** | `AgentRuntimeRegistry`, `PluginLoadContext`, `LocalShell`, `ChatRuntime.Save` |
| 9 | Промпт заморожен на старте runtime; основной чат и spawn ведут себя по-разному | **Medium** | `AgentRuntime`, `SystemPromptBuilder`, `SpawnedAgentRunner` |
| 10 | Небезопасные дефолты: режим `Edit` для нового проекта; PFX-пароль-константа `"spla"`; plaintext-секреты; HTTP по умолчанию при cookie-auth; нет rate-limit на `/login` | **Medium** | `ConfigLoader`, `ServiceOptions`, `FileSecretStore`, `AuthEndpoints` |

### Детали и способы исправления

**1. Сервер без sandbox (Critical).** Любой аутентифицированный пользователь в режиме Agent =
выполнение команд от сервисного аккаунта и чтение/запись всего диска. Шов готов — `AgentSession`
принимает `ISandbox`. Минимальный фикс до HardGate: в серверном режиме строить per-user
`PassthroughSandbox(workspace: под user-area, shell: null, gate: WorkspaceGate(userArea))` и
передавать через `ChatRuntime`. `WorkspaceGate` — roadmap B.4.

**2. Произвольный projectId (High).** `ProjectHandlers.Open` и `ClientConnection.Resolve` делают
`registry.Open(любой путь .spla)` без сверки с `_userArea`; `/chat-image?project=` — то же самое
анонимно по HTTP (и вдобавок *строит runtime* для произвольного пути — вектор исчерпания
ресурсов). Фикс: единая canonical-prefix-проверка против user-area в `Resolve` (одна точка) +
`/chat-image`/`/plugin-assets` через тот же резолвер; в local-режиме поведение не меняется.

**3. Самодекларация классификации (High).** Плагинный DLL может объявить `Scope=Agent` и
исполняться всегда, включая Chat. Фикс в два шага: (а) `McpHost.RegisterTool` отклоняет
`Scope∈{Agent,Skill}` для инструментов из плагинов (плагин опознаётся по префиксу имени,
конвенция enforced `ToolNameConventionTests`); (б) капабилити-заявка в `meta.yaml`, ядро
валидирует фактический Scope против заявки, UI показывает заявку пользователю.

**4. Типизированный ToolResult (High).** `record ToolResult(bool Ok, string Content,
ToolErrorKind? Error)`; `IMcpTool.ExecuteAsync` возвращает его; `McpHost` мапит permission-deny
в типизированный отказ; loop-guard ведётся по `Ok`. В LLM по-прежнему уходит строка — миграция
механическая, wire-совместимость не меняется.

**5. Персист tool-linkage (High).** Добавить в `ChatSessionMessage` поля `tool_calls`
(id/name/arguments) и `tool_call_id`; восстанавливать в конструкторе `ChatRuntime`. Без этого
«возобновляемый чат» работает только для разговоров без инструментов.

**6. Мёртвые loop-guard'ы (Medium-High).** `HasToolCallLoop`/`HasErrorLoop` написаны и покрыты,
но `EnableLoopGuard` никто не выставляет. Включить в `ChatRuntime`/`SpawnedAgentRunner`
(настройка в `.spla` при желании). Честная работа error-guard'а требует долга №4 — до тех пор
включать только tool-call-guard.

**7. `ResolvedSettings` (Medium-High).** Реализовать roadmap D.7: типизированные аспекты
(`PermissionProfile`, `LLMProviderConfig`, `AppearanceSettings`) через `IProject.GetStore<T>()`;
`ResolvedSettings` — фасад чтения. Предусловие и серверного бэкенда, и вменяемой permission-модели.

**8. Жизненный цикл (Medium).** Registry: idle-TTL + `Dispose` выселяемых runtime.
`PluginLoadContext`: `isCollectible: true` + `Unload()` в `LoadPlugins` (после декомпозиции —
локальная правка в `PluginAssemblyLoader`). `LocalShell`: настраиваемый таймаут + cap на объём
вывода (хвост — в blob через DataChannel, механика уже есть). Персист чата — append-only или
дифф вместо полной перезаписи.

**9. Один путь жизни промпта (Medium).** Промпт собирать per-chat (или инвалидировать по
событиям Skill/Plugin/Settings через уже существующий `ServiceEvents`) — тогда основной чат и
spawn ведут себя одинаково.

**10. Небезопасные дефолты (Medium).** Новый проект — `Research`, не `Edit`. PFX без
встроенного пароля-константы. Секреты — DPAPI на Windows (позже — ОС-агностичный провайдер).
`/login` — lockout/rate-limit (см. security-обзор). Cookie: `SecurePolicy=Always` при `--https`.

---

## 5. God-классы, длинные файлы, структура папок — выжимка

Полная версия: [2026-07-10-fable5-code-structure-review.md](2026-07-10-fable5-code-structure-review.md).

Утренние №1 и №3 (PluginManager, CLI Program.cs) **закрыты за день**. Осталось:
`SplaServiceHost` (517 строк — вырос: auth-режимы + HTTPS/cert + stats-wiring в одном файле;
`Build` всё ещё ~150 строк) — извлечь `Endpoints`/`StatsPlane`; `Payloads.cs` (646 строк,
~69 DTO) — разбить по модулям протокола; `AuthPages.cs` (265 строк C#-строк с HTML/CSS/JS
трёх страниц) — вынести разметку в embedded-ресурсы по образцу `main_sys_prompt.md`.
Папки: `MCP.Core/Tools` (19 файлов), `Network/Tools` (24), `tests/` (33, плоско) — подпапки;
`SPLA.Service` корень постепенно разгружается (`Auth/`, `Observability/`, `Protocol/` уже
выделены) — осталось `Runtime/` и `Ops/`.

---

## 6. Roadmap улучшения архитектуры (3 этапа)

Согласован с `docs/roadmap.md`; расставляет приоритеты, не заменяет его.

### Этап 1 — Quick wins (дни)

1. Валидация projectId по user-area в `ClientConnection.Resolve` + `/chat-image` (#2).
2. Per-user sandbox в серверном режиме: `Shell=null` + workspace под user-area (#1, минимум).
3. Включить tool-call loop-guard (#6).
4. Запрет `Scope=Agent/Skill` для плагинных инструментов в `McpHost.RegisterTool` (#3, шаг а).
5. Персист `tool_calls`/`tool_call_id` в чат-YAML (#5).
6. Таймаут + output-cap в `LocalShell`; collectible `PluginLoadContext` (#8, частично).
7. Дефолтный режим нового проекта → `Research`; убрать PFX-пароль-константу; lockout на `/login`;
   `SecurePolicy` для cookie (#10).
8. Починить путь в `agents/security.md:4`; дописать `Auth/`/`Observability/`/`Cli/` в
   `structure.md`; решить судьбу `SPLA.Plugins.Test`; консолидировать `docs/TODO*.md` (§3).
9. Разбить `Payloads.cs` по модулям; подпапки в `MCP.Core/Tools` и `tests/` (риск ~0).

### Этап 2 — Medium refactors (недели)

1. `WorkspaceGate` (roadmap B.4) + полная сборка per-user sandbox; обязателен на сервере,
   опционален локально (#1 целиком). Подключить `Gate.CanRead/CanWrite` к fs-инструментам.
2. Типизированный `ToolResult` через `IMcpTool`/`IToolHost`/оркестратор (#4); после него —
   включить error-loop-guard (#6 целиком).
3. `IProject.GetStore<T>()` и распил `ResolvedSettings` (#7; roadmap D.7).
4. Единый жизненный цикл system prompt с инвалидацией по событиям (#9).
5. Idle-eviction в `AgentRuntimeRegistry`; лимит одновременных turn'ов per-user (#8).
6. Генерация TS-типов из `Payloads.cs`; тест-линтер permission-матрицы (§3).
7. Извлечение `Endpoints`/`StatsPlane` из `SplaServiceHost`; HTML auth-страниц — в ресурсы.
8. Remembered-разрешения: паттерны по значимым полям аргументов вместо exact-JSON/`*`;
   скоуп — chat, не runtime.

### Этап 3 — Strategic changes (месяцы)

1. **HardGate / OS-изоляция shell** (Windows: Job Objects + restricted token; Linux: ns+seccomp) —
   после этого shell возвращается на сервер. Контракт с SoftGate уже спроектирован.
2. **Манифестная capability-модель плагинов** (#3, шаг б): заявка в `meta.yaml`, валидация ядром,
   показ пользователю; далее — подпись манифестов.
3. **Share-ACL по группам** поверх user-area-валидации; permission-профили per-identity
   (сейчас `Capabilities.Full` для всех аутентифицированных).
4. **Кросс-платформенный `IShell`** (pwsh/bash по конфигу) — предусловие Linux-сервера.
5. **Серверный backend хранения** (`ChatManager` → bucket KV, затем БД/объектное хранилище
   за `IProjectBackend`); инкрементальный персист чата.
6. **Аудит-лог permission-решений** — вырастает из единой точки gate'а Этапа 2; observability-
   плоскость (`/stats`) — готовое место для его отображения.

---

## 7. Итоговая оценка

Ядро зрелое и дисциплинированное, и проект демонстрирует лучшую из возможных динамик: рекомендации
утренней ревизии этого же обзора к вечеру уже реализованы (декомпозиция плагинной системы, распил
CLI, версия протокола), параллельно добавлены две качественно сделанные подсистемы (локальная
auth, observability). Но главный риск за день **вырос, а не сократился**: появление `--auth local`
с включённой по умолчанию self-registration расширяет круг тех, кто получает доступ к агенту,
исполняющемуся без всякой изоляции от имени сервисного аккаунта. Пока `ChatRuntime` не
пробрасывает per-user sandbox, любое расширение auth-поверхности — это масштабирование RCE, и
никакая аккуратность permission-матрицы это не компенсирует (Deny — строка для модели, не
enforcement). Приоритет №1 не изменился: пункты 1–2 Этапа 1 — до любого нового функционала
серверного режима. Второй фронт — строковые контракты (ToolResult, tool-linkage, ручные TS-типы):
не горят, но каждый месяц промедления удорожает миграцию.
