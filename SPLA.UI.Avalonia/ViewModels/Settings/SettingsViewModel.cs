using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.LMStudio;
using SPLA.MCP.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels.Settings;

public partial class SettingsViewModel : ViewModelBase
{
    public string ProjectFilePath => string.IsNullOrWhiteSpace(App.ProjectFilePath)
        ? "no project"
        : App.ProjectFilePath;

    [ObservableProperty]
    private string _baseUrl = "http://127.0.0.1:1234/v1/";

    [ObservableProperty]
    private string _apiKey = "lm-studio";

    [ObservableProperty]
    private AgentMode _mode = AgentMode.Edit;

    [ObservableProperty]
    private double _temperature = 0.7;

    [ObservableProperty]
    private string _customPrompt = "";

    partial void OnModeChanged(AgentMode value)
    {
        OnPropertyChanged(nameof(IsModeChat));
        OnPropertyChanged(nameof(IsModeResearch));
        OnPropertyChanged(nameof(IsModeInspect));
        OnPropertyChanged(nameof(IsModeEdit));
        OnPropertyChanged(nameof(IsModeAgent));
    }

    public bool IsModeChat { get => Mode == AgentMode.Chat; set { if (value) Mode = AgentMode.Chat; } }
    public bool IsModeResearch { get => Mode == AgentMode.Research; set { if (value) Mode = AgentMode.Research; } }
    public bool IsModeInspect { get => Mode == AgentMode.Inspect; set { if (value) Mode = AgentMode.Inspect; } }
    public bool IsModeEdit { get => Mode == AgentMode.Edit; set { if (value) Mode = AgentMode.Edit; } }
    public bool IsModeAgent { get => Mode == AgentMode.Agent; set { if (value) Mode = AgentMode.Agent; } }

    public AgentMode[] AvailableModes { get; } = (AgentMode[])Enum.GetValues(typeof(AgentMode));

    [ObservableProperty]
    private string _theme = "Dark";

    public string[] AvailableThemes { get; } = new[] { "Dark", "Light", "Cream", "Emerald" };

    [ObservableProperty]
    private string _density = "norm";

    public string[] AvailableDensities { get; } = new[] { "nano", "mini", "norm", "max" };

    [ObservableProperty]
    private string _activeProfileId = "bubbles";

    [ObservableProperty]
    private ObservableCollection<string> _availableModels = new();

    [ObservableProperty]
    private ObservableCollection<ModelInfo> _modelDetails = new();

    [ObservableProperty]
    private bool _hasModelDetails;

    private string _selectedModel = "";
    public string SelectedModel
    {
        get => _selectedModel;
        set
        {
            if (SetProperty(ref _selectedModel, value))
            {
                OnPropertyChanged(nameof(SelectedModelSupportsReasoning));
                OnPropertyChanged(nameof(SelectedModelReasoningOptions));
                // Snap the level to the new model's default (or clear when unsupported).
                var info = ModelDetails.FirstOrDefault(m => m.Id == value);
                ReasoningLevel = info != null && info.SupportsReasoning
                    ? (string.IsNullOrEmpty(info.ReasoningDefault) ? info.ReasoningOptions[0] : info.ReasoningDefault)
                    : "";
            }
        }
    }

    [ObservableProperty]
    private string _statusMessage = "";

    /// <summary>Currently selected reasoning option (persisted). Empty = model default / unsupported.</summary>
    [ObservableProperty]
    private string _reasoningLevel = "";

    private ModelInfo? SelectedModelInfo => ModelDetails.FirstOrDefault(m => m.Id == SelectedModel);

    /// <summary>True when the currently selected model advertises a reasoning channel.</summary>
    public bool SelectedModelSupportsReasoning => SelectedModelInfo?.SupportsReasoning ?? false;

    /// <summary>Reasoning options for the selected model (e.g. ["off","on"] or ["low","medium","high"]).</summary>
    public ObservableCollection<string> SelectedModelReasoningOptions =>
        new(SelectedModelInfo?.ReasoningOptions ?? new());

