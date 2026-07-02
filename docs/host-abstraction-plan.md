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

**Фаза 1.1 (следующее):** остальные потребители `.spla/` через бакеты: `ChatImages`,
`SplaTelemetry` (logs), `FileSecretStore` (решение о пути — в бэкенд), `IconGenerator`;
плагины получают свой бакет при init (OneC sqlite → bucket("onec"), Browser profile →
bucket("browser-profile")) вместо ручной склейки workspacePath.

## Открытые вопросы (следующие фазы)

- Identity/auth в хосте (Local/Windows-integrated/OIDC), `IAgentContextFactory`.
- `HardGate` (win Job Objects / linux seccomp+ns) — серверный этап.
- Квоты токенов и число параллельных сессий per-identity.
- `IProject` как брокер хранилищ + `IProjectBackend`/`IProjectProvider` (отдельный план).
- Транспорт: один WebSocket + `sessionId` в конверте (мультиплекс) — уже решено.
