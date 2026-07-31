# SPLA.Plugins.Browser — план (Playwright)

Плагин для полного управления браузером агентом: загрузка сайтов, навигация,
клики/ввод/перетаскивание, скриншоты, semantic snapshot, ожидания, диалоги, сеть.
Движок — **Microsoft.Playwright** (.NET). id плагина: `browser`.

> Статус: Волна 1 (MVP) РЕАЛИЗОВАНА (2026-06-30) — `SPLA.Plugins.Browser`, 21 тул + `image_view`
> (ядро). Solution + тесты зелёные (153/153). Реальный E2E прогон (запуск Edge и навигация) не
> подтверждён в среде разработки — `msedge --headless` падает с "Target page, context or browser
> has been closed" в этом контейнере (Edge бинарь присутствует и стартует, но пайп не поднимается;
> похоже на ограничение песочницы, не баг плагина). Нужна проверка на обычной машине/в сервисе.
> Волна 2/3 — НЕ начаты, см. ниже.

---

## 0. Как это ложится на существующую архитектуру

Сверено с кодом (Network/SQL/Roslyn плагины):

- **Контракт.** `ISplaPlugin.Initialize(ResolvedSettings) → IEnumerable<IMcpTool>`.
  Каждый инструмент — `IMcpTool { Name; GetDefinition(); ExecuteAsync(json, ct) → Task<string> }`.
  Возврат ТОЛЬКО строка. См. `NetworkPlugin.cs`, `HttpGetTool.cs`.
- **Session-scoped состояние.** Браузер живёт между вызовами инструментов → нужен
  один общий объект на сессию, как `SqlConnectionRegistry`: создаётся в
  `Initialize`, передаётся по ссылке во все инструменты. Для нас это
  **`BrowserSessionManager`** (держит Playwright, browser, контексты, страницы).
- **Per-chat изоляция.** Каждый чат —自己 VM ([[per-chat-autonomy]]). `Initialize`
  вызывается на сессию, так что `BrowserSessionManager` уже per-chat. Никакого
  глобального синглтона браузера — иначе два чата подерутся за страницы.
- **Прогресс.** Долгие операции (navigate, wait_*) репортят через
  `ProgressScope.Report(...)` ([[progress-tree]]) — как в `ScriptContext`.
- **Permissions.** `ToolDefinition.Function` несёт `Scope/Effect/Risk`. Браузер —
  `Scope = Internet`. Read-операции (snapshot/screenshot/inspect/get_text) →
  `Effect.Read, Risk.Low`. Действия (click/fill/press/navigate) → `Effect.Write`
  (или `Network`), `Risk.Medium`. Опасное (eval, route, set_files, cookies_set) →
  `Risk.High/Danger`. Ядро уже умеет Ask/Deny по этим полям — мы НЕ пишем свой
  confirm с нуля, а выставляем правильные Risk + полагаемся на pipeline.
- **Self-contained сборка.** ОБЯЗАТЕЛЬНО `<CopyLocalLockFileAssemblies>true</...>`
  ([[plugin-alc-native-dlls]]) — иначе `Microsoft.Playwright.dll` + натив не
  доедет до `plugins/browser/` под CLI/service-хостом.
- **meta.yaml** с `id: browser`, `type: dll`, `entry_point`, `default_prompt`
  (справка модели — список инструментов и правила, как у sql/meta.yaml).
- **slnx.** Сразу добавить проект в `SPLA.slnx` + PublishAll/PublishAll.cmd +
  ссылку из `SPLA.Tests` ([[feedback-slnx-sync]]). CopyPlugin-таргет в csproj
  (копия в `SPLA.UI.Avalonia/bin` и `SPLA.CLI/bin`), как у Network/Sql.

### Профиль браузера: project / detected / new (УТВЕРЖДЕНО пользователем, реализовано)

`browser_start` НЕ выбирает профиль молча. Три категории, всегда через `agent_clarify`
(`ClarifyScope.AskAsync`), если `profile` не передан явно:
- **`"project"`** — `.spla/browser-profile/` в воркспейсе, персистентный, копится между запусками.
- **detected** — реальные профили Edge/Chrome с этой машины (`BrowserProfileDiscovery` читает
  `Local State` каждого браузера, без доступа к содержимому). Выбор такого профиля даёт агенту
  доступ к сохранённым логинам/кукам пользователя — поэтому ВСЕГДА явное согласие через clarify,
  никогда не выбирается по умолчанию. `browser_list_profiles` — read-only превью того же списка.
