using System.Collections.Generic;

namespace SPLA.Domain.Editor;

/// <summary>
/// Реестр поставщиков схем. Собирает <see cref="IJsonSchemaProvider"/> (от плагинов, проекта, агента)
/// и резолвит схему по первому совпадению в порядке регистрации (см. docs/schema-editor-plan.md —
/// разрешение по имени класса). Подходит и для DATA-схем, и для UI-схем (имя несёт суффикс ".ui").
/// </summary>
public sealed class SchemaRegistry
{
    private readonly List<IJsonSchemaProvider> _providers = [];

    public void Register(IJsonSchemaProvider provider) => _providers.Add(provider);

    public bool TryResolve(string schemaName, out JsonSchema? schema)
    {
        foreach (var provider in _providers)
        {
            if (!provider.CanResolve(schemaName)) continue;
            var resolved = provider.Resolve(schemaName);
            if (resolved is null) continue;
            schema = resolved;
            return true;
        }
        schema = null;
        return false;
    }

    /// <summary>Резолв DATA-схемы (обязательна). Бросает, если ни один провайдер не отдал.</summary>
    public JsonSchema Resolve(string schemaName) =>
        TryResolve(schemaName, out var schema) && schema is not null
            ? schema
            : throw new KeyNotFoundException($"No provider resolved schema '{schemaName}'.");
}
