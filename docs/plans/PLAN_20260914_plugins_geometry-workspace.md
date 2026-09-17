# PLAN_20260914_plugins_geometry — geometry workspace, первая версия

Статус: **в работе**. Волна 0 не начата.

Реализует [`ADR_20260914_plugins_geometry-workspace.md`](../adr/ADR_20260914_plugins_geometry-workspace.md):
там причины, здесь порядок работ. **Этот план без ADR не выполнять.**

Главное для владельца: **модель ставит геометрию не с первого раза, а циклом
«поставила → увидела рендер → поправила → приняла»**. Всё остальное в плане обслуживает этот цикл.

---

## Правила для исполнителя

1. Перед шагом прочитай: этот план целиком, ADR, `agents/plugins.md`, `agents/tool-args.md`,
   `agents/toolsets.md`, `src/plugins/AGENTS.md`. Образец плагина — `src/plugins/SPLA.Plugins.Browser`
   (сессия на чат, картинка доходит до модели), образец полного `meta.yaml` —
   `src/plugins/SPLA.Plugins.Android/meta.yaml`.
2. **Один шаг = один коммит.** Сообщение — по-английски, `feat(geometry): ...`. В `git add`
   перечисляй файлы явными путями, никаких `git add .` и `git add <каталог>`: в дереве лежит чужая
   незакоммиченная работа.
3. После каждого шага: `dotnet build SPLA.slnx` без новых предупреждений в твоих файлах,
   `dotnet test tests/SPLA.Tests --filter FullyQualifiedName~Geometry` зелёный.
4. Трогай **только** файлы, названные в шаге. Нужен файл вне списка — остановись и спроси.
5. **Стоп-условия** (остановиться и написать человеку, не «чинить» самому):
   - нужно менять ядро (`src/core`, `src/agent`) вне волны 4;
   - нужен NuGet-пакет, кроме `SkiaSharp` (волна 0);
   - реальный размер рендера не совпадает с объявленным моделью пространством координат.
6. Новые файлы `docs/` и новый проект добавляй в `SPLA.slnx` сразу.
7. Текст, который видит модель (описания инструментов, `Details`, `default_prompt`, тексты ответов
   инструментов) — **только по-английски** (`agents/sys_prompt_rules.md`).

---

## Инвариант, который держит всю конструкцию

**Модель работает только в пикселях той картинки, которую ей отдали.**

Каждый рендер отдаётся в рабочем размере (длинная сторона ≤ `render_max_side`, по умолчанию 1024), и
именно этот размер объявляется системой координат в тексте ответа. Масштаб исходник→рендер — часть
матрицы вида. Модель никогда не называет координаты в пикселях исходника и никогда не делает
пересчётов.

Из этого следует всё остальное: геометрия хранится канонически в координатах **источника**, а в ответы
печатается в координатах **текущего вида**; пересчёт — только внутри инструмента.

---

## Волна 0 — каркас и графика

### 0.1. Проект и регистрация

Создать:

- `src/plugins/SPLA.Plugins.Geometry/SPLA.Plugins.Geometry.csproj` — копия
  `SPLA.Plugins.Browser.csproj` с заменой `browser` → `geometry`, `Microsoft.Playwright` заменить на
  `SkiaSharp` (последняя стабильная 2.x), добавить `<InternalsVisibleTo Include="SPLA.Tests" />`.
  `CopyLocalLockFileAssemblies` оставить `true` — плагин грузится в свой `PluginLoadContext`.
- `meta.yaml`: `id: geometry`, `version: 0.1.0`, `type: dll`,
  `entry_point: SPLA.Plugins.Geometry.dll`, `description`, `summon` и `default_prompt` — тексты из
  раздела «Промпт плагина» внизу плана.
- `GeometryPlugin.cs`: `public sealed class GeometryPlugin : ISplaPlugin`. `Initialize` **захватывает
  `ResolvedSettings` в поле** (нужен для `ResourceRegistry.For(settings)` в шаге 1.2) и пока
  возвращает пустой список.

