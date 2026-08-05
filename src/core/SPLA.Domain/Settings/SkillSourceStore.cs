using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Settings;

/// <summary>
/// The branches a person added themselves, as opposed to the ones a project or a deployment
/// prescribed.
///
/// <para><b>Why a second place at all.</b> The cut is by source of authority, not by "UI versus
/// server". Prescribed entries belong to the project and travel with it; granted ones belong to the
/// person and must never be committed — writing "I added a folder from my D: drive" into a shared
/// repository is one person's private decision arriving for everybody else. Secrets solved this
/// question already, with a store that the UI edits and git never sees; sources had no such place,
/// which is exactly why adding a folder was impossible from the panel.</para>
///
/// <para>It is one model with two stores, not two mechanisms: entries have the same shape, they merge
/// into the same list by the same key, and granted ones simply come last — the most specific layer.
/// That is what makes "switch off an inherited branch from the UI" expressible without the UI ever
/// writing to a committed file: it records an override under the same id.</para>
/// </summary>
public interface ISkillSourceStore
{
    /// <summary>The user's own entries, in order. Empty when nothing was ever added.</summary>
    IReadOnlyList<SplaSkillSourceSection> Load();

    /// <summary>Replaces the list wholesale — this store is small, personal and always written as a
    /// unit by the panel, so there is nothing to gain from a per-entry API and a lost-update to lose.</summary>
    void Save(IReadOnlyList<SplaSkillSourceSection> entries);
}

/// <summary>
/// YAML-backed <see cref="ISkillSourceStore"/>: one <c>skills.yaml</c> in the user's area, beside
/// <c>secrets.yaml</c> and named the same way. Read fresh on each access, because the panel, a hand
/// edit and a second process must not need a restart to agree.
/// </summary>
public sealed class FileSkillSourceStore : ISkillSourceStore
{
    private static readonly ISerializer Ser = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    private static readonly IDeserializer De = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly string _file;

    /// <param name="userDir">This person's area — <c>~/.spla</c> locally, their own folder on a server.</param>
    public FileSkillSourceStore(string userDir) => _file = Path.Combine(userDir, "skills.yaml");

    public string FilePath => _file;

    /// <summary>The file shape is the same <c>sources:</c> list the settings files use, so a person
    /// can move an entry between the two by copying the lines.</summary>
    private sealed class Document
    {
        [YamlMember(Alias = "sources")]
        public List<SplaSkillSourceSection>? Sources { get; set; }
    }

    public IReadOnlyList<SplaSkillSourceSection> Load()
    {
        if (!File.Exists(_file)) return [];
        try
        {
            var yaml = File.ReadAllText(_file);
            if (string.IsNullOrWhiteSpace(yaml)) return [];

            var doc = De.Deserialize<Document>(yaml);
            var sources = doc?.Sources ?? [];

            // Stamped on the way in, never read from the file — the same rule as every other layer,
            // and here it is the difference between "the user granted this" and "the file says so".
            foreach (var entry in sources) entry.Origin = SourceOrigin.Granted;
            return sources;
        }
        catch
        {
            // A corrupt personal file must not take the fond down with it: the prescribed branches
            // still resolve, and the panel will show the granted list as empty, which is visible.
            return [];
        }
    }

    public void Save(IReadOnlyList<SplaSkillSourceSection> entries)
    {
        var dir = Path.GetDirectoryName(_file);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        if (entries.Count == 0)
        {
            if (File.Exists(_file)) File.Delete(_file);
            return;
        }

        File.WriteAllText(_file, Ser.Serialize(new Document { Sources = entries.ToList() }));
    }
}
