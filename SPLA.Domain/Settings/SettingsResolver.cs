using SPLA.Domain.Models;

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
        Theme = Theme
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

        // Layer 1: defaults
        if (defaults != null)
        {
            if (defaults.Llm != null)
            {
                r.Endpoint = defaults.Llm.Endpoint ?? r.Endpoint;
                r.ApiKey = defaults.Llm.ApiKey ?? r.ApiKey;
                r.Model = defaults.Llm.Model ?? r.Model;
                r.Temperature = defaults.Llm.Temperature ?? r.Temperature;
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

            if (project.Llm != null)
            {
                r.Endpoint = project.Llm.Endpoint ?? r.Endpoint;
                r.ApiKey = project.Llm.ApiKey ?? r.ApiKey;
                r.Model = project.Llm.Model ?? r.Model;
                r.Temperature = project.Llm.Temperature ?? r.Temperature;
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

        return r;
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
