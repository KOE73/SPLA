# SPLA — обзор безопасности и целевая архитектура разрешений (2026-07-10, ревизия 2)

Тематическое приложение к [общему обзору](2026-07-10-fable5-architecture-review.md).
Фокус: реальная модель проверок по коду, пути эскалации, опасные дефолты, оценка новой
auth-подсистемы (`--auth local`) и — главное — **непротиворечивая целевая архитектура
разрешений**. Ревизия 2 учитывает добавленную за день локальную аутентификацию.

---

## 1. Как устроена защита сейчас (модель по коду)

Проверки «может ли агент это сделать» размазаны по **четырём независимым слоям**, отвечающим на
пересекающиеся вопросы по-разному:

| # | Слой | Где | Что решает | Единица решения | Кто реально спрашивает |
|---|------|-----|-----------|-----------------|------------------------|
| 1 | Видимость tools | `SPLA.Agent/ToolModeFilter` | какие tools уходят модели в режиме | Scope/Effect × Mode | оркестратор, каждый ход |
| 2 | Runtime-разрешение | `SPLA.MCP.Core/PermissionManager` | Allow/Ask/Deny | Scope/Effect/Risk × Mode × override × remembered | `McpHost.ExecuteToolAsync` |
| 3 | Capability-gate | `SPLA.Domain/Host/ICapabilityGate` | read/write/execute/network | путь/хост | **только** shell/build-инструменты, только `CanExecute()` |
| 4 | Path-guard | `SPLA.Service/WorkspaceOps.IsUnderRoot` | путь внутри workspace | canonical prefix | **только** web-редактор (Fs* сервисного API) |

Ключевые факты, подтверждённые по коду:

- **Слои 1 и 2 дублируют одну таблицу** «Scope×Mode», заданную в двух местах, обязанную совпадать
  вручную. `ToolModeFilter` — UX-фильтр, `PermissionManager` — enforcement; правила реплицированы.
- **Слой 3 спрашивают только shell/build-инструменты и только `CanExecute()`**. `CanRead`,
  `CanWrite`, `CanNetwork` не вызывает **никто**: файловые инструменты ходят в
  `HostServices.Sandbox.Workspace` напрямую, минуя gate. «Ось источников» физически не подключена.
- **Слой 4 — ad-hoc**: `IsUnderRoot` живёт только в сервисном FS-API для web-редактора. Агент
  пишет файлы через слой 3 (не проверяет путь); web-редактор — через слой 4 (проверяет). Один
  вопрос — два ответа.

### Как принимается решение об исполнении (`McpHost.ExecuteToolAsync`)

`PermissionManager.CheckPermission(mode, def.Function, argsJson)`, порядок:
1. `Scope==Agent` → **всегда Allow**, в любом режиме, включая Chat (memory/info/datetime/context).
2. `Scope==Skill` → Ask/Allow/Deny по режиму.
3. Проектный override (`perm_read/write/shell/internet`) — жёсткий пол/потолок в любом режиме.
4. Session-remembered (только при `mode != Agent`) — матч по имени + (`*` или точный JSON args).
5. Матрица режима: Chat=Deny всё; Research=read+internet; Inspect=read+ask-internet;
   Edit=read+ask-write+ask-shell(кроме Danger→Deny); Agent=почти всё Allow, `Shell&Danger`→Ask.

`Allow` → исполнить; `Deny` → **строка-ошибка модели**; `Ask` → `PermissionScope.RequestAsync`
маршрутизирует запрос в UI начавшего turn клиента и блокирует до ответа.

### Sandbox / host-граница

`ISandbox` = `IWorkspace` + `IShell?` + `ICapabilityGate`; инструменты ходят через ambient
`HostServices.Sandbox` (`AgentSessionScope.Current.Sandbox` ?? `PassthroughSandbox.Default`).
Задумано как шов: soft-gate (проверки в коде) и hard-gate (OS-изоляция) дают одно решение.
**Реализован только passthrough** (`LocalWorkspace` = весь диск, `LocalShell` = powershell.exe,
`AllowAllGate` = всё true).

### Auth (серверный режим) — три режима, один cookie

Проверено по коду `SplaServiceHost`/`Auth/*`. Все три режима сходятся в cookie `spla.auth`
(HttpOnly, 10ч, sliding), после чего `/ws` и per-user области одинаковы:

- **None** (`--no-auth`/`--auth none`): единственный `LocalIdentity.Single`. Для loopback/embedded
  или доверенной LAN с `--token`. `--no-auth` на не-loopback без `--token` печатает предупреждение.
