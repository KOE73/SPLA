using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SPLA.MCP.Core.Plugins;

public class SkillManager
{
    private readonly ILogger<SkillManager>? _logger;
    private readonly List<SkillMeta> _skills = [];
    private string? _pluginsDirectory;

    /// <summary>Raised after a successful <see cref="Reload"/>.</summary>
    public event EventHandler? Reloaded;

    public SkillManager(ILogger<SkillManager>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Re-scans the last loaded plugins directory and rebuilds the skill list.
    /// No-op if <see cref="LoadSkills"/> has not been called yet.
    /// </summary>
    public void Reload()
    {
        if (_pluginsDirectory is null) return;
        LoadSkills(_pluginsDirectory);
        Reloaded?.Invoke(this, EventArgs.Empty);
    }

    public void LoadSkills(string pluginsDirectory)
    {
        _pluginsDirectory = pluginsDirectory;
        _skills.Clear();

        if (!Directory.Exists(pluginsDirectory)) return;

        foreach (var pluginDir in Directory.GetDirectories(pluginsDirectory))
        {
            var pluginId = Path.GetFileName(pluginDir);
            var skillsDir = Path.Combine(pluginDir, "skills");
            if (!Directory.Exists(skillsDir)) continue;

            foreach (var file in Directory.GetFiles(skillsDir, "*.md").OrderBy(f => f))
            {
                var meta = ParseFrontmatter(file, pluginId);
                _skills.Add(meta);
                _logger?.LogInformation("Skill loaded. Plugin={PluginId} Skill={SkillId}", pluginId, meta.Id);
            }
        }

        _logger?.LogInformation("Skills loaded. Total={Count}", _skills.Count);
    }

    public IReadOnlyList<SkillMeta> GetAll() => _skills;

    public IReadOnlyList<SkillMeta> GetEnabled() =>
        _skills.Where(s => s.IsEnabled && (s.OwnerPlugin?.IsEffectivelyEnabled ?? true)).ToList();

    /// <summary>Register a single skill file sourced from a type:skills plugin descriptor.
    /// Replaces any scan-loaded entry with the same Id so there are no duplicates during migration.</summary>
    public void RegisterFromPlugin(string filePath, string pluginId, PluginDescriptor owner)
    {
        var meta = ParseFrontmatter(filePath, pluginId);
        meta.OwnerPlugin = owner;
        // Remove stale scan-path entry with the same Id (e.g. old plugins/network/skills/*.md)
        for (int i = _skills.Count - 1; i >= 0; i--)
            if (_skills[i].Id.Equals(meta.Id, StringComparison.OrdinalIgnoreCase) && _skills[i].OwnerPlugin == null)
                _skills.RemoveAt(i);
        _skills.Add(meta);
        _logger?.LogInformation("Skill registered from plugin. Plugin={PluginId} Skill={SkillId}", pluginId, meta.Id);
    }

    /// <summary>Remove all skills that were registered via <see cref="RegisterFromPlugin"/>.
    /// Call before re-running <see cref="LoadPlugins"/> on the owning PluginManager.</summary>
    public void ClearPluginSkills()
    {
        for (int i = _skills.Count - 1; i >= 0; i--)
            if (_skills[i].OwnerPlugin != null)
                _skills.RemoveAt(i);
    }

    public SkillMeta? Find(string id) =>
        _skills.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public string? LoadBody(string id)
    {
        var skill = Find(id);
        if (skill == null) return null;
        try
        {
            var raw = File.ReadAllText(skill.FilePath);
            return StripFrontmatter(raw);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to read skill body. SkillId={SkillId}", id);
            return null;
        }
    }

    public void ApplySettings(IDictionary<string, (bool enabled, bool preloaded)> settings)
    {
        foreach (var skill in _skills)
        {
            if (settings.TryGetValue(skill.Id, out var s))
            {
                skill.IsEnabled = s.enabled;
                skill.IsPreloaded = s.preloaded;
            }
        }
    }

    private static SkillMeta ParseFrontmatter(string filePath, string pluginId)
    {
        var id = Path.GetFileNameWithoutExtension(filePath);
        var description = string.Empty;

        try
        {
            var text = File.ReadAllText(filePath);
            if (text.StartsWith("---", StringComparison.Ordinal))
            {
                var end = text.IndexOf("---", 3, StringComparison.Ordinal);
                if (end > 0)
                {
                    foreach (var line in text[3..end].Split('\n'))
                    {
                        var t = line.Trim();
                        if (t.StartsWith("id:", StringComparison.OrdinalIgnoreCase))
                            id = t[3..].Trim();
                        else if (t.StartsWith("description:", StringComparison.OrdinalIgnoreCase))
                            description = t[12..].Trim();
                    }
                }
            }
        }
        catch { }

        return new SkillMeta
        {
            Id = id,
            Description = description,
            FilePath = filePath,
            PluginId = pluginId
        };
    }

    private static string StripFrontmatter(string content)
    {
        if (!content.StartsWith("---", StringComparison.Ordinal)) return content;
        var end = content.IndexOf("---", 3, StringComparison.Ordinal);
        return end < 0 ? content : content[(end + 3)..].TrimStart('\n', '\r');
    }
}
