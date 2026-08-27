using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Project;
using SPLA.Platform;

namespace SPLA.Instances;

/// <summary>
/// Starts an agent by launching <c>SPLA.CLI serve</c> on the project — the hub's own binary in the
/// role it already has for everything else.
///
/// <para><b>Why launch rather than host.</b> A hub that ran agents in-process would stop being an
/// index the moment the first project misbehaved: one bad plugin load, one runaway turn, and the
/// machine's whole view of what is running goes down with it. Separate processes keep the failure
/// where it belongs, and it is the same arrangement the desktop shell already uses.</para>
///
/// <para><b>The child is not owned.</b> It is started detached and registers itself, exactly like one
/// started by hand — the hub does not keep the handle, does not kill it, and learns it exists the same
/// way it learns about everything else. Started with an idle timeout, so an agent brought up for a
/// script and then forgotten does not sit there forever.</para>
/// </summary>
public sealed class CliInstanceSpawner : IInstanceSpawner
{
    private readonly string? _hubUrl;
    private readonly ILogger _log;
    private readonly TimeSpan _idleTimeout;

    /// <param name="hubUrl">Where the child should register. Passing it is what makes a spawned agent
    /// visible, so a hub always does. Null is for the other caller — <c>spla start</c> on a machine
    /// with no hub running: the agent still starts, still writes its lock, and is still found by
    /// <c>spla ps</c> through that lock. Only the hub's view is missing, which is the same trade the
    /// registry has always offered.</param>
    /// <param name="idleTimeout">How long a spawned agent stays with no clients and nothing running.
    /// Defaults to fifteen minutes: longer than the desktop's five, because nobody is holding a window
    /// open on this one and a script may well come back to it.</param>
    public CliInstanceSpawner(string? hubUrl, ILogger log, TimeSpan? idleTimeout = null)
    {
        _hubUrl = string.IsNullOrWhiteSpace(hubUrl) ? null : hubUrl.TrimEnd('/');
        _log = log;
        _idleTimeout = idleTimeout ?? TimeSpan.FromMinutes(15);
    }

    public Task<SpawnResult> StartAsync(string projectId, bool enableMcp = false, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            return Task.FromResult(new SpawnResult(false, "No project given."));

        if (!File.Exists(projectId))
            return Task.FromResult(new SpawnResult(false, $"No manifest at '{projectId}'."));

        // Somebody may already hold it — the lock is the authority on that, and starting a second
        // writer is refused by the project anyway. Answering honestly here turns a confusing failure
        // deep in the child into a plain "already running".
        var existing = InstanceLock.Read(Path.Combine(
            Path.GetDirectoryName(projectId)!, ".spla"));
        if (existing is { Pid: > 0 } && existing.IsLocal && IsAlive(existing.Pid))
            return Task.FromResult(new SpawnResult(false, null, AlreadyRunning: true));

        try
        {
            var (exe, baseArgs) = SelfInvocationLauncher.Resolve("SPLA.CLI.exe");
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                // The child resolves the project from its working directory, the same way a person
                // running `spla serve` in that folder would.
                WorkingDirectory = Path.GetDirectoryName(projectId)!
            };
            foreach (var a in baseArgs) psi.ArgumentList.Add(a);

            string[] serveArgs =
            [
                "serve", "--bind", "127.0.0.1",
                "--idle-timeout", ((int)_idleTimeout.TotalMinutes).ToString(),
                .. enableMcp ? ["--mcp"] : Array.Empty<string>(),
                .. _hubUrl is null ? Array.Empty<string>() : ["--registry", _hubUrl]
            ];
            foreach (var a in serveArgs) psi.ArgumentList.Add(a);

            var child = Process.Start(psi);
            if (child is null) return Task.FromResult(new SpawnResult(false, "Process.Start returned nothing."));

            _log.LogInformation(
                "Started an agent for {Project} (pid {Pid}) at the hub's request.", projectId, child.Id);
            return Task.FromResult(new SpawnResult(true));
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Could not start an agent for {Project}.", projectId);
            return Task.FromResult(new SpawnResult(false, ex.Message));
        }
    }

    /// <summary>A pid alone does not prove life — the OS reuses numbers — but a pid that is *gone*
    /// does prove death, which is the only direction this needs. A stale lock therefore reads as "not
    /// running" and the project can be started again.</summary>
    private static bool IsAlive(int pid)
    {
        try
        {
            using var p = Process.GetProcessById(pid);
            return !p.HasExited;
        }
        catch (ArgumentException) { return false; }
        catch { return true; }
    }
}
