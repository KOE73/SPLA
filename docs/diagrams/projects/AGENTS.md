# Проекты архитектурной модели — контракт v2

Здесь лежат **проекты**: наборы сущностей, связей, контейнеров, текстов и видов.
Пока пусто — папка подготовлена под переезд.

**Решение и обоснование:** [`ADR_20260828_diagrams_model-contract-v2`](../../adr/ADR_20260828_diagrams_model-contract-v2.md).
**Инструкция утилите:** [`tools/spla-atlas/AGENTS.md`](../../../tools/spla-atlas/AGENTS.md).

---

## 1. Раскладка

```
docs/diagrams/projects/<project-id>/
  project.json          манифест
  entities.json         сущности                [пишет утилита]
  relations.json        связи                   [утилита: origin=code; редактор: origin=manual]
  containers.json       контейнеры              [человек через редактор]
  text.en.json          тексты, базовый язык    [редактор / агент]
  text.ru.json          тексты, перевод         [редактор / агент]
  views/
    <view-id>.view.json вид                     [пишет ТОЛЬКО редактор]
```

Стили общие на проект и лежат выше: [`docs/diagrams/styles.json`](../styles.json).

## 2. Кто во что пишет

| Файл | Утилита | Редактор | Человек в текстовом виде |
|---|---|---|---|
| `project.json` | читает | читает | да |
| `entities.json` | **пишет** | читает | переименования |
| `relations.json` | пишет `origin: code` | пишет `origin: manual` | да |
| `containers.json` | читает | пишет | да |
| `text.*.json` | не касается | пишет | да |
| `views/*.view.json` | **никогда** | **пишет** | по своему усмотрению |

Одна граница: утилита не пишет в `views/` ни при каких условиях. Ручная раскладка
живёт только там и машиной не затирается по построению.

---

## 3. Формы файлов

### `project.json`

```json
{
  "contractVersion": 2,
  "id": "core",
  "title": "SPLA Core",
  "baseLanguage": "en",
  "languages": ["en", "ru"],
  "sources": {
    "include": ["src/core", "src/agent/SPLA.Agent/Guards", "src/agent/SPLA.Agent/Accounting"]
  },
  "styles": "../../styles.json"
}
```

Пути — от корня репозитория. Решение (`SPLA.slnx`) здесь **не указывается**
намеренно: утилита перечисляет `*.csproj` под этими путями и открывает их
поштучно, не полагаясь на поддержку формата `.slnx` в `MSBuildWorkspace`.

### `entities.json`

```json
{
  "contractVersion": 2,
  "entities": [
    {
      "id": "e_repetitionguardmiddleware",
      "name": "RepetitionGuardMiddleware",
      "kind": "class",
      "origin": "code",
      "status": "present",
      "namespace": "SPLA.Agent.Guards",
      "codeRef": "src/agent/SPLA.Agent/Guards/RepetitionGuardMiddleware.cs",
      "firstSeen": "2026-08-28",
      "members": [
        { "name": "InvokeAsync", "memberKind": "method",
          "signature": "Task<LlmTurnResult> InvokeAsync(LlmTurnContext, LlmMiddlewareDelegate, CancellationToken)" },
        { "name": "Stage", "memberKind": "property",
          "type": "LlmPipelineStage", "typeRef": "e_llmpipelinestage" }
      ]
    },
    {
      "id": "e_llmturnstatus",
      "name": "LlmTurnStatus",
      "kind": "enum",
      "origin": "code",
      "status": "present",
      "members": [
        { "name": "Completed", "memberKind": "enumValue", "value": 0 },
        { "name": "Degenerate", "memberKind": "enumValue", "value": 3 }
      ]
    },
    {
      "id": "e_lmstudio_api",
      "name": "LM Studio HTTP API",
      "kind": "external",
      "origin": "manual",
      "status": "present"
    }
  ]
}
```

- `id` выдаётся **один раз и не меняется никогда**, включая переименование типа.
- `name` — каноническое имя типа. **Не переводится**, в `text.*.json` его нет.
- `kind` — открытая строка. Из кода приходят `class`, `interface`, `record`,
  `struct`, `enum`; рукописные сущности берут что угодно (`external`, `database`,
  `service`). Проверки «такого вида не бывает» нет.
- `status`: `present` | `gone`. Записи не удаляются: `gone` сохраняет id для
  всего, что на него ссылается.
