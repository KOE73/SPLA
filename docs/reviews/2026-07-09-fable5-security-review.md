# SPLA — обзор безопасности и модели разрешений (2026-07-09)

Тематическое приложение к [общему обзору](2026-07-09-fable5-architecture-review.md).
Фокус: permission-модель, sandbox/capability-gate, shell/fs/network-инструменты, пути эскалации,
целевая архитектура безопасности с расширяемостью.

---

## 1. Как устроена защита сейчас (модель по коду)

Проверки «может ли агент это сделать» размазаны по **четырём независимым слоям**, которые не
знают друг о друге и отвечают на пересекающиеся вопросы по-разному:

| Слой | Где | Что решает | Единица решения |
|------|-----|-----------|-----------------|
| 1. Видимость инструментов | `SPLA.Agent/ToolModeFilter` | какие tools вообще уходят модели в данном режиме | Scope/Effect × Mode |
| 2. Runtime-разрешение | `SPLA.MCP.Core/PermissionManager` | Allow / Ask / Deny на исполнение | Scope/Effect/Risk × Mode × overrides × remembered |
| 3. Capability-gate | `SPLA.Domain/Host/ICapabilityGate` | можно ли read/write/execute/network | вызывается **только** shell/build-инструментами |
| 4. Path-guard | `SPLA.Service/WorkspaceOps.IsUnderRoot` | путь внутри workspace | только web-редактор (FsBrowse/Read/Write) |

Слои 1 и 2 дублируют логику (обе таблицы «Scope×Mode», заданы в двух местах и обязаны совпадать).
Слой 3 введён как «единая точка политики» (`ISandbox.Gate`), но по факту его спрашивают только
три инструмента (`RunCommandTool`, `DotnetBuildTool`, `DotnetTestTool`) и только про `CanExecute()`
— `CanRead/CanWrite/CanNetwork` не вызывает **никто**, а реализация всегда `AllowAllGate` (всё true).
Слой 4 — отдельный ad-hoc guard, живёт только в сервисном FS-API для web-редактора и не связан с
инструментами агента.

### Как принимается решение об исполнении (McpHost.ExecuteToolAsync)

1. `PermissionManager.CheckPermission(mode, def.Function, argsJson)`:
   - `Scope=Agent` → **всегда Allow** (в любом режиме, включая Chat);
   - `Scope=Skill` → Ask/Allow/Deny по режиму;
   - проектный override (`perm_read/write/shell/internet` = allow/deny/ask) — жёсткий пол/потолок;
   - иначе — матрица по режиму (Chat=Deny всё; Research=read+internet; Inspect=read+ask-internet;
     Edit=read+ask-write+ask-shell; Agent=почти всё allow, Danger-shell=Ask).
2. `Allow` → исполнить; `Deny` → строка-ошибка модели; `Ask` → `PermissionScope.RequestAsync`
   маршрутизирует запрос в UI того чата, что начал turn, и ждёт ответа.

### Sandbox / host-граница

`ISandbox` = `IWorkspace` + `IShell?` + `ICapabilityGate`. Инструменты ходят в систему через
`HostServices.Sandbox` (ambient, резолвится из `AgentSessionScope.Current.Sandbox`, иначе
`PassthroughSandbox.Default`). Задумано как шов: soft-gate (проверки в коде) и hard-gate
(OS-изоляция) дают одно решение, различается только enforcement. **Реализован только passthrough.**

### Auth (серверный режим)

`SPLA.Server` / `serve --bind`: Negotiate (NTLM/Kerberos) один раз на `/login` → cookie
(`spla.auth`, 10ч, HttpOnly, sliding) → все запросы, включая `/ws`, по cookie. Identity (SID +
группы) кладётся в cookie как claims. Per-user область `{root}/users/{sid}/`. Токен-режим —
альтернатива для не-доменного сетевого использования через `AuthGate`.

---

## 2. Пути эскалации и опасные дефолты (по убыванию критичности)

