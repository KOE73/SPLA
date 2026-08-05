using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SPLA.Agent;
using SPLA.Agent.Composition;
using SPLA.Domain.Agent;
using SPLA.MCP.Core.Composition;
using SPLA.Domain.Interfaces;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.LMStudio;
using SPLA.LLM.LocalAI;
using SPLA.LLM.OpenAiCompat;
using SPLA.LLM.OpenRouter;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.BasicTools.SystemTools;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Agent;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.Core.Plugins;
using SPLA.Library;
using SPLA.Library.Sources;
using SPLA.MCP.Core.ToolSets;

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
    /// <summary>
    /// The composed LLM pipeline — the only way anything in this runtime reaches a model. Provider
    /// clients are deliberately never exposed: if they were reachable, accounting, quotas and privacy
    /// would be optional in practice. See <see cref="SPLA.Domain.Llm.ILlmGateway"/>.
    /// </summary>
    public SPLA.Domain.Llm.ILlmGateway Llm { get; }

    /// <summary>The providers this runtime can dispatch to, keyed by a connection's <c>provider</c>
    /// field. Exposed for settings UIs that list what is available — never as a way to reach a
    /// client directly; turns go through <see cref="Llm"/>.</summary>
    public SPLA.Domain.Llm.LlmProviderRegistry Providers { get; }

    /// <summary>The OpenRouter provider, for the settings surfaces that need its catalog and account
    /// figures. Held concretely because those are provider-specific by nature — the generic path is
    /// the <see cref="SPLA.Domain.Llm.IProviderAccountInfo"/> type check, which is what callers that
    /// must stay provider-agnostic use instead.</summary>
    public OpenRouterProvider OpenRouter { get; private set; } = null!;

    /// <summary>Last-seen provider figures per connection (rate-limit budget, reset times). Distinct
    /// from the token ledger: this is current state, overwritten; the ledger is history, appended.</summary>
    public SPLA.Domain.Llm.ProviderStateStore ProviderState { get; } = new();
    public IModelManagementService ModelManagement { get; }
    public McpHost McpHost { get; }
    public SkillLibrary SkillLibrary { get; }

    /// <summary>The tag librarian over <see cref="SkillLibrary"/>. Reads holdings live, so it needs no
    /// rebuilding when a source changes.</summary>
    public SPLA.Library.Librarians.ITagLibrarian SkillLibrarian { get; }

    /// <summary>The model-backed librarian, consulted by <c>skill_find</c> only when the tag pass
    /// found nothing. Off unless <c>skills.librarian.enabled</c> says otherwise — it costs an LLM call
    /// before any work begins.</summary>
    public SPLA.Library.Librarians.IAgentLibrarian SkillAgentLibrarian { get; }
    public PluginManager PluginManager { get; }

    /// <summary>Kept because skill sources are rebuilt after startup now — see <see cref="RebuildSkillSources"/>.</summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>Tool sets and their levels — "allowed how far", never "raised right now".
    /// See <c>ToolSetRegistry</c> and PLAN_20260803_core.</summary>
    public ToolSetRegistry ToolSets { get; }
    public ProjectKvStore ProjectKv { get; }
    public ChatManager ChatManager { get; }

    /// <summary>
    /// The contributors that make up this agent's context surface, and the machinery that folds them
    /// into one. Everything that reaches the model as text — mode, capabilities, instruction files,
    /// skills, plugin prompts, working memory — comes through here, and the manifest it produces says
    /// which contributor is answerable for which piece.
    /// </summary>
    public AgentContextComposer ContextComposer { get; }

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

    /// <summary>Composes the agent's context surface: the system prompt, the per-turn items, and the
    /// manifest explaining both. Recomposed on every call so live settings edits (plugin
    /// settings/prompts, mode) reach chats without a restart, and so a skill activated mid-turn is
    /// reflected on the very next iteration.</summary>
    public ComposedContext ComposeContext() => ContextComposer.Compose(Settings, Settings.WorkspacePath);

    /// <summary>The system-prompt half of <see cref="ComposeContext"/>, for callers that only need
    /// the text (a freshly seeded chat, the debug view).</summary>
    public string SystemPrompt => ComposeContext().SystemPrompt;

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
        // ── The LLM pipeline, baked once ──────────────────────────────────────────────────────────
        // The blueprint is what a host varies (CLI, worker and server each declare their own set).
        // Nothing is assembled per turn — per-turn variation travels in LlmTurnContext instead.
        //
        // A provider is its own project, referenced directly. It is not a tool plugin and has nothing
        // to do with that host: it contributes data for the pipeline's terminal step and cannot
        // intercept a turn, which is what keeps accounting and credentials non-optional.
        Providers = BuildProviderRegistry(loggerFactory);
        Llm = new SPLA.Domain.Llm.LlmPipelineBlueprint()
            .Use(new SPLA.Domain.Llm.Middleware.TurnOutcomeMiddleware())
            // Records what the provider said about the key's standing, on both the success and the
            // failure path — a 429 is the response that reports the budget, and it throws.
            .Use(new SPLA.Domain.Llm.Middleware.ProviderStateMiddleware(ProviderState))
            // Credential materialization is the innermost layer: the key exists only for the provider
            // call itself, and never for accounting, which sits outside it.
            .Use(new SPLA.Domain.Llm.Middleware.CredentialsMiddleware(settings.SecretResolver))
            .Build(new SPLA.Domain.Llm.ProviderClientResolver(Providers));

        ModelManagement = new LMStudioManagementClient(_httpClient);

        var pluginsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

        // Plugins load first: their descriptors are what the plugin-backed skill sources are built
        // from, and a plugin's live enabled flag is what makes its skills appear or vanish.
        PluginManager = new PluginManager(settings, loggerFactory.CreateLogger<PluginManager>());
        PluginManager.LoadPlugins(pluginsDir);

        SchemaRegistry = new SPLA.Domain.Editor.SchemaRegistry();
        foreach (var p in PluginManager.GetSchemaProviders())
            SchemaRegistry.Register(p);

        // Skill providers: the built-in branches with the declared ones merged over them by name,
        // then one per plugin that ships skills. SetProbe below completes the wiring once the tool
        // host and feature set exist — requirement resolution needs both.
        _loggerFactory = loggerFactory;
        SkillLibrary = new SkillLibrary(BuildSkillSources(), loggerFactory.CreateLogger<SkillLibrary>());
        SkillLibrary.ApplySettings(settings.Skills);
        // Until now Reloaded fired into an empty room: a watcher would notice a new skill file and
        // no window would ever hear about it. One subscriber closes that, for every cause at once.
        SkillLibrary.Reloaded += (_, _) => Events.Publish(new SkillsChanged());
        SkillLibrarian = new SPLA.Library.Librarians.TagLibrarian(SkillLibrary);
        // Settings read through a lambda, not captured: a librarian switched on in a live settings
        // edit must take effect on the next call, the same way the mode already does.
        SkillAgentLibrarian = new SPLA.Library.Librarians.AgentLibrarian(
            SkillLibrary, Llm, () => Settings,
            loggerFactory.CreateLogger<SPLA.Library.Librarians.AgentLibrarian>());

        McpHost = new McpHost(
            new PermissionManager(settings: settings), PluginManager, loggerFactory.CreateLogger<McpHost>());

        // Tool sets: what exists and how far each may reach the model. Process-wide on purpose — a
        // level is the user's standing decision, while "raised right now" belongs to a chat and lives
        // in its AgentSession. Core features are not levelled yet: agent.capabilities already decides
        // whether they exist at all, so passing them here would give the same answer twice.
        ToolSets = new ToolSetRegistry(settings, PluginManager);
        McpHost.ToolSets = ToolSets;

        // ── Fundamental agent working memory (project-scoped shared; session resolves via scope) ──
        // Built before the feature catalog below — several features' tools (memory, spawn) need it.
        ProjectKv = new ProjectKvStore(
            settings.Project.GetBucket(SPLA.Domain.Project.IProjectBackend.RootBucket));
        SpawnedRunner = new SpawnedAgentRunner(Llm, McpHost, SkillLibrary, PluginManager, settings);

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
                new SPLA.MCP.Core.Tools.SkillActivateTool(SkillLibrary, ToolSets),
                new SPLA.MCP.Core.Tools.SkillDeactivateTool(),
                new SPLA.MCP.Core.Tools.SkillReadResourceTool(SkillLibrary),
                new SPLA.MCP.Core.Tools.SkillFindTool(SkillLibrarian, SkillAgentLibrarian)),
            Feature("core.toolsets",
                new SPLA.MCP.Core.Tools.ToolSetActivateTool(ToolSets),
                new SPLA.MCP.Core.Tools.ToolSetDeactivateTool()),
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

        // Now that every tool is registered and the feature set is known, skills can be told what
        // this agent can actually do. A skill declaring tools that are absent (its plugin is off)
        // resolves to MissingPrerequisites and stays out of the prompt instead of describing dead calls.
        RefreshSkillCapabilities();

        // The context surface: one contributor per source, gated by the same enabled-feature set that
        // decided which tools were registered above.
        var compositionLogger = loggerFactory.CreateLogger("SPLA.Agent.Composition");
        ContextComposer = new AgentContextComposer(
            AgentContributors.Default(SkillLibrary, PluginManager, null, enabledFeatures, ProjectKv.Store, ToolSets),
            compositionLogger);

        // What this agent was assembled from, once, at startup. Per-composition logging is Debug and
        // usually off; this line is what a service log has to carry, because "the model was told
        // something odd" is nearly always answered by which contributor put it there.
        compositionLogger.LogInformation("Agent surface at startup:\n{Manifest}",
            ComposeContext().Manifest.ToText());

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
    /// Re-reads what the agent can do and rebuilds the skill registry against it.
    ///
    /// <para>Call after anything that changes the live tool surface — notably enabling a plugin,
    /// which both adds tools and makes that plugin's own skills enumerable. Without it the skill list
    /// keeps the state it was resolved with at startup, so a freshly enabled plugin's skills stay
    /// invisible until restart while its tools are already live.</para>
    /// </summary>
    /// <summary>
    /// Resolves the configured branches into live sources. Kept as a method rather than inlined into
    /// the constructor because the source LIST is now editable at runtime: a person adding a folder
    /// in the settings panel has to get that folder without restarting the service, which is the
    /// difference between closing the original task and imitating it.
    /// </summary>
    private IReadOnlyList<ISkillSource> BuildSkillSources() =>
        SkillSourceRegistry.Build(
            Settings.EffectiveSkillSources(),
            new SkillSourceContext(
                Path.GetFullPath(Settings.WorkspacePath),
                ConfigLoader.GetDefaultsDir(),
                _loggerFactory,
                AppDomain.CurrentDomain.BaseDirectory),
            PluginManager.BuildSkillSources(_loggerFactory.CreateLogger<PluginSkillSource>()),
            _loggerFactory.CreateLogger<SkillLibrary>(),
            Settings.SkillsInheritDefaults,
            Settings.SkillsMaxTrust,
            Settings.SkillTrustStore);

    /// <summary>Rebuilds every skill source from the current settings and stores — call after the
    /// source list or a trust grant changed. Re-probes afterwards, because a new branch's skills have
    /// requirements that need resolving against the live tool surface.</summary>
    public void RebuildSkillSources()
    {
        SkillLibrary.SetSources(BuildSkillSources());
        RefreshSkillCapabilities();
    }

    public void RefreshSkillCapabilities() =>
        SkillLibrary.SetProbe(new SkillCapabilityProbe(
            // Permitted, not disclosed: a skill at the "on skill demand" level exists precisely to
            // raise the set it needs, so judging it against what is raised right now would mark
            // exactly those skills unavailable.
            McpHost.GetPermittedToolNames(),
            EnabledFeatureIds,
            PluginManager.GetToolOwners()));

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
            // Remote catalogs (OpenRouter: models are never "loaded") report the window per model id
            // directly; local runtimes (LM Studio) go through the load/unload management surface below.
            if (Providers.Resolve(llm.Provider) is SPLA.Domain.Llm.IModelCatalogInfo catalog)
            {
                detected = await catalog.GetContextLengthAsync(llm.BaseUrl, llm.ModelName, llm.ApiKey, ct);
            }
            else
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
        }
        catch
        {
            // Provider offline or no management/catalog surface — leave unknown; the reactive error
            // path still catches an actual overflow.
        }

        _contextLengthCache[key] = (detected, DateTimeOffset.UtcNow);
        return detected;
    }

    /// <summary>
    /// Composes the providers this runtime can reach. Each provider is its own project and owns its
    /// id, dialect and account telemetry; this method only says which ones are present.
    /// <para>
    /// Adding a provider is therefore: a project, and one line here. Nothing in the pipeline, the
    /// settings protocol or the UI has to know it exists.
    /// </para>
    /// <para>
    /// The plain family registers first, so <c>lmstudio</c> is the fallback for connections that name
    /// no provider — the least surprising default, since it needs no credential and cannot spend money.
    /// </para>
    /// </summary>
    private SPLA.Domain.Llm.LlmProviderRegistry BuildProviderRegistry(ILoggerFactory loggerFactory)
    {
        var registry = new SPLA.Domain.Llm.LlmProviderRegistry();

        foreach (var descriptor in LmStudioProvider.Create(_httpClient, loggerFactory))
            registry.Register(descriptor);

        registry.Register(new LocalAIProvider(_httpClient, loggerFactory));

        OpenRouter = new OpenRouterProvider(_httpClient, loggerFactory);
        registry.Register(OpenRouter);

        return registry;
    }

    public void Dispose() => _httpClient.Dispose();
}
