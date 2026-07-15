using System.Text;
using Renci.SshNet;
using SPLA.Domain.Secrets;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// Builds a connected <see cref="SshClient"/> from an <see cref="SshHostConfig"/>, resolving the
/// credential (a <see cref="SecretEntry"/> named by <c>credential:</c>, or the legacy
/// <c>secret:</c>/<c>env:</c> pointers) to values only here, at connect time. Resolved
/// passwords/keys/passphrases live only for the duration of the connect call and are never
/// returned, logged, or surfaced to the tool result. A <c>private_key</c> stored in the secret
/// store is fed to SSH.NET via an in-memory stream — it is never written to disk.
/// </summary>
internal static class SshConnectionFactory
{
    /// <summary>
    /// Resolves the host's login password (credential record field, or the legacy
    /// <c>secret:</c>/<c>env:</c> pointer). Null for key-auth hosts with no password on file.
    /// Used by the live session to auto-answer sudo prompts — the value must never reach tool output.
    /// </summary>
    public static async Task<string?> ResolveLoginPasswordAsync(
        SshHostConfig cfg, ISecretResolver resolver, CancellationToken ct)
    {
        SecretEntry? cred = null;
        if (!string.IsNullOrWhiteSpace(cfg.Credential))
            cred = await resolver.GetEntryAsync(cfg.Credential, ct);
        return cred?[SecretFields.Password] ?? cred?.DefaultValue
            ?? await resolver.ResolveAsync(cfg.Password, ct);
    }

    public static async Task<SshClient> ConnectAsync(
        SshHostConfig cfg, int timeoutSeconds, ISecretResolver resolver, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cfg.Host))
            throw new InvalidOperationException("host is not configured");

        // Whole credential record, when the host references one.
        SecretEntry? cred = null;
        if (!string.IsNullOrWhiteSpace(cfg.Credential))
            cred = await resolver.GetEntryAsync(cfg.Credential, ct)
                ?? throw new InvalidOperationException($"credential '{cfg.Credential}' not found in the secret store");

        var user = cfg.User ?? cred?[SecretFields.User]
            ?? throw new InvalidOperationException("user is not configured (set 'user' or a 'user' field in the credential entry)");

        var timeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 5, 120));
        AuthenticationMethod auth;

        if (cred?[SecretFields.PrivateKey] is { Length: > 0 } pem)
        {
            // Key auth from the store: PEM text → in-memory stream, optional passphrase field.
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(pem));
            var keyFile = cred[SecretFields.Passphrase] is { Length: > 0 } pass
                ? new PrivateKeyFile(ms, pass)
                : new PrivateKeyFile(ms);
            auth = new PrivateKeyAuthenticationMethod(user, keyFile);
        }
        else if (!string.IsNullOrWhiteSpace(cfg.KeyFile))
        {
            // Key auth from a file path (e.g. an existing ~/.ssh key). Passphrase: legacy pointer
            // first, then the credential record's field.
            var passphrase = await resolver.ResolveAsync(cfg.KeyPassphrase, ct) ?? cred?[SecretFields.Passphrase];
            var keyFile = string.IsNullOrEmpty(passphrase)
                ? new PrivateKeyFile(cfg.KeyFile)
                : new PrivateKeyFile(cfg.KeyFile, passphrase);
            auth = new PrivateKeyAuthenticationMethod(user, keyFile);
        }
        else
        {
            var password = cred?[SecretFields.Password] ?? cred?.DefaultValue
                ?? await resolver.ResolveAsync(cfg.Password, ct)
                ?? throw new InvalidOperationException(
                    "no credential resolved — set 'credential' (secret-store entry), 'password' (secret:/env: reference) or 'key_file'");
            auth = new PasswordAuthenticationMethod(user, password);
        }

        var connInfo = new ConnectionInfo(cfg.Host, cfg.Port, user, auth) { Timeout = timeout };
        var client = new SshClient(connInfo);
        // SSH.NET connect is synchronous; run it off the calling thread so cancellation/timeouts work.
        await Task.Run(() => client.Connect(), ct);
        return client;
    }
}
