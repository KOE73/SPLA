using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using SPLA.Domain.Secrets;

namespace SPLA.Secrets.Dpapi;

/// <summary>
/// Windows DPAPI backend for <see cref="ISecretStore"/>. Same two-file, project-overrides-machine
/// layout as <see cref="FileSecretStore"/>, but every FIELD VALUE is encrypted at rest with
/// <see cref="ProtectedData"/> under <see cref="DataProtectionScope.CurrentUser"/> — only the same
/// Windows user on the same machine can decrypt. Files are named <c>secrets.dpapi.yaml</c> so an
/// encrypted store never shares a file with, or clobbers, a plaintext <see cref="FileSecretStore"/>
/// — the two can coexist (e.g. prod-DPAPI + a dev copy under a different <c>SPLA_HOME</c>).
///
/// <para><b>Keys and field names stay readable.</b> Only values are encrypted; entry keys and field
/// names are stored verbatim so listing (management UIs) never needs to decrypt anything. Each value
/// is stored as <c>dpapi:&lt;base64&gt;</c>. A field that fails to decrypt (corrupt file, copied from
/// another user/machine) is treated as absent — a warning is logged rather than throwing, so one bad
/// field can't break the whole store.</para>
///
/// <para><b>DPAPI's known trade-off:</b> it protects at rest and against other users, but any process
/// running as the same user can decrypt. That is the accepted, standard compromise for a local
/// secret store — the goal here is "not plaintext on disk", not defence against same-user malware.</para>
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DpapiFileSecretStore : ISecretStore
{
    private const string Prefix = "dpapi:";

    // App-scoping entropy mixed into every Protect/Unprotect. NOT a secret and NOT a key — it just
    // ties blobs to this application so an unrelated CurrentUser DPAPI blob can't be swapped in.
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("SPLA.SecretStore.v1");

    private readonly string? _projectFile;
    private readonly string _machineFile;
    private readonly Action<string>? _warn;

    /// <param name="workspacePath">Project root, or null when running without a project (no project scope).</param>
    /// <param name="machineDir">Directory for machine-global secrets (typically <c>~/.spla</c>).</param>
    /// <param name="warn">Optional sink for decrypt-failure warnings. Never receives secret values.</param>
    public DpapiFileSecretStore(string? workspacePath, string machineDir, Action<string>? warn = null)
    {
        _projectFile = string.IsNullOrWhiteSpace(workspacePath)
            ? null
            : Path.Combine(workspacePath, ".spla", "secrets.dpapi.yaml");
        _machineFile = Path.Combine(machineDir, "secrets.dpapi.yaml");
        _warn = warn;
    }

    private string? FileFor(SecretScope scope) =>
        scope == SecretScope.Project ? _projectFile : _machineFile;

    private static string Protect(string value)
    {
        var cipher = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), Entropy, DataProtectionScope.CurrentUser);
        return Prefix + Convert.ToBase64String(cipher);
    }

    /// <summary>Decrypts a stored field value, or null if it is malformed / not decryptable by this user.</summary>
    private string? Unprotect(string key, string field, string stored)
    {
        if (!stored.StartsWith(Prefix, StringComparison.Ordinal))
        {
            _warn?.Invoke($"Secret '{key}#{field}' is not in DPAPI format and was ignored.");
            return null;
        }
        try
        {
            var cipher = Convert.FromBase64String(stored.Substring(Prefix.Length));
            var plain = ProtectedData.Unprotect(cipher, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            // Corrupt, or encrypted by a different user/machine. Never log the stored blob.
            _warn?.Invoke($"Secret '{key}#{field}' could not be decrypted and was ignored.");
            return null;
        }
    }

    private SecretEntry? Read(string? file, string key)
    {
        var stored = SecretYamlFile.Load(file).GetValueOrDefault(key);
        if (stored is not { Count: > 0 }) return null;
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, blob) in stored)
            if (Unprotect(key, name, blob) is { } plain)
                fields[name] = plain;
        return fields.Count > 0 ? new SecretEntry(key, fields) : null;
    }

    public ValueTask<SecretEntry?> GetEntryAsync(string key, CancellationToken ct = default)
        => ValueTask.FromResult(Read(_projectFile, key) ?? Read(_machineFile, key));

    public ValueTask<SecretEntry?> GetEntryAsync(string key, SecretScope scope, CancellationToken ct = default)
        => ValueTask.FromResult(Read(FileFor(scope), key));

    public ValueTask SetEntryAsync(string key, IReadOnlyDictionary<string, string> fields, SecretScope scope, CancellationToken ct = default)
    {
        var file = FileFor(scope)
            ?? throw new InvalidOperationException("No project is open — cannot store a project-scoped secret.");
        var map = SecretYamlFile.Load(file);
        if (fields.Count == 0) map.Remove(key);
        else
        {
            var protected_ = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (name, value) in fields) protected_[name] = Protect(value);
            map[key] = protected_;
        }
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
        // Keys and field names are stored verbatim — listing never decrypts.
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
