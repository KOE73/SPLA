using System.ComponentModel;
using SPLA.CLI.Wire;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;
using SPLA.Instances;
using Spectre.Console.Cli;

namespace SPLA.CLI;

internal sealed class PsSettings : CommandSettings
{
    [CommandOption("--registry")]
    [Description("Hub URL to query instead of this machine's lock files.")]
    public string? Registry { get; init; }

    [CommandOption("--token")]
    [Description("Bearer token for the hub (takes a secret reference or literal).")]
    public string? Token { get; init; }
}

/// <summary>
/// <c>spla ps</c> — list the SPLA instances currently running, across every project this machine
/// knows about.
///
/// <para>Recognised ahead of the command parser, same as <see cref="InitCommand"/>: this has to work
/// from anywhere (it is not "about" the directory the shell happens to be in) and must not open or
/// lock any project — it only reads lock files and asks over the wire, both safe to do without
/// becoming a writer.</para>
/// </summary>
internal sealed class PsCommand : AsyncCommand<PsSettings>
{
    public static bool IsPsCommand(string[] args)
        => args.Length > 0 && args[0].Equals("ps", StringComparison.OrdinalIgnoreCase);

    protected override async Task<int> ExecuteAsync(CommandContext context, PsSettings settings, CancellationToken cancellationToken)
    {
        // Which registry answers is a launch-time choice, not a different code path: a hub when one
        // was named, this machine's lock files otherwise.
        IInstanceRegistry registry = settings.Registry is { Length: > 0 }
            ? new RemoteInstanceRegistry(settings.Registry, ConfigLoader.LoadAndResolve().SecretResolver.Resolve(settings.Token))
            : new FileInstanceRegistry(probe: new WireInstanceProbe(TimeSpan.FromSeconds(5)));

        IReadOnlyList<InstanceRecord> running;
        try
        {
            running = await registry.ListAsync();
        }
        catch (Exception ex) when (settings.Registry is { Length: > 0 })
        {
            Console.Error.WriteLine($"Could not reach the hub at {settings.Registry}: {ex.Message}");
            return 1;
        }
        finally
        {
            (registry as IDisposable)?.Dispose();
        }

        if (running.Count == 0)
        {
            Console.WriteLine("No SPLA instances running.");
            return 0;
        }

        // ROLE sits next to MODE rather than replacing it: mode is what a participant is doing with the
        // project ("serve", "repl", "ui"), role is what it *is* to the hub. A window is the case that
        // makes the difference visible — it has a mode like everything else but holds nothing.
        Console.WriteLine($"{"PROJECT",-24} {"ROLE",-7} {"MODE",-9} {"STATE",-12} {"CLIENTS",-8} {"PID",-8} {"ENDPOINT",-28} STARTED");
        foreach (var r in running)
        {
            // An instance that is not serving was never asked, so it has no state to print and no
            // client count either — a dash says "not asked", which is not the same as "nothing there".
            var state = r.IsServing ? InstanceStates.Name(r.State) : "-";
            var clients = r.Clients?.ToString() ?? "-";
            Console.WriteLine(
                $"{Truncate(r.ProjectName ?? "?", 24),-24} {Truncate(r.Role, 7),-7} " +
                $"{Truncate(r.Info.Mode, 9),-9} {state,-12} " +
                $"{clients,-8} {r.Info.Pid,-8} {Truncate(r.Info.Endpoint ?? "-", 28),-28} " +
                $"{r.Info.StartedAt.LocalDateTime:g}");
        }

        return 0;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";
}