- **Negotiate** (default): NTLM/Kerberos на `/login` → cookie с UserKey(SID)+имя. Origin-gate на
  `/ws` (против CSWSH), token-gate (`_authenticated`: до успешного Hello при `RequiresToken` любое
  сообщение закрывает сокет) — обе дыры предыдущего дня закрыты, проверено.
- **Local** (`--auth local`, добавлен сегодня): `JsonUserStore` (`users.json`), пароли —
  PBKDF2 через `Microsoft.AspNetCore.Identity.PasswordHasher` (transparent re-hash при апгрейде
  алгоритма), seed-admin со случайным паролем в консоль при пустом store, self-registration
  (`/register`, включена по умолчанию), admin-панель `/admin` (роль-gated), self-service
  `/account`, last-admin guard.

Что в local-auth **сделано правильно** (проверено): open-redirect guard (`SafeReturn` — только
локальные пути), HTML-экранирование пользовательского ввода (`WebUtility.HtmlEncode` в
`AuthPages`), клиент-корректная семантика отказа (401/403 для `/admin/api`, редирект для
навигаций), нет захардкоженного дефолтного пароля, admin-панель без внешних ассетов.

---

## 2. Пути эскалации и опасные дефолты (по убыванию критичности)

### C1 — Сервер выполняет команды без изоляции (Critical, не исправлено; аудитория расширена)
[ChatRuntime.cs:112](../../src/agent/SPLA.Runtime/ChatRuntime.cs):
`new AgentSession(_sessionKv, _checkpoint, _skillSession)` — **без параметра `sandbox`** →
`PassthroughSandbox.Default`. На сервере turn исполняется от сервисного аккаунта. Любой
аутентифицированный пользователь, переключив чат в Agent (`Scope=Shell/High` в Agent = auto-Allow,
см. C4), выполняет произвольные команды и читает/пишет любой путь. Per-user области изолируют
только *листинг проектов*, не *действия инструментов*.

**Новое за день:** режим `--auth local` с включённой по умолчанию self-registration означает, что
теперь RCE-поверхность открыта не только доменным пользователям, но всякому, кто может достучаться
до `/register`. `docs/readme_auth.md` это честно оговаривает («run the server only where every
authenticated user is trusted to run commands on the host»), но это операционное предупреждение,
а не технический контроль.
→ **Минимальный фикс**: в серверном режиме собирать per-user `AgentSession` с
`PassthroughSandbox(workspace: bounded to userArea, shell: null, gate: WorkspaceGate(userArea))`.
Шов готов — `AgentSession` уже принимает `ISandbox`; проброс через `ChatRuntime`-конструктор.
До внедрения: `--no-register` и явно доверенный круг пользователей.

### C3 — Произвольный projectId → чужие проекты (High, не исправлено)
`ProjectHandlers.Open` и [ClientConnection.Resolve](../../src/service/SPLA.Service/Hosting/ClientConnection.cs)
вызывают `registry.Open(любой ProjectId = путь .spla)` без сверки с `_userArea`. В серверном
режиме `_userProvider` скоупит только `List()/Recent()`, но `Open`/`Resolve` идут в **общий**
`_registry`. Пользователь, зная/подобрав путь, открывает чужой `.spla` со всеми секретами и
историей. Хуже: `/chat-image/{chatId}/{file}?project=` в
[SplaServiceHost.cs:220](../../src/service/SPLA.Service/Hosting/SplaServiceHost.cs) делает
`registry.Open(project)` для **произвольного пути по HTTP** — утечка файлов + вектор исчерпания
ресурсов (строит runtime на каждый путь). `/plugin-assets` резолвит из default-проекта, но тоже
без скоупа.
→ **Фикс**: единая canonical-prefix-проверка `projectId` против `_userArea` в `Resolve` (одна
точка) + тот же резолвер для `/chat-image` и `/plugin-assets`; в local-режиме поведение не
меняется. Далее — share-ACL по группам (группы уже текут в identity).

### C4 — Самодекларация Scope/Effect/Risk + auto-allow shell (High, не исправлено)
Классификация инструмента — его собственное свойство в C#-коде (`Scope=…, Risk=…`); ядро доверяет
без проверки. Следствия:
- Плагинный DLL, объявивший `Scope=Agent`, исполняется **в любом режиме, включая Chat**, без Ask
  (правило 1 `PermissionManager`). Плагин — сторонний код в `PluginLoadContext`.
