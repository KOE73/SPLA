using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Secrets;
using SPLA.Plugins.Ssh;
using SPLA.Service.Contracts;

namespace SPLA.Service;

/// <summary>
/// Owns the live xterm terminals of one <see cref="ClientConnection"/> (phase B of the live console).
/// Each terminal bridges a browser xterm to an SSH pty via <see cref="SshTerminalStream"/>: a
/// background pump forwards raw pty output to the client as <c>terminal.data</c>, and human keystrokes
/// arrive as <c>terminal.input</c> and go straight into the pty. Per-connection and disposed with the
/// connection, so a dropped socket tears down every SSH session it opened.
///
/// <para>No read-only guard here, by design: this is the human's own supervised session (they typed
/// it, they watch it) — the guard exists to stop the <em>agent</em> mutating a host, not the operator.</para>
/// </summary>
internal sealed class SshTerminalManager : IAsyncDisposable
{
    private readonly Func<string, object, Task> _send;   // (messageType, payload) → send on this connection
    private readonly ILogger _log;
    private readonly ConcurrentDictionary<string, Entry> _terminals = new();

    private sealed record Entry(SshTerminalStream Stream, CancellationTokenSource Cts, Task Pump);

    public SshTerminalManager(Func<string, object, Task> send, ILogger log)
    {
        _send = send;
        _log = log;
    }

    /// <summary>Opens a terminal to a resolved host. Replaces any terminal already using this id.</summary>
    public async Task OpenAsync(string terminalId, string hostName, SshHostConfig cfg, int timeoutSeconds,
        ISecretResolver resolver, int cols, int rows, CancellationToken ct)
    {
        await CloseAsync(terminalId); // idempotent re-open

        var stream = await SshTerminalStream.OpenAsync(
            cfg, timeoutSeconds, (uint)Math.Clamp(cols, 20, 500), (uint)Math.Clamp(rows, 5, 200), resolver, ct);

        var cts = new CancellationTokenSource();
        var pump = PumpAsync(terminalId, stream, cts.Token);
        _terminals[terminalId] = new Entry(stream, cts, pump);

        await _send(MessageTypes.TerminalOpened, new TerminalOpenedPayload { TerminalId = terminalId, Host = hostName });
    }

    private async Task PumpAsync(string terminalId, SshTerminalStream stream, CancellationToken ct)
    {
        try
        {
            await stream.PumpAsync(
                chunk =>
                {
                    // Fire-and-forget send; ordering preserved because SendAsync serialises on the
                    // connection's send lock.
                    _ = _send(MessageTypes.TerminalData, new TerminalDataPayload { TerminalId = terminalId, Data = chunk });
                },
                ct);
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "Terminal {TerminalId} pump ended with error", terminalId);
        }
        finally
        {
            if (_terminals.TryRemove(terminalId, out var e)) e.Stream.Dispose();
            if (!ct.IsCancellationRequested)
                await SafeSend(MessageTypes.TerminalClosed, new TerminalClosedPayload { TerminalId = terminalId, Reason = "connection closed" });
        }
    }

    /// <summary>Writes human keystrokes into the pty.</summary>
    public void Input(string terminalId, string data)
    {
        if (_terminals.TryGetValue(terminalId, out var e)) e.Stream.Write(data);
    }

    /// <summary>Resize is accepted but not yet applied — SSH.NET's ShellStream has no post-open resize.
    /// Recorded as a no-op so the client contract is stable and a future PTY-resize can slot in here.</summary>
    public void Resize(string terminalId, int cols, int rows) { /* no-op for now */ }

    /// <summary>Closes one terminal and tells the client.</summary>
    public async Task CloseAsync(string terminalId)
    {
        if (!_terminals.TryRemove(terminalId, out var e)) return;
        e.Cts.Cancel();
        e.Stream.Dispose();
        e.Cts.Dispose();
        await SafeSend(MessageTypes.TerminalClosed, new TerminalClosedPayload { TerminalId = terminalId, Reason = "closed" });
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
                e.Cts.Cancel();
                e.Stream.Dispose();
                e.Cts.Dispose();
            }
        }
        await ValueTask.CompletedTask;
    }
}
