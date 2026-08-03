using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SPLA.MCP.Core.Skills;

/// <summary>
/// Skills shipped inside an installed plugin — either a <c>type: skills</c> plugin whose markdown
/// sits at the package root, or any plugin carrying a <c>skills/</c> subfolder next to its dll.
///
/// <para>The plugin's enabled state is expressed by returning NOTHING from <see cref="Enumerate"/>
/// while it is off. That is the whole fix for the original defect: "plugin disabled ⇒ its skills are
/// gone" became a property of the source instead of a nullable owner reference that a skill loaded
/// through a second code path simply did not have, and therefore passed every filter.</para>
///
/// <para>Enabled state arrives as a delegate rather than a PluginDescriptor so this namespace stays
/// free of the plugin types — the registry knows both sides and wires them together.</para>
/// </summary>
public sealed class PluginSkillSource : ISkillSource
{
    private const string SkillsSubfolder = "skills";

    private readonly string _directory;
    private readonly Func<bool> _isEnabled;
    private readonly ILogger? _logger;

    public string Id { get; }
    public string Label { get; }

    /// <summary>Installed plugins are as trusted as the rest of the installation — their dlls already
    /// run in-process, so their markdown is not the weak link.</summary>
    public SkillTrust Trust => SkillTrust.Trusted;

    /// <summary>Plugin packages do not change under a running process; the registry re-creates these
    /// sources on every plugin load pass instead.</summary>
    public event Action? Changed { add { } remove { } }

    public PluginSkillSource(string pluginId, string label, string directory, Func<bool> isEnabled,
        ILogger? logger = null)
    {
        Id = $"plugin:{pluginId}";
        Label = label;
        _directory = directory;
        _isEnabled = isEnabled;
        _logger = logger;
    }

    /// <summary>True when the plugin package carries skill files in either supported layout — the
    /// registry uses this to decide whether the plugin is worth a source at all.</summary>
    public static bool HasSkills(string pluginDirectory)
    {
        try
        {
            return EnumerateFiles(pluginDirectory).Any();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public IReadOnlyList<SkillEntry> Enumerate()
    {
        if (!_isEnabled()) return [];

        var entries = new List<SkillEntry>();
        try
        {
            foreach (var file in EnumerateFiles(_directory))
                entries.Add(SkillFrontmatter.Parse(
                    File.ReadAllText(file),
                    Path.GetFileNameWithoutExtension(file),
                    Path.GetRelativePath(_directory, file).Replace('\\', '/')));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Plugin skills unreadable. Source={SourceId} Path={Path}", Id, _directory);
        }

        return entries;
    }

    public string? ReadBody(string skillRef)
    {
        var full = Path.GetFullPath(Path.Combine(_directory, skillRef));
        var rootPrefix = Path.GetFullPath(_directory) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) || !File.Exists(full)) return null;

        try
        {
            return SkillFrontmatter.StripHeader(File.ReadAllText(full));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Plugin skill body unreadable. Source={SourceId} Ref={Ref}", Id, skillRef);
            return null;
        }
    }

    /// <summary>Both packaging layouts: markdown at the package root (type:skills packages) and a
    /// skills/ subfolder (a dll plugin bundling the procedures for its own tools).</summary>
    private static IEnumerable<string> EnumerateFiles(string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory)) return [];

        var root = Directory.EnumerateFiles(pluginDirectory, "*.md", SearchOption.TopDirectoryOnly);

        var sub = Path.Combine(pluginDirectory, SkillsSubfolder);
        if (Directory.Exists(sub))
            root = root.Concat(Directory.EnumerateFiles(sub, "*.md", SearchOption.TopDirectoryOnly));

        return root.OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
    }
}
