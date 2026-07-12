using Renci.SshNet;
using SPLA.Domain.Secrets;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// Builds a connected <see cref="SshClient"/> from an <see cref="SshHostConfig"/>, resolving the
/// credential reference (<c>secret:</c>/<c>env:</c>) to a value only here, at connect time. The
/// resolved password/passphrase lives only for the duration of the connect call and is never
/// returned, logged, or surfaced to the tool result.
/// </summary>
internal static class SshConnectionFactory
{
    public static async Task<SshClient> ConnectAsync(
        SshHostConfig cfg, int timeoutSeconds, ISecretResolver resolver, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cfg.Host))
            throw new InvalidOperationException("host is not configured");
        if (string.IsNullOrWhiteSpace(cfg.User))
            throw new InvalidOperationException("user is not configured");

        var timeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 5, 120));
        ConnectionInfo connInfo;

        if (!string.IsNullOrWhiteSpace(cfg.KeyFile))
        {
            var passphrase = await resolver.ResolveAsync(cfg.KeyPassphrase, ct);
            var keyFile = string.IsNullOrEmpty(passphrase)
                ? new PrivateKeyFile(cfg.KeyFile)
                : new PrivateKeyFile(cfg.KeyFile, passphrase);
            var auth = new PrivateKeyAuthenticationMethod(cfg.User, keyFile);
            connInfo = new ConnectionInfo(cfg.Host, cfg.Port, cfg.User, auth) { Timeout = timeout };
        }
        else
        {
            var password = await resolver.ResolveAsync(cfg.Password, ct)
                ?? throw new InvalidOperationException(
                    "no credential resolved — set 'password' (secret:/env: reference) or 'key_file'");
            var auth = new PasswordAuthenticationMethod(cfg.User, password);
            connInfo = new ConnectionInfo(cfg.Host, cfg.Port, cfg.User, auth) { Timeout = timeout };
        }

        var client = new SshClient(connInfo);
        // SSH.NET connect is synchronous; run it off the calling thread so cancellation/timeouts work.
        await Task.Run(() => client.Connect(), ct);
        return client;
    }
}
