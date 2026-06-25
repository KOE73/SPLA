using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.MCP.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SPLA.UI.Avalonia.ViewModels.Sidebar;

public partial class SidebarPanelViewModel : ObservableObject
{
    // Full unfiltered tree — source of truth
    private readonly ObservableCollection<SidebarNodeViewModel> _nodes = [];

    // Filtered tree — bound by the TreeView
    public ObservableCollection<SidebarNodeViewModel> FilteredNodes { get; } = [];

    private readonly Dictionary<string, SidebarNodeViewModel> _toolNodes   = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SidebarNodeViewModel> _skillNodes  = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    private string _filterText = "";

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    // ── Build ────────────────────────────────────────────────────────────────

    public void Build(IReadOnlyList<CapabilityItem> items)
    {
        _nodes.Clear();
        _toolNodes.Clear();
        _skillNodes.Clear();

        var pluginIds = new HashSet<string>(
            items.Where(i => i.Kind == CapabilityKind.Plugin).Select(i => i.Id),
            StringComparer.OrdinalIgnoreCase);

        // Plugin nodes (items that belong to a known plugin)
        var byPlugin = items
            .Where(i => i.Kind != CapabilityKind.Plugin && i.PluginId != null && pluginIds.Contains(i.PluginId))
            .GroupBy(i => i.PluginId!, StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in items.Where(i => i.Kind == CapabilityKind.Plugin).OrderBy(i => i.Id))
        {
            var isSkillPack = plugin.SourcePlugin?.Meta.Type.Equals("skills", System.StringComparison.OrdinalIgnoreCase) == true;
            var pluginNode = new SidebarNodeViewModel
            {
                Kind = isSkillPack ? SidebarNodeKind.SkillPlugin : SidebarNodeKind.Plugin,
                Id = plugin.Id,
                Label = plugin.Id,
                IsEnabled = plugin.IsEnabled
            };

            var children = byPlugin.FirstOrDefault(g => g.Key.Equals(plugin.Id, StringComparison.OrdinalIgnoreCase));
            if (children != null)
                PopulateChildren(pluginNode, children.ToList(), plugin.Id);

            _nodes.Add(pluginNode);
        }

        // Built-in node — all tools/skills without a plugin parent
        var builtins = items
            .Where(i => i.Kind != CapabilityKind.Plugin && (i.PluginId == null || !pluginIds.Contains(i.PluginId)))
            .ToList();

        if (builtins.Count > 0)
        {
            var builtinNode = new SidebarNodeViewModel
            {
                Kind = SidebarNodeKind.Plugin,
                Id = "__builtin__",
                Label = "built-in",
                IsExpanded = false
            };
            PopulateChildren(builtinNode, builtins, prefix: null);
            _nodes.Add(builtinNode);
        }

        ApplyFilter();
    }

    private void PopulateChildren(SidebarNodeViewModel parent, List<CapabilityItem> items, string? prefix)
    {
        // Skills go directly under plugin node (no sub-grouping)
        foreach (var skill in items.Where(i => i.Kind == CapabilityKind.Skill).OrderBy(i => i.Label))
        {
            var label = prefix != null && skill.Id.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase)
                ? skill.Id[(prefix.Length + 1)..]
                : skill.Id;

            var node = new SidebarNodeViewModel
            {
                Kind = SidebarNodeKind.Skill,
                Id = skill.Id,
                Label = label,
                Description = skill.Description,
                IsEnabled = skill.IsEnabled,
                IsPreloaded = skill.IsPreloaded,
                Source = skill
            };
            _skillNodes[skill.Id] = node;
            parent.Children.Add(node);
        }

        // Tools: group by second-level prefix
        var tools = items.Where(i => i.Kind == CapabilityKind.Tool).OrderBy(i => i.Id).ToList();
        if (tools.Count == 0) return;

        // Determine the effective prefix for label stripping
        var labelPrefix = prefix ?? string.Empty;

        var groups = tools
            .Select(t => (item: t, label: StripPrefix(t.Id, labelPrefix)))
            .GroupBy(x => { var dot = x.label.IndexOf('.'); return dot > 0 ? x.label[..dot] : ""; })
            .OrderBy(g => g.Key)
            .ToList();

        bool hasSubGroups = groups.Any(g => !string.IsNullOrEmpty(g.Key));

