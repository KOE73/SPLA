# Host Abstraction — план (Фаза 0 и далее)

Цель: **МАКСИМАЛЬНО абстрагировать ядро**, чтобы один и тот же агентный цикл работал во всех
сценариях (локально / сервер / embedded) без изменений. Меняется только слой реализаций,
собираемый снаружи. Контракты — в ядре, реализации — снаружи.

Связанные заметки памяти: `host-abstraction-sandbox`, `project-storage-broker`,
`capability-security-model`.

## Принципы

- **Capability injection.** Агент не знает, где запущен. Он знает только интерфейсы. Контекст
  собирается снаружи фабрикой.
- **ISandbox — риск-центр, не архитектурный верх.** Через него проходит всё, что трогает реальную
  систему (файлы, shell, сеть). Память/секреты/LLM уже безопасны по природе.
- **Path — это абстракция**, а не OS-путь. `MapPathToHost` / `MapPathToProject` симметричны.
- **Bypass через shell** закрывается тем, что shell — тоже член ISandbox (через `IShell`/gate),
  а не «инструмент вне системы».

## Слои (кто главный)

```
IAgentContext                 ← архитектурный верх (пока роль играет IAgentSession)
  ISandbox                    ← риск-центр
    IWorkspace                ← файлы (логические пути)
    IShell?                   ← выполнение (null = запрещено)
    ICapabilityGate           ← политика прав (единственная точка решения)
  IKeyValueStore, IBlobStore, ISkillSession, ... (уже есть)
```

## Контракты (SPLA.Domain/Host/)

| Тип | Роль |
|---|---|
| `IWorkspace` | Файлы через логические пути + `MapPathToHost`/`MapPathToProject`. |
| `IShell` | Выполнение команд. `RunAsync(ShellCommand) → ShellResult`. |
| `ICapabilityGate` | `CanRead/CanWrite/CanExecute/CanNetwork` — политика. |
| `ISandbox` | Бандл: `Workspace` + `Shell?` + `Gate`. |

## Реализации по фазам

| Контракт | Фаза 0 (сейчас) | Сервер (позже) | Тесты |
|---|---|---|---|
| `IWorkspace` | `LocalWorkspace` (System.IO, без ограничений — passthrough) | `ServerWorkspace` | `MemoryWorkspace` |
| `IShell` | `LocalShell` (powershell, перенесён из RunCommandTool) | `SandboxedShell` / null | `FakeShell` |
| `ICapabilityGate` | `AllowAllGate` (soft-lite, всё разрешено локально) | `HardGate` (OS-принуждение) | `AllowAllGate` |
| `ISandbox` | `PassthroughSandbox` (+ статический `Default`) | `ProcessSandbox` | ad-hoc |

## Точки инъекции

- `IAgentSession.Sandbox` — новое свойство. `AgentSession` ctor: `ISandbox? sandbox = null`
  → `PassthroughSandbox.Default` (обратная совместимость всех вызовов).
- `HostServices.Sandbox` — статический аксессор для инструментов:
  `AgentSessionScope.Current?.Sandbox ?? PassthroughSandbox.Default`. Вне чата (CLI, тесты)
  падает на passthrough = текущее поведение.

## Миграция инструментов (Фаза 0)

Только представительный набор, названный в памяти. Поведение и схемы НЕ меняются
(значит main_sys_prompt.md трогать не нужно).

- `FsReadTool` → `HostServices.Sandbox.Workspace`
- `FsWriteTool` → то же
- `FsListTool` → то же
- `RunCommandTool` (shell) → `HostServices.Sandbox.Shell` (если `null` — «shell disabled»);
  логика powershell/процесса переезжает в `LocalShell`.

**Фаза 0.1 (сделано):** мутационные fs-инструменты `FsCreateTool`/`FsDeleteTool`/`FsPatchTool`
переведены на `Sandbox.Workspace` (`IWorkspace` расширен `ReadAllTextAsync`/`DeleteFile`) —
write-bypass закрыт полностью.

