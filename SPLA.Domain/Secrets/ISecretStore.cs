namespace SPLA.Domain.Secrets;

/// <summary>Where a secret lives.</summary>
public enum SecretScope
{
    /// <summary>Bound to the current project (<c>&lt;workspace&gt;/.spla/secrets.yaml</c>). Travels with the project, not the machine.</summary>
    Project,

    /// <summary>Bound to the current user/machine (<c>~/.spla/secrets.yaml</c>). Shared across all projects.</summary>
    Machine
}

/// <summary>
/// Global secrets store — NOT tied to any plugin. SQL, other plugins, and the host all use it to
/// keep passwords / tokens out of committable config files. The interface is the stable contract;
/// only the backend changes (naive plaintext now → OS-encrypted later: DPAPI on Windows,
/// libsecret/Secret Service on Linux). Async because real backends (keychains, cloud KMS) are async.
/// </summary>
public interface ISecretStore
{
    /// <summary>Plaintext value by key, searching Project first then Machine (project overrides),
    /// or null if absent in both.</summary>
    ValueTask<string?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>Plaintext value from a specific scope, or null.</summary>
    ValueTask<string?> GetAsync(string key, SecretScope scope, CancellationToken ct = default);

    /// <summary>Stores (overwrites) a secret in the given scope.</summary>
    ValueTask SetAsync(string key, string value, SecretScope scope, CancellationToken ct = default);

    /// <summary>Removes a secret from the given scope. Returns true if it existed.</summary>
    ValueTask<bool> DeleteAsync(string key, SecretScope scope, CancellationToken ct = default);

    /// <summary>Keys in a scope — NEVER values. For management UIs / listing. Optional case-insensitive prefix filter.</summary>
    ValueTask<IReadOnlyList<string>> ListKeysAsync(SecretScope scope, string? prefix = null, CancellationToken ct = default);

    /// <summary>True if the key exists in either scope, without returning the value.</summary>
    ValueTask<bool> ContainsAsync(string key, CancellationToken ct = default);
}