### C1 — Сервер выполняет команды без изоляции (Critical)
`ChatRuntime` создаёт `AgentSession(sessionKv, checkpoint, skillSession)` **без параметра sandbox**
→ `PassthroughSandbox.Default` (`AllowAllGate` + `LocalWorkspace` весь диск + `LocalShell` =
`powershell.exe`). На сервере turn исполняется от имени сервисного аккаунта. Любой доменный
пользователь, переключив чат в режим Agent (а `Scope=Shell/High` в Agent = **auto-Allow**, см. C4),
выполняет произвольные команды на сервере и читает/пишет любой путь. Per-user области изолируют
только *листинг проектов*, но не *действия инструментов*.
→ Фикс: см. целевую модель §3; минимально — на сервере `Shell=null` + `WorkspaceGate(userArea)`.

### C2 — Token-auth не применяется к сообщениям (Critical)
`AuthGate.Authorize` вызывается **только** в `HandleHelloAsync`. `DispatchAsync` не проверяет, что
Hello состоялся и прошёл. Клиент, открывший сокет и сразу приславший `ChatSend`/`FsWrite`/
`ProjectOpen`, минует токен. То есть `serve --bind 0.0.0.0 --token X` (документированный способ
«безопасно выставить в сеть») не защищает. В доменном режиме спасает fallback-policy ASP.NET на
`/ws` (нужен cookie), но токен-путь — дыра.
→ Фикс: состояние `Authenticated` на соединении; до успешного Hello при `RequiresToken` любое
другое сообщение → close(PolicyViolation).

### C3 — Произвольный projectId → доступ к чужим проектам (High)
`ProjectOpen`/`Resolve` принимают любой `ProjectId` (= путь к `.spla`) и делают
`registry.Open(path)` → `ConfigLoader.LoadAndResolve(path)`. В серверном режиме `_userProvider`
скоупит только `List()/Recent()`, но `Open`/`Resolve` идут в **общий** `_registry`, не в
пользовательский провайдер, и не сверяют путь с `_userArea`. Доменный пользователь, зная/подобрав
путь, открывает чужой проект (или любой `.spla` на диске сервера) со всеми его секретами и историей.
`/chat-image/{chatId}/{file}?project=` — тот же произвольный `project`.
→ Фикс: единая точка `Resolve` — canonical-prefix-проверка `projectId` против `_userArea` +
share-ACL по группам; `Open` в серверном режиме только через `_userProvider`.

### C4 — Самодекларация Scope/Effect/Risk + auto-allow shell (High)
Классификация инструмента — его собственное свойство в коде (`Scope=…, Risk=…`). Ядро доверяет ей
без проверки. Следствия:
- плагинный DLL, объявивший `Scope=Agent`, исполняется **в любом режиме, включая Chat**, без Ask;
- `ToolRisk.Danger` не присвоен **ни одному** инструменту в репозитории (проверено). Единственная
  ветка, где Agent-режим спрашивает про shell — `Risk==Danger`. Значит `system_run_shell`
  (`Risk=High`) и `roslyn_script_run` (`Scope=Shell, Risk=High`) в Agent-режиме — **всегда Allow,
  без подтверждения**. `agents/security.md` обещает «Agent asks only high-risk shell depending on
  safety heuristics» — таких эвристик в коде нет.
→ Фикс: (а) капабилити плагина декларируются в `meta.yaml` и валидируются ядром при регистрации;
плагинам запрещены `Scope=Agent/Skill`; их фактический Scope не выше заявленного в манифесте.
(б) Ввести реальную оценку опасности команды (или трактовать любой `Scope=Shell` как минимум Ask
на сервере) — «shell в Agent без вопроса» это ок локально, но не для доменного сервера.

