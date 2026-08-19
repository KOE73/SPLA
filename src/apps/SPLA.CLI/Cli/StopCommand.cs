using SPLA.CLI.Wire;
using SPLA.Domain.Project;
using SPLA.Service.Contracts;

namespace SPLA.CLI;

/// <summary>
/// <c>spla stop</c> — asks a running instance to shut down.
///
/// <para>Recognised ahead of the command parser for the same reason as <see cref="PsCommand"/>: it
/// must not open or lock a project. Stopping somebody else's instance by first becoming a second
/// writer to the same project would be absurd, and naming a project that is not the current
/// directory has to work from anywhere.</para>
///
/// <para>A stop is refused while a turn is running, a question is waiting for an answer, or a turn
/// stopped halfway — the states somebody walks back to their desk for. <c>--force</c> cancels the
/// work instead of asking, which is a choice the caller makes explicitly and never a default.</para>
/// </summary>
internal static class StopCommand
{
    public static bool IsStopCommand(string[] args)
        => args.Length > 0 && args[0].Equals("stop", StringComparison.OrdinalIgnoreCase);

    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Any(a => a is "--help" or "-h"))
        {
            PrintHelp();
            return 0;
        }

        var force = args.Any(a => a.Equals("--force", StringComparison.OrdinalIgnoreCase));
        var target = args.Skip(1).FirstOrDefault(a => !a.StartsWith('-'));

        var resolved = Resolve(target);
        if (resolved is null)
        {
            Console.Error.WriteLine(target is null
                ? "No SPLA instance is running for this directory."
                : $"No running instance matches '{target}'. Try `spla ps`.");
            return 1;
        }

        var (name, info) = resolved.Value;
        if (info.Endpoint is not { Length: > 0 } endpoint)
        {
            // A REPL or an `mcp` session holds the project without offering an address. There is
            // nothing to ask politely, and killing the pid would be exactly the ownership model this
            // design removed — so say who is holding it and let the person decide.
            Console.Error.WriteLine(
                $"'{name}' is held by pid {info.Pid} ({info.Mode}), which is not serving — nothing to ask. " +
                "Stop it where it runs.");
            return 1;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await using var client = await CliWireClient.ConnectAsync(endpoint, null, cts.Token);
            var status = await client.RequestInstanceStatusAsync(
                MessageTypes.InstanceStop, new InstanceStopPayload { Force = force }, cts.Token);

            if (status is null)
            {
                Console.Error.WriteLine($"'{name}' did not answer the stop request.");
                return 1;
            }

            if (status.Stopping)
            {
                Console.WriteLine($"'{name}' is stopping.");
                return 0;
            }

            Console.Error.WriteLine(
                $"'{name}' refused to stop: {status.Refusal ?? status.State}. Use --force to cancel it anyway.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not reach '{name}' at {endpoint}: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Finds the instance to stop: the one holding the current directory when no target is named,
    /// otherwise a known project matching by name or by manifest path. Only ever reads lock files —
    /// see the class remarks.
    /// </summary>
    private static (string Name, InstanceInfo Info)? Resolve(string? target)
    {
        if (target is null)
        {
            var here = InstanceLock.Read(Path.Combine(Directory.GetCurrentDirectory(), ".spla"));
            return here is null ? null : (Path.GetFileName(Directory.GetCurrentDirectory()), here);
        }

        foreach (var project in new LocalProjectProvider().List())
        {
            if (project.ManifestPath is not { } manifest) continue;

            var name = project.Name ?? Path.GetFileNameWithoutExtension(manifest);
            var matches =
                name.Equals(target, StringComparison.OrdinalIgnoreCase) ||
                manifest.Equals(target, StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileNameWithoutExtension(manifest).Equals(target, StringComparison.OrdinalIgnoreCase);
            if (!matches) continue;

            var dir = Path.GetDirectoryName(manifest);
            if (dir is null) continue;

            var info = InstanceLock.Read(Path.Combine(dir, ".spla"));
            if (info is not null) return (name, info);
        }

        return null;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: spla stop [project] [--force]");
        Console.WriteLine();
        Console.WriteLine("Asks a running instance to shut down. With no argument, the instance holding");
        Console.WriteLine("the current directory; otherwise a known project matched by name or manifest path.");
        Console.WriteLine();
        Console.WriteLine("  --force   Cancel a running turn instead of refusing. Loses work in progress.");
        Console.WriteLine();
        Console.WriteLine("A stop is refused while a turn is running, a question is waiting for an answer,");
        Console.WriteLine("or a turn stopped halfway. Use `spla ps` to see what is running.");
    }
}
