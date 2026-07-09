using SPLA.Plugins.Host.Avalonia;

namespace SPLA.Plugins.Sql.Avalonia;

public sealed class SqlAvaloniaPlugin : IAvaloniaPlugin, IAvaloniaPluginSettingsProvider
{
    // No panels — this plugin only contributes a settings editor.
    public IEnumerable<AvaloniaPluginPanelDescriptor> GetPanels(AvaloniaPluginContext context)
        => [];

    public AvaloniaPluginSettingsDescriptor GetSettings(AvaloniaPluginSettingsContext context) => new()
    {
        PluginId = "sql",
        DisplayName = "Database Connections",
        Description = "Named connections available to the SQL agent tools.",
        CreateEditor = () => new DbConnectionsSettingsEditor(context.WorkspacePath)
    };
}
