using SPLA.Domain.Editor;
using SPLA.Service.Contracts;

namespace SPLA.Service;

internal static class SchemaOps
{
    /// <summary>
    /// Resolves a data schema by name and the matching UI schema by convention
    /// (strip "@version" suffix, append ".ui" — e.g. "sql-table@1" → "sql-table.ui").
    /// </summary>
    public static SchemaResultPayload Get(SchemaRegistry registry, string name)
    {
        if (!registry.TryResolve(name, out var dataSchema) || dataSchema is null)
            return new SchemaResultPayload { Name = name, Error = $"Schema '{name}' not found." };

        var atIdx = name.IndexOf('@');
        var uiName = (atIdx >= 0 ? name[..atIdx] : name) + ".ui";
        registry.TryResolve(uiName, out var uiSchema);

        return new SchemaResultPayload
        {
            Name = name,
            DataSchema = dataSchema.Json,
            UiSchema = uiSchema?.Json,
        };
    }
}
