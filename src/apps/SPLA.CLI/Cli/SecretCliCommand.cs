using System.ComponentModel;
using Spectre.Console.Cli;
using SPLA.Domain.Settings;

namespace SPLA.CLI;

internal sealed class SecretSettings : CommandSettings
{
    [CommandArgument(0, "<action>")]
    [Description("list | set | delete")]
    public required string Action { get; init; }

    [CommandArgument(1, "[key]")]
    public string? Key { get; init; }

    [CommandOption("--field")]
    public string? Field { get; init; }

    [CommandOption("--user")]
    public bool User { get; init; }

    [CommandOption("--project")]
    public bool Project { get; init; }

    [CommandOption("--shared")]
    public bool Shared { get; init; }
}

/// <summary><c>spla secret list|set|delete</c> — thin Spectre wrapper: reconstructs the argument
/// vector <see cref="SecretCommands.RunAsync"/> already understands, so its scope/hidden-prompt/field
/// handling (see its own doc comment) stays untouched.</summary>
internal sealed class SecretCliCommand(ResolvedSettings settings) : AsyncCommand<SecretSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, SecretSettings s, CancellationToken cancellationToken)
    {
        var args = new List<string> { "secret", s.Action };
        if (s.Key != null) args.Add(s.Key);
        if (s.Field != null) { args.Add("--field"); args.Add(s.Field); }
        if (s.User) args.Add("--user");
        if (s.Project) args.Add("--project");
        if (s.Shared) args.Add("--shared");

        return await SecretCommands.RunAsync(settings, args.ToArray());
    }
}
