using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using SPLA.Plugins.Host.Avalonia;
using SPLA.UI.Avalonia.Services.Plugins;

namespace SPLA.UI.Avalonia.ViewModels.Settings;

public partial class PluginSettingViewModel : ViewModelBase
{
    /// <summary>Optional settings editor contributed by an avalonia-ui plugin for this plugin id.</summary>
    public IPluginSettingsEditor? Editor { get; private set; }

    /// <summary>Backing store handed to the editor; persisted into .spla on save.</summary>
    public PluginSettingsStore? SettingsStore { get; private set; }

    public bool HasEditor => Editor is not null;

    /// <summary>The editor's control, for hosting in the settings view.</summary>
    public object? EditorView => Editor?.View;

    public void AttachEditor(IPluginSettingsEditor editor, PluginSettingsStore store)
    {
        Editor = editor;
        SettingsStore = store;
        OnPropertyChanged(nameof(HasEditor));
        OnPropertyChanged(nameof(EditorView));
    }

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
