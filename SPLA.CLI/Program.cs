using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.LMStudio;
using SPLA.MCP.Core;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.BasicTools.SystemTools;
using SPLA.Observability;
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

// Check command-line args
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
    // Auto-detect *.spla in current directory
    splaFile = ConfigLoader.FindProjectFile(Directory.GetCurrentDirectory());
}

ResolvedSettings settings;
if (splaFile != null)
{
    Console.WriteLine($"Project file: {splaFile}");
    settings = ConfigLoader.LoadAndResolve(splaFile);
    Directory.SetCurrentDirectory(settings.WorkspacePath);
    SplaTelemetry.ConfigureProjectLogs(settings.WorkspacePath);
    Console.WriteLine($"Project: {settings.ProjectName ?? Path.GetFileNameWithoutExtension(splaFile)}");
}
else
{
    settings = ConfigLoader.LoadAndResolve();
    SplaTelemetry.ConfigureProjectLogs(settings.WorkspacePath);
}
logger.LogInformation("CLI startup. ProjectFile={ProjectFile} WorkspacePath={WorkspacePath} Mode={Mode}", splaFile, settings.WorkspacePath, settings.Mode);

Console.WriteLine($"Workspace: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"Endpoint:  {settings.Endpoint}");
Console.WriteLine($"Mode:      {settings.Mode}");

// --- serve: run the WebSocket service, optionally with a parallel console REPL ---
// Same agent stack as the interactive REPL below, but exposed over the network so any client
// (Avalonia thin client, browser, phone) drives it. With --repl the console becomes just another
// входной канал alongside the sockets, sharing one chat registry.
if (args.Length > 0 && args[0].Equals("serve", StringComparison.OrdinalIgnoreCase))
{
    await RunServeAsync(args, settings, loggerFactory);
    return;
}

// --- Init LLM + MCP ---
using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
var llmClient = new LMStudioClient(httpClient, loggerFactory.CreateLogger<LMStudioClient>());
var llmSettings = settings.ToLLMSettings();

var pluginsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

var skillManager = new SPLA.MCP.Core.Plugins.SkillManager(loggerFactory.CreateLogger<SPLA.MCP.Core.Plugins.SkillManager>());
skillManager.LoadSkills(pluginsDir);

var pluginManager = new SPLA.MCP.Core.Plugins.PluginManager(settings, loggerFactory.CreateLogger<SPLA.MCP.Core.Plugins.PluginManager>(), skillManager);
pluginManager.LoadPlugins(pluginsDir);
skillManager.ApplySettings(settings.Skills.ToDictionary(
    kvp => kvp.Key,
    kvp => (kvp.Value.Enabled ?? true, kvp.Value.Preloaded ?? false)));

var mcpHost = new McpHost(
    new SPLA.MCP.Core.Permissions.PermissionManager(),
    pluginManager,
    loggerFactory.CreateLogger<McpHost>());
Func<ToolFunctionDefinition, string, Task<PermissionDecision>> permissionHandler = async (def, args) =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n[PERMISSION] Agent requests: {def.Name}");
    Console.WriteLine($"Arguments: {args}");
    Console.Write("Allow? (y/N): ");
    Console.ResetColor();
    var ans = Console.ReadLine();
    return ans?.ToLower().StartsWith("y") == true ? PermissionDecision.AllowOnce : PermissionDecision.Deny;
};
mcpHost.RegisterTool(new FsListTool());
mcpHost.RegisterTool(new FsReadTool());
mcpHost.RegisterTool(new FsSearchTextTool());
mcpHost.RegisterTool(new FsFindFilesTool());
mcpHost.RegisterTool(new FsCreateTool());
mcpHost.RegisterTool(new FsPatchTool());
mcpHost.RegisterTool(new FsWriteTool());
mcpHost.RegisterTool(new FsDeleteTool());
mcpHost.RegisterTool(new RunCommandTool());
mcpHost.RegisterTool(new GetContextTool());
mcpHost.RegisterTool(new GetCurrentDateTimeTool());
mcpHost.RegisterTool(new SPLA.MCP.BasicTools.Network.WebFetchTool());
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentInfoTool(mcpHost, skillManager));

