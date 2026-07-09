using SPLA.Domain.Models;
using SPLA.Domain.Settings;
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

// --- Resolve settings ---
string? splaFile = null;
bool isChatCommand = false;
if (args.Length > 0 && args[0].Equals("chat", StringComparison.OrdinalIgnoreCase))
{
    isChatCommand = true;
    splaFile = ConfigLoader.FindProjectFile(Directory.GetCurrentDirectory());
}
else if (args.Length > 0 && args[0].EndsWith(".spla", StringComparison.OrdinalIgnoreCase))
{
    splaFile = args[0];
}
else
{
    splaFile = ConfigLoader.FindProjectFile(Directory.GetCurrentDirectory());
}

ResolvedSettings settings;
if (splaFile != null)
{
    Console.WriteLine($"Project file: {splaFile}");
    ConfigLoader.ScaffoldIfNew(splaFile);
    settings = ConfigLoader.LoadAndResolve(splaFile);
    Directory.SetCurrentDirectory(settings.WorkspacePath);
    SplaTelemetry.ConfigureProjectLogs(settings.Project.GetBucket("logs").MapToHostDirectory());
    Console.WriteLine($"Project: {settings.ProjectName ?? Path.GetFileNameWithoutExtension(splaFile)}");
}
else
{
    settings = ConfigLoader.LoadAndResolve();
    SplaTelemetry.ConfigureProjectLogs(settings.Project.GetBucket("logs").MapToHostDirectory());
}
logger.LogInformation("CLI startup. ProjectFile={ProjectFile} WorkspacePath={WorkspacePath} Mode={Mode}",
    splaFile, settings.WorkspacePath, settings.Mode);

Console.WriteLine($"Workspace: {settings.WorkspacePath}");
Console.WriteLine($"Endpoint:  {settings.Connections.FirstOrDefault()?.Endpoint ?? "(none)"}");
Console.WriteLine($"Mode:      {settings.Mode}");

// --- serve: run the WebSocket service ---
if (args.Length > 0 && args[0].Equals("serve", StringComparison.OrdinalIgnoreCase))
{
    await RunServeAsync(args, settings, loggerFactory);
    return;
}

// --- Shared agent stack ---
using var runtime = new AgentRuntime(settings, loggerFactory);
Console.WriteLine($"Tools registered: {runtime.McpHost.GetToolDefinitions().Count()}");

// --- chat sub-command ---
if (isChatCommand)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: spla chat <list|open|fork> [id] [--model name]");
        return;
    }
    var cmd = args[1].ToLower();
    if (cmd == "list")
    {
        Console.WriteLine("Saved chats:");
        foreach (var c in runtime.ChatManager.ListChats())
            Console.WriteLine($"- {c.Id} | {c.Title} | {c.UpdatedAt:dd.MM HH:mm}");
        return;
    }
    if (cmd == "fork" && args.Length > 2)
    {
        var model = args.Length > 4 && args[3] == "--model" ? args[4] : null;
        var forked = runtime.ChatManager.DuplicateChat(args[2], model);
        Console.WriteLine($"Forked to new chat: {forked.Id}");
        return;
    }
}

// --- Open or create chat ---
ChatSession session;
if (isChatCommand && args.Length > 2 && args[1].ToLower() == "open")
{
    session = runtime.ChatManager.LoadChat(args[2]) ?? runtime.ChatManager.CreateNewChat();
    Console.WriteLine($"Loaded chat: {session.Title}");
}
else
{
    session = runtime.ChatManager.CreateNewChat();
}

var chat = new ChatRuntime(runtime, session);

