using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SPLA.Plugins.Ssh;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// The xterm terminals of one <see cref="ClientConnection"/>, each a VIEW over a shared
/// <see cref="SshLiveSession"/> from the project's <see cref="SshSessionHub"/> — the same sessions
/// the agent's tools drive, so the human watches the agent type and can type into the same shell.
/// A terminal subscribes (with replay, so attaching to a running session shows recent output) and
/// forwards raw pty chunks as <c>terminal.data</c>; keystrokes go straight in via
/// <c>terminal.input</c> (no read-only guard — the human's own supervised session).
///
/// <para>Closing a terminal DETACHES it; the session stays alive for other viewers and the agent.
/// Sessions end via ssh_session_close, the UI's close, or service shutdown.</para>
/// </summary>
internal sealed class SshTerminalManager : IAsyncDisposable
{
    private readonly Func<string, object, Task> _send;   // (messageType, payload) → send on this connection
    private readonly ILogger _log;
    private readonly ConcurrentDictionary<string, Entry> _terminals = new();

    private sealed record Entry(SshLiveSession Session, IDisposable Subscription, Action<SshLiveSession> OnClosed);

    public SshTerminalManager(Func<string, object, Task> send, ILogger log)
    {
        _send = send;
        _log = log;
    }

    /// <summary>Attaches a terminal to a live session (replaying recent output first).</summary>
    public async Task AttachAsync(string terminalId, SshLiveSession session)
    {
        await DetachAsync(terminalId); // idempotent re-open

        var subscription = session.Subscribe(
            chunk => _ = SafeSend(MessageTypes.TerminalData, new TerminalDataPayload { TerminalId = terminalId, Data = chunk }),
            withReplay: true);

        // When the session itself ends (agent closed it, connection dropped), tell this terminal.
        void OnClosed(SshLiveSession s)
        {
            if (_terminals.TryRemove(terminalId, out var e))
            {
                e.Subscription.Dispose();
                _ = SafeSend(MessageTypes.TerminalClosed, new TerminalClosedPayload { TerminalId = terminalId, Reason = "session closed" });
            }
        }
        session.Closed += OnClosed;

        _terminals[terminalId] = new Entry(session, subscription, OnClosed);

        await _send(MessageTypes.TerminalOpened, new TerminalOpenedPayload
        {
            TerminalId = terminalId,
            Host = session.HostName,
            SessionId = session.Id
        });
    }

    /// <summary>The terminals currently open on this connection: (terminalId, session).</summary>
    public IReadOnlyList<(string TerminalId, SshLiveSession Session)> List()
        => _terminals.Select(kv => (kv.Key, kv.Value.Session)).ToList();

    /// <summary>Writes human keystrokes into the session's pty.</summary>
    public void Input(string terminalId, string data)
    {
        if (_terminals.TryGetValue(terminalId, out var e)) e.Session.Write(data);
    }

    /// <summary>Resize is accepted but not applied — SSH.NET's ShellStream has no post-open resize,
    /// and one session may have several differently-sized viewers anyway. Kept for contract stability.</summary>
    public void Resize(string terminalId, int cols, int rows) { /* no-op */ }

    /// <summary>Detaches one terminal from its session (the session stays alive) and tells the client.</summary>
    public async Task DetachAsync(string terminalId)
    {
        if (!_terminals.TryRemove(terminalId, out var e)) return;
        e.Session.Closed -= e.OnClosed;
        e.Subscription.Dispose();
        await SafeSend(MessageTypes.TerminalClosed, new TerminalClosedPayload { TerminalId = terminalId, Reason = "detached" });
    }

    private async Task SafeSend(string type, object payload)
    {
        try { await _send(type, payload); } catch { /* connection gone */ }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var id in _terminals.Keys.ToList())
        {
            if (_terminals.TryRemove(id, out var e))
            {
                e.Session.Closed -= e.OnClosed;
                e.Subscription.Dispose();
            }
        }
        await ValueTask.CompletedTask;
    }
}
