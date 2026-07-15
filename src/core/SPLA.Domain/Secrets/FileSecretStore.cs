namespace SPLA.Domain.Secrets;

/// <summary>
/// Naive plaintext backend: YAML files. Project secrets in <c>&lt;workspace&gt;/.spla/secrets.yaml</c>,
/// machine secrets in <c>&lt;machineDir&gt;/secrets.yaml</c>. Both folders are gitignored, so this keeps
/// secrets out of source control — but NOT encrypted at rest. The DPAPI backend
/// (<c>SPLA.Secrets.Dpapi</c>) swaps in behind <see cref="ISecretStore"/> without touching callers;
/// this store stays the default and the intended choice for local development and tests, where
/// transparency (inspect / seed a fixture) is a feature, not a risk.
///
/// File format is shared with the DPAPI store via <see cref="SecretYamlFile"/> — entry key →
/// field map, read fresh each access so cross-process writes need no restart.
/// </summary>
public sealed class FileSecretStore : ISecretStore
{
    private readonly string? _projectFile;
    private readonly string _machineFile;

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

    private static SecretEntry? Read(string? file, string key)
    {
        var fields = SecretYamlFile.Load(file).GetValueOrDefault(key);
        return fields is { Count: > 0 } ? new SecretEntry(key, fields) : null;
    }

    public ValueTask<SecretEntry?> GetEntryAsync(string key, CancellationToken ct = default)
        // Project overrides Machine.
        => ValueTask.FromResult(Read(_projectFile, key) ?? Read(_machineFile, key));

    public ValueTask<SecretEntry?> GetEntryAsync(string key, SecretScope scope, CancellationToken ct = default)
        => ValueTask.FromResult(Read(FileFor(scope), key));

    public ValueTask SetEntryAsync(string key, IReadOnlyDictionary<string, string> fields, SecretScope scope, CancellationToken ct = default)
    {
        var file = FileFor(scope)
            ?? throw new InvalidOperationException("No project is open — cannot store a project-scoped secret.");
        var map = SecretYamlFile.Load(file);
        if (fields.Count == 0) map.Remove(key);
        else map[key] = new Dictionary<string, string>(fields, StringComparer.OrdinalIgnoreCase);
        SecretYamlFile.Save(file, map);
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> DeleteAsync(string key, SecretScope scope, CancellationToken ct = default)
    {
        var file = FileFor(scope);
        if (file is null) return ValueTask.FromResult(false);
        var map = SecretYamlFile.Load(file);
        var removed = map.Remove(key);
        if (removed) SecretYamlFile.Save(file, map);
        return ValueTask.FromResult(removed);
    }

    public ValueTask<IReadOnlyList<SecretEntryInfo>> ListEntriesAsync(SecretScope scope, string? prefix = null, CancellationToken ct = default)
    {
        IEnumerable<KeyValuePair<string, Dictionary<string, string>>> entries = SecretYamlFile.Load(FileFor(scope));
        if (!string.IsNullOrEmpty(prefix))
            entries = entries.Where(e => e.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return ValueTask.FromResult<IReadOnlyList<SecretEntryInfo>>(entries
            .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
            .Select(e => new SecretEntryInfo(e.Key, e.Value.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList()))
            .ToList());
    }

    public ValueTask<bool> ContainsAsync(string key, CancellationToken ct = default)
        => ValueTask.FromResult(SecretYamlFile.Load(_projectFile).ContainsKey(key) || SecretYamlFile.Load(_machineFile).ContainsKey(key));
}
