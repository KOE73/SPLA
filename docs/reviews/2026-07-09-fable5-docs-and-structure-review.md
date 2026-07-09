# SPLA — соответствие кода/документации и предложение по структуре репозитория (2026-07-09)

Тематическое приложение к [общему обзору](2026-07-09-fable5-architecture-review.md).

---

## 1. Насколько документация соответствует коду

Общий вывод: **проектная/плановая документация (docs/) — актуальна и высокого качества;
агентская нормативная документация (agents/) частично отстала от кода на 1–2 крупных этапа**
(появление `SPLA.Service`/`SPLA.Server`/web-клиента и удаление нативного Avalonia-chat).

### Актуально и хорошо

- `docs/roadmap.md`, `docs/host-abstraction-plan.md` — отражают реальное состояние (фазы 0–3.0,
  что сделано/что нет), честно называют дыры. Это лучший источник правды в репозитории.
- `docs/Doctrine.*.md` + преамбула `AGENTS.md` — стабильны, применяются на практике.
- `agents/skills.md`, `agents/data-ownership.md` — точно описывают `SkillManager`/`CapabilityRegistry`
  и правило «UI не владеет доменными данными»; код соответствует.
- `docs/TODO_SQLITE_VULN.md` — точный, с маркером `SQLITE_VULN_OVERRIDE`; override в csproj на месте.
- `agents/sys_prompt_rules.md` ↔ `SystemPromptBuilder` — согласованы (сегментная сборка, порядок слоёв).

### Существенно устарело (drift)

| Документ | Проблема | Реальность в коде |
|----------|----------|-------------------|
| `agents/structure.md` | Карта решения перечисляет ~12 проектов из 23. **Отсутствуют**: `SPLA.Service`, `SPLA.Service.Contracts`, `SPLA.Server`, `SPLA.Identity.Windows`, `SPLA.Editor.Schema`, `SPLA.Plugins.Browser`, `SPLA.Plugins.Sql(.Avalonia)`, весь `web/`. | 23 проекта в `SPLA.slnx`; сервисный слой и web-клиент — половина системы. |
| `agents/security.md` | Матрица не совпадает с `PermissionManager`. «Agent asks only high-risk shell depending on safety heuristics» — эвристик нет, `Risk=Danger` не присвоен никому → shell в Agent = auto-Allow. «Research: reading files Ask first» — в коде read=Allow. Нет `Scope=Agent`/`Skill`, нет проектных overrides, нет `internet=Ask` для Inspect. | `PermissionManager.CheckPermission` (см. security-обзор). |
| `agents/chat-messages.md` | Описывает нативный Avalonia-chat (`Views/Chat/`, `ViewModels/Messages/`, «создать подкласс `ChatMessageViewModel`»). | Нативного chat-UI больше нет: `SPLA.UI.Avalonia` — WebView-оболочка над web-клиентом; сообщения рендерит Vue (`web/src/surfaces/*`). Документ описывает удалённую подсистему. |
| `agents/protocol.md` | Заявлен как «реестр каждого `MessageTypes`». Документирует ~30–40 типов. | В `Protocol.cs` — **76** констант `MessageTypes`. Не описаны: все `Fs*`, `Project*` (кроме части), `Connection*`, `PluginAction*`, `Schema*`, `Usage*`, `Appearance*`, `Debug*`. |
| `agents/structure.md` (skills/plugins-разделы) | Ок по сути, но `SPLA.Plugins.Test` описан как «в репо, не в slnx» — стоит перепроверить при перекладке. | — |

### Системная причина drift'а

«STOP-документы» (`agents/*`) держатся только на дисциплине автора: ничто в сборке/тестах не
падает, когда код уходит вперёд. Три конкретных лекарства:

1. **Тест-линтер протокола** (`ProtocolDocTests`): каждая `public const string` из `MessageTypes`
   обязана встречаться в `agents/protocol.md`; иначе тест красный. Дёшево, закрывает самый
   ветвящийся контракт.
2. **Генерация нормативных таблиц из кода.** Permission-матрицу в `agents/security.md` не писать
   руками, а генерировать из `PermissionManager` (или из общего декларативного описания политики,
   к которому и код, и доктест обращаются). Тогда «матрица» физически не может разойтись.
3. **Тест-карта решения**: тест перечисляет проекты из `SPLA.slnx` и сверяет со списком в
   `structure.md` (либо `structure.md` генерируется из slnx). Ловит появление/удаление проектов.

---

## 2. Перекладка репозитория во вложенные папки (запрос пользователя)

Желание — разложить 23 проекта по вложенным папкам и дать каждому направлению **свой `AGENTS.md`**,
чтобы жёстче фокусировать внимание агента. Оценка: **оправдано и своевременно**, но с оговорками
по порядку и по механике `AGENTS.md`.

### Предлагаемая группировка (по слоям, а не по алфавиту)

