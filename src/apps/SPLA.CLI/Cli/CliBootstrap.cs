using Microsoft.Extensions.Logging;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;
using SPLA.Observability;

namespace SPLA.CLI;

/// <summary>Resolved startup context for the CLI: the effective settings, the two flags the
/// dispatcher needs (whether a <c>.spla</c> file was found, whether this is a <c>chat</c> command),
/// and the argument vector with the launch-profile flag already taken out of it.</summary>
internal readonly record struct CliContext(
    ResolvedSettings Settings, string? SplaFile, bool IsChatCommand, string[] Args);

/// <summary>Locates the project file, resolves settings, sets the working directory, and wires the
/// per-project log destination — the shared preamble every CLI entry point runs before dispatch.</summary>
internal static class CliBootstrap
{
    /// <param name="announce">Where the startup summary goes. Defaults to stdout, but a command
    /// that speaks a protocol there (<c>mcp</c>) passes stderr instead: the summary is worth having
    /// while debugging a connection, and worth nothing at all if it corrupts the first line the
    /// client reads.</param>
    public static CliContext Resolve(string[] args, ILogger logger, TextWriter? announce = null)
    {
        announce ??= Console.Out;

        var entry = ProjectEntryRequest.Parse(args);
        args = entry.Args;

        string? splaFile;
        bool isChatCommand = false;

        if (args.Length > 0 && args[0].Equals("chat", StringComparison.OrdinalIgnoreCase))
        {
            isChatCommand = true;
            splaFile = ConfigLoader.FindProjectFile(Directory.GetCurrentDirectory());
        }
        else if (args.Length > 0 && args[0].EndsWith(".spla", StringComparison.OrdinalIgnoreCase))
        {
            splaFile = args[0];
        }
        else
        {
            splaFile = ConfigLoader.FindProjectFile(Directory.GetCurrentDirectory());
        }

        // No manifest here: decide, out loud, instead of inheriting the machine's whole configuration
        // and dropping the boundary on the way. See ProjectEntryRequest for why this is not silent.
        splaFile ??= EnterWithoutManifest(entry, args, announce);

        ResolvedSettings settings;
        if (splaFile != null)
        {
            announce.WriteLine($"Project file: {splaFile}");
            ConfigLoader.ScaffoldIfNew(splaFile);
            settings = ConfigLoader.LoadAndResolve(splaFile);
            Directory.SetCurrentDirectory(settings.WorkspacePath);
            SplaTelemetry.ConfigureProjectLogs(settings.Project.GetBucket("logs").MapToHostDirectory());
            announce.WriteLine($"Project: {settings.ProjectName ?? Path.GetFileNameWithoutExtension(splaFile)}");
        }
        else
        {
            settings = ConfigLoader.LoadAndResolve();
            SplaTelemetry.ConfigureProjectLogs(settings.Project.GetBucket("logs").MapToHostDirectory());
            announce.WriteLine(
                "No project: machine defaults, no path boundary, state in the shared home. (--init=minimal makes this folder a project.)");
        }

        logger.LogInformation("CLI startup. ProjectFile={ProjectFile} WorkspacePath={WorkspacePath} Mode={Mode}",
            splaFile, settings.WorkspacePath, settings.Mode);

        announce.WriteLine($"Workspace: {settings.WorkspacePath}");
        announce.WriteLine($"Endpoint:  {settings.Connections.FirstOrDefault()?.Endpoint ?? "(none)"}");
        announce.WriteLine($"Mode:      {settings.Mode}");

        return new CliContext(settings, splaFile, isChatCommand, args);
    }

    /// <summary>
    /// Answers "there is no project here". Returns the manifest of a project just created, or null to
    /// run without one — which now only happens when somebody actually chose it.
    /// </summary>
    private static string? EnterWithoutManifest(ProjectEntryRequest entry, string[] args, TextWriter announce)
    {
        var profile = entry.Profile;

        if (profile is null)
        {
            if (!ProjectEntryRequest.CanAsk(args))
                throw new InvalidOperationException(BuildRefusal());

            profile = Ask(announce);
        }

        if (profile == ProjectProfile.Inherit) return null;

        var created = ProjectFactory.Create(Directory.GetCurrentDirectory(), name: null, profile.Value);
        announce.WriteLine($"Created {Path.GetFileName(created)} ({ProjectProfiles.Name(profile.Value)}).");
        return created;
    }

    /// <summary>The message a scripted run gets. It has to name the way forward, because the thing it
    /// cannot do is ask.</summary>
    private static string BuildRefusal()
    {
        var lines = ProjectProfiles.AllNames
            .Select(n => ProjectProfiles.TryParse(n, out var p)
                ? $"  --init={n,-9} {ProjectProfiles.Describe(p)}"
                : $"  --init={n}");

        return $"No project file in {Directory.GetCurrentDirectory()}, and this command cannot stop to ask.\n"
             + "Say what this folder should be:\n"
             + string.Join("\n", lines);
    }

    /// <summary>The interactive counterpart: same three answers, asked rather than declared.</summary>
    private static ProjectProfile Ask(TextWriter announce)
    {
        announce.WriteLine($"\nNo project file in {Directory.GetCurrentDirectory()}.");
        foreach (var name in ProjectProfiles.AllNames)
            if (ProjectProfiles.TryParse(name, out var p))
                announce.WriteLine($"  {name,-9} — {ProjectProfiles.Describe(p)}");

        var fallback = ProjectProfiles.Default;
        announce.Write($"Profile [{ProjectProfiles.Name(fallback)}]: ");
        announce.Flush();

        var answer = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(answer)) return fallback;

        return ProjectProfiles.TryParse(answer, out var chosen)
            ? chosen
            : throw new InvalidOperationException(
                $"Unknown launch profile '{answer.Trim()}'. Expected one of: {string.Join(", ", ProjectProfiles.AllNames)}.");
    }
}
