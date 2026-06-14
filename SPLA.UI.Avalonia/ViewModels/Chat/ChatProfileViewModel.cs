using CommunityToolkit.Mvvm.ComponentModel;
using SPLA.Domain.Settings;

namespace SPLA.UI.Avalonia.ViewModels.Chat;

/// <summary>
/// UI wrapper around ChatDisplayProfile that carries selection state for the profile picker buttons.
/// </summary>
public partial class ChatProfileViewModel(ChatDisplayProfile profile) : ObservableObject
{
    public ChatDisplayProfile Profile { get; } = profile;

    public string Id => Profile.Id;
    public string DisplayName => Profile.DisplayName;

    [ObservableProperty]
    private bool _isSelected;
}