**Фаза 0.2 (сделано):** ВСЕ оставшиеся инструменты на шве.
- `DotnetBuildTool`/`DotnetTestTool` → `IShell` (были `Process.Start("dotnet")` напрямую —
  execute-bypass; теперь уважают `Gate.CanExecute()` и отключаются вместе с shell).
- `FsFindFilesTool` — обход дерева переписан с `DirectoryInfo` на `IWorkspace.GetFiles`/
  `GetDirectories` (рекурсия по строковому API workspace).
- `FsSearchTextTool` — корень резолвится через `IWorkspace.MapPathToHost`; движки поиска
  (ripgrep/.NET) остаются диск-ориентированными, но виртуальный workspace вернёт `null` →
  «поиск недоступен». Полноценный поиск по виртуальному workspace — отдельная задача (движок
  внутри workspace), но граница «инструмент не лезет на диск сам» уже проведена.

**Итог Фазы 0:** ни один встроенный инструмент не трогает `System.IO`/`Process` напрямую —
всё через `ISandbox`. Весь сьют (158) зелёный.

## Проверка шва

Тест: подменить в scope `MemoryWorkspace`-песочницу, записать `FsWriteTool`, прочитать
`FsReadTool` — данные живут в памяти, не на диске. Доказывает, что инструменты не знают, что за
песочницей.

## Фаза 1 — проект как брокер хранилищ (в работе)

Контракты в `SPLA.Domain/Project/`: `IProject` (GetWorkspace/GetKV/GetSecrets/GetBucket),
`IProjectBackend` (ProjectId + GetBucket, физика хранения), `IBucket` (opaque key/value text +
`MapToHostDirectory()` для диск-зависимых потребителей — та же стадийность, что
`MapPathToHost`). Реализации: `FileBucket` (папка, ленивое создание), `LocalProjectBackend`
(`.spla/` рядом с манифестом, без проекта — `~/.spla`; ЕДИНСТВЕННАЯ точка этого решения),
`LocalProject`.

**Фаза 1.0 (сделано):** контракты + инъекция `ResolvedSettings.Project` (lazy default =
LocalProject, хост может подменить) + миграция ядровых потребителей: `ProjectKvStore` (ctor
принимает baseDir от брокера), `ChatManager` (chats/summaries/backups = бакеты), `AgentRuntime`
(project KV + token-usage через root-бакет). Раскладка на диске НЕ изменилась. Тесты:
`ProjectBrokerTests` (4) + весь сьют 162 зелёный.

**Фаза 1.1 (сделано):** `ChatImages` → бакет "chat-images" (API принимает `IProject`);
`SplaTelemetry.ConfigureProjectLogs` теперь принимает готовую директорию (наблюдаемость
не знает раскладку), вызывающие (CLI/UI) передают бакет "logs". Побочная консистентность:
без проекта картинки/логи теперь падают в `~/.spla` вместе с чатами (раньше — в
`cwd/.spla`, вразнобой с чатами).

**Фаза 1.2 (сделано):** плагины через `settings.Project` при init: OneC sqlite — root-бакет
(исторический `.spla/onec.sqlite`, совпадает с ожиданием Avalonia-панели), Browser profile —
бакет "browser-profile" (`BrowserProfilePaths` и оба инструмента принимают `IProject`).
`AvaloniaPluginContext` НЕ трогали — его сейчас никто не строит (нативный хост панелей
подвешен после пивота к сервису); Avalonia-сторона OneC оставлена на ручной склейке —
пути совпадают, пока открыт проект. `IconGenerator` — не трогаем (читает пользовательский
`.spla/icon.png`, косметика нативного UI).

**Открыто:** `FileSecretStore` кладёт project-секреты рядом с МАНИФЕСТОМ
(`manifestDir/.spla/secrets.yaml`), а runtime-зона брокера — от `WorkspacePath`. При
несовпадении workspace и папки манифеста это разные места. Перенос в бэкенд = миграция
существующих secrets.yaml; решить при выносе секретов в сервис (secrets-store roadmap).

