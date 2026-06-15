using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.MCP.Core.Plugins;
using System.Collections.ObjectModel;

namespace SPLA.UI.Avalonia.ViewModels.Sidebar;

public enum SidebarNodeKind { Plugin, Tool, Skill }

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

    public ObservableCollection<SidebarNodeViewModel> Children { get; } = [];

    public bool HasChildren => Children.Count > 0;
    public bool IsPlugin    => Kind == SidebarNodeKind.Plugin;
    public bool IsLeaf      => Kind != SidebarNodeKind.Plugin;
    public bool IsSkill     => Kind == SidebarNodeKind.Skill;

    public string KindIcon => Kind switch
    {
        SidebarNodeKind.Skill => "✨",
        SidebarNodeKind.Tool  => "🔨",
        _                     => ""
    };
}
