# PLAN_20260824_core — сессионный `IShell`: увидеть вопрос и ответить на него

Статус: **выполнено**

Основание: [`ADR_20260824_core_interactive-shell`](../adr/ADR_20260824_core_interactive-shell.md).
Ветка: `work`.

Читать перед правкой: [`agents/tool-args.md`](../../agents/tool-args.md) (обязательные аргументы,
`Details`), [`agents/protocol.md`](../../agents/protocol.md),
[`PLAN_20260701_core_host-abstraction`](PLAN_20260701_core_host-abstraction.md) (почему `Shell`
вынесен отдельным членом `ISandbox`).

---

## Что делается и чего НЕ делается

Делается: **`IShell` описывает интерактивную сессию.** Команда, что-либо спросившая, возвращает
модели управление вместе с текстом вопроса; модель отвечает, ждёт дальше или убивает процесс.

**НЕ делается ConPTY.** Транспорт остаётся пайпами (ADR §3). Следствие принимается осознанно:
программы, уходящие при пайпе в полную буферизацию, промпт не покажут.

**НЕ делается переживание рестарта** и **живой показ вывода по ходу** (ADR §4.2, §4.3).

---

## Конструкция

```
system_run_shell ──► IShell.RunAsync ──► процесс + 2 читающих насоса ──► буфер (+ метка времени)
                                                    │
                                    ┌───────────────┴───────────────┐
                              процесс вышел                    простой
                                    │                               │
                          Status=Exited                  хвост без \n ?
                          сессия закрыта                 ├─ да (>2с)  → WaitingForInput
                                                         └─ нет (>120с) → Running
                                                                │
                                              сессия жива, id возвращён модели
                                                                │
                             system_resume_shell (input?) ──────┤
                             system_kill_shell ─────────────────┘
```

### Контракт (`src/core/SPLA.Domain/Host/IShell.cs`)

| Тип | Изменение |
|---|---|
| `ShellStatus` | **новый** enum: `Exited` \| `WaitingForInput` \| `Running` |
| `ShellCommand` | `+ TimeSpan? PromptIdle = null`, `+ TimeSpan? SilentIdle = null` (null = дефолт хоста) |
| `ShellResult` | `+ ShellStatus Status = Exited`, `+ string? SessionId = null`; `ExitCode = -1` пока не вышел |
| `IShell` | `+ Task<ShellResult> ResumeAsync(string sessionId, string? input, CancellationToken)` |
| `IShell` | `+ Task<ShellResult> KillAsync(string sessionId, CancellationToken)` |

Все новые поля — со значениями по умолчанию: существующие вызовы компилируются без правок.

### Реализация (`LocalShell`)

- `RedirectStandardInput = true`; поток **остаётся открытым** после старта.
- Два фоновых насоса (`stdout`, `stderr`) читают чанками в `StringBuilder` под замком и двигают
  `_lastOutputUtc`. Никакого `ReadToEndAsync`.
- Цикл ожидания: опрос ~100 мс. Выход из цикла — завершение процесса, либо простой сверх порога,
  либо `ct`.
- Хвост буфера (`stdout` + `stderr`) заканчивается на `\n` или `\r` → это не промпт (порог
  `SilentIdle`); иначе — промпт (порог `PromptIdle`).
- Возвращается **дельта** вывода с прошлого возврата, а не весь буфер с начала: иначе каждый
  `resume` перечитывал бы модели всё сначала.
- `ResumeAsync` пишет `input` + `\n` в stdin и снова входит в тот же цикл ожидания.
- Сессия удаляется из реестра при `Exited`, при `KillAsync` и при `Dispose` shell'а
  (`Kill(entireProcessTree: true)` — как уже делает нынешний обработчик отмены).
- Лимит живых сессий (16): при исчерпании `RunAsync` отказывает с внятным текстом, а не копит
  процессы.

### Инструменты

| Инструмент | Scope / Effect / Risk | Аргументы |
|---|---|---|
| `system_run_shell` | Shell / Execute / High | без изменений + новый вид результата |
| `system_resume_shell` | Shell / Execute / High | `session`, `input` (nullable = просто ждать), `output`, `output_name` |
| `system_kill_shell` | Shell / Execute / **Medium** | `session` |

Правила в `Details`, не в `Description`:

- `status: waiting_for_input` — процесс жив и спрашивает; ответ отправляется `system_resume_shell`;
- пустой `input` (`null`) означает «подождать ещё», а не «отправить пустую строку»;
- сессию, которая больше не нужна, обязательно закрывать `system_kill_shell` — иначе процесс живёт;
- `status: running` — процесс просто молчит, а не спрашивает; это не повод его убивать.

---

## Этапы

- [x] **1. Контракт.** `ShellStatus`, поля `ShellCommand`/`ShellResult`, два метода в `IShell`.
- [x] **2. `LocalShell`.** Насосы, буфер с меткой времени, два порога, реестр сессий, `Dispose`.
- [x] **3. Инструменты.** Новый вывод у `RunCommandTool`; `ResumeShellTool`, `KillShellTool`.
- [x] **4. Регистрация.** `AgentRuntime` (`Feature("core.shell", ...)`), `AgentFeatureCatalog`
      (описание фичи со списком инструментов).
- [x] **5. Промпт.** `Features/CoreShell/prompt.md` + строка в `CoreFeaturePrompts.ResourceNames`
      + `EmbeddedResource` в `SPLA.Agent.csproj`. Английский, как все промпты.
- [x] **6. Долгие тихие команды.** `DotnetBuildTool` / `DotnetTestTool` → `SilentIdle` = infinite.
- [x] **7. Тесты** в `tests/SPLA.Tests`: команда, спрашивающая через `Read-Host`, ловится как
      `WaitingForInput` с видимым текстом вопроса; `ResumeAsync` доводит её до `Exited` с
      правильным кодом; тихая долгая команда не объявляется ждущей; `KillAsync` убивает дерево;
      обычная быстрая команда по-прежнему возвращает `Exited` за один вызов.
- [x] **8. Документы.** `SPLA.slnx` (новые файлы docs), `CHANGELOGS/current-log.md` +
      `current-list.md`.

## Дальше (не в этом плане)

- ConPTY как альтернативная реализация за тем же интерфейсом (ADR §3).
- Живой хвост вывода в `ProgressScope` по ходу выполнения (ADR §4.3).
- Сессии, переживающие рестарт SPLA (ADR §4.2).
- Замер и настройка порога `PromptIdle` по живым командам (ADR §4.5).
