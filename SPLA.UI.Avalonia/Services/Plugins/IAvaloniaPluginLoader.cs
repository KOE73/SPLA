using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Plugins;
using System.Collections.Generic;

namespace SPLA.UI.Avalonia.Services.Plugins;

public interface IAvaloniaPluginLoader
{
    void LoadPanels(IEnumerable<PluginDescriptor> plugins);
    IReadOnlyList<IMcpTool> LoadedTools { get; }
}