// Fundamental agent working memory: session (this chat) + project (shared, persistent).
var projectKv = new ProjectKvStore(settings);
var sessionKv = new KeyValueStore("session");
// Session-scoped tools resolve the active session via AgentSessionScope (opened around the run loop).
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemorySetTool(projectKv.Store));
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryGetTool(projectKv.Store));
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryDeleteTool(projectKv.Store));
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryListTool(projectKv.Store));
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryClearTool(projectKv.Store));

var skillSession = new SPLA.Domain.Agent.SkillSession();
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.SkillActivateTool(skillManager));
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.SkillDeactivateTool());
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentClarifyTool());
var spawnedRunner = new SPLA.Agent.SpawnedAgentRunner(llmClient, mcpHost, skillManager, pluginManager, settings);
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentSpawnTool(spawnedRunner));
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentSpawnBatchTool(spawnedRunner));
var checkpointManager = new SPLA.Agent.CheckpointManager();
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.ContextCheckpointSetTool());
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.ContextCheckpointRestoreTool());
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.MarkSetTool());
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.MarkRollbackTool());

// This CLI session's agent state, made ambient around each run so the tools above resolve it.
var cliAgentSession = new SPLA.Domain.Agent.AgentSession(sessionKv, checkpointManager, skillSession);

var tools = mcpHost.GetToolDefinitions().ToList();
Console.WriteLine($"Tools registered: {tools.Count}");

var chatManager = new ChatManager(settings);

if (isChatCommand)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: spla chat <list|open|compact|fork> [id] [--model name]");
        return;
    }
    
    var cmd = args[1].ToLower();
    if (cmd == "list")
    {
        Console.WriteLine("Saved chats:");
        foreach (var c in chatManager.ListChats())
        {
            Console.WriteLine($"- {c.Id} | {c.Title} | {c.UpdatedAt:dd.MM HH:mm}");
        }
        return;
    }
    else if (cmd == "open" && args.Length > 2)
    {
        // Handled below by loading the chat
    }
    else if (cmd == "compact" && args.Length > 2)
    {
        Console.WriteLine($"Compacting {args[2]} not fully implemented in CLI standalone mode yet. Use UI or /compact inside chat.");
        return;
    }
    else if (cmd == "fork" && args.Length > 2)
    {
        var model = args.Length > 4 && args[3] == "--model" ? args[4] : null;
        var forked = chatManager.DuplicateChat(args[2], model);
        Console.WriteLine($"Forked to new chat: {forked.Id}");
        return;
    }
}

ChatSession currentChat;
if (isChatCommand && args.Length > 2 && args[1].ToLower() == "open")
{
    currentChat = chatManager.LoadChat(args[2]) ?? chatManager.CreateNewChat();
    Console.WriteLine($"Loaded chat: {currentChat.Title}");
}
else
{
    currentChat = chatManager.CreateNewChat();
}

// --- Build system prompt (shared core: same builder the UI uses) ---
var currentDir = Directory.GetCurrentDirectory();
var promptBuilder = new SPLA.Agent.SystemPromptBuilder(skillManager, pluginManager);
var systemPrompt = promptBuilder.Build(settings, currentDir);

// Restore this chat's session memory (survives restart) and feed live context:* into each turn.
sessionKv.LoadFrom(currentChat.Kv);

var orchestrator = new SPLA.Agent.ConversationOrchestrator(llmClient, mcpHost)
{
    WorkingMemory = () => CollectWorkingMemory(sessionKv, projectKv.Store),
    Checkpoint = checkpointManager,
    Logger = loggerFactory.CreateLogger<SPLA.Agent.ConversationOrchestrator>()
};

// Persistent token tallies — cumulative across every run and chat. Project-scoped + machine-global.
SPLA.Domain.Interfaces.ITokenUsageStore tokenUsage = new SPLA.Domain.Agent.FileTokenUsageStore(
    System.IO.Path.Combine(settings.WorkspacePath, ".spla", "token-usage.json"));
