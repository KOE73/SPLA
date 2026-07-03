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
    }

    private void Save()
    {
        try
        {
            _bucket.WriteText(KvKey, Serializer.Serialize(_store.Snapshot()));
        }
        catch { /* best effort — memory still works in-process */ }
    }
}
