# PLAN 2026-08-26 — MCP-клиент: потребление чужих серверов, волна 1

**Статус: в работе. Шаги 0 и 1 сделаны (ветка mcp/client), 1125 тестов зелёные.** Решения — [`ADR_20260826_service_mcp-client`](../adr/ADR_20260826_service_mcp-client.md).
Всё, что ниже, — как их построить, а не почему они такие.

## Что строим одной фразой

Чужой MCP-сервер подключается по stdio или HTTP, его инструменты появляются в `McpHost` под
префиксом `<server_id>_`, идут через тот же конвейер, что свои, и уровнены одной корзиной
(`ToolSetDescriptor`) на весь сервер.

---

## Шаг 0 — оси и права (ядро) — **СДЕЛАНО**

`src/core/SPLA.Domain/Models/PermissionEnums.cs`

- [x] `ToolScope.Foreign` — новое значение в конце enum, с комментарием: инструмент, который
      исполняет не этот процесс и который не объявлял наших осей.

`src/core/SPLA.MCP.Core/Permissions/PermissionManager.cs`

- [x] `ClassifyCategory`: `{ Scope: ToolScope.Foreign } => "foreign"`.
- [x] `ProjectOverride`: `"foreign" => ParseOverride(_settings?.PermForeign)`.
- [x] `Decide`: ветка `Foreign` **до** режимных веток, после `ProjectOverride` **и после** блока
      запомненных разрешений: `Chat → Deny`, всё остальное → `Ask`. Порядок именно такой, а не
      «сразу после `ProjectOverride`»: иначе запомненное подтверждение не доживало бы до вердикта.
      Своего кода на запоминание не пишем, но условие входа в блок пришлось расширить — см. ниже.

`src/core/SPLA.Domain/Settings/`

- [x] `SplaPermissionsSection.Foreign` (`permissions.foreign`), флэттен в `ResolvedSettings.PermForeign`
      рядом с `PermRead`/`PermWrite`/`PermShell`/`PermInternet`.
      **Поправка к плану:** слой один, а не два — у `SplaDefaults` секции `permissions` нет вовсе,
      ни одна категория прав не резолвится из machine-слоя. Строка добавлена только в проектный блок,
      рядом с существующей `PermInternet`.

**Ловушка (сработала).** Существующие ветки `Edit`/`Agent` заканчиваются `Deny`/`Ask` для
нераспознанного инструмента, так что забытая ветка не открывает доступ, а закрывает — но в `Agent`
она неотличима от настоящей по `PermissionResult`. Тест сверяет `Category == "foreign"`: только
настоящая ветка его выставляет.

**Вторая ловушка, найденная при реализации и не предвиденная планом.** `Decide` пропускает поиск
запомненных разрешений целиком в режиме `Agent` — с верным обоснованием: там все прочие области и
так `Allow`, а протухший запомненный запрет не должен перебивать правила режима. `Foreign` ломает
эту посылку: он `Ask` и в `Agent`. Без правки «подтвердить первый вызов каждого инструмента»
превращалось бы в вопрос на **каждый** вызов в том самом режиме, в котором работают, — а
подтверждение, которое всплывает всегда, не читает никто. Условие пропуска расширено:
`if (mode != AgentMode.Agent || toolMetadata.Scope == ToolScope.Foreign)`. Два теста на обе половины
(запомненное «да» и запомненное «нет» в `Agent`).

---

## Шаг 1 — динамическая регистрация — **СДЕЛАНО** (то, чего не хватало `PLAN_20260819`)

`src/core/SPLA.MCP.Core/McpHost.cs`

- [x] `public bool UnregisterTool(string name)` — удаляет из `_tools`, логирует. **Без блокировок:**
      `_tools` читается на входе каждого вызова (`ToolResolutionStage`), вызов, уже прошедший
      резолюцию, доигрывает на своём объекте.
- [x] Проверить, что `Dictionary` не читается конкурентно с записью → заменить на
      `ConcurrentDictionary` либо обернуть запись. **Это реальный риск:** регистрация теперь
      происходит из фонового потока подключения, а не только в конструкторе.

`src/core/SPLA.MCP.Core/ToolSets/ToolSetRegistry.cs`

- [x] `AddDynamic(ToolSetDescriptor)` / `RemoveDynamic(string setId)` — сегодня `Add` приватный и
      зовётся только из конструктора.
