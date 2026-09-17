using System.Reflection;
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
///
/// <para>Because completion is marker-based, anything that stops the marker from printing wedges the
/// session — so both known causes are handled here rather than left to the caller's discipline:
/// interactive pagers are disabled at open (<see cref="PagerSuppression"/>), and a Ctrl+C — which
/// makes bash discard the marker along with the rest of the line — must go through
/// <see cref="InterruptAsync"/>, which settles the pending run instead of orphaning it.</para>
/// </summary>
public sealed class SshLiveSession : IDisposable
{
    private const int ReplayCapacity = 32 * 1024;

    /// <summary>How much of the raw stream one run keeps for marker detection. Generous next to a
    /// 40-character marker and tiny next to what the buffer it replaced used to reach.</summary>
    private const int RecentCapacity = 8 * 1024;

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

    /// <summary>Every token this class types into the shell for its own bookkeeping — end markers and
    /// shell probes, in both their echoed (<c>…$?</c>) and executed (<c>…0</c>) form. Stripped globally,
    /// not just per-run: a marker orphaned by an interrupt must not surface inside a LATER command's
    /// output, which is exactly the kind of ghost that makes an agent misread what it is looking at.</summary>
    private static readonly Regex SplaNoiseRx = new(
        @"(?:;\s*)?(?:echo\s+)?__SPLA_(?:END|PROBE)_[0-9a-f]{8}__(?:\$\?|-?\d+)?",
        RegexOptions.Compiled);

    /// <summary>
    /// Typed into every session before anything else runs: it neuters the interactive pagers that
    /// would otherwise swallow a command and wait for a keypress. <c>systemctl status</c>,
    /// <c>journalctl</c>, <c>git log</c> and <c>man</c> all pipe into <c>less</c> by default, and
    /// <c>less</c> never returns — so the end marker never prints and the agent sees a command that
    /// "never finishes" while a human looking at the terminal sees a blinking cursor and "lines 1-44".
    ///
    /// <para>This is deliberately enforced HERE and not asked of the agent in a prompt: a rule that
    /// must be remembered on every single command, by every model, is a rule that will be forgotten.
    /// The list of pager-using tools is open-ended; the environment closes the whole class at once.
    /// <c>LESS=FRX</c> is the belt-and-braces case — if something launches <c>less</c> anyway it now
    /// quits on short output (F), keeps colours (R) and leaves its output on screen (X).</para>
    ///
    /// <para>The line is visible to human viewers, like the end-marker echo: transparency over
    /// cosmetics — a human must be able to account for every character the agent typed.</para>
    /// </summary>
    private const string PagerSuppression =
        "export PAGER=cat SYSTEMD_PAGER=cat GIT_PAGER=cat MANPAGER=cat LESS=FRX";

    public string Id { get; }
    public string HostName { get; }
    /// <summary>"agent" or "human" — who created the session (either may use it afterwards).</summary>
    public string OpenedBy { get; }
    public DateTimeOffset OpenedAt { get; } = DateTimeOffset.UtcNow;
    public bool IsAlive => !_disposed && _client.IsConnected;

    /// <summary>The pty geometry the remote currently believes in — the open size until a viewer
    /// resizes it (see <see cref="Resize"/>).</summary>
    public uint Cols { get; private set; }
    public uint Rows { get; private set; }

    public int ViewerCount { get { lock (_sinkLock) return _sinks.Count; } }

    /// <summary>Fires once when the session ends (disposed or connection dropped).</summary>
    public event Action<SshLiveSession>? Closed;

    private SshLiveSession(string id, string hostName, string openedBy, SshClient client, ShellStream shell,
        string? sudoPassword, uint cols, uint rows)
    {
        Id = id;
        HostName = hostName;
        OpenedBy = openedBy;
        _client = client;
        _shell = shell;
        _sudoPassword = sudoPassword;
        Cols = cols;
        Rows = rows;
        _ = PumpAsync(_cts.Token);
    }

