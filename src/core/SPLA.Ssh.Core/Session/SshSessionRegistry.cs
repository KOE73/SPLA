using Renci.SshNet;
using SPLA.Domain.Secrets;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// Keeps live <see cref="SshSession"/> instances alive across tool calls, keyed by host name — one
/// shared registry per plugin instance (i.e. per agent session), the same lifetime model as the SQL
/// plugin's connection registry. Without this, every tool call would reconnect and lose shell state
/// (<c>cd</c>, env), which is exactly what a "console" must preserve.
/// </summary>
internal sealed class SshSessionRegistry : IAsyncDisposable
{
    private readonly SshSettings _settings;
    private readonly ISecretResolver _resolver;
    private readonly Dictionary<string, SshSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _gate = new(1, 1);

    public SshSessionRegistry(SshSettings settings, ISecretResolver resolver)
    {
        _settings = settings;
        _resolver = resolver;
    }

    /// <summary>Resolves the host name to use (explicit → default → the sole host), or null if unknown.</summary>
    public (string? Name, SshHostConfig? Cfg) ResolveHost(string? requested)
    {
        var name = requested ?? _settings.DefaultHost
            ?? (_settings.Hosts.Count == 1 ? _settings.Hosts.Keys.First() : null);
        if (name != null && _settings.Hosts.TryGetValue(name, out var cfg)) return (name, cfg);
        return (name, null);
    }

    public bool IsOpen(string hostName)
    {
        lock (_sessions) return _sessions.ContainsKey(hostName);
    }

    public IReadOnlyList<string> OpenSessions()
    {
        lock (_sessions) return _sessions.Keys.ToList();
    }

    /// <summary>Returns the live session for a host, opening (connecting + shell) it on first use.</summary>
    public async Task<SshSession> GetOrOpenAsync(string hostName, SshHostConfig cfg, CancellationToken ct)
    {
        lock (_sessions)
            if (_sessions.TryGetValue(hostName, out var existing)) return existing;

        await _gate.WaitAsync(ct);
        try
        {
            lock (_sessions)
                if (_sessions.TryGetValue(hostName, out var existing)) return existing;

            SshClient client = await SshConnectionFactory.ConnectAsync(cfg, _settings.TimeoutSeconds, _resolver, ct);
            var session = await SshSession.OpenAsync(hostName, client, ct);
            lock (_sessions) _sessions[hostName] = session;
            return session;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Closes and drops a session. Returns true if one was open.</summary>
    public bool Close(string hostName)
    {
        SshSession? session;
        lock (_sessions)
        {
            if (!_sessions.Remove(hostName, out session)) return false;
        }
        session.Dispose();
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        List<SshSession> all;
        lock (_sessions) { all = _sessions.Values.ToList(); _sessions.Clear(); }
        foreach (var s in all) s.Dispose();
        _gate.Dispose();
        await ValueTask.CompletedTask;
    }
}
