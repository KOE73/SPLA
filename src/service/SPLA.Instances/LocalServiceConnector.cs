using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using SPLA.Domain.Project;
using SPLA.Platform;

namespace SPLA.Instances;

/// <summary>
/// Join-or-start for a project's SPLA service, shared by every process that wants to talk to a live
/// instance without itself becoming a second writer.
///
/// <para><b>Why this lives here, not in the desktop assembly.</b> The join-or-start pattern was
/// written first for the Avalonia shell (<c>EmbeddedServiceLauncher</c>), and it never actually
/// depended on Avalonia — it only touches <see cref="System"/>, <see cref="System.Diagnostics"/>,
/// <see cref="System.Net.Http"/> and <see cref="SPLA.Domain.Project.InstanceLock"/>. <c>SPLA.Instances</c>
/// already sits below both the desktop app and the CLI and already references
/// <c>SPLA.Domain</c> (transitively, through <c>SPLA.Instances.Contracts</c>) and <c>SPLA.Platform</c>
/// (directly, for <see cref="SelfInvocationLauncher"/> — see <see cref="CliInstanceSpawner"/>, which
/// already resolves and launches <c>SPLA.CLI serve</c> the same way). Moving the logic here instead of
/// writing a second copy in <c>SPLA.CLI</c> means the two "am I talking to a live instance, and if not,
/// how do I get one" implementations cannot drift apart.</para>
///
/// <para><b>What stayed behind in <c>EmbeddedServiceLauncher</c>.</b> The desktop launcher's own
/// child-spawning path (<c>ResolveCliInvocation</c>) does more than this class needs — dev-tree build-
/// flavor matching so a Release window never launches a stale Debug CLI, <c>--init=inherit</c> for a
/// manifest-less folder, captured stdout tailored to the launcher's richer startup diagnostics. None of
/// that is specific to *joining*, so only the join check
/// (<see cref="TryJoinAsync"/>) was extracted; the desktop's own spawn path is untouched and still
/// spawns its own child exactly as before. This class's own <see cref="StartAsync"/> uses
/// <see cref="SelfInvocationLauncher"/> instead — the same resolver <see cref="CliInstanceSpawner"/>
/// already uses — because every caller of this class (so far, only <c>spla mcp</c>) is itself
/// <c>SPLA.CLI</c>, so "relaunch myself as serve" is exactly right and needs none of the desktop's
/// cross-assembly resolution.</para>
/// </summary>
public sealed class LocalServiceConnector : IDisposable
{
    private Process? _process;

    /// <summary>The address this connector ended up talking to, once <see cref="StartAsync"/> or a
    /// successful <see cref="TryJoinAsync"/> has run.</summary>
    public string? Url { get; private set; }

    /// <summary>
    /// The address of a live instance already holding <paramref name="workspacePath"/>, or null when
    /// nobody does (or the lock is unreadable, or the instance is unreachable, or it turns out to be a
    /// stranger reusing the port — see the checks below). Callers whose fallback is to become the
    /// writer themselves treat null as "the field is open"; callers whose fallback is to start a shared
    /// instance (this class's own <see cref="StartAsync"/>) treat it identically.
    ///
    /// <para>Health-checked before being returned, and only accepted when <c>/health</c> answers with
    /// the same instance id the lock claims: a published port outlives nothing, but a port number can
    /// be reused by something else entirely, and dialling a stranger is worse than starting fresh. This
    /// is <c>EmbeddedServiceLauncher.TryJoinRunningInstanceAsync</c> verbatim — see that class's remarks
    /// for the full reasoning ("only one process may write a project, so spawning a second here would
    /// simply be refused; joining is both the correct behaviour and the fast one").</para>
    /// </summary>
    public static async Task<string?> TryJoinAsync(string workspacePath, CancellationToken ct)
    {
        try
        {
            var info = InstanceLock.Read(Path.Combine(workspacePath, ".spla"));
            if (info?.Endpoint is not { Length: > 0 } endpoint || !info.IsLocal) return null;

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await http.GetAsync(endpoint.TrimEnd('/') + "/health", ct);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync(ct);
            return body.Contains(info.InstanceId, StringComparison.OrdinalIgnoreCase) ? endpoint.TrimEnd('/') : null;
        }
        catch
        {
            // Unreadable lock, unreachable port, malformed anything: fall through and let the caller
            // start its own.
            return null;
        }
    }

