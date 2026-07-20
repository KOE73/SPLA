using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Editor;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;

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

/// <summary>
/// Facade over the plugin system's three concerns: discovery (<see cref="PluginDiscovery"/>),
/// assembly loading (<see cref="PluginAssemblyLoader"/>), and the runtime registry (the mutable
/// collections owned here). It orchestrates a load pass and answers every query the host asks —
/// tools, UI commands, active plugins, schema providers, load errors — while each underlying
/// concern stays independently testable.
/// </summary>
public class PluginManager
{
    private readonly ResolvedSettings _settings;
    private readonly ILogger<PluginManager>? _logger;
    private readonly SkillManager? _skillManager;
    private readonly PluginAssemblyLoader _loader;

    // Runtime registry state — the result of the most recent LoadPlugins pass.
    private readonly List<PluginDescriptor> _plugins = new();
    private readonly List<PluginInstance> _activePlugins = new();
    private readonly List<IMcpTool> _dynamicTools = new();
    // Which plugin each dynamic tool came from — lets the host gate a tool by the plugin's LIVE
    // enabled flag (a disabled plugin's tools vanish immediately; enabling a plugin that was never
    // loaded still needs a restart because its assembly was skipped at startup).
    private readonly Dictionary<IMcpTool, string> _toolOwners = new();
    // dll plugins whose assembly has actually been loaded (enabled at startup, or lazily via
    // EnsureLoaded when the user enables one at runtime).
    private readonly HashSet<string> _loadedPluginIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SplaPluginUiCommand> _uiCommands = new();
    private readonly List<string> _loadErrors = new();
    private readonly Dictionary<string, ISplaPluginAction> _actionHandlers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ISplaPluginPanelProvider> _panelProviders = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IJsonSchemaProvider> _schemaProviders = new();

    public PluginManager(ResolvedSettings settings, ILogger<PluginManager>? logger = null, SkillManager? skillManager = null)
    {
        _settings = settings;
        _logger = logger;
        _skillManager = skillManager;
        _loader = new PluginAssemblyLoader(settings, logger);
    }

    private static bool IsSkillsType(PluginDescriptor d) =>
        d.Meta.Type.Equals("skills", StringComparison.OrdinalIgnoreCase);

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
        _toolOwners.Clear();
        _loadedPluginIds.Clear();
        _actionHandlers.Clear();
        _panelProviders.Clear();
        _schemaProviders.Clear();
        _skillManager?.ClearPluginSkills();

        _plugins.AddRange(PluginDiscovery.Discover(_settings, pluginsDirectory, _loadErrors));
        _logger?.LogInformation("Plugin discovery finished. Directory={PluginDirectory} Count={PluginCount} Errors={ErrorCount}", pluginsDirectory, _plugins.Count, _loadErrors.Count);

        // Register skills from type:skills plugins (regardless of enabled state so sidebar shows them)
        foreach (var descriptor in _plugins.Where(IsSkillsType))
            RegisterSkillPlugin(descriptor);

