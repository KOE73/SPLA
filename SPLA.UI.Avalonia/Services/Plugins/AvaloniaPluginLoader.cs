using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Plugins;
using SPLA.Plugins.Host.Avalonia;

namespace SPLA.UI.Avalonia.Services.Plugins;

public sealed class AvaloniaPluginLoader(
    IAvaloniaPluginPanelRegistry panelRegistry,
    IAvaloniaPluginSettingsRegistry settingsRegistry,
    IPluginInteractionService interaction,
    ILogger<AvaloniaPluginLoader> logger) : IAvaloniaPluginLoader
{
    private readonly List<AssemblyLoadContext> _loadContexts = [];
    private readonly List<IMcpTool> _loadedTools = [];

    public IReadOnlyList<IMcpTool> LoadedTools => _loadedTools;

    public void LoadPanels(IEnumerable<PluginDescriptor> plugins)
    {
        foreach (var plugin in plugins.Where(IsLoadableAvaloniaPlugin))
        {
            var entryPath = Path.Combine(plugin.DirectoryPath, plugin.Meta.EntryPoint);
            if (!File.Exists(entryPath))
            {
                logger.LogWarning("Avalonia plugin entry point missing. Plugin={PluginId} EntryPoint={EntryPoint}", plugin.Meta.Id, entryPath);
                continue;
            }

            try
            {
                logger.LogInformation("Loading Avalonia plugin. Plugin={PluginId} EntryPoint={EntryPoint}", plugin.Meta.Id, entryPath);
                var loadContext = new AvaloniaPluginLoadContext(entryPath);
                _loadContexts.Add(loadContext);
                var assembly = loadContext.LoadFromAssemblyPath(entryPath);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IAvaloniaPlugin).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                foreach (var pluginType in pluginTypes)
                {
                    if (Activator.CreateInstance(pluginType) is not IAvaloniaPlugin avaloniaPlugin)
                    {
                        continue;
                    }

                    var context = new AvaloniaPluginContext
                    {
                        WorkspacePath = App.ResolvedSettings.WorkspacePath,
                        Interaction = interaction
                    };

                    foreach (var panel in avaloniaPlugin.GetPanels(context))
                    {
                        panelRegistry.Register(panel);
                        logger.LogInformation("Avalonia plugin panel registered. Plugin={PluginId} Panel={PanelId}", plugin.Meta.Id, panel.Id);
                    }

                    if (avaloniaPlugin is IAvaloniaPluginSettingsProvider settingsProvider)
                    {
                        var settingsContext = new AvaloniaPluginSettingsContext
                        {
                            WorkspacePath = App.ResolvedSettings.WorkspacePath
                        };
                        var descriptor = settingsProvider.GetSettings(settingsContext);
                        settingsRegistry.Register(descriptor);
                        logger.LogInformation("Avalonia plugin settings editor registered. Plugin={PluginId}", descriptor.PluginId);
                    }

                    if (avaloniaPlugin is IAvaloniaPluginToolProvider toolProvider)
                    {
                        foreach (var tool in toolProvider.GetTools(context))
                        {
                            if (_loadedTools.Any(existing => string.Equals(existing.Name, tool.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                continue;
                            }

                            _loadedTools.Add(tool);
                            logger.LogInformation("Avalonia plugin tool registered. Plugin={PluginId} Tool={ToolName}", plugin.Meta.Id, tool.Name);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                logger.LogError(ex, "Avalonia plugin type loading failed. Plugin={PluginId} EntryPoint={EntryPoint} LoaderExceptions={LoaderExceptions}", plugin.Meta.Id, entryPath, string.Join(Environment.NewLine, ex.LoaderExceptions.Select(e => e?.ToString())));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Avalonia plugin loading failed. Plugin={PluginId} EntryPoint={EntryPoint}", plugin.Meta.Id, entryPath);
            }
        }
    }

    private static bool IsLoadableAvaloniaPlugin(PluginDescriptor plugin) =>
        plugin.IsEffectivelyEnabled
        && string.Equals(plugin.Meta.Type, "avalonia-ui", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(plugin.Meta.EntryPoint);
}

internal sealed class AvaloniaPluginLoadContext(string pluginPath) : AssemblyLoadContext(isCollectible: false)
{
    private static readonly HashSet<string> SharedAssemblies = new(StringComparer.OrdinalIgnoreCase)
    {
        "Avalonia.Base",
        "Avalonia.Controls",
        "Avalonia.Desktop",
        "Avalonia.Markup.Xaml",
        "SPLA.Domain",
        "SPLA.MCP.Core",
        "SPLA.Observability",
        "SPLA.Plugins.Host.Avalonia"
    };

    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is not null && SharedAssemblies.Contains(assemblyName.Name))
        {
            return null;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath is not null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    // Resolve native sub-dependencies (e.g. Microsoft.Data.SqlClient.SNI.dll) from the plugin's
    // own runtimes/<rid>/native folder via its .deps.json. Without this the default P/Invoke search
    // looks next to the host exe (which has no such natives) and fails with "Dll was not found".
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath is not null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}
