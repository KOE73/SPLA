using System.ComponentModel;
using SPLA.Domain.Project;
using Spectre.Console.Cli;

namespace SPLA.CLI;

internal sealed class InitSettings : CommandSettings
{
    [CommandArgument(0, "[directory]")]
    [Description("Directory to initialize. Defaults to the current directory.")]
    public string? Directory { get; init; }

    [CommandOption("--profile")]
    [Description("Project profile (e.g., minimal, standard, inherit).")]
    public string? Profile { get; init; }

    [CommandOption("--name")]
    [Description("Project name (defaults to directory name).")]
    public string? Name { get; init; }
}

/// <summary>
/// <c>spla init [--profile P] [--name N] [directory]</c> — make a folder a project and stop.
///
/// <para>Recognised ahead of the command parser, for the same reason <c>mcp</c> is: every other
/// command runs <see cref="CliBootstrap"/> first, and that is precisely the code that would refuse to
/// continue in a folder with no manifest. A command whose entire job is to create the manifest cannot
/// be gated on the manifest existing.</para>
/// </summary>
internal sealed class InitCommand : AsyncCommand<InitSettings>
{
    public static bool IsInitCommand(string[] args)
        => args.Length > 0 && args[0].Equals("init", StringComparison.OrdinalIgnoreCase);

    protected override Task<int> ExecuteAsync(CommandContext context, InitSettings settings, CancellationToken cancellationToken)
    {
        var profile = ProjectProfiles.Default;
        if (settings.Profile is { Length: > 0 })
        {
            if (!ProjectProfiles.TryParse(settings.Profile, out profile))
                return Task.FromResult(Fail($"Unknown profile '{settings.Profile}'. Expected one of: {string.Join(", ", ProjectProfiles.AllNames)}."));
        }

        if (profile == ProjectProfile.Inherit)
            return Task.FromResult(Fail("'inherit' means running without a project — there is nothing for `init` to create."));

        try
        {
            var directory = settings.Directory ?? System.IO.Directory.GetCurrentDirectory();
            var manifest = ProjectFactory.Create(directory, settings.Name, profile);
            Console.WriteLine($"Created {manifest} ({ProjectProfiles.Name(profile)}: {ProjectProfiles.Describe(profile)})");
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            return Task.FromResult(Fail(ex.Message));
        }
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }
}
