using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Library.Catalog;
using SPLA.Library.Sources;

namespace SPLA.Library;

/// <summary>
/// The skill registry: collects entries from every <see cref="ISkillSource"/> in priority order,
/// layers the user's settings over each skill's own defaults, resolves what the running agent can
/// actually satisfy, and answers what may reach the prompt.
///
/// <para><b>Fail-closed.</b> A skill is offered only when a source vouches for it AND its declared
/// requirements are met AND it is not switched off. There is no path by which an unowned skill
/// defaults to enabled — the previous design had one (a directory scan that ran beside the plugin
/// registration and produced entries with no owner, which then passed the "owner enabled?" filter by
/// way of `?? true`), and that is why disabled plugins kept injecting their skills into the prompt.</para>
/// </summary>
public sealed class SkillLibrary : IDisposable
{
    private readonly List<ISkillSource> _sources = [];
    private readonly ILogger<SkillLibrary>? _logger;
    private readonly List<SkillCard> _skills = [];
    private readonly Dictionary<string, SplaSkillSection> _settings =
        new(StringComparer.OrdinalIgnoreCase);

    private ISkillCapabilityProbe _probe = SkillCapabilityProbe.AllAvailable;

    /// <summary>Raised after the skill list has been rebuilt, for any reason.</summary>
    public event EventHandler? Reloaded;

    /// <summary>Optional guard: while it returns true, a source-triggered reload is skipped so a
    /// running procedure is not swapped out mid-flight. Hosts that track active skills wire it.</summary>
    public Func<bool>? IsSkillActive { get; set; }

    public SkillLibrary(IEnumerable<ISkillSource>? sources = null, ILogger<SkillLibrary>? logger = null)
    {
        _logger = logger;
        if (sources != null)
            foreach (var source in sources) Add(source);

        Rebuild();
    }

    /// <summary>Registers a source at the end of the priority order.</summary>
    public void Add(ISkillSource source)
    {
        _sources.Add(source);
        source.Changed += OnSourceChanged;
    }

    public IReadOnlyList<ISkillSource> Sources => _sources;

    /// <summary>Every known skill, including unavailable ones — this is what the settings panel
    /// lists, so a skill that is off still explains itself.</summary>
    public IReadOnlyList<SkillCard> Holdings() => _skills;

    /// <summary>The skills that may be offered to the model right now. The prompt builder and the
    /// skill tools use only this.</summary>
    public IReadOnlyList<SkillCard> Catalog() =>
        _skills.Where(s => s.State == SkillState.Available).ToList();

