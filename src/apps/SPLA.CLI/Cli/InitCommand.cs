using SPLA.Domain.Project;

namespace SPLA.CLI;

/// <summary>
/// <c>spla init [--profile P] [--name N] [directory]</c> — make a folder a project and stop.
///
/// <para>Recognised ahead of the command parser, for the same reason <c>mcp</c> is: every other
/// command runs <see cref="CliBootstrap"/> first, and that is precisely the code that would refuse to
/// continue in a folder with no manifest. A command whose entire job is to create the manifest cannot
/// be gated on the manifest existing.</para>
/// </summary>
internal static class InitCommand
{
    public static bool IsInitCommand(string[] args)
        => args.Length > 0 && args[0].Equals("init", StringComparison.OrdinalIgnoreCase);

    /// <returns>Process exit code.</returns>
    public static int Run(string[] args)
    {
        string? name = null;
        string? directory = null;
        var profile = ProjectProfiles.Default;

        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg.ToLowerInvariant())
            {
                case "--profile" when i + 1 < args.Length:
                    if (!ProjectProfiles.TryParse(args[++i], out profile))
                        return Fail($"Unknown profile '{args[i]}'. Expected one of: {string.Join(", ", ProjectProfiles.AllNames)}.");
                    break;
                case "--name" when i + 1 < args.Length:
                    name = args[++i];
                    break;
                case "--help" or "-h":
                    PrintHelp();
                    return 0;
                default:
                    if (arg.StartsWith('-')) return Fail($"Unknown option '{arg}'.");
                    directory = arg;
                    break;
            }
        }

        if (profile == ProjectProfile.Inherit)
            return Fail("'inherit' means running without a project — there is nothing for `init` to create.");

        try
        {
            var manifest = ProjectFactory.Create(directory ?? Directory.GetCurrentDirectory(), name, profile);
            Console.WriteLine($"Created {manifest} ({ProjectProfiles.Name(profile)}: {ProjectProfiles.Describe(profile)})");
            return 0;
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: spla init [--profile <profile>] [--name <name>] [directory]");
        Console.WriteLine();
        Console.WriteLine("Profiles:");
        foreach (var n in ProjectProfiles.AllNames)
            if (ProjectProfiles.TryParse(n, out var p))
                Console.WriteLine($"  {n,-9} {ProjectProfiles.Describe(p)}");
        Console.WriteLine();
        Console.WriteLine($"Default profile: {ProjectProfiles.Name(ProjectProfiles.Default)}.");
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }
}
