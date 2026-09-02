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

    /// <summary>Null = not sent, server default applies. See <see cref="LLMSettings.MaxTokens"/>.</summary>
    public int? MaxTokens { get; set; }
    /// <summary>Null = not sent, server default applies. See <see cref="LLMSettings.TopP"/>.</summary>
    public double? TopP { get; set; }
    /// <summary>Null = not sent, server default applies. See <see cref="LLMSettings.MinP"/>.</summary>
    public double? MinP { get; set; }

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
    /// <summary>
    /// Challenge, then stop, a turn that keeps making the same tool call. **On** — a chat without it
    /// was the odd one out, since a spawned run has had it unconditionally for as long as spawning has
    /// existed, and the chat is the one with a person paying for the tokens.
    /// <para>Off by default originally, on the reasonable worry that a guard fires on work that is
    /// merely repetitive. It cannot: <c>ToolRepeatTracker</c> wants the same tool, the same arguments,
    /// the same result, no accompanying text, and a round faster than ten seconds — all of them, in a
    /// row. Deliberate polling changes at least one, and the first trip only asks the model whether it
    /// is stuck. Turn it off per project with <see cref="SplaAgentSection.LoopGuard"/> if some workload
    /// proves otherwise.</para>
    /// </summary>
    public bool LoopGuard { get; set; } = true;
    public int LoopGuardRepeats { get; set; } = 3;

    /// <summary>Master switch for the resource-address abstraction (<c>file://</c>, <c>sftp://</c>,
    /// …). <b>Default false</b>, and that default is load-bearing: the foundation is meant to ship
    /// inert so the model can be measured with and without it, and a switch that defaults on erases
    /// the "without" arm of that comparison. See <see cref="SplaAgentSection.UnifiedResources"/>.</summary>
    public bool UnifiedResources { get; set; }

    /// <summary>Minutes a permission/clarify question waits for a person before it is denied; 0 = no
    /// limit. The wait is deliberately long: the question outlives the window that triggered it, so
    /// the bound exists only to stop an unattended instance blocking forever.</summary>
    public int AskTimeoutMinutes { get; set; } = 60;

    /// <summary>Seconds a <c>system_run_shell</c> command may sit silent before the tool returns
    /// control instead of continuing to wait; 0 = never (wait for exit or a prompt, however long that
    /// takes). See <see cref="SplaAgentSection.ShellTimeoutSeconds"/>.</summary>
    public int ShellTimeoutSeconds { get; set; } = 120;

    /// <summary>Persist the full tool-call/tool-result trace with the chat history. Default OFF —
    /// see <see cref="SplaAgentSection.SaveToolCalls"/>.</summary>
    public bool SaveToolCalls { get; set; }

    /// <summary>Persist abandoned-generation records with the chat history. Default OFF — see
    /// <see cref="SplaAgentSection.SaveAttempts"/>.</summary>
    public bool SaveAttempts { get; set; }

    /// <summary>Enabled built-in agent capabilities. Null = all enabled (backward compatible);
    /// see <see cref="SplaAgentSection.Capabilities"/> for full semantics.</summary>
    public List<string>? Capabilities { get; set; }

    /// <summary>Whether <c>spla serve</c> maps <c>POST /mcp</c> at all. Off by default — the strict
    /// case, no second head over HTTP. See <see cref="SplaMcpSection.Enabled"/>.</summary>
    public bool McpEnabled { get; set; } = false;

    /// <summary>Fixed port for <c>spla serve</c> to bind, or null for the usual ephemeral one. See
    /// <see cref="SplaMcpSection.Port"/>.</summary>
    public int? McpPort { get; set; }

    /// <summary>Foreign MCP servers this project consumes, merged across layers by
    /// <see cref="SplaMcpServerSection.Id"/> — a project entry replaces a machine entry of the same id
    /// wholesale, the same rule <see cref="Connections"/> uses. Nothing here connects to anything;
    /// this is only the declaration. See <see cref="SplaMcpSection.Servers"/>.</summary>
    public List<SplaMcpServerSection> McpServers { get; set; } = new();

    // UI
    public string Theme { get; set; } = "Dark";
    public string Density { get; set; } = "norm";

    // Project
    public string? ProjectName { get; set; }

    /// <summary>The project root: the directory holding the loaded manifest, always absolute.
    /// Falls back to the current directory when running without a project — and that case has no
    /// root at all, so nothing may be bounded by it.
    /// <para>Not configurable. A manifest cannot point the root elsewhere: one definition, or every
    /// boundary drawn on it is negotiable.</para></summary>
    public string WorkspacePath { get; set; } = ".";

    /// <summary>Whether a manifest was actually found. <c>false</c> ⇒ there is no project and
    /// <see cref="WorkspacePath"/> is merely where the process started, which is not a boundary.</summary>
    public bool HasProject => ProjectFilePath is not null;

    /// <summary>Folders outside the root this project declared, resolved and validated at load —
    /// see <c>MountResolver</c>. Empty when there is no project: without a manifest there is nothing
    /// to declare them in.</summary>
    public IReadOnlyList<Host.ProjectMount> Mounts { get; set; } = [];

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

    /// <summary>Domains the operator vouches for, accumulated across layers. Content from these is
    /// named content and does not raise a chat's doubt flag.</summary>
    public List<string> TrustedDomains { get; set; } = new();

    /// <summary>Whether <paramref name="host"/> is one the operator vouched for. Subdomains are
    /// included — vouching for <c>corp.local</c> vouches for its wiki — because the unit a person
    /// thinks in is the organisation, not each machine in it.</summary>
    public bool IsTrustedDomain(string? host)
    {
        if (string.IsNullOrWhiteSpace(host)) return false;
        var h = host.Trim().TrimEnd('.');

        foreach (var entry in TrustedDomains)
        {
            var d = entry.Trim().TrimEnd('.');
            if (d.Length == 0) continue;
            if (h.Equals(d, StringComparison.OrdinalIgnoreCase)) return true;
            if (h.EndsWith("." + d, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    public List<string> Docs { get; set; } = new();
    public List<string> Ignore { get; set; } = new();

    // Permission overrides (null = use mode defaults)
    public string? PermRead { get; set; }
    public string? PermWrite { get; set; }
    public string? PermShell { get; set; }
    public string? PermInternet { get; set; }
    /// <summary>Override for foreign MCP-server tools (ToolScope.Foreign) — see
    /// <see cref="SplaPermissionsSection.Foreign"/>.</summary>
    public string? PermForeign { get; set; }
    public List<SplaToolPermissionRule> ToolPermissionRules { get; set; } = new();

    // Plugins
    public Dictionary<string, SplaPluginSection> Plugins { get; set; } = new();

    /// <summary>Tool set id → disclosure level, as written. Parsing and the fallback to the
    /// supplier's flag belong to <c>ToolSetRegistry</c>: settings stay a transport for what the file
    /// said, and an unknown level word must not silently become a different level here.</summary>
    public Dictionary<string, string> ToolSets { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Per-scheme resource switches, as written. A scheme with no entry is enabled — see
    /// <c>ResourceRegistry.IsEnabled</c>, which applies the same absent-means-on rule at the point
    /// where it actually matters. This dictionary is only the transport for what the file said.</summary>
    public Dictionary<string, bool> ResourceSchemes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

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

    /// <summary>The administrator's ceiling on source trust (<c>skills.policy.max_trust</c>), read
    /// only from the machine layer. Null = no ceiling, i.e. a granted source may be trusted.</summary>
    public string? SkillsMaxTrust { get; set; }

    /// <summary>As declared by <c>skills.policy.user_may_vouch</c>. Null = nobody said, so the
    /// deployment's own default applies — see <see cref="SkillsUserMayVouchEffective"/>.</summary>
    public bool? SkillsUserMayVouch { get; set; }

    /// <summary>True when this install serves more than one person, i.e. personal directories are
    /// resolved per user rather than everyone sharing the machine home.</summary>
    public bool IsMultiUserDeployment { get; set; }

    /// <summary>
    /// Whether the person may vouch for a folder themselves.
    ///
    /// <para>True locally: they are their own administrator, and the confirmation dialog is the whole
    /// ceremony. False on a server unless an administrator says otherwise — a user writing in their
    /// own area is not a risk, but the trust level that entry claims is, and the trust level is the
    /// axis worth cutting on rather than the right to write.</para>
    /// </summary>
    public bool SkillsUserMayVouchEffective => SkillsUserMayVouch ?? !IsMultiUserDeployment;

    /// <summary>
    /// Where this person's own state lives — their skill branches, their trust grants, their
    /// machine-wide skills folder. <c>~/.spla</c> locally; on a server, their own area under
    /// <c>{root}/users/{userKey}</c>, so one server's users do not share a fond or each other's
    /// approvals.
    /// </summary>
    public string PersonalDir { get; set; } = string.Empty;

    /// <summary>The branches this person added themselves — the half of the fond they own and the UI
    /// may write. Null only in tests and embedded hosts that never granted anything.</summary>
    public ISkillSourceStore? SkillSourceStore { get; set; }

    /// <summary>Which locations this person has vouched for. Separate from the source list on
    /// purpose: a grant is a decision about safety, and it must not be editable as a field of the
    /// thing it is about.</summary>
    public ISkillTrustStore? SkillTrustStore { get; set; }

    /// <summary>
    /// The declared entries with the granted ones appended, which is the order the fold expects:
    /// granted last means most specific, so a personal entry overrides a prescribed one of the same
    /// name instead of being shadowed by it.
    ///
    /// <para>That ordering is what makes "switch off an inherited branch from the panel" possible
    /// without the UI writing into a committed project file — it records an override under the same
    /// id, in the person's own store.</para>
    /// </summary>
    public List<SplaSkillSourceSection> EffectiveSkillSources() =>
        SkillSourceStore is null
            ? SkillSources
            : [.. SkillSources, .. SkillSourceStore.Load()];

    // The model-backed librarian (skills.librarian). Null = off; skill_find stays deterministic.
    public SplaLibrarianSection? SkillLibrarian { get; set; }

    /// <summary>The role these settings were resolved for, or null for the project's own settings
    /// (what <c>ADR_20260827-2_core_roles</c> calls "role zero" — <c>agent:</c> itself). Set only by
    /// <see cref="SettingsResolver.ResolveForRole"/>.</summary>
    public string? RoleName { get; set; }

    /// <summary>The model id this role selected (<see cref="SplaRoleSection.Model"/>), or null to keep
    /// whatever the chat would otherwise resolve against. Only ever set alongside
    /// <see cref="RoleName"/>.</summary>
    public string? RoleModelId { get; set; }

    /// <summary>Islands (see <see cref="Security.IslandIdentity"/>) this role selected, by island key,
    /// as written in its file. A narrowing of what the prompt/tool surface mentions, never a grant —
    /// <c>ICapabilityGate</c> is still the only place a reach is actually decided. Empty = every island
    /// the project declares is in scope, same as a project with no role at all.</summary>
    public List<string> RoleIslands { get; set; } = new();

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
        RepeatPenalty    = RepeatPenalty,
        MaxTokens        = MaxTokens,
        TopP             = TopP,
        MinP             = MinP
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

    /// <summary>The reasoning options declared for this entry in config, or
    /// <see cref="Models.ReasoningCapability.Unknown"/> when none were. A declaration wins over
    /// whatever the provider says — same rule as <see cref="ContextLength"/>.</summary>
    public Models.ReasoningCapability DeclaredReasoning =>
        Entry.ReasoningOptions is { Count: > 0 }
            ? Models.ReasoningCapability.FromOptions(Entry.ReasoningOptions, Entry.ReasoningDefault)
            : Models.ReasoningCapability.Unknown;
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

        // mcp.servers merges across layers by id, same rule as connections above.
        var mcpServers = new Dictionary<string, SplaMcpServerSection>(StringComparer.OrdinalIgnoreCase);

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
                r.MaxTokens          = defaults.Llm.MaxTokens        ?? r.MaxTokens;
                r.TopP               = defaults.Llm.TopP             ?? r.TopP;
                r.MinP               = defaults.Llm.MinP             ?? r.MinP;
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
                r.AskTimeoutMinutes = defaults.Agent.AskTimeoutMinutes ?? r.AskTimeoutMinutes;
                r.ShellTimeoutSeconds = defaults.Agent.ShellTimeoutSeconds ?? r.ShellTimeoutSeconds;
                r.SaveToolCalls = defaults.Agent.SaveToolCalls ?? r.SaveToolCalls;
                r.SaveAttempts = defaults.Agent.SaveAttempts ?? r.SaveAttempts;
                r.Capabilities = defaults.Agent.Capabilities ?? r.Capabilities;
                r.UnifiedResources = defaults.Agent.UnifiedResources ?? r.UnifiedResources;
                AddTrustedDomains(r, defaults.Agent.TrustedDomains);
            }
            if (defaults.Mcp != null)
            {
                r.McpEnabled = defaults.Mcp.Enabled ?? r.McpEnabled;
                r.McpPort = defaults.Mcp.Port ?? r.McpPort;
                MergeMcpServers(mcpServers, defaults.Mcp.Servers);
            }
            if (defaults.Ui != null)
            {
                r.Theme = defaults.Ui.Theme ?? r.Theme;
                r.Density = defaults.Ui.Density ?? r.Density;
            }
            ApplySkills(r, defaults.Skills, SourceOrigin.Machine);
            ApplyToolSets(r, defaults.ToolSets);
            ApplyResourceSchemes(r, defaults.Resources);
        }

        // Layer 2: project overrides
        if (project != null)
        {
            r.ProjectName = project.Name;
            // WorkspacePath is NOT resolved here: it derives from where the manifest was found, and
            // only the loader knows that. See ConfigLoader.LoadAndResolve.
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
                r.MaxTokens          = project.Llm.MaxTokens        ?? r.MaxTokens;
                r.TopP               = project.Llm.TopP             ?? r.TopP;
                r.MinP               = project.Llm.MinP             ?? r.MinP;
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
                r.AskTimeoutMinutes = project.Agent.AskTimeoutMinutes ?? r.AskTimeoutMinutes;
                r.ShellTimeoutSeconds = project.Agent.ShellTimeoutSeconds ?? r.ShellTimeoutSeconds;
                r.SaveToolCalls = project.Agent.SaveToolCalls ?? r.SaveToolCalls;
                r.SaveAttempts = project.Agent.SaveAttempts ?? r.SaveAttempts;
                r.Capabilities = project.Agent.Capabilities ?? r.Capabilities;
                r.UnifiedResources = project.Agent.UnifiedResources ?? r.UnifiedResources;
                AddTrustedDomains(r, project.Agent.TrustedDomains);
            }
            if (project.Mcp != null)
            {
                r.McpEnabled = project.Mcp.Enabled ?? r.McpEnabled;
                r.McpPort = project.Mcp.Port ?? r.McpPort;
                MergeMcpServers(mcpServers, project.Mcp.Servers);
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
                r.PermForeign = project.Permissions.Foreign;
                r.ToolPermissionRules = project.Permissions.Tools ?? new();
            }
            if (project.Plugins != null)
            {
                foreach (var kvp in project.Plugins)
                    r.Plugins[kvp.Key] = kvp.Value;
            }
            ApplyToolSets(r, project.ToolSets);
            ApplyResourceSchemes(r, project.Resources);
            ApplySkills(r, project.Skills, SourceOrigin.Project);
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
        r.McpServers = mcpServers.Values.ToList();
        return r;
    }

    /// <summary>
    /// Layers one role's body over an already-resolved project baseline —
    /// <c>ADR_20260827-2_core_roles</c> §2.1. This is the only path that turns a role name into
    /// effective settings; nothing else may.
    ///
    /// <para><b>The one real invariant: a role is not self-assigned.</b> <paramref name="roleName"/>
    /// must appear, case-insensitively, in <paramref name="project"/>'s own <c>roles:</c> list, or this
    /// throws — regardless of whether <paramref name="roleSection"/> is a perfectly good, non-null
    /// role body. Nothing an agent can compute or fabricate at runtime substitutes for the owner
    /// having written the name into the manifest: possessing a <see cref="SplaRoleSection"/> instance
    /// is not possessing a grant. A file sitting in <c>roles/</c> that the manifest never named is,
    /// by design, exactly as inert as a role nobody wrote at all.</para>
    ///
    /// <para><b>Not a subset of the project's own capabilities.</b> The role's
    /// <see cref="SplaRoleSection.Capabilities"/>, when set, replaces <paramref name="baseline"/>'s
    /// list wholesale rather than being intersected with it — a role may declare a capability
    /// <c>agent:</c> never mentioned and gets it. The ceiling on what actually runs is the directory
    /// root and the owner's grants (<c>ICapabilityGate</c>, the zone model), never the union of what
    /// <c>agent:</c> happened to declare — requiring that union would make the project itself the
    /// maximally privileged entity, the exact failure <c>ADR_20260819</c> describes for project-less
    /// mode.</para>
    ///
    /// <para><b>Not a second axis of permissions.</b> A role has no <c>permissions:</c> block of its
    /// own (<see cref="SplaRoleSection"/> carries none): it picks a <see cref="SplaRoleSection.Mode"/>
    /// the same way <c>agent:</c> does, and narrows what runs inside that mode via capabilities and
    /// tool-set levels. <see cref="ResolvedSettings.PermRead"/> and friends are untouched here.</para>
    ///
    /// <para>Structurally unable to raise the ceiling: <see cref="SplaRoleSection"/> has no field that
    /// could move <see cref="ResolvedSettings.WorkspacePath"/>, <see cref="ResolvedSettings.Mounts"/>
    /// or the root — a role file simply has nowhere to write such a thing, so this method never touches
    /// them.</para>
    /// </summary>
    /// <param name="baseline">The project's own resolved settings — the result of <see cref="Resolve"/>,
    /// i.e. role zero. Not mutated; a new instance is returned.</param>
    /// <param name="project">The manifest, consulted only for its <see cref="SplaProject.Roles"/> list.</param>
    /// <param name="roleName">The role being resolved for. Never null/empty — callers wanting role zero
    /// simply use <paramref name="baseline"/> directly and do not call this method at all.</param>
    /// <param name="roleSection">The role's body, already loaded (see <see cref="ConfigLoader.LoadRole"/>).
    /// Loading is a separate, file-system concern; this method is the pure layering step.</param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="roleName"/> is not listed under the manifest's <c>roles:</c>, or
    /// <paramref name="roleSection"/> is null (the name was declared but its file could not be loaded).
    /// </exception>
    public static ResolvedSettings ResolveForRole(
        ResolvedSettings baseline, SplaProject project, string roleName, SplaRoleSection? roleSection)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(project);
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name must not be empty.", nameof(roleName));

        var declared = project.Roles ?? [];
        if (!declared.Any(name => string.Equals(name, roleName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                $"Role '{roleName}' is not declared under this project's 'roles:' list " +
                $"({(declared.Count == 0 ? "none declared" : string.Join(", ", declared))}). " +
                "A role file nobody named does not act.");

        if (roleSection is null)
            throw new InvalidOperationException(
                $"Role '{roleName}' is declared but its body (roles/{roleName}.yaml) could not be loaded.");

        var r = CloneForRole(baseline, roleName);

        if (roleSection.Mode != null && Enum.TryParse<AgentMode>(roleSection.Mode, true, out var mode))
            r.Mode = mode;
        r.Instructions = roleSection.Instructions ?? r.Instructions;
        if (roleSection.CompactTailMessages.HasValue)
            r.CompactTailMessages = roleSection.CompactTailMessages.Value;
        if (!string.IsNullOrEmpty(roleSection.CustomPrompt))
            r.CustomPrompt = roleSection.CustomPrompt;
        r.LoopGuard = roleSection.LoopGuard ?? r.LoopGuard;
        r.LoopGuardRepeats = roleSection.LoopGuardRepeats ?? r.LoopGuardRepeats;
        r.AskTimeoutMinutes = roleSection.AskTimeoutMinutes ?? r.AskTimeoutMinutes;
        r.ShellTimeoutSeconds = roleSection.ShellTimeoutSeconds ?? r.ShellTimeoutSeconds;
        r.SaveToolCalls = roleSection.SaveToolCalls ?? r.SaveToolCalls;
        r.SaveAttempts = roleSection.SaveAttempts ?? r.SaveAttempts;
        AddTrustedDomains(r, roleSection.TrustedDomains);

        // Wholesale replacement, deliberately not an intersection — see the type doc on
        // SplaRoleSection.Capabilities and the method doc above.
        if (roleSection.Capabilities != null)
            r.Capabilities = roleSection.Capabilities;

        ApplyToolSets(r, roleSection.ToolSets);

        r.RoleModelId = roleSection.Model;
        r.RoleIslands = roleSection.Islands ?? [];

        return r;
    }

    /// <summary>Shallow-copies a resolved baseline before layering a role over it, so the project's own
    /// <see cref="ResolvedSettings"/> is never mutated by resolving one of its roles. Collections that a
    /// role may narrow are copied into fresh lists/dictionaries; everything else (connections, secrets,
    /// the project root, mounts) is intentionally shared by reference — a role has no way to change any
    /// of it, so there is nothing to protect against.</summary>
    private static ResolvedSettings CloneForRole(ResolvedSettings baseline, string roleName) => new()
    {
        Temperature = baseline.Temperature,
        ReasoningLevel = baseline.ReasoningLevel,
        PresencePenalty = baseline.PresencePenalty,
        FrequencyPenalty = baseline.FrequencyPenalty,
        RepeatPenalty = baseline.RepeatPenalty,
        MaxTokens = baseline.MaxTokens,
        TopP = baseline.TopP,
        MinP = baseline.MinP,
        Connections = baseline.Connections,
        Models = baseline.Models,
        Mode = baseline.Mode,
        Instructions = [.. baseline.Instructions],
        CompactTailMessages = baseline.CompactTailMessages,
        CustomPrompt = baseline.CustomPrompt,
        LoopGuard = baseline.LoopGuard,
        LoopGuardRepeats = baseline.LoopGuardRepeats,
        UnifiedResources = baseline.UnifiedResources,
        AskTimeoutMinutes = baseline.AskTimeoutMinutes,
        ShellTimeoutSeconds = baseline.ShellTimeoutSeconds,
        SaveToolCalls = baseline.SaveToolCalls,
        SaveAttempts = baseline.SaveAttempts,
        Capabilities = baseline.Capabilities is null ? null : [.. baseline.Capabilities],
        McpEnabled = baseline.McpEnabled,
        McpPort = baseline.McpPort,
        McpServers = baseline.McpServers,
        Theme = baseline.Theme,
        Density = baseline.Density,
        ProjectName = baseline.ProjectName,
        WorkspacePath = baseline.WorkspacePath,
        Mounts = baseline.Mounts,
        ProjectFilePath = baseline.ProjectFilePath,
        Secrets = baseline.Secrets,
        // Not copied directly: the SecretAccessPolicy setter below rebuilds it from Secrets + the
        // policy, the same way the original ResolvedSettings did — assigning it here would only be
        // overwritten and reads as if it mattered.
        SecretAccessPolicy = baseline.SecretAccessPolicy,
        TrustedDomains = [.. baseline.TrustedDomains],
        Docs = baseline.Docs,
        Ignore = baseline.Ignore,
        PermRead = baseline.PermRead,
        PermWrite = baseline.PermWrite,
        PermShell = baseline.PermShell,
        PermInternet = baseline.PermInternet,
        PermForeign = baseline.PermForeign,
        ToolPermissionRules = baseline.ToolPermissionRules,
        Plugins = baseline.Plugins,
        ToolSets = new Dictionary<string, string>(baseline.ToolSets, StringComparer.OrdinalIgnoreCase),
        ResourceSchemes = baseline.ResourceSchemes,
        Skills = baseline.Skills,
        SkillSources = baseline.SkillSources,
        SkillsInheritDefaults = baseline.SkillsInheritDefaults,
        SkillsMaxTrust = baseline.SkillsMaxTrust,
        SkillsUserMayVouch = baseline.SkillsUserMayVouch,
        IsMultiUserDeployment = baseline.IsMultiUserDeployment,
        PersonalDir = baseline.PersonalDir,
        SkillSourceStore = baseline.SkillSourceStore,
        SkillTrustStore = baseline.SkillTrustStore,
        SkillLibrarian = baseline.SkillLibrarian,
        RoleName = roleName
    };

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

    /// <summary>Merges one layer's <c>resources:</c> entries key by key, same rule as
    /// <see cref="ApplyToolSets"/>: the more specific layer overrides a scheme it mentions and leaves
    /// every other scheme's inherited switch alone.</summary>
    private static void ApplyResourceSchemes(ResolvedSettings r, Dictionary<string, bool>? resources)
    {
        if (resources == null) return;

        foreach (var kvp in resources)
            r.ResourceSchemes[kvp.Key] = kvp.Value;
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
    /// <summary>Layers accumulate rather than override: a project vouching for its own wiki must not
    /// silently drop what the machine layer vouched for.</summary>
    private static void AddTrustedDomains(ResolvedSettings r, List<string>? declared)
    {
        if (declared is null) return;
        foreach (var d in declared)
        {
            if (string.IsNullOrWhiteSpace(d)) continue;
            if (!r.TrustedDomains.Any(x => string.Equals(x, d.Trim(), StringComparison.OrdinalIgnoreCase)))
                r.TrustedDomains.Add(d.Trim());
        }
    }

    private static void ApplySkills(ResolvedSettings r, SplaSkillsSection? skills, SourceOrigin origin)
    {
        if (skills == null) return;

        if (skills.InheritDefaults.HasValue)
            r.SkillsInheritDefaults = skills.InheritDefaults.Value;

        // Policy is the administrator's, so it is heard from the administrator's layer only. A
        // project raising its own ceiling would be the exact move the ceiling exists to stop.
        if (origin == SourceOrigin.Machine && skills.Policy is { } policy)
        {
            if (policy.MaxTrust is { } maxTrust) r.SkillsMaxTrust = maxTrust;
            if (policy.UserMayVouch is { } mayVouch) r.SkillsUserMayVouch = mayVouch;
        }

        if (skills.Sources != null)
            foreach (var source in skills.Sources)
            {
                source.Origin = origin;
                r.SkillSources.Add(source);
            }

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

    /// <summary>Adds/overrides mcp.servers entries by id, skipping entries without an id — same idiom
    /// as <see cref="MergeConnections"/>. A project entry replaces a machine entry of the same id
    /// wholesale rather than merging field by field: a server definition assembled from half-machine,
    /// half-project fields (an env var from one layer, a command from the other) would be very hard to
    /// reason about, so "the more specific layer wins, entirely" is the same call <c>connections:</c>
    /// already made and this follows it rather than inventing a second answer.</summary>
    private static void MergeMcpServers(
        Dictionary<string, SplaMcpServerSection> into, List<SplaMcpServerSection>? from)
    {
        if (from == null) return;
        foreach (var s in from)
            if (!string.IsNullOrWhiteSpace(s.Id))
                into[s.Id!] = s;
    }
}