Зарегистрировать:

- `SPLA.slnx`: папка `/src/plugins/vision/`, в ней проект и (позже) `AGENTS.md` плагина. В
  `/docs/adr/` и `/docs/plans/` добавить ADR и этот план.
- `PublishAll.ps1`: строка `@{ Name = 'geometry'; Proj = 'src/plugins/SPLA.Plugins.Geometry/SPLA.Plugins.Geometry.csproj' }`
  рядом с остальными.
- `tests/SPLA.Tests/SPLA.Tests.csproj`: `ProjectReference` на плагин.

**Стоп-условие:** SkiaSharp тянет нативный `libSkiaSharp`. Если после сборки он не оказался в
`plugins/geometry/` рядом с управляемой сборкой, или загрузка падает в рантайме — **остановись**:
у проекта уже решалась эта задача для SQL-плагина через `AssemblyDependencyResolver` в ALC
(`LoadUnmanagedDll`), но лезть в загрузчик плагинов самостоятельно нельзя, это ядро.

**Приёмка:** решение собирается; после сборки в
`src/apps/SPLA.CLI/bin/Debug/<tfm>/plugins/geometry/` лежат `SPLA.Plugins.Geometry.dll`, `meta.yaml`,
`SkiaSharp.dll` и нативная библиотека; `spla` видит плагин `geometry` без инструментов.

### 0.2. Настройки

`GeometrySettings.cs` — класс со свойствами и `FromBlob` по образцу `BrowserSettings.FromBlob`
(YAML snake_case):

| ключ | тип | по умолчанию | смысл |
|---|---|---|---|
| `render_max_side` | int | `1024` | длинная сторона рендера; она же — пространство координат модели |
| `line_width` | int | `3` | толщина линий в пикселях рендера |
| `font_size` | int | `16` | подписи имён объектов |
| `crop_padding` | double | `0.05` | поле вокруг рамки при `geom_view {to: box}` по умолчанию |
| `jpeg_quality` | int | `0` | 0 = рендер в PNG; 1–100 = в JPEG с этим качеством |

Ограничения: `render_max_side` 256–4096, `line_width` 1–16, `font_size` 8–48, `crop_padding` 0–1,
`jpeg_quality` 0 или 30–100.

**Приёмка:** `GeometrySettingsTests`: пустой blob → значения по умолчанию; `render_max_side: 99999` → 4096.

---

## Волна 1 — модель данных и загрузка изображения

### 1.1. Геометрия и виды

`Model/Obb.cs`:

```csharp
internal sealed record Obb(double Cx, double Cy, double Width, double Height, double AngleDeg)
{
    public (double X, double Y)[] Corners();   // 4 угла, по часовой от левого-верхнего до поворота
    public Obb Transformed(in Affine t);
}
```

`Model/Affine.cs` — 2×3 матрица:

```csharp
internal readonly record struct Affine(double A, double B, double C, double D, double E, double F)
{
    public static Affine Identity { get; }
    public static Affine Translate(double dx, double dy);
    public static Affine Scale(double s);
    public static Affine RotateDeg(double deg);
    public Affine Then(in Affine next);        // композиция: this, потом next
    public Affine Invert();
    public (double X, double Y) Apply(double x, double y);
    public double ScaleFactor { get; }         // средний масштаб, для пересчёта width/height
}
```

**Почему affine, а не offset:** ADR §3.3. `deskew` (шаг 2.3) поворачивает вид — на паре
`offset_x/offset_y` это не выражается.

`Model/GeometryObject.cs`:

```csharp
internal enum ObjectKind { Box, Point }
internal enum ObjectStatus { Editing, Accepted }

internal sealed class GeometryObject
{
    public string Name { get; init; } = "";
    public ObjectKind Kind { get; init; }
    public Obb Box { get; set; }            // Kind == Box; в координатах ИСТОЧНИКА
    public (double X, double Y) Point { get; set; }  // Kind == Point; в координатах ИСТОЧНИКА
    public ObjectStatus Status { get; set; }
}
```

