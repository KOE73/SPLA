Ниже — нормализованное резюме по SPLA и работе с документами.

## 1. Реальная задача

Исходный практический кейс:

```text
DOCX
  ↓
понять содержимое
  ↓
переработать / извлечь нужные данные
  ↓
добавить данные в Excel
```

Важно: конечная задача не «конвертировать Word в Markdown». Markdown — лишь возможное промежуточное представление для LLM.

Более правильная модель:

```text
Source document
      ↓
Context extraction
      ↓
Semantic representation
      ↓
LLM reasoning / transformation
      ↓
Structured data
      ↓
Spreadsheet operation
```

---

# 2. Главное архитектурное разделение

В разговоре выделились два принципиально разных класса работы с документами.

| Класс                 | Что важно                                                                      | Что не важно                                  | Пример                                            |
| --------------------- | ------------------------------------------------------------------------------ | --------------------------------------------- | ------------------------------------------------- |
| **Content / Data**    | смысл, текст, заголовки, списки, таблицы, значения                             | шрифты, цвета, точные координаты, внешний вид | прочитать DOCX и понять заявку                    |
| **Artifact / Layout** | структура самого файла, стили, формулы, объединения, изображения, расположение | —                                             | изменить Excel-шаблон и сохранить его внешний вид |

Это применимо не только к Excel:

```text
Word:
  Content → понять текст документа
  Artifact → изменить DOCX, сохранив оформление

Excel:
  Data → таблицы, строки, значения
  Artifact → стили, формулы, layout, charts

PDF:
  Content → извлечь читаемый context
  Artifact → работа с самим PDF как визуальным документом
```

---

# 3. Context — более правильная абстракция, чем ConvertToMarkdown

В SPLA уже есть идея:

> прочитать заданный путь и добавить содержимое в context.

Для такого сценария внешний формат файла — вторичен.

То есть правильнее:

```text
path
 ↓
ContextExtractor
 ↓
ContextDocument
 ↓
LLM context
```

а не:

```text
path
 ↓
ConvertToMarkdown
 ↓
markdown
```

Потому что `.md` сам по себе уже практически является Context-ready форматом, а `.docx/.xlsx/.pdf` требуют semantic extraction.

---

# 4. Что считается семантикой

Для обычного Context желательно сохранять:

* заголовки и их уровни;
* абзацы;
* списки;
* таблицы;
* ссылки;
* code blocks;
* страницы/секции там, где это существенно;
* metadata;
* изображения хотя бы как references/descriptions при необходимости.

Обычно не нужны:

* шрифт 12/14 pt;
* цвет;
* точные размеры;
* отступы;
* рамки;
* координаты элементов.

То есть:

```text
Heading 1
Paragraph
List
Table
...
```

важны, а:

```text
Arial 11pt
margin-left: 14 mm
RGB(35,35,35)
```

обычно нет.

---

# 5. Внутреннее представление Context

Не стоит архитектурно привязывать SPLA к Markdown.

Лучше:

```csharp
public sealed record ContextDocument(
    DocumentMetadata Metadata,
    IReadOnlyList<ContextBlock> Blocks);
```

Например:

```text
ContextBlock
 ├── Heading
 ├── Paragraph
 ├── List
 ├── Table
 ├── Code
 ├── Image
 └── PageBreak
```

А Markdown становится renderer'ом:

```text
DOCX/XLSX/PDF
      ↓
ContextDocument
      ↓
MarkdownContextRenderer
      ↓
LLM
```

Это позволяет потом иметь:

```text
Markdown renderer
Plain text renderer
JSON renderer
Chunk renderer
Embedding renderer
```

без переписывания extraction.

---

# 6. Контракт SPLA

Примерно такой уровень:

```csharp
public interface IContextExtractorPlugin
{
    IReadOnlyCollection<string> SupportedFormats { get; }

    Task<ContextDocument> ExtractAsync(
        string path,
        ContextExtractionOptions options,
        CancellationToken cancellationToken);
}
```

Ещё лучше определять capability не только по расширению:

```text
extension
media type
magic bytes
```

---

# 7. Архитектура плагина

Ты уточнил важный момент:

**динамически подключается твой plugin**, а не набор сторонних DLL непосредственно в SPLA.

То есть:

```text
SPLA
  ↓ dynamic load
DocumentContextPlugin
```

Для SPLA plugin выглядит монолитным.

Его внутренности SPLA не интересуют:

```text
DocumentContext.Native
 ├── OpenXML
 ├── ClosedXML
 ├── PdfPig
 └── собственная semantic normalization
```

или:

```text
DocumentContext.Pandoc
 ├── твой plugin
 └── pandoc executable
```

---

# 8. «Монолитный plugin» ≠ один физический DLL

Практичнее считать единицей развертывания **каталог plugin**:

```text
/plugins/
  DocumentContext.Native/
      DocumentContext.Native.dll
      DocumentFormat.OpenXml.dll
      ClosedXML.dll
      PdfPig.dll
      ...
```

SPLA знает только:

```text
DocumentContext.Native
```

Зависимости — внутренняя деталь plugin.

Для изоляции разумно использовать:

