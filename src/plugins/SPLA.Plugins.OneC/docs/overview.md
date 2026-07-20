# Обзор модуля SPLA.Plugins.OneC

## Зачем этот модуль?

Большая конфигурация 1С может содержать тысячи объектов и сотни BSL-файлов.
Загрузить всё это в контекст LLM — невозможно. Поиск по тексту файлов даёт много
шума и не различает семантику связей.

`SPLA.Plugins.OneC` решает эту проблему через детерминированный индекс и узкие инструменты агента:

```
Выгрузка конфигурации (файлы)
         ↓ детерминированный индексатор
SQLite-граф объектов и связей
         ↓
         ↓
Инструменты агента

Типизированный слой данных графа сохранён отдельно от UI. В дальнейшем сервис сможет отдавать эти
данные web-клиенту на Vue, не открывая клиенту прямой доступ к SQLite.
```

## Ключевые компоненты

| Компонент | Назначение |
|-----------|-----------|
| `Indexing/OneCFileScanner` | Рекурсивный обход каталога выгрузки, классификация файлов |
| `Indexing/OneCMetadataParser` | XML → объекты и `owns`-связи |
| `Indexing/OneCCodeAnalyzer` | BSL → `calls`, `reads`, `writes`, `uses` |
| `Indexing/OneCQueryAnalyzer` | Тексты запросов → `queries` |
| `Indexing/OneCIndexer` | Оркестратор двух проходов с инкрементальным хешированием |
| `Storage/OneCIndexDatabase` | DAL поверх SQLite (WAL-режим) |
| `Tools/*Tool` | `IMcpTool`-реализации для агента |
| `Graph/OneCGraph` | Типизированные узлы, связи, параметры и сводка графа |
| `Graph/OneCGraphBuilder` | Построение ограниченного подграфа из SQLite-индекса без UI-зависимостей |

## Поток данных

```
1С-разработчик выгружает конфигурацию в каталог
    → агент вызывает onec_build_index
    → OneCIndexer: 1-й проход (XML → Objects)
    → OneCIndexer: 2-й проход (BSL → Relations)
    → индекс записан в .spla/onec.sqlite
    → агент задаёт вопросы через onec_* tools
    → в будущем сервис сможет передать типизированный подграф web-клиенту
```

## Расположение файлов

```
src/plugins/SPLA.Plugins.OneC/
  docs/
    README.md                — этот файл
    overview.md              — архитектурный обзор
    dump-structure.md        — ожидаемая структура выгрузки 1С
    index-schema.md          — схема SQLite
    relation-types.md        — семантика типов связей
    tools-reference.md       — справочник по tools
    graph-view.md            — типизированный слой данных графа
    integration.md           — подключение к SPLA
    limitations.md           — ограничения v1
  Indexing/
    OneCFileScanner.cs
    OneCMetadataParser.cs
    OneCCodeAnalyzer.cs
    OneCQueryAnalyzer.cs
    OneCIndexer.cs
  Models/
    OneCObject.cs
    OneCRelation.cs
    OneCFileEntry.cs
    IndexingReport.cs
  Storage/
    OneCIndexSchema.cs
    OneCIndexDatabase.cs
  Graph/
    OneCGraph.cs
    OneCGraphBuilder.cs
  Tools/
    FindObjectTool.cs
    GetObjectTool.cs
    FindReferencesTool.cs
    GetDependenciesTool.cs
    GetReverseDependenciesTool.cs
    FindWritersTool.cs
    FindReadersTool.cs
    ExplainObjectTool.cs
    IndexConfigurationTool.cs
    YamlResponse.cs
  SPLA.Plugins.OneC.csproj
```
