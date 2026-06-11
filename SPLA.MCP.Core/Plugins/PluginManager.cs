using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.MCP.Core.Plugins;

public class PluginInstance
{
    public PluginMeta Meta { get; }
    public string EffectivePrompt { get; }

    public PluginInstance(PluginMeta meta, string effectivePrompt)
    {
        Meta = meta;
        EffectivePrompt = effectivePrompt;
    }
}

public class PluginManager
{
    private readonly ResolvedSettings _settings;
    private readonly ILogger<PluginManager>? _logger;
    private readonly List<PluginDescriptor> _plugins = new();
    private readonly List<PluginInstance> _activePlugins = new();
    private readonly List<SPLA.MCP.Core.Interfaces.IMcpTool> _dynamicTools = new();
    private readonly List<SPLA.MCP.Core.Interfaces.SplaPluginUiCommand> _uiCommands = new();
    private readonly List<string> _loadErrors = new();
    private readonly List<AssemblyLoadContext> _loadContexts = new();

    public PluginManager(ResolvedSettings settings, ILogger<PluginManager>? logger = null)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Scans the specified directory for plugins, loading those that are not disabled in settings.
    /// </summary>
    public void LoadPlugins(string pluginsDirectory)
    {
        _activePlugins.Clear();
        _dynamicTools.Clear();
        _uiCommands.Clear();
        _loadErrors.Clear();
        _plugins.Clear();

        _plugins.AddRange(DiscoverPlugins(_settings, pluginsDirectory, _loadErrors));
        _logger?.LogInformation("Plugin discovery finished. Directory={PluginDirectory} Count={PluginCount} Errors={ErrorCount}", pluginsDirectory, _plugins.Count, _loadErrors.Count);

        foreach (var descriptor in _plugins.Where(p => p.IsEffectivelyEnabled))
        {
            LoadEnabledPlugin(descriptor);
        }
    }

    public static IReadOnlyList<PluginDescriptor> DiscoverPlugins(
        ResolvedSettings settings,
        string pluginsDirectory,
        List<string>? loadErrors = null)
    {
        var descriptors = new List<PluginDescriptor>();
        if (!Directory.Exists(pluginsDirectory)) return descriptors;

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        foreach (var pluginDir in Directory.GetDirectories(pluginsDirectory))
        {
            var metaPath = Path.Combine(pluginDir, "meta.yaml");
            if (!File.Exists(metaPath)) continue;

            try
            {
                var yaml = File.ReadAllText(metaPath);
                var meta = deserializer.Deserialize<PluginMeta>(yaml);

                if (meta == null || string.IsNullOrWhiteSpace(meta.Id)) continue;

                settings.Plugins.TryGetValue(meta.Id, out var pluginSettings);
                var userEnabled = pluginSettings?.Enabled ?? true;
                var effectivePrompt = !string.IsNullOrWhiteSpace(pluginSettings?.CustomPrompt)
                    ? pluginSettings.CustomPrompt!
                    : meta.DefaultPrompt;

                var descriptor = new PluginDescriptor
                {
                    Meta = meta,
                    DirectoryPath = pluginDir,
                    UserEnabled = userEnabled,
                    EffectivePrompt = effectivePrompt,
                    EffectiveState = userEnabled
                        ? PluginEffectiveState.Enabled
                        : PluginEffectiveState.DisabledByUser,
                    EffectiveStateReason = userEnabled ? string.Empty : "disabled by user"
                };

                foreach (var command in meta.Commands)
                {
                    AddPluginCommand(descriptor, command);
                }

                descriptors.Add(descriptor);
            }
            catch (Exception ex)
            {
                loadErrors?.Add($"Error reading plugin manifest at {pluginDir}: {ex.Message}");
            }
        }

        ApplyDependencyStates(descriptors);
        return descriptors;
    }

