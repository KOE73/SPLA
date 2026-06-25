namespace SPLA.Domain.Editor;

/// <summary>
/// Источник редактируемого содержимого по ссылке (см. docs/schema-editor-doc-a-schemas.md §4).
/// Ссылка НЕ обязана быть путём в ФС — это может быть ключ проекта, blob-хэндл или in-memory.
/// </summary>
public interface IContentSource
{
    bool CanResolve(string contentRef);

    /// <summary>Сырое содержимое редактируемого документа (напр. JSONL-текст таблицы).</summary>
    string ReadText(string contentRef);

    /// <summary>Сохранить правки обратно туда же.</summary>
    void WriteText(string contentRef, string text);
}
