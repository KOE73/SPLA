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

**Фаза 0.2 (потом):** read-инструменты `FsSearchTextTool`/`FsFindFilesTool` (внешний ripgrep —
нужен `MapPathToHost`) и System `DotnetBuildTool`/`DotnetTestTool` (внешние процессы → `IShell`)
на тот же шов.

## Проверка шва

Тест: подменить в scope `MemoryWorkspace`-песочницу, записать `FsWriteTool`, прочитать
`FsReadTool` — данные живут в памяти, не на диске. Доказывает, что инструменты не знают, что за
песочницей.

## Открытые вопросы (следующие фазы)

- Identity/auth в хосте (Local/Windows-integrated/OIDC), `IAgentContextFactory`.
- `HardGate` (win Job Objects / linux seccomp+ns) — серверный этап.
- Квоты токенов и число параллельных сессий per-identity.
- `IProject` как брокер хранилищ + `IProjectBackend`/`IProjectProvider` (отдельный план).
- Транспорт: один WebSocket + `sessionId` в конверте (мультиплекс) — уже решено.