- [x] `ToolSetOrigin.Mcp` — третье значение рядом с `Core`/`Plugin`.
- [x] `LevelOf`: набор с `Origin: Mcp` без явной записи в `toolsets:` → `Enabled`.
      **Поправка к плану:** делегата «включён ли сервер» нет и не нужно, зеркало плагинной ветки
      здесь неверно. Плагин может быть выключен, а дескриптор его при этом существует (сборка
      остаётся загруженной, гасится только раскрытие). У сервера такого состояния нет: отключение
      снимает набор и разрегистрирует инструменты, поэтому само наличие дескриптора уже означает,
      что сервер на связи. Записано комментарием в коде и абзацем в `agents/toolsets.md`.

---

## Шаг 2 — транспорт и сессия

Новый проект `src/service/SPLA.Mcp.Client/` (ссылки: `SPLA.Domain`, `SPLA.MCP.Core`, `SPLA.Observability`).
Никто на него не ссылается, кроме `SPLA.Runtime` — цикла нет.

- [ ] `IMcpTransport` — `Task<JsonNode?> SendAsync(JsonNode request, CancellationToken)`,
      `event Action<JsonNode> Notification`, `Task<JsonNode> RequestFromServer` (входящие запросы
      сервера — нужны, чтобы отвечать `-32601`), `DisposeAsync`.
- [ ] `StdioTransport` — `Process` с redirect stdin/stdout/stderr.
      **Ловушки:** stderr обязательно вычитывать (иначе буфер забьётся и сервер встанет);
      кодировка UTF-8 без BOM в обе стороны; убийство **дерева** процессов на Windows при Dispose
      (`Process.Kill(entireProcessTree: true)`); `env`/`cwd` из конфига.
- [ ] `HttpTransport` — `POST` с `Accept: application/json, text/event-stream`; если ответ
      `text/event-stream` — разбор кадров `data: ...`, финальным считается кадр с тем же `id`
      (та же логика, что уже в `SplaServiceHost.IsFinalResponse` — **посмотреть на неё, не
      изобретать**: там уже поймали баг «первая записанная строка ≠ ответ»).
- [ ] `McpServerSession` — handshake (`initialize` → `notifications/initialized`),
      `ListToolsAsync`, `CallToolAsync`, состояние (`Disconnected/Connecting/Ready/Failed`),
      реконнект с backoff, `notifications/tools/list_changed` → перечитать и перерегистрировать.
- [ ] Объявляемые при handshake capabilities: **пусто**. Не объявляем `sampling`, `elicitation`,
      `roots`. `clientInfo` — `spla` + версия.
- [ ] Входящий запрос сервера любого метода → ответ `{"error":{"code":-32601,...}}`.
      Логировать на Warning: это единственный способ узнать, что сервер чего-то от нас хотел.

---

## Шаг 3 — проекция инструмента

- [ ] `McpToolNaming` — префикс, валидация `server_id` (`^[a-z][a-z0-9_]{0,15}$`), проверка длины
      ≤64 и столкновения с уже зарегистрированным именем. Оба провала — отказ от регистрации
      **этого** инструмента + Warning, остальные регистрируются.
- [ ] `McpProxyTool : IMcpTool`:
      - `GetDefinition()` → `Scope = Foreign`, `Effect = Write`, `Risk` = `High`, поднимается до
        `Danger` при `destructiveHint: true`. `readOnlyHint` **игнорируется**.
      - `Parameters` = `inputSchema` сервера как есть. `StrictSchema = false` — чужая схема почти
        наверняка не удовлетворяет strict-контракту OpenAI.
      - `Description` = описание сервера + строка «provided by MCP server '<id>'», чтобы модель
        видела источник в самой карточке инструмента.
      - `ConversationBound = false`, `SupportsBackground = false`.
      - `ExecuteAsync` → `session.CallToolAsync`, маппинг результата:
        `text`→`ToolText`, `image`→`ToolImage`, `resource`/`resource_link`→`ToolResource`,
        `isError: true` → `ToolResult.Fail`, отсутствие сессии → `Fail("server not connected")`.
      - Прогресс: передать `_meta.progressToken` (любая строка, наша), входящие
        `notifications/progress` → `ProgressScope.Report(progress, total, message)`
        (`src/core/SPLA.Domain/Tools/ProgressScope.cs`). Узел вызова уже открыт `ProgressNodeStage`,
        свой `BeginNode` не нужен. Зеркальная сторона — `McpProgressReporter` — показывает, как
        токен эхом возвращается в кадре.