- `ToolRisk.Danger` **не присвоен ни одному** инструменту в репозитории. Единственная ветка, где
  Agent спрашивает про shell — `Risk==Danger`. Значит `system_run_shell` и `roslyn_script_run`
  (`Scope=Shell`) в Agent — **всегда Allow без подтверждения**. Записано в `agents/security.md`
  («Caveats»), но от честности дыра не закрылась.
→ **Фикс**: (а) `McpHost.RegisterTool` отклоняет `Scope∈{Agent,Skill}` для инструментов из
плагинов (плагин опознаётся по префиксу имени `[plugin].`, конвенция enforced
`ToolNameConventionTests`); (б) капабилити-заявка в `meta.yaml`, ядро валидирует фактический Scope
против заявки; (в) на сервере трактовать любой `Scope=Shell` как минимум Ask (или Deny при
`Shell=null`).

### H2 — Небезопасные дефолты (Medium→High в сумме)
- **Дефолтный режим нового проекта — `Edit`** ([ConfigLoader.cs:59](../../src/core/SPLA.Domain/Settings/ConfigLoader.cs)):
  запись + ask-shell сразу, при том что README позиционирует Chat/Research как безопасные ступени.
  На сервере дефолт должен быть ниже (`Research`/`Inspect`).
- **PFX-пароль — константа `"spla"`** ([ServiceOptions.CertPassword](../../src/service/SPLA.Service/Hosting/SplaServiceHost.cs)):
  самоподписанный серт с известным паролем; `--cert-password` есть, но дефолт небезопасен.
- **HTTP по умолчанию при cookie-auth**: `UseHttps` выключен по умолчанию, cookie не имеет
  `SecurePolicy=Always` — на не-loopback без `--https` `spla.auth` (это и есть credential) ходит
  открытым текстом. `readme_auth.md` рекомендует `--https`, но код не форсит.
- **Секреты plaintext** (`FileSecretStore` — YAML на диске); `--token`/пароль в argv видны в
  списке процессов. `SecretResolver.Resolve` — sync-over-async.

### H3 — Local-auth: точечные замечания (Medium)
Реализация в целом добротная; что стоит поднять:
- **Нет lockout/rate-limit на `/login`** — онлайн-перебор пароля ничем не ограничен. Добавить
  экспоненциальную задержку/блокировку по учётке или IP.
- **Self-registration включена по умолчанию** — в связке с C1 (нет sandbox) это означает
  «любой прохожий регистрируется и получает shell на сервере». Дефолт стоит инвертировать
  (`--allow-register` вместо `--no-register`) либо жёстко связать с наличием sandbox.
- **`MinPasswordLength = 6`** — слабовато для сетевого сервиса; поднять до 8–10 и/или проверять
  по списку скомпрометированных.
- Cookie `ExpireTimeSpan = 10ч` + sliding — приемлемо, но при shared-машинах короче безопаснее.

### H4 — Remembered-разрешения слишком грубые (Medium)
Матч по точному JSON аргументов почти никогда не повторится → на практике remember = `*` (весь
инструмент). Хранится на уровне runtime → «запомнить» в одном чате действует на все чаты проекта;
на shared-сервере — на всех пользователей проекта.

---

## 3. Целевая архитектура безопасности (стройная и расширяемая)

Проблема не в «мало проверок», а в **четырёх слоях, отвечающих на один вопрос по-разному**.
Цель — свести к **одному центру решения** с чёткими ролями каждого участника.

### 3.1 Принцип: одно решение, нормализованное действие

Каждый инструмент, желающий тронуть систему, выражает намерение как **нормализованное действие**:

```
Action = ReadPath(p) | WritePath(p) | Execute(cmd) | Network(host) | AgentLocal
```

Единственный enforcement-центр — **`ICapabilityGate` ядра** — отвечает `Allow/Ask/Deny` на
`Action`. Всё остальное (`ToolModeFilter`, `PermissionManager`, `WorkspaceOps`) становится
**построителями политики этого gate'а**, а не независимыми проверками. Это убирает расхождение
«агент пишет минуя проверку пути, редактор — с проверкой»: оба идут через `Gate.CanWrite(path)`.

### 3.2 Две оси, как уже задумано в доктрине

- **Ось «права»** (Read/Write/Execute/Network) — *что за класс действия*. Задаётся режимом
  + проектным override. Уже реализовано в `PermissionManager`.
