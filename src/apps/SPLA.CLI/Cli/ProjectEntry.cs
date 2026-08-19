using SPLA.Domain.Project;

namespace SPLA.CLI;

/// <summary>
/// What to do when the launch directory holds no project manifest.
///
/// <para>The old answer was "nothing, quietly": settings came from the machine's <c>defaults.yaml</c>,
/// runtime state went to the shared <c>~/.spla</c>, and — because there was no manifest — there was no
/// root, no cutouts and no edge ledger either. A folder nobody declared anything about ended up with
/// MORE authority than a real project, which is the opposite of what anyone reading the code expects.
/// So the decision is now taken explicitly, and this type is that decision.</para>
/// </summary>
internal readonly record struct ProjectEntryRequest(ProjectProfile? Profile, string[] Args)
{
    /// <summary>The flag that carries the decision. <c>--init</c> alone means the default profile.</summary>
    public const string Flag = "--init";

    /// <summary>
    /// Pulls <c>--init[=profile]</c> out of the argument vector and returns the rest. Stripped rather
    /// than passed through because the command parser below knows nothing about it and would reject
    /// the whole invocation — the flag is answered before any command exists.
    /// </summary>
    /// <exception cref="InvalidOperationException">An unknown profile name.</exception>
    public static ProjectEntryRequest Parse(string[] args)
    {
        ProjectProfile? profile = null;
        var rest = new List<string>(args.Length);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith(Flag + "=", StringComparison.OrdinalIgnoreCase))
            {
                profile = ParseProfile(arg[(Flag.Length + 1)..]);
                continue;
            }

            if (arg.Equals(Flag, StringComparison.OrdinalIgnoreCase))
            {
                // "--init standard" as well as "--init=standard": a value only counts when it is a
                // profile name, so "--init" followed by a subcommand still means the default.
                if (i + 1 < args.Length && ProjectProfiles.TryParse(args[i + 1], out var next))
                {
                    profile = next;
                    i++;
                }
                else
                {
                    profile = ProjectProfiles.Default;
                }
                continue;
            }

            rest.Add(arg);
        }

        return new ProjectEntryRequest(profile, rest.ToArray());
    }

    private static ProjectProfile ParseProfile(string name)
        => ProjectProfiles.TryParse(name, out var p)
            ? p
            : throw new InvalidOperationException(
                $"Unknown launch profile '{name}'. Expected one of: {string.Join(", ", ProjectProfiles.AllNames)}.");

    /// <summary>
    /// Whether this invocation may stop and ask a person. Only the interactive REPL may: everything
    /// else is either a protocol on stdout (<c>mcp</c>), a daemon (<c>serve</c>) or a scripted run
    /// (<c>chat run</c>), and a prompt there is a process that hangs where nobody can see it.
    /// </summary>
    public static bool CanAsk(string[] args)
    {
        if (Console.IsInputRedirected) return false;

        // No command at all = the REPL.
        if (args.Length == 0) return true;

        // `chat open` is the REPL too; `chat run`/`chat list`/`chat fork` are not.
        return args[0].Equals("chat", StringComparison.OrdinalIgnoreCase)
            && args.Length > 1
            && args[1].Equals("open", StringComparison.OrdinalIgnoreCase);
    }
}
