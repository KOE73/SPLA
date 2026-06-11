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

    public bool IsEffectivelyEnabled => EffectiveState == PluginEffectiveState.Enabled;
}

public enum PluginEffectiveState
{
    Enabled,
    DisabledByUser,
    DisabledByMissingDependency,
    DisabledByDependency,
    LoadError
}
