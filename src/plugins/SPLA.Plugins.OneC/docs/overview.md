# Обзор модуля SPLA.Tools.OneC

## Зачем этот модуль?

Большая конфигурация 1С может содержать тысячи объектов и сотни BSL-файлов.
Загрузить всё это в контекст LLM — невозможно. Поиск по тексту файлов даёт много
шума и не различает семантику связей.

`SPLA.Tools.OneC` решает эту проблему через три уровня:

```
Выгрузка конфигурации (файлы)
         ↓ детерминированный индексатор
SQLite-граф объектов и связей
         ↓
    ┌────┴─────┐
    │          │
 Агент      UI / человек
(tools)    (граф объекта)
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
| `GraphDataBuilder` | JSON для Cytoscape.js (нет UI-зависимостей) |
| `Assets/onec_graph.html` | Встроенный WebView-вьюер (Cytoscape.js) |

## Поток данных

```
1С-разработчик выгружает конфигурацию в каталог
    → агент вызывает onec.index_configuration
    → OneCIndexer: 1-й проход (XML → Objects)
    → OneCIndexer: 2-й проход (BSL → Relations)
    → индекс записан в .spla/index/onec.sqlite
    → агент задаёт вопросы через onec.* tools
    → UI читает граф через GraphDataBuilder → WebView
```

## Расположение файлов

```
SPLA.Tools.OneC/
  Assets/
    onec_graph.html          — встроенный вьюер графа
  docs/
    README.md                — этот файл
    overview.md              — архитектурный обзор
    dump-structure.md        — ожидаемая структура выгрузки 1С
    index-schema.md          — схема SQLite
    relation-types.md        — семантика типов связей
    tools-reference.md       — справочник по tools
    graph-view.md            — использование WebView
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
  GraphDataBuilder.cs
  SPLA.Tools.OneC.csproj
```
