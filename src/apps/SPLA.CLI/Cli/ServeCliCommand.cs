using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using SPLA.Domain.Settings;

namespace SPLA.CLI;

internal sealed class ServeSettings : CommandSettings
{
    [CommandOption("--port")]
    public int Port { get; init; } = 5050;

    [CommandOption("--bind")]
    public string Bind { get; init; } = "127.0.0.1";

    [CommandOption("--token")]
    [Description("Bearer token required from clients. Strongly recommended when --bind is not loopback.")]
    public string? Token { get; init; }

    [CommandOption("--repl")]
    [Description("Also run a console REPL against the same runtime a socket client would drive.")]
    public bool Repl { get; init; }

    [CommandOption("--new-chat")]
    [Description("Message to send on a fresh chat as soon as the service starts.")]
    public string? NewChat { get; init; }
}

/// <summary><c>spla serve</c> — thin Spectre wrapper over the existing <see cref="ServeCommand"/>.</summary>
internal sealed class ServeCliCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : AsyncCommand<ServeSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ServeSettings s, CancellationToken cancellationToken)
    {
        await ServeCommand.RunAsync(s.Port, s.Bind, s.Token, s.Repl, s.NewChat, settings, loggerFactory);
        return 0;
    }
}