`Model/GeometryView.cs`:

```csharp
internal sealed class GeometryView
{
    public string Id { get; init; } = "";         // "source", "crop_1", …
    public string? ParentId { get; init; }
    public string? FromBox { get; init; }         // имя рамки, если вид вырезан по ней
    public Affine SourceToView { get; init; }     // источник → пиксели ЭТОГО вида
    public int Width { get; init; }               // размер вида в его собственных пикселях
    public int Height { get; init; }
}
```

Вид `source` — не единичная матрица: он тоже масштабирован до `render_max_side`. Это главное место,
где ошибаются; тест обязателен.

**Приёмка:** `AffineTests` — `Then`/`Invert` на известных примерах, `Apply` после
`Translate→RotateDeg→Scale`; круговой тест: точка → в вид → обратно → та же точка ±1e-9.
`ObbTests` — `Corners()` для `angle=0` и `angle=90` совпадает с ручным расчётом;
`Transformed` под поворотом даёт ожидаемый угол.

### 1.2. Загрузка изображения: три формы адреса

`Image/ImageSource.cs` — `static Task<(byte[] Bytes, string? Error)> LoadAsync(string address, ResolvedSettings settings, CancellationToken ct)`.

Порядок разбора **строго такой**:

1. `DataChannel.IsHandle(address)` → `DataChannel.ResolveBytes` (хендл `blob:<id|name>`).
2. Содержит `://` → `ResourceRegistry.For(settings).TryResolve(address, …)`, проверить
   `ResourceRegistry.Supports(provider, ResourceVerb.Read)`, затем `provider.ReadAsync(uri, ct)`.
   Так работают и `file:///…`, и серверные схемы вида `namespace://…`.
3. Иначе — путь в рабочей области: `HostServices.Sandbox.Workspace.ReadAllBytesAsync(address, ct)`.
   **Не `System.IO`**: путь должен пройти через `IWorkspace`.

Ошибку возвращать текстом, не исключением. `DataOrigin` от источника (если провайдер его даёт) —
передать дальше, чтобы шаг 3.1 проставил `session.Doubt.Observe`.

**Приёмка:** `ImageSourceTests` — три ветки на подменённых зависимостях; несуществующий адрес даёт
внятный текст ошибки, а не исключение.

### 1.3. Сессия и реестр

`Session/GeometrySession.cs`:

```csharp
internal sealed class GeometrySession
{
    public SKBitmap Source { get; }                  // декодированный исходник, полный размер
    public string SourceAddress { get; }
    public List<GeometryObject> Objects { get; } = [];
    public List<GeometryView> Views { get; } = [];
    public string CurrentViewId { get; set; } = "source";
    public DataOrigin? Origin { get; }
}
```

`Session/GeometrySessionRegistry.cs` — копия формы `BrowserSessionRegistry`:
`ConcurrentDictionary<IAgentSession, GeometrySession>`, `GetOrCreate` / `TryGet` / `Remove`.
Ключ — ссылка на `IAgentSession`. Известная течь (никто не зовёт `Remove` при закрытии чата) —
такая же, как у браузера; не чинить здесь.

Одна сессия на чат. `geom_open` на занятом чате — **молча заменяет** сессию: по ADR §3.5 рабочая
единица одна, кадр за кадром.

**Приёмка:** `GeometrySessionRegistryTests` — две разные `IAgentSession` не видят сессии друг друга.

---

## Волна 2 — рендер и виды

### 2.1. Рендер

`Render/GeometryRenderer.cs`:

```csharp
internal static byte[] Render(GeometrySession s, GeometryView view, bool grid, GeometrySettings cfg);
```

Что рисует:

- пиксели источника, преобразованные в вид (`SKCanvas.SetMatrix` из `view.SourceToView`, затем
  `DrawBitmap`) — сглаживание включить;
