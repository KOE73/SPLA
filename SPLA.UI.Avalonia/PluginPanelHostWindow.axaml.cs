using Avalonia.Controls;
using SPLA.Plugins.Host.Avalonia;
using System;

namespace SPLA.UI.Avalonia;

public partial class PluginPanelHostWindow : Window
{
    public PluginPanelHostWindow()
    {
        InitializeComponent();
    }

    public PluginPanelHostWindow(AvaloniaPluginPanelDescriptor descriptor, AvaloniaPluginPanelContext context)
        : this()
    {
        Title = descriptor.DisplayName;

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
}
