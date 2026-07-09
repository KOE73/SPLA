# SPLA — независимый архитектурный обзор (2026-07-09)

Автор: Claude (Fable 5), роль — senior software architect.
Метод: сначала построена собственная модель системы по коду (ядро прочитано полностью:
`SPLA.Agent`, `SPLA.Service`, `SPLA.Server`, `SPLA.CLI`, `SPLA.MCP.Core`, `SPLA.Domain/Host|Project|Settings|Identity|Secrets`,
`SPLA.MCP.BasicTools`), затем сверена с документацией (`AGENTS.md`, `agents/*`, `docs/roadmap.md`,
`docs/host-abstraction-plan.md`). Плагины смотрел поверхностно (декларации инструментов, манифесты).

Тематические файлы этого обзора:

- **[2026-07-09-fable5-security-review.md](2026-07-09-fable5-security-review.md)** — модель разрешений,
  sandbox/gate, пути эскалации, целевая архитектура безопасности.
- **[2026-07-09-fable5-docs-and-structure-review.md](2026-07-09-fable5-docs-and-structure-review.md)** —
  соответствие кода и документации + предложение по перекладке репозитория во вложенные папки
  с направленными `AGENTS.md`.

---

## 1. Моя модель системы (что это и как устроено)

SPLA — локально-ориентированная среда, в которой агент «живёт на проекте» и действует через
типизированные инструменты. Ставка (задокументирована в Doctrine): интеллект выносится из модели
в инструмент; слабая локальная модель выступает диспетчером над узкими детерминированными tools.

### Слои (снизу вверх)

1. **`SPLA.Domain`** — контракты и чистые правила. Ключевое:
   - *Host-граница*: `ISandbox` = `IWorkspace` (файлы, логические пути) + `IShell` (выполнение) +
     `ICapabilityGate` (политика). Сейчас везде живут passthrough-реализации
     (`LocalWorkspace`, `LocalShell` через PowerShell, `AllowAllGate`).
   - *Ambient-scope'ы на `AsyncLocal`*: `AgentSessionScope` (state чата: KV, blobs, checkpoint,
     sandbox), `ToolHostScope`, `ClarifyScope`, `ProgressScope` (+ `PermissionScope` в MCP.Core).
     Это транспорт per-chat контекста до инструментов без изменения сигнатур.
   - *Проектное хранилище*: `IProjectBackend`/`IBucket` (файловый и in-memory бэкенды),
     `ServerProjectRoot` (per-user области `{root}/users/{sid}/`).
   - *Настройки*: каскад `defaults.yaml` (~/.spla) → `*.spla` → per-chat секции; результат —
     мутабельный `ResolvedSettings`.
   - Identity (нейтральные контракты + reflection-загрузка платформенного провайдера), Secrets
     (`secret:`/`env:`-резолвер, plaintext YAML store).

2. **`SPLA.Agent`** — единственный агентный цикл. `ConversationOrchestrator.RunAsync`:
   сборка контекста (`ContextAssembler` — retention-политики, ephemeral, суперсид по ключу) →
   инъекция working memory → фильтр инструментов по режиму (`ToolModeFilter`) → стрим LLM →
   исполнение tool calls → анти-луп guard'ы → rollback по чекпойнтам/меткам. Плюс
   `SystemPromptBuilder` (сегментная сборка промпта, WYSIWYG-превью) и `SpawnedAgentRunner`
   (headless суб-агент для скиллов).

3. **`SPLA.MCP.Core`** — хост инструментов. `McpHost` (реестр, исполнение, телеметрия,
   permission-enforcement), `PermissionManager` (режим × Scope/Effect/Risk + проектные overrides +
   session-remembered решения), `PluginManager` (DLL-плагины через `AssemblyLoadContext` с общими
   контрактными сборками), `SkillManager` (скиллы = markdown-данные), агентные инструменты
   (память, скиллы, spawn, marks, data-channel blobs).

4. **`SPLA.MCP.BasicTools`** — fs/shell/build/web-инструменты, ходят в систему только через
   `HostServices.Sandbox` (шов под серверную изоляцию).

