using SPLA.Runtime;
using SPLA.CLI;
using SPLA.CLI.Batch;
using SPLA.Observability;
using SPLA.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Globalization;

// Same reasoning as the `mcp` check below: a raw args check ahead of everything else, so a foreign
// head can read usage without first standing up a runtime or tripping the project-file search.
if (args.Length > 0 && args[0].Equals("--help-mcp", StringComparison.OrdinalIgnoreCase))
{
    SPLA.CLI.McpCommand.PrintHelpMcp();
    return;
}

// Recognise pre-bootstrap commands (init, ps, stop, hub) and run them through a minimal
// Spectre parser that doesn't require project resolution. These commands have their own good reasons
// not to lock a project (init creates one, ps/stop read lock files, hub holds none).
if (args.Length > 0)
{
    var cmdName = args[0].ToLowerInvariant();
    if (cmdName is "init" or "ps" or "start" or "stop" or "hub")
    {
        Environment.ExitCode = await RunPreBootstrapCommandAsync(cmdName, args);
        return;
    }
}

// `mcp` speaks a protocol on stdout, so it must be recognised before anything prints. The banner
// below would be the first thing a client reads, and an unparsable first line kills the session.
// It stays a raw args check ahead of Spectre for exactly that reason — nothing may run first.
var isMcp = SPLA.CLI.McpCommand.IsMcpCommand(args);

// `spla mcp --help` means the same thing as `spla --help-mcp` above, and has to be answered in the
// same place: this command never reaches Spectre, so nobody downstream would recognise the flag.
if (isMcp && SPLA.CLI.McpCommand.IsHelpRequest(args))
{
    SPLA.CLI.McpCommand.PrintHelpMcp();
    return;
}

// Usage is an answer this program can always give: it describes the commands, not the folder. Asking
// for it in a directory with no manifest used to trip the project search and refuse ("say what this
// folder should be") — a demand for a decision the person had not asked to make. So help is answered
// here, ahead of bootstrap, from the same command tree the real run uses.
if (IsHelpRequest(args))
{
    // "--help all" / "help all": the person is digging rather than asking about one command, so dump
    // every leaf's usage in one pass instead of making them walk the tree by hand.
    if (args.Any(a => a.Equals("all", StringComparison.OrdinalIgnoreCase)))
    {
        await PrintAllHelpAsync();
        return;
    }

    var helpServices = new ServiceCollection();
    helpServices.AddSingleton(loggerFactoryForHelp());
    var helpApp = new CommandApp<ReplCommand>(new TypeRegistrar(helpServices));
    helpApp.Configure(ConfigureFullApp);
    Environment.ExitCode = await helpApp.RunAsync(NormalizeHelpArgs(args));
    return;
}

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
    await SPLA.CLI.McpCommand.RunAsync(ctx.Settings, loggerFactory, args);
    return;
}

var services = new ServiceCollection();
services.AddSingleton(loggerFactory);
services.AddSingleton(ctx.Settings);

var app = new CommandApp<ReplCommand>(new TypeRegistrar(services));
app.Configure(ConfigureFullApp);

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

// Helper: run a pre-bootstrap command (init, ps, stop, hub) through a minimal Spectre CommandApp
// that doesn't require project resolution. These commands have legitimate reasons not to lock
// a project file.
async Task<int> RunPreBootstrapCommandAsync(string cmd, string[] args)
{
    SplaTelemetry.ConfigureGlobalLogs();
    using var cmdLoggers = LoggerFactory.Create(b =>
    {
        b.ClearProviders();
        b.AddProvider(SplaTelemetry.CreateFileLoggerProvider());
        b.SetMinimumLevel(LogLevel.Information);
    });

    var services = new ServiceCollection();
    services.AddSingleton(cmdLoggers);
    var app = new CommandApp(new TypeRegistrar(services));
    app.Configure(config =>
    {
        ApplyCommonCliConventions(config);

        config.AddCommand<SPLA.CLI.InitCommand>("init")
            .WithDescription("Make a folder a project.");
        config.AddCommand<SPLA.CLI.PsCommand>("ps")
            .WithDescription("List currently running SPLA instances.");
        config.AddCommand<SPLA.CLI.StartCommand>("start")
            .WithDescription("Bring an agent up on a project and leave it running.");
        config.AddCommand<SPLA.CLI.StopCommand>("stop")
            .WithDescription("Ask a running instance to shut down.");
        config.AddCommand<SPLA.CLI.HubCommand>("hub")
            .WithDescription("Run a registry hub.");
    });

    return await app.RunAsync(args);
}

