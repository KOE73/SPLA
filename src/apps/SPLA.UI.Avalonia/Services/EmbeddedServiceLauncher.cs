using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.Services;

/// <summary>
/// Spins up the SPLA service as a child <c>SPLA.CLI serve</c> process bound to loopback, so the
/// Avalonia app can act as a thin client: it talks to the service over a WebSocket exactly like a
/// browser would. Running the service out-of-process (rather than referencing it in-process) keeps
/// the desktop assembly free of the ASP.NET dependency and avoids two agent stacks fighting over the
/// same files. The child is killed when this launcher is disposed.
/// <para>
/// This is the "embedded = client spins up its own service" case from the architecture: the very
/// same thing a remote connection does, except the service lives next door on 127.0.0.1.
/// </para>
/// </summary>
public sealed class EmbeddedServiceLauncher : IDisposable
{
    private Process? _process;

    public string? Url { get; private set; }

    /// <summary>
    /// Resolves a service to talk to and returns its base URL. Universal local-or-remote:
    /// when <paramref name="remoteUrl"/> is given, connects to that existing (possibly remote) service
    /// after verifying it is healthy — no child process is spawned. Otherwise starts a local child
    /// <c>SPLA.CLI serve</c>, which binds an ephemeral loopback port itself and reports it back on
    /// stdout. Either way the client then talks the same WebSocket protocol against the returned URL.
    /// </summary>
    /// <param name="noProject">True when the shell found no manifest to work in. The child is then
    /// launched with an explicit <c>--init=inherit</c>: a headless service cannot stop and ask what a
    /// manifest-less folder should become, and refusing to start would leave the window empty.</param>
    public async Task<string> StartAsync(
        string? remoteUrl = null, string? workingDirectory = null, bool noProject = false, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(remoteUrl))
        {
            var normalized = remoteUrl.TrimEnd('/');
            await WaitForHealthAsync(normalized, ct);
            Url = normalized;
            return normalized;
        }

        var (exe, args) = ResolveCliInvocation(noProject);

        var psi = new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = false,
            CreateNoWindow = true,
            // Captured only so a child that dies on startup can say why. Without this its output goes
            // to a console the desktop app does not have, and the single most common failure on a
            // fresh machine — SPLA.CLI.exe is framework-dependent, so "You must install .NET" — reached
            // the user as a bare 30-second health timeout with no cause attached.
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        _process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start SPLA.CLI serve.");