    public ObservableCollection<PluginSettingViewModel> PluginsSettings { get; } = new();

    public ObservableCollection<string> IgnorePatterns { get; } = new();

    [ObservableProperty]
    private string _newIgnorePattern = "";

    [RelayCommand]
    private void AddIgnorePattern()
    {
        var p = NewIgnorePattern.Trim();
        if (!string.IsNullOrEmpty(p) && !IgnorePatterns.Contains(p))
            IgnorePatterns.Add(p);
        NewIgnorePattern = "";
    }

    [RelayCommand]
    private void RemoveIgnorePattern(string? pattern)
    {
        if (pattern is not null) IgnorePatterns.Remove(pattern);
    }

    [RelayCommand]
    private void ResetIgnorePatterns()
    {
        IgnorePatterns.Clear();
        foreach (var p in ConfigLoader.DefaultIgnorePatterns) IgnorePatterns.Add(p);
    }

    // Categories for Left Menu (plugin-provided settings pages are inserted under "Plugins").
    private static readonly string[] BaseCategories =
        { "App Appearance", "LLM Provider", "Server", "Agent Settings", "Project", "Local Permissions", "System Integration", "Plugins" };
    private static readonly string[] TailCategories = { "", "Token Usage", "About SPLA" };

    public ObservableCollection<string> Categories { get; } = new(BaseCategories.Concat(TailCategories));

    /// <summary>Menu title -> the plugin row whose editor that page shows.</summary>
    private readonly Dictionary<string, PluginSettingViewModel> _pluginEditorPages = new();

    /// <summary>The plugin editor control shown when a plugin settings page is selected.
    /// Typed as <see cref="Control"/> so that <see cref="SettingsWindow"/> can assign it to
    /// <c>Border.Child</c> directly — avoids the "already has a visual parent" crash that
    /// ContentControl triggers when <c>IsVisible=False</c> keeps the child in the visual tree.</summary>
    public Control? CurrentPluginEditorView { get; private set; }

    /// <summary>True when the selected menu item is a plugin-provided settings page.</summary>
    public bool IsPluginEditorPage { get; private set; }

    [ObservableProperty]
    private string _selectedCategory = "App Appearance";

    private string _previousCategory = "App Appearance";

