using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Editor;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

namespace SPLA.MCP.Core.Plugins;

/// <summary>Everything a single DLL plugin contributes once loaded: its tools, any UI commands it
/// generates at runtime, an optional action handler, and any schema providers. Merged into the
/// runtime registry by the <see cref="PluginManager"/> facade.</summary>
internal sealed record LoadedPlugin(
    IReadOnlyList<IMcpTool> Tools,
    IReadOnlyList<SplaPluginUiCommand> GeneratedCommands,
    ISplaPluginAction? ActionHandler,
    IReadOnlyList<ISplaPluginPanelProvider> PanelProviders,
    IReadOnlyList<IJsonSchemaProvider> SchemaProviders,
    PluginHealth Health);

/// <summary>
/// The assembly-loading half of the plugin system: instantiates a DLL plugin inside its own
/// isolated <see cref="PluginLoadContext"/> and collects what it exposes. Owns the load-context
/// lifetimes; holds no runtime-registry state of its own, so loading is separable from the
/// discovery (<see cref="PluginDiscovery"/>) and registry (<see cref="PluginManager"/>) concerns.
/// </summary>
internal sealed class PluginAssemblyLoader
{
    private readonly ResolvedSettings _settings;
    private readonly ILogger? _logger;
    private readonly List<AssemblyLoadContext> _loadContexts = [];

    public PluginAssemblyLoader(ResolvedSettings settings, ILogger? logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>Loads the plugin's entry assembly and returns everything it contributes. Throws on a
    /// genuine load failure (missing entry point, bad assembly); returns an empty tool list when the
    /// DLL contains no <see cref="ISplaPlugin"/> — the caller treats that as a load error.</summary>
    public LoadedPlugin Load(PluginDescriptor descriptor)
    {
        var meta = descriptor.Meta;
        var dllPath = Path.Combine(descriptor.DirectoryPath, meta.EntryPoint);
        if (!File.Exists(dllPath))
            throw new FileNotFoundException($"Plugin '{meta.Id}' entry point was not found: {dllPath}");

        _logger?.LogInformation("Loading plugin. Plugin={PluginId} EntryPoint={EntryPoint}", meta.Id, dllPath);

        var loadContext = new PluginLoadContext(dllPath);
        _loadContexts.Add(loadContext);
        var assembly = loadContext.LoadFromAssemblyPath(dllPath);

        var tools = new List<IMcpTool>();
        var generatedCommands = new List<SplaPluginUiCommand>();
        ISplaPluginAction? actionHandler = null;
        var panelProviders = new List<ISplaPluginPanelProvider>();
        var schemaProviders = new List<IJsonSchemaProvider>();
        var health = PluginHealth.Ok;

        var pluginTypes = assembly.GetTypes()
            .Where(t => typeof(ISplaPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var pluginType in pluginTypes)
        {
            if (Activator.CreateInstance(pluginType) is not ISplaPlugin plugin) continue;

            foreach (var tool in plugin.Initialize(_settings))
            {
                tools.Add(tool);
                _logger?.LogInformation("Plugin tool loaded. Plugin={PluginId} Tool={ToolName}", meta.Id, tool.Name);
            }

            if (plugin is ISplaPluginUiCommands uiPlugin)
                generatedCommands.AddRange(uiPlugin.GetUiCommands());

            if (plugin is ISplaPluginAction handler)
                actionHandler = handler;

            if (plugin is ISplaPluginPanelProvider panelProvider)
                panelProviders.Add(panelProvider);

            if (plugin is ISchemaContributor schemaContributor)
                schemaProviders.Add(schemaContributor.GetSchemaProvider());

            // Optional load-time self-check. Host-guarded so a plugin that ignores the "don't throw"
            // contract can't crash the load — a thrown exception becomes Degraded. First non-OK wins.
            if (health.Status == PluginHealthStatus.Ok && plugin is ISplaPluginSelfCheck selfCheck)
                health = RunSelfCheck(meta.Id, selfCheck);
        }

        return new LoadedPlugin(tools, generatedCommands, actionHandler, panelProviders, schemaProviders, health);
    }

    private PluginHealth RunSelfCheck(string pluginId, ISplaPluginSelfCheck selfCheck)
    {
        try
        {
            var health = selfCheck.CheckHealth() ?? PluginHealth.Ok;
            if (health.Status != PluginHealthStatus.Ok)
                _logger?.LogWarning("Plugin self-check degraded. Plugin={PluginId} Reason={Reason}", pluginId, health.Message);
            else
                _logger?.LogInformation("Plugin self-check ok. Plugin={PluginId}", pluginId);
            return health;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Plugin self-check threw (treated as degraded). Plugin={PluginId}", pluginId);
            return PluginHealth.Degraded($"Self-check threw {ex.GetType().Name}: {ex.Message}");
        }
    }
}
