using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SPLA.Domain.Secrets;

/// <summary>
/// Phase-1 naive backend: plaintext YAML files. Project secrets in
/// <c>&lt;workspace&gt;/.spla/secrets.yaml</c>, machine secrets in <c>~/.spla/secrets.yaml</c>.
/// Both folders are gitignored, so this keeps secrets out of source control — but NOT encrypted
/// at rest. A future <c>ProtectedFileSecretStore</c> (DPAPI / libsecret) swaps in behind
/// <see cref="ISecretStore"/> without touching callers.
///
/// File format: a flat <c>key: value</c> map. Reads hit disk each time (files are tiny and local),
/// so writes from another instance/process are picked up without a restart.
/// </summary>
public sealed class FileSecretStore : ISecretStore
{
    private readonly string? _projectFile;
    private readonly string _machineFile;

    private static readonly ISerializer Ser = new SerializerBuilder()
        .WithNamingConvention(NullNamingConvention.Instance)
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    private static readonly IDeserializer De = new DeserializerBuilder()
        .WithNamingConvention(NullNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <param name="workspacePath">Project root, or null when running without a project (no project scope).</param>
    /// <param name="machineDir">Directory for machine-global secrets (typically <c>~/.spla</c>).</param>
    public FileSecretStore(string? workspacePath, string machineDir)
    {
        _projectFile = string.IsNullOrWhiteSpace(workspacePath)
            ? null
            : Path.Combine(workspacePath, ".spla", "secrets.yaml");
        _machineFile = Path.Combine(machineDir, "secrets.yaml");
    }

    private string? FileFor(SecretScope scope) =>
        scope == SecretScope.Project ? _projectFile : _machineFile;

    private static Dictionary<string, string> Load(string? file)
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

    private static void Save(string file, Dictionary<string, string> map)
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

    public ValueTask<string?> GetAsync(string key, CancellationToken ct = default)
    {
        // Project overrides Machine.
        var v = Load(_projectFile).GetValueOrDefault(key) ?? Load(_machineFile).GetValueOrDefault(key);
        return ValueTask.FromResult(v);
    }

    public ValueTask<string?> GetAsync(string key, SecretScope scope, CancellationToken ct = default)
        => ValueTask.FromResult(Load(FileFor(scope)).GetValueOrDefault(key));

    public ValueTask SetAsync(string key, string value, SecretScope scope, CancellationToken ct = default)
    {
        var file = FileFor(scope)
            ?? throw new InvalidOperationException("No project is open — cannot store a project-scoped secret.");
        var map = Load(file);
        map[key] = value;
        Save(file, map);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> DeleteAsync(string key, SecretScope scope, CancellationToken ct = default)
    {
        var file = FileFor(scope);
        if (file is null) return ValueTask.FromResult(false);
        var map = Load(file);
        var removed = map.Remove(key);
        if (removed) Save(file, map);
        return ValueTask.FromResult(removed);
    }

    public ValueTask<IReadOnlyList<string>> ListKeysAsync(SecretScope scope, string? prefix = null, CancellationToken ct = default)
    {
        IEnumerable<string> keys = Load(FileFor(scope)).Keys;
        if (!string.IsNullOrEmpty(prefix))
            keys = keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return ValueTask.FromResult<IReadOnlyList<string>>(keys.OrderBy(k => k).ToList());
    }

    public ValueTask<bool> ContainsAsync(string key, CancellationToken ct = default)
        => ValueTask.FromResult(Load(_projectFile).ContainsKey(key) || Load(_machineFile).ContainsKey(key));
}