/// <summary>Every leaf's usage, one after another, plus the MCP usage doc at the end — the whole
/// tree in one scroll instead of `--help` on each command in turn. The leaf list is hand-kept (Spectre
/// has no public API to walk its own tree), so a new command belongs here too; nothing enforces that,
/// but the list is short and lives next to <see cref="ConfigureFullApp"/> on purpose.</summary>
async Task PrintAllHelpAsync()
{
    string[][] leaves =
    [
        [], // top-level usage: the command list itself
        ["chat", "list"], ["chat", "open"], ["chat", "fork"], ["chat", "run"],
        ["serve"], ["secret"], ["system"],
        ["init"], ["ps"], ["start"], ["stop"], ["hub"],
    ];

    var helpServices = new ServiceCollection();
    helpServices.AddSingleton(loggerFactoryForHelp());
    var helpApp = new CommandApp<ReplCommand>(new TypeRegistrar(helpServices));
    helpApp.Configure(ConfigureFullApp);

    foreach (var leaf in leaves)
    {
        Console.WriteLine(leaf.Length == 0 ? "=== spla ===" : $"=== spla {string.Join(' ', leaf)} ===");
        await helpApp.RunAsync([.. leaf, "--help"]);
        Console.WriteLine();
    }

    Console.WriteLine("=== spla mcp ===");
    SPLA.CLI.McpCommand.PrintHelpMcp();
}

/// <summary>The command tree of a normal run. Shared with the pre-bootstrap help path above, so usage
/// describes the commands that actually exist rather than a second, drifting copy of the list.</summary>
void ConfigureFullApp(IConfigurator config)
{
    ApplyCommonCliConventions(config);

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

    // The pre-bootstrap commands, listed for help only: they are dispatched above, through their own
    // parser, but somebody reading `spla --help` is asking what they can type, not which side of the
    // bootstrap it runs on.
    config.AddCommand<SPLA.CLI.InitCommand>("init").WithDescription("Make a folder a project.");
    config.AddCommand<SPLA.CLI.PsCommand>("ps").WithDescription("List currently running SPLA instances.");
    config.AddCommand<SPLA.CLI.StartCommand>("start").WithDescription("Bring an agent up on a project and leave it running.");
    config.AddCommand<SPLA.CLI.StopCommand>("stop").WithDescription("Ask a running instance to shut down.");
    config.AddCommand<SPLA.CLI.HubCommand>("hub").WithDescription("Run a registry hub.");
}

/// <summary>True when the argument vector asks what this program can do rather than asking it to do
/// something: a bare `help`, or a help flag anywhere in the vector (so `spla chat run --help` counts).</summary>
bool IsHelpRequest(string[] argv)
{
    if (argv.Length == 0) return false;
    if (argv[0].Equals("help", StringComparison.OrdinalIgnoreCase)) return true;
    return argv.Any(IsHelpFlag);
}

bool IsHelpFlag(string a) =>
    a.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
    a.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
    a.Equals("-?", StringComparison.Ordinal) ||
    a.Equals("/?", StringComparison.Ordinal);

/// <summary>Hands Spectre a vector it recognises: `help` becomes `--help`, `help <cmd>` becomes
/// `<cmd> --help`, and anything already carrying a flag is passed through untouched.</summary>
string[] NormalizeHelpArgs(string[] argv)
{
    if (!argv[0].Equals("help", StringComparison.OrdinalIgnoreCase)) return argv;
    return argv.Skip(1).Where(a => !IsHelpFlag(a)).Append("--help").ToArray();
}

/// <summary>Logging for the help path. Help prints and exits, so this only exists because Spectre's
/// type registrar wants a container it can build.</summary>
ILoggerFactory loggerFactoryForHelp() => LoggerFactory.Create(b => b.ClearProviders());

/// <summary>
/// The conventions every SPLA command parser shares. Both <c>CommandApp</c>s here — the full one and
/// the minimal pre-bootstrap one — go through this, so a command cannot behave differently depending
/// on which side of the bootstrap it happens to live on.
/// </summary>
void ApplyCommonCliConventions(IConfigurator config)
{
    config.SetApplicationName("spla");

    // Without this, an unknown option is accepted and quietly parked in Remaining.Parsed, so
    // `spla serve --idle-timout 5` runs with the default timeout and says nothing.
    config.UseStrictParsing();

    // ── Help language ────────────────────────────────────────────────────────────────────────────
    // Spectre's own help chrome ("DESCRIPTION", "USAGE", "OPTIONS") is localised, and with no culture
    // set its resource manager follows CurrentUICulture — so the same binary printed Russian headings
    // on a Russian Windows and English ones elsewhere. Our own text (command and option descriptions)
    // is English either way, which made the output a mix of two languages rather than a translation.
    //
    // Pinned to English deliberately, and pinned in ONE place so it stays easy to undo: SPLA is
    // positioned as an international project and is not localised today. If that changes, this is the
    // line to change — pass the desired CultureInfo, or drop the call entirely to follow the machine
    // locale again. Nothing else in the CLI depends on it.
    config.SetApplicationCulture(CultureInfo.GetCultureInfo("en"));
}
