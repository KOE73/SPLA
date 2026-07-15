using System.Text;
using System.Text.RegularExpressions;
using Renci.SshNet;
using SPLA.Domain.Secrets;

namespace SPLA.Plugins.Ssh;

/// <summary>
/// ONE live SSH pty session shared by every viewer and driver: the agent runs commands in it
/// (<see cref="RunAsync"/>), any number of human terminals attach to watch and type
/// (<see cref="Subscribe"/> + <see cref="Write"/>), and a replay buffer catches a late attacher up.
/// This is the phase-C unification: previously the agent's marker-based session and the human's raw
/// terminal were two separate SSH connections to the same host — now "session" is the unit and
/// terminals are views, so the human literally watches the agent type and can intervene mid-command.
///
/// <para>One central pump reads the pty and fans raw chunks (ANSI included) out to all sinks.
/// <see cref="RunAsync"/> detects command completion with a per-call end marker
/// (<c>cmd; echo MARKER$?</c>) collected through its own subscription — so agent execution and
/// human keystrokes coexist on the same stream. The marker echo is visible to human viewers by
/// design: transparency over cosmetics.</para>
/// </summary>
public sealed class SshLiveSession : IDisposable
{
    private const int ReplayCapacity = 32 * 1024;

    private readonly SshClient _client;
    private readonly ShellStream _shell;
    private readonly CancellationTokenSource _cts = new();
    private readonly object _sinkLock = new();
    private readonly Dictionary<Guid, Action<string>> _sinks = new();
    private readonly StringBuilder _replay = new();
    // Login password kept only to auto-answer sudo prompts inside RunAsync; never surfaced to
    // callers or tool output (the pty doesn't echo it). Null on key-auth hosts without a password.
    private readonly string? _sudoPassword;
    private bool _disposed;

