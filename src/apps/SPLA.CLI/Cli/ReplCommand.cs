using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using SPLA.Domain.Settings;
using SPLA.Runtime;

namespace SPLA.CLI;

internal sealed class ReplSettings : CommandSettings
{
    // Not read here — CliBootstrap already resolved the project (including an explicit "<path>.spla"
    // positional) before Spectre ever saw the argument vector. This only exists so the default command
    // accepts that positional instead of Spectre rejecting it as an unknown argument.
    [CommandArgument(0, "[project]")]
    public string? Project { get; init; }
}

/// <summary>The bare <c>spla</c> (or <c>spla &lt;project&gt;.spla</c>) entry point: a fresh chat and
/// the interactive REPL. Registered as the app's default command.</summary>
internal sealed class ReplCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : AsyncCommand<ReplSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ReplSettings s, CancellationToken cancellationToken)
    {
        using var runtime = RuntimeBootstrap.Build(settings, loggerFactory);
        var session = runtime.ChatManager.CreateNewChat();
        var chat = new ChatRuntime(runtime, session);
        await InteractiveRepl.RunAsync(runtime, chat);
        return 0;
    }
}