### H1 — Cross-Site WebSocket Hijacking (High, серверный режим)
`/ws` аутентифицируется cookie. Проверки `Origin` при апгрейде нет. Любая веб-страница, открытая
в браузере доменного пользователя, может установить WebSocket к серверу SPLA — cookie уйдёт
автоматически — и управлять агентом от его имени. Классический CSWSH.
→ Фикс: проверять `Origin` против allowlist на апгрейде `/ws`; отклонять чужие источники.

### H2 — Remembered-разрешения утекают между чатами и пользователями (High на сервере)
`PermissionManager._rememberedPermissions` живёт на уровне `AgentRuntime` (один на проект).
«Разрешить и запомнить» в одном чате действует на все чаты проекта; на shared-проекте — на всех
пользователей. Плюс матчинг по точному JSON аргументов (кроме `*`) почти никогда не повторяется →
пользователь на практике вынужден ставить `*`, что снимает защиту полностью для этого инструмента.
→ Фикс: remembered-грант скоупить per-chat (или per-identity) и хранить как паттерн по значимым
полям (путь-префикс, домен), а не по сырой строке.

### M1 — Секреты и токены в открытую (Medium)
- `FileSecretStore` — plaintext YAML (честно помечено «Phase-1 naive»), но резолвер уже используется
  для строк подключения плагинов.
- `--token X`, `--cert-password` — в argv (видны в списке процессов); дефолтный пароль PFX — `"spla"`.
- Self-signed cert генерируется автоматически, клиентам предлагается добавить в Trusted Root —
  нормально для интранета, но без pinningّа MITM в домене возможен.
→ Фикс: DPAPI/libsecret-store за тем же `ISecretStore`; читать токен/пароль из env/файла, не argv;
сменить дефолтный пароль на генерируемый.

### M2 — Опасные дефолты режима (Medium)
Дефолтный режим нового `defaults.yaml` — `Edit` (`ConfigLoader.LoadDefaults`). На сервере
пользователь по умолчанию сразу может писать файлы (Ask) и, переключив на Agent, — исполнять.
Серверный дефолт должен быть `Research`/`Inspect`.

### M3 — Shell без бортовых ограничений (Medium)
`LocalShell`: нет таймаута, нет лимита вывода (весь stdout в память), отмена не убивает процесс
(осиротевшие `powershell.exe`). `-ExecutionPolicy Bypass`. Даже локально это DoS-поверхность.
→ Фикс: kill-tree + таймаут + cap вывода (в §6 общего обзора, п.9).

---

## 3. Целевая архитектура безопасности (стройная и расширяемая)

Цель — **одно решение, один enforcement-механизм, декларативная политика, проверяемая расширяемость.**

### Принцип 1. Gate — единственный, кто решает; на уровне ядра
Всякое действие инструмента нормализуется до **capability-запроса** и проходит через `ICapabilityGate`
в одной точке (`McpHost`), а не в каждом инструменте по отдельности:

```
CapabilityRequest =
  | ReadPath(path)
  | WritePath(path)
  | Execute(commandDescriptor)
  | Network(host, port)
```

`IWorkspace` уже даёт `MapPathToHost` — это естественная точка, где путь превращается в
capability-запрос и проверяется. Сейчас fs-инструменты зовут `Workspace.ReadAllLines` напрямую,
минуя gate; надо, чтобы workspace (или обёртка над ним) **сам** спрашивал gate. Тогда path-guard
из `WorkspaceOps` (слой 4) и gate (слой 3) сливаются в один, и web-редактор с агентом получают
одну и ту же границу.

### Принцип 2. Mode/permission — это конфигуратор политики gate'а, а не отдельный enforcement
`ToolModeFilter` (что показать модели) остаётся UX-фильтром (не безопасностью — модель всё равно
не должна мочь исполнить скрытый tool: это гарантирует gate). `PermissionManager` перестаёт быть
вторым местом enforcement и становится источником **политики**: «в режиме X категория Y →
allow/ask/deny». Эта политика материализуется в конкретный `ICapabilityGate` для сессии
(`ModeGate` поверх `WorkspaceGate` поверх `ShareAclGate`), который и есть единственный проверяющий.

