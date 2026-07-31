namespace SPLA.Domain.Secrets;

/// <summary>
/// Naive plaintext backend: YAML files, one per scope. The folders are gitignored, so this keeps
/// secrets out of source control — but NOT encrypted at rest. The DPAPI backend
/// (<c>SPLA.Secrets.Dpapi</c>) swaps in behind <see cref="ISecretStore"/> without touching callers;
/// this store stays the default and the intended choice for local development and tests, where
/// transparency (inspect / seed a fixture) is a feature, not a risk.
///
/// <para>Scope → file:</para>
/// <list type="bullet">
/// <item><see cref="SecretScope.User"/> → <c>&lt;userDir&gt;/secrets.yaml</c></item>
/// <item><see cref="SecretScope.Project"/> → <c>&lt;project&gt;/.spla/secrets.yaml</c></item>
/// <item><see cref="SecretScope.Shared"/> → <c>&lt;sharedDir&gt;/secrets.shared.yaml</c></item>
/// </list>
/// <para>
/// Locally User and Shared sit under the same home and enjoy the same OS protection — the difference
/// only becomes real on a server, where User is the caller's private area and Shared is administered
/// centrally. Keeping them separate even locally means a project authored on a laptop declares the
/// same scopes it will need on the server, instead of being rewritten on arrival.
/// </para>
///
/// File format is shared with the DPAPI store via <see cref="SecretYamlFile"/> — entry key →
/// field map, read fresh each access so cross-process writes need no restart.
/// </summary>
public sealed class FileSecretStore : ISecretStore
{
    private readonly string? _projectFile;
    private readonly string _userFile;
    private readonly string _sharedFile;

    /// <param name="workspacePath">Project root, or null when running without a project (no project scope).</param>
    /// <param name="userDir">Directory for this person's secrets (typically <c>~/.spla</c>).</param>
    /// <param name="sharedDir">Directory for administered/shared secrets. Null = same place as
    /// <paramref name="userDir"/>, which is what a single-user local install wants.</param>
    public FileSecretStore(string? workspacePath, string userDir, string? sharedDir = null)
    {
        _projectFile = string.IsNullOrWhiteSpace(workspacePath)
            ? null
            : Path.Combine(workspacePath, ".spla", "secrets.yaml");
        _userFile = Path.Combine(userDir, "secrets.yaml");
        _sharedFile = Path.Combine(sharedDir ?? userDir, "secrets.shared.yaml");

        Acl = new FileSecretAclStore(workspacePath, userDir, sharedDir);
    }

    public ISecretAclStore Acl { get; }

    private string? FileFor(SecretScope scope) => scope switch
    {
        SecretScope.Project => _projectFile,
        SecretScope.User => _userFile,
        SecretScope.Shared => _sharedFile,
        _ => throw new ArgumentOutOfRangeException(nameof(scope))
    };

    private static SecretEntry? Read(string? file, string key)
    {
        var fields = SecretYamlFile.Load(file).GetValueOrDefault(key);
        return fields is { Count: > 0 } ? new SecretEntry(key, fields) : null;
    }

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

    public ValueTask<bool> ContainsAsync(string key, SecretScope scope, CancellationToken ct = default)
        => ValueTask.FromResult(SecretYamlFile.Load(FileFor(scope)).ContainsKey(key));
}
