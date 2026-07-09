namespace SPLA.MCP.Core.Plugins;

public class SkillMeta
{
    public required string Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public required string FilePath { get; init; }
    public string PluginId { get; init; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsPreloaded { get; set; } = false;

    /// <summary>Set when the skill was registered from a type:skills plugin descriptor.
    /// GetEnabled() uses this to honour the plugin's IsEffectivelyEnabled state.</summary>
    public PluginDescriptor? OwnerPlugin { get; set; }
}