    public IReadOnlyList<PluginDescriptor> GetPlugins() => _plugins;
    public IReadOnlyList<PluginInstance> GetActivePlugins() => _activePlugins;
    public IReadOnlyList<SPLA.MCP.Core.Interfaces.IMcpTool> GetDynamicTools() => _dynamicTools;
    public IReadOnlyList<SPLA.MCP.Core.Interfaces.SplaPluginUiCommand> GetUiCommands() => _uiCommands;
    public IReadOnlyList<string> GetLoadErrors() => _loadErrors;

    private static string ResolvePluginCommandTarget(string pluginDir, SPLA.MCP.Core.Interfaces.SplaPluginUiCommand command)
    {
        if (command.Kind is SPLA.MCP.Core.Interfaces.SplaPluginUiCommandKind.OpenUrl
            or SPLA.MCP.Core.Interfaces.SplaPluginUiCommandKind.OpenPanel
            or SPLA.MCP.Core.Interfaces.SplaPluginUiCommandKind.RunTool
            or SPLA.MCP.Core.Interfaces.SplaPluginUiCommandKind.CopyText
            or SPLA.MCP.Core.Interfaces.SplaPluginUiCommandKind.InsertText
            or SPLA.MCP.Core.Interfaces.SplaPluginUiCommandKind.ExecuteAction)
        {
            return command.Target;
        }

        if (string.IsNullOrWhiteSpace(command.Target) || Path.IsPathRooted(command.Target))
        {
            return command.Target;
        }

        return Path.GetFullPath(Path.Combine(pluginDir, command.Target));
    }

