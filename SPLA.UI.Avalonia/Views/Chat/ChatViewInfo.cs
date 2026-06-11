using CommunityToolkit.Mvvm.ComponentModel;

namespace SPLA.UI.Avalonia.Views.Chat;

public partial class ChatViewInfo : ObservableObject
{
    public string Id { get; }
    public string DisplayName { get; }

    [ObservableProperty]
    private bool _isSelected;

    public ChatViewInfo(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }
}
