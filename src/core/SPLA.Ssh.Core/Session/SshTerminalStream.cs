using Renci.SshNet;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// Raw bidirectional pty passthrough for the interactive xterm terminal (phase B). Unlike the
/// command-oriented <see cref="SshSession"/> (which injects end markers and strips ANSI so the agent
/// gets clean, bounded output), this streams the real shell verbatim: full ANSI/colors, the real
/// prompt, no markers — the browser terminal renders it. Human keystrokes go straight in via
/// <see cref="Write"/>. There is deliberately no read-only guard here: this is the human's own
/// supervised session, and they are the safety net (see the design notes).
/// </summary>
internal sealed class SshTerminalStream : IDisposable
{
    private readonly SshClient _client;
    private readonly ShellStream _shell;
    private bool _disposed;

    private SshTerminalStream(SshClient client, ShellStream shell)
    {
        _client = client;
        _shell = shell;
    }

    /// <summary>Connects and opens a raw interactive shell of the given size.</summary>
    public static async Task<SshTerminalStream> OpenAsync(
        SshHostConfig cfg, int timeoutSeconds, uint cols, uint rows, SPLA.Domain.Secrets.ISecretResolver resolver, CancellationToken ct)
    {
        var client = await SshConnectionFactory.ConnectAsync(cfg, timeoutSeconds, resolver, ct);
        var shell = client.CreateShellStream("xterm-256color", cols == 0 ? 120 : cols, rows == 0 ? 30 : rows, 0, 0, 64 * 1024);
        return new SshTerminalStream(client, shell);
    }

    /// <summary>Pumps shell output to <paramref name="onData"/> until cancelled or the connection drops.
    /// Data is passed through verbatim (ANSI included) for the terminal to render.</summary>
    public async Task PumpAsync(Action<string> onData, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (_shell.DataAvailable)
                {
                    var s = _shell.Read();
                    if (!string.IsNullOrEmpty(s)) onData(s);
                    continue;
                }
                if (!_client.IsConnected) break;
                await Task.Delay(20, ct);
            }
            catch (OperationCanceledException) { break; }
            catch { break; } // stream closed / disconnected — end the pump
        }
    }

    /// <summary>Writes raw bytes (human keystrokes) straight into the pty.</summary>
    public void Write(string data)
    {
        if (_disposed) return;
        try { _shell.Write(data); } catch { /* closed — pump will end */ }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _shell.Dispose(); } catch { }
        try { if (_client.IsConnected) _client.Disconnect(); } catch { }
        try { _client.Dispose(); } catch { }
    }
}
