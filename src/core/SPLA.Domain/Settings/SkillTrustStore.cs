using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Settings;

/// <summary>One recorded decision to trust the contents of a location.</summary>
/// <param name="Path">The normalised absolute path that was approved — the key.</param>
/// <param name="GrantedAt">When, so a person can tell an old grant from one they just made.</param>
/// <param name="GrantedBy">User key of whoever approved it. Meaningless locally, load-bearing on a
/// server, where one person's grant must not silently cover another's.</param>
public sealed record SkillTrustGrant(string Path, DateTimeOffset GrantedAt, string? GrantedBy = null);

/// <summary>
/// Who vouched for which folder — kept apart from the sources themselves and addressed by path.
///
/// <para><b>Why a separate file.</b> The same rule this project already states for
/// <see cref="Secrets.SecretAcl"/>: an ACL lives beside the store, never inside it. The source list
/// is edited as data; a grant is a decision about safety, and putting them in one document creates a
/// way to award yourself trust by editing the field next door. Self-signed trust is not trust — the
/// reason git refuses a config from a repository owned by someone else, and apt keeps repository
/// keys in a keyring the repository does not control.</para>
///
/// <para><b>Why the key is a path and not an id.</b> You trust contents, and contents live at a
/// location. Rename the folder and the grant does not follow — correct, not inconvenient, because
/// what is there now is not what was read. Rename the entry in a config and the grant stays with the
/// folder, so a renamed entry cannot inherit somebody else's approval.</para>
/// </summary>
public interface ISkillTrustStore
{
    /// <summary>True when this exact location has been approved.</summary>
    bool IsGranted(string absolutePath);

    /// <summary>Records approval. Idempotent — approving twice is not an error, it just refreshes.</summary>
    void Grant(string absolutePath, string? userKey = null);

    /// <summary>Withdraws approval. Returns false when there was none.</summary>
    bool Revoke(string absolutePath);

    IReadOnlyList<SkillTrustGrant> List();
}

/// <summary>
/// YAML-backed <see cref="ISkillTrustStore"/>: <c>skills.acl.yaml</c> beside the user's
/// <c>skills.yaml</c>, matching the <c>secrets.yaml</c> / <c>secrets.acl.yaml</c> pair next to it.
/// </summary>
public sealed class FileSkillTrustStore : ISkillTrustStore
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

    public FileSkillTrustStore(string userDir) => _file = Path.Combine(userDir, "skills.acl.yaml");

    public string FilePath => _file;

    /// <summary>
    /// One spelling per location. Without this, <c>D:\ops\</c> and <c>D:/ops</c> would be two grants
    /// for one folder — and, worse, a lookup could miss a grant that was genuinely made and leave a
    /// person clicking "trust" on something already trusted.
    /// </summary>
    public static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        try
        {
            return Path.GetFullPath(path.Trim())
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch { return path.Trim(); }
    }

    /// <summary>
    /// The on-disk shape. <see cref="GrantedAt"/> is a STRING, not a <see cref="DateTimeOffset"/>:
    /// YamlDotNet serialises that type by walking its properties, so a timestamp came out as twenty
    /// lines of day-of-year and total-offset-minutes. This file is meant to be readable by the person
    /// whose decisions it records.
    /// </summary>
    private sealed class Record
    {
        [YamlMember(Alias = "granted_at")] public string? GrantedAt { get; set; }
        [YamlMember(Alias = "granted_by")] public string? GrantedBy { get; set; }
    }

    private static DateTimeOffset ParseGrantedAt(string? value) =>
        DateTimeOffset.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : default;

    private Dictionary<string, Record> Load()
    {
        // Case-insensitive because Windows paths are; a Linux server pays a false match on two paths
        // differing only in case, which is a far cheaper mistake than failing to find a real grant.
        var map = new Dictionary<string, Record>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(_file)) return map;
        try
        {
            var yaml = File.ReadAllText(_file);
            if (string.IsNullOrWhiteSpace(yaml)) return map;
            var raw = De.Deserialize<Dictionary<string, Record>>(yaml);
            if (raw is null) return map;
            foreach (var (k, v) in raw) map[Normalize(k)] = v ?? new Record();
            return map;
        }
        // An unreadable grant file means NOTHING is trusted, never everything: the failure has to
        // fall on the safe side, and an untrusted source is visible and fixable.
        catch { return new(StringComparer.OrdinalIgnoreCase); }
    }

    private void Store(Dictionary<string, Record> map)
    {
        var dir = Path.GetDirectoryName(_file);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        if (map.Count == 0)
        {
            if (File.Exists(_file)) File.Delete(_file);
            return;
        }
        File.WriteAllText(_file, Ser.Serialize(map));
    }

    public bool IsGranted(string absolutePath)
    {
        var key = Normalize(absolutePath);
        return key.Length > 0 && Load().ContainsKey(key);
    }

    public void Grant(string absolutePath, string? userKey = null)
    {
        var key = Normalize(absolutePath);
        if (key.Length == 0) return;

        var map = Load();
        map[key] = new Record { GrantedAt = DateTimeOffset.Now.ToString("o"), GrantedBy = userKey };
        Store(map);
    }

    public bool Revoke(string absolutePath)
    {
        var map = Load();
        if (!map.Remove(Normalize(absolutePath))) return false;
        Store(map);
        return true;
    }

    public IReadOnlyList<SkillTrustGrant> List() =>
        Load().Select(kv => new SkillTrustGrant(kv.Key, ParseGrantedAt(kv.Value.GrantedAt), kv.Value.GrantedBy))
              .OrderBy(g => g.Path, StringComparer.OrdinalIgnoreCase)
              .ToList();
}