- **Ось «источники»** (какие пути/хосты видимы/доступны) — *над чем*. Сейчас `AllowAllGate`
  везде; должна прийти как `WorkspaceGate(roots)` + конфиг `workspace.roots`/`extra_paths`.

Решение = пересечение обеих осей. Сегодня работает только первая; вторая физически не подключена.

### 3.3 Роли слоёв в целевой модели

| Роль | Ответственность | Ближе к |
|------|-----------------|---------|
| Манифест плагина | *заявляет* максимум капабилити (`capabilities:` в meta.yaml) | plugin layer |
| `McpHost.RegisterTool` | *валидирует* фактический Scope ≤ заявленного; запрещает Agent/Skill плагинам | **core** |
| `PermissionManager` | строит **политику прав** (режим × override) → нормализованное `Effect` | **core** |
| `ICapabilityGate` (Hard/Soft) | **единственный enforcement** по `Action` (права ∧ источники) | **core** |
| `ISandbox` реализация | *механизм* (passthrough / bounded / OS-isolated) под решение gate'а | core/host |
| `ToolModeFilter` | UX: не показывать модели то, что gate точно запретит (выводится из gate) | agent |
| Ask-маршрутизация | `PermissionScope` → клиент | service |
| Аудит | лог каждого `Gate.Decide(Action)` → observability-плоскость (`/stats`) | service/obs |

**Правило «что ближе к ядру»**: любое *решение* (можно ли) — в ядре, у gate'а; любой *механизм*
(как физически ограничить) — в host/sandbox; *заявка* капабилити — в манифесте плагина; *UX*
(что показать) — в agent-слое. Плагин **никогда** не источник решения о собственных правах.

### 3.4 Что это даёт по расширяемости

- Новый инструмент не добавляет проверок — лишь формулирует `Action`; политика и enforcement есть.
- Новый режим/профиль = новая функция «Mode → права», не трогает инструменты.
- Серверная изоляция = замена реализации `ISandbox` (SoftGate→HardGate), контракт неизменен.
- Аудит: единая точка `Gate.Decide(Action)` — естественное место лога; observability-плоскость
  (`TelemetryCollector` + `/stats`), добавленная сегодня, — готовый приёмник этих событий.

### 3.5 Порядок внедрения (безопасными шагами)

1. **Подключить существующий gate к fs-инструментам**: `FsWrite/Delete/Create` → `Gate.CanWrite`,
   `FsRead/List/Search` → `Gate.CanRead`. При `AllowAllGate` поведение не меняется — но ось
   источников становится физически подключённой.
2. **`WorkspaceGate(roots)`** (roadmap B.4): реальная проверка границы. Локально по умолчанию
   `AllowAllGate`; на сервере — обязательно `WorkspaceGate(userArea)`.
3. **Проброс sandbox в `ChatRuntime`** (закрывает C1): per-user на сервере, passthrough локально.
4. **Валидация Scope при регистрации** (закрывает половину C4): плагинам запрещены Agent/Skill.
5. **Манифестная заявка капабилити** + показ пользователю (вторая половина C4).
6. **Единый резолвер проекта** с user-area-проверкой (закрывает C3) — включая `/chat-image`.
7. **Выведение `ToolModeFilter` из gate'а**: устранить дублирование таблицы Scope×Mode.
8. **HardGate** (Strategic): OS-изоляция, после чего shell возвращается на сервер.

---

## 4. Приоритезация (что делать до любого нового функционала сервера)

**Немедленно (дни):** C1-минимум (`Shell=null` + bounded workspace на сервере) — теперь тем
острее, что `--auth local` расширил аудиторию; C3 (user-area-валидация, включая `/chat-image`);
C4-шаг-а (запрет Agent/Skill плагинам); H2 (дефолт `Research`, PFX без константы, `SecurePolicy`
для cookie); H3 (lockout на `/login`, инвертировать дефолт self-registration).

**Скоро (недели):** `WorkspaceGate` + полный per-user sandbox; подключение gate к fs-инструментам;
единый резолвер; аудит-лог решений в observability-плоскость.

**Стратегически (месяцы):** HardGate/OS-изоляция; манифестные подписанные капабилити; share-ACL
по группам; permission-профили per-identity (сейчас `Capabilities.Full` всем аутентифицированным).

Всё согласовано с `docs/roadmap.md` (секции B/D/E). Пока сервер без изоляции, а auth-поверхность
растёт — это приоритет №1: каждый новый способ впустить пользователя без sandbox масштабирует RCE.
