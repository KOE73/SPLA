using SPLA.Domain.Models;
using SPLA.Domain.Secrets;

namespace SPLA.Domain.Settings;

/// <summary>
/// Resolved, ready-to-use settings after merging defaults + project.
/// </summary>
public class ResolvedSettings
{
    // LLM
    public string Endpoint { get; set; } = "http://127.0.0.1:1234/v1/";
    public string ApiKey { get; set; } = "lm-studio";
    public string Model { get; set; } = "auto";
    public double Temperature { get; set; } = 0.7;
    public string? ReasoningLevel { get; set; }

    /// <summary>Named connections available to chats. Never empty after resolution — a default is
    /// synthesized from the single <c>llm</c> section when none are configured.</summary>
    public List<SplaConnectionSection> Connections { get; set; } = new();

    // Agent
    public AgentMode Mode { get; set; } = AgentMode.Edit;
    public List<string> Instructions { get; set; } = new();
    public int CompactTailMessages { get; set; } = 2;
    public string? CustomPrompt { get; set; }

    // UI
    public string Theme { get; set; } = "Dark";
    public string Density { get; set; } = "norm";

    // Chat engine: "native" | "web"
    public string ChatRenderMode { get; set; } = "native";

    // Active display profile id
    public string ActiveProfileId { get; set; } = "bubbles";

    // User-defined profiles added on top of built-ins (override by id)
    public List<ChatDisplayProfile> CustomProfiles { get; set; } = [];

    /// <summary>Built-in profiles merged with any user-defined overrides/additions.</summary>
    public IReadOnlyList<ChatDisplayProfile> EffectiveProfiles
    {
        get
        {
            if (CustomProfiles.Count == 0) return ChatDisplayProfile.BuiltInProfiles;
            var dict = ChatDisplayProfile.BuiltInProfiles.ToDictionary(p => p.Id);
            foreach (var cp in CustomProfiles) dict[cp.Id] = cp;
            return [.. dict.Values];
        }
    }

    // Project
    public string? ProjectName { get; set; }
    public string WorkspacePath { get; set; } = ".";

    /// <summary>Absolute path to the .spla file that was loaded, or null when running without a project.
    /// Plugins that need to persist their own settings (e.g. sql_manage_connection) use this.</summary>
    public string? ProjectFilePath { get; set; }

    /// <summary>Global secrets store (project + machine scoped). Set during load. Never null after
    /// <see cref="ConfigLoader.LoadAndResolve"/>; plugins reach it via this property.</summary>
    public ISecretStore Secrets { get; set; } = null!;

    /// <summary>Resolves <c>secret:</c> / <c>env:</c> references in config values to plaintext.</summary>
    public ISecretResolver SecretResolver { get; set; } = null!;

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

    // Skills
    public Dictionary<string, SplaSkillSection> Skills { get; set; } = new();

    /// <summary>
    /// Converts to LLMSettings for backward compatibility with LLM client.
    /// </summary>
    public LLMSettings ToLLMSettings() => new()
    {
        BaseUrl = Endpoint,
        ApiKey = ApiKey,
        ModelName = Model == "auto" ? "local-model" : Model,
        Temperature = Temperature,
        Mode = Mode,
        Theme = Theme,
        ReasoningLevel = ReasoningLevel
    };
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

        // Layer 1: defaults
        if (defaults != null)
        {
            MergeConnections(connections, defaults.Connections);
            if (defaults.Llm != null)
            {
                r.Endpoint = defaults.Llm.Endpoint ?? r.Endpoint;
                r.ApiKey = defaults.Llm.ApiKey ?? r.ApiKey;
                r.Model = defaults.Llm.Model ?? r.Model;
                r.Temperature = defaults.Llm.Temperature ?? r.Temperature;
                r.ReasoningLevel = defaults.Llm.ReasoningLevel ?? r.ReasoningLevel;
            }
            if (defaults.Agent != null)
            {
                if (defaults.Agent.Mode != null && Enum.TryParse<AgentMode>(defaults.Agent.Mode, true, out var m))
                    r.Mode = m;
                if (defaults.Agent.CompactTailMessages.HasValue)
                    r.CompactTailMessages = defaults.Agent.CompactTailMessages.Value;
                if (!string.IsNullOrEmpty(defaults.Agent.CustomPrompt))
                    r.CustomPrompt = defaults.Agent.CustomPrompt;
            }
            if (defaults.Ui != null)
            {
                r.Theme = defaults.Ui.Theme ?? r.Theme;
                r.Density = defaults.Ui.Density ?? r.Density;
                r.ChatRenderMode = defaults.Ui.ChatRenderMode ?? r.ChatRenderMode;
                r.ActiveProfileId = ResolveActiveProfileId(defaults.Ui) ?? r.ActiveProfileId;
                if (defaults.Ui.ChatProfiles?.Count > 0) r.CustomProfiles = defaults.Ui.ChatProfiles;
            }
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
                r.Endpoint = project.Llm.Endpoint ?? r.Endpoint;
                r.ApiKey = project.Llm.ApiKey ?? r.ApiKey;
                r.Model = project.Llm.Model ?? r.Model;
                r.Temperature = project.Llm.Temperature ?? r.Temperature;
                r.ReasoningLevel = project.Llm.ReasoningLevel ?? r.ReasoningLevel;
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
            }
            if (project.Ui != null)
            {
                r.Theme = project.Ui.Theme ?? r.Theme;
                r.Density = project.Ui.Density ?? r.Density;
                r.ChatRenderMode = project.Ui.ChatRenderMode ?? r.ChatRenderMode;
                r.ActiveProfileId = ResolveActiveProfileId(project.Ui) ?? r.ActiveProfileId;
                if (project.Ui.ChatProfiles?.Count > 0) r.CustomProfiles = project.Ui.ChatProfiles;
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
            if (project.Skills != null)
            {
                foreach (var kvp in project.Skills)
                    r.Skills[kvp.Key] = kvp.Value;
            }
        }

        // Finalize connections: keep configured ones; synthesize a default from the resolved single
        // llm section when none were declared, so chats always have at least one connection to use.
        r.Connections = connections.Values.ToList();
        if (r.Connections.Count == 0)
        {
            r.Connections.Add(new SplaConnectionSection
            {
                Id = "default",
                Name = "Default",
                Provider = "lmstudio",
                Endpoint = r.Endpoint,
                ApiKey = r.ApiKey,
                Model = r.Model
            });
        }

        return r;
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

    /// <summary>
    /// Resolves the active profile id from a UI section, migrating legacy fields.
    /// Returns null if nothing is configured (caller keeps current default).
    /// </summary>
    private static string? ResolveActiveProfileId(SplaUiSection ui)
    {
        // New field wins
        if (!string.IsNullOrEmpty(ui.ActiveProfileId)) return ui.ActiveProfileId;

        // Migrate from old selected_chat_view_id (skip "web" — that's a render mode now)
        if (!string.IsNullOrEmpty(ui.SelectedChatViewId) && ui.SelectedChatViewId != "web")
            return ui.SelectedChatViewId;

        // Migrate from bubble_chat bool
        if (ui.BubbleChat == true) return "bubbles";
        if (ui.BubbleChat == false) return "classic";

        return null;
    }
}