- `origin: manual` — для того, чего нет в C#: внешний API, СУБД, чужой сервис.
  Утилита такие записи не трогает.

**Переименование класса** делается правкой `name` при неизменном `id`. Ни один
другой файл при этом не меняется.

### `relations.json`

```json
{
  "contractVersion": 2,
  "relations": [
    { "id": "r_repetitionguardmiddleware_illmmiddleware_implements",
      "from": "e_repetitionguardmiddleware", "to": "e_illmmiddleware",
      "type": "implements", "origin": "code", "status": "present" },

    { "id": "r_gateway_repguard",
      "from": "e_illmgateway", "to": "e_repetitionguardmiddleware",
      "type": "data-flow", "origin": "manual", "status": "present",
      "evidence": [
        { "codeRef": "src/core/SPLA.Domain/Llm/LlmGateway.cs", "symbol": "SendAsync" }
      ] }
  ]
}
```

- `type` — **открытая строка**. Структурные (`extends`, `implements`, `composes`)
  ставит утилита; потоки (`call`, `data-flow`, `event`, `security`, `storage` и
  любые новые) заводятся руками. Проверки «такого типа не бывает» нет.
- `id` выводится из **id сущностей**, не из имён:
  `r_<from>_<to>_<type>` без префиксов `e_`. Рукописная связь может иметь свой
  короткий id.
- `evidence` проверяемо: если файл или символ исчезли, сверка сообщает. Так
  логические связи гниют громко, а не молча.
- Подпись связи — **не здесь**, а в `text.*.json`: она переводимая.

### `containers.json`

```json
{
  "contractVersion": 2,
  "containers": [
    { "id": "c_llm", "parent": null, "theme": "green" },
    { "id": "c_llm_middleware", "parent": "c_llm",
      "match": {
        "name": ["ILlmMiddleware", "LlmPipelineBlueprint", "RepetitionGuardMiddleware"],
        "path": ["src/core/SPLA.Domain/Llm/Middleware/"]
      } }
  ],
  "overrides": {
    "e_repetitionguardmiddleware": "c_llm_middleware"
  }
}
```

- Порядок разрешения: `match.name` → `match.nameRegex` → `match.path`
  (длиннейший префикс) → сосед по файлу.
- `overrides` — явное назначение человеком. **Всегда бьёт правило.** Именно так
  правка правила перестаёт молча перетаскивать типы.
- Одна сущность может состоять в нескольких контейнерах. Это не ошибка.
- Принадлежность — **подсказка**: группировка в панели базы, подсказка при
  вытаскивании на вид, отчёт о дрейфе. На геометрию видов она не влияет никак.
- Имя контейнера — в `text.*.json`.

### `text.<lang>.json`

```json
{
  "contractVersion": 2,
  "language": "en",
  "entries": {
    "e_repetitionguardmiddleware": {
      "description": "Stage Output. Observes the streamed generation, cancels a degenerate one and asks for another attempt."
    },
    "c_llm_middleware": {
      "name": "Middleware chain",
      "description": "Ordered interceptors wrapped around one model call."
    },
    "r_gateway_repguard": { "label": "LlmTurnContext ↓" },
    "v_semantic_atlas":   { "name": "Semantic atlas" }
  }
}
```

- База — **английская** (`project.json` → `baseLanguage`). Отсутствующий ключ —
  фолбэк на базу, это норма, а не ошибка. Частичный перевод допустим.
- Переводятся: имена контейнеров и видов, все описания, подписи связей.
- **Не переводится** имя сущности — это имя типа.
- **Счётчиков в именах нет.** Нынешнее `"Цепочка middleware (11)"` разделено:
  имя в текст, число считает рендер по содержимому вида. Иначе каждая пересборка
  пачкала бы все переводы разом.

### `views/<view-id>.view.json`

```json
{
  "contractVersion": 2,
  "id": "v_semantic_atlas",
  "project": "core",

  "relations": { "default": "visible", "except": ["r_gateway_repguard"] },

  "zones": [
    { "id": "z_llm", "container": "c_llm", "parent": null,
      "x": -1950, "y": 1380, "width": 2852, "height": 1290, "styleId": "domain.llm" },
    { "id": "z_llm_middleware", "container": "c_llm_middleware", "parent": "z_llm",
      "x": -1120, "y": 1480, "width": 900, "height": 1040, "styleId": "zone.green" }
  ],

  "placements": [
    { "entity": "e_repetitionguardmiddleware", "zone": "z_llm_middleware",
      "x": -750, "y": 1650, "width": 220, "height": 90, "render": "compact" },
    { "entity": "e_lmstudio_api", "zone": null,
      "x": 4200, "y": 100, "width": 200, "height": 60, "render": "compact" }
  ]
}
```

