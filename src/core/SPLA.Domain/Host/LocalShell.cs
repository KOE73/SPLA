using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Host;

/// <summary>
/// Passthrough shell: runs commands through PowerShell on the host. On a server this is replaced by
/// an OS-isolated shell — or by <c>null</c> in <see cref="ISandbox.Shell"/> to forbid execution
/// entirely.
/// </summary>
/// <remarks>
/// Output is pumped incrementally rather than read to the end, because the point of the pump is to
/// have the text <em>before</em> the process exits: a command blocked on <c>Overwrite? [y/N]</c>
/// never exits, so <c>ReadToEndAsync</c> would hold its question forever. stdin stays redirected and
/// open so that question can actually be answered.
/// See <c>docs/adr/ADR_20260824_core_interactive-shell.md</c>.
/// </remarks>
public sealed class LocalShell : IShell, IDisposable
{
    /// <summary>A prompt-shaped tail this old means the command is asking, not working.</summary>
    private static readonly TimeSpan DefaultPromptIdle = TimeSpan.FromSeconds(2);

    /// <summary>Total silence this long returns control so a long command is not a black box, unless
    /// overridden — see <see cref="DefaultSilentIdle"/> and <c>ResolvedSettings.ShellTimeoutSeconds</c>,
    /// which is how a project configures this.</summary>
    private static readonly TimeSpan HardcodedSilentIdle = TimeSpan.FromSeconds(120);

    /// <summary>Guard against a model that never closes what it opens; each live session is a process.</summary>
    private const int MaxLiveSessions = 16;

    private readonly ConcurrentDictionary<string, Session> _sessions = new(StringComparer.Ordinal);
    private int _nextSessionId;

    /// <summary>Falls back to this instead of the hardcoded 120s default when a
    /// <see cref="ShellCommand"/> doesn't specify its own — the configured project timeout. Settable
    /// (not just constructor-supplied) so a live settings change applies to commands started after
    /// it, without needing a fresh <see cref="LocalShell"/> instance. Set to
    /// <see cref="Timeout.InfiniteTimeSpan"/> to disable the check entirely.</summary>
    public TimeSpan DefaultSilentIdle { get; set; }

    public LocalShell() : this(null) { }

    public LocalShell(TimeSpan? defaultSilentIdle) => DefaultSilentIdle = defaultSilentIdle ?? HardcodedSilentIdle;

    public async Task<ShellResult> RunAsync(ShellCommand command, CancellationToken ct = default)
    {
        if (_sessions.Count >= MaxLiveSessions)
        {
            throw new InvalidOperationException(
                $"Too many live shell sessions ({MaxLiveSessions}). Finish or kill one before starting another.");
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding(command.CodePage);
        var encodedScript = Convert.ToBase64String(
            Encoding.Unicode.GetBytes(BuildPowerShellScript(command.Command, command.CodePage)));

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encodedScript}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            // Redirected AND left open: closing it would unblock the process too, but by feeding it
            // EOF instead of the answer — which is not the same thing as answering.
            RedirectStandardInput = true,
            StandardOutputEncoding = encoding,
            StandardErrorEncoding = encoding,
            StandardInputEncoding = encoding,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = string.IsNullOrEmpty(command.WorkingDirectory)
                ? Directory.GetCurrentDirectory()
                : command.WorkingDirectory
        };
        psi.Environment["PYTHONIOENCODING"] = encoding.WebName;
        psi.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";

        var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Could not start process.");

        var id = "sh_" + Interlocked.Increment(ref _nextSessionId);
        var session = new Session(
            id,
            process,
            command.PromptIdle ?? DefaultPromptIdle,
            command.SilentIdle ?? DefaultSilentIdle);
        _sessions[id] = session;

