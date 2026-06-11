namespace SPLA.UI.Avalonia.Services.Plugins;

using System.Threading.Tasks;

public interface IPluginPanelHostService
{
    Task<bool> OpenPanelAsync(string panelId, object? payload = null);
}