```
/src
  /core            — фундамент, максимально стабильный, без сетевых/UI зависимостей
    SPLA.Domain
    SPLA.MCP.Core
    SPLA.Observability
  /agent           — агентный цикл и промпт
    SPLA.Agent
    SPLA.MCP.BasicTools
  /llm
    SPLA.LLM.LMStudio
  /service         — сервисное ядро + контракты + хосты
    SPLA.Service
    SPLA.Service.Contracts
    SPLA.Server
    SPLA.CLI
  /identity
    SPLA.Identity.Windows      (+ будущий SPLA.Identity.Linux)
  /editor
    SPLA.Editor.Schema
  /plugins
    /network   (SPLA.Plugins.Network + SPLA.Skills.Network)
    /roslyn
    /sql       (SPLA.Plugins.Sql + SPLA.Plugins.Sql.Avalonia)
    /onec      (SPLA.Plugins.OneC + SPLA.Plugins.OneC.Avalonia)
    /browser
    /host-avalonia  (SPLA.Plugins.Host.Avalonia — контракты UI-плагинов)
  /clients
    SPLA.UI.Avalonia
    /web
  /tests
    SPLA.Tests   (позже дробить по слоям синхронно с src)
```

Границы намеренно совпадают с реальными архитектурными швами: `core` ничего не знает про сеть/UI;
`service` — единственный, кто тянет ASP.NET; `plugins/*` изолированы и грузятся через
`AssemblyLoadContext`. Это делает направленные `AGENTS.md` осмысленными (у каждой папки — свой
инвариант), а не косметикой.

### Направленные `AGENTS.md` — как это работает и чего избегать

Плюс реальный: агент, работающий в `/src/service`, получает в контекст только релевантные правила
(протокол, framing, auth), а не всю простыню про Avalonia-темы. Оговорки:

1. **Иерархия, а не дублирование.** Корневой `AGENTS.md` остаётся носителем Doctrine и глобальных
   правил (язык промптов, C#-стиль, translation policy). Папочные — только дельту направления.
   Не копировать общие правила вниз (иначе тот же drift, помноженный на число папок).
2. **`core/AGENTS.md`** — инварианты стабильности: «никаких зависимостей на ASP.NET/Avalonia/сеть»,
   «ambient-scope'ы — единственный способ протащить per-chat контекст», «менять контракт
   `IMcpTool`/`ISandbox` — ломающее изменение, требует ревизии всех плагинов».
3. **`service/AGENTS.md`** — «протокол версионируется; каждый новый `MessageTypes` — в реестр
   protocol.md; enforcement безопасности — здесь и в core, не в tool-слое».
4. **`plugins/AGENTS.md`** — правила именования `[plugin].[domain].[action]`, запрет
   `Scope=Agent/Skill`, декларация capabilities в манифесте.
5. **Не порождать 15 полупустых `AGENTS.md`.** Файл заводить только там, где есть **нетривиальный
   локальный инвариант**. Пустой «см. корневой» — шум.

### Риски перекладки и как их снять

- **`.slnx` пути** — правятся массово, но механически.
- **`PublishAll.cmd`/`build_network.bat`/`Directory.Build.props`** — жёстко зашитые относительные
  пути к проектам и `plugins/`-раскладке; при перекладке ломаются молча (скрипты, не компиляция).
  Перед перекладкой прогнать `PublishAll.cmd` дважды, зафиксировать эталон вывода, сверить после.
- **`CopySkills`/`xcopy`-таргеты** (`SPLA.Skills.Network`) — пути к `plugins/network/skills/`.
- **`ProjectReference`** относительные пути во всех csproj.
- **Документация со ссылками на пути** (`AGENTS.md`, `agents/structure.md`, эти обзоры) — обновить.

Рекомендация по порядку: **сначала Этап-2 рефакторы** (декомпозиция `ClientConnection`, типизация
`ToolResult`, распил `ResolvedSettings`) — они и так трогают файлы массово; перекладку сделать
**одним отдельным коммитом после**, чтобы move-diff не смешивался с содержательными правками
(иначе history/blame по ядру станет нечитаемым). Git move + правка ссылок + двойной прогон
`PublishAll.cmd` + зелёный тест-сьют — критерий приёмки.

### Что перекладка НЕ решает

Направленные `AGENTS.md` фокусируют внимание, но не заменяют пункты 1–3 из §1 (тест-линтеры).
Без них дельта-документы разойдутся с кодом ровно так же, как сейчас `structure.md`. Перекладку
стоит делать **вместе** с введением доктестов — иначе получим больше документов и ту же проблему
их поддержания, только распределённую.

---

## 3. Краткий чек-лист для синхронизации (можно сделать сразу, независимо от перекладки)

- [ ] `agents/structure.md`: дописать 11 недостающих проектов + `web/`; убрать/пометить нативный chat.
- [ ] `agents/security.md`: переписать матрицу под реальный `PermissionManager` (Scope=Agent/Skill,
      overrides, отсутствие Danger-эвристик, shell auto-allow в Agent).
- [ ] `agents/chat-messages.md`: переписать под web-клиент (`web/src/surfaces`) или архивировать.
- [ ] `agents/protocol.md`: дополнить до всех 76 `MessageTypes` + добавить `ProtocolDocTests`.
- [ ] Добавить `ProtocolDocTests`, `SolutionMapTests`, генерацию permission-матрицы.
