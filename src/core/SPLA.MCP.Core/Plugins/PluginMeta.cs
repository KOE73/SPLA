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

    /// <summary>Relative path (from the plugin's directory) to a prebuilt, self-contained ES module
    /// the web client dynamically imports to render this plugin's own DOCK PANEL — a tab in the
    /// workspace, not a settings page. Same <c>mount(el, api)</c> contract as
    /// <see cref="WebSettingsEntry"/>; absent means the plugin contributes no panel.</summary>
    [YamlMember(Alias = "web_panel_entry")]
    public string? WebPanelEntry { get; set; }

    /// <summary>The panel's tab title. Lives here rather than in the bundle because the tool strip
    /// has to draw the button BEFORE the bundle is loaded — and still draw it if the bundle fails to
    /// load. English only. Falls back to the plugin's name.</summary>
    [YamlMember(Alias = "panel_title")]
    public string? PanelTitle { get; set; }

    /// <summary>An emoji for the panel's tool-strip button and tab. Here for the same reason as
    /// <see cref="PanelTitle"/>.</summary>
    [YamlMember(Alias = "panel_icon")]
    public string? PanelIcon { get; set; }

    /// <summary>One line: what this plugin's tool set is. Shown in the UI and used as the first half
    /// of the set's declaration when its level is "on agent demand". English only.</summary>
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>When the agent should call for this set — the half of a declaration that cannot be
    /// derived from <see cref="Description"/> and has to be written by the set's author. Read only
    /// at the "on agent demand" level; see <c>agents/toolsets.md</c>. English only.</summary>
    [YamlMember(Alias = "summon")]
    public string Summon { get; set; } = string.Empty;

    [YamlMember(Alias = "default_prompt")]
    public string DefaultPrompt { get; set; } = string.Empty;

    [YamlMember(Alias = "depends_on")]
    public List<string> DependsOn { get; set; } = [];

    [YamlMember(Alias = "metadata")]
    public Dictionary<string, string> Metadata { get; set; } = [];

    [YamlMember(Alias = "commands")]
    public List<SplaPluginUiCommand> Commands { get; set; } = [];
}
