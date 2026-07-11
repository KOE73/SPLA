using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.MCP.Core.Plugins;

/// <summary>
/// The stateless discovery half of the plugin system: turns a plugins directory into a list of
/// <see cref="PluginDescriptor"/> (manifest parse + user-enable resolution + dependency states),
/// with no assembly loading and no runtime registry. Pure over the file system + settings, so it
/// is unit-testable without loading a single plugin DLL. The <see cref="PluginManager"/> facade
/// composes this with <see cref="PluginAssemblyLoader"/>.
/// </summary>
internal static class PluginDiscovery
{
    /// <summary>Scans <paramref name="pluginsDirectory"/> for <c>meta.yaml</c> manifests and builds a
    /// descriptor per plugin, resolving user-enable state and cross-plugin dependency states. Never
    /// loads an assembly.</summary>
    public static IReadOnlyList<PluginDescriptor> Discover(
        ResolvedSettings settings, string pluginsDirectory, List<string>? loadErrors = null)
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
                var meta = deserializer.Deserialize<PluginMeta>(File.ReadAllText(metaPath));
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
                    AttachCommand(descriptor, command);

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

    /// <summary>Normalizes a UI command (assigns its owning plugin id, resolves a file target relative
    /// to the plugin directory) and records it on the descriptor. Used both for manifest-declared
    /// commands here and for commands a loaded plugin generates at runtime.</summary>
    public static void AttachCommand(PluginDescriptor descriptor, SplaPluginUiCommand command)
    {
        command.PluginId = string.IsNullOrWhiteSpace(command.PluginId) ? descriptor.Meta.Id : command.PluginId;
        command.Target = ResolveCommandTarget(descriptor.DirectoryPath, command);
        descriptor.Commands.Add(command);
    }

    private static string ResolveCommandTarget(string pluginDir, SplaPluginUiCommand command)
    {
        if (command.Kind is SplaPluginUiCommandKind.OpenUrl
            or SplaPluginUiCommandKind.OpenPanel
            or SplaPluginUiCommandKind.RunTool
            or SplaPluginUiCommandKind.CopyText
            or SplaPluginUiCommandKind.InsertText
            or SplaPluginUiCommandKind.ExecuteAction)
        {
            return command.Target;
        }

        if (string.IsNullOrWhiteSpace(command.Target) || Path.IsPathRooted(command.Target))
            return command.Target;

        return Path.GetFullPath(Path.Combine(pluginDir, command.Target));
    }

    /// <summary>Resolves each plugin's effective state against its declared dependencies: a missing
    /// or user-disabled dependency disables the dependant, then the disable propagates transitively
    /// until the graph reaches a fixed point.</summary>
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
}