    private void LoadEnabledPlugin(PluginDescriptor descriptor)
    {
        try
        {
            var meta = descriptor.Meta;

            if (string.Equals(meta.Type, "dll", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(meta.EntryPoint))
            {
                var dllPath = Path.Combine(descriptor.DirectoryPath, meta.EntryPoint);
                if (File.Exists(dllPath))
                {
                    _logger?.LogInformation("Loading plugin. Plugin={PluginId} EntryPoint={EntryPoint}", meta.Id, dllPath);
                    var loadContext = new PluginLoadContext(dllPath);
                    _loadContexts.Add(loadContext);
                    var assembly = loadContext.LoadFromAssemblyPath(dllPath);
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(SPLA.MCP.Core.Interfaces.ISplaPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    var loadedTools = 0;
                    foreach (var pt in pluginTypes)
                    {
                        if (Activator.CreateInstance(pt) is SPLA.MCP.Core.Interfaces.ISplaPlugin plugin)
                        {
                            var tools = plugin.Initialize(_settings);
                            foreach (var tool in tools)
                            {
                                _dynamicTools.Add(tool);
                                _logger?.LogInformation("Plugin tool loaded. Plugin={PluginId} Tool={ToolName}", meta.Id, tool.Name);
                                loadedTools++;
                            }

                            if (plugin is SPLA.MCP.Core.Interfaces.ISplaPluginUiCommands uiPlugin)
                            {
                                foreach (var command in uiPlugin.GetUiCommands())
                                {
                                    AddPluginCommand(descriptor, command, publish: true);
                                }
                            }
                        }
                    }

                    if (loadedTools > 0)
                    {
                        _activePlugins.Add(new PluginInstance(meta, descriptor.EffectivePrompt));
                        _logger?.LogInformation("Plugin loaded. Plugin={PluginId} ToolCount={ToolCount}", meta.Id, loadedTools);
                    }
                    else
                    {
                        SetLoadError(descriptor, $"Plugin '{meta.Id}' did not expose any tools.");
                    }
                }
                else
                {
                    SetLoadError(descriptor, $"Plugin '{meta.Id}' entry point was not found: {dllPath}");
                }
            }
            else
            {
                _activePlugins.Add(new PluginInstance(meta, descriptor.EffectivePrompt));
            }

            foreach (var command in descriptor.Commands)
            {
                if (!_uiCommands.Contains(command))
                {
                    _uiCommands.Add(command);
                }
            }
        }
        catch (Exception ex)
        {
            SetLoadError(descriptor, $"Error loading plugin '{descriptor.Meta.Id}': {ex.Message}");
        }
    }

    private void AddPluginCommand(PluginDescriptor descriptor, SPLA.MCP.Core.Interfaces.SplaPluginUiCommand command, bool publish)
    {
        AddPluginCommand(descriptor, command);

        if (publish && descriptor.IsEffectivelyEnabled)
        {
            _uiCommands.Add(command);
        }
    }

    private static void AddPluginCommand(PluginDescriptor descriptor, SPLA.MCP.Core.Interfaces.SplaPluginUiCommand command)
    {
        command.PluginId = string.IsNullOrWhiteSpace(command.PluginId) ? descriptor.Meta.Id : command.PluginId;
        command.Target = ResolvePluginCommandTarget(descriptor.DirectoryPath, command);
        descriptor.Commands.Add(command);
    }

    private static void ApplyDependencyStates(List<PluginDescriptor> plugins)
    {
        var byId = plugins.ToDictionary(p => p.Meta.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in plugins.Where(p => p.UserEnabled))
        {
            foreach (var dependencyId in plugin.Meta.DependsOn.Where(d => !string.IsNullOrWhiteSpace(d)))
            {
                if (!byId.TryGetValue(dependencyId, out var dependency))
                {
                    plugin.EffectiveState = PluginEffectiveState.DisabledByMissingDependency;
                    plugin.EffectiveStateReason = $"missing dependency: {dependencyId}";
                    break;
                }

                if (!dependency.UserEnabled)
                {
                    plugin.EffectiveState = PluginEffectiveState.DisabledByDependency;
                    plugin.EffectiveStateReason = $"dependency disabled: {dependencyId}";
                    break;
                }
            }
        }

        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var plugin in plugins.Where(p => p.EffectiveState == PluginEffectiveState.Enabled))
            {
                foreach (var dependencyId in plugin.Meta.DependsOn.Where(d => !string.IsNullOrWhiteSpace(d)))
                {
                    if (byId.TryGetValue(dependencyId, out var dependency)
                        && dependency.EffectiveState != PluginEffectiveState.Enabled)
                    {
                        plugin.EffectiveState = PluginEffectiveState.DisabledByDependency;
                        plugin.EffectiveStateReason = $"dependency unavailable: {dependencyId}";
                        changed = true;
                        break;
                    }
                }
            }
        }
    }

    private void SetLoadError(PluginDescriptor descriptor, string message)
    {
        descriptor.EffectiveState = PluginEffectiveState.LoadError;
        descriptor.EffectiveStateReason = message;
        _loadErrors.Add(message);
        _logger?.LogWarning("Plugin load issue. Plugin={PluginId} Message={Message}", descriptor.Meta.Id, message);
    }

    /// <summary>
    /// Checks if a specific tool is enabled based on the plugin configuration.
    /// Expected tool name format: [pluginId].[domain].[action]
    /// </summary>
    public bool IsToolEnabled(string toolName)
    {
        var parts = toolName.Split('.');
        if (parts.Length == 0) return true; // Can't identify plugin, default to true

        var pluginId = parts[0];
        
        if (_settings.Plugins.TryGetValue(pluginId, out var pluginSettings))
        {
            if (pluginSettings.Tools != null && pluginSettings.Tools.TryGetValue(toolName, out var isToolEnabled))
            {
                return isToolEnabled;
            }
        }

        // Default to enabled if no explicit rule is found
        return true;
    }
}

internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private static readonly HashSet<string> SharedAssemblies = new(StringComparer.OrdinalIgnoreCase)
    {
        "SPLA.Domain",
        "SPLA.MCP.Core",
        "SPLA.Observability"
    };

    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: false)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name != null && SharedAssemblies.Contains(assemblyName.Name))
        {
            return null;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}