### Принцип 3. Классификация — свойство контракта, подтверждённое ядром, а не самоназначение
- Встроенные инструменты: класс фиксируется белым списком в ядре (не читается из свойства класса,
  которое можно подменить в форке/плагине).
- Плагинные: заявляются в `meta.yaml` (подписываемый манифест), ядро при регистрации не даёт
  инструменту прав выше заявленного и запрещает привилегированные Scope (`Agent`, `Skill`).
- UI показывает пользователю, какие capabilities просит плагин (как Android-permission), решение —
  явное.

### Принцип 4. Двухосевая модель (уже выбрана в roadmap — довести)
- Ось «права»: Read / Write / Execute / Network (сделано на уровне классификации).
- Ось «источники»: какие пути/хосты видимы и доступны (`WorkspaceGate` по корню проекта +
  `workspace.roots`/`extra_paths` из манифеста; сетевой allowlist для Network-инструментов).
Обе оси — параметры одного gate'а, а не новые слои.

### Принцип 5. Enforcement по среде доверия
Одна политика — разные механизмы: локально SoftGate (проверки в коде, shell разрешён);
на сервере тот же контракт, но `WorkspaceGate` включён принудительно, `Shell=null` (или HardGate
с OS-изоляцией, когда появится). Никакой инструмент не знает, где он исполняется.

### Что где должно проверяться

| Проверка | Уровень | Почему |
|----------|---------|--------|
| Классификация инструмента (Scope/Effect/Risk) достоверна | **core** (реестр/манифест) | нельзя доверять коду плагина о самом себе |
| read/write/execute/network разрешён | **core gate** (одна точка) | иначе слои расходятся |
| Граница путей (workspace + extra roots) | **core** (внутри `IWorkspace`) | и агент, и web-редактор через один путь |
| Сетевой allowlist | **core gate** | Network-инструменты сейчас вообще не спрашивают gate |
| ACL проекта (владелец/группы) | **core** (`Resolve`) | единственная точка резолва проекта |
| Интерактивный Ask + запоминание | **plugin/tool boundary → UI** (`PermissionScope`) | это UX-политика, не enforcement |
| Специфичная валидация ввода (SQL injection и т.п.) | **tool layer** | знание домена — в инструменте |
| OS-изоляция процесса | **host layer** (HardGate) | зависит от ОС |

Итог: **enforcement — в ядре и в host-границе; политика — в mode/permission-слое; UX подтверждений
и доменная валидация — в tool-слое.** Сейчас enforcement ошибочно частично живёт в tool-слое
(каждый shell-инструмент сам зовёт `CanExecute`) и в сервисном слое (`WorkspaceOps`), а в ядре —
только Allow/Ask/Deny поверх самодекларации.

---

## 4. Приоритетный порядок работ по безопасности

1. C2 (token-bypass) — тривиально, закрыть немедленно.
2. C1 + C3 (sandbox на сервере + ACL проекта) — до дальнейшего распространения сервера по домену.
   Минимальный безопасный серверный профиль: `Shell=null`, `WorkspaceGate(userArea)`,
   дефолт-режим `Research`, `projectId` только внутри области.
3. H1 (Origin-check) — одна проверка на апгрейде `/ws`.
4. C4 (манифест-классификация плагинов) — предотвращает будущую эскалацию через сторонние плагины.
5. H2 (скоуп remembered-грантов) + M1 (секреты вне argv, DPAPI) + M3 (shell kill/timeout).
6. Стратегически: слияние четырёх слоёв в единый core-gate (Принципы 1–2), затем HardGate и
   share-ACL по группам.

Направление, заложенное в roadmap (двухосевая модель, gate=policy, sandbox=mechanism), —
правильное. Не хватает не идей, а (а) синхронизации фазы enforcement с уже выехавшей фазой сетевого
доступа и (б) схлопывания дублирующих слоёв в одну проверяемую точку.
