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

// --- Init LLM + MCP ---
using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
var llmClient = new LMStudioClient(httpClient);
var llmSettings = settings.ToLLMSettings();

var pluginManager = new SPLA.MCP.Core.Plugins.PluginManager(settings, loggerFactory.CreateLogger<SPLA.MCP.Core.Plugins.PluginManager>());
var pluginsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
pluginManager.LoadPlugins(pluginsDir);

var skillManager = new SPLA.MCP.Core.Plugins.SkillManager(loggerFactory.CreateLogger<SPLA.MCP.Core.Plugins.SkillManager>());
skillManager.LoadSkills(pluginsDir);
skillManager.ApplySettings(settings.Skills.ToDictionary(
    kvp => kvp.Key,
    kvp => (kvp.Value.Enabled ?? true, kvp.Value.Preloaded ?? false)));

var mcpHost = new McpHost(
    new SPLA.MCP.Core.Permissions.PermissionManager(),
    pluginManager,
    loggerFactory.CreateLogger<McpHost>());
mcpHost.OnPermissionRequested = async (def, args) =>
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
mcpHost.RegisterTool(new SPLA.MCP.BasicTools.Network.WebFetchTool());
mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentInfoTool(mcpHost, skillManager));

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

var orchestrator = new SPLA.Agent.ConversationOrchestrator(llmClient, mcpHost);

var messages = new List<ChatMessage>
{
    new ChatMessage { Role = ChatRole.System, Content = systemPrompt }
};

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
    messages.Add(new ChatMessage { Role = role, Content = m.Content });
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
        messages.Add(new ChatMessage { Role = ChatRole.User, Content = $"[Skill loaded: {id}]\n\n{body}" });
        Console.WriteLine($"Skill '{id}' loaded into context.");
        SaveCliChat(chatManager, currentChat, messages);
        continue;
    }

    var requestId = Guid.NewGuid().ToString("N");
    messages.Add(new ChatMessage { Role = ChatRole.User, Content = input });
    SaveCliChat(chatManager, currentChat, messages);

    using var requestTelemetryContext = SplaTelemetry.PushContext(new(
        ConversationId: currentChat.Id,
        RequestId: requestId,
        ProjectId: settings.ProjectName,
        WorkspacePath: settings.WorkspacePath));

    Console.Write("SPLA: ");
    var callbacks = new SPLA.Agent.AgentCallbacks
    {
        OnDelta = chunk => { Console.Write(chunk); return Task.CompletedTask; },
        OnAssistantMessage = msg =>
        {
            Console.WriteLine();
            SaveCliChat(chatManager, currentChat, messages);
            return Task.CompletedTask;
        },
        OnToolCallStarted = tc =>
        {
            Console.WriteLine($" -> Call: {tc.Function.Name}");
            return Task.CompletedTask;
        },
        OnToolResult = (tc, result) =>
        {
            Console.WriteLine($" -> Result received ({result.Length} chars).");
            SaveCliChat(chatManager, currentChat, messages);
            return Task.CompletedTask;
        },
        OnNotice = note => { Console.WriteLine($"\n{note}"); return Task.CompletedTask; }
    };

    try
    {
        await orchestrator.RunAsync(messages, llmSettings, settings.Mode, callbacks);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[Error]: {ex.Message}");
    }
}

static void SaveCliChat(ChatManager manager, ChatSession chat, List<ChatMessage> msgs)
{
    chat.Messages.Clear();
    foreach (var m in msgs.Where(x => x.Role != ChatRole.System))
    {
        chat.Messages.Add(new ChatSessionMessage
        {
            Role = m.Role.ToString().ToLower(),
            Content = m.Content ?? ""
        });
    }
    manager.SaveChat(chat);
}