```text
AssemblyLoadContext
```

Так можно иметь разные версии зависимостей у разных plugins.

---

# 9. Первый вариант реализации

При текущих ограничениях:

* .NET;
* желательно in-process;
* Windows + потенциальный Linux;
* без обязательного внешнего Java/Tika-сервера;
* отдельный exe допустим;
* лицензия должна позволять коммерческое применение;

самый понятный первый стек:

| Формат    | Реализация V1    | Роль                                |
| --------- | ---------------- | ----------------------------------- |
| DOCX      | **Open XML SDK** | извлечение структуры Word           |
| XLSX/XLSM | **ClosedXML**    | семантическое чтение workbook/table |
| PDF       | **PdfPig**       | текст + layout + coordinates        |
| PPTX      | **Open XML SDK** | семантика презентации               |
| MD        | native .NET      | почти напрямую                      |
| TXT       | native .NET      | напрямую                            |
| JSON      | native .NET      | structured context                  |
| XML       | native .NET      | structured/text context             |
| CSV/TSV   | native .NET      | table context                       |

Архитектура:

```text
                 DocumentContext.Native
                          │
          ┌───────────────┼───────────────┐
          ↓               ↓               ↓
       OpenXML         ClosedXML        PdfPig
     DOCX/PPTX           XLSX             PDF
          │               │               │
          └───────────────┼───────────────┘
                          ↓
                 SemanticNormalizer
                          ↓
                   ContextDocument
```

---

# 10. Что дают эти библиотеки

## Open XML SDK

Подходит для:

```text
DOCX
XLSX
PPTX
```

Но является достаточно низкоуровневой моделью OOXML.

Получишь примерно:

```text
Paragraph
Run
Table
StyleId
NumberingProperties
Drawing
...
```

А преобразование:

```text
StyleId=Heading1 → Heading(level=1)
```

уже будет нашей логикой.

Плюсы:

* native .NET;
* Linux;
* MIT;
* Microsoft/.NET Foundation;
* полный доступ к OOXML.

Минус:

* semantic normalization надо делать.

---

## ClosedXML

Для Excel значительно удобнее прямого OpenXML.

Подходит для:

```text
XLSX
XLSM
```

Нормально работает с:

```text
Workbook
Worksheet
Range
Table
Row
Cell
Formula
```

Для Context это может выглядеть так:

```markdown
# Workbook: requests.xlsx

## Sheet: Requests

| Date | Company | Amount |
|---|---|---:|
| 2026-08-20 | A | 123000 |
| 2026-08-21 | B | 87000 |
```

Лицензия: MIT.

---

## PdfPig

Для PDF:

```text
PDF
 ↓
pages
 ↓
letters / words
 ↓
coordinates
 ↓
reading order/layout heuristics
 ↓
Context
```

Плюсы:

* .NET;
* Linux;
* Apache-2.0;
* текст;
* координаты;
* изображения;
* metadata;
* layout information.

Но PDF принципиально сложнее DOCX.

В PDF часто физически нет:

```text
это Heading1
это paragraph
это table
```

Есть:

```text
символ X расположен в координатах 53,712
```

Поэтому semantic reconstruction неизбежно будет эвристическим.

---

# 11. Pandoc

Pandoc не забываем.

Он особенно хорош для **семантического преобразования текстовых документов**:

```text
DOCX
 ↓
Pandoc Reader
 ↓
Pandoc AST
 ↓
Markdown Writer
```

Он уже сам понимает:

* headings;
* paragraphs;
* lists;
* tables;
* links;
* footnotes;
* images.

Пример:

```bash
pandoc file.docx -t gfm
```

Можно читать stdout и вообще не создавать `.md` на диске.

### Основные форматы Pandoc

Хорошо покрывает:

```text
DOCX
ODT
RTF
HTML
Markdown
CommonMark
GFM
AsciiDoc
reStructuredText
Org
LaTeX
DocBook
JATS
TEI
MediaWiki
EPUB
ipynb
и др.
```

Но как универсальный Context provider:

* PDF input — слабое место/нет;
* Excel — не его основная задача.

Лицензия:

```text
GPL-2.0-or-later
```

Для SPLA можно использовать как отдельный executable/provider:

```text
DocumentContext.Pandoc
       ↓
    pandoc.exe
```

Windows/Linux поддерживаются.

---

# 12. Apache Tika

Также **не забываем**, но сейчас не первая реализация.

Технически он очень хорошо соответствует задаче universal ingestion:

```text
Word
Excel
PDF
PowerPoint
RTF
ODF
HTML
XML
EPUB
email
archives
...
```

и ориентирован именно на:

```text
content + metadata extraction
```

Лицензия:

```text
Apache-2.0
```

Но требует Java/Tika Server или другого внешнего runtime.

Ты пока этот класс решений отложил.

---

# 13. Коммерческие варианты

Они могут стать отдельными реализациями того же plugin contract.

## Aspose

Очень интересный вариант для будущего:

```text
DocumentContext.Aspose
```

Покрывает большую часть:

```text
Word
Excel
PDF
PowerPoint
HTML
Markdown
RTF
ODT/ODS
EPUB
CSV
JSON
и много других
```

