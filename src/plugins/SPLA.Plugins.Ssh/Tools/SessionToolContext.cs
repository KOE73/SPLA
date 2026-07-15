using SPLA.Domain.Secrets;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// Shared plumbing for the session tools: live settings access (re-read per call so Settings-UI
/// edits apply without restart), the secret resolver, and the session/host resolution rule —
/// explicit <c>session</c> id wins; else the host's most recent open session is reused; else a new
/// session is opened on the resolved host (explicit → default → the sole configured host).
/// </summary>
internal sealed class SessionToolContext
{
    public required Func<SshSettings> Settings { get; init; }
    public required ISecretResolver Resolver { get; init; }

    public async Task<(SshLiveSession? Session, SshHostConfig? Cfg, string? Error)> ResolveAsync(
        SshSessionHub hub, string? sessionId, string? hostName, string openedBy, CancellationToken ct)
    {
        var settings = Settings();

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var session = hub.Get(sessionId);
            if (session == null)
                return (null, null, $"Error: no open session '{sessionId}'. Use ssh_sessions to list open sessions.");
            // The host may have been renamed/removed since the session opened — fall back to a
            // read-only policy then, never to a more permissive one.
            var cfg = settings.Hosts.GetValueOrDefault(session.HostName) ?? new SshHostConfig();
            return (session, cfg, null);
        }

        var name = hostName ?? settings.DefaultHost
            ?? (settings.Hosts.Count == 1 ? settings.Hosts.Keys.First() : null);
        if (name == null || !settings.Hosts.TryGetValue(name, out var hostCfg))
            return (null, null, hostName == null
                ? "Error: no default host configured. Set plugins.ssh.settings.default_host or pass 'host'/'session'."
                : $"Error: unknown host '{hostName}'. Use ssh_list_hosts to see configured hosts.");

        var existing = hub.ForHost(name);
        if (existing.Count > 0)
            return (existing[^1], hostCfg, null);

        var opened = await hub.OpenAsync(name, hostCfg, settings.TimeoutSeconds, Resolver, openedBy, ct);
        return (opened, hostCfg, null);
    }
}