    public SkillCard? Find(string id) =>
        _skills.FirstOrDefault(s => s.State != SkillState.Superseded &&
                                    s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    /// <summary>Procedure text for a skill, fetched from its owning source. Null when the skill is
    /// unknown or its source cannot produce the body.</summary>
    public string? LoadBody(string id)
    {
        var skill = Find(id);
        if (skill is null) return null;

        var source = SourceOf(skill.SourceId);
        if (source is null)
        {
            _logger?.LogWarning("Skill source vanished. Skill={SkillId} Source={SourceId}", id, skill.SourceId);
            return null;
        }

        return source.ReadBody(skill.Ref);
    }

    /// <summary>The attachments a skill came with, listed by its owning source. Empty when the skill is
    /// unknown or carries none. Called once, at activation, to fill the loan slip.</summary>
    public IReadOnlyList<string> ListResources(string id)
    {
        var skill = Find(id);
        if (skill is null) return [];

        return SourceOf(skill.SourceId)?.ListResources(skill.Ref) ?? [];
    }

    /// <summary>
    /// One attachment's text, addressed by the coordinates pinned at activation rather than by skill
    /// id. That is the point: the loan slip stays valid across a rebuild that shadowed the skill,
    /// renamed nothing and moved nothing — and it stops resolving the moment the source itself is
    /// gone, which is the honest answer to "the shelf disappeared while the book is out".
    /// </summary>
    public string? ReadResource(string sourceId, string skillRef, string resourcePath)
    {
        var source = SourceOf(sourceId);
        if (source is null)
        {
            _logger?.LogWarning("Skill source vanished. Source={SourceId} Ref={Ref}", sourceId, skillRef);
            return null;
        }

        return source.ReadResource(skillRef, resourcePath);
    }

    private ISkillSource? SourceOf(string sourceId) =>
        _sources.FirstOrDefault(s => s.Id.Equals(sourceId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Supplies the capability answers used to resolve requirements, then rebuilds. Called
    /// once the tool host and the feature set exist — until then requirements resolve optimistically,
    /// which is harmless because nothing reads the list before the runtime is fully constructed.</summary>
    public void SetProbe(ISkillCapabilityProbe probe)
    {
        _probe = probe;
        Rebuild();
    }

    /// <summary>Applies the <c>skills.items</c> overrides and rebuilds.</summary>
    public void ApplySettings(IReadOnlyDictionary<string, SplaSkillSection>? settings)
    {
        _settings.Clear();
        if (settings != null)
            foreach (var kvp in settings) _settings[kvp.Key] = kvp.Value;

        Rebuild();
    }

    /// <summary>Re-enumerates every source and recomputes states.</summary>
    public void Reload() => Rebuild();

    private void OnSourceChanged()
    {
        if (IsSkillActive?.Invoke() == true)
        {
            _logger?.LogDebug("Skill source changed while a skill is active — reload skipped.");
            return;
        }
        Rebuild();
    }

    private void Rebuild()
    {
        _skills.Clear();

        // Source order IS priority order: the first source to claim an id owns it, later ones are
        // marked Superseded rather than dropped so the panel can show that an override is in effect.
        var claimed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in _sources)
        {
            IReadOnlyList<SkillEntry> entries;
            try
            {
                entries = source.Enumerate();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Skill source failed to enumerate. Source={SourceId}", source.Id);
                continue;
            }

            foreach (var entry in entries)
            {
                var meta = new SkillCard
                {
                    Id = entry.Id,
                    SourceId = source.Id,
                    SourceLabel = source.Label,
                    Ref = entry.Ref,
                    Trust = source.Trust,
                    Level = source.Level,
                    Description = entry.Description,
                    Tags = entry.Tags,
                    Requires = entry.Requires,
                    Uses = entry.Uses
                };

                if (claimed.TryGetValue(entry.Id, out var owner))
                {
                    meta.State = SkillState.Superseded;
                    meta.StateReason = $"overridden by source '{owner}'";
                    meta.IsEnabled = false;
                    _skills.Add(meta);
                    continue;
                }

                claimed[entry.Id] = source.Id;
                Resolve(meta, entry);
                _skills.Add(meta);
            }
        }

        _logger?.LogInformation("Skills rebuilt. Total={Total} Available={Available}",
            _skills.Count, _skills.Count(s => s.State == SkillState.Available));

        Reloaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Decides a single skill's effective flags and state. Order matters: an explicit user
    /// decision outranks trust, and both outrank a missing tool — telling someone their disabled
    /// skill also lacks a plugin is noise.</summary>
    private void Resolve(SkillCard meta, SkillEntry entry)
    {
        _settings.TryGetValue(entry.Id, out var configured);

        var enabled = configured?.Enabled ?? entry.DefaultEnabled;
        meta.IsEnabled = enabled;

        if (!enabled)
        {
            meta.State = SkillState.DisabledByUser;
            meta.StateReason = configured?.Enabled == false ? "disabled in settings" : "disabled by the skill itself";
            return;
        }

        // An untrusted source's skill becomes part of the system prompt, so it needs a deliberate
        // opt-in — its own frontmatter saying enabled:true is not the user's decision.
        if (meta.Trust == SkillTrust.Untrusted && configured?.Enabled != true)
        {
            meta.State = SkillState.DisabledByTrust;
            meta.StateReason = $"source '{meta.SourceId}' is untrusted — enable this skill explicitly to use it";
            meta.IsEnabled = false;
            return;
        }

        var missingTools = entry.Requires.Tools.Where(t => !_probe.HasTool(t)).ToList();
        var missingFeatures = entry.Requires.Features.Where(f => !_probe.HasFeature(f)).ToList();

        if (missingTools.Count == 0 && missingFeatures.Count == 0)
        {
            meta.State = SkillState.Available;
            meta.StateReason = string.Empty;
            return;
        }

        var plugins = missingTools
            .Select(_probe.PluginOfTool)
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => p!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        meta.State = SkillState.MissingPrerequisites;
        meta.MissingTools = missingTools;
        meta.MissingFeatures = missingFeatures;
        meta.MissingPlugins = plugins;
        meta.StateReason = BuildMissingReason(missingTools, missingFeatures, plugins);
    }

    private static string BuildMissingReason(
        IReadOnlyList<string> tools, IReadOnlyList<string> features, IReadOnlyList<string> plugins)
    {
        var parts = new List<string>();
        if (tools.Count > 0) parts.Add($"needs {string.Join(", ", tools)}");
        if (features.Count > 0) parts.Add($"needs capability {string.Join(", ", features)}");

        var reason = string.Join("; ", parts);
        if (plugins.Count > 0)
            reason += plugins.Count == 1
                ? $" — from plugin '{plugins[0]}'"
                : $" — from plugins {string.Join(", ", plugins.Select(p => $"'{p}'"))}";

        return reason;
    }

    public void Dispose()
    {
        foreach (var source in _sources)
        {
            source.Changed -= OnSourceChanged;
            (source as IDisposable)?.Dispose();
        }
        _sources.Clear();
    }
}
