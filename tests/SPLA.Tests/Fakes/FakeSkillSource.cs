using SPLA.Library.Sources;

namespace SPLA.Tests.Fakes;

/// <summary>
/// In-memory skill source for tests. Exists so skill tests stop writing temp folders and poking at
/// private fields — the whole point of the source abstraction is that the manager does not care
/// where entries come from, and the tests should demonstrate that rather than work around it.
/// </summary>
public sealed class FakeSkillSource : ISkillSource
{
    private readonly Dictionary<string, (SkillEntry Entry, string Body)> _skills = new();
    private readonly Dictionary<(string Ref, string Path), string> _resources = new();

    public string Id { get; }
    public string Label { get; }
    public SkillTrust Trust { get; }

    /// <summary>Flipped by tests to model a provider that has gone away (a disabled plugin).</summary>
    public bool Offline { get; set; }

    public event Action? Changed;

    public SourceLevel Level { get; }

    public FakeSkillSource(string id = "test", SkillTrust trust = SkillTrust.Trusted, string? label = null,
        SourceLevel level = SourceLevel.OnShelf)
    {
        Id = id;
        Label = label ?? id;
        Trust = trust;
        Level = level;
    }

    /// <summary>Adds a skill. Requirements default to none — the common case for a plain procedure.</summary>
    public FakeSkillSource With(
        string id,
        string body = "Step 1: do the thing.",
        string description = "a test skill",
        IReadOnlyList<string>? requiresTools = null,
        IReadOnlyList<string>? requiresFeatures = null,
        bool enabled = true,
        bool preloaded = false,
        IReadOnlyList<string>? tags = null)
    {
        _skills[id] = (new SkillEntry(
            id, description, $"{id}.md",
            new SkillRequirements(requiresTools ?? [], requiresFeatures ?? []),
            SkillRequirements.None,
            enabled, preloaded,
            SPLA.Library.Catalog.SkillTag.NormalizeAll(tags)), body);
        return this;
    }

    /// <summary>Attaches a resource to an already-added skill. Keyed by the skill's ref, the way a
    /// real source addresses it — the manager never passes an id down here.</summary>
    public FakeSkillSource WithResource(string skillId, string path, string text)
    {
        _resources[($"{skillId}.md", path)] = text;
        return this;
    }

    public void Raise() => Changed?.Invoke();

    public IReadOnlyList<SkillEntry> Enumerate() =>
        Offline ? [] : _skills.Values.Select(v => v.Entry).ToList();

    public string? ReadBody(string skillRef) =>
        _skills.Values.FirstOrDefault(v => v.Entry.Ref == skillRef).Body;

    public IReadOnlyList<string> ListResources(string skillRef) =>
        Offline
            ? []
            : _resources.Keys.Where(k => k.Ref == skillRef).Select(k => k.Path)
                             .OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();

    public string? ReadResource(string skillRef, string resourcePath) =>
        Offline ? null : _resources.GetValueOrDefault((skillRef, resourcePath));
}
