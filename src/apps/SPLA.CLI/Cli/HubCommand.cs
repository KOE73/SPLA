using System.ComponentModel;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Instances;
using SPLA.Service;
using Spectre.Console.Cli;

namespace SPLA.CLI;

internal sealed class HubSettings : CommandSettings
{
    [CommandOption("--port")]
    [Description("Port to bind. Default 5060.")]
    public int Port { get; init; } = 5060;

    [CommandOption("--bind")]
    [Description("Address to bind. Default 127.0.0.1. Binding wider needs --token.")]
    public string Bind { get; init; } = "127.0.0.1";

    [CommandOption("--token")]
    [Description("Secret reference (secret:user:registry) or a literal.")]
    public string? Token { get; init; }
}

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
internal sealed class HubCommand : AsyncCommand<HubSettings>
{
    private readonly ILoggerFactory _loggers;

    public static bool IsHubCommand(string[] args)
        => args.Length > 0 && args[0].Equals("hub", StringComparison.OrdinalIgnoreCase);

    public HubCommand(ILoggerFactory loggers)
    {
        _loggers = loggers;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, HubSettings settings, CancellationToken cancellationToken)
    {
        // Resolved through the machine-level store, because a hub has no project to resolve one
        // against — which is exactly why the ADR put this token in the user scope.
        var token = ConfigLoader.LoadAndResolve().SecretResolver.Resolve(settings.Token);

        // Built against the address about to be bound rather than the one reported afterwards: those
        // differ only for port 0, which a hub never uses — being findable without being told is its
        // whole job. The children it starts are handed this address so they register back here.
        var spawner = new CliInstanceSpawner(
            $"http://{(settings.Bind == "0.0.0.0" ? "127.0.0.1" : settings.Bind)}:{settings.Port}",
            _loggers.CreateLogger<CliInstanceSpawner>());

        var host = RegistryHubHost.Build(settings.Bind, settings.Port, token, _loggers, spawner);
        await host.StartAsync();

        Console.WriteLine($"\nSPLA registry hub listening on {host.Url}");
        Console.WriteLine(
            $"  instances register on {RegistryRoutes.Channel}; observers read {RegistryRoutes.Instances} " +
            $"or watch {RegistryRoutes.Watch}");
        if (token is null && settings.Bind != "127.0.0.1")
            Console.WriteLine(
                "WARNING: bound beyond loopback without --token — anyone who can reach this port can " +
                "list your agents, ask them to stop, and start new ones.");

        host.Hub.Changed += () => Console.WriteLine($"[hub] {host.Hub.List().Count} instance(s) registered.");

        var stop = new TaskCompletionSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.TrySetResult(); };
        Console.WriteLine("Press Ctrl+C to stop.");
        await stop.Task;

        await host.StopAsync();
        return 0;
    }
}