    public static async Task<SshLiveSession> OpenAsync(
        string id, string hostName, SshHostConfig cfg, int timeoutSeconds, ISecretResolver resolver,
        string openedBy, CancellationToken ct, uint cols = 120, uint rows = 30)
    {
        var client = await SshConnectionFactory.ConnectAsync(cfg, timeoutSeconds, resolver, ct);
        var (c, r) = ClampSize((int)cols, (int)rows);
        var shell = client.CreateShellStream("xterm-256color", c, r, 0, 0, 64 * 1024);
        var sudoPassword = await SshConnectionFactory.ResolveLoginPasswordAsync(cfg, resolver, ct);
        var session = new SshLiveSession(id, hostName, openedBy, client, shell, sudoPassword, c, r);
        // Before the session is handed to anyone: kill the pagers (see PagerSuppression). Queued into
        // the pty straight away — the shell consumes it as soon as it is ready, so the first real
        // command already inherits the environment.
        session.Write(PagerSuppression + "\n");
        return session;
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

    // ── Geometry ───────────────────────────────────────────────────────────────

    /// <summary>Sane pty bounds; the open size and every later resize pass through here.</summary>
    private static (uint Cols, uint Rows) ClampSize(int cols, int rows)
        => ((uint)Math.Clamp(cols, 20, 500), (uint)Math.Clamp(rows, 5, 200));

    /// <summary>
    /// The <c>window-change</c> channel request, reached by reflection because SSH.NET exposes no
    /// public way to resize a <see cref="ShellStream"/> after <c>CreateShellStream</c> fixed its
    /// geometry. The protocol message and the code that sends it both exist — the channel is simply
    /// stored in a private field behind an internal interface.
    ///
    /// <para>Skipping the resize is not a cosmetic compromise, which is why it is worth reaching for:
    /// the remote's <c>COLUMNS</c> stays at the open size forever, so readline wraps and repositions
    /// the cursor at the wrong column (arrow keys and history smear over the previous line), and
    /// full-screen programs — far2l, htop, vim — paint into the opening rectangle and leave the rest
    /// of the window dead. Nothing the client can send as ordinary input fixes that: the size lives in
    /// the pty on the far side.</para>
    ///
    /// <para>Null if a future SSH.NET renames either member — then <see cref="Resize"/> degrades to
    /// the old no-op instead of throwing, and <see cref="SupportsResize"/> says so.</para>
    /// </summary>
    private static readonly FieldInfo? ChannelField =
        typeof(ShellStream).GetField("_channel", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly MethodInfo? WindowChangeMethod =
        ChannelField?.FieldType.GetMethod("SendWindowChangeRequest",
            BindingFlags.Public | BindingFlags.Instance);

    /// <summary>False when this SSH.NET build no longer matches <see cref="WindowChangeMethod"/>'s
    /// assumptions — resizes are then accepted and dropped, as they were before.</summary>
    public static bool SupportsResize => ChannelField != null && WindowChangeMethod != null;

    /// <summary>
    /// Tells the remote pty the window is now <paramref name="cols"/>×<paramref name="rows"/>, so the
    /// shell reflows and full-screen programs repaint at the new size. A session may have several
    /// viewers of different sizes — LAST RESIZE WINS, which is what a human resizing their own window
    /// expects; other viewers reflow on their next own resize.
    /// </summary>
    /// <returns>True when the remote was actually told (false = unchanged, dead, or unsupported).</returns>
    public bool Resize(int cols, int rows)
    {
        if (_disposed || !SupportsResize) return false;
        var (c, r) = ClampSize(cols, rows);
        lock (_sinkLock)
        {
            if (c == Cols && r == Rows) return false;
        }
        try
        {
            var channel = ChannelField!.GetValue(_shell);
            if (channel == null) return false;
            WindowChangeMethod!.Invoke(channel, [c, r, 0u, 0u]);
        }
        catch { return false; } // channel closed mid-flight — the pump will end the session
        AgentRun? run;
        lock (_sinkLock) { Cols = c; Rows = r; run = _run; }
        // The agent's screen has to follow the pty it is emulating, or every line the remote wraps
        // at the new width gets folded at the old one.
        if (run != null) lock (run.Screen) run.Screen.Resize((int)c, (int)r);
        return true;
    }

    // ── Agent command execution ────────────────────────────────────────────────
    //
    // Model: ExecAsync STARTS a command and waits up to a timeout. If the marker hasn't arrived the
    // command KEEPS RUNNING as the session's pending run — the tool returns "running" with the output
    // so far, and later WaitAsync calls continue reading from the same cursor (nothing between calls
    // is lost; output keeps accumulating in the run buffer). A dropped connection is a RESULT
    // ("disconnected"), never a hang. See ../../../../docs/adr/ADR_20260715_plugins_ssh-live-session.md.

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

        /// <summary>What the agent actually reads: the pty stream PAINTED, not accumulated. See
        /// <see cref="TerminalScreen"/> for why the difference is the whole point. Guarded by
        /// lock(Screen), which is also the lock every other field here is read under.</summary>
        public required TerminalScreen Screen { get; init; }

        /// <summary>A small rolling window of the RAW stream, kept for one job only: spotting the end
        /// marker. Deliberately not the screen — a marker echoed at the moment a program repaints
        /// could be overwritten before anyone looked, and a run whose completion is missed hangs the
        /// session forever. Bounded, because the unbounded version of this field is what made a
        /// ten-minute docker pull cost half a megabyte of memory per session.</summary>
        public readonly StringBuilder Recent = new();

        /// <summary>Absolute index of the first settled line the agent has NOT been shown
        /// (see <see cref="TerminalScreen.SettledCount"/>).</summary>
        public int Cursor;

        /// <summary>Set when output arrived since the last look, so a waiting loop re-runs an
        /// <c>until</c> pattern only when there is something new to run it against.</summary>
        public volatile bool Dirty;
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
                    $"a command is still running here: '{_run.Command}'. Pick ONE: ssh_session_wait to keep " +
                    "waiting; ssh_session_send with ctrl_c to interrupt it (that also frees the session); " +
                    "ssh_session_send with q if a pager or other full-screen program has the terminal; " +
                    "ssh_session_exec again with force=true to abandon it and run anyway; or ssh_session_exec " +
                    "with a different 'host'/'session' to work in a second session. Do NOT simply retry the " +
                    "same call — it will fail identically.");
            var marker = "__SPLA_END_" + Guid.NewGuid().ToString("N")[..8] + "__";
            run = new AgentRun
            {
                Command = command,
                Marker = marker,
                MarkerRx = new Regex(Regex.Escape(marker) + @"(-?\d+)", RegexOptions.Compiled),
                EchoTail = "; echo " + marker + "$?",
                Screen = new TerminalScreen((int)Cols, (int)Rows),
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

    /// <summary>
    /// Ctrl+C as a COMPLETE operation: interrupt, verify the shell came back, and settle the pending
    /// run — the three things that have to happen together.
    ///
    /// <para>Why this can't be a bare <see cref="Write"/> of <c>\x03</c>: on SIGINT bash discards the
    /// rest of the command line, so the <c>; echo MARKER$?</c> tail never runs and the end marker
    /// never prints. A run whose completion is detected only by that marker therefore stays pending
    /// FOREVER — every later <see cref="ExecAsync"/> refuses with "a command is still running" even
    /// though the shell is sitting at a fresh prompt. The session is wedged until it is closed.
    /// (<see cref="ExecAsync"/>'s own interrupt-on-timeout path always knew this and finished the run
    /// by hand; the externally driven Ctrl+C did not, which is the bug this method exists to kill.)</para>
    ///
    /// <para>Verification is a probe: <c>echo PROBE$?</c> typed after the interrupt. If its OUTPUT
    /// comes back, the shell owns the terminal again and the run is genuinely over. If it doesn't,
    /// something interactive still holds the tty (a pager, top, vim, an installer) — the run stays
    /// pending and the caller is told to send <c>q</c> instead of pretending the interrupt worked.</para>
    /// </summary>
    /// <returns>Status: <c>done</c> (it had actually finished on its own, ExitCode set),
    /// <c>interrupted</c> (killed, shell is back), <c>still-busy</c> (a full-screen program has the
    /// terminal — send <c>q</c>), <c>idle</c> (nothing was running; shell is free),
    /// <c>disconnected</c>.</returns>
    public async Task<AgentRunResult> InterruptAsync(CancellationToken ct)
    {
        AgentRun? pending;
        lock (_sinkLock) pending = _run;

        Write("\x03");
        await Task.Delay(200, ct); // let the signal land and bash print its new prompt
        var shellIsBack = await ProbeShellAsync(TimeSpan.FromSeconds(3), ct);

        if (!IsAlive)
        {
            var dropped = pending == null ? "" : TakeOutput(pending, holdback: false);
            if (pending != null) FinishRun(pending);
            return new AgentRunResult(dropped, null, "disconnected");
        }
        if (pending == null)
            return new AgentRunResult("", null, shellIsBack ? "idle" : "still-busy");

        // The command may have completed on its own in the moment before the Ctrl+C landed — then the
        // marker DID arrive and we have a real exit code. Prefer that over reporting an interrupt.
        var exit = pending.Done.Task.IsCompleted ? pending.Done.Task.Result : null;
        var output = TakeOutput(pending, holdback: false);

        if (!shellIsBack && exit == null)
            return new AgentRunResult(output, null, "still-busy"); // keep it pending — nothing was freed

        FinishRun(pending);
        return new AgentRunResult(output, exit, exit != null ? "done" : "interrupted");
    }

    /// <summary>Last-resort escape hatch: forgets the pending run WITHOUT touching the remote, so the
    /// session accepts commands again. The remote command may well still be running — its leftover
    /// output lands in whatever runs next (its marker is stripped by <see cref="SplaNoiseRx"/>, so it
    /// cannot be mistaken for a result). Returns true when something was actually abandoned.</summary>
    public bool ForceReleaseRun()
    {
        AgentRun? run;
        lock (_sinkLock) { run = _run; _run = null; }
        if (run == null) return false;
        run.Sub?.Dispose();
        run.OnChunk = null;
        return true;
    }

    /// <summary>Asks the shell to prove it is at a prompt by echoing a one-off token. Only the
    /// EXECUTED form matches (<c>PROBE0</c>) — the pty's echo of the typed line ends in a literal
    /// <c>$?</c>, so seeing our own keystrokes can never be mistaken for an answer.</summary>
    private async Task<bool> ProbeShellAsync(TimeSpan timeout, CancellationToken ct)
    {
        if (!IsAlive) return false;
        var probe = "__SPLA_PROBE_" + Guid.NewGuid().ToString("N")[..8] + "__";
        var rx = new Regex(Regex.Escape(probe) + @"(-?\d+)");
        var seen = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var buf = new StringBuilder();
        using var sub = Subscribe(chunk =>
        {
            lock (buf)
            {
                buf.Append(chunk);
                if (rx.IsMatch(buf.ToString())) seen.TrySetResult(true);
            }
        }, withReplay: false);

        Write("echo " + probe + "$?\n");

        using var timer = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var delay = Task.Delay(timeout, timer.Token);
        var winner = await Task.WhenAny(seen.Task, delay);
        timer.Cancel(); // don't leave the loser pending
        return winner == seen.Task;
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
            run = _run ?? new AgentRun
            {
                Command = "(watch)",
                Screen = new TerminalScreen((int)Cols, (int)Rows)
            };
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
        string recent, cursorLine;
        lock (run.Screen)
        {
            run.Screen.Feed(chunk);
            run.Recent.Append(chunk);
            if (run.Recent.Length > RecentCapacity)
                run.Recent.Remove(0, run.Recent.Length - RecentCapacity);
            recent = run.Recent.ToString();
            cursorLine = run.Screen.CursorLine;
            run.Dirty = true;
        }
        if (run.MarkerRx != null)
        {
            var m = run.MarkerRx.Match(recent);
            if (m.Success)
            {
                run.Done.TrySetResult(int.TryParse(m.Groups[1].Value, out var c) ? c : null);
                return;
            }
        }
        // A password prompt is the LINE THE CURSOR SITS ON with no newline after it — which is what
        // the screen gives directly. The old test ran over the whole accumulated stream, where the
        // "waiting right now" part of the question had to be reconstructed from an end-anchored
        // regex over text that had long since scrolled past.
        if (!run.SudoAnswered && _sudoPassword != null && SudoPromptRx.IsMatch(CleanFor(run, cursorLine)))
        {
            // sudo (or su) is waiting for the login password — answer with the stored
            // credential so agent commands don't stall. The pty doesn't echo it back.
            run.SudoAnswered = true;
            Write(_sudoPassword + "\n");
        }
        // The progress tick is the state, not the delta: a chunk carrying three repaints of one
        // line has nothing sensible to show, and the line under the cursor always does.
        if (cursorLine.Length > 0) run.OnChunk?.Invoke(CleanFor(run, cursorLine));
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
            if (until != null && run.Dirty)
            {
                string snapshot;
                lock (run.Screen) { snapshot = run.Screen.Text(); run.Dirty = false; }
                // Matching on the SCREEN, not the stream: a pattern a human sees on the terminal is
                // the one that should fire, and a pattern split by a repaint in the raw bytes never
                // would. Re-run only on new output — this used to rebuild and re-scan the entire
                // accumulated stream ten times a second, which on a long job is a growing string
                // copied 600 times a minute.
                if (until.IsMatch(CleanFor(run, snapshot)))
                    return new AgentRunResult(TakeOutput(run, holdback: false), null, "matched");
            }
            if (DateTimeOffset.UtcNow >= deadline)
                // A passive watch has nothing to "still be running" — say so plainly ("quiet") rather
                // than reporting a running command that does not exist.
                return new AgentRunResult(TakeOutput(run, holdback: true), null,
                    run.Marker == null ? "quiet" : "running");
            await Task.Delay(100, ct);
        }
    }

    /// <summary>
    /// What the agent has not seen yet: the lines that have SETTLED since its cursor, plus the live
    /// rectangle — the part of the screen still being painted.
    ///
    /// <para>Only the settled lines advance the cursor. The live frame is deliberately re-sent on
    /// every look, and that is not duplication: a line is live precisely because it is still
    /// changing, so what comes back next time is its new content, not a repeat. It costs at most one
    /// screenful and it is the only way "what is the terminal showing right now" survives a call
    /// boundary.</para>
    ///
    /// <para><paramref name="holdback"/> suppresses the live frame's LAST line while the run is
    /// unfinished, which is where a half-printed end marker would otherwise sit. The old character
    /// arithmetic that hunted for a partial marker across a byte slice is gone with the byte slice
    /// itself: on a screen the unfinished line is simply the one under the cursor.</para>
    /// </summary>
    private string TakeOutput(AgentRun run, bool holdback)
    {
        var lines = new List<string>();
        lock (run.Screen)
        {
            var dropped = run.Screen.DroppedLines;
            if (run.Cursor < dropped)
            {
                lines.Add($"…[{dropped - run.Cursor} earlier lines scrolled out of this session's buffer]");
                run.Cursor = dropped;
            }
            lines.AddRange(run.Screen.SettledFrom(run.Cursor));
            run.Cursor = run.Screen.SettledCount;

            var live = run.Screen.Live;
            var take = holdback && live.Count > 0 ? live.Count - 1 : live.Count;
            for (var i = 0; i < take; i++) lines.Add(live[i]);
            run.Dirty = false;
        }
        return CleanFor(run, string.Join("\n", lines)).TrimEnd('\r', '\n');
    }

    private void FinishRun(AgentRun run)
    {
        run.Sub?.Dispose();
        run.OnChunk = null;
        lock (_sinkLock) { if (ReferenceEquals(_run, run)) _run = null; }
    }

    private string CleanFor(AgentRun run, string s)
    {
        // The screen has already executed and consumed every sequence it understands; this catches
        // the residue of the ones it does not, and still does the whole job for the raw window.
        s = AnsiRx.Replace(s, "");
        if (run.Marker != null) s = s.Replace(run.EchoTail, "");
        // Global, not just this run's marker: probes and markers orphaned by an interrupt can print
        // long after their run ended, and must never be readable as part of some later result.
        return SplaNoiseRx.Replace(s, "");
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
