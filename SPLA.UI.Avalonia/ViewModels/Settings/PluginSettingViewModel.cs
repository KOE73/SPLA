using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SPLA.UI.Avalonia.ViewModels.Settings;

public partial class PluginSettingViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _id = "";

    [ObservableProperty]
    private string _type = "";

    [ObservableProperty]
    private string _effectiveState = "";

    [ObservableProperty]
    private string _effectiveStateReason = "";

    [ObservableProperty]
    private string _dependsOn = "";

    [ObservableProperty]
    private bool _enabled = true;

    [ObservableProperty]
    private string _customPrompt = "";

    public ObservableCollection<PluginSettingViewModel> Children { get; } = new();

    public bool HasChildren => Children.Count > 0;

    public string EffectiveStateDisplay => string.IsNullOrWhiteSpace(EffectiveStateReason)
        ? EffectiveState
        : $"{EffectiveState}: {EffectiveStateReason}";

    public void AddChild(PluginSettingViewModel child)
    {
        Children.Add(child);
        OnPropertyChanged(nameof(HasChildren));
    }
}
