# PLAN_20260824_plugins — плагин `documents`: DOCX → семантика → Excel/CSV

Статус: **в работе**

Основание: [`ADR_20260824_plugins_document-context`](../adr/ADR_20260824_plugins_document-context.md),
идея — [`IDEA_20260824_plugins_document-context`](../ideas/IDEA_20260824_plugins_document-context.md).
Ветка: `work` (работа начата в общем чекауте).

Читать перед правкой: [`agents/plugins.md`](../../agents/plugins.md) (имена инструментов, `Details`),
[`src/plugins/AGENTS.md`](../../src/plugins/AGENTS.md) (плагин — низкодоверенный, ALC-изоляция),
[`agents/tool-args.md`](../../agents/tool-args.md).

---

## Что делается и чего НЕ делается

Делается класс **Context/Data**: прочитать документ, отдать смысл, дописать строки в таблицу
по заголовкам колонок.

**НЕ делается** класс `Artifact/Layout`: ни стилей, ни объединений, ни формул, ни печатных
областей, ни правки самого `.docx`. Отдельная работа с отдельным API (см. ADR, §4).

**НЕ трогается ядро.** Ни одного файла в `src/core`, `src/agent`, `src/service`. Если правка ядра
кажется нужной — это сигнал, что решение поехало мимо ADR; сначала перечитать §2 ADR.

---

## Конструкция

Три новых проекта:

```
src/documents/SPLA.Documents.Model/     — семантическая модель + рендереры (обычная библиотека)
src/plugins/SPLA.Plugins.Documents/     — плагин id=documents: экстракторы, конвертеры, инструменты
src/plugins/SPLA.Skills.Documents/      — скиллы плагина (.md), кладутся в plugins/documents/skills/
```

Поток:

```
файл (.docx)
   ↓  DocxExtractor (Open XML SDK)
ContextDocument { Metadata, Blocks[] }
   ↓  MarkdownContextRenderer / JsonContextRenderer / PlainTextContextRenderer
байты + MIME
   ↓  document_extract (в контекст или в blob)   |   IFormatConverter → resource_read as=...
модель
   ↓  spreadsheet_append_rows  (заголовки колонок, не адреса ячеек)
.xlsx (ClosedXML) / .csv (нативно)
```

### Модель (`SPLA.Documents.Model`)

| Тип | Смысл |
|---|---|
| `ContextDocument(DocumentMetadata Metadata, IReadOnlyList<ContextBlock> Blocks)` | корень |
| `DocumentMetadata` | `SourceName`, `SourceType`, `Title`, `Author`, `Created`, `Modified`, `Extra` |
| `HeadingBlock(int Level, string Text)` | уровень 1..9 |
| `ParagraphBlock(string Text)` | |
| `ListBlock(bool Ordered, IReadOnlyList<ListItemLine> Items)`, `ListItemLine(int Level, string Text)` | уровень вложенности с нуля |
| `TableBlock(IReadOnlyList<string>? Header, IReadOnlyList<IReadOnlyList<string>> Rows, string? Caption)` | |
| `CodeBlock(string? Language, string Text)` | |
| `ImageBlock(string? Name, string? AltText, string? ContentType, long? ByteCount)` | **ссылка, а не payload** — картинка в дерево не кладётся |
| `SectionBreak(string? Label)` | граница страницы/листа/секции |
| `IContextRenderer { string TargetType; string Render(ContextDocument) }` | реализации: `MarkdownContextRenderer` (`text/markdown`), `PlainTextContextRenderer` (`text/plain`), `JsonContextRenderer` (`application/json`) |
| `IDocumentExtractor { IReadOnlyCollection<string> SourceTypes; Task<ContextDocument> ExtractAsync(Stream, string sourceName, CancellationToken) }` | контракт бэкенда |

Опций у экстрактора в V1 **нет** намеренно: у `IFormatConverter` мешок опций уже есть и всегда
`null`; вторая пустая точка расширения ничего не добавляет. Появится реальная нужда (страница PDF,
интервал кадров) — там и заводить.

### Плагин (`SPLA.Plugins.Documents`, id `documents`)

| Файл | Содержимое |
|---|---|
| `meta.yaml` | `id: documents`, `type: dll`, `entry_point: SPLA.Plugins.Documents.dll`, `default_prompt` (English!) |
| `DocumentsPlugin.cs` | `ISplaPlugin`: регистрирует конвертеры в `FormatConverterRegistry.For(settings)`, возвращает инструменты |
| `Docx/DocxExtractor.cs` | Open XML SDK → `ContextDocument` |
| `Docx/DocxStyles.cs` | pStyle/outlineLvl → уровень заголовка; numPr → список |
| `Formats/DocumentRenderConverter.cs` | `IFormatConverter`: (docx MIME → target) через экстрактор + рендерер |
| `Spreadsheet/ISpreadsheetStore.cs`, `XlsxStore.cs`, `CsvStore.cs`, `SpreadsheetStores.cs` | чтение/дозапись по заголовкам |
| `Tools/DocumentExtractTool.cs` | `document_extract` |
| `Tools/SpreadsheetInspectTool.cs` | `spreadsheet_inspect` |
| `Tools/SpreadsheetReadRowsTool.cs` | `spreadsheet_read_rows` |
| `Tools/SpreadsheetAppendRowsTool.cs` | `spreadsheet_append_rows` |

