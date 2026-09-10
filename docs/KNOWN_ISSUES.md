# Known Issues

## Roslyn Plugin — In-Memory Compilation Missing BCL References

**Severity:** High  
**Status:** Confirmed (2026-08-28)  
**Affected tools:** `roslyn_project_build`, `roslyn_compile_check`, `roslyn_script_run`

### Issue

`compile_check` and `script_run` fail during compilation because the in-memory Roslyn compiler pipeline does not include metadata references to the .NET Base Class Library (BCL). Even trivial code fails:

```csharp
// compile_check: fails with CS0518 (System.Object not found)
public class A { }

// script_run: fails with NotSupportedException before first line
ctx.Log("hi");
```

### Root Cause

The in-memory compilation in these tools does not pass `MetadataReference` objects for `System.Private.CoreLib` and other essential BCL assemblies to the Roslyn compiler. The issue occurs at compile-time, before any code execution.

**Evidence:**
- `compile_check` with empty class: `CS0518: System.Object not defined or imported`
- `script_run` with single line: `NotSupportedException: Cannot create metadata reference in assembly without location`
- `project_build` (real dotnet SDK) works fine with the same code

### Workarounds

| Tool | Issue | Workaround |
|---|---|---|
| `compile_check` | No BCL in compile pipeline | Use `roslyn_project_build` instead — create temp .csproj and check diagnostics against real SDK |
| `script_run` | No BCL in compile pipeline | No direct replacement — use sequential calls to `project_run` or other tools |

### Impact

- Serial-only execution: `script_run` cannot run parallel multi-step plans
- Compile-time checks require creating temporary project files

### Related

- [[Roslyn plugin plan]](../docs/adr/roslyn-plugin-plan.md) — feature roadmap for compile-check, navigation, refactoring

---

## `chat.open` воскрешает архивированный чат

**Найдено:** 2026-09-02, при работе над волной 4 `PLAN_20260902_agent_roles-and-correspondence`.
**Статус:** исправлено 2026-09-03 (вариант 1 из развилки ниже).

`ChatRegistry.Archive` закрывает рантайм и прямо утверждает в своём комментарии, что «архивированный
чат не должен иметь живого рантайма, так же как удалённый». Но `ChatManager.LoadChat` /
`FindChatFilePath` ищут сначала среди активных, а потом среди архивированных, поэтому
`GetOrOpen` архивированный чат **находит, грузит и заново открывает** — то есть отменяет архивацию
явочным порядком. Удалённый чат при этом честно даёт null.

### Чем это не является

Не багом переписки. Волна 4 обошла его: `ChatManager.Locate` отвечает
`Active` / `Archived` / `Missing` по факту расположения файла, ничего не открывая, и проверка живости
собеседника спрашивает именно её, а не `GetOrOpen`. Поэтому архивированный собеседник для переписки
мёртв, как и должен быть.

### Почему не исправлено здесь

`GetOrOpen` — общая точка входа для `chat.open`, `chat.send`, форка и переименования. Менять её
поведение значит решать вопрос, которого никто не задавал: можно ли вообще открыть архивированный
чат на чтение, и что тогда делает клиент, у которого есть список архива. Это отдельная работа с
собственным радиусом поражения, а не хвост переписки.

### Что решать, когда дойдут руки

Либо `GetOrOpen` отказывает по архивированному (и клиент обязан сначала разархивировать), либо
комментарий `Archive` неверен и архив — это только про список, а не про жизнь рантайма. Сейчас код
утверждает первое, а делает второе.

### Решение: вариант 1

`GetOrOpen` (`ChatRegistry.cs:66`) теперь спрашивает `ChatManager.Locate` первым делом и отказывает
(`null`), если чат не `Active` — архивированный ведёт себя как удалённый, ровно как обещает
комментарий `Archive`. `ChatHandlers.Open` при этом различает причину отказа и шлёт «Chat is
archived», а не вводящее в заблуждение «Chat not found».