- **`"new"`** — временный пустой профиль (как раньше). Дефолт, если clarify недоступен
  (автономный/headless режим — без диалога с пользователем рискованный профиль не трогаем).

Технически: реальный профиль = `userDataDir` (корень "User Data") + `Args=["--profile-directory=
<folder>"]` — Chromium не даёт указать суб-профиль просто путём, нужен именно этот флаг
(`BrowserSessionManager.LaunchAsync` теперь принимает `extraArgs`). Channel при выборе detected-
профиля принудительно ставится в channel этого браузера (Edge-профиль ⇒ только msedge).

### Два архитектурных решения (УТВЕРЖДЕНО пользователем)

1. **Бинарники Playwright → «оба, опцией».** `browser_start` принимает параметр
   `channel`:
   - `channel: "msedge" | "chrome"` — системный браузер, без скачивания (быстро).
   - `channel: "chromium"` (или отсутствует) — управляемый Playwright Chromium;
     если бинарей нет (`%LOCALAPPDATA%\ms-playwright` пуст) — поставить программно
     `Microsoft.Playwright.Program.Main(["install","chromium"])` (один раз, с
     прогрессом через `ProgressScope`).
   - **Default для MVP:** пробовать системный Edge, при отсутствии — fallback на
     install+chromium. Не тащить ~150MB в репозиторий/паблиш.

2. **Скриншот ОБЯЗАН дойти до модели как НАСТОЯЩАЯ картинка.** В SPLA картинки
   уходят в модель только через `ChatMessage.Images` (data-URL) на сообщении с
   мультимодальным контентом (`BuildMultimodalContent` → `image_url` parts,
   `LMStudioClient.cs:412`). Tool-роль сообщение картинку надёжно НЕ донесёт (не
   все vision-API принимают image в tool-role). Поэтому:

   **Механизм (реализовать в ядре `SPLA.Agent`, переиспользуемо любым плагином):**
   - Вводим ambient image-sink на активной сессии чата — по образцу
     `ProgressScope`/`AgentSessionScope` ([[per-chat-autonomy]], [[data-channel-blob]]).
     Напр. `AgentSessionScope.Current` получает очередь `PendingImages` (data-URL).
   - `browser_screenshot` кладёт PNG в `BlobStore` (data-channel) И толкает data-URL
     в этот sink; возвращает модели короткую строку-summary (что снято, размеры,
     `blob:`-handle).
   - `ConversationOrchestrator` после добавления tool-результата (точка
     `ConversationOrchestrator.cs:211`) **дренирует** sink: если есть картинки —
     добавляет эфемерное **user**-сообщение с `Images = [...]` (по готовому шаблону
     эфемерной инъекции, как rollback на `ConversationOrchestrator.cs:243`,
     `IsEphemeral = true`). Текст части — «[browser screenshot]». Модель видит
     картинку настоящим image_url на следующем шаге.
   - **Бонус (идея пользователя, делаем заодно — generic):** Agent-scoped тул
     `image_view`/`context_image(handle)` — берёт `blob:`-handle картинки из
     `BlobStore` и толкает в тот же sink. Тогда модель может САМА подтянуть в
     контекст любую сохранённую картинку (не только свежий скриншот). Полезно вне
     браузера. Scope=Agent, Effect=Read.

   Итог: плагин остаётся чистым (produce bytes → blob + sink), вся «магия»
   инъекции живёт в ядре один раз. `browser_snapshot`/`get_text` (текст) —
   по-прежнему основной канал восприятия, скриншот — настоящий визуальный довесок.

---

## Внутренние сущности (сервисный слой, не инструменты)

Строить инкрементально — в MVP нужны только первые четыре.

- **`BrowserSessionManager`** (MVP) — владеет `IPlaywright`, `IBrowser`/контекстом,
  жизненным циклом. start/stop/status. Реализует `IAsyncDisposable`; диспоуз при
  завершении сессии чата.