- **каждый объект, попадающий в вид**, в координатах вида:
  - `Box`, `Editing` — сплошной контур по четырём углам, толщина `line_width`;
  - `Box`, `Accepted` — тот же контур, но тоньше и приглушённым цветом;
  - `Point` — перекрестие (не точка: точку в 1 пиксель модель не увидит) размером ~12 px плюс
    окружность;
  - подпись `name` рядом с объектом, `font_size`, с тёмной подложкой под текстом — иначе на светлом
    кадре не читается;
- `grid: true` — сетка каждые 100 px координат вида с подписями осей. **Не по умолчанию** (ADR §3.6).

Цвета: `Editing` — насыщенный (например `#FF3B30`), `Accepted` — приглушённый (`#34C759`), разные
объекты одного статуса **не различаются цветом** — различаются подписью. Цвет несёт статус, а не
идентичность.

Кодирование: `jpeg_quality == 0` → PNG, иначе JPEG. Возвращает байты.

**Приёмка:** `GeometryRendererTests` — отрендерить синтетический кадр 400×300 с одной рамкой,
декодировать результат обратно через SkiaSharp и проверить, что (а) размер рендера равен размеру
вида, (б) пиксель в центре стороны рамки отличается от исходного (что-то нарисовано), (в) пиксель
далеко от рамки не изменился.

### 2.2. Текст ответа — единый для всех инструментов

`Tools/GeometryToolBase.cs`, метод `RenderResult(session, view, string action, GeometrySettings cfg)`:
собирает `ToolResult.From(new ToolText(text), new ToolImage(base64, mime))` и **кладёт те же байты в
`session.Blobs`** с `DataOrigin` источника (как `BrowserScreenshotTool`).

Формат текста (по-английски, дословно):

```
<action>
view: <id> — <W>x<H> px<, from box '<name>', deskewed>
ALL coordinates you pass are in this <W>x<H> space.
objects here:
  bag    box    cx=470 cy=325 w=360 h=180 angle=-6   editing
  mark   point  x=418 y=203                          accepted
```

Абсолютные значения печатаются **всегда** — модель не обязана помнить, что слала. Объекты, не
попавшие в текущий вид, перечисляются отдельной строкой `outside this view: <names>`.

### 2.3. `geom_view`

Цели (`to`):

- имя объекта → вырезать по его рамке (для `Point` — квадрат `2 × crop_padding × min(source)` вокруг точки);
- `"source"` → корневой вид;
- `"parent"` → родитель текущего;
- `"current"` → перерисовать текущий (так работает «просто посмотреть ещё раз»);
- `id` вида (`crop_1`) → вернуться в него.

Плюс `rect: [x, y, w, h]` — прямоугольник **в координатах текущего вида**; нужен, чтобы заглянуть в
угол кадра до того, как там что-то поставлено.

`deskew` (по умолчанию `false`): при вырезке по рамке с углом — довернуть вид так, чтобы рамка стала
горизонтальной. Это единственное место, где матрица реально вращает.

Новый вид кладётся в `Views`, `CurrentViewId` переключается. Виды не удаляются.

**Приёмка:** `GeometryViewTests` — поставить рамку в `source`, войти в неё `geom_view`, поставить
объект в crop, вернуться в `source`: координаты объекта в источнике совпадают с ожидаемыми ±1 px.
Отдельный тест с `deskew: true` на рамке с `angle=30`.

---

## Волна 3 — инструменты

Шесть штук. Все — `StrictSchema = true`: каждое свойство либо в `required`, либо типа
`["...", "null"]` **и тоже в `required`** (`agents/tool-args.md`). Аргументы разбирать только через
`ToolJson`. Все, кроме `geom_result`, возвращают текст + картинку.

| имя | Scope / Effect / Risk |
|---|---|
| `geom_open` | Project / Read / Low |
| `geom_box` | Project / Write / Low |
| `geom_point` | Project / Write / Low |
| `geom_view` | Project / Read / Low |
| `geom_accept` | Project / Write / Low |
| `geom_result` | Project / Read / Low |

