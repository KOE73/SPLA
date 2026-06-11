using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace SPLA.UI.Avalonia.Views.Status;

public partial class StatusView : UserControl
{
    public event EventHandler? SettingsRequested;

    public StatusView()
    {
        InitializeComponent();
    }

    private void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }
}