// --- REPL ---
while (true)
{
    Console.Write("\nYou: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

    if (input.Equals("/skills", StringComparison.OrdinalIgnoreCase))
    {
        var all = runtime.SkillManager.GetAll();
        if (all.Count == 0) Console.WriteLine("No skills available.");
        else foreach (var s in all)
            Console.WriteLine($"  [{(s.IsEnabled ? "on " : "off")}] {s.Id} — {s.Description}");
        continue;
    }

    if (input.StartsWith("/skills load ", StringComparison.OrdinalIgnoreCase))
    {
        var id = input["/skills load ".Length..].Trim();
        var body = runtime.SkillManager.LoadBody(id);
        if (body == null) { Console.WriteLine($"Skill '{id}' not found."); continue; }
        chat.InjectMessage(ChatRole.User, $"[Skill loaded: {id}]\n\n{body}");
        Console.WriteLine($"Skill '{id}' loaded into context.");
        continue;
    }

    var lastCliProgress = DateTime.MinValue;
    var callbacks = new SPLA.Agent.AgentCallbacks
    {
        OnDelta = chunk => { Console.Write(chunk); return Task.CompletedTask; },
        OnAssistantMessage = _ => { Console.WriteLine(); return Task.CompletedTask; },
        OnToolCallStarted = tc =>
        {
            Console.WriteLine($" -> Call: {tc.Function.Name}");
            return Task.CompletedTask;
        },
        OnToolProgress = (tc, progress) =>
        {
            var now = DateTime.UtcNow;
            if ((now - lastCliProgress).TotalMilliseconds < 150 && progress.Fraction < 1.0) return;
            lastCliProgress = now;
            var pct = progress.Fraction is double f ? $" {f * 100:0}%" : "";
            var detail = progress.Details is { Count: > 0 }
                ? "  " + string.Join("  ", progress.Details.Select(d => $"{d.Label}: {d.Value}"))
                : "";
            Console.Write($"\r    {tc.Function.Name}{pct} ({progress.Current}/{progress.Total}){detail}        ");
        },
        OnToolResult = (_, result) =>
        {
            Console.WriteLine($"\r -> Result received ({result.Length} chars).            ");
            return Task.CompletedTask;
        },
        OnNotice = note => { Console.WriteLine($"\n{note}"); return Task.CompletedTask; },
        OnTokenUsage = (prompt, completion) =>
        {
            runtime.TokenUsageProject.Record(prompt, completion);
            runtime.TokenUsageGlobal.Record(prompt, completion);
            if (prompt is int || completion is int)
            {
                var t = runtime.TokenUsageProject.Total;
                var g = runtime.TokenUsageGlobal.Total;
                Console.WriteLine(
                    $"   [tokens] turn in:{prompt?.ToString() ?? "?"} out:{completion?.ToString() ?? "?"}" +
                    $"  ·  project Σ {t.TotalTokens:N0} (in {t.PromptTokens:N0}/out {t.CompletionTokens:N0})" +
                    $"  ·  machine Σ {g.TotalTokens:N0}");
            }
        }
    };

    Func<ToolFunctionDefinition, string, Task<PermissionDecision>> permHandler = (def, argsJson) =>
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n[PERMISSION] Agent requests: {def.Name}");
        Console.WriteLine($"Arguments: {argsJson}");
        Console.Write("Allow? (y/N): ");
        Console.ResetColor();
        var ans = Console.ReadLine();
        return Task.FromResult(ans?.ToLower().StartsWith("y") == true
            ? PermissionDecision.AllowOnce
            : PermissionDecision.Deny);
    };

    Func<ClarifyRequest, Task<string?>> clarifyHandler = req =>
    {
        Console.WriteLine($"\n[?] {req.Question}");
        for (int i = 0; i < req.Options.Count; i++)
        {
            var o = req.Options[i];
            var desc = o.Description != null ? $" — {o.Description}" : "";
            Console.WriteLine($"  {i + 1}. {o.Label}{desc}");
        }
        Console.Write("Choice (number, or Enter to skip): ");
        var line = Console.ReadLine()?.Trim();
        return Task.FromResult(int.TryParse(line, out var idx) && idx >= 1 && idx <= req.Options.Count
            ? req.Options[idx - 1].Label
            : (string?)null);
    };

    Console.Write("SPLA: ");
    try
    {
        await chat.SendAsync(input, callbacks, permHandler, clarifyHandler, CancellationToken.None);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[Error]: {ex.Message}");
    }
}

// ── serve command ────────────────────────────────────────────────────────────
static async Task RunServeAsync(string[] args, ResolvedSettings settings, ILoggerFactory loggerFactory)
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

    using var registry = new AgentRuntimeRegistry(loggerFactory)
    {
        DefaultProjectId = settings.ProjectFilePath ?? AgentRuntimeRegistry.NoProjectId
    };
    // Same cached entry Build() resolves internally — opening it here first just lets the parallel
    // REPL (below) drive the identical runtime/chats a socket client would.
    var (runtime, chats) = registry.Open(registry.DefaultProjectId);
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
        var explicitExit = await RunServeReplAsync(runtime, chats);
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

static async Task<bool> RunServeReplAsync(AgentRuntime runtime, ChatRegistry chats)
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

        var callbacks = new SPLA.Agent.AgentCallbacks
        {
            OnDelta = chunk => { Console.Write(chunk); return Task.CompletedTask; },
            OnAssistantMessage = _ => { Console.WriteLine(); return Task.CompletedTask; },
            OnToolCallStarted = tc => { Console.WriteLine($" -> Call: {tc.Function.Name}"); return Task.CompletedTask; },
            OnToolResult = (_, result) => { Console.WriteLine($" -> Result ({result.Length} chars)"); return Task.CompletedTask; },
            OnNotice = note => { Console.WriteLine($"\n{note}"); return Task.CompletedTask; }
        };

        Func<ToolFunctionDefinition, string, Task<PermissionDecision>> perm = (def, argsJson) =>
        {
            Console.Write($"\n[PERMISSION] {def.Name} {argsJson}\nAllow? (y/N): ");
            var ans = Console.ReadLine();
            return Task.FromResult(ans?.Trim().ToLower().StartsWith("y") == true
                ? PermissionDecision.AllowOnce
                : PermissionDecision.Deny);
        };

        Func<ClarifyRequest, Task<string?>> clarify = req =>
        {
            Console.WriteLine($"\n[?] {req.Question}");
            for (int i = 0; i < req.Options.Count; i++)
                Console.WriteLine($"  {i + 1}. {req.Options[i].Label}");
            Console.Write("Choice: ");
            var line = Console.ReadLine()?.Trim();
            return Task.FromResult(int.TryParse(line, out var idx) && idx >= 1 && idx <= req.Options.Count
                ? req.Options[idx - 1].Label
                : (string?)null);
        };

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
