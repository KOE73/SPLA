using SPLA.Domain.Secrets;
using SPLA.Domain.Settings;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// The PROJECT-scoped registry of live SSH sessions — the single meeting point of the agent's tools
/// (SPLA.Plugins.Ssh) and the service's human terminals (TerminalHandlers). Both sides obtain the
/// same instance via <see cref="For"/> (ResolvedSettings.SharedServices), so a session the agent
/// opens is the very session a human terminal attaches to, and vice versa.
///
/// <para>Sessions are identified as <c>host#N</c> (ioBroker#1, ioBroker#2 …) — several parallel
/// sessions to one host are normal. A session lives until it is explicitly closed (agent tool,
/// UI, or the connection drops); detaching a terminal does NOT close it.</para>
/// </summary>
public sealed class SshSessionHub
{
    private const string ServiceKey = "ssh.session-hub";

    private readonly object _lock = new();
    private readonly Dictionary<string, SshLiveSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _hostCounters = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Fires on open/close so the service can broadcast <c>ssh.sessions.changed</c>.</summary>
    public event Action? Changed;

    /// <summary>The one hub for this project's runtime — created by whichever side touches it first.</summary>
    public static SshSessionHub For(ResolvedSettings settings)
        => (SshSessionHub)settings.SharedServices.GetOrAdd(ServiceKey, _ => new SshSessionHub());

    public SshLiveSession? Get(string sessionId)
    {
        lock (_lock) return _sessions.GetValueOrDefault(sessionId);
    }

    public IReadOnlyList<SshLiveSession> List()
    {
        lock (_lock) return _sessions.Values.OrderBy(s => s.OpenedAt).ToList();
    }

    /// <summary>Open sessions on one host, oldest first.</summary>
    public IReadOnlyList<SshLiveSession> ForHost(string hostName)
    {
        lock (_lock)
            return _sessions.Values
                .Where(s => string.Equals(s.HostName, hostName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.OpenedAt).ToList();
    }

    /// <summary>Opens a new session to a host and registers it under a fresh <c>host#N</c> id.</summary>
    public async Task<SshLiveSession> OpenAsync(
        string hostName, SshHostConfig cfg, int timeoutSeconds, ISecretResolver resolver,
        string openedBy, CancellationToken ct, uint cols = 120, uint rows = 30)
    {
        string id;
        lock (_lock)
        {
            var n = _hostCounters.GetValueOrDefault(hostName) + 1;
            _hostCounters[hostName] = n;
            id = $"{hostName}#{n}";
        }

        var session = await SshLiveSession.OpenAsync(id, hostName, cfg, timeoutSeconds, resolver, openedBy, ct, cols, rows);
        session.Closed += s =>
        {
            lock (_lock) _sessions.Remove(s.Id);
            RaiseChanged();
        };
        lock (_lock) _sessions[id] = session;
        RaiseChanged();
        return session;
    }

    /// <summary>Closes and removes one session. Returns false when it wasn't open.</summary>
    public bool Close(string sessionId)
    {
        SshLiveSession? session;
        lock (_lock) session = _sessions.GetValueOrDefault(sessionId);
        if (session == null) return false;
        session.Dispose(); // Closed handler removes it and raises Changed
        return true;
    }

    private void RaiseChanged()
    {
        try { Changed?.Invoke(); } catch { /* a broken listener must not break the hub */ }
    }
}