        if (!hasSubGroups)
        {
            foreach (var (item, label) in groups.SelectMany(g => g))
            {
                var node = MakeToolNode(item, label);
                _toolNodes[item.Id] = node;
                parent.Children.Add(node);
            }
            return;
        }

        foreach (var group in groups)
        {
            if (string.IsNullOrEmpty(group.Key))
            {
                foreach (var (item, label) in group)
                {
                    var node = MakeToolNode(item, label);
                    _toolNodes[item.Id] = node;
                    parent.Children.Add(node);
                }
            }
            else
            {
                var subId = string.IsNullOrEmpty(labelPrefix) ? group.Key : $"{labelPrefix}.{group.Key}";
                var subNode = new SidebarNodeViewModel
                {
                    Kind = SidebarNodeKind.Plugin,
                    Id = subId,
                    Label = group.Key
                };
                foreach (var (item, fullLabel) in group)
                {
                    var leafLabel = StripPrefix(item.Id, subId);
                    var node = MakeToolNode(item, leafLabel);
                    _toolNodes[item.Id] = node;
                    subNode.Children.Add(node);
                }
                parent.Children.Add(subNode);
            }
        }
    }

    private static string StripPrefix(string id, string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return id;
        var stripped = prefix + ".";
        return id.StartsWith(stripped, StringComparison.OrdinalIgnoreCase) && id.Length > stripped.Length
            ? id[stripped.Length..]
            : id;
    }

    private static SidebarNodeViewModel MakeToolNode(CapabilityItem item, string label) => new()
    {
        Kind = SidebarNodeKind.Tool,
        Id = item.Id,
        Label = label,
        Description = item.Description
    };

    // ── Filter ───────────────────────────────────────────────────────────────

    private void ApplyFilter()
    {
        FilteredNodes.Clear();

        if (string.IsNullOrWhiteSpace(FilterText))
        {
            foreach (var n in _nodes) FilteredNodes.Add(n);
            return;
        }

        foreach (var node in _nodes)
        {
            var filtered = FilterNode(node, FilterText);
            if (filtered != null) FilteredNodes.Add(filtered);
        }
    }

    private static SidebarNodeViewModel? FilterNode(SidebarNodeViewModel node, string filter)
    {
        bool selfMatch = node.Label.Contains(filter, StringComparison.OrdinalIgnoreCase);

        if (!node.HasChildren)
            return selfMatch ? node : null;

        var matchingChildren = node.Children
            .Select(c => FilterNode(c, filter))
            .OfType<SidebarNodeViewModel>()
            .ToList();

        if (!selfMatch && matchingChildren.Count == 0)
            return null;

        // Build a view node with filtered children
        var viewNode = new SidebarNodeViewModel
        {
            Kind = node.Kind,
            Id = node.Id,
            Label = node.Label,
            Description = node.Description,
            IsEnabled = node.IsEnabled,
            IsPreloaded = node.IsPreloaded,
            IsExpanded = true, // auto-expand matched parents
            Source = node.Source
        };

        var childSource = selfMatch ? node.Children.AsEnumerable() : matchingChildren;
        foreach (var child in childSource)
            viewNode.Children.Add(child);

        return viewNode;
    }

    // ── Expand / Collapse ────────────────────────────────────────────────────

    [RelayCommand]
    private void ExpandAll() => SetExpanded(_nodes, true);

    [RelayCommand]
    private void CollapseAll() => SetExpanded(_nodes, false);

    private static void SetExpanded(IEnumerable<SidebarNodeViewModel> nodes, bool expanded)
    {
        foreach (var node in nodes)
        {
            node.IsExpanded = expanded;
            SetExpanded(node.Children, expanded);
        }
    }

    // ── State accessors ──────────────────────────────────────────────────────

    public bool IsToolEnabled(string toolName)
        => !_toolNodes.TryGetValue(toolName, out var node) || node.IsEnabled;

    public void SetSkillEnabled(string id, bool enabled)
    {
        if (_skillNodes.TryGetValue(id, out var node))
        {
            node.IsEnabled = enabled;
            if (node.Source != null) node.Source.IsEnabled = enabled;
        }
    }

    public void SetSkillPreloaded(string id, bool preloaded)
    {
        if (_skillNodes.TryGetValue(id, out var node))
        {
            node.IsPreloaded = preloaded;
            if (node.Source != null) node.Source.IsPreloaded = preloaded;
        }
    }
}