Правила вида:

1. **Вид — выборка.** Сущность попадает сюда только вытаскиванием из реестра.
   Ничего не появляется само; неразмещённое живёт в реестре и попадает в отчёт.
   Зоны `НЕ РАЗМЕЩЕНО` не существует.
2. **Размещение свободно.** `zone: null` — элемент стоит в пустом поле, вне
   любого контейнера. Это законно, даже если по реестру он куда-то принадлежит.
3. **Принадлежность записана явно**, из геометрии не выводится. Прямоугольники —
   чистая визуализация; элемент внутри рамки, но с `zone: null`, легален.
4. **Вложенность зон** — поле `parent`, тоже явно. Может отличаться от иерархии
   в `containers.json`, и это нормально: вид волен уплощать и перегруппировывать.
5. **Зона может не ссылаться на контейнер** (`container: null`) и иметь
   собственное имя в текстах — чтобы декоративная рамка не засоряла реестр.
6. `render` — именованный профиль рендера (`compact`, `detail`, далее по мере
   надобности). Свойство размещения, не сущности.

#### Видимость связей

Три механизма, по убыванию дешевизны:

1. **Правило обоих концов** — ничего не хранит. Связь видна, только если на виде
   есть **обе** её сущности. Следствие, не настройка; съедает основную массу.
2. **`relations.default`** — что делать со связью, по которой решения ещё не
   принимали. `visible` для рабочих видов, `hidden` для обзорных.
3. **`relations.except`** — поимённые решения, противоположные умолчанию. Всегда
   меньшая сторона: скрыть 900 из 1000 значит поставить `default: "hidden"` и
   перечислить сотню показанных, а не тысячу скрытых.

Политики по типу связи **нет** и не будет: типы — открытое множество, новый тип
при такой политике оказался бы в неопределённом состоянии.

---

## 4. Что переезжает и куда

Переезд разовым конвертером (`spla-atlas convert`), см.
[инструкцию утилите §5](../../../tools/spla-atlas/AGENTS.md).

| Сейчас | Станет |
|---|---|
| [`docs/diagrams/model-core-full.json`](../model-core-full.json) | `projects/core/` целиком |
| [`mapping/semantic-atlas/core.map.json`](../mapping/semantic-atlas/core.map.json) | `containers.json` |
| [`mapping/semantic-atlas/core.edges.json`](../mapping/semantic-atlas/core.edges.json) | `relations.json`, `origin: manual` |
| [`mapping/semantic-atlas/core.known.json`](../mapping/semantic-atlas/core.known.json) | **выбрасывается** |

**Ручная раскладка обязана перенестись один в один** — координаты, размеры,
`styleId` каждого узла и каждой зоны. Это требование к конвертеру и предмет
теста, а не пожелание.

До конца переезда старые файлы **остаются на месте и работают**: `tools/spla-arch`
продолжает собирать `model-core-full.json`, редактор различает форматы по
`metadata.contractVersion`.

---

## 5. Референсы

Только для чтения, как материал, а не как образец:

- [`docs/diagrams/README_RU.md`](../README_RU.md) — как всё работало в v1;
- [`tools/spla-arch/AGENTS.md`](../../../tools/spla-arch/AGENTS.md) — философия
  трёх слоёв (актуальна); «свой набор правил на каждую точку зрения» — отменено
  ADR;
- [`tools/spla-diagram/docs/CONTRACT_V2.md`](../../../tools/spla-diagram/docs/CONTRACT_V2.md)
  — разбор, предшествовавший ADR; его выводы читать через §7 ADR.

---

## 6. Правило, которое не обсуждается

**Координаты правит только владелец руками в редакторе.** Ни агент, ни утилита,
ни скрипт «для восстановления». Если файл вида выглядит сломанным — сообщить
человеку, а не чинить. Однажды ручную раскладку уже потеряли откатом файла к
`git HEAD`; вся эта конструкция существует ради того, чтобы такое перестало быть
возможным.
