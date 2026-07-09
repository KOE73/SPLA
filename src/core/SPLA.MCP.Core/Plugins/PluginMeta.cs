using YamlDotNet.Serialization;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.MCP.Core.Plugins;

public class PluginMeta
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "version")]
    public string Version { get; set; } = string.Empty;

    [YamlMember(Alias = "type")]
    public string Type { get; set; } = "dll"; // "dll" or "exe"

    [YamlMember(Alias = "entry_point")]
    public string EntryPoint { get; set; } = string.Empty;

    /// <summary>Relative path (from the plugin's directory) to a prebuilt, self-contained ES module
    /// the web client dynamically imports to render this plugin's settings UI. Optional — plugins
    /// without one fall back to the generic YAML-blob editor.</summary>
    [YamlMember(Alias = "web_settings_entry")]
    public string? WebSettingsEntry { get; set; }

    [YamlMember(Alias = "default_prompt")]
    public string DefaultPrompt { get; set; } = string.Empty;

    [YamlMember(Alias = "depends_on")]
    public List<string> DependsOn { get; set; } = [];

    [YamlMember(Alias = "metadata")]
    public Dictionary<string, string> Metadata { get; set; } = [];

    [YamlMember(Alias = "commands")]
    public List<SplaPluginUiCommand> Commands { get; set; } = [];
}
