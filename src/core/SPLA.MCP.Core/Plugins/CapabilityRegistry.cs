using SPLA.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.MCP.Core.Plugins;

public class CapabilityRegistry
{
    private readonly List<CapabilityItem> _items = [];

    public IReadOnlyList<CapabilityItem> Items => _items;

    public void Build(
        IReadOnlyList<PluginDescriptor> plugins,
        IEnumerable<ToolDefinition> tools,
        IReadOnlyList<SkillMeta> skills)
    {
        _items.Clear();

        // Plugins
        foreach (var plugin in plugins)
        {
            _items.Add(new CapabilityItem
            {
                Id = plugin.Meta.Id,
                Kind = CapabilityKind.Plugin,
                Label = plugin.Meta.Id,
                Description = plugin.Meta.DefaultPrompt.Split('\n').FirstOrDefault()?.Trim() ?? string.Empty,
                PluginId = plugin.Meta.Id,
                IsEnabled = plugin.IsEffectivelyEnabled,
                SourcePlugin = plugin
            });
        }

        // Tools
        foreach (var tool in tools)
        {
            var pluginId = tool.Function.Name.Contains('.')
                ? tool.Function.Name.Split('.')[0]
                : null;

            _items.Add(new CapabilityItem
            {
                Id = tool.Function.Name,
                Kind = CapabilityKind.Tool,
                Label = tool.Function.Name,
                Description = tool.Function.Description,
                PluginId = pluginId,
            });
        }

        // Skills
        foreach (var skill in skills)
        {
            _items.Add(new CapabilityItem
            {
                Id = skill.Id,
                Kind = CapabilityKind.Skill,
                Label = skill.Id,
                Description = skill.Description,
                PluginId = string.IsNullOrEmpty(skill.PluginId) ? null : skill.PluginId,
                IsEnabled = skill.IsEnabled && (skill.OwnerPlugin?.IsEffectivelyEnabled ?? true),
                IsPreloaded = skill.IsPreloaded,
                SourceSkill = skill
            });
        }
    }

    public IEnumerable<CapabilityItem> GetByKind(CapabilityKind kind) =>
        _items.Where(i => i.Kind == kind);

    public IEnumerable<CapabilityItem> GetEnabledSkills() =>
        _items.Where(i => i.Kind == CapabilityKind.Skill && i.IsEnabled);
}
