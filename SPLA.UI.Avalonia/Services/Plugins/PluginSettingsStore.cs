using SPLA.Plugins.Host.Avalonia;

namespace SPLA.UI.Avalonia.Services.Plugins;

/// <summary>
/// Host-side implementation of the opaque plugin settings store.
/// Holds a YAML string; the host converts it to/from the nested mapping persisted in .spla.
/// </summary>
public sealed class PluginSettingsStore : IPluginSettingsStore
{
    private string? _yaml;

    public PluginSettingsStore(string? initialYaml) => _yaml = initialYaml;

    public string? GetYaml() => _yaml;

    public void SetYaml(string? yaml) => _yaml = yaml;
}
