using SPLA.Domain.Agent;
using SPLA.Domain.Project;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Settings;

/// <summary>
/// Project-scoped agent working memory, always persistent. Persisted as the <c>project-kv.yaml</c>
/// key in the project's root <see cref="IBucket"/> — for a local project that is
/// <c>.spla/project-kv.yaml</c> on disk exactly as before, but going through the bucket's key/value
/// API (not a host directory) means this store now works over ANY backend, including a non-disk
/// server/in-memory one where <see cref="IBucket.MapToHostDirectory"/> returns null. Autosaves on
/// every change — this is shared knowledge about the project, not tied to one chat.
/// </summary>
public sealed class ProjectKvStore
{
    private const string KvKey = "project-kv.yaml";

    /// <summary>Labels beside the values, not inside them. The values file is something a person
    /// reads and edits; a machine-written origin on every line would spoil that for the sake of the
    /// few entries that carry one. A project written before labels existed simply has no such file,
    /// and loads with everything unlabelled — which is the honest state, not a guess.</summary>
    private const string OriginsKey = "project-kv-origins.yaml";
    private readonly IBucket _bucket;
    private readonly KeyValueStore _store = new("project");

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public ProjectKvStore(IBucket bucket)
    {
        _bucket = bucket;

        Load();
        // Subscribe AFTER the initial load so loading does not trigger a redundant save.
        _store.Changed += (_, _) => Save();
    }

    public IKeyValueStore Store => _store;

    private void Load()
    {
        var yaml = _bucket.ReadText(KvKey);
        if (string.IsNullOrWhiteSpace(yaml)) return;
        try
        {
            var data = Deserializer.Deserialize<Dictionary<string, string>>(yaml);
            if (data is { Count: > 0 }) _store.LoadFrom(data);
        }
        catch { /* ignore malformed content — start empty */ }

        try
        {
            var originsYaml = _bucket.ReadText(OriginsKey);
            if (string.IsNullOrWhiteSpace(originsYaml)) return;

            var origins = Deserializer.Deserialize<Dictionary<string, PersistedOrigin>>(originsYaml);
            if (origins is { Count: > 0 })
                _store.LoadOrigins(origins
                    .Where(o => o.Value is not null)
                    .Select(o => new KeyValuePair<string, Security.DataOrigin>(
                        o.Key, new Security.DataOrigin(o.Value.Zone, o.Value.Named))));
        }
        catch { /* a lost label must not cost the memory itself */ }
    }

    private void Save()
    {
        try
        {
            _bucket.WriteText(KvKey, Serializer.Serialize(_store.Snapshot()));

            var origins = _store.OriginSnapshot();
            if (origins.Count > 0)
                _bucket.WriteText(OriginsKey, Serializer.Serialize(
                    origins.ToDictionary(
                        o => o.Key,
                        o => new PersistedOrigin { Zone = o.Value.Zone, Named = o.Value.OperatorNamed })));
        }
        catch { /* best effort — memory still works in-process */ }
    }
}

/// <summary>One label as it sits on disk. A record rather than a bare string so the two things that
/// matter — which zone, and whether anybody named it — stay separate instead of being re-derived by
/// parsing a name.</summary>
public sealed class PersistedOrigin
{
    [YamlMember(Alias = "zone")]
    public string Zone { get; set; } = string.Empty;

    [YamlMember(Alias = "named")]
    public bool Named { get; set; } = true;
}
