using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Domain.Host;

/// <summary>A command to run through <see cref="IShell"/>.</summary>
/// <param name="Command">The command line to execute.</param>
/// <param name="WorkingDirectory">Working directory, or <c>null</c> for the host default.</param>
/// <param name="CodePage">Console code page for native output (default 65001 = UTF-8).</param>
public sealed record ShellCommand(string Command, string? WorkingDirectory = null, int CodePage = 65001);

/// <summary>The result of a <see cref="IShell"/> run.</summary>
public sealed record ShellResult(int ExitCode, string StandardOutput, string StandardError);

/// <summary>
/// Arbitrary code execution — the one capability a workspace can't fence in, so it is a distinct
/// member of <see cref="ISandbox"/>. A scenario that forbids execution supplies
/// <see cref="ISandbox.Shell"/> = <c>null</c> rather than a throwing implementation.
/// </summary>
public interface IShell
{
    Task<ShellResult> RunAsync(ShellCommand command, CancellationToken ct = default);
}
