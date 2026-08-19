using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Instances;
using SPLA.Service;

namespace SPLA.CLI;

/// <summary>
/// <c>spla hub</c> — run a registry hub and nothing else.
///
/// <para>Recognised ahead of the command parser, like <see cref="PsCommand"/>: a hub holds no
/// project and must not resolve or lock one. It is the same binary in a different role rather than a
/// separate program, which is the whole answer to "does this mean another executable" — a daemon
/// with its own release, its own upgrade path and its own version skew against the CLI is a cost
/// nothing here was willing to pay.</para>
///
/// <para>Instances find it because they were told its address (<c>spla serve --registry</c>), and
/// observers read it over plain HTTP. It stores nothing: an instance that outlives a hub restart
/// registers again, and losing the hub loses the view, never a project.</para>
/// </summary>
internal static class HubCommand
{
    public static bool IsHubCommand(string[] args)
        => args.Length > 0 && args[0].Equals("hub", StringComparison.OrdinalIgnoreCase);

    public static async Task<int> RunAsync(string[] args, ILoggerFactory loggers)
    {
        if (args.Any(a => a is "--help" or "-h"))
        {
            PrintHelp();
            return 0;
        }

        var bind = Option(args, "--bind") ?? "127.0.0.1";
        var port = int.TryParse(Option(args, "--port"), out var p) ? p : 5060;

        // Resolved through the machine-level store, because a hub has no project to resolve one
        // against — which is exactly why the ADR put this token in the user scope.
        var token = ConfigLoader.LoadAndResolve().SecretResolver.Resolve(Option(args, "--token"));

        var host = RegistryHubHost.Build(bind, port, token, loggers);
        await host.StartAsync();

        Console.WriteLine($"\nSPLA registry hub listening on {host.Url}");
        Console.WriteLine(
            $"  instances register on {RegistryRoutes.Channel}; observers read {RegistryRoutes.Instances} " +
            $"or watch {RegistryRoutes.Watch}");
        if (token is null && bind != "127.0.0.1")
            Console.WriteLine(
                "WARNING: bound beyond loopback without --token — anyone who can reach this port can " +
                "list your agents and ask them to stop.");

        host.Hub.Changed += () => Console.WriteLine($"[hub] {host.Hub.List().Count} instance(s) registered.");

        var stop = new TaskCompletionSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.TrySetResult(); };
        Console.WriteLine("Press Ctrl+C to stop.");
        await stop.Task;

        await host.StopAsync();
        return 0;
    }

    private static string? Option(string[] args, string name)
    {
        var i = Array.FindIndex(args, a => a.Equals(name, StringComparison.OrdinalIgnoreCase));
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: spla hub [--port N] [--bind addr] [--token <ref-or-literal>]");
        Console.WriteLine();
        Console.WriteLine("Runs a registry hub: instances register themselves with it and push what they");
        Console.WriteLine("are doing, observers read the list over HTTP. Holds no project and runs no agent.");
        Console.WriteLine();
        Console.WriteLine("  --port   Default 5060.");
        Console.WriteLine("  --bind   Default 127.0.0.1. Binding wider needs --token.");
        Console.WriteLine("  --token  Secret reference (secret:user:registry) or a literal.");
        Console.WriteLine();
        Console.WriteLine("Point instances at it with `spla serve --registry http://host:5060`,");
        Console.WriteLine("and read it with `spla ps --registry http://host:5060`.");
    }
}
