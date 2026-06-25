using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Agent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SPLA.UI.Avalonia.ViewModels.Debug;

/// <summary>One coloured block in the system-prompt preview — a single <see cref="PromptSegment"/>.</summary>
public sealed class PromptSegmentViewModel
{
    public PromptSegmentViewModel(PromptSegment segment)
    {
        Title       = segment.Title;
        Body        = segment.Body;
        AccentColor = ColorFor(segment.Kind);
        CharLabel   = $"{segment.Body.Length} ch";
    }

    public string Title { get; }
    public string Body { get; }
    public string AccentColor { get; }
    public string CharLabel { get; }

    // Colour by origin so each layer is visually distinct at a glance.
    private static string ColorFor(PromptSegmentKind kind) => kind switch
    {
        PromptSegmentKind.Mode           => "#6B7280",
        PromptSegmentKind.Core           => "#2563EB",
        PromptSegmentKind.Instructions   => "#0891B2",
        PromptSegmentKind.Custom         => "#D97706",
        PromptSegmentKind.ActiveSkill    => "#DC2626",
        PromptSegmentKind.Skill          => "#7C3AED",
        PromptSegmentKind.SkillsIndex    => "#9333EA",
        PromptSegmentKind.Plugin         => "#059669",
        PromptSegmentKind.PluginCommands => "#65A30D",
        _                                => "#6B7280"
    };
}

/// <summary>Live preview of the active chat's assembled system prompt, broken into source-tagged blocks.
/// Pulls fresh segments from the active chat on demand — so it always reflects what would be sent now.</summary>
public partial class PromptDebugWindowViewModel : ObservableObject
{
    private readonly Func<IReadOnlyList<PromptSegment>?> _provider;

    public ObservableCollection<PromptSegmentViewModel> Segments { get; } = new();

    [ObservableProperty] private string _summary = string.Empty;

    public PromptDebugWindowViewModel(Func<IReadOnlyList<PromptSegment>?> provider)
    {
        _provider = provider;
        Refresh();
    }

    [RelayCommand]
    public void Refresh()
    {
        Segments.Clear();
        var segments = _provider();
        var totalChars = 0;
        if (segments != null)
        {
            foreach (var seg in segments)
            {
                Segments.Add(new PromptSegmentViewModel(seg));
                totalChars += seg.Text.Length;
            }
        }
        Summary = $"{Segments.Count} blocks · {totalChars} chars";
    }
}