        var output = new StringBuilder();
        // The child now binds an ephemeral port (no more --port from here — see ResolveCliInvocation),
        // so the port it actually got is only known once Kestrel has bound it, which the child reports
        // on its own stdout ("SPLA service listening on <url>  (WebSocket: ...)"). Watch for that line
        // instead of probing a port we picked ourselves — the old pre-flight probe-then-launch approach
        // was inherently racy (something else could grab the port between the probe and the bind).
        var listeningUrl = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _process.OutputDataReceived += (_, e) => { Append(output, e.Data); TryCaptureUrl(e.Data, listeningUrl); };
        _process.ErrorDataReceived += (_, e) => Append(output, e.Data);
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        var url = await WaitForListeningUrlAsync(listeningUrl.Task, _process, exe, output, ct);
        await WaitForHealthAsync(url, ct, _process, exe, output);
        Url = url;
        return url;
    }

    private static readonly Regex ListeningUrlPattern =
        new(@"listening on (?<url>\S+)", RegexOptions.Compiled);

    private static void TryCaptureUrl(string? line, TaskCompletionSource<string> sink)
    {
        if (string.IsNullOrWhiteSpace(line) || sink.Task.IsCompleted) return;
        var match = ListeningUrlPattern.Match(line);
        if (match.Success) sink.TrySetResult(match.Groups["url"].Value);
    }

    /// <summary>Waits for the child to announce its bound URL on stdout, or reports why it can't: the
    /// child exited before printing one, or the wait timed out. Same 120s budget as the health check
    /// below, since both are waiting out the same worst-case cold start (self-contained exe unpacking
    /// under antivirus scanning, plugin folder load, etc).</summary>
    private static async Task<string> WaitForListeningUrlAsync(
        Task<string> listeningUrl, Process child, string exe, StringBuilder output, CancellationToken ct)
    {
        var timeout = Task.Delay(TimeSpan.FromSeconds(120), ct);
        var exited = WaitForExitAsync(child, ct);
        var completed = await Task.WhenAny(listeningUrl, timeout, exited);

        if (completed == listeningUrl) return await listeningUrl;
        if (completed == exited) throw new InvalidOperationException(DescribeDeadChild(child, exe, output));
        throw new TimeoutException(
            $"SPLA service did not report its listening URL within 120s, and its process is still running." + Tail(output));
    }

    private static async Task WaitForExitAsync(Process child, CancellationToken ct)
    {
        while (!child.HasExited)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(300, ct);
        }
    }

    /// <summary>Finds the CLI: a published SPLA.CLI.exe next to us, or the dll run via dotnet, or a
    /// dev-tree build output. Returns the executable and the argument list for <c>serve</c>.</summary>
    private static (string Exe, string[] Args) ResolveCliInvocation(bool noProject = false)
    {
        var baseDir = AppContext.BaseDirectory;
        // No --port: the child binds an ephemeral port (the CLI's own default) and this launcher reads
        // the actual address back off its stdout — see StartAsync.
        string[] serveArgs = noProject
            ? ["--init=inherit", "serve", "--bind", "127.0.0.1"]
            : ["serve", "--bind", "127.0.0.1"];

        // 1) Published: SPLA.CLI.exe sits next to the UI exe.
        var exe = Path.Combine(baseDir, "SPLA.CLI.exe");
        if (File.Exists(exe)) return (exe, serveArgs);

        // 2) A SPLA.CLI.dll next to us (framework-dependent layout) → run via dotnet.
        var dllHere = Path.Combine(baseDir, "SPLA.CLI.dll");
        if (File.Exists(dllHere)) return ("dotnet", Prepend(dllHere, serveArgs));

        // 3) Dev tree: walk up to the repo root and find SPLA.CLI/bin/<cfg>/<tfm>/SPLA.CLI.dll.
        var devDll = FindDevCliDll(baseDir);
        if (devDll != null) return ("dotnet", Prepend(devDll, serveArgs));

        throw new FileNotFoundException(
            "Could not locate SPLA.CLI (serve host). Looked next to the app and in the dev build tree.");
    }

    private static string? FindDevCliDll(string fromDir)
    {
        var (configuration, targetFramework) = GetBuildFlavor(fromDir);
        var dir = new DirectoryInfo(fromDir);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "SPLA.CLI", "bin");
            if (!Directory.Exists(candidate)) continue;

            // Keep the desktop shell and its child service on the same build flavor. Returning the
            // first recursive match was filesystem-order dependent and a Release UI regularly
            // launched an older Debug CLI, including a stale embedded web bundle.
            if (configuration is not null && targetFramework is not null)
            {
                var matchingDll = Path.Combine(candidate, configuration, targetFramework, "SPLA.CLI.dll");
                return File.Exists(matchingDll) ? matchingDll : null;
            }

            return Directory.EnumerateFiles(candidate, "SPLA.CLI.dll", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }
        return null;
    }

    private static (string? Configuration, string? TargetFramework) GetBuildFlavor(string baseDir)
    {
        var dir = new DirectoryInfo(baseDir);
        for (var i = 0; i < 4 && dir?.Parent?.Parent is not null; i++, dir = dir.Parent)
        {
            if (dir.Parent.Parent.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
                return (dir.Parent.Name, dir.Name);
        }
        return (null, null);
    }

    private static string[] Prepend(string first, string[] rest)
    {
        var arr = new string[rest.Length + 1];
        arr[0] = first;
        Array.Copy(rest, 0, arr, 1, rest.Length);
        return arr;
    }

    private static void Append(StringBuilder sink, string? line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        lock (sink)
        {
            // Enough to carry a host error; not enough for a chatty service to grow without bound.
            if (sink.Length < 4000) sink.AppendLine(line.Trim());
        }
    }

    /// <summary>
    /// Polls /health until it answers. <paramref name="child"/>, when given, turns "the service is not
    /// up yet" into "the service is gone": a child that has already exited will never answer, so waiting
    /// out the remaining timeout only delays the report and throws away the reason, which is sitting in
    /// its exit code and its output.
    /// </summary>
    private static async Task WaitForHealthAsync(
        string url, CancellationToken ct, Process? child = null, string? exe = null, StringBuilder? output = null)
    {
        // 30s was chosen against a warm dev tree and is not a safe budget for a first run from a zip:
        // a self-contained single-file exe unpacks itself, an antivirus scans every byte of it while it
        // does, and the plugin folder is loaded before the listener opens. Waiting longer costs nothing
        // now that a child which has actually died is reported the moment it dies, instead of being
        // indistinguishable from one that is merely slow.
        var budget = child is null ? TimeSpan.FromSeconds(30) : TimeSpan.FromSeconds(120);
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

            if (child is { HasExited: true })
                throw new InvalidOperationException(DescribeDeadChild(child, exe, output));

            await Task.Delay(300, ct);
        }
        throw new TimeoutException(
            $"SPLA service did not become healthy at {url} within {budget.TotalSeconds:0}s, and its process is still running." + Tail(output));
    }

    private static string DescribeDeadChild(Process child, string? exe, StringBuilder? output)
    {
        var name = Path.GetFileName(exe) ?? "the SPLA service";
        var code = child.ExitCode;
        var text = $"{name} exited with code {code} ({unchecked((uint)code):X8}) before the service came up.";

        // 0x80008096 = FrameworkMissingFailure from the apphost. The desktop app is published
        // self-contained and the CLI is not, so this is exactly the machine where the app window opens
        // and nothing behind it works — worth naming the missing piece rather than the number.
        if (unchecked((uint)code) == 0x80008096)
            text += " The .NET 10 runtime it needs is not installed — SPLA needs the ASP.NET Core Runtime 10.0 (x64), not only the .NET Desktop Runtime.";

        return text + Tail(output);
    }

    private static string Tail(StringBuilder? output)
    {
        if (output is null) return string.Empty;
        lock (output)
        {
            var text = output.ToString().Trim();
            return text.Length == 0 ? string.Empty : $"\n\n{text}";
        }
    }

    public void Dispose()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
                _process.WaitForExit(2000);
            }
        }
        catch { /* best effort */ }
        _process?.Dispose();
        _process = null;
    }
}
