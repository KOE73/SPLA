namespace SPLA.Domain.Editor;

/// <summary>
/// Optional interface for plugins that contribute JSON schemas to the shared <see cref="SchemaRegistry"/>.
/// <c>PluginManager</c> checks for it after each plugin initializes and registers the provider automatically.
/// </summary>
public interface ISchemaContributor
{
    IJsonSchemaProvider GetSchemaProvider();
}