### 3.1. `geom_open`

`{ image: string, grid?: bool }` — адрес по правилам 1.2. Создаёт сессию, вид `source`, зовёт
`session.Doubt.Observe(origin, address)`, возвращает рендер.

### 3.2. `geom_box`

`{ name, cx?, cy?, width?, height?, angle?, dx?, dy?, dw?, dh?, dangle?, delete? }`

- Имени нет в сессии → создать (`created '<name>'`); есть → обновить (`updated '<name>'`).
- Абсолютные поля (`cx…angle`) и дельты (`dx…dangle`) **в одном вызове смешивать нельзя** — ошибка
  с пояснением. При создании дельты недопустимы.
- Координаты приходят **в текущем виде** — перевести в источник через `view.SourceToView.Invert()`.
  Дельты применяются тоже в координатах вида.
- `delete: true` — удалить объект. Редкая операция, в `Details` описать одной строкой.
- Число рамок не ограничено: имена — ключ (ADR: N объектов).

### 3.3. `geom_point`

`{ name, x?, y?, dx?, dy?, delete? }` — то же самое для точки. Отдельный инструмент, а не флаг на
`geom_box`: у точки два параметра вместо пяти, и смешивать их в одной схеме — верный способ получить
от слабой модели рамку без размеров.

### 3.4. `geom_view`

`{ to?, rect?, padding?, deskew?, grid? }` — см. 2.3. Ни `to`, ни `rect` не заданы → ошибка.

### 3.5. `geom_accept`

`{ name?: string }` — имя не задано → принять все `Editing`. Возвращает рендер, где принятое
нарисовано приглушённо.

### 3.6. `geom_result`

`{ output?, output_name? }` — `SchemaParts.Output` / `SchemaParts.OutputName` дословно, разбор через
`DataChannel.ParseTarget`, отдача через `DataChannel.Route`. **Единственный инструмент без картинки** —
это данные для вызывающего, не для разглядывания.

JSON: `session`, `source { address, width, height }`, `objects[]` (каждый — `name`, `kind`, `status`
и `coordinates` с ключом на каждый вид, где объект был, **плюс обязательно `source`**; для `Box` в
`source` — ещё и `corners`, чтобы снять неоднозначность соглашения об угле), `views[]` с матрицами.

**Приёмка волны:** `GeometryToolsTests` по образцу `BrowserPluginTests`: имена уникальны,
`Parameters` сериализуется в JSON-объект, каждый инструмент без сессии возвращает внятный отказ, а не
исключение. Плюс сквозной тест: `open → box → box(дельта) → accept → view(to:box) → point → accept →
result`, в конце проверить координаты в `source`.

---

## Волна 4 — ядро: переключатель картинок в контексте

Единственная правка вне плагина. Делать **после** волны 3 и отдельным коммитом:
`feat(core): add agent.tool_images retention switch`.

**Задача.** Сейчас каждый `ToolImage` становится обычным user-сообщением с
`RetentionPolicy = Persistent` и пересылается провайдеру в каждом последующем запросе до конца чата
(`ConversationOrchestrator.cs` ~313-341, `OpenAiCompatibleClient.BuildMultimodalContent`). Для цикла
геометрии это 6–10 картинок на кадр, из которых актуальна ровно последняя. При этом локальные стеки
(llama.cpp, на нём LM Studio) старые картинки из истории, как правило, всё равно не обсчитывают —
токены платятся, пользы нет.

**Решение.** Настройка `agent: tool_images: all | last`, по умолчанию `all` (текущее поведение).
При `last` оркестратор ставит вставляемому сообщению с картинкой
`RetentionPolicy = ContextRetention.UntilSuperseded` и общий `ReplacementKey` (одна константа на все
инструменты — «в контексте живёт последняя картинка, чья бы ни была»).

