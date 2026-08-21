using SPLA.Runtime;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Project;
using SPLA.Domain.Settings;
using SPLA.Instances;
using SPLA.Service;

namespace SPLA.CLI;

/// <summary>The <c>spla serve</c> command: installs crash logging, parses the serve flags, builds and
/// starts the WebSocket <see cref="SplaServiceHost"/>, and optionally runs a parallel console REPL
/// against the same runtime a socket client would drive.</summary>
internal static class ServeCommand
{
    public static async Task RunAsync(
        int port, string bind, string? token, bool repl, string? initialChatMessage,
        int idleTimeoutMinutes, string? registryUrl, string? registryToken,
        ResolvedSettings settings, ILoggerFactory loggerFactory)
    {
        InstallCrashLogging(loggerFactory);

        using var registry = new AgentRuntimeRegistry(loggerFactory)
        {
            DefaultProjectId = settings.ProjectFilePath ?? AgentRuntimeRegistry.NoProjectId,
            InstanceMode = "serve"
        };
        // Same cached entry Build() resolves internally — opening it here first just lets the parallel
        // REPL (below) drive the identical runtime/chats a socket client would.
        var (runtime, chats) = registry.Open(registry.DefaultProjectId);
        var options = new ServiceOptions
        {
            // An explicit --port beats the project's remembered one — a flag typed for this run is a
            // stronger statement than what the project asks for by default.
            Port = port != 0 ? port : (settings.McpPort ?? 0),
            Bind = bind,
            // A reference (secret:user:...) resolves through the project's store; anything else is
            // taken literally, so an existing --token on a command line keeps working. The reference
            // form exists because a literal is readable in the process list by anyone on the machine.
            Token = settings.SecretResolver.Resolve(token),
            InitialChatMessage = initialChatMessage,
            IdleTimeout = idleTimeoutMinutes > 0 ? TimeSpan.FromMinutes(idleTimeoutMinutes) : TimeSpan.Zero,
            McpEnabled = settings.McpEnabled
        };
        var host = SplaServiceHost.Build(registry, options);
        await host.StartAsync();

        // Published only now: an address written before the listener accepts connections is a lie for
        // however long startup takes, and the whole point of publishing it is that somebody else will
        // dial it instead of opening a second writer.
        runtime.Instance?.Publish(host.Url);

        var wsUrl = host.Url.Replace("http://", "ws://") + "/ws";
        Console.WriteLine($"\nSPLA service listening on {host.Url}  (WebSocket: {wsUrl})");
        if (token == null && !options.IsLoopback)
            Console.WriteLine("WARNING: bound to a non-loopback address without --token — anyone who can reach this port controls the agent.");

        var stop = new TaskCompletionSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.TrySetResult(); };

        // Told about a hub, this instance announces itself to it and keeps saying what it is doing.
        // Registration is a view, never a dependency: an unreachable hub costs this process nothing —
        // the project is still locked, clients still dial the endpoint directly, and the registrar
        // simply keeps retrying in the background.
        await using var registrar = StartRegistrar(
            registryUrl, settings.SecretResolver.Resolve(registryToken), registry, runtime, host, settings,
            force => { if (force) runtime.Turns.CancelAll(); stop.TrySetResult(); return Task.CompletedTask; },
            loggerFactory);

        // Nothing owns this instance; it holds a lease. When there is nobody connected and nothing
        // running for long enough, it lets go — see InstanceLease for why ownership was the wrong
        // model. Off by default; a window that spawned this process passes --idle-timeout.
        host.LeaseExpired += () =>
        {
            Console.WriteLine($"\nIdle for {idleTimeoutMinutes} min with no clients — stopping.");
            stop.TrySetResult();
        };

        // A client asked this instance to stop (spla stop) and the request was accepted or forced —
        // same stop signal the idle lease uses, so an explicit shutdown and an idle one wind down the
        // same way.
        registry.ShutdownRequested += () =>
        {
            Console.WriteLine("\nShutdown requested over the wire — stopping.");
            stop.TrySetResult();
        };

        if (repl)
        {
            Console.WriteLine("Parallel console REPL active. Type a message, or 'exit' to stop the service.");
            // A person typing at this terminal is a user of the instance even though they are not a
            // socket; without the hold the lease would count the instance as unattended.
            using var consoleHold = host.HoldLease();
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

    /// <summary>
    /// Builds and starts the hub registrar, or returns null when no hub was named.
    ///
    /// <para>The status callback samples rather than caches: the registrar asks the runtime what it
    /// is doing each tick, so nothing can leave a stale state behind by forgetting to publish one.</para>
    /// </summary>
    private static InstanceRegistrar? StartRegistrar(
        string? registryUrl, string? token, AgentRuntimeRegistry registry, AgentRuntime runtime,
        SplaServiceHost host, ResolvedSettings settings, Func<bool, Task> onStop, ILoggerFactory loggers)
    {
        if (string.IsNullOrWhiteSpace(registryUrl) || runtime.Instance?.Info is not { } info) return null;

        var registrar = new InstanceRegistrar(
            registryUrl,
            token,
            new RegisterFrame
            {
                ProjectId = settings.ProjectFilePath ?? AgentRuntimeRegistry.NoProjectId,
                ProjectName = settings.ProjectName,
                Info = info
            },
            () => new StatusFrame
            {
                State = InstanceStates.Name(registry.State(TimeSpan.FromMinutes(10))),
                Clients = host.ClientCount
            },
            onStop,
            loggers.CreateLogger("registry"));

        registrar.Start();
        Console.WriteLine($"Registering with hub {registryUrl}.");
        return registrar;
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
