using SPLA.Domain.Agent;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Settings;

/// <summary>
/// Project-scoped agent working memory, always persistent. Backed by <c>project-kv.yaml</c> in the
/// project's <c>.spla</c> directory (or the user defaults dir when there is no project file).
/// Autosaves on every change — this is shared knowledge about the project, not tied to one chat.
/// </summary>
public sealed class ProjectKvStore
{
    private readonly string _filePath;
    private readonly KeyValueStore _store = new("project");

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public ProjectKvStore(ResolvedSettings settings)
    {
        var baseDir = settings.ProjectName != null
            ? Path.Combine(settings.WorkspacePath, ".spla")
            : ConfigLoader.GetDefaultsDir();
        Directory.CreateDirectory(baseDir);
        _filePath = Path.Combine(baseDir, "project-kv.yaml");

        Load();
        // Subscribe AFTER the initial load so loading does not trigger a redundant save.
        _store.Changed += (_, _) => Save();
    }

    public IKeyValueStore Store => _store;

    private void Load()
    {
        if (!File.Exists(_filePath)) return;
        try
        {
            var yaml = File.ReadAllText(_filePath);
            var data = Deserializer.Deserialize<Dictionary<string, string>>(yaml);
            if (data is { Count: > 0 }) _store.LoadFrom(data);
        }
        catch { /* ignore malformed file — start empty */ }
    }

    private void Save()
    {
        try
        {
            File.WriteAllText(_filePath, Serializer.Serialize(_store.Snapshot()));
        }
        catch { /* best effort — memory still works in-process */ }
    }
}
