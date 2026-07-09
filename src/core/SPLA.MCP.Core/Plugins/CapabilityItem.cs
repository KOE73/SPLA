namespace SPLA.MCP.Core.Plugins;

public enum CapabilityKind { Plugin, Tool, Skill }

public class CapabilityItem
{
    public required string Id { get; init; }
    public required CapabilityKind Kind { get; init; }
    public required string Label { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? PluginId { get; init; }
    public bool IsEnabled { get; set; } = true;
    public bool IsPreloaded { get; set; } = false;

    // Source references — one will be set depending on Kind
    public PluginDescriptor? SourcePlugin { get; init; }
    public SkillMeta? SourceSkill { get; init; }
}