Механизм в ядре уже есть и покрыт тестом (`ContextAssembler.Assemble`, строки 60-64;
`ContextAssemblerTests.UntilSuperseded_keeps_only_the_latest_per_key`) — **не существует только тот,
кто его выставляет**. Ничего нового в `ContextAssembler` писать не нужно.

Свойство, которое надо сохранить: это **не удаление**. Сообщение остаётся в истории чата, в UI и в
sidecar-файле (`ChatImages`); отсекается только сборка контекста. Смена настройки обратима без потери
данных.

**Приёмка:** тест на оркестраторе — при `last` два подряд `ToolImage` дают два сообщения в
`Conversation`, но `ContextAssembler.Assemble` возвращает одно; при `all` — оба. Живая проверка на
LM Studio: цикл из пяти `geom_box` подряд, посмотреть `PromptTokens` последнего запроса при обоих
значениях настройки.

**Отдельно, не в этом плане:** `ContextRetention.UntilResolved` и `NextStepOnly` объявлены в
перечислении, сериализуются `ChatRuntime`, но `Assemble` их не обрабатывает — ведут себя как
`Persistent`. Это либо доделать, либо убрать; вопрос к владельцу, трогать здесь нельзя.

---

## Волна 5 — проверка на синтетике

Без неё ничего не доказано: цикл может собираться и при этом не работать.

`tests/SPLA.Tests/Geometry/SyntheticFixtures.cs` — генератор кадров **в коде**, без бинарных файлов
в репозитории: на сером фоне рисуется прямоугольник известного цвета с известными
`cx, cy, w, h, angle`, внутри него — второй прямоугольник поменьше (аналог маркировки на мешке) и
крестик в известной точке.

Два теста:

1. **Геометрический (детерминированный, в CI).** Прогнать `open → box(точно по эталону) → view(to:box,
   deskew) → point(точно) → result` и сверить координаты в `source` с теми, из которых фикстура
   рисовалась, с допуском 1 px. Это ловит ошибку в матрицах — самую вероятную и самую незаметную.
2. **Живой [ЧЕЛОВЕК].** Тот же кадр отдать реальной модели через LM Studio и посмотреть, сходится ли
   цикл: попадает ли она грубо с первого `geom_box`, уменьшается ли ошибка после дельты, за сколько
   шагов принимает. Здесь же замерить, **не ужимает ли провайдер картинку** сверх нашего
   `render_max_side`: отрендерить кадр с сеткой (`grid: true`) и спросить модель, какие подписи она
   на нём видит — если названные ею числа не совпадают с нарисованными, пространство координат едет,
   и `render_max_side` надо подбирать.

Второй тест исполнитель **не имитирует и не объявляет пройденным** — готовит и пишет в отчёте, что
проверить руками.

---

## Промпт плагина (`default_prompt` в meta.yaml)

```
Geometry tools let you mark up an image by iteration instead of guessing coordinates in one shot.

The loop: geom_box (rough guess) -> look at the returned picture -> geom_box again with deltas
(dx/dy/dw/dh/dangle) to correct it -> geom_accept when it sits right. Two or three corrections are
normal and expected; a first guess that is visibly off is not a failure.

Every reply states the pixel size of the picture you just received. ALL coordinates you pass are in
that space — never in the original image's pixels. The tool converts; you never do arithmetic on
coordinates.

geom_view zooms into an object (or a rectangle) so small things become big enough to place
accurately: mark the large object first, view into it, then mark what is inside it. geom_result
returns everything in original-image coordinates when you are done.
```

## Общая приёмка плана

- `dotnet test tests/SPLA.Tests --filter FullyQualifiedName~Geometry` зелёный, включая сквозной тест
  волны 3 и геометрический тест волны 5;
- живая проверка волны 5 пройдена человеком на LM Studio;
- после закрытия плана описание работы плагина живёт в
  `src/plugins/SPLA.Plugins.Geometry/AGENTS.md` и совпадает с кодом; статус этого плана — «закрыт».