        return await WaitAsync(session, forceSilentThreshold: false, ct);
    }

    public async Task<ShellResult> ResumeAsync(string sessionId, string? input, CancellationToken ct = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"No live shell session '{sessionId}'.");

        if (session.Process.HasExited) return Finish(session);

        if (input is not null)
        {
            try
            {
                await session.Process.StandardInput.WriteLineAsync(input.AsMemory(), ct);
                await session.Process.StandardInput.FlushAsync(ct);
            }
            catch (IOException)
            {
                // The process died between our check and the write — nothing to answer any more.
                return Finish(session);
            }

            // The question has been answered, so its shape must stop describing the session:
            // otherwise the tail left by the old prompt would make the next quiet stretch look
            // like a fresh question.
            session.ClearPromptShape();
        }

        // "Resume without input" is an explicit request to keep waiting, so honouring the short
        // prompt threshold would just bounce the same answer straight back.
        return await WaitAsync(session, forceSilentThreshold: input is null, ct);
    }

    public Task<ShellResult> KillAsync(string sessionId, CancellationToken ct = default)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
            throw new InvalidOperationException($"No live shell session '{sessionId}'.");

        TryKillTree(session.Process);
        return Task.FromResult(Finish(session));
    }

    /// <summary>
    /// Watches one session until it exits, asks something, or goes quiet past its threshold.
    /// </summary>
    private async Task<ShellResult> WaitAsync(Session session, bool forceSilentThreshold, CancellationToken ct)
    {
        // Idle is measured from the moment someone starts waiting, not from the last byte ever
        // printed: on resume that byte is the question itself, already seconds old, which would
        // time out instantly and report "still waiting" before the process could react.
        session.MarkActivity();

        try
        {
            while (true)
            {
                if (session.Process.HasExited)
                {
                    await session.DrainAsync();
                    return Finish(session);
                }

                var isPrompt = !forceSilentThreshold && session.LooksLikePrompt;
                var threshold = isPrompt ? session.PromptIdle : session.SilentIdle;

                if (threshold != Timeout.InfiniteTimeSpan && session.IdleFor >= threshold)
                {
                    var (output, error) = session.TakeBuffered();
                    return new ShellResult(
                        ExitCode: -1,
                        StandardOutput: output,
                        StandardError: error,
                        Status: isPrompt ? ShellStatus.WaitingForInput : ShellStatus.Running,
                        SessionId: session.Id);
                }

                await Task.Delay(100, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelling the turn must not leave an orphaned PowerShell — and whatever long-running
            // command it spawned (a build, a scan) — running on the host. Kill the whole tree.
            TryKillTree(session.Process);
            _sessions.TryRemove(session.Id, out _);
            session.Dispose();
            throw;
        }
    }

    /// <summary>Closes a session out: last of its output, real exit code, gone from the registry.</summary>
    private ShellResult Finish(Session session)
    {
        _sessions.TryRemove(session.Id, out _);
        var (output, error) = session.TakeBuffered();
        var exitCode = session.ExitCodeOrDefault();
        session.Dispose();
        return new ShellResult(exitCode, output, error, ShellStatus.Exited);
    }

    public void Dispose()
    {
        foreach (var session in _sessions.Values)
        {
            TryKillTree(session.Process);
            session.Dispose();
        }

        _sessions.Clear();
    }

    private static void TryKillTree(Process process)
    {
        try
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Already exited, or we lost the race / lack access — nothing more we can do here.
        }
    }

    private static string BuildPowerShellScript(string command, int codePage)
    {
        var escapedCommand = command.Replace("'", "''");
        var script = new StringBuilder();
        script.AppendLine("try {");
        script.AppendLine($"    [Console]::InputEncoding = [System.Text.Encoding]::GetEncoding({codePage})");
        script.AppendLine($"    [Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding({codePage})");
        script.AppendLine($"    $OutputEncoding = [System.Text.Encoding]::GetEncoding({codePage})");
        script.AppendLine("    if ($IsWindows -or $env:OS -eq 'Windows_NT') {");
        script.AppendLine($"        chcp.com {codePage} > $null");
        script.AppendLine("    }");
        script.AppendLine($"    Invoke-Expression '{escapedCommand}'");
        script.AppendLine("    exit $LASTEXITCODE");
        script.AppendLine("} catch {");
        script.AppendLine("    [Console]::Error.WriteLine($_.Exception.Message)");
        script.AppendLine("    exit 1");
        script.AppendLine("}");
        return script.ToString();
    }

    /// <summary>
    /// One live command: its process, the text it has produced since the caller last looked, and
    /// when that text last changed.
    /// </summary>
    private sealed class Session : IDisposable
    {
        private readonly StringBuilder _output = new();
        private readonly StringBuilder _error = new();
        private readonly Lock _gate = new();
        private readonly CancellationTokenSource _pumpCts = new();
        private readonly Task _outPump;
        private readonly Task _errPump;

        private long _lastOutputTicks = DateTime.UtcNow.Ticks;

        /// <summary>Last character ever written, kept across drains — the buffer itself is emptied
        /// on every return, so it cannot answer "did this end mid-line?" on the next round.</summary>
        private char _lastChar;

        public Session(string id, Process process, TimeSpan promptIdle, TimeSpan silentIdle)
        {
            Id = id;
            Process = process;
            PromptIdle = promptIdle;
            SilentIdle = silentIdle;

            // Pumps belong to the session, not to one RunAsync call: the caller comes and goes
            // while the process keeps printing.
            _outPump = PumpAsync(process.StandardOutput, _output);
            _errPump = PumpAsync(process.StandardError, _error);
        }

        public string Id { get; }
        public Process Process { get; }
        public TimeSpan PromptIdle { get; }
        public TimeSpan SilentIdle { get; }

        public TimeSpan IdleFor =>
            TimeSpan.FromTicks(DateTime.UtcNow.Ticks - Interlocked.Read(ref _lastOutputTicks));

        /// <summary>A tail with no line break is how a question leaves the cursor sitting after it.</summary>
        public bool LooksLikePrompt
        {
            get
            {
                lock (_gate) return _lastChar is not ('\0' or '\n' or '\r');
            }
        }

        /// <summary>Restarts the idle clock — the caller has just done something.</summary>
        public void MarkActivity() => Interlocked.Exchange(ref _lastOutputTicks, DateTime.UtcNow.Ticks);

        /// <summary>Forgets the mid-line tail, so an answered prompt stops looking like a pending one.</summary>
        public void ClearPromptShape()
        {
            lock (_gate) _lastChar = '\0';
        }

        private async Task PumpAsync(StreamReader reader, StringBuilder sink)
        {
            var buffer = new char[4096];
            try
            {
                int count;
                while ((count = await reader.ReadAsync(buffer.AsMemory(), _pumpCts.Token)) > 0)
                {
                    lock (_gate)
                    {
                        sink.Append(buffer, 0, count);
                        _lastChar = buffer[count - 1];
                    }

                    Interlocked.Exchange(ref _lastOutputTicks, DateTime.UtcNow.Ticks);
                }
            }
            catch (OperationCanceledException) { /* session torn down */ }
            catch (ObjectDisposedException) { /* process disposed under us */ }
            catch (IOException) { /* pipe broken by a killed process */ }
        }

        /// <summary>Lets the pumps reach EOF after the process exits, so the tail is not lost.</summary>
        public async Task DrainAsync()
            => await Task.WhenAny(Task.WhenAll(_outPump, _errPump), Task.Delay(TimeSpan.FromSeconds(2)));

        /// <summary>Takes everything buffered and empties the buffers, so each return is a delta
        /// rather than the whole transcript re-read from the top.</summary>
        public (string Output, string Error) TakeBuffered()
        {
            lock (_gate)
            {
                var output = _output.ToString();
                var error = _error.ToString();
                _output.Clear();
                _error.Clear();
                return (output, error);
            }
        }

        public int ExitCodeOrDefault()
        {
            try
            {
                return Process.HasExited ? Process.ExitCode : -1;
            }
            catch
            {
                return -1;
            }
        }

        public void Dispose()
        {
            try { _pumpCts.Cancel(); } catch { /* already gone */ }
            _pumpCts.Dispose();
            try { Process.Dispose(); } catch { /* already gone */ }
        }
    }
}