- [ ] Бит недоверия: после успешного вызова, если у сервера `origin != named`, поднять
      `ChatDoubt` с `DoubtCause(new DataOrigin($"mcp:{id}", OperatorNamed: false), toolName, now)`.

---

## Шаг 4 — конфигурация

`SplaMcpSection` (существующая секция `mcp:` — обе стороны провода в одном месте):

```yaml
mcp:
  enabled: true          # существующее: отдача наружу
  port: 7777             # существующее
  servers:               # новое: потребление
    - id: ghmcp
      name: GitHub
      enabled: true
      transport: stdio            # stdio | http
      command: npx                # stdio
      args: ["-y", "@modelcontextprotocol/server-github"]
      cwd: null
      env:
        GITHUB_TOKEN: "secret:github-pat"
      url: null                   # http
      headers: {}                 # http, значения тоже через secret:
      description: "Issues и PR в наших репозиториях"
      origin: unnamed             # unnamed (умолч.) | named
      level: enabled              # необязательно; иначе через toolsets:
```

- [ ] `SplaMcpServerSection` + `List<SplaMcpServerSection>? Servers` в `SplaMcpSection`
      (`SplaSections.cs:302`).
- [ ] Слияние по `id` через слои — **дословно по образцу `MergeConnections`**
      (`SettingsResolver.cs:615`): словарь заводится в начале `Resolve` (стр. 363), вызывается
      дважды (стр. 375 — defaults, стр. 432 — project), раскладывается в `r.<Prop>` в конце
      (стр. 496). Второй код слияния не писать.
- [ ] `ResolvedSettings.McpServers` — рядом с `McpEnabled`/`McpPort` (`SettingsResolver.cs:83-96`).
- [ ] Сохранение: `ConfigLoader.SaveProjectSections(project, path, "mcp")` — `GetSectionValue`
      (`ConfigLoader.cs:327`) уже знает ключ `"mcp"`, править нечего. Сплайсер переписывает **только**
      эту секцию, комментарии в остальном файле выживают.
- [ ] Токены только `secret:`/`env:`; резолв в момент коннекта через
      `Settings.SecretResolver.ResolveAsync` (образец вызова —
      `ConnectionHandlers.cs:124`), не при загрузке настроек
      ([`agents/secrets.md`](../../agents/secrets.md)). В `ResolvedSettings` кладём ссылку, не
      значение; в `McpServersPayload` наружу — тоже ссылку.

---

## Шаг 5 — включение в рантайм

`src/agent/SPLA.Runtime/AgentRuntime.cs`

- [ ] `McpClients` (по образцу `PluginManager`) — создаётся после `McpHost` и `ToolSets`.
- [ ] **Фоновый коннект** (решение ADR §4, вопрос 1): `_ = Task.Run(() => McpClients.ConnectAllAsync())`
      в конце конструктора. Каждый успешный handshake:
      1. регистрирует инструменты в `McpHost`,
      2. добавляет корзину в `ToolSets`,
      3. `RefreshSkillCapabilities()`,
      4. `Events.Publish(new McpServersChanged())`.
- [ ] `DisposeAsync` рантайма гасит клиентов (убийство дочерних процессов).
- [ ] **В коде рядом с фоновым коннектом — комментарий, что это временное решение**, со ссылкой на
      ADR §4 вопрос 1 и на `PLAN_20260819`. Без него через месяц никто не отличит долг от замысла.

---

## Шаг 6 — UI

- [ ] `McpPanel.vue`: два раздела — «Отдача наружу» (существующее) и «Подключённые серверы» (новое).
      Строка сервера: имя, транспорт, статус (цветом), число инструментов, уровень корзины,
      вкл/выкл, кнопка «Переподключить», развёртка со списком инструментов и последней ошибкой.
- [ ] Форма добавления сервера: id, имя, транспорт, команда/URL, env/headers, описание, origin.
- [ ] Протокол — по образцу существующего `mcp.get`/`mcp.save`/`mcp.result`:
      константы в `SPLA.Service.Contracts/Protocol.cs` (`McpGet` — стр. 189, `McpSave` — 192,
      `McpResult` — 356), payload в `Payloads.cs` (`McpSettingsPayload`, стр. 477), маршрутизация в
      `SPLA.Service/Protocol/Handlers/SettingsHandlers.cs` (стр. 26–27, 58–70), зеркальные типы в
      `web/src/protocol/types.ts` (стр. 238). Добавляем `mcp.servers.get/save/result` +
      `McpServersPayload`.
