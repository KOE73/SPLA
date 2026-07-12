using SPLA.CLI;
using SPLA.Observability;
using SPLA.Service;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== SPLA CLI ===");
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

var ctx = CliBootstrap.Resolve(args, logger);

// serve: run the WebSocket service (its own runtime lifecycle) and return.
if (args.Length > 0 && args[0].Equals("serve", StringComparison.OrdinalIgnoreCase))
{
    await ServeCommand.RunAsync(args, ctx.Settings, loggerFactory);
    return;
}

// secret: manage the secret store (no agent stack needed).
if (SecretCommands.IsSecretCommand(args))
{
    Environment.ExitCode = await SecretCommands.RunAsync(ctx.Settings, args);
    return;
}

// Shared agent stack for the interactive REPL and chat sub-commands.
using var runtime = new AgentRuntime(ctx.Settings, loggerFactory);
Console.WriteLine($"Tools registered: {runtime.McpHost.GetToolDefinitions().Count()}");

// chat list / fork print-and-exit sub-commands.
if (ctx.IsChatCommand && ChatCommands.TryHandleTerminal(runtime, args))
    return;

var session = ChatCommands.OpenOrCreate(runtime, args, ctx.IsChatCommand);
var chat = new ChatRuntime(runtime, session);

await InteractiveRepl.RunAsync(runtime, chat);
