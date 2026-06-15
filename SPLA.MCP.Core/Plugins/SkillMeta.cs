namespace SPLA.MCP.Core.Plugins;

public class SkillMeta
{
    public required string Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public required string FilePath { get; init; }
    public string PluginId { get; init; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsPreloaded { get; set; } = false;
}