## Фаза 2 — провайдер проектов (в работе)

**Фаза 2.0 (сделано):** `IProjectProvider` (List/Recent/Open/Create) + `ProjectDescriptor` +
`LocalProjectProvider`: реестр `~/.spla/registry.json` (id = путь манифеста, lastOpened),
одноразовый посев из legacy `recent_projects.txt`, пропавшие манифесты фильтруются на чтении
и вычищаются на записи. `Open` регистрирует recency и отдаёт `LocalProject` через обычный
resolve. Тесты: `ProjectProviderTests` (4); сьют 166 зелёный.

**Фаза 2.1 (сделано):** `AgentRuntimeRegistry` в `SPLA.Service/` — реестр `ProjectId → (AgentRuntime,
ChatRegistry)`, ленивая постройка, кэш на весь процесс (повторное открытие того же проекта отдаёт
ТОТ ЖЕ `AgentRuntime` — состояние чатов/KV/health-кеша одно и то же независимо от вызывающего).
Листинг/recency идут через `IProjectProvider` (backend-agnostic); резолв `ResolvedSettings` для
самого `AgentRuntime` — по-прежнему через `ConfigLoader` (агент сейчас неотделим от локальной
модели настроек; серверный бэкенд подменит этот шаг резолва, не форму реестра). CLI `serve`
переведён на реестр (`registry.Open(settings.ProjectFilePath)`) вместо ручного `new AgentRuntime`.
`SPLA.Tests` теперь ссылается на `SPLA.Service` (без цикла). Тесты: `AgentRuntimeRegistryTests`
(3 — одинаковый инстанс при повторном открытии, изоляция разных проектов, стабильный
no-project-сентинел). Сьют 169 зелёный.

**Фаза 2.2 (сделано, вариант Б — принят пользователем):** `ProtocolEnvelope.ProjectId` — на КАЖДОМ
чат-/проект-scoped сообщении; null = проект по умолчанию для соединения
(`AgentRuntimeRegistry.DefaultProjectId`, тот, с которым сервис стартовал — однопроектные клиенты
ничего не меняют). Никакого мутабельного «текущего проекта» на соединении — та же логика, что и
у sessionId транспорта уровнем ниже: меньше состояния, которое может «уехать».

- `ClientConnection` больше не хранит `_runtime`/`_chats` полями — держит `AgentRuntimeRegistry` и
  резолвит `(RuntimeEntry, projectId)` на каждое сообщение через приватный `Resolve(env)`.
  `SettingsOps`/`ConnectionDiagOps`/`WorkspaceOps`/`SchemaOps` НЕ пришлось трогать — они уже
  принимали `AgentRuntime` параметром, а не полем.
- `ProjectOps` в протоколе: `project.list`/`project.recent`/`project.open`/`project.create` →
  `project.list.result`/`project.context` (последний = те же поля, что `Welcome` для проекта по
  умолчанию, но под явным ProjectId — открытие второго проекта без второго раунд-трипа).
- `ConnectionHub.BroadcastToProjectAsync` — broadcast'ы настроек/подключений/usage/списка чатов
  теперь скоуплены по проекту (соединение "смотрит" на проект, если хоть раз коснулось его через
  `Resolve`), иначе клиент проекта A видел бы настройки/чаты проекта B через общий hub.
  `AgentRuntimeRegistry.RuntimeCreated` — хук, которым `SplaServiceHost` подключает
  events/health-warmup к КАЖДОМУ рантайму (и стартовому, и созданному позже через ProjectOps), а не
  только первому.
- Тест-доказательство изоляции: `MultiProjectProtocolTests` — РЕАЛЬНЫЙ `ClientWebSocket` на реально
  поднятом `SplaServiceHost`, два проекта на одном сокете, `chat.new` в проект B не протекает в
  список чатов проекта A (при первой попытке тест поймал побочный `chat.list.result`-broadcast от
  `chat.new` вместо ответа на явный запрос — тест переделан на корреляцию по RequestId, заодно
  добавлен `requestId` в ответ `chat.list`, которого раньше не было).