- **`BrowserPageRegistry`** (MVP) — вкладки/страницы по `tabId` (стабильный id,
  напр. `t1,t2…`), активная вкладка, подписка на popup.
- **`ElementRefRegistry`** (MVP) — сопоставляет `ref` (`e12`) ↔ Playwright locator,
  привязан к версии последнего snapshot. Stale-detection: ref из старого snapshot
  → внятная ошибка «сделай browser_snapshot заново».
- **`BrowserSnapshotService`** (MVP) — строит компактное semantic-дерево
  интерактивных элементов с `ref`, role, name, value. Источник — Playwright
  accessibility snapshot (`Page.Accessibility` / `Locator.AriaSnapshot`), а НЕ
  сырой DOM.
- **`BrowserActionExecutor`** (MVP, можно слить с инструментами) — общий resolve
  цели по приоритету `ref → role/name → label/text → CSS → координаты`, retry,
  единые ошибки.
- **`BrowserScreenshotService`** (MVP) — viewport/full-page/element → PNG → blob/файл.
- `BrowserEventCollector` (Волна 2) — console, pageerror, dialog, download —
  буфер на страницу, читается `browser_console`/`browser_page_errors`/`browser_dialogs`.
- `BrowserDownloadManager` (Волна 2) — ожидание/сохранение/список загрузок + DownloadPolicy.
- `BrowserPolicyService` (Волна 2) — allowlist/blocklist доменов, опасные действия,
  каталоги upload/download.

---

## Приоритет resolve цели (единый во всех action-инструментах)

```
ref  →  role+name  →  label/text  →  CSS  →  XPath  →  координаты
```
`ref` (из последнего snapshot) — основной. Координаты — крайний случай.

---

# ВОЛНА 1 — MVP (делать сейчас)

Цель: модель открывает сайт, видит его через snapshot, кликает/вводит/жмёт,
читает текст/ошибки, делает скриншот, грузит файл. Persistent profile опционально.

### Жизненный цикл
- `browser_start` — запустить Chromium (или channel=msedge/chrome), profile:
  temporary | persistent(path). Опции: headless(bool, default false для контроля),
  viewport. Если бинарей нет — поставить (или внятная ошибка с инструкцией).
- `browser_stop` — закрыть браузер, освободить процессы.
- `browser_status` — состояние: запущен ли, профиль, активная вкладка, список вкладок.

### Вкладки
- `browser_tabs` — список: tabId, url, title, active.
- `browser_new_tab` — открыть (опц. сразу url).
- `browser_switch_tab` — сделать активной по tabId.
- `browser_close_tab` — закрыть по tabId.

### Навигация
- `browser_navigate` — открыть url (на активной вкладке), ждать load по умолчанию.
  Параметр `wait_until`: load | domcontentloaded | networkidle.
- `browser_wait_navigation` — дождаться завершения перехода (после клика-ссылки).

### Состояние страницы (главное для модели)
- `browser_snapshot` — semantic-дерево интерактивных элементов с `ref`. ОСНОВНОЙ
  инструмент. Опц. фильтр по области/роли, лимит размера.
- `browser_screenshot` — viewport | full page | element(ref). → blob/файл + summary.
- `browser_inspect` — детали элемента по ref: role, name, value, attributes, bounds,
  + состояния visible/enabled/editable/checked/focused (см. п.16 спецификации —
  состояния отдаём ЗДЕСЬ, без отдельных is_visible/is_enabled инструментов).
- `browser_get_text` — видимый текст страницы или элемента (ref).

### Мышь / элементы
- `browser_click` — клик по ref | селектору | координатам. Опц. button, modifiers.
- `browser_scroll` — прокрутка страницы/контейнера (by/в элемент).

### Клавиатура / ввод
- `browser_fill` — очистить и установить значение поля (ref/селектор, value).
- `browser_press` — нажать клавишу/комбинацию (напр. "Enter", "Control+A").
- `browser_select` — выбрать значение(я) в `<select>`.

### Ожидания (чтобы модель не делала sleep)
- `browser_wait_element` — появление | исчезновение | состояние (visible/hidden/
  attached/detached) по ref/селектору, timeout.

