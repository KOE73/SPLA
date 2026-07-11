using SPLA.MCP.Core.Interfaces;

namespace SPLA.MCP.Core.Plugins;

public sealed class PluginDescriptor
{
    public required PluginMeta Meta { get; init; }
    public required string DirectoryPath { get; init; }
    public bool UserEnabled { get; init; } = true;
    public PluginEffectiveState EffectiveState { get; set; } = PluginEffectiveState.Enabled;
    public string EffectiveStateReason { get; set; } = string.Empty;
    public string EffectivePrompt { get; set; } = string.Empty;
    public List<SplaPluginUiCommand> Commands { get; } = [];

    /// <summary>Whether the plugin's tools are live. A <see cref="PluginEffectiveState.Degraded"/>
    /// plugin still counts — it loaded and registered its tools; the self-check only flags a risk.</summary>
    public bool IsEffectivelyEnabled =>
        EffectiveState is PluginEffectiveState.Enabled or PluginEffectiveState.Degraded;
}

public enum PluginEffectiveState
{
    Enabled,
    DisabledByUser,
    DisabledByMissingDependency,
    DisabledByDependency,
    LoadError,
    /// <summary>Loaded and active, but the plugin's own load-time self-check reported a problem
    /// (a runtime prerequisite missing/broken). Tools stay registered; this is a diagnostic flag.</summary>
    Degraded
}