5. **`SPLA.Service`** — «сервисное» ядро: `AgentRuntime` (стек одного проекта: LLM-клиент, McpHost
   со всеми tools, плагины, скиллы, промпт, KV), `ChatRuntime` (per-chat VM: conversation, session-KV,
   checkpoint, orchestrator, `_turnGate`), `AgentRuntimeRegistry` (ленивые runtime'ы по projectId),
   `SplaServiceHost` (ASP.NET + WebSocket `/ws`, встроенные web-ассеты, Negotiate→cookie auth),
   `ClientConnection` (диспетчер wire-протокола, 837 строк switch).

6. **Хосты/клиенты** — все над одним протоколом:
   - `SPLA.CLI` — REPL и `spla serve` (loopback или сеть + token).
   - `SPLA.Server` — доменное развёртывание: NTLM/Kerberos, per-user области, identity-DLL по конфигу.
   - `SPLA.UI.Avalonia` — теперь тонкая оболочка: WebView + `EmbeddedServiceLauncher`, который
     поднимает дочерний `SPLA.CLI serve` на loopback. Нативного chat-UI больше нет.
   - `web/` — Vue-клиент (chat, settings, workspace-редакторы, project picker).

7. **Плагины** — Network (диагностика сети), Browser (Playwright), Roslyn (compile-check +
   `roslyn_script_run` — исполнение C#-скрипта, дирижирующего другими tools через `ctx.Run`),
   Sql, OneC (+ Avalonia-панель), Skills.Network (скиллы-данные).

### Принятые архитектурные решения (реконструированы по коду)

- Один агентный цикл для всех входов (UI/CLI/Server) — дублирование уже устранено.
- Клиенты тонкие; вся власть (permissions, secrets, память) — на стороне агента/сервиса.
- «Embedded = клиент поднимает свой сервис» — desktop и remote неразличимы для клиента.
- Permission-модель декларативная: инструмент декларирует `Scope/Effect/Risk`, режим агента задаёт
  матрицу, проектная политика — жёсткий пол/потолок, интерактивный Ask через `PermissionScope`.
- Sandbox — как шов: интерфейсы введены заранее, реализация пока passthrough (осознанная Фаза 0,
  зафиксировано в `docs/host-abstraction-plan.md` и `docs/roadmap.md`).
- Скиллы — данные (markdown), не код; индекс в промпте, тело — по запросу.
- Bulk-данные — по handle (`DataChannel`/blobs), а не через окно модели.

---

## 2. Что сделано хорошо

1. **Единый агентный цикл** (`ConversationOrchestrator`) вместо трёх копий — правильная и уже
   завершённая консолидация; CLI/Server/UI действительно стоят на одном стеке (`AgentRuntime`/
   `ChatRuntime`). Это редко доводят до конца — здесь доведено.
2. **Ambient-scope'ы на `AsyncLocal`** — элегантное решение проблемы «инструменту нужен per-chat
   контекст, но сигнатура tool фиксирована». Параллельные чаты изолированы без глобального state.
3. **Швы до механизмов**: `ISandbox`/`IWorkspace`/`ICapabilityGate`, `IProjectBackend`/`IBucket`,
   `IIdentityProvider` введены и покрыты тестами (`HostSandboxSeamTests`, `ServerBackendSeamTests`)
   до того, как понадобилась серверная реализация. Архитектурная дисциплина выше средней.
4. **Тонкий клиент над одним протоколом** — desktop (WebView+дочерний serve), браузер и удалённый
   сервер используют один код. Стратегически сильно: сервер получен почти бесплатно.
5. **Доктрина как фильтр решений** (`docs/Doctrine.*.md`, преамбула AGENTS.md) — явная система
   ценностей для отбора фич. Видно, что она реально применяется (узкие схемы tools, data-channel,
   `agent_info` вместо раздувания промпта).
6. **Слоистый system prompt с сегментами** (`PromptSegment`) — «что видишь, то и отправлено», плюс
   правила непротиворечивости (`agents/sys_prompt_rules.md`). Зрелый подход к промпту как к артефакту.
7. **Скиллы как данные + lifecycle** — отделение процедурного знания от кода, hot reload, гейтинг
   активации через permission-модель.
8. **Плагинная изоляция** через `AssemblyLoadContext` с явным списком общих сборок — правильный
   механизм для .NET; deps-резолвер, зависимость плагинов друг от друга, состояния
   (DisabledByDependency/LoadError) — аккуратно.
9. **Наблюдаемость встроена в ядро**: телеметрия/логгирование в `McpHost` и оркестраторе, учёт
   токенов в цикле (не в UI), progress-tree для вложенных вызовов.
10. **Roadmap честный**: `docs/roadmap.md` сам называет главные дыры (AllowAllGate, HardGate,
    ACL шаринга, квоты). Самодиагностика проекта совпадает с моей внешней оценкой — это хороший знак.

---

## 3. Риски и хрупкие места

Полные детали по безопасности — в [security-обзоре](2026-07-09-fable5-security-review.md).
Здесь — сводка всех классов рисков.

### 3.1 Безопасность (главный риск: сервер опередил sandbox)

Серверный режим (Negotiate, per-user области) уже смёржен, но исполнительная изоляция — нет:
- `ChatRuntime` создаёт `AgentSession` **без sandbox** → все серверные чаты работают через
  `PassthroughSandbox.Default` (`AllowAllGate` + реальный диск + PowerShell от имени сервисного
  аккаунта). Любой доменный пользователь в режиме Agent = выполнение команд на сервере.
- `ClientConnection.Resolve`/`ProjectOpen` принимает **произвольный путь манифеста** как projectId
  без проверки принадлежности области пользователя → доступ к чужим проектам.
- Token-auth в `AuthGate` проверяется **только в обработчике Hello**; остальные типы сообщений
  диспетчеризуются без проверки, что Hello вообще был → token-режим (`serve --bind 0.0.0.0 --token X`)
  фактически не защищает.
- Permission-модель доверяет метаданным, которые инструмент декларирует о себе сам
  (`Scope=Agent` → always-allow в любом режиме, включая Chat).

### 3.2 Хрупкий контракт результата инструмента

Результат tool — «просто строка», а признак ошибки — эвристика в оркестраторе
([ConversationOrchestrator.cs:209](../../src/agent/SPLA.Agent/ConversationOrchestrator.cs)):
`StartsWith("Error:") || Contains("exception")`. Инструмент, вернувший текст со словом «exception»
(например, содержимое файла с C#-кодом), ложно считается упавшим для error-loop-guard; инструмент,
вернувший ошибку в другом формате, — успешным. На этой же строковой конвенции построены permission-отказы
(«Error: … denied…» уходит модели как результат). Нужен структурированный `ToolResult
{ Ok, Payload, ErrorKind }` на уровне `IMcpTool`/`IToolHost`, строка — только на границе с LLM.

### 3.3 Потеря tool-linkage при персисте чата

`ChatSessionMessage` ([ChatSession.cs:51](../../src/core/SPLA.Domain/Models/ChatSession.cs)) хранит только
role/content/reasoning/images — **`ToolCalls` и `ToolCallId` не сохраняются**. После перезапуска
`ChatRuntime` восстанавливает историю, где tool-сообщения потеряли `ToolCallId`, и
`ContextAssembler.ShouldSend` молча выбрасывает их как «orphan tool results», а assistant-ходы,
состоявшие только из tool calls, становятся пустыми и тоже выпадают. Итог: возобновлённый чат видит
«дырявую» историю — модель теряет знание о том, что она делала инструментами. Это тихая деградация,
которую пользователь заметит как «после перезапуска агент забыл, что уже сделал».

### 3.4 Жизненный цикл runtime'ов и ресурсов

- `AgentRuntimeRegistry` кэширует runtime каждого когда-либо открытого проекта навсегда
  (`GetOrAdd`, без выселения). На многопользовательском сервере это неограниченный рост: каждый
  пользователь при первом коннекте создаёт runtime (LLM-клиент, плагины, промпт, KV). Нужна
  политика выселения/idle-dispose.
- `LocalShell.RunAsync`: отмена CancellationToken прерывает `WaitForExitAsync`, но **не убивает
  процесс** — осиротевшие PowerShell'ы. Нет таймаута и лимита объёма вывода (весь stdout читается
  в память; `output='blob'` — опция модели, а не защита хоста).
- `PluginLoadContext` создаётся `isCollectible: false`, а `LoadPlugins` умеет перезагружаться —
  повторная загрузка накапливает контексты.

### 3.5 Что плохо масштабируется

- **`ClientConnection.DispatchAsync`** — 837-строчный switch по 40+ типам сообщений, всё в одном
  классе вместе с framing, авторизацией, correlation и broadcast-логикой. Каждый новый surface
  (schema, fs, plugins, debug) расширяет его. Это уже сейчас самое горячее место мержей; при росте
  протокола станет неподдерживаемым. Напрашивается таблица хендлеров
  (`Dictionary<string, Func<Envelope, Task>>` по модулям-Ops) + middleware (auth, resolve, логирование).
- **`ResolvedSettings` как мутабельный god-object**: его читают PermissionManager (live-ссылка),
  PluginManager, ChatRuntime, промпт-билдер; правки идут через `agent.save` откуда угодно. Изменение
  формы настроек затрагивает всё. Roadmap-пункт D.7 (`IProject.GetStore<T>()`) — правильное лекарство,
  его стоит поднять в приоритете: это предусловие и серверного бэкенда, и вменяемой permission-модели.
- **Протокол без версии**: 76 констант `MessageTypes`, на JS-стороне — мягкие строки. Ни версии
  в handshake, ни codegen типов для TS (типы в `web/src/protocol/types.ts` дублируются вручную).
  При «desktop клиент v1 ↔ сервер v2» ломаться будет молча.
- **Один `SystemPrompt` на runtime, собранный при старте** ([AgentRuntime.cs:136](../../src/service/SPLA.Service/AgentRuntime.cs)):
  смена скиллов/плагинов/инструкций требует пересоздания runtime; per-chat активный скилл при этом
  собирается отдельным путём (`SpawnedAgentRunner` строит свой промпт). Два пути сборки промпта —
  расхождение уже заложено.
- **PowerShell-only shell** (`LocalShell` жёстко `powershell.exe`) — блокирует Linux-сервер, который
  документация и identity-слой уже обещают.

### 3.6 Разное (заметное, но не критичное)

- `PermissionManager._rememberedPermissions` живёт на уровне runtime → «запомнить разрешение»
  в одном чате действует на все чаты проекта (а на сервере с shared-проектом — на всех
  пользователей проекта).
- Сравнение remembered-разрешения по **точному совпадению JSON аргументов** — почти никогда не
  сработает повторно (любой другой путь/пробел), кроме `*`, который наоборот слишком широк.
  Нужны паттерны по значимым полям (как permission-rules в Claude Code).
- `SecretResolver.Resolve` — sync-over-async (`GetAwaiter().GetResult()`).
- `ConnectionHub.Broadcast*` — fire-and-forget `_ =` в событиях; ошибки фан-аута глотаются.
- Дефолтный режим нового пользователя — `Edit` (см. `ConfigLoader.LoadDefaults`), при том что
  README позиционирует Chat/Research как безопасные ступени. Для сервера дефолт должен быть ниже.

---

## 4. Security and permissions — краткая выжимка

(полная версия: [2026-07-09-fable5-security-review.md](2026-07-09-fable5-security-review.md))

Направление выбрано верно (двухосевая модель «права × источники», gate как policy, sandbox как
механизм), но сейчас в системе **четыре несогласованных слоя проверок**:
`ToolModeFilter` (что видит модель) → `PermissionManager` (Allow/Ask/Deny) → `ICapabilityGate`
(вызывается только shell-инструментами) → `WorkspaceOps.IsUnderRoot` (только для web-редактора).
Они дают разные ответы на один и тот же вопрос. Целевая картина: **одно решение принимает gate
на уровне ядра по нормализованному действию** (read path / write path / execute / network host),
а mode/permission-слой лишь formирует политику gate'а. Классификация инструмента должна
подтверждаться ядром/манифестом плагина, а не самодекларацией tool-класса.

Приоритетные дыры (по убыванию): (1) отсутствие sandbox на сервере при уже включённом доменном
доступе; (2) обход token-auth мимо Hello; (3) произвольный projectId → чужие проекты; (4)
`Scope=Agent`-доверие к плагинам; (5) auto-allow шелла в Agent-режиме (Risk=High не спрашивает,
Danger не присваивается ничему); (6) отсутствие Origin-проверки на cookie-аутентифицированном
WebSocket (cross-site WebSocket hijacking); (7) секреты plaintext + `--token`/пароль PFX в argv.

---

## 5. Соответствие кода и документации — краткая выжимка

(полная версия: [2026-07-09-fable5-docs-and-structure-review.md](2026-07-09-fable5-docs-and-structure-review.md))

- **Актуальны и качественны**: `docs/roadmap.md`, `docs/host-abstraction-plan.md`, `agents/skills.md`,
  `agents/data-ownership.md`, Doctrine.
- **Существенно устарели**: `agents/structure.md` (нет SPLA.Service/Server/web/Browser/Sql/
  Editor.Schema/Identity — т.е. половины системы), `agents/chat-messages.md` (описывает удалённый
  нативный Avalonia-chat), `agents/security.md` (матрица не совпадает с `PermissionManager`:
  «Agent спрашивает high-risk shell» — в коде High = auto-allow; «Research: чтение Ask first» —
  в коде Allow), `agents/protocol.md` (заявлен как реестр всех MessageTypes; в коде 76 констант,
  в доке отсутствуют ~30: весь Fs*, Project*, Connection*, PluginAction, Schema*).
- Систематическая причина: документы-«STOP» не привязаны к коду ничем, кроме дисциплины.
  Лечится тестом-линтером (например, `ProtocolDocTests`: каждый `MessageTypes.*` упомянут в
  protocol.md) и переносом нормативных таблиц (permission-матрица) в генерируемые из кода.

---

## 6. Code quality — 10 важнейших технических долгов

Отсортировано по убыванию (severity × стоимость промедления). Косметика исключена.

| # | Долг | Severity | Где |
|---|------|----------|-----|
| 1 | Сервер без исполнительной изоляции | **Critical** | `ChatRuntime`, `PassthroughSandbox`, `AllowAllGate` |
| 2 | Token-auth обходится мимо Hello | **Critical** | `ClientConnection`, `AuthGate` |
| 3 | Произвольный projectId → чужие проекты | **High** | `ClientConnection.Resolve`, `AgentRuntimeRegistry.Open` |
| 4 | Самодекларация Scope/Effect/Risk инструментами | **High** | `PermissionManager`, `IMcpTool`, все плагины |
| 5 | Строковый контракт результата инструмента | **High** | `IMcpTool`, `McpHost`, `ConversationOrchestrator` |
| 6 | Потеря ToolCalls/ToolCallId при персисте чата | **High** | `ChatSessionMessage`, `ChatRuntime.Save` |
| 7 | `ClientConnection` — протокольный монолит | **Medium-High** | `SPLA.Service/ClientConnection.cs` |
| 8 | `ResolvedSettings` — мутабельный god-object | **Medium-High** | `SPLA.Domain/Settings`, все слои |
| 9 | Жизненный цикл: registry без выселения, shell без kill/timeout, non-collectible ALC | **Medium** | `AgentRuntimeRegistry`, `LocalShell`, `PluginManager` |
| 10 | Протокол без версии и без общей типизации с TS | **Medium** | `SPLA.Service.Contracts`, `web/src/protocol` |

### Детали и способы исправления

**1. Сервер без sandbox (Critical).**
Риск: любой аутентифицированный доменный пользователь получает чтение/запись всего диска сервера и
выполнение команд от сервисного аккаунта — это RCE «по дизайну текущей фазы», но фаза уже выставлена
в домен. Исправление (минимум, до HardGate): в `SplaServiceHost`/`ChatRuntime` собирать per-user
`AgentSession` с `PassthroughSandbox(workspace: bounded, shell: null, gate: WorkspaceGate(userArea))`;
`WorkspaceGate` уже запланирован в roadmap B.4 — реализовать и включать **обязательно в серверном
режиме** (локально можно оставить AllowAll по умолчанию). Shell на сервере — `null` до появления
OS-изоляции.

**2. Обход token-auth (Critical).**
Риск: `spla serve --bind 0.0.0.0 --token X` не защищает ничего — `Authorize` вызывается только в
`HandleHelloAsync`, а `ChatSend`/`FsWrite`/etc. обрабатываются без него. Исправление: флаг
`_authenticated` на соединении; при `RequiresToken` любой не-Hello до успешного Hello → закрытие
сокета. Одновременно это точка для будущих capability-проверок per-message.

**3. Cross-user доступ к проектам (High).**
`ProjectOpen`/`Resolve` открывают любой путь `.spla`. Исправление: в серверном режиме валидация
`projectId` против `_userArea` (canonical path prefix) + share-ACL (roadmap 8b) в одной точке —
`ClientConnection.Resolve`. Плюс `/chat-image` endpoint должен резолвить проект с той же проверкой.

**4. Доверие к самодекларации инструментов (High).**
Плагинный DLL может объявить `Scope=Agent` и исполняться всегда, в т.ч. в Chat-режиме. Исправление:
классификация — в манифесте плагина (`meta.yaml`: заявленные capability-классы) + валидация ядром
при регистрации (`McpHost.RegisterTool`): встроенные тулзы — белый список; плагинные — максимум
заявленного в манифесте, `Scope=Agent`/`Skill` плагинам запрещены. Подробнее в security-обзоре.

**5. Строковый ToolResult (High).**
Исправление: `record ToolResult(bool Ok, string Content, ToolErrorKind? Error)`;
`IMcpTool.ExecuteAsync` возвращает его; `McpHost` мапит permission-deny в типизированный отказ;
оркестратор ведёт error-loop-guard по `Ok`, а не по подстрокам. Миграция механическая (32 базовых
инструмента + плагины), совместимость с LLM не меняется (в сообщение по-прежнему уходит строка).

**6. Персист tool-linkage (High).**
Добавить в `ChatSessionMessage` поля `tool_calls` (id, name, arguments) и `tool_call_id`; в
`ChatRuntime.Save/ctor` сериализовать/восстанавливать. Иначе вся конструкция «чат переживает
перезапуск» работает только для болтовни без инструментов.

**7. Разбор `ClientConnection` (Medium-High).**
Вынести: framing/receive-loop; auth-middleware; резолв проекта; таблицу хендлеров по модулям
(ChatOps, ProjectOps, FsOps, SettingsOps уже частично есть — довести до конца, switch заменить
реестром). Критерий: добавление нового типа сообщения не трогает `ClientConnection`.

**8. `ResolvedSettings` (Medium-High).**
Реализовать roadmap D.7: типизированные аспекты (`PermissionProfile`, `LLMProviderConfig`,
`AppearanceSettings`) через `IProject.GetStore<T>()`; `ResolvedSettings` становится фасадом чтения.
Это предусловие серверного бэкенда и нормальной permission-модели — тянуть дальше дорого.

**9. Жизненный цикл (Medium).**
Registry: idle-TTL + Dispose runtime'ов (сейчас `Dispose` только на shutdown). `LocalShell`:
`try { … } finally { if (!exited) process.Kill(entireProcessTree: true); }` + настраиваемый таймаут
+ cap на объём вывода. `PluginLoadContext`: либо collectible + выгрузка при reload, либо запрет
повторного `LoadPlugins`.

**10. Протокол (Medium).**
Версия в `Hello/Welcome` + минимальный генератор TS-типов из `Payloads.cs` (или JSON-schema).
Плюс тест «каждая константа `MessageTypes` задокументирована в protocol.md» — закрывает и docs-drift.

---

## 7. Roadmap улучшения архитектуры (3 этапа)

Согласован с существующим `docs/roadmap.md` — не заменяет его, а расставляет акценты.

### Этап 1 — Quick wins (дни; без изменения формы системы)

1. Закрыть обход token-auth (#2) — десятки строк.
2. Валидация projectId по user-area в серверном режиме (#3, без share-ACL).
3. Отключить shell в серверном режиме (`Shell = null` в per-user sandbox) до появления изоляции.
4. Валидация `Scope` при регистрации инструментов: плагинам запретить `Agent`/`Skill` (#4, первый шаг).
5. Kill+timeout+output-cap в `LocalShell` (#9).
6. Персист `ToolCalls`/`ToolCallId` в чат-YAML (#6).
7. Origin-check на `/ws` при cookie-auth; убрать дефолтный пароль PFX «spla».
8. Синхронизировать `agents/security.md`, `agents/structure.md`, `agents/protocol.md` с кодом;
   добавить тест-линтер протокола (#10, doc-часть).

### Этап 2 — Medium refactors (недели)

1. `WorkspaceGate` (roadmap B.4) + сборка per-user sandbox в `ChatRuntime`/`SplaServiceHost` (#1) —
   включён по умолчанию на сервере, опционален локально.
2. Типизированный `ToolResult` через `IMcpTool`/`IToolHost`/оркестратор (#5).
3. Декомпозиция `ClientConnection`: middleware + реестр хендлеров (#7).
4. `IProject.GetStore<T>()` и распил `ResolvedSettings` (#8; roadmap D.7).
5. Единый путь сборки system prompt (per-chat, с инвалидацией по событиям Skill/Plugin/Settings)
   вместо «раз при старте + отдельный путь в SpawnedAgentRunner».
6. Idle-eviction в `AgentRuntimeRegistry`; квоты на одновременные turn'ы per-user.
7. Версия протокола + генерация TS-типов (#10).
8. Перекладка репозитория во вложенные папки с направленными AGENTS.md (см. тематический файл) —
   осмысленно делать здесь, после декомпозиции, одним движением.

### Этап 3 — Strategic changes (месяцы)

1. **HardGate / OS-изоляция shell** (Windows Job Objects + restricted token; Linux ns+seccomp) —
   после этого shell можно вернуть на сервер. Единый контракт с SoftGate (уже спроектировано).
2. **Share-ACL по группам** (roadmap 8b) поверх валидации #3; permission-профили per-identity
   (сейчас `Capabilities.Full` для всех).
3. **Кросс-платформенный `IShell`** (bash/pwsh по конфигу) — предусловие Linux-сервера.
4. **`ChatManager` → bucket KV** (roadmap G) и далее серверный backend (БД/S3) за `IProjectBackend`.
5. **Manifest-based capability-модель плагинов**: подписанные/проверяемые манифесты с заявкой
   capabilities; UI показывает пользователю, что плагин просит; ядро enforce'ит максимум.
6. **Session/permission-модель уровня протокола**: per-message capability-чек (сейчас только
   Ask-прокси до клиента), аудит-лог решений — вырастает из точки, созданной в Этапе 1.1.

---

## 8. Итоговая оценка

Ядро спроектировано зрело: единый цикл, ambient-scope'ы, заранее вырезанные швы, честный roadmap.
Главный системный риск — **несинхронность фаз**: многопользовательский сетевой доступ (Server,
Negotiate, per-user области) уже реализован и выглядит «безопасным», а исполнительная изоляция
осталась на фазе «passthrough, локально доверяем». Это классическая ловушка: интерфейсы безопасности
есть, ощущение безопасности есть, enforcement — нет. Второй по важности риск — строковые контракты
(результаты инструментов, категории permission-overrides, wire-протокол на JS-стороне), которые
работают, пока проект маленький, и начнут молча ломаться при росте. Оба риска лечатся тем, что уже
запланировано в roadmap — вопрос только в приоритете: пункты B.4/E.9 стоит поднять выше UI-задач,
пока сервер не разошёлся по домену.
