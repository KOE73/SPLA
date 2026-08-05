using SPLA.Domain.Models;
using SPLA.Domain.Secrets;

namespace SPLA.Domain.Settings;

/// <summary>
/// Resolved, ready-to-use settings after merging defaults + project.
/// </summary>
public class ResolvedSettings
{
    // LLM behaviour (endpoint/key/model live in Connections — not here)
    public double Temperature { get; set; } = 0.7;
    public string? ReasoningLevel { get; set; }
    public double PresencePenalty { get; set; } = 0.0;
    public double FrequencyPenalty { get; set; } = 0.0;
    public double RepeatPenalty { get; set; } = 1.0;

    /// <summary>Connections available to this project, each owning its models. Never empty after
    /// resolution — a default is synthesized from the <c>llm:</c> section when none are configured.
    /// This is the <i>tree</i>, for the settings UI; consumers that need to run a turn want
    /// <see cref="Models"/>.</summary>
    public List<SplaConnectionSection> Connections { get; set; } = new();

    /// <summary>
    /// Every model entry across every connection, flattened, each still knowing its owner. This is
    /// what a chat resolves against: the chat holds a model id, and running a turn needs the model's
    /// wire name together with its connection's endpoint and credential.
    /// <para>
    /// Ids are globally unique (enforced at resolution), so the flat lookup is unambiguous and a
    /// chat reference does not have to name the owning connection.
    /// </para>
    /// </summary>
    public List<ResolvedModelEntry> Models { get; set; } = new();

    // Agent
    public AgentMode Mode { get; set; } = AgentMode.Edit;
    public List<string> Instructions { get; set; } = new();
    public int CompactTailMessages { get; set; } = 2;
    public string? CustomPrompt { get; set; }
    /// <summary>Stop the turn when the model repeats the same tool call. Default OFF — see
    /// <see cref="SplaAgentSection.LoopGuard"/>.</summary>
    public bool LoopGuard { get; set; }
    public int LoopGuardRepeats { get; set; } = 3;

    /// <summary>Enabled built-in agent capabilities. Null = all enabled (backward compatible);
    /// see <see cref="SplaAgentSection.Capabilities"/> for full semantics.</summary>
    public List<string>? Capabilities { get; set; }

    // UI
    public string Theme { get; set; } = "Dark";
    public string Density { get; set; } = "norm";

    // Project
    public string? ProjectName { get; set; }
    public string WorkspacePath { get; set; } = ".";

    /// <summary>Absolute path to the .spla file that was loaded, or null when running without a project.
    /// Plugins that need to persist their own settings use this.</summary>
    public string? ProjectFilePath { get; set; }

    /// <summary>Global secrets store (user / project / shared scopes). Set during load. Never null
    /// after <see cref="ConfigLoader.LoadAndResolve"/>; plugins reach it via this property.</summary>
    public ISecretStore Secrets { get; set; } = null!;

    /// <summary>Resolves <c>secret:</c> / <c>env:</c> references in config values to plaintext.</summary>
    public ISecretResolver SecretResolver { get; set; } = null!;

    /// <summary>Who may use or manage which entry. Permissive locally (one person, nothing to
    /// arbitrate); a server replaces it with the ACL-backed policy before serving anyone. Assigning
    /// it also rebuilds <see cref="SecretResolver"/>, because the resolver is where the check
    /// actually bites — see <see cref="Secrets.SecretResolver"/>.</summary>
    public ISecretAccessPolicy SecretAccessPolicy
    {
        get => _secretAccessPolicy;
        set
        {
            _secretAccessPolicy = value;
            SecretResolver = new Secrets.SecretResolver(Secrets, value);
        }
    }

    private ISecretAccessPolicy _secretAccessPolicy = PermissiveSecretAccessPolicy.Instance;

    /// <summary>Cross-component shared services scoped to this project's runtime. Lets independently
    /// loaded parties (a plugin's tools, the service's protocol handlers) meet on one object without
    /// the domain knowing its type — e.g. the SSH session hub is created here by whoever touches it
    /// first (<c>GetOrAdd</c>) and every later consumer gets the same instance. Keys are owned by the
    /// registering component ("ssh.session-hub").</summary>
    public System.Collections.Concurrent.ConcurrentDictionary<string, object> SharedServices { get; } = new();

    private Project.IProject? _project;

    /// <summary>The project as a storage broker. Defaults to a local project over the same
    /// runtime layout the app always used (<c>.spla/</c> or <c>~/.spla</c>); a host can inject
    /// a different backend (server, memory) before anything touches it.</summary>
    public Project.IProject Project
    {
        get => _project ??= Domain.Project.LocalProject.For(this);
        set => _project = value;
    }

