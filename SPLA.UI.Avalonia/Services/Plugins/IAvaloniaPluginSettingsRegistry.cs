using SPLA.Plugins.Host.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SPLA.UI.Avalonia.Services.Plugins;

public interface IAvaloniaPluginSettingsRegistry
{
    IReadOnlyList<AvaloniaPluginSettingsDescriptor> Descriptors { get; }
    void Register(AvaloniaPluginSettingsDescriptor descriptor);
    AvaloniaPluginSettingsDescriptor? FindByPluginId(string pluginId);
}

public sealed class AvaloniaPluginSettingsRegistry : IAvaloniaPluginSettingsRegistry
{
    private readonly List<AvaloniaPluginSettingsDescriptor> _descriptors = [];

    public IReadOnlyList<AvaloniaPluginSettingsDescriptor> Descriptors => _descriptors;

    public void Register(AvaloniaPluginSettingsDescriptor descriptor)
    {
        if (_descriptors.Any(d => string.Equals(d.PluginId, descriptor.PluginId, StringComparison.OrdinalIgnoreCase)))
            return;
        _descriptors.Add(descriptor);
    }

    public AvaloniaPluginSettingsDescriptor? FindByPluginId(string pluginId) =>
        _descriptors.FirstOrDefault(d => string.Equals(d.PluginId, pluginId, StringComparison.OrdinalIgnoreCase));
}
