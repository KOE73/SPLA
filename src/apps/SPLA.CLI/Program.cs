using SPLA.Runtime;
using SPLA.CLI;
using SPLA.CLI.Batch;
using SPLA.Observability;
using SPLA.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

// Same reasoning as the `mcp` check below: a raw args check ahead of everything else, so a foreign
// head can read usage without first standing up a runtime or tripping the project-file search.
if (args.Length > 0 && args[0].Equals("--help-mcp", StringComparison.OrdinalIgnoreCase))
{
    SPLA.CLI.McpCommand.PrintHelpMcp();
    return;
}

// `init` creates the manifest, so it cannot run behind the bootstrap that refuses to continue
// without one. See InitCommand.
if (SPLA.CLI.InitCommand.IsInitCommand(args))
{
    Environment.ExitCode = SPLA.CLI.InitCommand.Run(args);
    return;
}

// `ps` and `stop` are about instances, not about the directory the shell happens to be in, and
// neither may become a writer of any project just to look at (or ask about) one. So they run ahead
// of the bootstrap that resolves and locks a project — they read lock files and talk over sockets.
if (SPLA.CLI.PsCommand.IsPsCommand(args))
{
    Environment.ExitCode = await SPLA.CLI.PsCommand.RunAsync(args);
    return;
}

if (SPLA.CLI.StopCommand.IsStopCommand(args))
{
    Environment.ExitCode = await SPLA.CLI.StopCommand.RunAsync(args);
    return;
}

// A hub holds no project and runs no agent, so it must not go through the bootstrap that resolves
// and locks one either. Same binary, different role — see HubCommand for why it is not its own exe.
if (SPLA.CLI.HubCommand.IsHubCommand(args))
{
    SplaTelemetry.ConfigureGlobalLogs();
    using var hubLoggers = LoggerFactory.Create(b =>
    {
        b.ClearProviders();
        b.AddProvider(SplaTelemetry.CreateFileLoggerProvider());
        b.SetMinimumLevel(LogLevel.Information);
    });
    Environment.ExitCode = await SPLA.CLI.HubCommand.RunAsync(args, hubLoggers);
    return;
}

// `mcp` speaks a protocol on stdout, so it must be recognised before anything prints. The banner
// below would be the first thing a client reads, and an unparsable first line kills the session.
// It stays a raw args check ahead of Spectre for exactly that reason — nothing may run first.
var isMcp = SPLA.CLI.McpCommand.IsMcpCommand(args);

if (!isMcp) Console.WriteLine("=== SPLA CLI ===");
SplaTelemetry.ConfigureGlobalLogs();
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.ClearProviders();
    builder.AddProvider(SplaTelemetry.CreateFileLoggerProvider());
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger("SPLA.CLI");

// Make the DPAPI secret backend selectable (secrets.backend: dpapi in ~/.spla/defaults.yaml).
// No-op on non-Windows — the config loader then falls back to the plaintext file store.
SPLA.Secrets.Dpapi.DpapiSecrets.Register(msg => logger.LogWarning("{Message}", msg));

// For `mcp` the startup summary goes to stderr: stdout is the protocol from the first byte. This also
// resolves the project file, including an explicit "<path>.spla" positional argument — done once,
// here, so no command below has to re-sniff the argument vector for it.
CliContext ctx;
try
{
    ctx = CliBootstrap.Resolve(args, logger, isMcp ? Console.Error : Console.Out);
}
catch (InvalidOperationException ex)
{
    // "no project here and I cannot ask", "two manifests in one folder", "unknown profile" — all
    // decisions the person has to make, and all useless as a stack trace. `mcp` gets it on stderr
    // because stdout is its protocol.
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
    return;
}

// mcp: serve this project's tools to a foreign head over stdio. Before every other command, and
// before the tool count a chat command would print below — stdout belongs to the protocol from here on.
if (isMcp)
{
    await SPLA.CLI.McpCommand.RunAsync(ctx.Settings, loggerFactory);
    return;
}

var services = new ServiceCollection();
services.AddSingleton(loggerFactory);
services.AddSingleton(ctx.Settings);

var app = new CommandApp<ReplCommand>(new TypeRegistrar(services));
app.Configure(config =>
{
    config.SetApplicationName("spla");

    config.AddBranch("chat", chat =>
    {
        chat.SetDescription("Manage or run chat sessions.");
        chat.AddCommand<ChatListCommand>("list").WithDescription("List saved chats.");
        chat.AddCommand<ChatOpenCommand>("open").WithDescription("Resume a saved chat (or start a new one) in the REPL.");
        chat.AddCommand<ChatForkCommand>("fork").WithDescription("Duplicate a saved chat, optionally onto a different model.");
        chat.AddCommand<ChatRunCommand>("run").WithDescription("Run one or more prompts against one or more models, headlessly, to the screen or to files.");
    });

    config.AddCommand<ServeCliCommand>("serve").WithDescription("Run the WebSocket service.");
    config.AddCommand<SecretCliCommand>("secret").WithDescription("Manage the secret store.");
    config.AddCommand<SystemCliCommand>("system").WithDescription("OS-level integration (file association).");
});

// ctx.Args, not args: the launch-profile flag was answered during bootstrap and the command parser
// has never heard of it.
try
{
    Environment.ExitCode = await app.RunAsync(ctx.Args);
}
catch (SPLA.Domain.Project.ProjectBusyException ex)
{
    // Somebody already has this project open. The message carries where they are, which is the only
    // useful thing to say — a stack trace would bury it.
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
}