### Файлы
- `browser_upload` — установить файлы в input[type=file] (ref/селектор + локальные
  пути). Ограничить разрешёнными каталогами (UploadPolicy — простая версия в MVP).

### Диагностика
- `browser_console` — собранные console-сообщения (log/warn/error) текущей страницы.
- `browser_page_errors` — необработанные JS-ошибки страницы.

**MVP-инварианты:**
- persistent profile (опция в `browser_start`),
- `ref`-элементы + stale snapshot detection,
- ограничение доменов (минимальный allowlist/blocklist из настроек плагина) и
  подтверждение опасных действий через `Risk` (navigate за пределы allowlist,
  upload — повышенный Risk),
- snapshot — не полный DOM.

### Risk-разметка MVP
| Инструмент | Effect | Risk |
|---|---|---|
| status, tabs, snapshot, screenshot, inspect, get_text, console, page_errors | Read | Low |
| start, stop, new_tab, switch_tab, close_tab, scroll, wait_element | Write/Read | Low |
| navigate, click, fill, press, select | Write | Medium |
| upload | Write | High |

---

# ВОЛНА 2 — расширение (после MVP)

Полнота управления, синхронизация, сеть, диалоги, окружение.

### Сессии / контексты
- `browser_new_context`, `browser_close_context`, `browser_list_sessions`,
  `browser_bring_to_front`, `browser_wait_popup`.

### Навигация
- `browser_back`, `browser_forward`, `browser_reload`,
  `browser_wait_load`, `browser_get_url`.

### Состояние страницы
- `browser_query` (поиск по role/text/label/CSS/XPath),
- `browser_get_html` (HTML элемента с ограничением глубины),
- `browser_page_info` (размер/viewport/scroll/frames).

### Мышь
- `browser_double_click`, `browser_right_click`, `browser_hover`,
  `browser_drag_drop`, `browser_mouse_move`, `browser_focus`,
  `browser_check`, `browser_uncheck`.

### Клавиатура
- `browser_type` (посимвольно), `browser_clear`, `browser_set_files`,
  `browser_clipboard_read`, `browser_clipboard_write`.

### Ожидания
- `browser_wait`, `browser_wait_text`, `browser_wait_url`,
  `browser_wait_stable` (затихание DOM/layout), `browser_wait_response`,
  `browser_wait_request`, `browser_wait_download`.

### Frames / Shadow DOM
- `browser_frames`, `browser_switch_frame`, `browser_main_frame`,
  `browser_frame_snapshot`. Shadow DOM — автоматически через Playwright locator.

### Файлы / загрузки
- `browser_downloads`, `browser_save_download`, `browser_cancel_download`.
  + полноценный `DownloadPolicy` (разрешённые каталоги/типы).

### Cookies / storage / авторизация
- `browser_cookies_get/set/clear`, `browser_storage_get/set`,
  `browser_storage_state_save/load`. Секреты/токены модели не показывать в открытую
  (`SecretRedactor`). Привязать к [[secrets-store-service-roadmap]].

### Сеть
- `browser_network_log`, `browser_get_response_body`, `browser_wait_response`.
  (route/перехват — Волна 3.)

### Диалоги / разрешения
- `browser_dialogs`, `browser_dialog_accept`, `browser_dialog_dismiss`,
  `browser_permissions_grant`, `browser_permissions_clear`.

### Эмуляция окружения
- `browser_set_viewport`, `browser_set_user_agent`, `browser_set_locale`,
  `browser_set_timezone`, `browser_set_geolocation`, `browser_set_color_scheme`,
  `browser_set_media`, `browser_set_extra_headers`.

### Политики (внутренний слой)
- `BrowserPolicyService`: DomainAllowlist/Blocklist, DangerousActionDetector
  (submit/delete/pay/publish), журнал всех действий модели.

---

# ВОЛНА 3 — повышенный риск / диагностика (по умолчанию ВЫКЛ)

- **JS/DOM (Risk.Danger, off by default):** `browser_eval`, `browser_eval_element`,
  `browser_expose_function`, `browser_add_init_script`.
