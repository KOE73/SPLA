using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.MCP.Core.Plugins;
using System.Collections.ObjectModel;
using System.Linq;

namespace SPLA.UI.Avalonia.ViewModels.Sidebar;

public enum SidebarNodeKind { Plugin, SkillPlugin, Tool, Skill }

public partial class SidebarNodeViewModel : ObservableObject
{
    public SidebarNodeKind Kind { get; init; }
    public required string Id { get; init; }
    public required string Label { get; init; }
    public string Description { get; init; } = "";

    // For skills: reference to the domain object so toggles write through
    public CapabilityItem? Source { get; init; }

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private bool _isPreloaded = false;

    [ObservableProperty]
    private bool _isExpanded = true;

    partial void OnIsEnabledChanged(bool value)
    {
        // Group toggle on a skills-type plugin: cascade to all child skills
        if (Kind == SidebarNodeKind.SkillPlugin)
        {
            foreach (var child in Children)
            {
                child.IsEnabled = value;
                // Write through to domain object so GetEnabled() reflects the change immediately
                if (child.Source?.SourceSkill != null)
                    child.Source.SourceSkill.IsEnabled = value;
            }
        }
    }

    public ObservableCollection<SidebarNodeViewModel> Children { get; } = [];

    public bool HasChildren    => Children.Count > 0;
    public bool IsPlugin       => Kind is SidebarNodeKind.Plugin or SidebarNodeKind.SkillPlugin;
    public bool IsLeaf         => Kind is SidebarNodeKind.Tool or SidebarNodeKind.Skill;
    public bool IsSkill        => Kind == SidebarNodeKind.Skill;
    public bool IsSkillPlugin  => Kind == SidebarNodeKind.SkillPlugin;

    public string KindIcon => Kind switch
    {
        SidebarNodeKind.Skill       => "✨",
        SidebarNodeKind.Tool        => "🔨",
        _                           => ""
    };

    public string PluginKindIcon => IsSkillPlugin ? "✨" : "";
}
