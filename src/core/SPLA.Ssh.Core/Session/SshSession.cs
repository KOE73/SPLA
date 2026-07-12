using System.Text;
using System.Text.RegularExpressions;
using Renci.SshNet;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// A live, persistent SSH shell session over an SSH.NET <see cref="ShellStream"/> (a real pty), as
/// opposed to the one-shot <c>ssh_run</c>. State carries over between commands — <c>cd</c>, exported
/// vars, a running interpreter — which is what makes it a "console" rather than a sequence of
/// unrelated executions.
///
/// <para>This is the reusable core: <see cref="RunAsync"/> streams output through an <c>onChunk</c>
/// callback as it arrives. The live-console tool (phase A) pipes that into <c>ProgressScope</c> so the
/// human watches output appear in real time; a future xterm terminal (phase B) will pipe the same
/// callback into the browser terminal and feed human keystrokes back in via <see cref="SendRaw"/>.
/// Nothing about command execution needs to change between the two.</para>
///
/// <para>A raw pty has no command boundary, so completion is detected with a unique end marker:
/// each command is sent as <c>{cmd}; echo {MARKER}$?</c>, and the line <c>{MARKER}&lt;exitcode&gt;</c>
/// in the output signals "done" and carries the exit status. The marker is generated per session and
/// stripped from what the caller sees.</para>
/// </summary>
internal sealed class SshSession : IDisposable
{
    private readonly SshClient _client;
    private readonly ShellStream _shell;
    private readonly string _marker;
    private readonly Regex _markerRx;      // MARKER<digits> — the completion echo (has $? expanded)
    private bool _disposed;

    // Terminal control noise from a real pty: CSI (colors, bracketed-paste ?2004h/l), OSC
    // (window-title), and stray BEL/SO/SI. ESC/BEL are held as C# escapes so the source stays pure
    // ASCII; the compiled strings match the real control chars. Stripped so output is clean text.
    private static readonly string Esc = ((char)27).ToString();
    private static readonly string Bel = ((char)7).ToString();
    private static readonly Regex AnsiRx = new(
        Esc + @"\[[0-9;?]*[ -/]*[@-~]" +                                  // CSI ... final byte
        "|" + Esc + @"\][^" + Bel + "]*(?:" + Bel + "|" + Esc + @"\\)" +  // OSC ... BEL or ST
        "|[" + Bel + "]",                                     // stray BEL / SO / SI
        RegexOptions.Compiled);

    public string HostName { get; }

    private SshSession(string hostName, SshClient client, ShellStream shell, string marker)
    {
        HostName = hostName;
        _client = client;
        _shell = shell;
        _marker = marker;
        _markerRx = new Regex(Regex.Escape(marker) + @"(-?\d+)", RegexOptions.Compiled);
    }

    /// <summary>Opens a shell on an already-connected client, drains the login banner/prompt, quiets
    /// the prompt to reduce escape noise, and returns a ready session.</summary>
    public static async Task<SshSession> OpenAsync(string hostName, SshClient client, CancellationToken ct)
    {
        var shell = client.CreateShellStream("xterm", 200, 50, 800, 600, 64 * 1024);
        var marker = "__SPLA_END_" + Guid.NewGuid().ToString("N")[..8] + "__";
        var session = new SshSession(hostName, client, shell, marker);
        // Let the login shell settle and emit its prompt/MOTD, then discard it.
        await session.DrainQuietAsync(TimeSpan.FromMilliseconds(700), TimeSpan.FromSeconds(6), ct);
        // Simplify the prompt and disable the title escape so command output stays clean. Trusted
        // setup written straight to the pty (not agent input) — not subject to the read-only guard.
        shell.Write("PROMPT_COMMAND=''; PS1='$ '\n");
        await session.DrainQuietAsync(TimeSpan.FromMilliseconds(400), TimeSpan.FromSeconds(3), ct);
        return session;
    }

    /// <summary>
    /// Runs one command line, streaming output to <paramref name="onChunk"/> as it arrives (ANSI and
    /// marker noise removed), and returns (cleanedOutput, exitCode). Exit code is null if the marker
    /// never arrived within the timeout.
    /// </summary>
    public async Task<(string Output, int? ExitCode)> RunAsync(
        string command, Action<string>? onChunk, TimeSpan timeout, CancellationToken ct)
    {
        _shell.Write(command + "; echo " + _marker + "$?\n");

        var full = new StringBuilder();
        var deadline = DateTime.UtcNow + timeout;
        int? exit = null;

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();

            var piece = _shell.Read();
            if (!string.IsNullOrEmpty(piece))
            {
                full.Append(piece);

                var m = _markerRx.Match(full.ToString());
                if (m.Success)
                {
                    exit = int.TryParse(m.Groups[1].Value, out var code) ? code : null;
                    onChunk?.Invoke(Clean(piece));
                    break;
                }

                onChunk?.Invoke(Clean(piece));
                continue;
            }

            await Task.Delay(40, ct);
        }

        return (CleanFull(full.ToString()), exit);
    }

    /// <summary>Writes raw bytes straight into the pty (future: human keystrokes from the terminal).
    /// Bypasses the read-only guard by design — the caller decides what may use it.</summary>
    public void SendRaw(string data) => _shell.Write(data);

    /// <summary>Reads until no data arrives for <paramref name="quiet"/>, or <paramref name="max"/> elapses.</summary>
    private async Task DrainQuietAsync(TimeSpan quiet, TimeSpan max, CancellationToken ct)
    {
        var hardDeadline = DateTime.UtcNow + max;
        var lastData = DateTime.UtcNow;
        while (DateTime.UtcNow < hardDeadline)
        {
            ct.ThrowIfCancellationRequested();
            var piece = _shell.Read();
            if (!string.IsNullOrEmpty(piece)) lastData = DateTime.UtcNow;
            else if (DateTime.UtcNow - lastData > quiet) break;
            else await Task.Delay(40, ct);
        }
    }

    // Remove ANSI + marker noise from a streamed increment (keep the visible command, drop the echo tail).
    private string Clean(string s)
    {
        s = AnsiRx.Replace(s, "");
        s = s.Replace("; echo " + _marker + "$?", "");
        s = _markerRx.Replace(s, "");
        return s;
    }

    // Clean the whole accumulated output for the returned string.
    private string CleanFull(string s)
    {
        s = AnsiRx.Replace(s, "");
        s = s.Replace("; echo " + _marker + "$?", "");
        s = _markerRx.Replace(s, "");
        return s.TrimEnd();
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