    /// <summary>
    /// Spawns <c>SPLA.CLI serve</c> as a loopback child bound to <paramref name="workspacePath"/> and
    /// waits until it is actually accepting connections, returning its base URL.
    /// </summary>
    /// <param name="idleTimeout">Passed straight through as <c>--idle-timeout</c>. The child decides
    /// for itself when it has no clients and nothing running left to do — see
    /// <c>CliInstanceSpawner</c>'s remarks on why ownership (killing the child on our own exit) is the
    /// wrong model: it would cut a turn short mid-generation. The caller picks the value because "how
    /// long is reasonable to sit idle" depends on who is asking (a script vs. a desktop window vs. an
    /// MCP session) — this class has no opinion of its own.</param>
    /// <param name="hubUrl">Machine registry hub to register the spawned instance with, or null. Purely
    /// additive, same as everywhere else this parameter appears.</param>
    /// <param name="enableMcp">Forces <c>--mcp</c> on the child regardless of what the project's own
    /// settings say. This exists because the only reason <c>spla mcp</c> ever reaches
    /// <see cref="StartAsync"/> is to serve <c>POST /mcp</c> to itself — starting a child that then
    /// refuses that exact route (because the project has <c>mcp.enabled</c> left at its default of
    /// false) would be a self-defeating join-or-start. A caller that just wants an ordinary shared
    /// instance (nothing MCP-specific) passes false.</param>
    public async Task<string> StartAsync(
        string workspacePath, TimeSpan idleTimeout, string? hubUrl, bool enableMcp, CancellationToken ct)
    {
        var (exe, baseArgs) = SelfInvocationLauncher.Resolve("SPLA.CLI.exe");

        string[] serveArgs =
        [
            "serve", "--bind", "127.0.0.1",
            "--idle-timeout", Math.Max(1, (int)idleTimeout.TotalMinutes).ToString(),
            .. hubUrl is { Length: > 0 } ? new[] { "--registry", hubUrl } : Array.Empty<string>(),
            .. enableMcp ? new[] { "--mcp" } : Array.Empty<string>()
        ];

        var psi = new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = false,
            CreateNoWindow = true,
            // Captured only so a child that dies on startup can say why — same reasoning as
            // EmbeddedServiceLauncher: without this the failure reaches the caller as a bare timeout.
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workspacePath
        };
        foreach (var a in baseArgs) psi.ArgumentList.Add(a);
        foreach (var a in serveArgs) psi.ArgumentList.Add(a);

        var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start SPLA.CLI serve.");
        _process = process;

        var output = new StringBuilder();
        // Same racing-port avoidance as EmbeddedServiceLauncher: the child binds an ephemeral port and
        // reports the actual address on its own stdout, so we watch for that line instead of guessing.
        var listeningUrl = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.OutputDataReceived += (_, e) => { Append(output, e.Data); TryCaptureUrl(e.Data, listeningUrl); };
        process.ErrorDataReceived += (_, e) => Append(output, e.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var url = await WaitForListeningUrlAsync(listeningUrl.Task, process, exe, output, ct);
        await WaitForHealthAsync(url, ct, process, exe, output);
        Url = url;
        return url;
    }

    private static readonly Regex ListeningUrlPattern = new(@"listening on (?<url>\S+)", RegexOptions.Compiled);

    private static void TryCaptureUrl(string? line, TaskCompletionSource<string> sink)
    {
        if (string.IsNullOrWhiteSpace(line) || sink.Task.IsCompleted) return;
        var match = ListeningUrlPattern.Match(line);
        if (match.Success) sink.TrySetResult(match.Groups["url"].Value);
    }

    private static async Task<string> WaitForListeningUrlAsync(
        Task<string> listeningUrl, Process child, string exe, StringBuilder output, CancellationToken ct)
    {
        var timeout = Task.Delay(TimeSpan.FromSeconds(120), ct);
        var exited = WaitForExitAsync(child, ct);
        var completed = await Task.WhenAny(listeningUrl, timeout, exited);

        if (completed == listeningUrl) return await listeningUrl;
        if (completed == exited) throw new InvalidOperationException(DescribeDeadChild(child, exe, output));
        throw new TimeoutException(
            "SPLA service did not report its listening URL within 120s, and its process is still running." + Tail(output));
    }

    private static async Task WaitForExitAsync(Process child, CancellationToken ct)
    {
        while (!child.HasExited)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(300, ct);
        }
    }

    private static async Task WaitForHealthAsync(
        string url, CancellationToken ct, Process child, string exe, StringBuilder output)
    {
        var budget = TimeSpan.FromSeconds(120);
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow + budget;
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var resp = await http.GetAsync(url + "/health", ct);
                if (resp.IsSuccessStatusCode) return;
            }
            catch { /* not up yet */ }

            if (child.HasExited) throw new InvalidOperationException(DescribeDeadChild(child, exe, output));

            await Task.Delay(300, ct);
        }
        throw new TimeoutException(
            $"SPLA service did not become healthy at {url} within {budget.TotalSeconds:0}s, and its process is still running." + Tail(output));
    }

    private static void Append(StringBuilder sink, string? line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        lock (sink)
        {
            if (sink.Length < 4000) sink.AppendLine(line.Trim());
        }
    }

    private static string DescribeDeadChild(Process child, string exe, StringBuilder output)
    {
        var name = Path.GetFileName(exe);
        var code = child.ExitCode;
        var text = $"{name} exited with code {code} ({unchecked((uint)code):X8}) before the service came up.";

        if (unchecked((uint)code) == 0x80008096)
            text += " The .NET 10 runtime it needs is not installed — SPLA needs the ASP.NET Core Runtime 10.0 (x64).";

        return text + Tail(output);
    }

    private static string Tail(StringBuilder output)
    {
        lock (output)
        {
            var text = output.ToString().Trim();
            return text.Length == 0 ? string.Empty : $"\n\n{text}";
        }
    }

    /// <summary>
    /// Lets go of an owned child without killing it — same reasoning as
    /// <c>EmbeddedServiceLauncher.Dispose</c>: the child was started with <c>--idle-timeout</c> and
    /// holds its own lease, so it reclaims itself once nothing (including us) is using it any more.
    /// A connector that only ever joined (never called <see cref="StartAsync"/>) has nothing to
    /// release here — it never owned a process in the first place.
    /// </summary>
    public void Dispose()
    {
        _process?.Dispose();
        _process = null;
    }
}
