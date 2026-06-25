using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SPLA.Plugins.Host.Avalonia;
using System;

namespace SPLA.UI.Avalonia;

public partial class PluginPanelHostWindow : Window
{
    public PluginPanelHostWindow()
    {
        InitializeComponent();
        ExtendClientAreaTitleBarHeightHint = 30;
    }

    public PluginPanelHostWindow(AvaloniaPluginPanelDescriptor descriptor, AvaloniaPluginPanelContext context)
        : this()
    {
        Title = descriptor.DisplayName;
        TitleBarText.Text = descriptor.DisplayName;

        try
        {
            PanelContent.Content = descriptor.CreateContent(context);
        }
        catch (Exception ex)
        {
            PanelContent.Content = new TextBlock
            {
                Text = $"Plugin panel failed to load: {ex.Message}",
                TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
            };
        }
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e) => BeginMoveDrag(e);
    private void MinimizeButton_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseButton_Click(object? sender, RoutedEventArgs e) => Close();
}
