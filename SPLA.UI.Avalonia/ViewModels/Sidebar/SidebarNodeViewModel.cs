using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SPLA.UI.Avalonia.ViewModels.Sidebar;

public enum SidebarNodeKind { Plugin, Tool, Skill }

public partial class SidebarNodeViewModel : ObservableObject
{
    public SidebarNodeKind Kind { get; init; }
    public required string Id { get; init; }
    public required string Label { get; init; }
    public string Description { get; init; } = "";
    public string? FilePath { get; init; }

    [ObservableProperty]
    private bool _isEnabled = true;

    public ObservableCollection<SidebarNodeViewModel> Children { get; } = [];

    public bool HasChildren => Children.Count > 0;
    public bool IsPlugin    => Kind == SidebarNodeKind.Plugin;
    public bool IsLeaf      => Kind != SidebarNodeKind.Plugin;

    public string KindIcon => Kind switch
    {
        SidebarNodeKind.Skill => "✨",
        SidebarNodeKind.Tool  => "🔨",
        _                     => ""
    };
}
