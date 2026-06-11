using SPLA.Plugins.Host.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.UI.Avalonia.Services.Plugins;

public interface IAvaloniaPluginPanelRegistry
{
    IReadOnlyList<AvaloniaPluginPanelDescriptor> Panels { get; }
    void Register(AvaloniaPluginPanelDescriptor panel);
    AvaloniaPluginPanelDescriptor? Find(string panelId);
}

public sealed class AvaloniaPluginPanelRegistry : IAvaloniaPluginPanelRegistry
{
    private readonly List<AvaloniaPluginPanelDescriptor> _panels = [];

    public IReadOnlyList<AvaloniaPluginPanelDescriptor> Panels => _panels;

    public void Register(AvaloniaPluginPanelDescriptor panel)
    {
        if (_panels.Any(p => string.Equals(p.Id, panel.Id, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _panels.Add(panel);
    }

    public AvaloniaPluginPanelDescriptor? Find(string panelId) =>
        _panels.FirstOrDefault(p => string.Equals(p.Id, panelId, StringComparison.OrdinalIgnoreCase));
}