Регистрируемые пары (все — из одного класса `DocumentRenderConverter`, разные target):

```
application/vnd.openxmlformats-officedocument.wordprocessingml.document -> text/markdown
application/vnd.openxmlformats-officedocument.wordprocessingml.document -> text/plain
application/vnd.openxmlformats-officedocument.wordprocessingml.document -> application/json
```

### Инструменты: контракт для модели

| Инструмент | Scope / Effect / Risk | Аргументы |
|---|---|---|
| `document_extract` | Project / Read / Low | `path`, `as` (`markdown`\|`text`\|`json`, по умолчанию `markdown`), `output`, `output_name` |
| `spreadsheet_inspect` | Project / Read / Low | `path`, `sheet?` → листы, заголовки, число строк |
| `spreadsheet_read_rows` | Project / Read / Low | `path`, `sheet?`, `limit?`, `offset?`, `output`, `output_name` |
| `spreadsheet_append_rows` | Project / **Write** / Medium | `path`, `sheet?`, `rows` (массив объектов `{"Заголовок": значение}`), `create?` |

Правила, которые обязаны быть в `Details` (а не в `Description`):

- ключи в `rows` — **заголовки колонок из первой строки листа**, не адреса ячеек;
- незнакомый заголовок = отказ со списком известных, а не молчаливое создание колонки;
- отсутствующие в объекте колонки остаются пустыми;
- `.xlsx` и `.csv` различаются по расширению пути; иное расширение — отказ.

### Границы и безопасность

- Пути только через `HostServices.Sandbox.Workspace`. Чтение — `ReadAllBytesAsync`;
  запись `.xlsx` — через `MapPathToHost` (у `IWorkspace` нет `WriteAllBytes`; это санкционированный
  выход, тем же пользуется SFTP-транспорт). `MapPathToHost == null` → отказ, а не `File.*` напрямую.
- `Scope.Agent` / `Scope.Skill` плагину запрещены (см. `src/plugins/AGENTS.md`).
- Никаких сетевых обращений, никакого запуска процессов.

---

## Этапы

- [x] **1. Модель.** Проект `src/documents/SPLA.Documents.Model` + блоки + три рендерера.
- [x] **2. DOCX-экстрактор.** Open XML SDK: заголовки, абзацы, списки, таблицы, гиперссылки,
      картинки как ссылки, разрывы страниц, метаданные пакета.
- [x] **3. Конвертеры в реестр.** `DocumentRenderConverter` + регистрация трёх пар из
      `DocumentsPlugin.Initialize`.
- [x] **4. Таблицы.** `XlsxStore` (ClosedXML) и `CsvStore`: `Inspect`, `ReadRows`, `AppendRows`.
- [x] **5. Инструменты.** Четыре инструмента с `Details`, честными `Scope/Effect/Risk`.
- [x] **6. Сборка и регистрация.** `SPLA.slnx`, `CopyPlugin` в csproj, строка в `PublishAll.ps1`,
      `agents/structure.md`, `agents/plugins.md` (список манифестов), `CHANGELOGS/current-log.md`
      + `current-list.md`.
- [x] **7. Тесты** в `tests/SPLA.Tests`: рендереры, экстрактор на собранном на лету `.docx`,
      CSV/XLSX round-trip, отказ на незнакомом заголовке, регистрация пар в реестре.
- [x] **8. Скилл** `documents.docx-to-registry` (`src/plugins/SPLA.Skills.Documents/`): процедура
      «прочитать документ → вытащить поля → дописать строку в реестр». Английский, как все скиллы.
      Кладётся в `plugins/documents/skills/` и в dev-, и в publish-сборке — расхождения, как у
      `SPLA.Skills.Network`, здесь нет.
- [ ] **9. Проверка на живом документе.** Синтетический `.docx` (заголовки, абзац, таблица с жирной
      шапкой) прогнан через MCP end-to-end: `document_extract` вернул корректный markdown, а
      `spreadsheet_append_rows` создал `registry.csv` с BOM и дописал строку. **Остаётся** прогон на
      настоящем документе пользователя: посмотреть нумерацию списков, вложенные таблицы, колонтитулы,
      и поправить нормализацию по факту.

## Дальше (не в этом плане)

- XLSX/PPTX/PDF на вход (`PdfPig`), провайдер `documents_pandoc`.
- Политика предпочтения при двух провайдерах на одну пару (ADR §4.1).
- Плагин, регистрирующий только конвертеры, без инструментов (ADR §4.2).
- Класс `Artifact/Layout`.