Сьют 170 зелёный. Не тронуто сознательно: HTTP-роуты `chat-image`/`plugin-assets` получили
опциональный `?project=` (по умолчанию — DefaultProjectId), но НЕ протестированы отдельно —
низкий риск (то же поведение что раньше, если `?project=` не передан).

## Фаза 3.0 — реальный Gate: закрыт немой разрыв в PermissionManager (сделано)

При разборе `capability-security-model` для планирования Identity+Gate обнаружился реальный,
уже существующий разрыв, независимый от Identity: `ResolvedSettings.PermRead/PermWrite/PermShell/
PermInternet` (плюс `ToolPermissionRules`) уже год как гоняются туда-обратно через настройки
(`SettingsOps.GetAgent`/`SaveAgent`, персист в `.spla`), но **`PermissionManager.CheckPermission`
их вообще не читал** — пользователь мог поставить в UI «Shell: deny» для проекта, это молча
сохранялось и НИКАК не влияло на поведение агента. Это не Identity-вопрос — это уже сломанная
project-policy часть модели «источники × права», которую стоило починить раньше самого Identity.

Правка: `PermissionManager` получил (опциональную) живую ссылку на `ResolvedSettings` — читает
`PermRead/PermWrite/PermShell/PermInternet` НА КАЖДОЙ проверке (не копирует при конструировании),
поэтому `agent.save` из UI применяется со следующего же вызова инструмента без пересборки
`McpHost`/`PermissionManager`. Категория берётся из существующей дискриминации по
`ToolScope`/`ToolEffect` (Shell/Internet — по Scope, Write/Read — по Effect для Project|Local) —
той же, что уже используют ветки по режимам. Явный проект-оверрайд — теперь ЖЁСТКИЙ пол/потолок:
побеждает и над дефолтом режима, и над «remembered» разрешениями сессии, действует ВО ВСЕХ
режимах включая Agent (раньше «remembered» специально игнорировался в Agent-режиме — эта же логика
теперь верна и для проектного оверрайда, но по другой причине: явная политика проекта весомее
разового согласия). `AgentRuntime` передаёт `settings` в конструктор `PermissionManager`.

`ToolPermissionRules` (`SplaToolPermissionRule`, per-tool оверрайды) НЕ тронуты — они не
достижимы ни из одного UI (в `AgentSettingsPayload` нет поля `Tools`), чинить нечего показывать.

Тесты: `PermissionOverrideTests` (5 — deny шелла даже в Agent-режиме, allow снимает Ask в Edit,
живое чтение той же ссылки на settings без пересборки, дефолты без оверрайда не сломаны,
`settings: null` эквивалентен старому конструктору). Сьют 175 зелёный.

## Открытые вопросы (следующие фазы)

- `IIdentity`/auth в хосте (Local/Windows-integrated/OIDC), `IAgentContextFactory`. Сознательно
  не начато: локально тривиально (один пользователь, всегда полный доступ — уже верно и без
  Identity), а реальной multi-tenant server-инфраструктуры (владение проектами per-user) пока
  нет — строить абстракцию без потребителя = театр безопасности, которого модель explicitly
  избегает.
- `HardGate` (win Job Objects / linux seccomp+ns) — серверный этап, зависит от Identity выше.
- Квоты токенов и число параллельных сессий per-identity.
- `ICapabilityGate.CanRead/CanWrite` (path-уровень, ось «источники»: project-only vs +explicit
  paths vs full) — сейчас `AllowAllGate` everywhere; для этого нужна конфигурация «доп. папок»,
  которой в `SplaProject` пока нет (отдельная фича, не просто gate-проводка).
- Транспорт: один WebSocket + `sessionId` в конверте (мультиплекс) — уже решено.
