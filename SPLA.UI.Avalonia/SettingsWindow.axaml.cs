using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SPLA.UI.Avalonia;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
