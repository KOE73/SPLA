using SPLA.Runtime;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Settings;
using SPLA.Service;

namespace SPLA.CLI;

/// <summary>The <c>spla serve</c> command: installs crash logging, parses the serve flags, builds and
/// starts the WebSocket <see cref="SplaServiceHost"/>, and optionally runs a parallel console REPL
/// against the same runtime a socket client would drive.</summary>
internal static class ServeCommand
{
    public static async Task RunAsync(string[] args, ResolvedSettings settings, ILoggerFactory loggerFactory)
    {
        InstallCrashLogging(loggerFactory);

        var (port, bind, token, repl) = ParseArgs(args);

        using var registry = new AgentRuntimeRegistry(loggerFactory)
        {
            DefaultProjectId = settings.ProjectFilePath ?? AgentRuntimeRegistry.NoProjectId
        };
        // Same cached entry Build() resolves internally — opening it here first just lets the parallel
        // REPL (below) drive the identical runtime/chats a socket client would.
        var (_, chats) = registry.Open(registry.DefaultProjectId);
        var options = new ServiceOptions { Port = port, Bind = bind, Token = token };
        var host = SplaServiceHost.Build(registry, options);
        await host.StartAsync();

        var wsUrl = host.Url.Replace("http://", "ws://") + "/ws";
        Console.WriteLine($"\nSPLA service listening on {host.Url}  (WebSocket: {wsUrl})");
        if (token == null && !options.IsLoopback)
            Console.WriteLine("WARNING: bound to a non-loopback address without --token — anyone who can reach this port controls the agent.");

        var stop = new TaskCompletionSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.TrySetResult(); };

        if (repl)
        {
            Console.WriteLine("Parallel console REPL active. Type a message, or 'exit' to stop the service.");
            var explicitExit = await RunReplAsync(chats);
            if (!explicitExit)
            {
                Console.WriteLine("Console input closed — service still running. Press Ctrl+C to stop.");
                await stop.Task;
            }
        }
        else
        {
            Console.WriteLine("Press Ctrl+C to stop.");
            await stop.Task;
        }

        await host.StopAsync();
    }

    private static (int Port, string Bind, string? Token, bool Repl) ParseArgs(string[] args)
    {
        int port = 5050;
        string bind = "127.0.0.1";
        string? token = null;
        bool repl = false;

        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--repl": repl = true; break;
                case "--port": if (i + 1 < args.Length && int.TryParse(args[++i], out var p)) port = p; break;
                case "--bind": if (i + 1 < args.Length) bind = args[++i]; break;
                case "--token": if (i + 1 < args.Length) token = args[++i]; break;
            }
        }

        return (port, bind, token, repl);
    }

    private static void InstallCrashLogging(ILoggerFactory loggerFactory)
    {
        var crashLog = loggerFactory.CreateLogger("serve");
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            crashLog.LogCritical(ex, "Unhandled exception (terminating={Terminating})", e.IsTerminating);
            Console.Error.WriteLine($"\n[FATAL] {ex}");
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            crashLog.LogError(e.Exception, "Unobserved task exception");
            Console.Error.WriteLine($"\n[UNOBSERVED] {e.Exception}");
            e.SetObserved();
        };
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            crashLog.LogInformation("serve process exiting.");
    }

    /// <summary>The parallel console chat attached to a running service. Returns true on an explicit
    /// <c>exit</c>/<c>quit</c> (stop the service), false when stdin simply closed (keep serving).</summary>
    private static async Task<bool> RunReplAsync(ChatRegistry chats)
    {
        var chat = chats.CreateNew("Console");
        Console.WriteLine($"[console chat: {chat.ChatId}]");

        while (true)
        {
            Console.Write("\nYou: ");
            var input = Console.ReadLine();
            if (input == null) return false;
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("quit", StringComparison.OrdinalIgnoreCase)) return true;

            var callbacks = ConsoleHandlers.BasicCallbacks();
            var perm = ConsoleHandlers.Permission(colored: false);
            var clarify = ConsoleHandlers.Clarify();

            Console.Write("SPLA: ");
            try
            {
                await chat.SendAsync(input, callbacks, perm, clarify, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error]: {ex.Message}");
            }
        }
    }
}
