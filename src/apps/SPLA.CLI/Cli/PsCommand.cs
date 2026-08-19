using SPLA.CLI.Wire;
using SPLA.Domain.Project;
using SPLA.Service.Contracts;

namespace SPLA.CLI;

/// <summary>
/// <c>spla ps</c> — list the SPLA instances currently running, across every project this machine
/// knows about.
///
/// <para>Recognised ahead of the command parser, same as <see cref="InitCommand"/>: this has to work
/// from anywhere (it is not "about" the directory the shell happens to be in) and must not open or
/// lock any project — it only reads lock files and known-project registry entries, both of which are
/// safe to look at without becoming a writer.</para>
/// </summary>
internal static class PsCommand
{
    public static bool IsPsCommand(string[] args)
        => args.Length > 0 && args[0].Equals("ps", StringComparison.OrdinalIgnoreCase);

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Any(a => a is "--help" or "-h"))
        {
            PrintHelp();
            return 0;
        }

        var provider = new LocalProjectProvider();
        var rows = new List<(string Name, InstanceInfo Info)>();

        foreach (var project in provider.List())
        {
            if (project.ManifestPath is not { } manifest) continue;
            var dir = Path.GetDirectoryName(manifest);
            if (dir is null) continue;

            // Liveness is the OS's answer, not ours: Read() returns the lock only while the holder
            // still has the file open, so a leftover from a process that died reads back as null
            // here without anybody having to check a pid.
            var info = InstanceLock.Read(Path.Combine(dir, ".spla"));
            if (info is null) continue;

            rows.Add((project.Name ?? Path.GetFileNameWithoutExtension(manifest), info));
        }

        if (rows.Count == 0)
        {
            Console.WriteLine("No SPLA instances running.");
            return 0;
        }

        Console.WriteLine($"{"PROJECT",-24} {"MODE",-9} {"STATE",-11} {"PID",-8} {"ENDPOINT",-28} STARTED");
        foreach (var (name, info) in rows)
        {
            var state = await ResolveStateAsync(info);
            var started = info.StartedAt.LocalDateTime.ToString("g");
            Console.WriteLine(
                $"{Truncate(name, 24),-24} {Truncate(info.Mode, 9),-9} {state,-11} {info.Pid,-8} {Truncate(info.Endpoint ?? "-", 28),-28} {started}");
        }

        return 0;
    }

    /// <summary>
    /// Asks the instance what it is doing, when there is somewhere to ask — a lock with no endpoint is
    /// a REPL or an <c>mcp</c> session, which claims the project without ever offering an address, so
    /// there is nobody on the other end of a socket to ask.
    /// </summary>
    private static async Task<string> ResolveStateAsync(InstanceInfo info)
    {
        if (info.Endpoint is not { Length: > 0 }) return "-";

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await using var client = await CliWireClient.ConnectAsync(info.Endpoint, null, cts.Token);
            var status = await client.RequestInstanceStatusAsync(MessageTypes.InstanceStatus, null, cts.Token);
            return status?.State ?? "unreachable";
        }
        catch
        {
            // The lock says somebody holds it, but nobody answered — a stale lock over SMB, a firewall,
            // a process wedged past even a socket accept. "unreachable" is InstanceState's own name for
            // exactly this: a claim the observer could not confirm.
            return "unreachable";
        }
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: spla ps");
        Console.WriteLine();
        Console.WriteLine("Lists every known project that currently has a live instance, with its mode,");
        Console.WriteLine("state (asked over the wire when the instance is serving), pid, endpoint and");
        Console.WriteLine("start time. An instance holding a project without serving (a REPL or an mcp");
        Console.WriteLine("session) shows state '-': there is no address to ask.");
    }
}
