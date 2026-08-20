using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using SPLA.Domain.Settings;

namespace SPLA.CLI;

internal sealed class ServeSettings : CommandSettings
{
    // Ephemeral by default: two "spla serve" invocations for two different projects used to both
    // grab the fixed 5050 and the second one failed to bind. Nothing needs a well-known port any
    // more — the project's instance lock file (.spla/instance.json) publishes whatever port was
    // actually bound, so callers discover it instead of guessing. Pass --port for a fixed address.
    [CommandOption("--port")]
    [Description("Port to bind. Defaults to 0 (OS-assigned ephemeral port); pass a nonzero value for a fixed address.")]
    public int Port { get; init; } = 0;

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

    // Off by default on purpose: nobody expects a daemon they started by hand to leave on its own.
    // A window that spawns this process passes a few minutes, so closing the window reclaims the
    // process eventually — without ever cutting a turn short or answering a pending question.
    [CommandOption("--registry")]
    [Description("Hub to register with, e.g. http://build-server:5077. Absent = this machine's lock files are the only registry.")]
    public string? Registry { get; init; }

    [CommandOption("--registry-token")]
    [Description("Token for the hub. Takes a secret reference (secret:user:registry) or a literal.")]
    public string? RegistryToken { get; init; }

    [CommandOption("--idle-timeout")]
    [Description("Minutes with no clients and nothing running before the instance stops itself. 0 (default) = never.")]
    public int IdleTimeoutMinutes { get; init; }
}

/// <summary><c>spla serve</c> — thin Spectre wrapper over the existing <see cref="ServeCommand"/>.</summary>
internal sealed class ServeCliCommand(ResolvedSettings settings, ILoggerFactory loggerFactory)
    : AsyncCommand<ServeSettings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, ServeSettings s, CancellationToken cancellationToken)
    {
        await ServeCommand.RunAsync(
            s.Port, s.Bind, s.Token, s.Repl, s.NewChat, s.IdleTimeoutMinutes,
            s.Registry, s.RegistryToken, settings, loggerFactory);
        return 0;
    }
}
