using System;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Host;

/// <summary>How a <see cref="IShell"/> run stood when control came back.</summary>
public enum ShellStatus
{
    /// <summary>The process finished; <see cref="ShellResult.ExitCode"/> is real and the session is gone.</summary>
    Exited,

    /// <summary>Still alive and, judging by the shape of its output, asking something: the tail has
    /// no line break, which is how a prompt like <c>Overwrite? [y/N] </c> leaves the cursor.</summary>
    WaitingForInput,

    /// <summary>Still alive and simply quiet — working, not asking. Control came back only so the
    /// caller is not blind to a long command.</summary>
    Running
}

/// <summary>A command to run through <see cref="IShell"/>.</summary>
/// <param name="Command">The command line to execute.</param>
/// <param name="WorkingDirectory">Working directory, or <c>null</c> for the host default.</param>
/// <param name="CodePage">Console code page for native output (default 65001 = UTF-8).</param>
/// <param name="PromptIdle">How long a prompt-shaped tail may sit unchanged before the run comes
/// back as <see cref="ShellStatus.WaitingForInput"/>. <c>null</c> = host default (short).</param>
/// <param name="SilentIdle">How long the command may produce nothing at all before the run comes
/// back as <see cref="ShellStatus.Running"/>. <c>null</c> = host default; pass
/// <see cref="Timeout.InfiniteTimeSpan"/> for commands that legitimately go quiet for minutes
/// (builds, test runs) but never ask anything.</param>
public sealed record ShellCommand(
    string Command,
    string? WorkingDirectory = null,
    int CodePage = 65001,
    TimeSpan? PromptIdle = null,
    TimeSpan? SilentIdle = null);

/// <summary>
/// The result of a <see cref="IShell"/> run — deliberately NOT terminal. A command that asked a
/// question is neither finished nor lost: it comes back with <see cref="Status"/> other than
/// <see cref="ShellStatus.Exited"/>, whatever it printed so far, and a <see cref="SessionId"/> to
/// answer through.
/// </summary>
/// <param name="ExitCode">The process exit code, or -1 while it has not exited.</param>
/// <param name="StandardOutput">Output produced since the previous return, not since the start.</param>
/// <param name="StandardError">Error output produced since the previous return.</param>
/// <param name="Status">Whether the process finished, is asking, or is merely quiet.</param>
/// <param name="SessionId">Handle for <see cref="IShell.ResumeAsync"/> / <see cref="IShell.KillAsync"/>,
/// or <c>null</c> once the process has exited.</param>
public sealed record ShellResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    ShellStatus Status = ShellStatus.Exited,
    string? SessionId = null);

/// <summary>
/// Arbitrary code execution — the one capability a workspace can't fence in, so it is a distinct
/// member of <see cref="ISandbox"/>. A scenario that forbids execution supplies
/// <see cref="ISandbox.Shell"/> = <c>null</c> rather than a throwing implementation.
/// </summary>
/// <remarks>
/// The interface describes an interactive session rather than a fire-and-forget run, because a
/// command that blocks on stdin is otherwise invisible: its question sits in a pipe nobody reads
/// until a process exit that will never come. See
/// <c>docs/adr/ADR_20260824_core_interactive-shell.md</c>.
/// </remarks>
public interface IShell
{
    /// <summary>Starts a command and returns when it exits, asks something, or goes quiet past the
    /// idle thresholds — whichever happens first.</summary>
    Task<ShellResult> RunAsync(ShellCommand command, CancellationToken ct = default);

    /// <summary>Hands <paramref name="input"/> to a waiting session's stdin (a line break is added)
    /// and waits again on the same terms. <paramref name="input"/> = <c>null</c> means "just keep
    /// waiting" — nothing is written.</summary>
    Task<ShellResult> ResumeAsync(string sessionId, string? input, CancellationToken ct = default);

    /// <summary>Kills a session's whole process tree and drops it. Returns whatever it had printed
    /// since the previous return, with <see cref="ShellStatus.Exited"/>.</summary>
    Task<ShellResult> KillAsync(string sessionId, CancellationToken ct = default);
}
