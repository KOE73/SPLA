using SPLA.MCP.Core.Skills;

namespace SPLA.Tests.Fakes;

/// <summary>
/// In-memory skill source for tests. Exists so skill tests stop writing temp folders and poking at
/// private fields — the whole point of the source abstraction is that the manager does not care
/// where entries come from, and the tests should demonstrate that rather than work around it.
/// </summary>
public sealed class FakeSkillSource : ISkillSource
{
    private readonly Dictionary<string, (SkillEntry Entry, string Body)> _skills = new();

    public string Id { get; }
    public string Label { get; }
    public SkillTrust Trust { get; }

    /// <summary>Flipped by tests to model a provider that has gone away (a disabled plugin).</summary>
    public bool Offline { get; set; }

    public event Action? Changed;

    public FakeSkillSource(string id = "test", SkillTrust trust = SkillTrust.Trusted, string? label = null)
    {
        Id = id;
        Label = label ?? id;
        Trust = trust;
    }

    /// <summary>Adds a skill. Requirements default to none — the common case for a plain procedure.</summary>
    public FakeSkillSource With(
        string id,
        string body = "Step 1: do the thing.",
        string description = "a test skill",
        IReadOnlyList<string>? requiresTools = null,
        IReadOnlyList<string>? requiresFeatures = null,
        bool enabled = true,
        bool preloaded = false)
    {
        _skills[id] = (new SkillEntry(
            id, description, $"{id}.md",
            new SkillRequirements(requiresTools ?? [], requiresFeatures ?? []),
            SkillRequirements.None,
            enabled, preloaded), body);
        return this;
    }

    public void Raise() => Changed?.Invoke();

    public IReadOnlyList<SkillEntry> Enumerate() =>
        Offline ? [] : _skills.Values.Select(v => v.Entry).ToList();

    public string? ReadBody(string skillRef) =>
        _skills.Values.FirstOrDefault(v => v.Entry.Ref == skillRef).Body;
}