SPLA.Domain.Interfaces.ITokenUsageStore tokenUsageGlobal = new SPLA.Domain.Agent.FileTokenUsageStore(
    System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".spla", "token-usage.json"));

var conversation = new Conversation();
conversation.Add(new ChatMessage { Role = ChatRole.System, Content = systemPrompt });

// Load existing messages if opening chat
foreach (var m in currentChat.Messages)
{
    var role = m.Role.ToLower() switch
    {
        "user" => ChatRole.User,
        "assistant" => ChatRole.Assistant,
        "tool" => ChatRole.Tool,
        _ => ChatRole.System
    };
    conversation.Add(new ChatMessage { Role = role, Content = m.Content });
}

// --- Chat loop ---
while (true)
{
    Console.Write("\nYou: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase)) break;

    if (input.StartsWith("/compact"))
    {
        Console.WriteLine("Use UI to trigger /compact compression for now.");
        continue;
    }

    if (input.Equals("/skills", StringComparison.OrdinalIgnoreCase))
    {
        var all = skillManager.GetAll();
        if (all.Count == 0) { Console.WriteLine("No skills available."); }
        else foreach (var s in all)
            Console.WriteLine($"  [{(s.IsEnabled ? "on " : "off")}] {s.Id} — {s.Description}");
        continue;
    }

    if (input.StartsWith("/skills load ", StringComparison.OrdinalIgnoreCase))
    {
        var id = input["/skills load ".Length..].Trim();
        var body = skillManager.LoadBody(id);
        if (body == null) { Console.WriteLine($"Skill '{id}' not found."); continue; }
        conversation.Add(new ChatMessage { Role = ChatRole.User, Content = $"[Skill loaded: {id}]\n\n{body}" });
        Console.WriteLine($"Skill '{id}' loaded into context.");
        SaveCliChat(chatManager, currentChat, conversation, sessionKv);
        continue;
    }

    var requestId = Guid.NewGuid().ToString("N");
    conversation.Add(new ChatMessage { Role = ChatRole.User, Content = input });
    SaveCliChat(chatManager, currentChat, conversation, sessionKv);

    using var requestTelemetryContext = SplaTelemetry.PushContext(new(
        ConversationId: currentChat.Id,
        RequestId: requestId,
        ProjectId: settings.ProjectName,
        WorkspacePath: settings.WorkspacePath));

    Console.Write("SPLA: ");
    var lastCliProgress = DateTime.MinValue;
    var callbacks = new SPLA.Agent.AgentCallbacks
    {
        OnDelta = chunk => { Console.Write(chunk); return Task.CompletedTask; },
        OnAssistantMessage = msg =>
        {
            Console.WriteLine();
            SaveCliChat(chatManager, currentChat, conversation, sessionKv);
            return Task.CompletedTask;
        },
        OnToolCallStarted = tc =>
        {
            Console.WriteLine($" -> Call: {tc.Function.Name}");
            return Task.CompletedTask;
        },
        OnToolProgress = (tc, progress) =>
        {
            // Overwrite a single line with the latest tick. Throttled by time to avoid console spam.
            var now = DateTime.UtcNow;
            if ((now - lastCliProgress).TotalMilliseconds < 150 && progress.Fraction < 1.0) return;
            lastCliProgress = now;

            var pct = progress.Fraction is double f ? $" {f * 100:0}%" : "";
            var detail = progress.Details is { Count: > 0 }
                ? "  " + string.Join("  ", progress.Details.Select(d => $"{d.Label}: {d.Value}"))
                : "";
            Console.Write($"\r    {tc.Function.Name}{pct} ({progress.Current}/{progress.Total}){detail}        ");
        },
        OnToolResult = (tc, result) =>
        {
            Console.WriteLine($"\r -> Result received ({result.Length} chars).            ");
            SaveCliChat(chatManager, currentChat, conversation, sessionKv);
            return Task.CompletedTask;
        },
        OnNotice = note => { Console.WriteLine($"\n{note}"); return Task.CompletedTask; },
        OnTokenUsage = (prompt, completion) =>
        {
            tokenUsage.Record(prompt, completion);
            tokenUsageGlobal.Record(prompt, completion);
            if (prompt is int || completion is int)
            {
                var t = tokenUsage.Total;
                var g = tokenUsageGlobal.Total;
                Console.WriteLine(
                    $"   [tokens] turn in:{prompt?.ToString() ?? "?"} out:{completion?.ToString() ?? "?"}" +
                    $"  ·  project Σ {t.TotalTokens:N0} (in {t.PromptTokens:N0}/out {t.CompletionTokens:N0})" +
                    $"  ·  machine Σ {g.TotalTokens:N0}");
            }
        }
    };

    try
    {
        using var clarifyScope = SPLA.Domain.Tools.ClarifyScope.Begin(async req =>
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
            if (int.TryParse(line, out var idx) && idx >= 1 && idx <= req.Options.Count)
                return req.Options[idx - 1].Label;
            return null;
        });

        using var agentScope = SPLA.Domain.Agent.AgentSessionScope.Begin(cliAgentSession);
        using var permScope  = SPLA.MCP.Core.Permissions.PermissionScope.Begin(permissionHandler);
        await orchestrator.RunAsync(conversation, llmSettings, settings.Mode, callbacks);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[Error]: {ex.Message}");
    }
}

