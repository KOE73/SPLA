# Архитектурный формат проектов SPLA Diagram

Единый стандарт хранения моделей архитектуры, сущностей кода, связей и визуальных видов в SPLA.

---

## 1. Структура проекта на диске

Каждый проект хранится в отдельной директории внутри `docs/diagrams/projects/<project_id>/`:

```
docs/diagrams/projects/<project_id>/
  project.json          Манифест проекта
  entities.json         Каталог сущностей кода (классы, интерфейсы, рекорды)
  relations.json        Каталог зависимостей и связей (наследование, DI, вызовы)
  text.ru.json          Локализованные описания, названия и аннотации
  views/
    <view_id>.view.json Геометрия, зоны, размещение узлов и визуальные стрелки
```

Общая библиотека стилей для всех проектов хранится в [`docs/diagrams/styles.json`](../styles.json).

---

## 2. Разделение ответственности файлов

| Файл | Назначение | Кто генерирует / обновляет |
|---|---|---|
| `project.json` | Идентификатор, заголовок, список видов и языков | Автор / Редактор |
| `entities.json` | Реестр всех известных компонентов/типов | Экстракторы кода / Редактор |
| `relations.json` | Реестр всех потенциальных и реальных связей | Экстракторы кода / Редактор |
| `text.*.json` | Документация, описания, назначения | Разработчики / LLM / Редактор |
| `views/*.view.json` | Физическая раскладка на холсте (координаты, зоны) | Редактор диаграмм |

---

## 3. Спецификация JSON-схем

### `project.json`
```json
{
  "id": "full_core",
  "title": "FULL Core (All Classes)",
  "subtitle": "240+ classes, interfaces, middlewares",
  "defaultView": "v_semantic_atlas",
  "languages": ["ru"],
  "views": ["v_semantic_atlas"]
}
```

### `entities.json`
```json
{
  "entities": [
    {
      "id": "n_repetitionguardmiddleware",
      "name": "RepetitionGuardMiddleware",
      "kind": "Class",
      "origin": "code",
      "status": "present",
      "namespace": "SPLA.Agent.Guards",
      "codeRef": "src/agent/SPLA.Agent/Guards/RepetitionGuardMiddleware.cs",
      "members": []
    }
  ]
}
```

### `relations.json`
```json
{
  "relations": [
    {
      "id": "r_repetitionguardmiddleware_itoolmiddleware_implements",
      "from": "n_repetitionguardmiddleware",
      "to": "n_itoolmiddleware",
      "type": "implements",
      "relation": "implements",
      "label": "",
      "origin": "code",
      "status": "present",
      "evidence": [
        {
          "codeRef": "src/agent/SPLA.Agent/Guards/RepetitionGuardMiddleware.cs"
        }
      ]
    }
  ]
}
```

#### Допустимые типы связей (`type` / `relation`):
- `call` — вызов метода, использование сервиса, зависимость в параметре.
- `implements` — реализация интерфейса.
- `extends` — наследование базового класса.
- `composes` — внедрение зависимости (DI), приватное поле, компоновка.
- `event` — подписка или публикация события.
- `storage` — доступ к базе данных или файловому хранилищу.
- `relates` — общая смысловая связь.

### `text.ru.json`
```json
{
  "entries": {
    "n_repetitionguardmiddleware": {
      "name": "RepetitionGuardMiddleware",
      "doc": "Защитный middleware для предотвращения зацикливания и повторяющихся шагов агента."
    },
    "z_llm_middleware": {
      "name": "LLM Middleware Pipeline",
      "doc": "Конвейер обработки вызовов языковой модели."
    }
  }
}
```

### `views/<view_id>.view.json`
```json
{
  "id": "v_semantic_atlas",
  "project": "full_core",
  "zones": [
    {
      "id": "z_llm_middleware",
      "container": null,
      "parent": "z_llm",
      "x": -1120,
      "y": 1480,
      "width": 900,
      "height": 1040,
      "styleId": "zone.green"
    }
  ],
  "nodes": [
    {
      "id": "n_repetitionguardmiddleware",
      "container": "z_llm_middleware",
      "x": -950,
      "y": 1600,
      "width": 210,
      "height": 60,
      "styleId": "node.class"
    }
  ],
  "edges": [
    {
      "id": "f_guard_attempt",
      "from": "n_repetitionguardmiddleware",
      "to": "n_turnattempt",
      "type": "call",
      "label": "Abort() → cancel",
      "styleId": "relation.strong"
    }
  ]
}
```

---

## 4. Архитектурные правила в коде

1. **Единый источник правды о связях**: все связи хранятся в `relations.json`. Визуальный вид `views/*.view.json` содержит только список фактически включённых на холсте связей (`edges: [...]`).
2. **Теневой режим**: связи из `relations.json`, которых нет в `edges`, отображаются пунктиром только при фокусе/клике на блок.
3. **Бесшовная синхронизация**: добавление/скрытие связей на холсте обновляет `edges` вида, не повреждая глобальный реестр связей.
