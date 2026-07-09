using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SPLA.Domain.Editor;

/// <summary>
/// Переиспользуемый <see cref="IJsonSchemaProvider"/>, отдающий схемы из <b>embedded-ресурсов</b>
/// произвольной сборки (см. docs/schema-editor-doc-a-schemas.md §4 — «источник не обязан быть ФС»).
///
/// Назначение: плагин (напр. SQL) вшивает свои схемы как ресурсы и отдаёт их через этот провайдер,
/// не таща ни одного файла рядом с приложением. Это ровно тот же приём, что и у Core-редактора,
/// который вшивает свой HTML-бандл в DLL.
///
/// Провайдер — чистый словарь «имя схемы → имя ресурса», поэтому одной сборке достаточно одного
/// экземпляра, покрывающего и DATA-схему ("sql-table@1"), и UI-схему ("sql-table.ui").
/// </summary>
public sealed class EmbeddedJsonSchemaProvider : IJsonSchemaProvider
{
    private readonly Assembly _assembly;

    // Имя схемы (то, что декларирует файл данных) → полное имя manifest-ресурса в сборке.
    private readonly IReadOnlyDictionary<string, string> _nameToResource;

    // Кэш уже прочитанных схем: ресурсы неизменны на время жизни процесса, читать повторно незачем.
    private readonly Dictionary<string, JsonSchema> _cache = new(StringComparer.Ordinal);

    /// <param name="assembly">Сборка, в которой лежат embedded-ресурсы схем.</param>
    /// <param name="nameToResource">
    /// Карта «имя схемы → имя ресурса». Имя схемы сравнивается строго (ordinal), потому что версия
    /// — часть имени ("sql-table@1" ≠ "sql-table@2"), и путать версии нельзя.
    /// </param>
    public EmbeddedJsonSchemaProvider(
        Assembly assembly,
        IReadOnlyDictionary<string, string> nameToResource)
    {
        _assembly = assembly;
        _nameToResource = new Dictionary<string, string>(nameToResource, StringComparer.Ordinal);
    }

    public bool CanResolve(string schemaName) => _nameToResource.ContainsKey(schemaName);

    public JsonSchema? Resolve(string schemaName)
    {
        // Не наш — пусть реестр спросит следующего провайдера.
        if (!_nameToResource.TryGetValue(schemaName, out var resourceName))
            return null;

        // Отдаём из кэша, если уже читали.
        if (_cache.TryGetValue(schemaName, out var cached))
            return cached;

        // Ресурс объявлен в карте, но физически отсутствует в сборке — это ошибка сборки/конфигурации,
        // а не «не нашёл по имени». Падаем громко, чтобы поймать на этапе разработки, а не молча.
        using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Schema '{schemaName}' maps to embedded resource '{resourceName}', " +
                $"which was not found in assembly '{_assembly.GetName().Name}'. " +
                "Check the <EmbeddedResource>/<LogicalName> wiring in the .csproj.");

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var json = reader.ReadToEnd();

        // JsonSchema.FromJson проверит, что это валидный JSON (синтаксис). Соответствие самому
        // стандарту JSON Schema 2020-12 для DATA-схем — ответственность поставщика ресурса.
        var schema = JsonSchema.FromJson(schemaName, json);
        _cache[schemaName] = schema;
        return schema;
    }
}
