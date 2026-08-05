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
    private bool _rebuildPending;

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

    /// <summary>Registers a source at the end of the list.</summary>
    public void Add(ISkillSource source)
    {
        _sources.Add(source);
        source.Changed += OnSourceChanged;
    }

    /// <summary>
    /// Replaces the whole set of branches — what happens when the source list itself is edited,
    /// rather than the contents of a branch.
    ///
    /// <para>Old sources are unsubscribed and disposed, because a folder watcher nobody reads is a
    /// handle nobody closes. The rebuild is deferred while a skill is running, for the same reason
    /// a content change is: swapping the fond under a procedure mid-flight is a failure nobody would
    /// think to suspect.</para>
    /// </summary>
    public void SetSources(IEnumerable<ISkillSource> sources)
    {
        foreach (var source in _sources)
        {
            source.Changed -= OnSourceChanged;
            (source as IDisposable)?.Dispose();
        }
        _sources.Clear();

        foreach (var source in sources)
        {
            _sources.Add(source);
            source.Changed += OnSourceChanged;
        }

        RebuildOrDefer();
    }

    public IReadOnlyList<ISkillSource> Sources => _sources;

    /// <summary>Every known skill, including unavailable ones — this is what the settings panel
    /// lists, so a skill that is off still explains itself.</summary>
    public IReadOnlyList<SkillCard> Holdings() => _skills;

    /// <summary>The skills that may be offered to the model right now. The prompt builder and the
    /// skill tools use only this.</summary>
    public IReadOnlyList<SkillCard> Catalog() =>
        _skills.Where(s => s.State == SkillState.Available).ToList();

    /// <summary>
    /// Resolves an address to one card. Accepts the full <c>branch:id</c> and the bare id, which keeps
    /// working for as long as it is unambiguous.
    ///
    /// <para><b>Ambiguity is judged among the usable books first.</b> Two editions of a name where
    /// only one is available is not a question anyone needs asked: the offered one is meant. Two
    /// AVAILABLE editions is a real question, and it is returned as one. When none is available the
    /// single match is still returned, so the caller can report the actual reason it cannot be used —
    /// "needs port_scan, from plugin 'network'" is actionable and "unknown skill" is not.</para>
    /// </summary>
    public SkillLookup Resolve(string address)
    {
        var wanted = address?.Trim() ?? string.Empty;
        if (wanted.Length == 0) return SkillLookup.Miss();

        // Matched whole, never split on ':'. Source ids carry colons of their own — a plugin branch is
        // literally "plugin:network", so "plugin:network:net.audit" has two — and any rule about which
        // colon separates what would be a rule someone eventually gets wrong. A full address names one
        // shelf, so there is nothing left to be ambiguous about.
        if (_skills.FirstOrDefault(s => s.Address.Equals(wanted, StringComparison.OrdinalIgnoreCase))
            is { } exact)
            return SkillLookup.Hit(exact);

        var matches = _skills
            .Where(s => s.Id.Equals(wanted, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0) return SkillLookup.Miss();
        if (matches.Count == 1) return SkillLookup.Hit(matches[0]);

        var available = matches.Where(s => s.State == SkillState.Available).ToList();
        return available.Count == 1
            ? SkillLookup.Hit(available[0])
            : SkillLookup.Ambiguous(available.Count > 1 ? available : matches);
    }

    /// <summary>Convenience for callers that only need the card and treat "ambiguous" as "no".</summary>
    public SkillCard? Find(string address) => Resolve(address).Card;

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

    private void OnSourceChanged() => RebuildOrDefer();

    /// <summary>
    /// Rebuilds now, or remembers to once the running skill finishes.
    ///
    /// <para>This used to SKIP the rebuild outright, which was survivable while the only trigger was
    /// a file changing — another save would come along and <see cref="Rebuild"/> re-reads everything
    /// anyway. It stops being survivable once the source LIST can change: a person adds a folder,
    /// a skill happens to be running, and the folder simply never appears, with nothing further
    /// scheduled to make it appear. Deferring costs one flag and closes that.</para>
    /// </summary>
    private void RebuildOrDefer()
    {
        if (IsSkillActive?.Invoke() == true)
        {
            _rebuildPending = true;
            _logger?.LogDebug("Skill fond changed while a skill is active — rebuild deferred.");
            return;
        }
        Rebuild();
    }

    /// <summary>Applies a rebuild that was deferred while a skill was running. Called by the host
    /// when a skill is released; a no-op when nothing was deferred.</summary>
    public void ApplyDeferredRebuild()
    {
        if (!_rebuildPending) return;
        _rebuildPending = false;
        _logger?.LogDebug("Applying the rebuild deferred while a skill was active.");
        Rebuild();
    }

    private void Rebuild()
    {
        _skills.Clear();

        // Source order is display order and the tie-break in ranking — it is NOT priority. Two
        // branches holding the same skill id both keep their book; the reader picks by address. The
        // shelf that used to claim an id and mark the rest Superseded is gone with the idea behind
        // it: a library where the second edition evicts the first is not a library.
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

                ResolveState(meta, entry);
                _skills.Add(meta);
            }
        }

        AssignDisplayIds();

        _logger?.LogInformation("Skills rebuilt. Total={Total} Available={Available}",
            _skills.Count, _skills.Count(s => s.State == SkillState.Available));

        Reloaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gives every card the shortest name that still identifies it: the bare id while it occurs once,
    /// the full address once two branches hold it.
    ///
    /// <para>One rule, computed in one place, rather than each caller deciding how much to print.
    /// Always printing the address would be correct and would also charge every prompt for a branch
    /// name in the overwhelmingly common case where there is nothing to disambiguate.</para>
    /// </summary>
    private void AssignDisplayIds()
    {
        var shared = _skills
            .GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var card in _skills)
            card.DisplayId = shared.Contains(card.Id) ? card.Address : card.Id;
    }

    /// <summary>
    /// The <c>skills.items</c> entry that applies to a card. A full address selects one edition; a
    /// bare id applies to every edition of that name and is never ambiguous.
    ///
    /// <para>The key is a PREDICATE here, not an address, and that is the difference between settings
    /// and a loan. "The skill called foo is not for me" is a statement about a name, and switching off
    /// everything under one name is both legitimate and impossible to misread. Needing exactly one
    /// book is a property of borrowing, which is where ambiguity is an error.</para>
    /// </summary>
    private SplaSkillSection? SettingFor(SkillCard meta) =>
        _settings.TryGetValue(meta.Address, out var byAddress) ? byAddress
        : _settings.TryGetValue(meta.Id, out var byId) ? byId
        : null;

    /// <summary>Decides a single skill's effective flags and state. Order matters: an explicit user
    /// decision outranks trust, and both outrank a missing tool — telling someone their disabled
    /// skill also lacks a plugin is noise.</summary>
    private void ResolveState(SkillCard meta, SkillEntry entry)
    {
        var configured = SettingFor(meta);

        var enabled = configured?.Enabled ?? entry.DefaultEnabled;
        meta.IsEnabled = enabled;

        if (!enabled)
        {
            meta.State = SkillState.DisabledByUser;
            meta.StateReason = configured?.Enabled == false ? "disabled in settings" : "disabled by the skill itself";
            return;
        }

        // An untrusted source's skill becomes part of the system prompt, and there is now exactly one
        // way past that: trusting the source, recorded outside anything the source can write.
        //
        // Switching a single skill on used to lift this, and no longer does. If the branch as a whole
        // is not trusted, one arbitrarily trusted book out of it is a contradiction — the contents
        // arrived together, from the same place, on the same terms. The way up is to read the book
        // and copy it into a branch you do trust: one act, visible in a diff, instead of a flag
        // somebody has to remember the meaning of.
        if (meta.Trust == SkillTrust.Untrusted)
        {
            meta.State = SkillState.DisabledByTrust;
            meta.StateReason =
                $"source '{meta.SourceId}' is untrusted — trust the source itself, or copy this skill into one you trust";
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