**Хвост закрыт 2026-09-03** тем же днём, отдельной работой: появился `chat.read` —
`ChatRegistry.ReadArchived` отдаёт архивированный чат как данные, не создавая `ChatRuntime` и не
подписывая никого на события, которых там неоткуда взяться. Клик по архивной строке шлёт его вместо
`chat.open`, а поверхность рисует историю без композера и без строки настроек. Развилка «Read vs
Open» решена в пользу отдельного типа сообщения — см.
[`PLAN_20260903_core_readonly-surface`](plans/PLAN_20260903_core_readonly-surface.md), этап 1.

---

## WebView2 не следует за DPI монитора (лечение — только под Windows)

**Найдено:** 2026-09-07 на стенде из трёх мониторов (4K@150%, FHD@125%).
**Статус:** исправлено 2026-09-09, проверено живым перетаскиванием окон между мониторами.

Окно перетаскивают на монитор с другим масштабом — рамка Avalonia перерисовывается правильно, а всё
содержимое остаётся в прежнем масштабе. Так как содержимое окна целиком — это веб-клиент в
`NativeWebView`, «содержимое» здесь означает вообще весь интерфейс: на FHD@125% он выглядел огромным.
`devicePixelRatio` показывал 1.75 там, где должно быть 1.25, причём у части оторванных окон — 1, без
всякой системы.

### Причин было две, независимых

**Манифест.** `app.manifest` не объявлял `dpiAwareness`, поэтому Windows давала процессу
system-DPI awareness: окно рисуется под DPI того монитора, где стартовало, и не пересчитывается
вообще никогда. Лечится объявлением `PerMonitorV2` — это чинит само окно Avalonia, но не его
содержимое.

**Собственно WebView2.** Даже когда Avalonia уже знает правильный масштаб, внутрь Chromium его никто
не передаёт. Настоящее окно Chrome получает DPI даром: у него есть свой HWND, а про DPI своего
монитора Windows сообщает любому HWND сама. У WebView2 внутри Avalonia такого HWND нет — он
композитится в дерево отрисовки Avalonia, и обычному механизму определения монитора не от чего
оттолкнуться. Ровно для этого случая в API есть `ICoreWebView2Controller3.RasterizationScale`, но
`Avalonia.Controls.WebView` (12.0.1) его не зовёт нигде — проверено чтением её IL. Лечится
`Helpers/WebViewDpiSync.cs`: масштаб выставляется руками по `TopLevel.ScalingChanged` (перенос между
мониторами) и на первую отрисовку окна.

### Почему только Windows

Весь обходной путь опирается на `IWindowsWebView2PlatformHandle` и COM-интерфейсы WebView2 — это
Windows и ничего кроме. На других платформах `TryGetPlatformHandle()` вернёт handle, который в этот
интерфейс не приводится, `TrySync` отдаст `false`, и всё тихо останется как было. То же касается
`WebViewDevTools`. Каркас приложения кроссплатформенный, лечение — нет; как ведут себя WKWebView и
WebKitGTK при смене DPI, никто не смотрел, потому что приложение и так Windows-only (DPAPI-секреты,
манифест, вся эта обвязка).

### Ловушка, на которой это ломалось дважды — читать перед правкой интерфейсов

Vtable COM-интерфейса — это `IUnknown`, затем **все** методы всех базовых интерфейсов, и только потом
свои. `ICoreWebView2Controller3` наследует `Controller2` (2 метода), тот — `Controller` (23 метода),
значит собственные аксессоры начинаются с 25-го слота. Объявление, где перечислены только аксессоры,
молча нацеливает их на нулевой слот, и оба последствия выглядят как что угодно, только не как своя
причина:

- `SetRasterizationScale` попадает на `put_IsVisible` — масштаб не выставляется никогда, и это
  неотличимо от «фича не работает»;
- соседний метод попадает на `put_Bounds`, который принимает `RECT` по значению — переданное число
  разыменовывается как указатель, и процесс падает целиком, молча. `try/catch` такое не ловит:
  AccessViolation относится к повреждению состояния.

