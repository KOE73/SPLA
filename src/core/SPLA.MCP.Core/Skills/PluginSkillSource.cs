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
        var full = ResolveUnder(_directory, skillRef);
        if (full is null || !File.Exists(full)) return null;

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

    /// <summary>
    /// A plugin skill is a single markdown file, so its attachments live in the folder named after it
    /// beside it — <c>skills/host-audit.md</c> is served by <c>skills/host-audit/</c>. That folder is
    /// invisible to <see cref="Enumerate"/> (which only reads top-level <c>*.md</c>), so the
    /// convention costs nothing and cannot turn an appendix into a skill of its own.
    /// </summary>
    public IReadOnlyList<string> ListResources(string skillRef)
    {
        var root = ResourceRoot(skillRef);
        if (root is null || !Directory.Exists(root)) return [];

        try
        {
            return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Plugin skill resources unreadable. Source={SourceId} Ref={Ref}", Id, skillRef);
            return [];
        }
    }

    public string? ReadResource(string skillRef, string resourcePath)
    {
        var root = ResourceRoot(skillRef);
        if (root is null) return null;

        var path = ResolveUnder(root, resourcePath);
        if (path is null || !File.Exists(path)) return null;

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger?.LogWarning(ex, "Plugin skill resource unreadable. Source={SourceId} Ref={Ref} Resource={Resource}",
                Id, skillRef, resourcePath);
            return null;
        }
    }

    /// <summary>The sidecar folder for a skill ref, or null when the ref itself escapes the package.
    /// A disabled plugin serves nothing here either — the skill does not exist while it is off.</summary>
    private string? ResourceRoot(string skillRef)
    {
        if (!_isEnabled()) return null;

        var file = ResolveUnder(_directory, skillRef);
        if (file is null || !File.Exists(file)) return null;

        return Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileNameWithoutExtension(file));
    }

    private static string? ResolveUnder(string baseDir, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative)) return null;

        var root = Path.GetFullPath(baseDir);
        var full = Path.GetFullPath(Path.Combine(root, relative));
        return full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            ? full
            : null;
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
