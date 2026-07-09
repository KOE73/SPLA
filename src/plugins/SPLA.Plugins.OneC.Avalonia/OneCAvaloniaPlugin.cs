using Avalonia.Controls;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Host.Avalonia;
using SPLA.Plugins.OneC.Avalonia.Views.OneC;

namespace SPLA.Plugins.OneC.Avalonia;

public sealed class OneCAvaloniaPlugin : IAvaloniaPlugin, IAvaloniaPluginToolProvider
{
    public IEnumerable<AvaloniaPluginPanelDescriptor> GetPanels(AvaloniaPluginContext context)
    {
        yield return new AvaloniaPluginPanelDescriptor
        {
            Id = "onec.overview",
            DisplayName = "1C Configuration Browser",
            Description = "Browse the 1C index, objects, and dependencies.",
            Metadata = new Dictionary<string, string>
            {
                ["icon"] = "database",
                ["group"] = "1C"
            },
            CreateContent = CreateOverviewPanel
        };
    }

    private static Control CreateOverviewPanel(AvaloniaPluginPanelContext context) => new OneCOverviewPanel(context);

    public IEnumerable<IMcpTool> GetTools(AvaloniaPluginContext context)
    {
        yield return new OneCGraphOpenTool(context.WorkspacePath);
    }
}
