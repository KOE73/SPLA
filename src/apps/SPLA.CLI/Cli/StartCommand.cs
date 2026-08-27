using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Instances;
using Spectre.Console.Cli;

namespace SPLA.CLI;

internal sealed class StartSettings : CommandSettings
{
    [CommandArgument(0, "[project]")]
    [Description("Project name, manifest path, or nothing for the current directory.")]
    public string? Project { get; init; }

    [CommandOption("--registry")]
    [Description("Hub URL to start through, instead of starting the agent directly.")]
    public string? Registry { get; init; }

    [CommandOption("--token")]
    [Description("Bearer token for the hub (takes a secret reference or literal).")]
    public string? Token { get; init; }
}

/// <summary>
/// <c>spla start</c> — brings an agent up on a project and returns, leaving it running.
///
/// <para><b>Why this exists separately from <c>serve</c>.</b> <c>serve</c> is the agent: it occupies
/// the terminal it was run in and dies with it. That is right for someone watching it and wrong for
/// everything else — a script, a scheduled task, a machine being brought back up after a reboot. This
/// starts one and walks away, which is the shape management needs.</para>
///
/// <para><b>With and without a hub.</b> Given <c>--registry</c> the hub does the starting, so a
/// project on another machine can be brought up from here. Without one this process starts the child
/// itself. Both paths run the same spawner and produce the same thing — an ordinary detached agent
/// that registers, locks and idles out exactly like a hand-started one — because a management command
/// that produced a *special* kind of agent would be a second kind of thing to reason about.</para>
///
/// <para>Recognised ahead of the command parser like its siblings: starting a project must not begin
/// by becoming a writer of it.</para>
/// </summary>
internal sealed class StartCommand : AsyncCommand<StartSettings>
{
    private readonly ILoggerFactory _loggers;

    public StartCommand(ILoggerFactory loggers) => _loggers = loggers;

    public static bool IsStartCommand(string[] args)
        => args.Length > 0 && args[0].Equals("start", StringComparison.OrdinalIgnoreCase);

    protected override async Task<int> ExecuteAsync(
        CommandContext context, StartSettings settings, CancellationToken cancellationToken)
    {
        var manifest = ResolveManifest(settings.Project);
        if (manifest is null)
        {
            Console.Error.WriteLine(settings.Project is null
                ? "No project in this directory. Name one, or run `spla init` here first."
                : $"No project matches '{settings.Project}'. Try `spla ps` or give a path to a .spla file.");
            return 1;
        }

        var name = Path.GetFileNameWithoutExtension(manifest);

        return settings.Registry is { Length: > 0 } hub
            ? await StartViaHubAsync(hub, settings.Token, manifest, name, cancellationToken)
            : await StartHereAsync(manifest, name, cancellationToken);
    }

    private async Task<int> StartHereAsync(string manifest, string name, CancellationToken ct)
    {
        // No hub named: the child is told about none either, so it is visible through its lock file
        // and `spla ps` rather than through a hub. Same agent, narrower view.
        var spawner = new CliInstanceSpawner(hubUrl: null, _loggers.CreateLogger<CliInstanceSpawner>());
        var result = await spawner.StartAsync(manifest, ct: ct);
        return Report(result.Started, result.AlreadyRunning, result.Error, name);
    }

    private static async Task<int> StartViaHubAsync(
        string hub, string? tokenRef, string manifest, string name, CancellationToken ct)
    {
        var token = ConfigLoader.LoadAndResolve().SecretResolver.Resolve(tokenRef);

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            if (token is { Length: > 0 })
                http.DefaultRequestHeaders.Authorization = new("Bearer", token);

            var url = $"{hub.TrimEnd('/')}{RegistryRoutes.Start}?project={Uri.EscapeDataString(manifest)}";
            var response = await http.PostAsync(url, content: null, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotImplemented)
            {
                Console.Error.WriteLine($"The hub at {hub} does not start instances.");
                return 1;
            }

            var body = await response.Content.ReadFromJsonAsync<StartResponse>(ct);
            return Report(body?.Started ?? false, body?.AlreadyRunning ?? false, body?.Error, name);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not reach the hub at {hub}: {ex.Message}");
            return 1;
        }
    }

    /// <summary>"Already running" is success, not a failure: the caller asked for the project to be
    /// up, and it is. Only an actual refusal to start is an error worth a non-zero exit.</summary>
    private static int Report(bool started, bool alreadyRunning, string? error, string name)
    {
        if (alreadyRunning)
        {
            Console.WriteLine($"'{name}' is already running.");
            return 0;
        }

        if (started)
        {
            Console.WriteLine($"'{name}' is starting.");
            return 0;
        }

        Console.Error.WriteLine($"Could not start '{name}'{(error is null ? "." : ": " + error)}");
        return 1;
    }

    /// <summary>Finds the manifest to start: the one in this directory when nothing is named, a path
    /// taken as given, or a name matched against the projects this machine remembers. Deliberately
    /// does not resolve or load the project — that is the child's job, and doing it here would be the
    /// second writer this command exists to avoid.</summary>
    private static string? ResolveManifest(string? target)
    {
        if (target is null)
        {
            var here = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.spla");
            return here.Length == 1 ? here[0] : null;
        }

        if (target.EndsWith(".spla", StringComparison.OrdinalIgnoreCase))
            return File.Exists(target) ? Path.GetFullPath(target) : null;

        return ConfigLoader.LoadRecentProjects().FirstOrDefault(p =>
            string.Equals(Path.GetFileNameWithoutExtension(p), target, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class StartResponse
    {
        public bool Started { get; set; }
        public bool AlreadyRunning { get; set; }
        public string? Error { get; set; }
    }
}
