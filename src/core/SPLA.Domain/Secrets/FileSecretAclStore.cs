using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Secrets;

/// <summary>
/// File-backed <see cref="ISecretAclStore"/>: one <c>secrets.acl.yaml</c> per scope directory,
/// always plaintext — even under the DPAPI backend.
/// <para>
/// That is deliberate, not an oversight. An ACL contains no credential material, and it has to be
/// readable to filter a listing; encrypting it would force a decrypt on every list and break the
/// rule that listing never touches secret values.
/// </para>
/// <code>
/// ssh/prod/root:
///   owner: S-1-5-21-…-1013
///   use:    [S-1-5-21-…-513]     # a group: every operator may connect
///   manage: [S-1-5-21-…-1014]    # a person: the deputy admin may rotate it
/// </code>
/// </summary>
public sealed class FileSecretAclStore : ISecretAclStore
{
    private static readonly ISerializer Ser = new SerializerBuilder()
        .WithNamingConvention(NullNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    private static readonly IDeserializer De = new DeserializerBuilder()
        .WithNamingConvention(NullNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly string? _projectFile;
    private readonly string _userFile;
    private readonly string _sharedFile;

    public FileSecretAclStore(string? workspacePath, string userDir, string? sharedDir = null)
    {
        _projectFile = string.IsNullOrWhiteSpace(workspacePath)
            ? null
            : Path.Combine(workspacePath, ".spla", "secrets.acl.yaml");
        _userFile = Path.Combine(userDir, "secrets.acl.yaml");
        _sharedFile = Path.Combine(sharedDir ?? userDir, "secrets.shared.acl.yaml");
    }

    private string? FileFor(SecretScope scope) => scope switch
    {
        SecretScope.Project => _projectFile,
        SecretScope.User => _userFile,
        SecretScope.Shared => _sharedFile,
        _ => throw new ArgumentOutOfRangeException(nameof(scope))
    };

    public ValueTask<SecretAcl?> GetAsync(SecretScope scope, string key, CancellationToken ct = default)
    {
        var dto = Load(FileFor(scope)).GetValueOrDefault(key);
        return ValueTask.FromResult(dto is null
            ? null
            : new SecretAcl(dto.Owner ?? "", dto.Use ?? [], dto.Manage ?? []));
    }

    public ValueTask SetAsync(SecretScope scope, string key, SecretAcl acl, CancellationToken ct = default)
    {
        var file = FileFor(scope)
            ?? throw new InvalidOperationException("No project is open — cannot store a project-scoped ACL.");
        var map = Load(file);
        map[key] = new AclDto
        {
            Owner = acl.Owner,
            Use = acl.Use.Count == 0 ? null : acl.Use.ToList(),
            Manage = acl.Manage.Count == 0 ? null : acl.Manage.ToList()
        };
        Save(file, map);
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(SecretScope scope, string key, CancellationToken ct = default)
    {
        var file = FileFor(scope);
        if (file is null) return ValueTask.CompletedTask;
        var map = Load(file);
        if (map.Remove(key)) Save(file, map);
        return ValueTask.CompletedTask;
    }

    private static Dictionary<string, AclDto> Load(string? file)
    {
        var empty = new Dictionary<string, AclDto>(StringComparer.OrdinalIgnoreCase);
        if (file is null || !File.Exists(file)) return empty;
        try
        {
            var yaml = File.ReadAllText(file);
            if (string.IsNullOrWhiteSpace(yaml)) return empty;
            var raw = De.Deserialize<Dictionary<string, AclDto>>(yaml);
            return raw is null
                ? empty
                : new Dictionary<string, AclDto>(raw, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            // A corrupt ACL file must not hand out access it cannot prove: treat as "no ACLs
            // recorded", which the server policy reads as closed.
            return empty;
        }
    }

    private static void Save(string file, Dictionary<string, AclDto> map)
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

    private sealed class AclDto
    {
        public string? Owner { get; set; }
        public List<string>? Use { get; set; }
        public List<string>? Manage { get; set; }
    }
}
