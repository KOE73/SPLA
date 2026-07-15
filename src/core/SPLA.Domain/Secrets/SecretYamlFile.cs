using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Secrets;

/// <summary>
/// Shared on-disk format for the file-based secret stores: <c>entry key → { field: value }</c> YAML,
/// read fresh on each access (files are tiny and local, so cross-process writes need no restart).
/// <see cref="FileSecretStore"/> stores plaintext field values; the DPAPI store stores encrypted
/// blobs per field — the container format is identical (keys and field names always readable), only
/// the value payload differs. Keeping this in one place keeps the two backends byte-for-byte
/// consistent about how the map is loaded, saved, and deleted.
///
/// <para>A scalar value at the top level (the pre-entry <c>key: value</c> shape) is tolerated on
/// read as an entry with a single <c>password</c> field, so a stray old file degrades gracefully
/// instead of vanishing.</para>
/// </summary>
internal static class SecretYamlFile
{
    private static readonly ISerializer Ser = new SerializerBuilder()
        .WithNamingConvention(NullNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    private static readonly IDeserializer De = new DeserializerBuilder()
        .WithNamingConvention(NullNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private static Dictionary<string, string> NewFields() => new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Loads the map, or an empty (case-insensitive) map when the file is absent/blank/corrupt.</summary>
    public static Dictionary<string, Dictionary<string, string>> Load(string? file)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        if (file is null || !File.Exists(file)) return result;
        try
        {
            var yaml = File.ReadAllText(file);
            if (string.IsNullOrWhiteSpace(yaml)) return result;
            var raw = De.Deserialize<Dictionary<string, object?>>(yaml);
            if (raw is null) return result;

            foreach (var (key, value) in raw)
            {
                switch (value)
                {
                    case IDictionary<object, object?> map:
                    {
                        var fields = NewFields();
                        foreach (var (f, v) in map)
                            if (f is string name && v is not null)
                                fields[name] = v.ToString() ?? "";
                        result[key] = fields;
                        break;
                    }
                    case string s: // legacy flat shape — degrade to a single password field
                        result[key] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            { [SecretFields.Password] = s };
                        break;
                }
            }
            return result;
        }
        catch { return new(StringComparer.OrdinalIgnoreCase); }
    }

    /// <summary>Writes the map, creating the directory. An empty map deletes the file rather than leaving a stub.</summary>
    public static void Save(string file, Dictionary<string, Dictionary<string, string>> map)
    {
        var dir = Path.GetDirectoryName(file);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        if (map.Count == 0)
        {
            if (File.Exists(file)) File.Delete(file);
            return;
        }
        File.WriteAllText(file, Ser.Serialize(map));
    }
}
