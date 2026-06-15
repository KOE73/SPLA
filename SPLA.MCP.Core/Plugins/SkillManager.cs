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

    public SkillManager(ILogger<SkillManager>? logger = null)
    {
        _logger = logger;
    }

    public void LoadSkills(string pluginsDirectory)
    {
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

    public IReadOnlyList<SkillMeta> GetEnabled() => _skills.Where(s => s.IsEnabled).ToList();

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
