using System.IO;

namespace SPLA.Domain.Editor;

/// <summary>
/// Простейший <see cref="IContentSource"/> поверх файловой системы: ссылка = путь к файлу
/// (напр. путь к table_АсбестКоличество.jsonl). Это базовый источник для standalone-сценария
/// и для проекта, лежащего на диске.
///
/// Важно: это лишь ОДНА из реализаций. Контракт <see cref="IContentSource"/> намеренно не привязан
/// к ФС — другие источники (ключ проекта, blob, in-memory от агента) реализуют тот же интерфейс,
/// а Core-редактор не знает разницы (см. docs/schema-editor-doc-a-schemas.md §4).
/// </summary>
public sealed class FileContentSource : IContentSource
{
    /// <summary>
    /// Считаем ссылку «нашей», если это полный (rooted) путь. Существование файла НЕ требуем:
    /// запись допустима в ещё не созданный файл, поэтому проверка на File.Exists была бы неверной.
    /// </summary>
    public bool CanResolve(string contentRef) =>
        !string.IsNullOrWhiteSpace(contentRef) && Path.IsPathRooted(contentRef);

    public string ReadText(string contentRef) => File.ReadAllText(contentRef);

    public void WriteText(string contentRef, string text)
    {
        // Гарантируем наличие каталога — иначе запись в новый подкаталог упадёт.
        var dir = Path.GetDirectoryName(contentRef);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(contentRef, text);
    }
}
