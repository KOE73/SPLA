namespace SPLA.Domain.Secrets;

/// <summary>
/// Phase-1 naive backend: plaintext YAML files. Project secrets in
/// <c>&lt;workspace&gt;/.spla/secrets.yaml</c>, machine secrets in <c>&lt;machineDir&gt;/secrets.yaml</c>.
/// Both folders are gitignored, so this keeps secrets out of source control — but NOT encrypted
/// at rest. The DPAPI backend (<c>SPLA.Secrets.Dpapi</c>) swaps in behind <see cref="ISecretStore"/>
/// without touching callers; this store stays the default and the intended choice for local
/// development and tests, where transparency (inspect / seed a fixture) is a feature, not a risk.
///
/// File format is shared with the DPAPI store via <see cref="SecretYamlFile"/> — a flat
/// <c>key: value</c> map, read fresh each access so cross-process writes need no restart.
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

    public ValueTask<string?> GetAsync(string key, CancellationToken ct = default)
    {
        // Project overrides Machine.
        var v = SecretYamlFile.Load(_projectFile).GetValueOrDefault(key)
                ?? SecretYamlFile.Load(_machineFile).GetValueOrDefault(key);
        return ValueTask.FromResult(v);
    }

    public ValueTask<string?> GetAsync(string key, SecretScope scope, CancellationToken ct = default)
        => ValueTask.FromResult(SecretYamlFile.Load(FileFor(scope)).GetValueOrDefault(key));

    public ValueTask SetAsync(string key, string value, SecretScope scope, CancellationToken ct = default)
    {
        var file = FileFor(scope)
            ?? throw new InvalidOperationException("No project is open — cannot store a project-scoped secret.");
        var map = SecretYamlFile.Load(file);
        map[key] = value;
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

    public ValueTask<IReadOnlyList<string>> ListKeysAsync(SecretScope scope, string? prefix = null, CancellationToken ct = default)
    {
        IEnumerable<string> keys = SecretYamlFile.Load(FileFor(scope)).Keys;
        if (!string.IsNullOrEmpty(prefix))
            keys = keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        return ValueTask.FromResult<IReadOnlyList<string>>(keys.OrderBy(k => k).ToList());
    }

    public ValueTask<bool> ContainsAsync(string key, CancellationToken ct = default)
        => ValueTask.FromResult(SecretYamlFile.Load(_projectFile).ContainsKey(key) || SecretYamlFile.Load(_machineFile).ContainsKey(key));
}