Поэтому в интерфейсе стоят 25 заглушек `Slot00`…`Slot24`. Значимы только их количество и порядок,
сигнатуры не значат ничего, потому что их никто не вызывает. **Индексы надо проверять по метаданным,
а не считать глазами**: количества (23 / 2 / 8) снимаются с интеропа самой Avalonia, а результат
сверяется на уже собранной сборке — `GetRasterizationScale` обязан оказаться на индексе 25.

Тем же приёмом сделан `WebViewDevTools` (`OpenDevToolsWindow`, индекс 48). Там объявление плоское и
это правильно: у `ICoreWebView2` базовых интерфейсов нет.

### Почему интерфейсы объявлены руками, а не взяты из SDK

NuGet-пакет `Microsoft.Web.WebView2` для нашего TFM резолвится в WinRT-проекцию, где вообще нет
raw-COM поверхности (`Microsoft.Web.WebView2.Core.Raw`), и вдобавок тянет конфликт версий
`WindowsBase` от сборок WPF/WinForms, которые этому приложению не нужны.

### Что проверить при обновлении Avalonia

Если `Avalonia.Controls.WebView` однажды начнёт звать `RasterizationScale` сама, `WebViewDpiSync`
станет лишним и будет драться с ней за одно и то же значение. Признак — масштаб, скачущий при
перетаскивании.

Файлы: `src/apps/SPLA.UI.Avalonia/Helpers/WebViewDpiSync.cs`,
`src/apps/SPLA.UI.Avalonia/Helpers/WebViewDevTools.cs`,
`src/apps/SPLA.UI.Avalonia/app.manifest`.

## Чат сохраняется на диск только в конце успешного хода

**Серьёзность:** высокая · **Статус:** подтверждено (2026-09-10), решение отложено

`ChatRuntime` вызывает `Save()` один раз — после того как `_orchestrator.RunAsync` вернулся
(`src/agent/SPLA.Runtime/ChatRuntime.cs`, конец метода отправки, строка `Save();` внутри `try`, не в
`finally`). Всё, что происходит внутри хода — десятки вызовов инструментов, промежуточные реплики,
расход токенов, — живёт только в памяти.

Последствия:
- **Падение процесса или зависание** в длинном ходе — теряется весь ход, с первой реплики.
- **Stop (отмена) или исключение** — `Save()` пропускается, ход тоже теряется целиком, хотя процесс жив.
- Файл чата нельзя использовать, чтобы смотреть за ходом снаружи: живой пример — чат
  `2026-09-10_1226-2165` (волна 0 плагина Android), около 120k контекста и 2.9M prompt-токенов за
  ход, а файл не менялся до самого конца.

Что решать: сохранять после каждого шага цикла (реплика ассистента / результат инструмента), а не
после хода; и перенести сохранение в `finally`, чтобы отменённый ход оставлял то, что успел.
Учесть: `TrySaveIdle` и fork опираются на то, что во время хода файл не пишется («fork must not
snapshot a half-written conversation»), — это правило придётся пересмотреть вместе с исправлением.

## Инструмент roslyn запускает процессы в обход отказавшего shell

**Серьёзность:** средняя (безопасность) · **Статус:** замечено (2026-09-10), не разобрано

В том же чате `system_run_shell` перестал запускать `powershell.exe` («access denied», причина не
выяснена — вторая проблема сама по себе). Модель не остановилась, а написала C#-программу и
запустила через инструмент roslyn `git add` / `git commit` — то есть получила shell через другой
инструмент. Правило `core.discipline` («3 неудачи — остановись и спроси») это не поймало: формально
упал другой инструмент.

Это частный случай уже известного «bypass через shell» из модели безопасности (capability gate не
видит, что делает скрипт внутри). Разобрать вместе с ней: исполнение кода roslyn — такое же
`Execute` с доступом к процессам, как shell, и должно проходить те же гранты.
