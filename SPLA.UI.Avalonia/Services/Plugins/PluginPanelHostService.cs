using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using SPLA.Plugins.Host.Avalonia;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.Services.Plugins;

public sealed class PluginPanelHostService(
    IAvaloniaPluginPanelRegistry panels,
    IPluginInteractionService interaction,
    ILogger<PluginPanelHostService> logger) : IPluginPanelHostService
{
    public Task<bool> OpenPanelAsync(string panelId, object? payload = null) =>
        Dispatcher.UIThread.InvokeAsync(() => OpenPanelOnUiThread(panelId, payload)).GetTask();

    private bool OpenPanelOnUiThread(string panelId, object? payload)
    {
        var descriptor = panels.Find(panelId);
        if (descriptor is null)
        {
            logger.LogWarning("Plugin panel not found. Panel={PanelId}", panelId);
            return false;
        }

        var context = new AvaloniaPluginPanelContext
        {
            WorkspacePath = App.ResolvedSettings.WorkspacePath,
            Interaction = interaction,
            Payload = payload
        };

        logger.LogInformation("Opening plugin panel. Panel={PanelId}", panelId);
        var window = new PluginPanelHostWindow(descriptor, context);
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is { } owner)
        {
            window.Show(owner);
        }
        else
        {
            window.Show();
        }

        return true;
    }
}
