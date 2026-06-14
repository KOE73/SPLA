using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private string _chatRenderMode = "native";

    [ObservableProperty]
    private ObservableCollection<string> _availableModels = new();

    [ObservableProperty]
    private string _selectedModel = "";

    [ObservableProperty]
    private string _statusMessage = "";

    public ObservableCollection<PluginSettingViewModel> PluginsSettings { get; } = new();

    // Categories for Left Menu
    public string[] Categories { get; } = new[] { "App Appearance", "LLM Provider", "Models", "Agent Settings", "Local Permissions", "System Integration", "Plugins", "", "About SPLA" };

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
        OnPropertyChanged(nameof(IsModels));
        OnPropertyChanged(nameof(IsAgentSettings));
        OnPropertyChanged(nameof(IsLocalPermissions));
        OnPropertyChanged(nameof(IsSystemIntegration));
        OnPropertyChanged(nameof(IsPlugins));
        OnPropertyChanged(nameof(IsAbout));
    }

    public bool IsAppAppearance => SelectedCategory == "App Appearance";
    public bool IsLlmProvider => SelectedCategory == "LLM Provider";
    public bool IsModels => SelectedCategory == "Models";
    public bool IsAgentSettings => SelectedCategory == "Agent Settings";
    public bool IsLocalPermissions => SelectedCategory == "Local Permissions";
    public bool IsSystemIntegration => SelectedCategory == "System Integration";
    public bool IsPlugins => SelectedCategory == "Plugins";
    public bool IsAbout => SelectedCategory == "About SPLA";

    public LLMSettings GetSettings() => new LLMSettings
    {
        BaseUrl = BaseUrl,
        ApiKey = ApiKey,
        ModelName = SelectedModel ?? "local-model",
        Temperature = Temperature,
        Mode = Mode,
        Theme = Theme
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
        Theme = resolved.Theme;
        Density = resolved.Density;
        ActiveProfileId = resolved.ActiveProfileId;
        ChatRenderMode = resolved.ChatRenderMode;
        CustomPrompt = resolved.CustomPrompt ?? "";

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
                    Temperature = Temperature
                };
                project.Agent ??= new SplaAgentSection();
                project.Agent.Mode = Mode.ToString();
                project.Agent.CustomPrompt = CustomPrompt;
                project.Ui = new SplaUiSection
                {
                    Theme = Theme,
                    Density = Density,
                    ChatRenderMode = ChatRenderMode,
                    ActiveProfileId = ActiveProfileId
                };

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
                    }
                }

                ConfigLoader.SaveProject(project, App.ProjectFilePath);
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
                    Temperature = Temperature
                },
                Agent = new SplaAgentSection { Mode = Mode.ToString(), CustomPrompt = CustomPrompt },
                Ui = new SplaUiSection
                {
                    Theme = Theme,
                    Density = Density,
                    ChatRenderMode = ChatRenderMode,
                    ActiveProfileId = ActiveProfileId
                }
            };

            ConfigLoader.SaveDefaults(defaults);
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
            StatusMessage = "Fetching models...";
            using var http = new HttpClient();
            var client = new LMStudioClient(http);
            var models = await client.GetModelsAsync(BaseUrl, ApiKey);
            
            AvailableModels.Clear();
            foreach (var m in models) AvailableModels.Add(m);
            
            if (AvailableModels.Count > 0)
                SelectedModel = AvailableModels[0];
                
            StatusMessage = $"Found {models.Count} models.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error fetching models: {ex.Message}";
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