- [ ] **Событие домена нужно, в отличие от `mcp.save`.** Настройки отдачи наружу меняются только по
      действию клиента, поэтому там хватает `BroadcastToProjectAsync` из обработчика. Статус
      подключения меняется сам — в фоне, без запроса, — значит `McpServersChanged : ServiceEvent`
      в `src/agent/SPLA.Runtime/ServiceEvents.cs` (образец: `AppearanceChanged`, стр. 17) и
      рассылка всем клиентам ([[service-event-bus]]).
- [ ] Регистрация обоих в [`agents/protocol.md`](../../agents/protocol.md) — **обязательна**, это
      правило репозитория.
- [ ] Вкладка уже есть: `Settings.vue` стр. 27 (компонент), 69 (`TABS`), 128/151 (карта панелей для
      сохранения) — новый раздел встаёт внутрь существующей вкладки «MCP», новой вкладки не заводим.
- [ ] Правка настроек — **явный Save**, не авто ([[feedback-auto-apply-vs-save]]): подключение
      чужого сервера — транзакция, а не преференс.
- [ ] В заголовке раздела — одна честная строка: гранты берутся на сервер целиком, оси риска у
      чужих инструментов нет. **Название «наивно», а не «модель безопасности»** (ADR).

---

## Шаг 7 — проверка

- [ ] Юнит-тесты: префиксация и её отказы (длина, столкновение, кривой id); маппинг схемы туда;
      маппинг `content[]`/`isError` обратно; вердикт для `Foreign` во всех пяти режимах;
      «аннотации только ужесточают»; `UnregisterTool`; слияние `servers:` по слоям.
- [ ] **Петля в одном процессе, без процессов и сети.** `McpStdioServerTests.cs:130` уже гоняет наш
      *сервер* через подставные `TextReader`/`StringWriter` (`ScriptedReader`). Тот же приём в
      обратную сторону: наш клиент на одном конце пары потоков, наш `McpStdioServer` — на другом.
      Это самый дешёвый честный end-to-end, и он существует до того, как появится хоть один
      транспорт. Строить тесты от него.
- [ ] xunit, файл `tests/SPLA.Tests/McpClientTests.cs`, методы —
      `Snake_case_предложением` (образец: `A_tool_that_was_not_offered_cannot_be_called_by_naming_it`).
      Минимальная сборка хоста — `new McpHost(new PermissionManager())` (`ToolExposureTests.cs:35`).
      Запуск: `dotnet test tests/SPLA.Tests`.
- [ ] **Живой прогон №1 — SPLA↔SPLA:** `spla serve` с `mcp.enabled: true`, второй проект
      подключается к нему по HTTP. Проверяет handshake, `tools/list`, вызов, SSE-прогресс.
- [ ] **Живой прогон №2 — чужой stdio:** `npx -y @modelcontextprotocol/server-everything`.
      Он специально гоняет прогресс, `elicitation`, `sampling`, ресурсы — то есть показывает наши
      отказы своими глазами, а не в теории ([[feedback-verify-against-live-providers]]).
- [ ] Проверить, что модель действительно вызывает чужой инструмент по префиксованному имени, а не
      по исходному: это единственное, что нельзя проверить юнит-тестом.

---

## Порядок и что чем блокируется

```
Шаг 0 (оси) ─┬─→ Шаг 3 (проекция) ─→ Шаг 5 (рантайм) ─→ Шаг 6 (UI)
Шаг 1 (дин.) ─┤                            │
Шаг 2 (транспорт) ─┘                       └─→ Шаг 7 (проверка)
Шаг 4 (конфиг) ──────────────────────────→ ┘
```

Шаги 0, 1, 2, 4 независимы и делаются параллельно. Шаг 7 начинается с живого прогона №1, как только
готовы 2+3+5 — раньше, чем UI.

---

## Не в этой волне

`elicitation`, `sampling`, `roots`, `resources/*`, `prompts/*`, OAuth к удалённым серверам, фоновый
чужой вызов. Причины — в ADR §2 и §3. `elicitation` и `sampling` ждут двунаправленного канала,
который приедет один раз и целиком, вместе с насосом ходов ([[chat-turn-pump-decision]]).