        foreach (var descriptor in _plugins.Where(p => p.IsEffectivelyEnabled && !IsSkillsType(p)))
            LoadEnabledPlugin(descriptor);
    }

    /// <summary>Discovery-only enumeration (no assembly loading) — used by the settings UI to list
    /// plugins and their effective states without paying the cost or side effects of loading DLLs.</summary>
    public static IReadOnlyList<PluginDescriptor> DiscoverPlugins(
        ResolvedSettings settings, string pluginsDirectory, List<string>? loadErrors = null)
        => PluginDiscovery.Discover(settings, pluginsDirectory, loadErrors);

    private void RegisterSkillPlugin(PluginDescriptor descriptor)
    {
        if (_skillManager == null) return;
        try
        {
            foreach (var file in System.IO.Directory.GetFiles(descriptor.DirectoryPath, "*.md").OrderBy(f => f))
                _skillManager.RegisterFromPlugin(file, descriptor.Meta.Id, descriptor);

            if (descriptor.IsEffectivelyEnabled)
                _activePlugins.Add(new PluginInstance(descriptor.Meta, descriptor.EffectivePrompt));

            _logger?.LogInformation("Skills plugin registered. Plugin={PluginId} Enabled={Enabled}", descriptor.Meta.Id, descriptor.IsEffectivelyEnabled);
        }
        catch (Exception ex)
        {
            SetLoadError(descriptor, $"Error registering skills plugin '{descriptor.Meta.Id}': {ex.Message}");
        }
    }

    public IReadOnlyList<PluginDescriptor> GetPlugins() => _plugins;
    public IReadOnlyList<PluginInstance> GetActivePlugins() => _activePlugins;
    public IReadOnlyList<IMcpTool> GetDynamicTools() => _dynamicTools;
    public IReadOnlyList<SplaPluginUiCommand> GetUiCommands() => _uiCommands;
    public IReadOnlyList<string> GetLoadErrors() => _loadErrors;
    public IReadOnlyList<IJsonSchemaProvider> GetSchemaProviders() => _schemaProviders;
    public ISplaPluginPanelProvider? GetPanelProvider(string panelType) =>
        _panelProviders.GetValueOrDefault(panelType);

    /// <summary>Invokes an action exposed by a plugin's <see cref="ISplaPluginAction"/>
    /// (e.g. "Test Connection" from its web settings UI). Throws if the plugin has no handler.</summary>
    public Task<object?> InvokeActionAsync(string pluginId, string action, string? valueJson, CancellationToken ct = default)
    {
        if (!_actionHandlers.TryGetValue(pluginId, out var handler))
            throw new InvalidOperationException($"Plugin '{pluginId}' has no action handler.");
        return handler.InvokeActionAsync(action, valueJson, ct);
    }

    /// <summary>Resolves the on-disk directory for a discovered plugin, or null if unknown.
    /// Used to serve the plugin's prebuilt web settings asset (see <see cref="PluginMeta.WebSettingsEntry"/>).</summary>
    public string? GetPluginDirectory(string pluginId) =>
        _plugins.FirstOrDefault(p => string.Equals(p.Meta.Id, pluginId, StringComparison.OrdinalIgnoreCase))?.DirectoryPath;

    /// <summary>
    /// Lazily loads a dll plugin that was skipped at startup (disabled then, enabled now) and
    /// returns the tools it just contributed — the caller registers them with its tool host. This is
    /// what makes enable/disable symmetric at runtime: DISABLE only gates exposure (assembly stays),
    /// ENABLE loads on demand — no service restart, which a multi-user server cannot afford.
    /// Returns an empty list when the plugin is unknown, already loaded, still disabled, or not a
    /// dll plugin.
    /// </summary>
    public IReadOnlyList<IMcpTool> EnsureLoaded(string pluginId)
    {
        var descriptor = _plugins.FirstOrDefault(p =>
            string.Equals(p.Meta.Id, pluginId, StringComparison.OrdinalIgnoreCase));
        if (descriptor == null || IsSkillsType(descriptor)) return [];
        if (_loadedPluginIds.Contains(descriptor.Meta.Id)) return [];
        if (!IsPluginEnabled(descriptor.Meta.Id)) return [];

        var before = _dynamicTools.Count;
        LoadEnabledPlugin(descriptor);
        if (descriptor.EffectiveState == PluginEffectiveState.DisabledByUser)
            descriptor.EffectiveState = PluginEffectiveState.Enabled;
        return _dynamicTools.Skip(before).ToList();
    }

    private void LoadEnabledPlugin(PluginDescriptor descriptor)
    {
        try
        {
            var meta = descriptor.Meta;
            _loadedPluginIds.Add(meta.Id);

            if (string.Equals(meta.Type, "dll", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(meta.EntryPoint))
            {
                var loaded = _loader.Load(descriptor);
                _dynamicTools.AddRange(loaded.Tools);
                foreach (var tool in loaded.Tools) _toolOwners[tool] = meta.Id;
                _schemaProviders.AddRange(loaded.SchemaProviders);
                if (loaded.ActionHandler != null)
                    _actionHandlers[meta.Id] = loaded.ActionHandler;
                foreach (var provider in loaded.PanelProviders)
                    _panelProviders[provider.PanelType] = provider;

                foreach (var command in loaded.GeneratedCommands)
                    PublishGeneratedCommand(descriptor, command);

                if (loaded.Tools.Count > 0)
                {
                    _activePlugins.Add(new PluginInstance(meta, descriptor.EffectivePrompt));
                    _logger?.LogInformation("Plugin loaded. Plugin={PluginId} ToolCount={ToolCount}", meta.Id, loaded.Tools.Count);

                    // A self-check failure does not unload the plugin (its tools may still partly work,
                    // and other tools in the same plugin are unaffected) — it flags Degraded so the
                    // Plugins panel and logs surface the problem instead of it only showing up when the
                    // model finally calls the broken tool.
                    if (loaded.Health.Status == PluginHealthStatus.Degraded)
                    {
                        descriptor.EffectiveState = PluginEffectiveState.Degraded;
                        descriptor.EffectiveStateReason = loaded.Health.Message ?? "self-check reported a problem";
                        _loadErrors.Add($"Plugin '{meta.Id}' degraded: {descriptor.EffectiveStateReason}");
                    }
                }
                else
                {
                    SetLoadError(descriptor, $"Plugin '{meta.Id}' did not expose any tools.");
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

    /// <summary>Attaches a runtime-generated UI command to its descriptor and publishes it to the live
    /// command list when the owning plugin is enabled — the runtime analogue of the manifest commands
    /// <see cref="PluginDiscovery"/> attaches at discovery time.</summary>
    private void PublishGeneratedCommand(PluginDescriptor descriptor, SplaPluginUiCommand command)
    {
        PluginDiscovery.AttachCommand(descriptor, command);
        if (descriptor.IsEffectivelyEnabled)
            _uiCommands.Add(command);
    }

    private void SetLoadError(PluginDescriptor descriptor, string message)
    {
        descriptor.EffectiveState = PluginEffectiveState.LoadError;
        descriptor.EffectiveStateReason = message;
        _loadErrors.Add(message);
        _logger?.LogWarning("Plugin load issue. Plugin={PluginId} Message={Message}", descriptor.Meta.Id, message);
    }

    /// <summary>True when the tool may be offered/executed RIGHT NOW: non-plugin tools always are;
    /// a plugin tool follows its plugin's live <c>Enabled</c> flag in settings (mutated in place by
    /// plugins.save), so disabling a plugin takes effect without a restart.</summary>
    /// <summary>Live enabled flag for a plugin id (settings mutated in place by plugins.save).</summary>
    public bool IsPluginEnabled(string pluginId)
        => !_settings.Plugins.TryGetValue(pluginId, out var section) || section.Enabled != false;

    public bool IsToolAvailable(IMcpTool tool)
    {
        return !_toolOwners.TryGetValue(tool, out var pluginId) || IsPluginEnabled(pluginId);
    }

    /// <summary>
    /// Checks if a specific tool is enabled based on the plugin configuration.
    /// Expected tool name format: [pluginId].[domain].[action]
    /// </summary>
    public bool IsToolEnabled(string toolName)
    {
        foreach (var pluginSettings in _settings.Plugins.Values)
        {
            if (pluginSettings.Tools != null && pluginSettings.Tools.TryGetValue(toolName, out var isToolEnabled))
                return isToolEnabled;
        }

        // Tool names are globally unique. An absent explicit rule keeps the plugin default.
        return true;
    }
}
