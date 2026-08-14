using System.ComponentModel;
using Spectre.Console.Cli;

namespace SPLA.CLI;

internal sealed class SystemSettings : CommandSettings
{
    [CommandArgument(0, "<action>")]
    [Description("register-association")]
    public required string Action { get; init; }
}

/// <summary><c>spla system register-association</c> — thin Spectre wrapper over
/// <see cref="SystemCommands"/> (see its own doc comment for what it does).</summary>
internal sealed class SystemCliCommand : Command<SystemSettings>
{
    protected override int Execute(CommandContext context, SystemSettings s, CancellationToken cancellationToken) =>
        SystemCommands.Run(["system", s.Action]);
}