    private static readonly Regex SudoPromptRx = new(
        @"(\[sudo\] password for [^\n:]*|^Password)\s*:\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    // Terminal control noise stripped from the agent-facing output (viewers get the raw stream).
    private static readonly string Esc = ((char)27).ToString();
    private static readonly string Bel = ((char)7).ToString();
    private static readonly Regex AnsiRx = new(
        Esc + @"\[[0-9;?]*[ -/]*[@-~]" +
        "|" + Esc + @"\][^" + Bel + "]*(?:" + Bel + "|" + Esc + @"\\)" +
        "|[" + Bel + "]",
        RegexOptions.Compiled);

    public string Id { get; }
    public string HostName { get; }
    /// <summary>"agent" or "human" — who created the session (either may use it afterwards).</summary>
    public string OpenedBy { get; }
    public DateTimeOffset OpenedAt { get; } = DateTimeOffset.UtcNow;
    public bool IsAlive => !_disposed && _client.IsConnected;

    public int ViewerCount { get { lock (_sinkLock) return _sinks.Count; } }

    /// <summary>Fires once when the session ends (disposed or connection dropped).</summary>
    public event Action<SshLiveSession>? Closed;

    private SshLiveSession(string id, string hostName, string openedBy, SshClient client, ShellStream shell,
        string? sudoPassword)
    {
        Id = id;
        HostName = hostName;
        OpenedBy = openedBy;
        _client = client;
        _shell = shell;
        _sudoPassword = sudoPassword;
        _ = PumpAsync(_cts.Token);
    }

    public static async Task<SshLiveSession> OpenAsync(
        string id, string hostName, SshHostConfig cfg, int timeoutSeconds, ISecretResolver resolver,
        string openedBy, CancellationToken ct, uint cols = 120, uint rows = 30)
    {
        var client = await SshConnectionFactory.ConnectAsync(cfg, timeoutSeconds, resolver, ct);
        var shell = client.CreateShellStream("xterm-256color",
            Math.Clamp(cols, 20, 500), Math.Clamp(rows, 5, 200), 0, 0, 64 * 1024);
        var sudoPassword = await SshConnectionFactory.ResolveLoginPasswordAsync(cfg, resolver, ct);
        return new SshLiveSession(id, hostName, openedBy, client, shell, sudoPassword);
    }

    // ── Fan-out ────────────────────────────────────────────────────────────────

    /// <summary>Attaches an output sink. When <paramref name="withReplay"/> the recent output is
    /// delivered first, so a terminal attaching to a running session shows what already happened.</summary>
    public IDisposable Subscribe(Action<string> sink, bool withReplay)
    {
        var key = Guid.NewGuid();
        lock (_sinkLock)
        {
            if (withReplay && _replay.Length > 0) sink(_replay.ToString());
            _sinks[key] = sink;
        }
        return new Unsubscriber(this, key);
    }

    private sealed class Unsubscriber(SshLiveSession owner, Guid key) : IDisposable
    {
        public void Dispose() { lock (owner._sinkLock) owner._sinks.Remove(key); }
    }

    private async Task PumpAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                string? s = null;
                try
                {
                    if (_shell.DataAvailable) s = _shell.Read();
                    else if (!_client.IsConnected) break;
                }
                catch { break; } // stream closed

                if (!string.IsNullOrEmpty(s)) Distribute(s);
                else await Task.Delay(20, ct);
            }
        }
        catch (OperationCanceledException) { /* normal teardown */ }
        finally
        {
            Dispose();
        }
    }

    private void Distribute(string chunk)
    {
        Action<string>[] sinks;
        lock (_sinkLock)
        {
            _replay.Append(chunk);
            if (_replay.Length > ReplayCapacity) _replay.Remove(0, _replay.Length - ReplayCapacity);
            sinks = _sinks.Values.ToArray();
        }
        foreach (var sink in sinks)
        {
            try { sink(chunk); } catch { /* one dead viewer must not stall the pump */ }
        }
    }

    // ── Input ──────────────────────────────────────────────────────────────────

    /// <summary>Raw input straight into the pty (human keystrokes, or agent control keys where the
    /// host policy allows). No guard here — callers enforce their own policy.</summary>
    public void Write(string data)
    {
        if (_disposed) return;
        try { _shell.Write(data); } catch { /* closed — pump ends the session */ }
    }

    // ── Agent command execution ────────────────────────────────────────────────
    //
    // Model: ExecAsync STARTS a command and waits up to a timeout. If the marker hasn't arrived the
    // command KEEPS RUNNING as the session's pending run — the tool returns "running" with the output
    // so far, and later WaitAsync calls continue reading from the same cursor (nothing between calls
    // is lost; output keeps accumulating in the run buffer). A dropped connection is a RESULT
    // ("disconnected"), never a hang. See docs/DESIGN_SSH_LiveSession_Tools.md.

    /// <summary>Terminal states: done (marker seen, ExitCode set), running (timeout, command still
    /// going), matched (a WaitAsync 'until' regex hit), interrupted (Ctrl+C sent after timeout),
    /// disconnected (connection dropped — e.g. remote reboot).</summary>
    public sealed record AgentRunResult(string NewOutput, int? ExitCode, string Status);

    private sealed class AgentRun
    {
        public required string Command { get; init; }
        /// <summary>Null for a passive watch (WaitAsync with no pending command).</summary>
        public string? Marker { get; init; }
        public Regex? MarkerRx { get; init; }
        public string EchoTail { get; init; } = "";
        public readonly StringBuilder Raw = new();
        /// <summary>Raw chars already returned to the agent; guarded by lock(Raw).</summary>
        public int Cursor;
        public readonly TaskCompletionSource<int?> Done =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool SudoAnswered; // once per run — a re-prompt means wrong password, don't loop
        public IDisposable? Sub;
        /// <summary>Live progress sink of the CURRENT tool call; re-pointed by each exec/wait so
        /// ticks never flow into a finished call's scope.</summary>
        public volatile Action<string>? OnChunk;
    }

    private AgentRun? _run;

    /// <summary>The command the agent is still running in this session, if any (shown in lists).</summary>
    public string? PendingCommand => _run?.Command;

    /// <summary>Starts a command and waits up to <paramref name="timeout"/>. On timeout the command
    /// keeps running (status "running") unless <paramref name="interruptOnTimeout"/> — then Ctrl+C
    /// is sent and the shell is given 2s to come back ("interrupted"). Throws
    /// <see cref="InvalidOperationException"/> when a previous command is still pending.</summary>
    public async Task<AgentRunResult> ExecAsync(
        string command, TimeSpan timeout, Action<string>? onChunk, bool interruptOnTimeout,
        CancellationToken ct)
    {
        AgentRun run;
        lock (_sinkLock)
        {
            if (_run != null)
                throw new InvalidOperationException(
                    $"a command is still running here: '{_run.Command}'. Use ssh_session_wait to keep " +
                    "waiting, ssh_session_send ctrl_c to interrupt, or another session.");
            var marker = "__SPLA_END_" + Guid.NewGuid().ToString("N")[..8] + "__";
            run = new AgentRun
            {
                Command = command,
                Marker = marker,
                MarkerRx = new Regex(Regex.Escape(marker) + @"(-?\d+)", RegexOptions.Compiled),
                EchoTail = "; echo " + marker + "$?",
                OnChunk = onChunk
            };
            _run = run;
        }
        run.Sub = Subscribe(chunk => OnRunChunk(run, chunk), withReplay: false);
        Write(command + run.EchoTail + "\n");

        var result = await WaitCoreAsync(run, timeout, null, ct);
        if (result.Status == "running" && interruptOnTimeout)
        {
            Write("\x03"); // Ctrl+C — bash discards the rest of the line, so the marker never prints
            var extra = await WaitCoreAsync(run, TimeSpan.FromSeconds(2), null, ct);
            FinishRun(run);
            result = new AgentRunResult(result.NewOutput + extra.NewOutput, extra.ExitCode,
                extra.Status == "done" ? "done" : "interrupted");
        }
        return result;
    }

    /// <summary>Continues reading the session from the agent's cursor: returns when the pending
    /// command completes, <paramref name="until"/> matches the accumulated output, the connection
    /// drops, or the timeout passes (status "running" — call again). Works without a pending command
    /// too (passive watch, e.g. waiting for boot messages).</summary>
    public async Task<AgentRunResult> WaitAsync(
        TimeSpan timeout, Regex? until, Action<string>? onChunk, CancellationToken ct)
    {
        AgentRun run;
        bool passive;
        lock (_sinkLock)
        {
            passive = _run == null;
            run = _run ?? new AgentRun { Command = "(watch)" };
            if (!passive) run.OnChunk = onChunk;
        }
        if (passive)
        {
            run.OnChunk = onChunk;
            run.Sub = Subscribe(chunk => OnRunChunk(run, chunk), withReplay: false);
        }
        try
        {
            return await WaitCoreAsync(run, timeout, until, ct);
        }
        finally
        {
            if (passive) run.Sub?.Dispose();
            else run.OnChunk = null; // this call's progress scope is over
        }
    }

    private void OnRunChunk(AgentRun run, string chunk)
    {
        string snapshot;
        lock (run.Raw) { run.Raw.Append(chunk); snapshot = run.Raw.ToString(); }
        if (run.MarkerRx != null)
        {
            var m = run.MarkerRx.Match(snapshot);
            if (m.Success)
            {
                run.Done.TrySetResult(int.TryParse(m.Groups[1].Value, out var c) ? c : null);
                return;
            }
        }
        if (!run.SudoAnswered && _sudoPassword != null && SudoPromptRx.IsMatch(CleanFor(run, snapshot)))
        {
            // sudo (or su) is waiting for the login password — answer with the stored
            // credential so agent commands don't stall. The pty doesn't echo it back.
            run.SudoAnswered = true;
            Write(_sudoPassword + "\n");
        }
        run.OnChunk?.Invoke(CleanFor(run, chunk));
    }

    private async Task<AgentRunResult> WaitCoreAsync(AgentRun run, TimeSpan timeout, Regex? until,
        CancellationToken ct)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (true)
        {
            if (run.Done.Task.IsCompleted)
            {
                var exit = run.Done.Task.Result;
                var output = TakeOutput(run, holdback: false);
                FinishRun(run);
                return new AgentRunResult(output, exit, "done");
            }
            if (!IsAlive)
            {
                var output = TakeOutput(run, holdback: false);
                FinishRun(run);
                return new AgentRunResult(output, null, "disconnected");
            }
            if (until != null)
            {
                string snapshot;
                lock (run.Raw) snapshot = run.Raw.ToString();
                if (until.IsMatch(CleanFor(run, snapshot)))
                    return new AgentRunResult(TakeOutput(run, holdback: false), null, "matched");
            }
            if (DateTimeOffset.UtcNow >= deadline)
                return new AgentRunResult(TakeOutput(run, holdback: true), null, "running");
            await Task.Delay(100, ct);
        }
    }

    /// <summary>Returns the raw output accumulated since the agent's cursor, cleaned, and advances
    /// the cursor. With <paramref name="holdback"/> a trailing partial marker/echo-tail is withheld
    /// (returned by the next call) so a marker split across reads never leaks half-printed.</summary>
    private string TakeOutput(AgentRun run, bool holdback)
    {
        string slice;
        lock (run.Raw)
        {
            var snapshot = run.Raw.ToString();
            var end = snapshot.Length;
            if (holdback && run.Marker != null)
            {
                var tail = run.EchoTail + "\n"; // the echoed command line contains this text
                while (end > run.Cursor && (EndsWithPrefixOf(snapshot, end, tail)
                                            || EndsWithPrefixOf(snapshot, end, run.Marker)))
                    end--;
            }
            slice = snapshot[run.Cursor..end];
            run.Cursor = end;
        }
        return CleanFor(run, slice).TrimEnd('\r', '\n');
    }

    /// <summary>True when s[..end] ends with a non-empty prefix of <paramref name="token"/> that
    /// could still be completing (i.e. the last char belongs to a partial token occurrence).</summary>
    private static bool EndsWithPrefixOf(string s, int end, string token)
    {
        var max = Math.Min(token.Length, end);
        for (var len = max; len >= 1; len--)
            if (string.CompareOrdinal(s, end - len, token, 0, len) == 0)
                return true;
        return false;
    }

    private void FinishRun(AgentRun run)
    {
        run.Sub?.Dispose();
        run.OnChunk = null;
        lock (_sinkLock) { if (ReferenceEquals(_run, run)) _run = null; }
    }

    private string CleanFor(AgentRun run, string s)
    {
        s = AnsiRx.Replace(s, "");
        if (run.Marker != null)
        {
            s = s.Replace(run.EchoTail, "");
            if (run.MarkerRx != null) s = run.MarkerRx.Replace(s, "");
        }
        return s;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _cts.Cancel(); } catch { }
        try { _shell.Dispose(); } catch { }
        try { if (_client.IsConnected) _client.Disconnect(); } catch { }
        try { _client.Dispose(); } catch { }
        _cts.Dispose();
        try { Closed?.Invoke(this); } catch { }
    }
}
