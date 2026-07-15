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
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Plugins;

namespace SPLA.Runtime;

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

    /// <summary>The <c>agent.capabilities</c> setting resolved against <see cref="AgentFeatureCatalog"/>:
    /// unknown ids dropped, Requires auto-included, null configured = every feature. Drives which
    /// features' tools were registered into <see cref="McpHost"/> and which Core prompt segments
    /// <see cref="PromptBuilder"/> renders — the single gating decision, made once here.</summary>
    public IReadOnlyCollection<string> EnabledFeatureIds { get; }

    /// <summary>True when the given "core.*" feature id was enabled for this project.</summary>
    public bool HasFeature(string featureId) => EnabledFeatureIds.Contains(featureId);

    /// <summary>Project-lifetime token tally (workspace/.spla/token-usage.json).</summary>
    public SPLA.Domain.Interfaces.ITokenUsageStore TokenUsageProject { get; }

    /// <summary>Machine-global token tally (~/.spla/token-usage.json).</summary>
    public SPLA.Domain.Interfaces.ITokenUsageStore TokenUsageGlobal { get; }

    /// <summary>The system prompt for this workspace. Rebuilt on every read so live settings edits
    /// (plugin settings/prompts, mode) reach chats without a restart — ChatRuntime re-reads this at
    /// the start of each turn.</summary>
    public string SystemPrompt => PromptBuilder.Build(Settings, Settings.WorkspacePath);

    public AgentMode Mode => Settings.Mode;

    /// <summary>Cached reachability per connection id. null = not yet checked.</summary>
    public ConcurrentDictionary<string, (bool Ok, string? Error)> ConnectionHealth { get; } = new();

    /// <summary>Cached operative context window per endpoint+model, with a short TTL so a model
    /// reloaded with a different window is picked up. See <see cref="GetContextLengthAsync"/>.</summary>
    private readonly ConcurrentDictionary<string, (int? Length, DateTimeOffset At)> _contextLengthCache = new();
    private static readonly TimeSpan ContextLengthTtl = TimeSpan.FromSeconds(60);

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

        McpHost = new McpHost(
            new PermissionManager(settings: settings), PluginManager, loggerFactory.CreateLogger<McpHost>());

        // ── Fundamental agent working memory (project-scoped shared; session resolves via scope) ──
        // Built before the feature catalog below — several features' tools (memory, spawn) need it.
        ProjectKv = new ProjectKvStore(
            settings.Project.GetBucket(SPLA.Domain.Project.IProjectBackend.RootBucket));
        SpawnedRunner = new SpawnedAgentRunner(Llm, McpHost, SkillManager, PluginManager, settings);

        // ── Modular built-in capabilities: one IAgentFeature per "core.*" id, in
        // AgentFeatureCatalog.Order. Each feature carries its tools AND its prompt fragment
        // (CoreFeaturePrompts — the single id→fragment registry), so agent.capabilities
        // (settings.Capabilities) gates both together and they can never drift apart.
        // Plugin tools are unaffected: they are registered separately by McpHost's constructor
        // from PluginManager.GetDynamicTools().
        IAgentFeature Feature(string id, params SPLA.MCP.Core.Interfaces.IMcpTool[] tools)
            => new AgentFeature(id, tools, CoreFeaturePrompts.Load(id), AgentFeatureCatalog.RequiresOf(id));

        var featureCatalog = new List<IAgentFeature>
        {
            Feature("core.workspace",
                new GetContextTool(),
                new GetCurrentDateTimeTool()),
            Feature("core.tool-help",
                new SPLA.MCP.Core.Tools.AgentInfoTool(McpHost, SkillManager)),
            Feature("core.discipline"),
            Feature("core.files",
                new FsListTool(),
                new FsReadTool(),
                new FsSearchTextTool(),
                new FsFindFilesTool(),
                new FsCreateTool(),
                new FsPatchTool(),
                new FsWriteTool(),
                new FsDeleteTool(),
                new SPLA.MCP.Core.Tools.ImageViewTool()),
            Feature("core.shell",
                new RunCommandTool()),
            Feature("core.web",
                new SPLA.MCP.BasicTools.Network.WebFetchTool()),
            Feature("core.memory",
                new SPLA.MCP.Core.Tools.AgentMemorySetTool(ProjectKv.Store),
                new SPLA.MCP.Core.Tools.AgentMemoryGetTool(ProjectKv.Store),
                new SPLA.MCP.Core.Tools.AgentMemoryDeleteTool(ProjectKv.Store),
                new SPLA.MCP.Core.Tools.AgentMemoryListTool(ProjectKv.Store),
                new SPLA.MCP.Core.Tools.AgentMemoryClearTool(ProjectKv.Store)),
            Feature("core.checkpoints",
                new SPLA.MCP.Core.Tools.ContextCheckpointSetTool(),
                new SPLA.MCP.Core.Tools.ContextCheckpointRestoreTool(),
                new SPLA.MCP.Core.Tools.MarkSetTool(),
                new SPLA.MCP.Core.Tools.MarkRollbackTool()),
            Feature("core.skills",
                new SPLA.MCP.Core.Tools.SkillActivateTool(SkillManager),
                new SPLA.MCP.Core.Tools.SkillDeactivateTool()),
            Feature("core.spawn",
                new SPLA.MCP.Core.Tools.AgentSpawnTool(SpawnedRunner),
                new SPLA.MCP.Core.Tools.AgentSpawnBatchTool(SpawnedRunner)),
            Feature("core.clarify",
                new SPLA.MCP.Core.Tools.AgentClarifyTool()),
            Feature("core.blobs"),
        };

        var enabledIds = AgentFeatureCatalog.Resolve(settings.Capabilities, loggerFactory.CreateLogger("SPLA.Agent.Capabilities"));
        EnabledFeatureIds = new HashSet<string>(enabledIds, StringComparer.Ordinal);

        // Enabled features in catalog order: the SAME objects drive tool registration here and
        // Core prompt segments in the builder below — the one gating decision applied to both.
        var enabledFeatures = featureCatalog.Where(f => EnabledFeatureIds.Contains(f.Id)).ToList();
        foreach (var feature in enabledFeatures)
            foreach (var tool in feature.Tools)
                McpHost.RegisterTool(tool);

        ChatManager = new ChatManager(settings);

        PromptBuilder = new SystemPromptBuilder(SkillManager, PluginManager, null, enabledFeatures);

        // Project tally goes through the broker only when a real project is open; the historical
        // no-project path (cwd/.spla) is kept as-is so the tally never collides with the global file.
        TokenUsageProject = new FileTokenUsageStore(
            settings.ProjectFilePath != null
                ? Path.Combine(
                    settings.Project.GetBucket(SPLA.Domain.Project.IProjectBackend.RootBucket)
                        .MapToHostDirectory()!,
                    "token-usage.json")
                : Path.Combine(settings.WorkspacePath, ".spla", "token-usage.json"));
        TokenUsageGlobal = new FileTokenUsageStore(
            Path.Combine(SPLA.Domain.Settings.ConfigLoader.GetDefaultsDir(), "token-usage.json"));
    }

    /// <summary>
    /// Resolves the operative context window (tokens) for a connection's model, or null when unknown.
    /// Priority: the connection's manual <c>context_length</c> override → the provider-reported value
    /// (LM Studio native API: the LOADED instance's configured window, which is what requests actually
    /// fail against — not the model's max; vLLM: <c>max_model_len</c>) → null. Detection results are
    /// cached per endpoint+model for <see cref="ContextLengthTtl"/> so a reload with a different
    /// window is picked up without hammering the provider on every turn.
    /// </summary>
    public async Task<int?> GetContextLengthAsync(LLMSettings llm, CancellationToken ct = default)
    {
        if (llm.ContextLength is > 0) return llm.ContextLength;

        var key = $"{llm.BaseUrl}|{llm.ModelName}";
        if (_contextLengthCache.TryGetValue(key, out var hit) && DateTimeOffset.UtcNow - hit.At < ContextLengthTtl)
            return hit.Length;

        int? detected = null;
        try
        {
            var models = await ModelManagement.GetModelDetailsAsync(llm.BaseUrl, llm.ApiKey, ct);

            // A named model matches by id; "auto"/empty means "whatever is loaded" — take the loaded
            // LLM instance (LM Studio picks the same one for chat), else the single listed model.
            var model = !string.IsNullOrEmpty(llm.ModelName)
                ? models.FirstOrDefault(m => string.Equals(m.Id, llm.ModelName, StringComparison.OrdinalIgnoreCase))
                : models.FirstOrDefault(m => m.IsLoaded && !string.Equals(m.Type, "embedding", StringComparison.OrdinalIgnoreCase))
                  ?? (models.Count == 1 ? models[0] : null);

            var len = model?.EffectiveContextLength ?? 0;
            detected = len > 0 ? len : null;
        }
        catch
        {
            // Provider offline or no management surface — leave unknown; the reactive error path
            // still catches an actual overflow.
        }

        _contextLengthCache[key] = (detected, DateTimeOffset.UtcNow);
        return detected;
    }

    public void Dispose() => _httpClient.Dispose();
}
