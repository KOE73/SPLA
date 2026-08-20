using System.ComponentModel;
using SPLA.CLI.Wire;
using SPLA.Domain.Project;
using SPLA.Instances;
using SPLA.Service.Contracts;
using Spectre.Console.Cli;

namespace SPLA.CLI;

internal sealed class StopSettings : CommandSettings
{
    [CommandArgument(0, "[project]")]
    [Description("Project name, manifest path, or nothing for the current directory.")]
    public string? Project { get; init; }

    [CommandOption("--force")]
    [Description("Cancel a running turn instead of refusing. Loses work in progress.")]
    public bool Force { get; init; }
}

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
internal sealed class StopCommand : AsyncCommand<StopSettings>
{
    public static bool IsStopCommand(string[] args)
        => args.Length > 0 && args[0].Equals("stop", StringComparison.OrdinalIgnoreCase);

    protected override async Task<int> ExecuteAsync(CommandContext context, StopSettings settings, CancellationToken cancellationToken)
    {
        var resolved = await ResolveAsync(settings.Project);
        if (resolved is null)
        {
            Console.Error.WriteLine(settings.Project is null
                ? "No SPLA instance is running for this directory."
                : $"No running instance matches '{settings.Project}'. Try `spla ps`.");
            return 1;
        }

        var name = resolved.ProjectName ?? settings.Project ?? "the project";
        if (resolved.Info.Endpoint is not { Length: > 0 } endpoint)
        {
            // A REPL or an `mcp` session holds the project without offering an address. There is
            // nothing to ask politely, and killing the pid would be exactly the ownership model this
            // design removed — so say who is holding it and let the person decide.
            Console.Error.WriteLine(
                $"'{name}' is held by pid {resolved.Info.Pid} ({resolved.Info.Mode}), which is not " +
                "serving — nothing to ask. Stop it where it runs.");
            return 1;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await using var client = await CliWireClient.ConnectAsync(endpoint, null, cts.Token);
            var status = await client.RequestInstanceStatusAsync(
                MessageTypes.InstanceStop, new InstanceStopPayload { Force = settings.Force }, cts.Token);

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
    /// otherwise a known project matched by name or manifest path. Reads lock files only — see the
    /// class remarks — and does not probe, because a stop is about to ask the instance anyway.
    /// </summary>
    private static async Task<InstanceRecord?> ResolveAsync(string? target)
    {
        var registry = new FileInstanceRegistry();

        if (target is null)
        {
            var cwd = Directory.GetCurrentDirectory();
            var manifest = Directory.GetFiles(cwd, "*.spla").FirstOrDefault();
            return manifest is null ? null : await registry.FindAsync(manifest);
        }

        // A path names the project directly; anything else is matched against the known list by name.
        if (target.EndsWith(".spla", StringComparison.OrdinalIgnoreCase) && File.Exists(target))
            return await registry.FindAsync(Path.GetFullPath(target));

        var all = await registry.ListAsync();
        return all.FirstOrDefault(r =>
            string.Equals(r.ProjectName, target, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Path.GetFileNameWithoutExtension(r.ProjectId), target, StringComparison.OrdinalIgnoreCase));
    }
}