    partial void OnSelectedCategoryChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            SelectedCategory = _previousCategory;
            return;
        }
        _previousCategory = value;

        OnPropertyChanged(nameof(IsAppAppearance));
        OnPropertyChanged(nameof(IsLlmProvider));
        OnPropertyChanged(nameof(IsServer));
        OnPropertyChanged(nameof(IsAgentSettings));
        OnPropertyChanged(nameof(IsProject));
        OnPropertyChanged(nameof(IsLocalPermissions));
        OnPropertyChanged(nameof(IsSystemIntegration));
        OnPropertyChanged(nameof(IsPlugins));
        OnPropertyChanged(nameof(IsTokenUsage));
        OnPropertyChanged(nameof(IsAbout));

        if (IsTokenUsage) RefreshTokenUsage();

        IsPluginEditorPage = _pluginEditorPages.TryGetValue(value, out var page);
        if (IsPluginEditorPage && page?.Editor != null && page.SettingsStore != null)
            ReloadPluginEditor(page);
        CurrentPluginEditorView = IsPluginEditorPage ? page!.EditorView as Control : null;
        OnPropertyChanged(nameof(IsPluginEditorPage));
        OnPropertyChanged(nameof(CurrentPluginEditorView));
    }

    public bool IsAppAppearance => SelectedCategory == "App Appearance";
    public bool IsLlmProvider => SelectedCategory == "LLM Provider";
    public bool IsServer => SelectedCategory == "Server";
    public bool IsAgentSettings => SelectedCategory == "Agent Settings";
    public bool IsProject => SelectedCategory == "Project";
    public bool IsLocalPermissions => SelectedCategory == "Local Permissions";
    public bool IsSystemIntegration => SelectedCategory == "System Integration";
    public bool IsPlugins => SelectedCategory == "Plugins";
    public bool IsTokenUsage => SelectedCategory == "Token Usage";
    public bool IsAbout => SelectedCategory == "About SPLA";

    public LLMSettings GetSettings() => new LLMSettings
    {
        BaseUrl = BaseUrl,
        ApiKey = ApiKey,
        ModelName = SelectedModel ?? "local-model",
        Temperature = Temperature,
        Mode = Mode,
        Theme = Theme,
        ReasoningLevel = string.IsNullOrEmpty(ReasoningLevel) ? null : ReasoningLevel
    };

    /// <summary>
    /// Loads settings from ResolvedSettings (already merged from defaults.yaml + .spla).
    /// </summary>
    public void LoadFromResolved(ResolvedSettings resolved)
    {
        BaseUrl = resolved.Endpoint;
        ApiKey = resolved.ApiKey;
        Mode = resolved.Mode;
        Temperature = resolved.Temperature;
        ReasoningLevel = resolved.ReasoningLevel ?? "";
        Theme = resolved.Theme;
        Density = resolved.Density;
        ActiveProfileId = resolved.ActiveProfileId;
        CustomPrompt = resolved.CustomPrompt ?? "";

        IgnorePatterns.Clear();
        foreach (var p in resolved.Ignore) IgnorePatterns.Add(p);

        if (resolved.Model != "auto" && !string.IsNullOrEmpty(resolved.Model))
        {
            AvailableModels.Add(resolved.Model);
            SelectedModel = resolved.Model;
        }

        LoadPluginSettingsTree(resolved);
    }

    [RelayCommand]
    public async Task SaveSettingsAsync()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(App.ProjectFilePath))
            {
                var project = ConfigLoader.LoadProjectRaw(App.ProjectFilePath);
                project.Llm = new SplaLlmSection
                {
                    Provider = "lmstudio",
                    Endpoint = BaseUrl,
                    ApiKey = ApiKey,
                    Model = SelectedModel ?? "auto",
                    Temperature = Temperature,
                    ReasoningLevel = string.IsNullOrEmpty(ReasoningLevel) ? null : ReasoningLevel
                };
                project.Agent ??= new SplaAgentSection();
                project.Agent.Mode = Mode.ToString();
                project.Agent.CustomPrompt = CustomPrompt;
                project.Ui = new SplaUiSection
                {
                    Theme = Theme,
                    Density = Density,
                    ActiveProfileId = ActiveProfileId
                };

                project.Ignore = IgnorePatterns.Count > 0 ? IgnorePatterns.ToList() : null;

                if (PluginsSettings.Count > 0)
                {
                    project.Plugins ??= new System.Collections.Generic.Dictionary<string, SplaPluginSection>();
                    foreach (var p in EnumeratePluginSettings())
                    {
                        if (!project.Plugins.TryGetValue(p.Id, out var pSec))
                        {
                            pSec = new SplaPluginSection();
                            project.Plugins[p.Id] = pSec;
                        }
                        pSec.Enabled = p.Enabled;
                        pSec.CustomPrompt = string.IsNullOrWhiteSpace(p.CustomPrompt) ? null : p.CustomPrompt;

                        // Let a plugin-provided settings editor persist its own opaque blob.
                        if (p.Editor is not null && p.SettingsStore is not null)
                        {
                            p.Editor.Save(p.SettingsStore);
                            pSec.Settings = ConfigLoader.DeserializeBlob(p.SettingsStore.GetYaml());
                        }
                    }
                }

                ConfigLoader.SaveProject(project, App.ProjectFilePath);
                App.ReloadResolvedSettings();   // refresh the in-memory snapshot so new chats pick this up without a restart
                StatusMessage = $"Project settings saved: {Path.GetFileName(App.ProjectFilePath)}";
                return;
            }

            var defaults = new SplaDefaults
            {
                Version = 1,
                Llm = new SplaLlmSection
                {
                    Provider = "lmstudio",
                    Endpoint = BaseUrl,
                    ApiKey = ApiKey,
                    Model = SelectedModel ?? "auto",
                    Temperature = Temperature,
                    ReasoningLevel = string.IsNullOrEmpty(ReasoningLevel) ? null : ReasoningLevel
                },
                Agent = new SplaAgentSection { Mode = Mode.ToString(), CustomPrompt = CustomPrompt },
                Ui = new SplaUiSection
                {
                    Theme = Theme,
                    Density = Density,
                    ActiveProfileId = ActiveProfileId
                }
            };

            ConfigLoader.SaveDefaults(defaults);
            App.ReloadResolvedSettings();   // refresh the in-memory snapshot so new chats pick this up without a restart
            StatusMessage = "Settings saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void LoadPluginSettingsTree(ResolvedSettings resolved)
    {
        PluginsSettings.Clear();
        var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        var descriptors = PluginManager.DiscoverPlugins(resolved, pluginsDir);
        var byId = descriptors.ToDictionary(p => p.Meta.Id, StringComparer.OrdinalIgnoreCase);
        var viewModels = descriptors.ToDictionary(p => p.Meta.Id, p =>
        {
            resolved.Plugins.TryGetValue(p.Meta.Id, out var existingCfg);
            return new PluginSettingViewModel
            {
                Id = p.Meta.Id,
                Type = p.Meta.Type,
                Enabled = existingCfg?.Enabled ?? p.UserEnabled,
                CustomPrompt = existingCfg?.CustomPrompt ?? "",
                EffectiveState = p.EffectiveState.ToString(),
                EffectiveStateReason = p.EffectiveStateReason,
                DependsOn = string.Join(", ", p.Meta.DependsOn)
            };
        }, StringComparer.OrdinalIgnoreCase);

        foreach (var descriptor in descriptors)
        {
            var viewModel = viewModels[descriptor.Meta.Id];
            var parentId = descriptor.Meta.DependsOn.FirstOrDefault(d => byId.ContainsKey(d));
            if (!string.IsNullOrWhiteSpace(parentId))
            {
                viewModels[parentId].AddChild(viewModel);
                continue;
            }

            PluginsSettings.Add(viewModel);
        }

        WirePluginSettingsEditors(resolved, viewModels);
    }

    /// <summary>
    /// Attaches plugin-provided settings editors (registered by avalonia-ui plugins) to the
    /// matching plugin rows, and primes each editor from its opaque blob in .spla.
    /// </summary>
    private void WirePluginSettingsEditors(
        ResolvedSettings resolved, Dictionary<string, PluginSettingViewModel> viewModels)
    {
        _pluginEditorPages.Clear();
        RebuildCategories();

        var registry = App.Services.GetService<Services.Plugins.IAvaloniaPluginSettingsRegistry>();
        if (registry is null) return;

        foreach (var descriptor in registry.Descriptors)
        {
            if (!viewModels.TryGetValue(descriptor.PluginId, out var vm))
                continue;

            resolved.Plugins.TryGetValue(descriptor.PluginId, out var section);
            var store = new Services.Plugins.PluginSettingsStore(ConfigLoader.SerializeBlob(section?.Settings));
            var editor = descriptor.CreateEditor();
            editor.Load(store);

            vm.AttachEditor(editor, store);

            // Each editor becomes its own page nested under "Plugins" in the left menu.
            var pageTitle = "    " + descriptor.DisplayName;
            _pluginEditorPages[pageTitle] = vm;
        }

        RebuildCategories();
    }

    /// <summary>
    /// Re-reads plugin settings from the .spla file and reloads the editor.
    /// Called each time the user navigates to a plugin settings page so changes
    /// made by the agent are reflected without reopening the Settings window.
    /// </summary>
    private static void ReloadPluginEditor(PluginSettingViewModel page)
    {
        if (page.Editor == null || page.SettingsStore == null) return;
        var projectFilePath = App.ProjectFilePath;
        if (string.IsNullOrWhiteSpace(projectFilePath) || !File.Exists(projectFilePath)) return;

        try
        {
            var project = ConfigLoader.LoadProjectRaw(projectFilePath);
            SplaPluginSection? section = null;
            project.Plugins?.TryGetValue(page.Id, out section);
            var freshYaml = ConfigLoader.SerializeBlob(section?.Settings);
            page.SettingsStore.SetYaml(freshYaml ?? "");
            page.Editor.Load(page.SettingsStore);
        }
        catch { /* ignore read errors; stale view is acceptable */ }
    }

    private void RebuildCategories()
    {
        Categories.Clear();
        foreach (var c in BaseCategories) Categories.Add(c);
        foreach (var title in _pluginEditorPages.Keys) Categories.Add(title);
        foreach (var c in TailCategories) Categories.Add(c);
    }

    private IEnumerable<PluginSettingViewModel> EnumeratePluginSettings()
    {
        foreach (var plugin in PluginsSettings)
        {
            foreach (var item in EnumeratePluginSettings(plugin))
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<PluginSettingViewModel> EnumeratePluginSettings(PluginSettingViewModel plugin)
    {
        yield return plugin;

        foreach (var child in plugin.Children)
        {
            foreach (var item in EnumeratePluginSettings(child))
            {
                yield return item;
            }
        }
    }

    [RelayCommand]
    private async Task FetchModelsAsync()
    {
        try
        {
            StatusMessage = "Scanning provider...";
            using var http = new HttpClient();
            var mgmt = new LMStudioManagementClient(http);
            var details = await mgmt.GetModelDetailsAsync(BaseUrl, ApiKey);

            var previouslySelected = SelectedModel;

            ModelDetails.Clear();
            foreach (var d in details) ModelDetails.Add(d);
            HasModelDetails = ModelDetails.Count > 0;

            AvailableModels.Clear();
            foreach (var d in details) AvailableModels.Add(d.Id);

            if (AvailableModels.Contains(previouslySelected))
                SelectedModel = previouslySelected;
            else if (AvailableModels.Count > 0)
                SelectedModel = AvailableModels[0];

            OnPropertyChanged(nameof(SelectedModelSupportsReasoning));
            OnPropertyChanged(nameof(SelectedModelReasoningOptions));
            if (SelectedModelInfo is { SupportsReasoning: true } info && !info.ReasoningOptions.Contains(ReasoningLevel))
                ReasoningLevel = string.IsNullOrEmpty(info.ReasoningDefault) ? info.ReasoningOptions[0] : info.ReasoningDefault;
            else if (SelectedModelInfo is null or { SupportsReasoning: false })
                ReasoningLevel = "";

            var rich = details.Count(d => d.HasDetails);
            StatusMessage = rich > 0
                ? $"Found {details.Count} models ({rich} with capabilities)."
                : $"Found {details.Count} models (basic list — capabilities unavailable for this provider).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error fetching models: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadModelAsync(ModelInfo? model)
    {
        if (model == null) return;
        try
        {
            StatusMessage = $"Loading {model.Id}…";
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            await new LMStudioManagementClient(http).LoadModelAsync(BaseUrl, ApiKey, model.Id);
            StatusMessage = $"Loaded {model.Id}.";
            await FetchModelsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UnloadModelAsync(ModelInfo? model)
    {
        if (model == null) return;
        try
        {
            StatusMessage = $"Unloading {model.Id}…";
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            await new LMStudioManagementClient(http).UnloadModelAsync(BaseUrl, ApiKey, model.UnloadId);
            StatusMessage = $"Unloaded {model.Id}.";
            await FetchModelsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unload failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RegisterAssociation()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                StatusMessage = "File association is only supported on Windows.";
                return;
            }

            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath)) return;

            // Associate .spla with SPLA.Project.1
            RunCmd($@"reg add HKCU\Software\Classes\.spla /d ""SPLA.Project.1"" /f");
            // Define SPLA.Project.1
            RunCmd($@"reg add HKCU\Software\Classes\SPLA.Project.1 /d ""SPLA Project"" /f");
            // Define the shell command
            RunCmd($@"reg add ""HKCU\Software\Classes\SPLA.Project.1\shell\open\command"" /d ""\""{exePath}\"" \""%1\"""" /f");
            // Define the icon
            RunCmd($@"reg add ""HKCU\Software\Classes\SPLA.Project.1\DefaultIcon"" /d ""\""{exePath}\"",0"" /f");
            
            StatusMessage = "File association registered for .spla files.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error registering association: {ex.Message}";
        }
    }

    private void RunCmd(string arguments)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {arguments}",
            CreateNoWindow = true,
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
        })?.WaitForExit();
    }

    // ── Token usage statistics (real provider-reported tokens) ──────────────────
    // Three scopes: this run (Session), this project's lifetime, and the whole machine.

    [ObservableProperty] private long _usageSessionPrompt;
    [ObservableProperty] private long _usageSessionCompletion;
    [ObservableProperty] private long _usageSessionTurns;

    [ObservableProperty] private long _usageProjectPrompt;
    [ObservableProperty] private long _usageProjectCompletion;
    [ObservableProperty] private long _usageProjectTurns;

    [ObservableProperty] private long _usageGlobalPrompt;
    [ObservableProperty] private long _usageGlobalCompletion;
    [ObservableProperty] private long _usageGlobalTurns;

    public long UsageSessionTotal => UsageSessionPrompt + UsageSessionCompletion;
    public long UsageProjectTotal => UsageProjectPrompt + UsageProjectCompletion;
    public long UsageGlobalTotal  => UsageGlobalPrompt + UsageGlobalCompletion;

    // Prompt share (0..1) of each scope's total, for the in/out split bars.
    public double UsageSessionPromptFraction => Fraction(UsageSessionPrompt, UsageSessionTotal);
    public double UsageProjectPromptFraction => Fraction(UsageProjectPrompt, UsageProjectTotal);
    public double UsageGlobalPromptFraction  => Fraction(UsageGlobalPrompt, UsageGlobalTotal);

    private static double Fraction(long part, long whole) => whole > 0 ? (double)part / whole : 0;

    [RelayCommand]
    public void RefreshTokenUsage()
    {
        var session = App.TokenUsage.Session;
        var project = App.TokenUsage.Total;
        var global  = App.TokenUsageGlobal.Total;

        UsageSessionPrompt = session.PromptTokens;
        UsageSessionCompletion = session.CompletionTokens;
        UsageSessionTurns = session.Turns;

        UsageProjectPrompt = project.PromptTokens;
        UsageProjectCompletion = project.CompletionTokens;
        UsageProjectTurns = project.Turns;

        UsageGlobalPrompt = global.PromptTokens;
        UsageGlobalCompletion = global.CompletionTokens;
        UsageGlobalTurns = global.Turns;

        OnPropertyChanged(nameof(UsageSessionTotal));
        OnPropertyChanged(nameof(UsageProjectTotal));
        OnPropertyChanged(nameof(UsageGlobalTotal));
        OnPropertyChanged(nameof(UsageSessionPromptFraction));
        OnPropertyChanged(nameof(UsageProjectPromptFraction));
        OnPropertyChanged(nameof(UsageGlobalPromptFraction));
    }

    public string BuildTime
    {
        get
        {
            try
            {
                var dllPath = Path.Combine(AppContext.BaseDirectory, "SPLA.UI.Avalonia.dll");
                if (File.Exists(dllPath))
                {
                    return File.GetLastWriteTime(dllPath).ToString("yyyy-MM-dd HH:mm:ss");
                }
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                {
                    return File.GetLastWriteTime(exePath).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch {}
            return "Unknown";
        }
    }
}