static void SaveCliChat(ChatManager manager, ChatSession chat, Conversation conversation, IKeyValueStore sessionKv)
{
    chat.Messages.Clear();
    foreach (var m in conversation.Persistable)
    {
        chat.Messages.Add(new ChatSessionMessage
        {
            Role = m.Role.ToString().ToLower(),
            Content = m.Content ?? "",
            Reasoning = string.IsNullOrEmpty(m.Reasoning) ? null : m.Reasoning
        });
    }
    chat.Kv = ((KeyValueStore)sessionKv).Snapshot();
    manager.SaveChat(chat);
}

static IReadOnlyList<(string scope, string key, string value)> CollectWorkingMemory(IKeyValueStore session, IKeyValueStore project)
    => session.List().Select(kv => (session.Scope, kv.Key, kv.Value))
        .Concat(project.List().Select(kv => (project.Scope, kv.Key, kv.Value)))
        .ToList();

// ── serve command ────────────────────────────────────────────────────────────
static async Task RunServeAsync(string[] args, ResolvedSettings settings, ILoggerFactory loggerFactory)
{
    // The CLI had no global exception net, so a fault on a background turn task killed the whole
    // process silently (nothing reached the log). Install one: log + print, never let it vanish.
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

    using var runtime = new SPLA.Service.AgentRuntime(settings, loggerFactory);
    var chats = new SPLA.Service.ChatRegistry(runtime);
    var options = new SPLA.Service.ServiceOptions { Port = port, Bind = bind, Token = token };
    var host = SPLA.Service.SplaServiceHost.Build(runtime, options, chats);
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
        // Only an explicit 'exit' stops the service. A null/EOF stdin (e.g. piped, or focus lost)
        // must NOT kill the host — the sockets keep serving; wait for Ctrl+C instead.
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

// A minimal console chat that drives a shared ChatRuntime — proves console + sockets share one agent.
// Returns true only when the user explicitly typed 'exit'/'quit'; false on EOF (so the caller keeps
// the service alive rather than tearing it down).
static async Task<bool> RunServeReplAsync(SPLA.Service.AgentRuntime runtime, SPLA.Service.ChatRegistry chats)
{
    var chat = chats.CreateNew("Console");
    Console.WriteLine($"[console chat: {chat.ChatId}]");

    while (true)
    {
        Console.Write("\nYou: ");
        var input = Console.ReadLine();
        if (input == null) return false; // EOF — stdin closed, keep the service running
        if (string.IsNullOrWhiteSpace(input)) continue;
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase)) return true;

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
                : null);
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
