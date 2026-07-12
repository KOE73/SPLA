using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Secrets;

/// <summary>
/// Shared on-disk format for the file-based secret stores: a flat <c>key: value</c> YAML map,
/// read fresh on each access (files are tiny and local, so cross-process writes need no restart).
/// <see cref="FileSecretStore"/> stores plaintext values; the DPAPI store stores encrypted blobs —
/// the container format is identical, only the value payload differs. Keeping this in one place
/// keeps the two backends byte-for-byte consistent about how the map is loaded, saved, and deleted.
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

    /// <summary>Loads the map, or an empty (case-insensitive) map when the file is absent/blank/corrupt.</summary>
    public static Dictionary<string, string> Load(string? file)
    {
        if (file is null || !File.Exists(file)) return new(StringComparer.OrdinalIgnoreCase);
        try
        {
            var yaml = File.ReadAllText(file);
            var map = string.IsNullOrWhiteSpace(yaml)
                ? null
                : De.Deserialize<Dictionary<string, string>>(yaml);
            return map is null
                ? new(StringComparer.OrdinalIgnoreCase)
                : new(map, StringComparer.OrdinalIgnoreCase);
        }
        catch { return new(StringComparer.OrdinalIgnoreCase); }
    }

    /// <summary>Writes the map, creating the directory. An empty map deletes the file rather than leaving a stub.</summary>
    public static void Save(string file, Dictionary<string, string> map)
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
