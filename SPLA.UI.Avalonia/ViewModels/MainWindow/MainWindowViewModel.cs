using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Models;
using SPLA.Domain.Settings;
using SPLA.LLM.LMStudio;
using SPLA.MCP.Core;
using SPLA.MCP.Core.Interfaces;
using SPLA.MCP.Core.Permissions;
using SPLA.MCP.BasicTools.FileSystem;
using SPLA.MCP.BasicTools.SystemTools;
using SPLA.UI.Avalonia.ViewModels.Messages;
using SPLA.UI.Avalonia.ViewModels.Projects;
using SPLA.UI.Avalonia.ViewModels.Settings;
using SPLA.UI.Avalonia.ViewModels.Status;
using SPLA.UI.Avalonia.ViewModels.Chat;
using SPLA.UI.Avalonia.ViewModels.Sidebar;
using SPLA.UI.Avalonia.Views.Chat;
using SPLA.UI.Avalonia.Services;
using SPLA.UI.Avalonia.Services.Guards;
using SPLA.UI.Avalonia.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
        public SettingsViewModel Settings { get; } = new();
        public StatusViewModel Status { get; } = new();
        public SidebarPanelViewModel Sidebar { get; } = new();

        [ObservableProperty]
        private bool _isSidebarOpen = false;

        [RelayCommand]
        private void ToggleSidebar() => IsSidebarOpen = !IsSidebarOpen;
        public Func<ChatSession, Task<bool>>? ConfirmDeleteChatAsync { get; set; }
        public Func<Task<string?>>? SelectProjectFolderAsync { get; set; }
    
        // ── Display profiles ───────────────────────────────────────────────────

        [ObservableProperty]
        private ObservableCollection<ChatProfileViewModel> _availableProfiles = new();

        [ObservableProperty]
        private ChatProfileViewModel? _selectedProfile;

        partial void OnSelectedProfileChanged(ChatProfileViewModel? value)
        {
            OnPropertyChanged(nameof(ActiveProfile));
            OnPropertyChanged(nameof(ActiveProfileUsesBubbles));
            OnPropertyChanged(nameof(ActiveProfileUsesLinear));

            if (value != null && Settings.ActiveProfileId != value.Id)
            {
                Settings.ActiveProfileId = value.Id;
                _ = Settings.SaveSettingsAsync();
            }
        }

        public bool ActiveProfileUsesBubbles => SelectedProfile?.Profile.UseBubbleLayout == true;
        public bool ActiveProfileUsesLinear  => SelectedProfile?.Profile.UseBubbleLayout != true;

        // ── Render mode: "native" | "web" ─────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNativeChatViewSelected))]
        [NotifyPropertyChangedFor(nameof(IsWebChatViewSelected))]
        private string _activeRenderMode = "native";

        partial void OnActiveRenderModeChanged(string value)
        {
            if (Settings.ChatRenderMode != value)
            {
                Settings.ChatRenderMode = value;
                _ = Settings.SaveSettingsAsync();
            }
        }

        public bool IsNativeChatViewSelected => ActiveRenderMode == "native";
        public bool IsWebChatViewSelected => ActiveRenderMode == "web";

        /// <summary>The active display profile domain object (null only before initialization).</summary>
        public ChatDisplayProfile? ActiveProfile => SelectedProfile?.Profile;
    
        [ObservableProperty]
        private ObservableCollection<MessageViewModel> _messages = new();
    
        [ObservableProperty]
        private string _inputText = "";
    
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;
    
        public bool IsNotBusy => !IsBusy;
    
        [ObservableProperty]
        private ObservableCollection<ChatSession> _chats = new();
    
        [ObservableProperty]
        private ObservableCollection<RecentProjectViewModel> _recentProjects = new();
    
        public bool HasRecentProjects => RecentProjects.Count > 0;
    
        [ObservableProperty]
        private ObservableCollection<SplaPluginUiCommand> _pluginUiCommands = new();
    
        public bool HasPluginUiCommands => PluginUiCommands.Count > 0;
    
        [ObservableProperty]
        private ChatSession? _currentChat;
    
        private readonly McpHost _mcpHost;
        private readonly PermissionManager _permissionManager;
        private readonly SPLA.MCP.Core.Plugins.PluginManager _pluginManager;
        private readonly SPLA.MCP.Core.Plugins.SkillManager _skillManager;
        private readonly SPLA.MCP.Core.Plugins.CapabilityRegistry _capabilityRegistry;
        private readonly HttpClient _httpClient;
        private readonly List<ToolDefinition> _tools;
        private ChatManager? _chatManager;
        private bool _isReloadingChats;
        private string? _loadedChatId;
        private CancellationTokenSource? _currentRequestCts;
        private DispatcherTimer? _tokenEstimateTimer;
        private DateTime _tokenEstimateStartedAt;
    
        public MainWindowViewModel()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            _permissionManager = CreatePermissionManager();
            
            var logger = App.Services.GetRequiredService<ILogger<MainWindowViewModel>>();
            logger.LogInformation("MainWindowViewModel initialization started.");

            _pluginManager = new SPLA.MCP.Core.Plugins.PluginManager(
                App.ResolvedSettings,
                App.Services.GetRequiredService<ILogger<SPLA.MCP.Core.Plugins.PluginManager>>());
            var pluginsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            _pluginManager.LoadPlugins(pluginsDir);

            _skillManager = new SPLA.MCP.Core.Plugins.SkillManager(
                App.Services.GetRequiredService<ILogger<SPLA.MCP.Core.Plugins.SkillManager>>());
            _skillManager.LoadSkills(pluginsDir);
            _skillManager.ApplySettings(App.ResolvedSettings.Skills.ToDictionary(
                kvp => kvp.Key,
                kvp => (kvp.Value.Enabled ?? true, kvp.Value.Preloaded ?? false)));

            var avaloniaPluginLoader = App.Services.GetRequiredService<IAvaloniaPluginLoader>();
            avaloniaPluginLoader.LoadPanels(_pluginManager.GetPlugins());
    
            _mcpHost = new McpHost(
                _permissionManager,
                _pluginManager,
                App.Services.GetRequiredService<ILogger<McpHost>>());
            _mcpHost.OnPermissionRequested = HandlePermissionRequestAsync;
            
            _mcpHost.RegisterTool(new FsListTool());
            _mcpHost.RegisterTool(new FsReadTool());
            _mcpHost.RegisterTool(new FsSearchTextTool());
            _mcpHost.RegisterTool(new FsFindFilesTool());
            _mcpHost.RegisterTool(new FsCreateTool());
            _mcpHost.RegisterTool(new FsPatchTool());
            _mcpHost.RegisterTool(new FsWriteTool());
            _mcpHost.RegisterTool(new FsDeleteTool());
            _mcpHost.RegisterTool(new RunCommandTool());
            _mcpHost.RegisterTool(new DotnetBuildTool());
            _mcpHost.RegisterTool(new DotnetTestTool());
            _mcpHost.RegisterTool(new GetContextTool());
            _mcpHost.RegisterTool(new GetCurrentDateTimeTool());
            _mcpHost.RegisterTool(new SPLA.MCP.BasicTools.Network.WebFetchTool());
            _mcpHost.RegisterTool(new SPLA.MCP.BasicTools.Network.WebSearchTool());
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentInfoTool(_mcpHost, _skillManager));
            _mcpHost.RegisterTool(new PluginCommandRunTool(
                _pluginManager.GetUiCommands(),
                App.Services.GetRequiredService<IPluginPanelHostService>(),
                App.Services.GetRequiredService<SPLA.Plugins.Host.Avalonia.IPluginInteractionService>()));
            foreach (var tool in avaloniaPluginLoader.LoadedTools)
            {
                _mcpHost.RegisterTool(tool);
            }
            
            _tools = _mcpHost.GetToolDefinitions().ToList();
            logger.LogInformation("MCP host initialized. ToolCount={ToolCount}", _tools.Count);

            _capabilityRegistry = new SPLA.MCP.Core.Plugins.CapabilityRegistry();
            _capabilityRegistry.Build(_pluginManager.GetPlugins(), _tools, _skillManager.GetAll());

            Sidebar.Build(_capabilityRegistry.Items);
            LoadPluginUiCommands();
    
            _ = InitializeAsync();
            Status.PropertyChanged += Status_PropertyChanged;
        }
}

