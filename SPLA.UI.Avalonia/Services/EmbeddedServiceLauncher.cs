using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
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

    /// <summary>Starts the child service on a free loopback port and waits until it answers /health.</summary>
    public async Task<string> StartAsync(string? workingDirectory = null, CancellationToken ct = default)
    {
        var port = FreeLoopbackPort();
        var (exe, args) = ResolveCliInvocation(port);

        var psi = new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        _process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start SPLA.CLI serve.");

        var url = $"http://127.0.0.1:{port}";
        await WaitForHealthAsync(url, ct);
        Url = url;
        return url;
    }

    private static int FreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>Finds the CLI: a published SPLA.CLI.exe next to us, or the dll run via dotnet, or a
    /// dev-tree build output. Returns the executable and the argument list for <c>serve</c>.</summary>
    private static (string Exe, string[] Args) ResolveCliInvocation(int port)
    {
        var baseDir = AppContext.BaseDirectory;
        string[] serveArgs = { "serve", "--port", port.ToString(), "--bind", "127.0.0.1" };

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
        var dir = new DirectoryInfo(fromDir);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "SPLA.CLI", "bin");
            if (Directory.Exists(candidate))
            {
                foreach (var dll in Directory.EnumerateFiles(candidate, "SPLA.CLI.dll", SearchOption.AllDirectories))
                    return dll;
            }
        }
        return null;
    }

    private static string[] Prepend(string first, string[] rest)
    {
        var arr = new string[rest.Length + 1];
        arr[0] = first;
        Array.Copy(rest, 0, arr, 1, rest.Length);
        return arr;
    }

    private static async Task WaitForHealthAsync(string url, CancellationToken ct)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var resp = await http.GetAsync(url + "/health", ct);
                if (resp.IsSuccessStatusCode) return;
            }
            catch { /* not up yet */ }
            await Task.Delay(300, ct);
        }
        throw new TimeoutException($"SPLA service did not become healthy at {url} within 30s.");
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
