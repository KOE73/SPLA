using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SPLA.UI.Avalonia.ViewModels.Settings;

namespace SPLA.UI.Avalonia;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        // Subscribe once the DataContext is ready.
        Opened += OnOpened;
        Closed += OnClosed;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;

        // Set initial value (e.g. the window re-opens on the plugin editor page).
        PluginEditorHost.Child = vm.CurrentPluginEditorView;

        vm.PropertyChanged += OnVmPropertyChanged;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;
        vm.PropertyChanged -= OnVmPropertyChanged;
        // Release the child so the Control's visual parent is cleared before the window is GCed.
        PluginEditorHost.Child = null;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.CurrentPluginEditorView)
            && sender is SettingsViewModel vm)
        {
            // Border.Child setter detaches the old child before attaching the new one —
            // this is the correct way to swap Avalonia controls without the
            // "already has a visual parent" crash that ContentControl causes.
            PluginEditorHost.Child = vm.CurrentPluginEditorView;
        }
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
