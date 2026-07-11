using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.Plugins.Roslyn;

/// <summary>
/// Runs the real <c>dotnet</c> SDK CLI (build/run/test) against files on disk in the user's workspace,
/// as opposed to <see cref="CompileCheckTool"/> / <see cref="ScriptRunTool"/> which work in-process
/// against the BCL only. Shelling out is deliberate: building or running an actual project means
/// MSBuild, NuGet restore, project references and the correct target framework — none of which the
/// isolated in-memory compilation can see. Output is captured; on cancellation/timeout the whole
/// process tree is killed so a hung build never orphans on the host.
/// </summary>
internal static class DotnetCli
{
    /// <summary>Result of a finished CLI run. <see cref="TimedOut"/> distinguishes a hard timeout from a
    /// non-zero exit so the caller can word the message accordingly.</summary>
    internal readonly record struct Result(int ExitCode, string Stdout, string Stderr, bool TimedOut);

    /// <summary>
    /// Resolves a caller-supplied path against <paramref name="workspaceRoot"/> and enforces that it
    /// stays inside the workspace — the model must not build or run arbitrary paths on the host. An
    /// empty <paramref name="relativeOrAbsolute"/> resolves to the workspace root itself.
    /// </summary>
    internal static bool ResolveInWorkspace(string workspaceRoot, string? relativeOrAbsolute,
        out string fullPath, out string? error)
    {
        error = null;
        var root = Path.GetFullPath(workspaceRoot);
        var target = string.IsNullOrWhiteSpace(relativeOrAbsolute)
            ? root
            : Path.GetFullPath(Path.Combine(root, relativeOrAbsolute));

        // Containment: target must equal the root or sit under it. Compare with a trailing separator so
        // "…/workspace-evil" is not accepted as being under "…/workspace".
        var rootWithSep = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;
        if (target != root && !target.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
        {
            fullPath = target;
            error = $"path '{relativeOrAbsolute}' resolves outside the workspace ({root}); refused.";
            return false;
        }

        fullPath = target;
        return true;
    }

    /// <summary>
    /// Runs <c>dotnet <paramref name="arguments"/></c> in <paramref name="workingDirectory"/> with a hard
    /// timeout, returning captured output. Never throws for a non-zero exit — that is a normal result the
    /// caller inspects. Only a caller-initiated cancel (not the internal timeout) propagates as
    /// <see cref="OperationCanceledException"/>.
    /// </summary>
    internal static async Task<Result> RunAsync(string arguments, string workingDirectory,
        int timeoutSeconds, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };
        // Deterministic, machine-readable output regardless of the host's locale, and no first-run banner.
        psi.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";
        psi.Environment["DOTNET_NOLOGO"] = "1";
        psi.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";

        using var process = new Process { StartInfo = psi };

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        process.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

        if (!process.Start())
            throw new InvalidOperationException("Could not start 'dotnet'. Is the .NET SDK on PATH?");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
            // WaitForExitAsync returns once the process ends, but the async output/error readers may still
            // be draining; a bare WaitForExit() flushes them.
            process.WaitForExit();
            return new Result(process.ExitCode, stdout.ToString(), stderr.ToString(), TimedOut: false);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            TryKillTree(process);
            return new Result(-1, stdout.ToString(), stderr.ToString(), TimedOut: true);
        }
        catch (OperationCanceledException)
        {
            // Caller cancelled the turn — don't leave the build running on the host.
            TryKillTree(process);
            throw;
        }
    }

    private static void TryKillTree(Process process)
    {
        try
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Already exited, or we lost the race / lack access — nothing more we can do.
        }
    }
}
