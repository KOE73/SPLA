using Avalonia.Controls;

namespace SPLA.Plugins.Host.Avalonia;

/// <summary>
/// Implemented by an Avalonia plugin that contributes its own settings UI.
/// The editor is rendered nested under the plugin's row in the Settings window.
/// </summary>
public interface IAvaloniaPluginSettingsProvider
{
    AvaloniaPluginSettingsDescriptor GetSettings(AvaloniaPluginSettingsContext context);
}

public sealed class AvaloniaPluginSettingsContext
{
    public required string WorkspacePath { get; init; }
}

public sealed class AvaloniaPluginSettingsDescriptor
{
    /// <summary>The plugin id this editor belongs to — used to nest it under the right plugin.</summary>
    public required string PluginId { get; init; }
    public required string DisplayName { get; init; }
    public string? Description { get; init; }

    /// <summary>Factory for the editor instance (one per Settings window open).</summary>
    public required Func<IPluginSettingsEditor> CreateEditor { get; init; }
}

/// <summary>
/// The live editor: an Avalonia control bound to the plugin's own view model,
/// plus load/save against an opaque settings store the host persists into .spla.
/// </summary>
public interface IPluginSettingsEditor
{
    Control View { get; }

    /// <summary>Populate the editor from the current settings.</summary>
    void Load(IPluginSettingsStore store);

    /// <summary>Write the edited values back into the store. Called on "Save Settings".</summary>
    void Save(IPluginSettingsStore store);
}

/// <summary>
/// Opaque per-plugin settings storage. The host owns persistence; the plugin owns the schema.
/// Only YAML strings cross the boundary so each side may use its own serializer.
/// </summary>
public interface IPluginSettingsStore
{
    /// <summary>Current settings as a YAML string, or null when nothing is stored yet.</summary>
    string? GetYaml();

    /// <summary>Replace the stored settings with the given YAML string (null clears them).</summary>
    void SetYaml(string? yaml);
}
