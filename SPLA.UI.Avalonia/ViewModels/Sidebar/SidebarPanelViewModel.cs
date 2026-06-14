using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SPLA.UI.Avalonia.ViewModels.Sidebar;

public partial class SidebarPanelViewModel : ObservableObject
{
    public ObservableCollection<SidebarNodeViewModel> Nodes { get; } = [];

    private readonly Dictionary<string, SidebarNodeViewModel> _toolNodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SidebarNodeViewModel> _skillNodes = [];

    public void Build(IReadOnlyList<PluginDescriptor> plugins, IEnumerable<ToolDefinition> allTools, string pluginsDir)
    {
        Nodes.Clear();
        _toolNodes.Clear();
        _skillNodes.Clear();

        var toolList = allTools.ToList();
        var pluginIds = new HashSet<string>(plugins.Select(p => p.Meta.Id), StringComparer.OrdinalIgnoreCase);

        var toolGroups = toolList
            .GroupBy(t => t.Function.Name.Split('.')[0], StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Built-in groups (not managed by any loaded plugin)
        foreach (var groupKey in toolGroups.Keys.Where(k => !pluginIds.Contains(k)).OrderBy(k => k))
        {
            var groupNode = new SidebarNodeViewModel
            {
                Kind = SidebarNodeKind.Plugin,
                Id = groupKey,
                Label = groupKey,
            };
            AddToolChildren(groupNode, toolGroups[groupKey], groupKey);
            Nodes.Add(groupNode);
        }

        // Plugin nodes with their tools and skills
        foreach (var plugin in plugins)
        {
            var pluginNode = new SidebarNodeViewModel
            {
                Kind = SidebarNodeKind.Plugin,
                Id = plugin.Meta.Id,
                Label = plugin.Meta.Id,
                IsEnabled = plugin.IsEffectivelyEnabled
            };

            if (toolGroups.TryGetValue(plugin.Meta.Id, out var pluginTools))
            {
                AddToolChildren(pluginNode, pluginTools, plugin.Meta.Id);
            }

            var skillsDir = Path.Combine(pluginsDir, plugin.Meta.Id, "skills");
            if (Directory.Exists(skillsDir))
            {
                foreach (var skillFile in Directory.GetFiles(skillsDir, "*.md").OrderBy(f => f))
                {
                    var (id, description) = ParseSkillFrontmatter(skillFile);
                    var label = id.StartsWith(plugin.Meta.Id + ".", StringComparison.OrdinalIgnoreCase)
                        ? id[(plugin.Meta.Id.Length + 1)..]
                        : id;
                    var skillNode = new SidebarNodeViewModel
                    {
                        Kind = SidebarNodeKind.Skill,
                        Id = id,
                        Label = label,
                        Description = description,
                        FilePath = skillFile
                    };
                    _skillNodes.Add(skillNode);
                    pluginNode.Children.Add(skillNode);
                }
            }

            Nodes.Add(pluginNode);
        }
    }

    // Groups tool children under sub-nodes when their labels contain a dot (second-level namespace).
    private void AddToolChildren(SidebarNodeViewModel parent, List<ToolDefinition> tools, string prefix)
    {
        var labeledTools = tools
            .Select(t => (tool: t, label: GetLabel(t.Function.Name, prefix)))
            .OrderBy(x => x.label)
            .ToList();

        // Check if any label has a sub-prefix (contains a dot)
        var grouped = labeledTools
            .GroupBy(x =>
            {
                var dot = x.label.IndexOf('.');
                return dot > 0 ? x.label[..dot] : "";
            })
            .OrderBy(g => g.Key)
            .ToList();

        bool hasSubGroups = grouped.Any(g => !string.IsNullOrEmpty(g.Key));

        if (!hasSubGroups)
        {
            // Flat list — no sub-prefixes
            foreach (var (tool, label) in labeledTools)
            {
                var node = MakeToolNode(tool, label);
                _toolNodes[tool.Function.Name] = node;
                parent.Children.Add(node);
            }
            return;
        }

        foreach (var group in grouped)
        {
            if (string.IsNullOrEmpty(group.Key))
            {
                // Tools without a sub-prefix land directly on the parent
                foreach (var (tool, label) in group)
                {
                    var node = MakeToolNode(tool, label);
                    _toolNodes[tool.Function.Name] = node;
                    parent.Children.Add(node);
                }
            }
            else
            {
                // Create a sub-node for this prefix group
                var subNode = new SidebarNodeViewModel
                {
                    Kind = SidebarNodeKind.Plugin,
                    Id = $"{prefix}.{group.Key}",
                    Label = group.Key,
                };
                foreach (var (tool, fullLabel) in group)
                {
                    var leafLabel = GetLabel(tool.Function.Name, $"{prefix}.{group.Key}");
                    var node = MakeToolNode(tool, leafLabel);
                    _toolNodes[tool.Function.Name] = node;
                    subNode.Children.Add(node);
                }
                parent.Children.Add(subNode);
            }
        }
    }

    private static string GetLabel(string toolName, string prefix)
    {
        var stripped = prefix + ".";
        return toolName.StartsWith(stripped, StringComparison.OrdinalIgnoreCase) && toolName.Length > stripped.Length
            ? toolName[stripped.Length..]
            : toolName;
    }

    private static SidebarNodeViewModel MakeToolNode(ToolDefinition tool, string label) =>
        new()
        {
            Kind = SidebarNodeKind.Tool,
            Id = tool.Function.Name,
            Label = label,
            Description = tool.Function.Description
        };

    public bool IsToolEnabled(string toolName)
        => !_toolNodes.TryGetValue(toolName, out var node) || node.IsEnabled;

    public IEnumerable<(string id, string content)> GetEnabledSkillContents()
    {
        foreach (var skill in _skillNodes.Where(s => s.IsEnabled && s.FilePath != null))
        {
            string raw;
            try { raw = File.ReadAllText(skill.FilePath!); }
            catch { continue; }
            yield return (skill.Id, StripFrontmatter(raw));
        }
    }

    private static (string id, string description) ParseSkillFrontmatter(string filePath)
    {
        var id = Path.GetFileNameWithoutExtension(filePath);
        var description = "";
        try
        {
            var text = File.ReadAllText(filePath);
            if (!text.StartsWith("---", StringComparison.Ordinal)) return (id, description);
            var end = text.IndexOf("---", 3, StringComparison.Ordinal);
            if (end < 0) return (id, description);

            foreach (var line in text[3..end].Split('\n'))
            {
                var t = line.Trim();
                if (t.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
                    id = t[3..].Trim();
                else if (t.StartsWith("description:", StringComparison.OrdinalIgnoreCase))
                    description = t[12..].Trim();
            }
        }
        catch { }
        return (id, description);
    }

    private static string StripFrontmatter(string content)
    {
        if (!content.StartsWith("---", StringComparison.Ordinal)) return content;
        var end = content.IndexOf("---", 3, StringComparison.Ordinal);
        return end < 0 ? content : content[(end + 3)..].TrimStart('\n', '\r');
    }
}