    public List<string> Docs { get; set; } = new();
    public List<string> Ignore { get; set; } = new();

    // Permission overrides (null = use mode defaults)
    public string? PermRead { get; set; }
    public string? PermWrite { get; set; }
    public string? PermShell { get; set; }
    public string? PermInternet { get; set; }
    public List<SplaToolPermissionRule> ToolPermissionRules { get; set; } = new();

    // Plugins
    public Dictionary<string, SplaPluginSection> Plugins { get; set; } = new();

    /// <summary>Tool set id → disclosure level, as written. Parsing and the fallback to the
    /// supplier's flag belong to <c>ToolSetRegistry</c>: settings stay a transport for what the file
    /// said, and an unknown level word must not silently become a different level here.</summary>
    public Dictionary<string, string> ToolSets { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Skills — per-skill overrides (skills.items), keyed by skill id.
    public Dictionary<string, SplaSkillSection> Skills { get; set; } = new();

    /// <summary>
    /// Declared skill providers (skills.sources), accumulated across layers in declaration order —
    /// the machine layer first, the project's after it.
    ///
    /// <para>This is what was WRITTEN, not the effective set: entries are merged by their declared
    /// id, and the built-in entries are prepended, by <c>SkillSourceRegistry.Build</c>. That work
    /// happens there rather than here because deriving the fallback id of an unnamed entry means
    /// resolving its path, and this resolver is a pure function with no idea where anything lives.</para>
    /// </summary>
    public List<SplaSkillSourceSection> SkillSources { get; set; } = new();

    /// <summary>Whether the built-in source entries are part of the fond. False is a deployment's
    /// white list — "only what I named". Last layer to mention it wins, like every other scalar.</summary>
    public bool SkillsInheritDefaults { get; set; } = true;

    // The model-backed librarian (skills.librarian). Null = off; skill_find stays deterministic.
    public SplaLibrarianSection? SkillLibrarian { get; set; }

    /// <summary>Looks up a model entry by its global id. Null id or unknown id = null.</summary>
    public ResolvedModelEntry? FindModel(string? modelId) =>
        string.IsNullOrWhiteSpace(modelId)
            ? null
            : Models.FirstOrDefault(m => string.Equals(m.Id, modelId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Builds LLMSettings from the first model entry + behaviour fields.
    /// Callers that need a specific entry should use <see cref="ToLLMSettings(ResolvedModelEntry?)"/>.</summary>
    public LLMSettings ToLLMSettings() => ToLLMSettings(Models.FirstOrDefault());

    public LLMSettings ToLLMSettings(ResolvedModelEntry? entry) => new()
    {
        Provider         = entry?.Provider,
        ConnectionId     = entry?.Connection.Id,
        BaseUrl          = entry?.Endpoint ?? "http://127.0.0.1:1234/v1/",
        ApiKey           = entry?.ApiKey   ?? "lm-studio",
        ModelName        = entry?.Model is { Length: > 0 } m && m != "auto" ? m : "",
        ContextLength    = entry?.ContextLength is > 0 ? entry.ContextLength : null,
        Temperature      = Temperature,
        Mode             = Mode,
        Theme            = Theme,
        ReasoningLevel   = ReasoningLevel,
        PresencePenalty  = PresencePenalty,
        FrequencyPenalty = FrequencyPenalty,
        RepeatPenalty    = RepeatPenalty
    };
}

/// <summary>
/// One model entry paired with the connection that owns it — everything a turn needs in one object,
/// so callers never have to walk back up the tree to find an endpoint or a key.
/// <para>
/// Both halves are the live config objects, not copies: editing a connection in the settings panel
/// is visible to chats immediately, which is the behaviour the flat list had and nothing should
/// have lost.
/// </para>
/// </summary>
public sealed class ResolvedModelEntry
{
    public required SplaConnectionSection Connection { get; init; }
    public required SplaModelSection Entry { get; init; }

    /// <summary>The model entry's globally unique id — what a chat stores.</summary>
    public string Id => Entry.Id;

    /// <summary>Label for pickers: the model's own name, qualified by its connection. Two entries for
    /// the same model under different keys are told apart by the connection half, so it is never
    /// dropped.</summary>
    public string DisplayName => $"{Connection.DisplayName} · {Entry.DisplayName}";

    public string? Provider => Connection.Provider;
    public string? Endpoint => Connection.Endpoint;
    public string? ApiKey => Connection.ApiKey;
    public string? Model => Entry.Model;
    public int? ContextLength => Entry.ContextLength;
}

/// <summary>
/// Merges defaults + project into a single ResolvedSettings.
/// Pure function, no I/O.
/// </summary>
public static class SettingsResolver
{
    public static ResolvedSettings Resolve(SplaDefaults? defaults, SplaProject? project)
    {
        var r = new ResolvedSettings();

        // Connections merge across layers by id (project overrides/extends defaults).
        var connections = new Dictionary<string, SplaConnectionSection>(StringComparer.OrdinalIgnoreCase);

        // llm: section fields — tracked locally and used only to synthesize the fallback default
        // connection when no connections are declared. Endpoint/ApiKey/Model no longer live on
        // ResolvedSettings; Connections is the single source of truth for those.
        string llmEndpoint = "http://127.0.0.1:1234/v1/";
        string llmApiKey   = "lm-studio";
        string llmModel    = "auto";

        // Layer 1: defaults
        if (defaults != null)
        {
            MergeConnections(connections, defaults.Connections);
            if (defaults.Llm != null)
            {
                llmEndpoint          = defaults.Llm.Endpoint    ?? llmEndpoint;
                llmApiKey            = defaults.Llm.ApiKey      ?? llmApiKey;
                llmModel             = defaults.Llm.Model       ?? llmModel;
                r.Temperature        = defaults.Llm.Temperature ?? r.Temperature;
                r.ReasoningLevel     = defaults.Llm.ReasoningLevel  ?? r.ReasoningLevel;
                r.PresencePenalty    = defaults.Llm.PresencePenalty  ?? r.PresencePenalty;
                r.FrequencyPenalty   = defaults.Llm.FrequencyPenalty ?? r.FrequencyPenalty;
                r.RepeatPenalty      = defaults.Llm.RepeatPenalty    ?? r.RepeatPenalty;
            }
            if (defaults.Agent != null)
            {
                if (defaults.Agent.Mode != null && Enum.TryParse<AgentMode>(defaults.Agent.Mode, true, out var m))
                    r.Mode = m;
                if (defaults.Agent.CompactTailMessages.HasValue)
                    r.CompactTailMessages = defaults.Agent.CompactTailMessages.Value;
                if (!string.IsNullOrEmpty(defaults.Agent.CustomPrompt))
                    r.CustomPrompt = defaults.Agent.CustomPrompt;
                r.LoopGuard = defaults.Agent.LoopGuard ?? r.LoopGuard;
                r.LoopGuardRepeats = defaults.Agent.LoopGuardRepeats ?? r.LoopGuardRepeats;
                r.Capabilities = defaults.Agent.Capabilities ?? r.Capabilities;
            }
            if (defaults.Ui != null)
            {
                r.Theme = defaults.Ui.Theme ?? r.Theme;
                r.Density = defaults.Ui.Density ?? r.Density;
            }
            ApplySkills(r, defaults.Skills);
            ApplyToolSets(r, defaults.ToolSets);
        }

        // Layer 2: project overrides
        if (project != null)
        {
            r.ProjectName = project.Name;
            r.WorkspacePath = project.Workspace ?? ".";
            r.Docs = project.Docs ?? new();
            r.Ignore = project.Ignore ?? new();

            MergeConnections(connections, project.Connections);
            if (project.Llm != null)
            {
                llmEndpoint          = project.Llm.Endpoint    ?? llmEndpoint;
                llmApiKey            = project.Llm.ApiKey      ?? llmApiKey;
                llmModel             = project.Llm.Model       ?? llmModel;
                r.Temperature        = project.Llm.Temperature ?? r.Temperature;
                r.ReasoningLevel     = project.Llm.ReasoningLevel  ?? r.ReasoningLevel;
                r.PresencePenalty    = project.Llm.PresencePenalty  ?? r.PresencePenalty;
                r.FrequencyPenalty   = project.Llm.FrequencyPenalty ?? r.FrequencyPenalty;
                r.RepeatPenalty      = project.Llm.RepeatPenalty    ?? r.RepeatPenalty;
            }
            if (project.Agent != null)
            {
                if (project.Agent.Mode != null && Enum.TryParse<AgentMode>(project.Agent.Mode, true, out var m))
                    r.Mode = m;
                r.Instructions = project.Agent.Instructions ?? r.Instructions;
                if (project.Agent.CompactTailMessages.HasValue)
                    r.CompactTailMessages = project.Agent.CompactTailMessages.Value;
                if (!string.IsNullOrEmpty(project.Agent.CustomPrompt))
                    r.CustomPrompt = project.Agent.CustomPrompt;
                r.LoopGuard = project.Agent.LoopGuard ?? r.LoopGuard;
                r.LoopGuardRepeats = project.Agent.LoopGuardRepeats ?? r.LoopGuardRepeats;
                r.Capabilities = project.Agent.Capabilities ?? r.Capabilities;
            }
            if (project.Ui != null)
            {
                r.Theme = project.Ui.Theme ?? r.Theme;
                r.Density = project.Ui.Density ?? r.Density;
            }
            if (project.Permissions != null)
            {
                r.PermRead = project.Permissions.Read;
                r.PermWrite = project.Permissions.Write;
                r.PermShell = project.Permissions.Shell;
                r.PermInternet = project.Permissions.Internet;
                r.ToolPermissionRules = project.Permissions.Tools ?? new();
            }
            if (project.Plugins != null)
            {
                foreach (var kvp in project.Plugins)
                    r.Plugins[kvp.Key] = kvp.Value;
            }
            ApplyToolSets(r, project.ToolSets);
            ApplySkills(r, project.Skills);
        }

        // Finalize: keep configured connections; synthesize a default from the llm: section when none
        // are declared, so chats always have at least one model entry to resolve against.
        r.Connections = connections.Values.ToList();
        if (r.Connections.Count == 0)
            r.Connections.Add(new SplaConnectionSection
            {
                Id = "default", Name = "Default", Provider = "lmstudio",
                Endpoint = llmEndpoint, ApiKey = llmApiKey,
                Models = { new SplaModelSection { Id = "default", Name = "Default", Model = llmModel } }
            });

        r.Models = FlattenModels(r.Connections);
        return r;
    }

    /// <summary>
    /// Projects the connection tree onto the flat, globally-keyed model list chats resolve against.
    /// <para>
    /// Duplicate ids throw rather than silently winning: a chat reference is a bare id, so two entries
    /// sharing one would make which model a chat runs on depend on list order — a bug that surfaces as
    /// "it answered from the wrong key" long after the config was edited. A connection with no models
    /// is legal (half-configured, being set up) and simply contributes nothing.
    /// </para>
    /// </summary>
    private static List<ResolvedModelEntry> FlattenModels(List<SplaConnectionSection> connections)
    {
        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var models = new List<ResolvedModelEntry>();

        foreach (var conn in connections)
            foreach (var entry in conn.Models)
            {
                if (string.IsNullOrWhiteSpace(entry.Id)) continue;
                if (seen.TryGetValue(entry.Id, out var owner))
                    throw new InvalidOperationException(
                        $"Duplicate model id '{entry.Id}': declared under both connection '{owner}' and " +
                        $"'{conn.Id}'. Model ids are referenced by chats without naming a connection, so " +
                        $"they must be unique across the whole project.");
                seen[entry.Id] = conn.Id;
                models.Add(new ResolvedModelEntry { Connection = conn, Entry = entry });
            }

        return models;
    }

    /// <summary>Merges one layer's <c>toolsets:</c> entries key by key — the more specific layer
    /// overrides a set it mentions and leaves the rest of the inherited levels alone.</summary>
    private static void ApplyToolSets(ResolvedSettings r, Dictionary<string, string>? toolSets)
    {
        if (toolSets == null) return;

        foreach (var kvp in toolSets)
            r.ToolSets[kvp.Key] = kvp.Value;
    }

    /// <summary>
    /// Layers one <c>skills:</c> block over the result. Both halves now merge, and by the same
    /// principle: per-skill items by skill id, sources by their declared source id. A layer that
    /// omits either half changes neither.
    ///
    /// <para>Sources are only ACCUMULATED here, in layer order — the merge itself happens in
    /// <c>SkillSourceRegistry.Build</c>, which is the first place an unnamed entry's fallback id can
    /// be worked out. Appending rather than replacing is the whole point of the change: adding a
    /// folder is one entry in any layer, and dropping an inherited one is <c>enabled: false</c>.</para>
    /// </summary>
    private static void ApplySkills(ResolvedSettings r, SplaSkillsSection? skills)
    {
        if (skills == null) return;

        if (skills.InheritDefaults.HasValue)
            r.SkillsInheritDefaults = skills.InheritDefaults.Value;

        if (skills.Sources != null)
            r.SkillSources.AddRange(skills.Sources);

        if (skills.Items != null)
            foreach (var kvp in skills.Items)
                r.Skills[kvp.Key] = kvp.Value;

        // Replaced wholesale, like sources: a two-field block is not worth a merge, and a project
        // that names a librarian means that one.
        if (skills.Librarian != null)
            r.SkillLibrarian = skills.Librarian;
    }

    /// <summary>Adds/overrides connections by id, skipping entries without an id.</summary>
    private static void MergeConnections(
        Dictionary<string, SplaConnectionSection> into, List<SplaConnectionSection>? from)
    {
        if (from == null) return;
        foreach (var c in from)
            if (!string.IsNullOrWhiteSpace(c.Id))
                into[c.Id] = c;
    }
}
