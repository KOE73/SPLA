namespace SPLA.Domain.Editor;

/// <summary>
/// Поставщик схемы по имени класса (см. docs/schema-editor-doc-a-schemas.md §4).
/// Имя включает версию для DATA-схемы ("sql-table@1") или суффикс ".ui" для UI-схемы ("sql-table.ui").
/// Реализация НЕ предполагает файловую систему: схема может прийти из embedded-ресурса плагина,
/// из проекта или из in-memory от агента. Несколько провайдеров опрашиваются реестром по очереди.
/// </summary>
public interface IJsonSchemaProvider
{
    /// <summary>Может ли отдать ИМЕННО эту схему по имени.</summary>
    bool CanResolve(string schemaName);

    /// <summary>Вернуть схему. Возвращать null, если !CanResolve(schemaName).</summary>
    JsonSchema? Resolve(string schemaName);
}