- **Сеть-перехват:** `browser_route_add`, `browser_route_remove`,
  `browser_set_offline`, `browser_set_headers`.
- **Запись/трейс:** `browser_trace_start/stop`, `browser_video_start`,
  `browser_har_start/stop`, `browser_dom_changes`.

---

## Anti-detection / stealth — ОТКАЗАНО

Пользователь столкнулся с капчей Google при автоматизации (Playwright палится через
`navigator.webdriver`, флаги автоматизации и т.п.) и просил патчить fingerprint, чтобы обойти
детект бота. Это ОТКАЗАНО — целенаправленный обход защиты стороннего сервиса (detection evasion),
даже для личного агента. Вместо этого реализован раздел выше (project/detected/new profile) —
легитимный путь: реальный профиль с уже выполненным входом и решённой один раз капчей снижает
трение естественным образом, без подмены fingerprint'а. Если капча всё равно системно мешает —
для Google-поиска использовать `network_http_get`/обычный API, а не браузер: это заведомо
состязательная зона, оптимизировать плагин под победу в ней не будем.

## Сведено: п.16 «состояния элементов» и п.17 «безопасность»

- **Состояния элементов** (visible/enabled/editable/checked/selected/focused/value/
  text/boundingBox) — НЕ отдельные инструменты, а поля ответа `browser_inspect`.
  Не раздувать API. (Точечные `browser_is_visible` и т.п. — не делаем.)
- **Безопасность** (`BrowserPolicy`, `DangerousActionDetector`, `ConfirmationService`,
  `Download/UploadPolicy`, `SecretRedactor`, allowlist/blocklist, журнал) — это
  внутренний слой, НЕ инструменты модели. В MVP — минимум (домены + Risk-разметка +
  upload-каталог), полноценно — Волна 2. Подтверждение опасного НЕ изобретать: на
  максимум использовать существующий permission-pipeline через `Risk`.

---

## Порядок реализации MVP (для sonnet)

1. Проект `SPLA.Plugins.Browser` (net10.0, `CopyLocalLockFileAssemblies=true`,
   ссылки на `SPLA.Domain` + `SPLA.MCP.Core`, `Microsoft.Playwright`), `meta.yaml`,
   CopyPlugin-таргет. Добавить в `SPLA.slnx`, PublishAll, `SPLA.Tests`.
2. `browser_start` с `channel` (системный Edge/Chrome ИЛИ управляемый chromium с
   install-on-start fallback). См. решение 1.
   - **Ядро (отдельно от плагина):** image-sink на `AgentSessionScope` +
     дренаж в `ConversationOrchestrator` (эфемерное user-сообщение с `Images`) +
     generic тул `image_view(handle)`. См. решение 2. Это делается ОДИН раз и
     нужно прежде, чем `browser_screenshot` сможет реально показать картинку.
3. `BrowserSessionManager` + `BrowserPageRegistry` + lifecycle (start/stop/status).
4. `BrowserSnapshotService` + `ElementRefRegistry` (+ stale-detection) → `browser_snapshot`.
5. `BrowserActionExecutor` (resolve-приоритет) → click/fill/press/select/scroll.
6. navigate + wait_navigation + wait_element.
7. inspect / get_text / screenshot (screenshot → blob + image-sink ядра из шага 2).
8. console / page_errors (минимальный `BrowserEventCollector`).
9. upload (минимальный UploadPolicy).
10. `default_prompt` в meta.yaml: список инструментов + правила (snapshot перед
    действиями, ref-приоритет, не использовать sleep — есть wait_*, домены).
11. Тесты в `SPLA.Tests` (headless): open → snapshot → click → fill → assert.
12. После фундаментальных изменений инструментов — обновить `main_sys_prompt.md`?
    НЕТ (это плагин): справка живёт в `meta.yaml default_prompt`, ядро не трогаем
    ([[feedback-main-prompt-sync]] — про ядро; плагины через meta).

## Чего сознательно НЕ делаем в MVP
Контексты, back/forward/reload, query/get_html/page_info, double/right/hover/drag,
type/clipboard, frames, cookies/storage, сеть-лог, диалоги, эмуляция, eval/route/
trace/video/har. Всё — Волна 2/3 выше.
