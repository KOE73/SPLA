using System.Text.Json;

namespace SPLA.Domain.Editor;

/// <summary>
/// Нейтральная обёртка над JSON-документом схемы (см. docs/schema-editor-doc-a-schemas.md).
/// Для DATA-схемы содержимое — всегда валидный JSON Schema 2020-12 (стандарт, рендер-независим).
/// Для UI-схемы — отдельный документ (диалект выбранного рендера), резолвится по имени "<name>.ui".
/// Тип намеренно тонкий: хранит сырой JSON-текст + распарсенный документ; редактор просто прокидывает
/// его в WebView, не интерпретируя на стороне C#.
/// </summary>
public sealed class JsonSchema
{
    /// <summary>Имя класса схемы с версией, напр. "sql-table@1" (для UI — "sql-table.ui").</summary>
    public string Name { get; }

    /// <summary>Сырой JSON-текст схемы — то, что уходит в WebView как есть.</summary>
    public string Json { get; }

    private JsonSchema(string name, string json)
    {
        Name = name;
        Json = json;
    }

    /// <summary>Создать из сырого JSON-текста. Бросает, если текст не парсится как JSON.</summary>
    public static JsonSchema FromJson(string name, string json)
    {
        // Валидируем только синтаксис JSON. Соответствие стандарту JSON Schema 2020-12 для DATA-схем —
        // ответственность поставщика (плагина); здесь его не навязываем, чтобы не тащить валидатор в Domain.
        using var _ = JsonDocument.Parse(json);
        return new JsonSchema(name, json);
    }
}