Большой плюс:

```text
in-process .NET
Windows/Linux
без установленного Office
```

Минус:

```text
commercial license
```

Но поскольку provider заменяемый, это не архитектурная проблема.

---

## Syncfusion

Аналогично:

```text
DocumentContext.Syncfusion
```

Поддерживает:

```text
Word
Excel
PDF
PowerPoint
Markdown
...
```

Native .NET, Linux.

Есть Community License, но она ограничена условиями бизнеса.

Ты справедливо отметил: для тебя сейчас лимит $1M выручки не проблема, **но потенциальные потребители SPLA могут быть гораздо крупнее**.

Поэтому не стоит делать Syncfusion обязательной фундаментальной зависимостью.

Как optional commercial provider — нормально.

---

# 14. Сводная таблица вариантов

| Provider                                 | In-process .NET | Linux | Word | Excel |     PDF | Другие форматы               | Семантика готова          | Лицензия             | Статус        |
| ---------------------------------------- | --------------: | ----: | ---: | ----: | ------: | ---------------------------- | ------------------------- | -------------------- | ------------- |
| **Native: OpenXML + ClosedXML + PdfPig** |               ✅ |     ✅ |    ✅ |     ✅ |       ✅ | PPTX, MD, TXT, CSV...        | частично, normalizer свой | MIT + Apache-2       | **V1**        |
| **Pandoc**                               |           ❌ exe |     ✅ |    ✅ |    ⚠️ | ❌ input | очень много document formats | ✅ очень хорошая           | GPL-2+               | V2/provider   |
| **Aspose.Total**                         |               ✅ |     ✅ |    ✅ |     ✅ |       ✅ | очень много                  | ✅                         | Commercial           | V2/provider   |
| **Syncfusion**                           |               ✅ |     ✅ |    ✅ |     ✅ |       ✅ | много                        | ✅                         | Community/Commercial | V2/provider   |
| **Apache Tika**                          |      ❌ external |     ✅ |    ✅ |     ✅ |       ✅ | огромный набор               | ✅                         | Apache-2             | позже         |
| **NPOI**                                 |               ✅ |     ✅ |    ✅ |     ✅ |       ❌ | Office legacy                | частично                  | сейчас есть нюансы   | пока не брать |

---

# 15. Excel: чтение и запись — тоже два уровня

Для задачи:

> прочитать DOCX → понять → добавить строку в Excel

не нужен специальный `ExcelAppendTool`.

Лучше capability:

```text
spreadsheet.data
```

Например:

```text
inspect
read
query
append_rows
update_rows
delete_rows
```

Пример:

```json
{
  "operation": "append_rows",
  "file": "registry.xlsx",
  "sheet": "Requests",
  "rows": [
    {
      "Дата": "2026-08-24",
      "Организация": "ООО Ромашка",
      "Сумма": 1250000
    }
  ]
}
```

То есть модель работает не с:

```text
A27
B27
C27
```

а с:

```text
Дата
Организация
Сумма
```

По заголовкам.

---

# 16. Spreadsheet Artifact API — отдельно

Если пользователь скажет:

> добавь строку в шаблон, скопируй оформление предыдущей, сохрани формулы, объединения и печатную область

это другой класс:

```text
spreadsheet.artifact
```

Например:

```text
read_cells
write_cells
copy_range
copy_style
merge
set_formula
set_dimensions
charts
images
print_settings
```

Для первого можно использовать ClosedXML.

Для глубокого Artifact уровня — OpenXML.

А если когда-нибудь потребуется поведение именно настоящего Excel:

```text
Excel Application Automation
```

но это уже третий уровень и потенциально Windows-зависимая история.

---

# 17. Итоговая модель начинает выглядеть так

```text
                            SPLA
                              │
                    Dynamic Plugin API
                              │
                 DocumentContext.Native
                              │
       ┌──────────────────────┼──────────────────────┐
       │                      │                      │
     DOCX                   XLSX                   PDF
       │                      │                      │
    OpenXML               ClosedXML              PdfPig
       │                      │                      │
       └──────────────────────┼──────────────────────┘
                              │
                     Semantic Normalizer
                              │
                       ContextDocument
                              │
               ┌──────────────┼──────────────┐
               ↓              ↓              ↓
           Markdown          JSON          Chunks
               │
               ↓
              LLM
               │
               ↓
        Structured Result
               │
               ↓
        spreadsheet.data
               │
               ↓
              XLSX
```

---

# 18. Что пока считаем решённым

**Не делать фундамент SPLA вокруг `DOCX → MD`.**

Фундамент:

```text
file → semantic Context
```

**V1 provider**:

```text
DocumentContext.Native
```

с внутренними:

```text
OpenXML
ClosedXML
PdfPig
native text readers
```

**Следующие альтернативные providers**:

```text
DocumentContext.Pandoc
DocumentContext.Aspose
DocumentContext.Syncfusion
DocumentContext.Tika
```

Они могут сосуществовать.

И самое существенное архитектурное решение из разговора:

```text
Context/Data
```

и

```text
Artifact/Layout
```

— это **разные уровни работы с документом**, их не стоит смешивать в одном API.
