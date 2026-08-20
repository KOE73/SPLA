using System.ComponentModel;
using System.Net.Http.Json;
using SPLA.CLI.Wire;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;
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

    [CommandOption("--registry")]
    [Description("Hub URL to relay the stop through, for a project this machine cannot see.")]
    public string? Registry { get; init; }

    [CommandOption("--token")]
    [Description("Bearer token for the hub (takes a secret reference or literal).")]
    public string? Token { get; init; }

    [CommandOption("--all")]
    [Description("Close the whole project: the agent and every window on it. Needs --registry.")]
    public bool All { get; init; }
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
        if (settings.All)
        {
            return settings.Registry is { Length: > 0 } hub
                ? await CloseProjectAsync(hub, settings, cancellationToken)
                : Fail("--all closes a project through a hub, which is the only thing that knows " +
                       "about its windows. Add --registry <url>.");
        }

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

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }

    /// <summary>
    /// Closes a whole project through the hub: the agent and every window on it, asked together.
    ///
    /// <para>Only the hub can do this, and that is not an implementation detail — a window is not
    /// written into any lock file, so a purely local stop has no way to learn one exists. Stopping the
    /// agent alone is exactly what used to leave a window talking to a service that would never
    /// answer, which is the whole reason this option is here.</para>
    ///
    /// <para>Each participant still decides for itself; the count reported is how many were asked.</para>
    /// </summary>
    private static async Task<int> CloseProjectAsync(string hub, StopSettings settings, CancellationToken ct)
    {
        var manifest = settings.Project;
        if (string.IsNullOrWhiteSpace(manifest))
        {
            var here = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.spla");
            if (here.Length != 1) return Fail("Name the project to close — this directory holds no single one.");
            manifest = here[0];
        }
        else if (manifest.EndsWith(".spla", StringComparison.OrdinalIgnoreCase) && File.Exists(manifest))
        {
            manifest = Path.GetFullPath(manifest);
        }

        var token = ConfigLoader.LoadAndResolve().SecretResolver.Resolve(settings.Token);

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            if (token is { Length: > 0 })
                http.DefaultRequestHeaders.Authorization = new("Bearer", token);

            var url = $"{hub.TrimEnd('/')}{RegistryRoutes.StopProject}" +
                      $"?project={Uri.EscapeDataString(manifest)}&force={(settings.Force ? "true" : "false")}";
            var response = await http.PostAsync(url, content: null, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.Error.WriteLine($"Nothing is registered against '{manifest}' at {hub}.");
                return 1;
            }
            if (!response.IsSuccessStatusCode)
                return Fail($"The hub refused: {(int)response.StatusCode}.");

            var body = await response.Content.ReadFromJsonAsync<StopProjectResponse>(ct);
            Console.WriteLine($"Asked {body?.Asked ?? 0} participant(s) on '{Path.GetFileNameWithoutExtension(manifest)}' to close.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not reach the hub at {hub}: {ex.Message}");
            return 1;
        }
    }

    private sealed class StopProjectResponse
    {
        public int Asked { get; set; }
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
