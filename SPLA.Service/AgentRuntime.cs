using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Domain.Agent;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.LMStudio;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.BasicTools.SystemTools;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Plugins;

namespace SPLA.Service;

/// <summary>
/// The shared agent stack, built once for a service instance. This is the same construction the CLI
/// does imperatively in its <c>Program.cs</c>, lifted into a reusable object so the CLI, the service
/// host, and (later) the embedded Avalonia client all stand up the agent identically instead of
/// keeping three divergent copies.
/// <para>
/// Everything here is process-wide and chat-agnostic: the LLM client, the tool host with every tool
/// registered, the plugin/skill managers, the project-scoped KV, the chat store, the system prompt.
/// Per-chat state (the conversation, session KV, checkpoint, agent session, orchestrator) lives in
/// <see cref="ChatRuntime"/> — created per chat and resolved by the tools through the ambient
/// <see cref="AgentSessionScope"/>/<see cref="PermissionScope"/>, exactly as the UI's per-chat
/// autonomy model already works.
/// </para>
/// </summary>
public sealed class AgentRuntime : IDisposable
{
    private readonly HttpClient _httpClient;

    public ResolvedSettings Settings { get; }
    public ILoggerFactory LoggerFactory { get; }

    /// <summary>Process-wide domain-event hub. Mutators publish state changes here; the host fans them
    /// out to clients. The single "say what changed once" point — see <see cref="ServiceEvents"/>.</summary>
    public ServiceEvents Events { get; } = new();
    public LMStudioClient Llm { get; }
    public IModelManagementService ModelManagement { get; }
    public McpHost McpHost { get; }
    public SkillManager SkillManager { get; }
    public PluginManager PluginManager { get; }
    public ProjectKvStore ProjectKv { get; }
    public ChatManager ChatManager { get; }
    public SystemPromptBuilder PromptBuilder { get; }
    public SpawnedAgentRunner SpawnedRunner { get; }

    /// <summary>Project-lifetime token tally (workspace/.spla/token-usage.json).</summary>
    public SPLA.Domain.Interfaces.ITokenUsageStore TokenUsageProject { get; }

    /// <summary>Machine-global token tally (~/.spla/token-usage.json).</summary>
    public SPLA.Domain.Interfaces.ITokenUsageStore TokenUsageGlobal { get; }

    /// <summary>The system prompt for this workspace, built once at startup.</summary>
    public string SystemPrompt { get; }

    public AgentMode Mode => Settings.Mode;

    /// <summary>Cached reachability per connection id. null = not yet checked.</summary>
    public ConcurrentDictionary<string, (bool Ok, string? Error)> ConnectionHealth { get; } = new();

    /// <summary>Schemas registered by plugins at startup (data + UI, resolved by name).</summary>
    public SPLA.Domain.Editor.SchemaRegistry SchemaRegistry { get; }

    public AgentRuntime(ResolvedSettings settings, ILoggerFactory loggerFactory)
    {
        Settings = settings;
        LoggerFactory = loggerFactory;

        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        Llm = new LMStudioClient(_httpClient, loggerFactory.CreateLogger<LMStudioClient>());
        ModelManagement = new LMStudioManagementClient(_httpClient);

        var pluginsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

        SkillManager = new SkillManager(loggerFactory.CreateLogger<SkillManager>());
        SkillManager.LoadSkills(pluginsDir);

        PluginManager = new PluginManager(settings, loggerFactory.CreateLogger<PluginManager>(), SkillManager);
        PluginManager.LoadPlugins(pluginsDir);

        SchemaRegistry = new SPLA.Domain.Editor.SchemaRegistry();
        foreach (var p in PluginManager.GetSchemaProviders())
            SchemaRegistry.Register(p);

        SkillManager.ApplySettings(settings.Skills.ToDictionary(
            kvp => kvp.Key,
            kvp => (kvp.Value.Enabled ?? true, kvp.Value.Preloaded ?? false)));

        McpHost = new McpHost(new PermissionManager(), PluginManager, loggerFactory.CreateLogger<McpHost>());

        // ── Basic tools (mirrors CLI registration) ──────────────────────────
        McpHost.RegisterTool(new FsListTool());
        McpHost.RegisterTool(new FsReadTool());
        McpHost.RegisterTool(new FsSearchTextTool());
        McpHost.RegisterTool(new FsFindFilesTool());
        McpHost.RegisterTool(new FsCreateTool());
        McpHost.RegisterTool(new FsPatchTool());
        McpHost.RegisterTool(new FsWriteTool());
        McpHost.RegisterTool(new FsDeleteTool());
        McpHost.RegisterTool(new RunCommandTool());
        McpHost.RegisterTool(new GetContextTool());
        McpHost.RegisterTool(new GetCurrentDateTimeTool());
        McpHost.RegisterTool(new SPLA.MCP.BasicTools.Network.WebFetchTool());
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentInfoTool(McpHost, SkillManager));

        // ── Fundamental agent working memory (project-scoped shared; session resolves via scope) ──
        ProjectKv = new ProjectKvStore(settings);
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemorySetTool(ProjectKv.Store));
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryGetTool(ProjectKv.Store));
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryDeleteTool(ProjectKv.Store));
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryListTool(ProjectKv.Store));
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryClearTool(ProjectKv.Store));
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.ImageViewTool());

        // ── Skill lifecycle + spawn + checkpoint/mark (resolve per-chat via ambient scope) ──
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.SkillActivateTool(SkillManager));
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.SkillDeactivateTool());
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentClarifyTool());
        SpawnedRunner = new SpawnedAgentRunner(Llm, McpHost, SkillManager, PluginManager, settings);
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentSpawnTool(SpawnedRunner));
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentSpawnBatchTool(SpawnedRunner));
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.ContextCheckpointSetTool());
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.ContextCheckpointRestoreTool());
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.MarkSetTool());
        McpHost.RegisterTool(new SPLA.MCP.Core.Tools.MarkRollbackTool());

        ChatManager = new ChatManager(settings);

        PromptBuilder = new SystemPromptBuilder(SkillManager, PluginManager);
        SystemPrompt = PromptBuilder.Build(settings, settings.WorkspacePath);

        TokenUsageProject = new FileTokenUsageStore(
            Path.Combine(settings.WorkspacePath, ".spla", "token-usage.json"));
        TokenUsageGlobal = new FileTokenUsageStore(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".spla", "token-usage.json"));
    }

    public void Dispose() => _httpClient.Dispose();
}
