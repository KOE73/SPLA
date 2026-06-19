using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPLA.Domain.Agent;
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
    
        // ── Display profiles (global list; selection lives in ChatSessionViewModel) ──
        [ObservableProperty]
        private ObservableCollection<ChatProfileViewModel> _availableProfiles = new();

        /// <summary>Owns all conversation state for the currently active chat session.</summary>
        public ChatSessionViewModel Session { get; private set; } = null!;

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
        private readonly SPLA.LLM.LMStudio.LMStudioClient _llmClient;
        private readonly List<ToolDefinition> _tools;
        // Fundamental agent working memory. session = current chat (persisted with the chat);
        // project = shared across chats (persisted to project-kv.yaml). Both exposed to the debug view.
        private readonly ProjectKvStore _projectKv;
        private readonly SPLA.Agent.CheckpointManager _checkpointManager = new();
        private readonly SPLA.Domain.Agent.SkillSession _skillSession = new();
        private SPLA.MCP.Core.Plugins.SkillFileWatcher? _skillFileWatcher;
        private ChatManager? _chatManager;
        private bool _isReloadingChats;
    
        public MainWindowViewModel()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            _llmClient = new SPLA.LLM.LMStudio.LMStudioClient(_httpClient);
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
            _skillFileWatcher = new SPLA.MCP.Core.Plugins.SkillFileWatcher(_skillManager, _skillSession, pluginsDir);
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

            _projectKv = new ProjectKvStore(App.ResolvedSettings);

            Session = new SPLA.UI.Avalonia.ViewModels.Chat.ChatSessionViewModel(
                this,
                _llmClient,
                _mcpHost,
                _skillManager,
                _pluginManager,
                _skillSession,
                Settings,
                Status,
                GetFilteredToolsForMode,
                _checkpointManager);
            Session.OnChatSaved = LoadChatsList;

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
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemorySetTool(Session.SessionKv, _projectKv.Store));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryGetTool(Session.SessionKv, _projectKv.Store));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryDeleteTool(Session.SessionKv, _projectKv.Store));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryListTool(Session.SessionKv, _projectKv.Store));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentMemoryClearTool(Session.SessionKv, _projectKv.Store));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.SkillActivateTool(_skillSession, _skillManager));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.SkillDeactivateTool(_skillSession));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentClarifyTool());
            var spawnedRunner = new SPLA.Agent.SpawnedAgentRunner(_llmClient, _mcpHost, _skillManager, _pluginManager, App.ResolvedSettings);
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentSpawnTool(spawnedRunner));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.AgentSpawnBatchTool(spawnedRunner));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.ContextCheckpointSetTool(_checkpointManager));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.ContextCheckpointRestoreTool(_checkpointManager));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.MarkSetTool(_checkpointManager));
            _mcpHost.RegisterTool(new SPLA.MCP.Core.Tools.MarkRollbackTool(_checkpointManager));
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
    
            Status.AttachSkillSession(_skillSession);
            _ = InitializeAsync();
            Status.PropertyChanged += Status_PropertyChanged;
        }
}

