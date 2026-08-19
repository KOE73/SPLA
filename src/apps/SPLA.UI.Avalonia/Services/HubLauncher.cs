using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.Services;

/// <summary>
/// Finds the machine's registry hub, starting one if nobody has yet.
///
/// <para><b>Why the shell starts it but never hosts it.</b> Hosting the hub in this process would tie
/// the machine's view of what is running to whether a window happens to be open — and the whole point
/// of the instance model is that work outlives windows. A headless server keeps the same accounting
/// with no Avalonia anywhere, and <c>spla ps</c> must work with no UI at all. So the hub is the CLI
/// in a different role, and this class only makes sure one exists.</para>
///
/// <para>The port is fixed rather than ephemeral, and that is the one place a well-known number earns
/// its keep: everything else publishes its address, but the thing addresses get published *to* has to
/// be findable without having been told. A hub somebody started themselves on that port is simply
/// joined — the health probe cannot tell the difference and does not need to.</para>
/// </summary>
public sealed class HubLauncher : IDisposable
{
    /// <summary>The machine hub's conventional port. Matches <c>spla hub</c>'s own default.</summary>
    public const int DefaultPort = 5060;

    private Process? _process;

    /// <summary>Where the hub is, once resolved. Null when none could be reached or started — which
    /// is not fatal anywhere: without a hub the shell simply has no cross-project view.</summary>
    public string? Url { get; private set; }

    public async Task<string?> ResolveAsync(CancellationToken ct = default)
    {
        var url = $"http://127.0.0.1:{DefaultPort}";

        if (await IsHealthyAsync(url, ct)) return Url = url;
        if (!TryStart()) return null;

        // Give the child a short budget: a hub opens a socket and nothing else, so if it is not up in
        // a few seconds it is not coming up. Failing quietly is correct — the shell works without it.
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            if (_process is { HasExited: true }) return null;
            if (await IsHealthyAsync(url, ct)) return Url = url;

            try { await Task.Delay(250, ct); }
            catch (OperationCanceledException) { return null; }
        }
        return null;
    }

    private static async Task<bool> IsHealthyAsync(string url, CancellationToken ct)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
            var response = await http.GetAsync(url + "/health", ct);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    private bool TryStart()
    {
        try
        {
            var (exe, args) = ResolveCliInvocation();
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (var a in args) psi.ArgumentList.Add(a);

            _process = Process.Start(psi);
            return _process is not null;
        }
        catch { return false; }
    }

    /// <summary>Finds the CLI in its hub role. The dev-tree walk itself is shared with
    /// <see cref="EmbeddedServiceLauncher.FindCliDll"/>, because that one matches build flavour and a
    /// second copy would eventually launch a stale Debug binary from a Release shell.</summary>
    private static (string Exe, string[] Args) ResolveCliInvocation()
    {
        string[] hubArgs = ["hub", "--port", DefaultPort.ToString(), "--bind", "127.0.0.1"];
        var baseDir = AppContext.BaseDirectory;

        var exe = Path.Combine(baseDir, "SPLA.CLI.exe");
        if (File.Exists(exe)) return (exe, hubArgs);

        var dllHere = Path.Combine(baseDir, "SPLA.CLI.dll");
        if (File.Exists(dllHere)) return ("dotnet", ViaDotnet(dllHere, hubArgs));

        var devDll = EmbeddedServiceLauncher.FindCliDll(baseDir);
        if (devDll != null) return ("dotnet", ViaDotnet(devDll, hubArgs));

        throw new FileNotFoundException("Could not locate SPLA.CLI to run the registry hub.");
    }

    private static string[] ViaDotnet(string dll, string[] args) => [dll, .. args];

    /// <summary>
    /// Leaves the hub running.
    ///
    /// <para>Same reasoning as the service child: this window did not own it. Other windows and other
    /// <c>spla serve</c> processes may still be registered with it, and a hub that died with whichever
    /// window happened to start it would take the machine's whole view down with it. It holds nothing
    /// worth keeping, so a stale one costs a socket until the machine reboots.</para>
    /// </summary>
    public void Dispose()
    {
        _process?.Dispose();
        _process = null;
    }
}
