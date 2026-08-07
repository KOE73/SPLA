using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Observability;

namespace SPLA.CLI;

/// <summary>Resolved startup context for the CLI: the effective settings plus the two flags the
/// dispatcher needs (whether a <c>.spla</c> file was found, whether this is a <c>chat</c> command).</summary>
internal readonly record struct CliContext(ResolvedSettings Settings, string? SplaFile, bool IsChatCommand);

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
        }

        logger.LogInformation("CLI startup. ProjectFile={ProjectFile} WorkspacePath={WorkspacePath} Mode={Mode}",
            splaFile, settings.WorkspacePath, settings.Mode);

        announce.WriteLine($"Workspace: {settings.WorkspacePath}");
        announce.WriteLine($"Endpoint:  {settings.Connections.FirstOrDefault()?.Endpoint ?? "(none)"}");
        announce.WriteLine($"Mode:      {settings.Mode}");

        return new CliContext(settings, splaFile, isChatCommand);
    }
}
