using SPLA.CLI.Wire;
using SPLA.Domain.Project;
using SPLA.Registry;

namespace SPLA.CLI;

/// <summary>
/// <c>spla ps</c> — list the SPLA instances currently running, across every project this machine
/// knows about.
///
/// <para>Recognised ahead of the command parser, same as <see cref="InitCommand"/>: this has to work
/// from anywhere (it is not "about" the directory the shell happens to be in) and must not open or
/// lock any project — it only reads lock files and asks over the wire, both safe to do without
/// becoming a writer.</para>
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

        var registry = new FileInstanceRegistry(probe: new WireInstanceProbe(TimeSpan.FromSeconds(5)));
        var running = await registry.ListAsync();

        if (running.Count == 0)
        {
            Console.WriteLine("No SPLA instances running.");
            return 0;
        }

        Console.WriteLine($"{"PROJECT",-24} {"MODE",-9} {"STATE",-12} {"CLIENTS",-8} {"PID",-8} {"ENDPOINT",-28} STARTED");
        foreach (var r in running)
        {
            // An instance that is not serving was never asked, so it has no state to print and no
            // client count either — a dash says "not asked", which is not the same as "nothing there".
            var state = r.IsServing ? InstanceStates.Name(r.State) : "-";
            var clients = r.Clients?.ToString() ?? "-";
            Console.WriteLine(
                $"{Truncate(r.ProjectName ?? "?", 24),-24} {Truncate(r.Info.Mode, 9),-9} {state,-12} " +
                $"{clients,-8} {r.Info.Pid,-8} {Truncate(r.Info.Endpoint ?? "-", 28),-28} " +
                $"{r.Info.StartedAt.LocalDateTime:g}");
        }

        return 0;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: spla ps");
        Console.WriteLine();
        Console.WriteLine("Lists every known project that currently has a live instance, with its mode,");
        Console.WriteLine("state (asked over the wire when the instance is serving), connected clients,");
        Console.WriteLine("pid, endpoint and start time. An instance holding a project without serving");
        Console.WriteLine("(a REPL or an mcp session) shows '-': there is no address to ask.");
        Console.WriteLine();
        Console.WriteLine("CLIENTS counts every client connected at the moment of asking, including this");
        Console.WriteLine("command's own connection — an otherwise unattended instance reads as 1 here.");
    }
}
